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
        private const string ASKFORDATA = "$OSD,C,G,Y,S,D*3E\r\n";  // checksum at the end MUST be correct

        private SerialPort port_; // Port for communicating between primary and secondary CPUs.

        // VehicleState fields
        private double[] latLong_;
        private double speed_ = 0;
        private List<double> compassStates_ = new List<double>(new[] { 0.0, 0.0, 0.0, 0.0, 0.0 });
        private LinkedList<double[]> previousLocations_ = new LinkedList<double[]>();

        private const int HISTORY_LENGTH = 12;

        /// <summary>
        /// Sets up the port for intra-Iver CPU communications.
        /// </summary>
        /// <param name="comPortName">The comPort between the primary and secondary CPUs.</param>
        /// <param name="latOrig">The latitude of the orgin of the coordinate system.</param>
        /// <param name="longOrig">The longitude of the origin of the coordinate system. </param>
        public HardwareIverComm(string comPortName)
        {
            port_ = new SerialPort(comPortName, 19200, Parity.None, 8, StopBits.One);
            port_.ReadTimeout = 10;
        }

        /// <summary>
        /// Sends a request to the frontseat for data, then clears the buffer of messages and
        /// updates vehicle state. 
        /// </summary>
        /// <param name="vState">Only here to satisfy the interface requirements.</param>
        /// <returns>Returns an updated VehicleState</returns>
        public void UpdateFrontseatData()
        {
            sendCommand(ASKFORDATA);
            string data = readFrontseatData();

            while (data != null)
            {
                UpdateVehicleState(data);
                data = readFrontseatData();
            }
        }

        /// <summary>
        /// Pulls a message off of the intra-AUV comPort buffer. 
        /// </summary>
        /// <returns>A message from the comPort.</returns>
        private string readFrontseatData()
        {
            try
            {
                return port_.ReadLine();
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
                updatePosition(data);
            }
            else if (data.StartsWith("$C"))
            {
                updateCompass(data);
            }
        }

        public static double distFrom(double lat1, double lng1, double lat2, double lng2)
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

        private void updatePosition(string data)
        {
            string[] splitData = data.Split(new char[] { ',' });

            int length = splitData.Length;

            if (length >= 10 && length <= 12)
            {
                string[] messageEnd = splitData[length - 1].Split(new char[] { '*' });
                latLong_ = new double[2] { Convert.ToDouble(splitData[4]), Convert.ToDouble(splitData[5]) };
                previousLocations_.AddFirst(latLong_);

                if (previousLocations_.Count > HISTORY_LENGTH)
                {
                    previousLocations_.RemoveLast();
                }

                if (previousLocations_.Count == HISTORY_LENGTH)
                {
                    speed_ = distFrom((previousLocations_.First.Value)[0], (previousLocations_.First.Value)[1],
                                      (previousLocations_.Last.Value)[0], (previousLocations_.Last.Value)[1])/2.4;
                }
            }
        }


        private void updateCompass(string data)
        {
            string[] splitData;
            splitData = data.Split(new char[] { 'C', 'P', 'R', 'T', 'D', '*' }, StringSplitOptions.RemoveEmptyEntries);

            double compHeading = Convert.ToDouble(splitData[1]);
            double pitch = Convert.ToDouble(splitData[2]);
            double roll = Convert.ToDouble(splitData[3]);
            double compTemp = Convert.ToDouble(splitData[4]);
            double depth = Convert.ToDouble(splitData[5]);

            compassStates_ = new List<double>(new[] { compHeading, pitch, roll, compTemp, depth });
        }

        /// <summary>
        /// Properly closes the com port
        /// </summary>
        public void ClosePort()
        {
            port_.Close();
            port_.Dispose();
        }

        private int limitValue(int val)
        {
            if (val > 256) { return 256; }
            else if (val < 0) { return 0; }
            else { return val; }
        }

        /// <summary>
        /// Send the most recent backseat commands to the frontseat. 
        /// </summary>
        /// <param name="controlVariables">Contains most recent controller command.</param>
        public void SendBackseatCommands(int topFin, int bottomFin, int portFin, int starboardFin, int motorSpeed, int timeOut)
        {
            string message;
            string cSum;

            message = "$OMP," + toHexString(limitValue(topFin)) + toHexString(limitValue(bottomFin))
                              + toHexString(limitValue(portFin)) + toHexString(limitValue(starboardFin)) 
                              + toHexString(limitValue(motorSpeed)) + ",00," + timeOut.ToString() + "*";

            cSum = getCheckSum(message);

            message += cSum.Substring(1, 1) + cSum.Substring(2, 1) + "\r\n";


            sendCommand(message);
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

            return System.Uri.HexEscape((char)checksum);
        }

        /// <summary>
        /// Converts an integer to a hex value in string form.
        /// </summary>
        /// <param name="val">An integer to convert.</param>
        /// <returns>it's hex equivalent in string form.</returns>
        private string toHexString(int valToConvert)
        {
            return System.Uri.HexEscape((char)valToConvert).Substring(1, 2);
        }

        /// <summary>
        /// Writes a command to the intra-AUV comPort.
        /// </summary>
        /// <param name="command">The command to write.</param>
        private void sendCommand(string command)
        {
            try
            {
                if (!(port_.IsOpen))
                {
                    port_.Open();
                }
                port_.Write(command);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public double Speed
        {
            get { return speed_; }
        }

        public double[] LatLong
        {
            get { return latLong_; }
        }

        public double Heading
        {
            get { return compassStates_[0]; }
        }

        public double Pitch
        {
            get { return compassStates_[1]; }
        }

        public double Row
        {
            get { return compassStates_[2]; }
        }
    }
}
