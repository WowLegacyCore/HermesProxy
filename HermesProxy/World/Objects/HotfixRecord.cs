using Framework.IO;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class HotfixRecord
    {
        public uint HotfixId;
        public uint UniqueId;
        public DB2Hash TableHash;
        public uint RecordId;
        public HotfixStatus Status;
        public ByteBuffer HotfixContent = new();

        public void WriteAvailable(WorldPacket data)
        {
            data.WriteUInt32(HotfixId);
            data.WriteUInt32((uint)TableHash);
        }
        public void WriteHotFixMessageContent(WorldPacket data)
        {
            data.WriteUInt32(HotfixId);
            data.WriteUInt32(UniqueId);
            data.WriteUInt32((uint)TableHash);
            data.WriteUInt32(RecordId);
            data.WriteUInt32(HotfixContent.GetSize());
            data.WriteBits((byte)Status, 3);
            data.FlushBits();
        }
    }
}
