using PacketSniffer.PackageTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApplication.DataAccessLayer;
using WindowsFormsApplication.Utils;

namespace WindowsFormsApplication.Monitoring
{
    public class SocketMonitor
    {
        private MainForm _uiWindow;
        public List<IPPacket> IPPackets;

        private Socket _socket;
        private byte[] _buffer;
        
        private byte[] _bIn;
        private byte[] _bOut;

        public bool IsRunning { get; set; }
        private IPAddress _localIP;
        private DAL _dal;

        private object syncLock;

        //private int _packetCount;

        private Logger _logger;

        public SocketMonitor(IPAddress ip, DAL dal)
        {
            _localIP = ip;

            _bIn = new byte[4] { 1, 0, 0, 0 };
            _bOut = new byte[4];
            _buffer = new byte[8192];

            IPPackets = new List<IPPacket>();

            _dal = dal;

            syncLock = new object();            
        }

        public void StartMonitoring()
        {
            // Initializarea unui nou socket pentru monitorizarea traficului
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            _socket.Bind(new IPEndPoint(_localIP, 0));
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            _socket.IOControl(IOControlCode.ReceiveAll, _bIn, _bOut);

            // Resetarea datelor din baza de date
            _dal.resetDatabase();

            IsRunning = true;

            if (Settings.LogToFile)
            {
                _logger = new Logger();
            }

            while (IsRunning)
            {
                int size = _socket.ReceiveBufferSize;
                int bytesReceived = _socket.Receive(_buffer, 0, _buffer.Length, SocketFlags.None);

                if (bytesReceived > 0)
                {
                    IPPacket ipPacket = new IPPacket(_buffer, bytesReceived);
                    
                    if(ipPacket.Protocol != "Unknown")
                    {
                        bool blockPacket = false;

                        if (ipPacket.SourceAddress.Equals(_localIP))
                        {
                            ipPacket.Direction = "Outgoing";

                            if (Settings.BlockedPorts.Contains(ipPacket.TransportLayerPacket.SourcePort))
                            {
                                blockPacket = true;
                            }
                        }
                        else
                        {
                            ipPacket.Direction = "Incoming";

                            if (Settings.BlockedPorts.Contains(ipPacket.TransportLayerPacket.DestinationPort))
                            {
                                blockPacket = true;
                            }
                        }

                        if (blockPacket == false)
                        {
                            BackgroundWorker bw = new BackgroundWorker();
                            bw.DoWork += (s, e) =>
                            {
                                ApplicationBinder binder = new ApplicationBinder(_localIP, ipPacket.Protocol);
                                binder.BindApplicationToPacket(ipPacket);
                            };

                            bw.RunWorkerCompleted += (s, e) =>
                            {
                                // Verifica daca un pachet are o aplicatie corelata.
                                // Daca are atunci continua procesarea.
                                // In caz ca nu are si optiune pentru omiterea pachetelor fata aplicatie este activa
                                // Opreste procesarea
                                if (!(Settings.BlockPacketsWithNoProcess == true && (ipPacket.ApplicationName.Equals(string.Empty) || ipPacket.ApplicationName.Equals("Could not find process"))))
                                {
                                    if (ipPacket.Protocol != "Unknown")
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendFormat("insert into IPPackets(TimeStamp, Date, Direction, Protocol, ApplicationName, SourceAddress, SourcePort, DestinationAddress");
                                        sb.AppendFormat(", DestinationPort, TTL, PacketLength) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, '{7}', {8}, {9}, {10})"
                                            , ipPacket.TimeStamp, ipPacket.Date, ipPacket.Direction, ipPacket.Protocol, ipPacket.ApplicationName, ipPacket.SourceAddress
                                            , ipPacket.TransportLayerPacket.SourcePort, ipPacket.DestinationAddress, ipPacket.TransportLayerPacket.DestinationPort, ipPacket.TTL
                                            , (int.Parse(ipPacket.HeaderLength) + ipPacket.MessageLength));
                                                                                
                                        _dal.AddIPPacketToDataBase(sb.ToString());

                                        if (Settings.LogToFile)
                                        {
                                            _logger.WriteToFile(ipPacket);
                                        }
                                    }
                                }
                            };

                            bw.RunWorkerAsync();
                        }
                    }                    
                }
            }

            if (Settings.LogToFile)
            {
                _logger.CloseFile();
            }
        }        
    }
}
