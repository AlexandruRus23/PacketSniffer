using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer.PackageTypes.TransportLayerPacketTypes
{
    class UDPPacket : BasePacket
    {
        private ushort _sourcePort;      
        private ushort _destinationPort;    
        private ushort _length;            
        private short _checksum;              
                
        private byte[] _UDPData = new byte[4096];
        
        public UDPPacket(byte[] byBuffer, int nReceived)
        {
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;

            try
            {
                memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                binaryReader = new BinaryReader(memoryStream);

                // portul sursa - 16 biti
                _sourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // portul destinatie - 16 bits 
                _destinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // lungimea pachetului - 16 biti
                _length = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // checksum - 16 biti
                _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                
                Array.Copy(byBuffer, 8, _UDPData, 0, nReceived - 8);
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

        public string Length
        {
            get { return _length.ToString(); }
        }

        public string Checksum
        {
            get { return "0x" + _checksum.ToString("x"); }
        }

        public byte[] Data
        {
            get { return _UDPData; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\tSource Port: {0} ", _sourcePort);
            sb.AppendFormat("\tDestination Port: {0} ", _destinationPort);
            sb.AppendFormat("\tLength: {0} ", _length);

            return sb.ToString();
        }
    }
}
