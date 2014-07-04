using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IverRemoteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port;
            string serialport;

            try
            {
                var stream = new StreamReader(File.Open("SETTINGS.txt", FileMode.Open,
                                             FileAccess.Read, FileShare.ReadWrite));

                var line = stream.ReadLine();
                port = Convert.ToInt32(line);
                serialport = stream.ReadLine();
                // Read until eof
            }
            catch (Exception e)
            {
                Console.WriteLine("SETTING.txt not present or in right format, enter settings manually");
                Console.Write("Enter Network Port: ");
                port = Convert.ToInt32(Console.ReadLine());

                Console.Write("Enter Serial Port: ");
                serialport = Console.ReadLine();
            }
           
            Server server = new Server(port, serialport);
            server.StartListening();
        }
    }
}
