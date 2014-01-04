﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Proxy
{
    public class ProxyServer
    {
        private int networkPort_;

        public ProxyServer(int networkPort)
        {
            networkPort_ = networkPort;
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
                Console.WriteLine("Waiting for a connection...");
                
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();
                handler.ReceiveTimeout = 5000;
                data = null;

                // Create a TCP/IP  socket to remote.
               // IPHostEntry RemoteHostInfo = Dns.Resolve("192.168.1.13");
                IPAddress RemoteAddress =  IPAddress.Parse("192.168.1.13");
               // Console.WriteLine(RemoteAddress.ToString());
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