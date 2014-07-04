using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IverRemoteServer
{
    public class Server
    {
        private readonly int _networkPort;
        private IverComm _iverComm;
        private readonly string _serialPort;

        public Server(int networkPort, string serialPort)
        {
            _networkPort = networkPort;
            _serialPort = serialPort;
        }

        // Incoming data from the client.
        private static string _data;

        public void StartListening()
        {
            // Data buffer for incoming data.

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());

            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                Console.WriteLine("Listening at address {0}", ip);
            }

            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _networkPort);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.

            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            while (true)
            {
                _iverComm = new HardwareIverComm(_serialPort);

                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();
                handler.ReceiveTimeout = 5000;
                _data = null;

                // An incoming connection needs to be processed.
                while (true)
                {
                    var bytes = new byte[1024];

                    try
                    {
                        int bytesRec = handler.Receive(bytes);

                        if (bytesRec == 0)
                        {
                            Console.WriteLine("0 bytes receive, connection probably terminated");
                            throw new SocketException();
                        }

                        _data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (_data.IndexOf("\n") > -1)
                        {
                            string [] split = (_data.Split('\n')[0]).Split(',');

                            int trim, throttle, rudder, pitchOffset;
                            pitchOffset = 0;
                            trim = throttle = rudder = 128;
                            try
                            {
                                trim = Convert.ToInt32(split[0]);
                                throttle = Convert.ToInt32(split[1]);
                                rudder = Convert.ToInt32(split[2]);

                                pitchOffset = (int) ((rudder - 128) * .5);

                                Console.WriteLine("Trim: {0}, Roll : {1}, Throttle: {2}, Rudder: {3}",
                                    trim, pitchOffset, throttle, rudder);
                            }
                            catch (FormatException) { Console.WriteLine("Parse err"); }
                            catch (IndexOutOfRangeException) { Console.WriteLine("Parse err"); }


                            _iverComm.SendBackseatCommands(rudder, rudder, 
                                                           trim + pitchOffset, 
                                                           trim - pitchOffset,
                                                           throttle, 3);
                            _iverComm.UpdateFrontseatData();

                            string reply = String.Format("{0:0.#}", _iverComm.Heading) + "," 
                                         + String.Format("{0:0.#}",_iverComm.Speed)+ "," 
                                         + String.Format("{0:0.#}",_iverComm.Pitch) + "," 
                                         + String.Format("{0:0.#}", _iverComm.Row);

                            byte[] msg = Encoding.ASCII.GetBytes(reply);
                            handler.Send(msg);

                            _data = null;
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Time out has occured");
                        break;
                    }
                }

                _iverComm.ClosePort();
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
    }
}