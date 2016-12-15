using PacketSniffer.PackageTypes;
using PacketSniffer.PackageTypes.TransportLayerPacketTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication.Monitoring
{
    class SocketMonitor
    {
        //private ConsoleWriter _consoleWriter;
        private MainWindow _uiWindow;

        private Socket _socket;
        private byte[] _buffer;

        // Codes for low-level operating modes for the Socket
        private byte[] _bIn;
        private byte[] _bOut;

        public bool IsRunning { get; set; }

        public SocketMonitor(IPAddress ip, MainWindow window)
        {
            _uiWindow = window;

            _bIn = new byte[4] { 1, 0, 0, 0 };
            _bOut = new byte[4];
            _buffer = new byte[8192];

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            _socket.Bind(new IPEndPoint(ip, 0));
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            _socket.IOControl(IOControlCode.ReceiveAll, _bIn, _bOut);
        }

        public void StartMonitoring()
        {
            IsRunning = true;

            while (IsRunning)
            {
                int size = _socket.ReceiveBufferSize;
                int bytesReceived = _socket.Receive(_buffer, 0, _buffer.Length, SocketFlags.None);

                if (bytesReceived > 0)
                {
                    IPPacket ipPacket = new IPPacket(_buffer, bytesReceived);
                    _uiWindow.AddPacketToList(ipPacket);

                    //if (ipPacket.Protocol == "TCP")
                    //{
                    //    TCPPacket tcpPacket = new TCPPacket(ipPacket.Data, ipPacket.MessageLength);                        
                    //    //_consoleWriter.Write(ipPacket, tcpPacket);
                    //}
                    //else if (ipPacket.Protocol == "UDP")
                    //{
                    //    PacketUdp udpPacket = new PacketUdp(ipPacket.Data, ipPacket.MessageLength);
                    //    //_consoleWriter.Write(ipPacket, udpPacket);
                    //}
                    //else if (ipPacket.Protocol == "IGMP")
                    //{
                    //    PacketIgmp igmpPacket = new PacketIgmp(ipPacket.Data, ipPacket.MessageLength);
                    //    //_consoleWriter.Write(ipPacket, igmpPacket);
                    //}
                    //else
                    //{
                    //    PacketIcmp icmpPacket = new PacketIcmp(ipPacket.Data, ipPacket.MessageLength);
                    //    //_consoleWriter.Write(ipPacket, icmpPacket);
                    //}
                }
            }
        }
    }
}
