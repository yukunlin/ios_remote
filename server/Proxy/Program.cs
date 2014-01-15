using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter Network Port: ");
            int port = Convert.ToInt32(Console.ReadLine());

            Console.Write("Enter Forwarding address: ");
            string forwardAddress = Console.ReadLine();
            
            ProxyServer p = new ProxyServer(port, forwardAddress);
            p.StartListening();
        }
    }
}
