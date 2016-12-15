using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer.PackageTypes.TransportLayerPacketTypes
{
    abstract public class BasePacket
    {
        public abstract string DestinationPort { get; }
        public abstract string SourcePort { get; }
    }
}
