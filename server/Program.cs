using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IO_lab_2
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPAsynchClasses.serverEchoA server = new TCPAsynchClasses.serverEchoA(IPAddress.Parse("127.0.0.1"), 2048);
            server.Start();
        }
    }
}
