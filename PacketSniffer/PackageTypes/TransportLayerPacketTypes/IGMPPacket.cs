using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer.PackageTypes.TransportLayerPacketTypes
{
    public class IGMPPacket : BasePacket
    {
        private byte _type;                       
        private short _maxResponseTime;         
        private short _checksum;                
        private int _groupAddress;            

        public IGMPPacket(byte[] buffer, int iReceived)
        {
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;

            try
            {
                if (buffer.Length > 0)
                {
                    memoryStream = new MemoryStream(buffer, 0, iReceived);
                    binaryReader = new BinaryReader(memoryStream);

                    // tip - 1 bit
                    _type = binaryReader.ReadByte();

                    // timp de raspuns maxim - 16 biti
                    _maxResponseTime = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    // checksum - 16 biti
                    _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    // adresa grupului - 32 de biti
                    _groupAddress = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                }
            }
            catch 
            {
            }
            finally
            {
                binaryReader.Close();
                memoryStream.Close();
            }
        }

        public string Type
        {
            get { return _type.ToString(); }
        }
        public string MaxResponseTime
        {
            get { return _maxResponseTime.ToString(); }
        }
        public string Checksum
        {
            get { return "0x" + _checksum.ToString("X"); }
        }
        public string GroupAddress
        {
            get { return _groupAddress.ToString(); }
        }

        public override string DestinationPort
        {
            get
            {
                return "0";
            }
        }

        public override string SourcePort
        {
            get
            {
                return "0";
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\tChecksum: {0} ", this.Checksum);
            sb.AppendFormat("\tGropeAddress: {0} ", this.GroupAddress);
            sb.AppendFormat("\tMaxResponseTime: {0} ", this.MaxResponseTime);
            sb.AppendFormat("\tType: {0} ", this.Type);

            return sb.ToString();
        }
    }
}
