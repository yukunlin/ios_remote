using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IverRemoteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter Network Port: ");
            int port = Convert.ToInt32(Console.ReadLine());

            Console.Write("Enter Serial Port: ");
            string serialport = Console.ReadLine();
           
            Server server = new Server(port, serialport);
            server.StartListening();
        }
    }
}
