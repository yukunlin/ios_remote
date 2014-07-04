using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IverRemoteServer
{
    public class Server
    {
        private int networkPort_;
        private IverComm iverComm_;
        private string serialPort_;

        public Server(int networkPort, string serialPort)
        {
            networkPort_ = networkPort;
            serialPort_ = serialPort;
        }

        private static int LimitValue(int x)
        {
            return Math.Min(Math.Max(x, 0), 255);
        }

        // Incoming data from the client.
        public static string data = null;

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());

            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                Console.WriteLine("Listening at address {0}", ip);
            }

            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, networkPort_);

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
                iverComm_ = new HardwareIverComm(serialPort_);

                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();
                handler.ReceiveTimeout = 5000;
                data = null;

                // An incoming connection needs to be processed.
                while (true)
                {
                    bytes = new byte[1024];

                    try
                    {
                        int bytesRec = handler.Receive(bytes);

                        if (bytesRec == 0)
                        {
                            Console.WriteLine("0 bytes receive, connection probably terminated");
                            throw new SocketException();
                        }

                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("\n") > -1)
                        {
                            string [] split = (data.Split('\n')[0]).Split(',');

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


                            iverComm_.SendBackseatCommands(rudder, rudder, 
                                                           LimitValue(trim + pitchOffset), 
                                                           LimitValue(trim - pitchOffset),
                                                           throttle, 3);
                            iverComm_.UpdateFrontseatData();

                            string reply = String.Format("{0:0.#}", iverComm_.Heading) + "," 
                                         + String.Format("{0:0.#}",iverComm_.Speed)+ "," 
                                         + String.Format("{0:0.#}",iverComm_.Pitch) + "," 
                                         + String.Format("{0:0.#}", iverComm_.Row);

                            byte[] msg = Encoding.ASCII.GetBytes(reply);
                            handler.Send(msg);

                            data = null;
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Time out has occured");
                        break;
                    }
                }

                iverComm_.ClosePort();
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
    }
}