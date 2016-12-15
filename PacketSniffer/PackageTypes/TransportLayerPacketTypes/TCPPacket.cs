using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer.PackageTypes.TransportLayerPacketTypes
{
    public class TCPPacket : BasePacket
    {
        private ushort _sourcePort;       
        private ushort _destinationPort;    
        private uint _sequenceNumber; 
        private uint _ackNumber;        
        private ushort _dataOffsetAndFlags;
        private ushort _window;            
        private short _checksum;     

        private ushort _urgentPointer;  

        private byte _headerLength;         
        private ushort _messageLength;          
        private byte[] _tcpData = new byte[4096]; 

        public TCPPacket(byte[] bBuffer, int iReceived)
        {
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;

            try
            {
                memoryStream = new MemoryStream(bBuffer, 0, iReceived);
                binaryReader = new BinaryReader(memoryStream);

                // portul sursa - 16 biti
                _sourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // portul destinatie - 16 biti
                _destinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // numarul in secventa - 32 de biti
                _sequenceNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());

                // acknowledgement - 32 de biti
                _ackNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());

                // offset si flags - 16 biti 
                _dataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // window size - 16 biti
                _window = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // checksum - 16 biti
                _checksum = (short)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //urgent pointer - 16 biti
                _urgentPointer = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // lungime header 
                _headerLength = (byte)(_dataOffsetAndFlags >> 12);
                _headerLength *= 4;

                // lungime date
                _messageLength = (ushort)(iReceived - _headerLength);
                
                Array.Copy(bBuffer, _headerLength, _tcpData, 0, iReceived - _headerLength);
            }
            catch (Exception) { }

            finally
            {
                binaryReader.Close();
                memoryStream.Close();
            }
        }

        public override string SourcePort
        {
            get { return _sourcePort.ToString(); }
        }

        public override string DestinationPort
        {
            get { return _destinationPort.ToString(); }
        }

        public string SequenceNumber
        {
            get { return _sequenceNumber.ToString(); }
        }

        public string AcknowledgementNumber
        {
            get
            {
                if ((_dataOffsetAndFlags & 0x10) != 0)
                    return _ackNumber.ToString();
                else
                    return "";
            }
        }

        public string HeaderLength
        {
            get { return _headerLength.ToString(); }
        }

        public string WindowSize
        {
            get { return _window.ToString(); }
        }

        public string UrgentPointer
        {
            get
            {
                if ((_dataOffsetAndFlags & 0x20) != 0)
                    return _urgentPointer.ToString();
                else
                    return "";
            }
        }

        public string Flags
        {
            get
            {
                int iFlags = _dataOffsetAndFlags & 0x3F;

                string strFlags = string.Format("0x{0:x2} ", iFlags);

                if ((iFlags & 0x01) != 0)
                    strFlags += "FIN  ";

                if ((iFlags & 0x02) != 0)
                    strFlags += "SYN  ";

                if ((iFlags & 0x04) != 0)
                    strFlags += "RST  ";

                if ((iFlags & 0x08) != 0)
                    strFlags += "PSH  ";

                if ((iFlags & 0x10) != 0)
                    strFlags += "ACK  ";

                if ((iFlags & 0x20) != 0)
                    strFlags += "URG ";

                if (strFlags.Contains("()"))
                    strFlags = strFlags.Remove(strFlags.Length - 3);

                else if (strFlags.Contains(", )"))
                    strFlags = strFlags.Remove(strFlags.Length - 3, 2);

                return strFlags;
            }
        }

        public string Checksum
        {
            get { return "0x" + _checksum.ToString("x"); }
        }

        public byte[] Data
        {
            get { return _tcpData; }
        }

        public string MessageLength
        {
            get { return _messageLength.ToString(); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\tSource Port: {0} ", this._sourcePort);
            sb.AppendFormat("\tDestination Port: {0} ", this._destinationPort);
            sb.AppendFormat("\tHeader length: {0} ", this._headerLength);
            sb.AppendFormat("\tMessage length: {0} ", this._messageLength);

            return sb.ToString();
        }
    }
}
