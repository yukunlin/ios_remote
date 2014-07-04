using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections;

namespace IverRemoteServer
{
    public class HardwareIverComm : IverComm
    {
        // ReSharper disable once InconsistentNaming
        private const string ASKFORDATA = "$OSD,C,G,Y,S,D*3E\r\n";  // checksum at the end MUST be correct

        private readonly SerialPort _port; // Port for communicating between primary and secondary CPUs.

        // VehicleState fields
        private double[] _latLong;
        private double _speed;
        private List<double> _compassStates = new List<double>(new[] { 0.0, 0.0, 0.0, 0.0, 0.0 });
        private readonly LinkedList<double[]> _previousLocations = new LinkedList<double[]>();

        // ReSharper disable once InconsistentNaming
        private const int HISTORY_LENGTH = 12;

        /// <summary>
        /// Sets up the port for intra-Iver CPU communications.
        /// </summary>
        /// <param name="comPortName">The comPort between the primary and secondary CPUs.</param>
        public HardwareIverComm(string comPortName)
        {
            _port = new SerialPort(comPortName, 19200, Parity.None, 8, StopBits.One);
            _port.ReadTimeout = 10;
        }

        /// <summary>
        /// Sends a request to the frontseat for data, then clears the buffer of messages and
        /// updates vehicle state. 
        /// </summary>
        public void UpdateFrontseatData()
        {
            SendCommand(ASKFORDATA);
            var data = ReadFrontseatData();

            while (data != null)
            {
                UpdateVehicleState(data);
                data = ReadFrontseatData();
            }
        }

        /// <summary>
        /// Pulls a message off of the intra-AUV comPort buffer. 
        /// </summary>
        /// <returns>A message from the comPort.</returns>
        private string ReadFrontseatData()
        {
            try
            {
                return _port.ReadLine();
            }
            catch (TimeoutException) { }

            return null;
        }

        /// <summary>
        /// Calls the appropriate function to update vehcile state data based on the leading characters.
        /// </summary>
        /// <remarks>
        /// We can ignore the GPRMC data stream because long/lat coordinates
        /// are contained in the OSI stream and thos coordinates are also
        /// not dependent upon a GPS signal (the also factor in DVL dead-reckoning).
        /// </remarks>
        /// <param name="data"></param>
        private void UpdateVehicleState(string data)
        {
            if (data.StartsWith("$OSI"))
            {
                UpdatePosition(data);
            }
            else if (data.StartsWith("$C"))
            {
                UpdateCompass(data);
            }
        }

        public static double DistFrom(double lat1, double lng1, double lat2, double lng2)
        {
            double earthRadius = 3958.75;
            double dLat = (Math.PI / 180) * (lat2 - lat1);
            double dLng = (Math.PI / 180) * (lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos((Math.PI / 180) * (lat1)) * Math.Cos((Math.PI / 180) * (lat2)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double dist = earthRadius * c;
            
            double meterConversion = 1609;
            dist = dist * meterConversion;

            return dist;
        }

        private void UpdatePosition(string data)
        {
            string[] splitData = data.Split(new [] { ',' });

            int length = splitData.Length;

            if (length >= 10 && length <= 12)
            {
                _latLong = new [] { Convert.ToDouble(splitData[4]), Convert.ToDouble(splitData[5]) };
                _previousLocations.AddFirst(_latLong);

                if (_previousLocations.Count > HISTORY_LENGTH)
                {
                    _previousLocations.RemoveLast();
                }

                if (_previousLocations.Count == HISTORY_LENGTH)
                {
                    _speed = DistFrom((_previousLocations.First.Value)[0], (_previousLocations.First.Value)[1],
                                      (_previousLocations.Last.Value)[0], (_previousLocations.Last.Value)[1])/2.4;
                }
            }
        }


        private void UpdateCompass(string data)
        {
            string[] splitData = data.Split(new [] { 'C', 'P', 'R', 'T', 'D', '*' }, StringSplitOptions.RemoveEmptyEntries);

            double compHeading = Convert.ToDouble(splitData[1]);
            double pitch = Convert.ToDouble(splitData[2]);
            double roll = Convert.ToDouble(splitData[3]);
            double compTemp = Convert.ToDouble(splitData[4]);
            double depth = Convert.ToDouble(splitData[5]);

            _compassStates = new List<double>(new[] { compHeading, pitch, roll, compTemp, depth });
        }

        /// <summary>
        /// Properly closes the com port
        /// </summary>
        public void ClosePort()
        {
            _port.Close();
            _port.Dispose();
        }

        private static int LimitValue(int x)
        {
            return Math.Min(Math.Max(x, 0), 255);
        }

        /// <summary>
        /// Send the most recent backseat commands to the frontseat. 
        /// </summary>
        /// <param name="topFin"></param>
        /// <param name="bottomFin"></param>
        /// <param name="portFin"></param>
        /// <param name="starboardFin"></param>
        /// <param name="motorSpeed"></param>
        /// <param name="timeOut"></param>
        public void SendBackseatCommands(int topFin, int bottomFin, int portFin, int starboardFin, int motorSpeed, int timeOut)
        {
            var message = "$OMP," + ToHexString(LimitValue(topFin)) + ToHexString(LimitValue(bottomFin))
                             + ToHexString(LimitValue(portFin)) + ToHexString(LimitValue(starboardFin)) 
                             + ToHexString(LimitValue(motorSpeed)) + ",00," + timeOut + "*";

            var cSum = getCheckSum(message);

            message += cSum.Substring(1, 1) + cSum.Substring(2, 1) + "\r\n";


            SendCommand(message);
        }

        private string getCheckSum(string commandMessage)
        {
            int checksum = 0;

            foreach (char c in commandMessage)
            {
                if (c == '$') { }  //Ignore
                else if (c == '*') break;
                else
                {
                    if (checksum == 0) checksum = Convert.ToByte(c);
                    else checksum = checksum ^ Convert.ToByte(c);
                }
            }

            return Uri.HexEscape((char)checksum);
        }

        /// <summary>
        /// Converts an integer to a hex value in string form.
        /// </summary>
        /// <param name="valToConvert">An integer to convert.</param>
        /// <returns>it's hex equivalent in string form.</returns>
        private static string ToHexString(int valToConvert)
        {
            return Uri.HexEscape((char)valToConvert).Substring(1, 2);
        }

        /// <summary>
        /// Writes a command to the intra-AUV comPort.
        /// </summary>
        /// <param name="command">The command to write.</param>
        private void SendCommand(string command)
        {
            try
            {
                if (!(_port.IsOpen))
                {
                    _port.Open();
                }
                _port.Write(command);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public double Speed
        {
            get { return _speed; }
        }

        public double[] LatLong
        {
            get { return _latLong; }
        }

        public double Heading
        {
            get { return _compassStates[0]; }
        }

        public double Pitch
        {
            get { return _compassStates[1]; }
        }

        public double Row
        {
            get { return _compassStates[2]; }
        }
    }
}
