using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer.PackageTypes.TransportLayerPacketTypes
{
    public class ICMPPacket : BasePacket
    {
        private byte _type;                
        private byte _code;               
        private short _checksum;           
        private ushort _identifier;      
        private ushort _sequenceNumber;     
        private int _iAddressMask;        

        public ICMPPacket(byte[] buffer, int iReceived)
        {
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;

            try
            {

                memoryStream = new MemoryStream(buffer, 0, iReceived);
                binaryReader = new BinaryReader(memoryStream);


                // tip - 1 bit
                _type = binaryReader.ReadByte();

                // cod - 1 bit
                _code = binaryReader.ReadByte();

                // checksum - 16 biti
                _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // identificator - 16 biti
                _identifier = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // sequence number - 16 biti
                _sequenceNumber = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                // masca de adresa - 32 de biti
                _iAddressMask = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
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

        public string Code
        {
            get { return _code.ToString(); }
        }
        public string Checksum
        {
            get { return "0x" + _checksum.ToString("X"); }
        }
        public string Identifier
        {
            get { return _identifier.ToString(); }
        }
        public string SequenceNUmber
        {
            get { return _sequenceNumber.ToString(); }
        }
        public string AddressMask
        {
            get { return "0x" + _iAddressMask.ToString("X"); }
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
            var sb = new StringBuilder();

            sb.AppendFormat("\tAddress Mask: {0} ", this.AddressMask);
            sb.AppendFormat("\tChecksum: {0} ", this.Checksum);
            sb.AppendFormat("\tCode: {0} ", this.Code);
            sb.AppendFormat("\tIdentifier: {0} ", this.Identifier);
            sb.AppendFormat("\tSeq Number: {0} ", this.SequenceNUmber);
            sb.AppendFormat("\tType: {0} ", this.Type);

            return sb.ToString();
        }
    }
}
