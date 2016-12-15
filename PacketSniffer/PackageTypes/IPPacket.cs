using PacketSniffer.PackageTypes.TransportLayerPacketTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer.PackageTypes
{
    public class IPPacket
    {
        private byte _version;     
        private byte _typeOfService;        
        private ushort _totalLength;         
        private ushort _Identification;      
        private ushort _flagsAndOffset;    
        private byte _ttl;                
        private byte _protocol;          
        private short _checksum;            

        private uint _sourceAddress;     
        private uint _destinationAddress; 

        private byte _headerLength;      

        private byte[] byIPData = new byte[8192]; 
        private string _timeStamp;
        private string _date;
        public BasePacket TransportLayerPacket { get; }
        public string ApplicationName { get; set; }
        public string Direction { get; set; }

        public IPPacket(byte[] bBuffer, int iReceived)
        {            
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;

            _timeStamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            _date = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                memoryStream = new MemoryStream(bBuffer, 0, iReceived);
                binaryReader = new BinaryReader(memoryStream);

                //versiune - 4 biti
                _version = binaryReader.ReadByte();

                //tip de serviciu - 8 biti
                _typeOfService = binaryReader.ReadByte();

                //lungimea totala a pachetului - 16 biti 
                _totalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //identificare - 16 biti
                _Identification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //flags - 16 biti 
                _flagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //time to live 8 biti
                _ttl = binaryReader.ReadByte();

                //protocolul pachetului continut - 8 biti
                _protocol = binaryReader.ReadByte();

                //checksum - 16 biti
                _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // adresa sursa - 32 de biti
                _sourceAddress = (uint)(binaryReader.ReadInt32());

                //adresa destinatie - 32 de biti
                _destinationAddress = (uint)(binaryReader.ReadInt32());

                _headerLength = _version;

                _headerLength <<= 4;
                _headerLength >>= 4;

                _headerLength *= 4;
                
                Array.Copy(bBuffer, _headerLength, byIPData, 0, _totalLength - _headerLength);

                switch (Protocol)
                {
                    case "TCP":
                        TransportLayerPacket = new TCPPacket(Data, MessageLength);
                        break;
                    case "UDP":
                        TransportLayerPacket = new UDPPacket(Data, MessageLength);
                        break;
                    case "ICMP":
                        TransportLayerPacket = new ICMPPacket(Data, MessageLength);
                        break;
                    case "IGMP":
                        TransportLayerPacket = new IGMPPacket(Data, MessageLength);
                        break;
                    case "Unknown":
                        break;
                }
            }
            finally
            {
                binaryReader.Close();
                memoryStream.Close();
            }
        }

        public string Version
        {
            get
            {

                if ((_version >> 4) == 4)
                {
                    return "IP v4";
                }

                else if ((_version >> 4) == 6)
                {
                    return "IP v6";
                }

                else
                {
                    return "Unknown";
                }
            }
        }

        public string TimeStamp
        {
            get { return _timeStamp; }
        }

        public string Date
        {
            get { return _date; }
        }

        public string HeaderLength
        {
            get { return _headerLength.ToString(); }
        }

        public ushort MessageLength
        {
            get { return (ushort)(_totalLength - _headerLength); }
        }

        public string TypeOfService
        {
            get { return string.Format("0x{0:x2} ({1})", _typeOfService, _typeOfService); }
        }

        public string Flags
        {
            get
            {
                int iFlags = _flagsAndOffset >> 13;
                if (iFlags == 2)
                {
                    return "Not fragmented";
                }
                else if (iFlags == 1)
                {
                    return "Fragmented";
                }
                else
                {
                    return iFlags.ToString();
                }
            }
        }

        public string FragmentationOffset
        {
            get
            {
                int iOffset = _flagsAndOffset << 3;
                iOffset >>= 3;

                return iOffset.ToString();
            }
        }

        public string TTL
        {
            get { return _ttl.ToString(); }
        }

        public string Protocol
        {
            get
            {
                if (_protocol == 6)
                    return "TCP";
                else if (_protocol == 17)
                    return "UDP";
                else if (_protocol == 1)
                    return "ICMP";
                else if (_protocol == 2)
                    return "IGMP";

                else
                {
                    return "Unknown";
                }
            }
        }

        public string Checksum
        {
            get { return "0x" + _checksum.ToString("x"); }
        }

        public IPAddress SourceAddress
        {
            get { return new IPAddress(_sourceAddress); }
        }

        public IPAddress DestinationAddress
        {
            get { return new IPAddress(_destinationAddress); }
        }

        public string TotalLength
        {
            get { return _totalLength.ToString(); }
        }

        public string Identification
        {
            get { return _Identification.ToString(); }
        }

        public byte[] Data
        {
            get { return byIPData; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0, -12}{1,-10}", Date, TimeStamp);
            sb.AppendFormat("{0, -10}", Protocol);
            sb.AppendFormat("{0, -30}", ApplicationName);
            sb.AppendFormat("{0, -18}", SourceAddress);
            sb.AppendFormat("{0, -15}", TransportLayerPacket.SourcePort);
            sb.AppendFormat("{0, -18}", DestinationAddress);
            sb.AppendFormat("{0, -18}", TransportLayerPacket.DestinationPort);
            sb.AppendFormat("{0, -10}", TotalLength);

            return sb.ToString();
        }
    }
}
