using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Active TCP Connections:");
            var properties = IPGlobalProperties.GetIPGlobalProperties();            

            var connections = properties.GetActiveTcpConnections();
            foreach(var con in connections)
            {
                Console.WriteLine("{0} <==> {1} - {2}", con.LocalEndPoint, con.RemoteEndPoint, con.State);
            }

            Console.WriteLine("\n\n\tTCP Listeners\n");
            var listeners = properties.GetActiveTcpListeners();
            foreach(var lis in listeners)
            {
                Console.WriteLine("{0} - {1}", lis.Address, lis.Port);
            }

            Console.WriteLine("\n\n\tUDP Listeners\n");
            var udpListeners = properties.GetActiveUdpListeners();
            foreach(var udplist in udpListeners)
            {
                Console.WriteLine("{0} - {1}", udplist.Address, udplist.Port);
            }

            Console.ReadLine();
        }
    }
}
