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
            ProxyServer p = new ProxyServer(9000);
            p.StartListening();
        }
    }
}
