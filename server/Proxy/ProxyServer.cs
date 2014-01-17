using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Proxy
{
    public class ProxyServer
    {
        private int networkPort_;
        private string listeningAddress_;
        private string forwardingAddress_;

        public ProxyServer(int networkPort, string listeningAddress, string forwardingAdress)
        {
            networkPort_ = networkPort;
            forwardingAddress_ = forwardingAdress;
            listeningAddress_ = listeningAddress;
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
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(listeningAddress_), networkPort_);

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
                Console.WriteLine("Waiting for a connection...");
                
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();
                handler.ReceiveTimeout = 5000;
                data = null;

                // Create a TCP/IP  socket to remote.
                IPAddress RemoteAddress =  IPAddress.Parse(forwardingAddress_);
                IPEndPoint remoteEP = new IPEndPoint(RemoteAddress, networkPort_);
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                sender.Connect(remoteEP);

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
                            // Forward the data to remote
                            Console.WriteLine("Forwarding message to remote: {0}", data);
                            byte[] msg = Encoding.ASCII.GetBytes(data);
                            sender.Send(msg);
                            
                            Console.WriteLine("message sent, waiting for reply");
                            // Receive reply from remote
                            bytesRec = sender.Receive(bytes);
                            Console.WriteLine("Recieved {0} bytes", bytesRec);
                            string reply = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                           // Forward reply back
                            Console.WriteLine("Sending back reply: {0}", reply);
                            handler.Send(Encoding.ASCII.GetBytes(reply));

                            data = null;
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Time out has occured");
                        break;
                    }
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }
    }
}