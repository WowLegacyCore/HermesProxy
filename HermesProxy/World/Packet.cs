/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.IO;
using Framework.Constants;
using System;
using HermesProxy.World.Enums;
using HermesProxy.World.Client;
using Ionic.Zlib;
using System.Collections.Generic;

namespace HermesProxy.World
{
    public abstract class ClientPacket : IDisposable
    {
        protected ClientPacket(WorldPacket worldPacket)
        {
            _worldPacket = worldPacket;
        }

        public abstract void Read();

        public void Dispose()
        {
            _worldPacket.Dispose();
        }

        public uint GetOpcode() { return _worldPacket.GetOpcode(); }
        public Opcode GetUniversalOpcode()
        {
            return Opcodes.GetUniversalOpcode(GetOpcode(), Framework.Settings.ClientBuild);
        }

        public void LogPacket(ref SniffFile sniffFile)
        {
            if (!Framework.Settings.PacketsLog)
                return;

            if (sniffFile == null)
            {
                sniffFile = new SniffFile("modern", (ushort)Framework.Settings.ClientBuild);
                sniffFile.WriteHeader();
            }
            sniffFile.WritePacket(GetOpcode(), true, _worldPacket.GetData());
        }

        protected WorldPacket _worldPacket;
    }

    public abstract class ServerPacket
    {
        protected ServerPacket(Opcode universalOpcode)
        {
            connectionType = ConnectionType.Realm;

            uint opcode = Opcodes.GetOpcodeValueForVersion(universalOpcode, Framework.Settings.ClientBuild);
            System.Diagnostics.Trace.Assert(opcode != 0);
            _worldPacket = new WorldPacket(opcode);
        }

        protected ServerPacket(Opcode universalOpcode, ConnectionType type = ConnectionType.Realm)
        {
            connectionType = type;

            uint opcode = Opcodes.GetOpcodeValueForVersion(universalOpcode, Framework.Settings.ClientBuild);
            System.Diagnostics.Trace.Assert(opcode != 0);
            _worldPacket = new WorldPacket(opcode);
        }

        public void Clear()
        {
            _worldPacket.Clear();
            buffer = null;
        }

        public uint GetOpcode()
        {
            return _worldPacket.GetOpcode();
        }
        public Opcode GetUniversalOpcode()
        {
            return Opcodes.GetUniversalOpcode(GetOpcode(), Framework.Settings.ClientBuild);
        }

        public byte[] GetData()
        {
            return buffer;
        }

        public void LogPacket(ref SniffFile sniffFile)
        {
            if (!Framework.Settings.PacketsLog)
                return;

            if (sniffFile == null)
            {
                sniffFile = new SniffFile("modern", (ushort)Framework.Settings.ClientBuild);
                sniffFile.WriteHeader();
            }
            sniffFile.WritePacket(GetOpcode(), false, GetData());
        }

        public abstract void Write();

        public void WritePacketData()
        {
            if (buffer != null)
                return;

            Write();

            buffer = _worldPacket.GetData();
            _worldPacket.Dispose();
        }

        public ConnectionType GetConnection() { return connectionType; }

        byte[] buffer;
        ConnectionType connectionType;
        protected WorldPacket _worldPacket;
    }

    public class WorldPacket : ByteBuffer
    {
        public WorldPacket(uint opcode = 0)
        {
            this.opcode = opcode;
        }

        public WorldPacket(Opcode opcode)
        {
            this.opcode = Opcodes.GetOpcodeValueForVersion(opcode, Framework.Settings.ServerBuild);
            System.Diagnostics.Trace.Assert(this.opcode != 0);
        }

        public WorldPacket(uint opcode, byte[] data) : base(data)
        {
            this.opcode = opcode;
            System.Diagnostics.Trace.Assert(this.opcode != 0);
        }

        public WorldPacket(byte[] data) : base(data)
        {
            opcode = ReadUInt16();
        }

        public KeyValuePair<int, bool> ReadEntry()
        {
            // Entries masked with 0x80000000 are invalid entries OR used to tell apart NPCs and GOs

            var entry = ReadUInt32();
            var realEntry = entry & 0x7FFFFFFF;

            return new KeyValuePair<int, bool>((int)realEntry, realEntry != entry);
        }

        public WowGuid64 ReadGuid()
        {
            var guid = new WowGuid64(ReadUInt64());
            return guid;
        }

        public WowGuid64 ReadPackedGuid()
        {
            var guid = new WowGuid64(ReadPackedUInt64(ReadUInt8()));
            return guid;
        }

        public WowGuid128 ReadPackedGuid128()
        {
            var loLength = ReadUInt8();
            var hiLength = ReadUInt8();
            var low = ReadPackedUInt64(loLength);
            return new WowGuid128(ReadPackedUInt64(hiLength), low);
        }

        private ulong ReadPackedUInt64(byte length)
        {
            if (length == 0)
                return 0;

            var guid = 0ul;

            for (var i = 0; i < 8; i++)
                if ((1 << i & length) != 0)
                    guid |= (ulong)ReadUInt8() << (i * 8);

            return guid;
        }

        public UpdateField ReadUpdateField()
        {
            uint val = ReadUInt32();

            var field = new UpdateField(val);
            return field;
        }

        public WorldPacket Inflate(int inflatedSize)
        {
            var arr = ReadToEnd();
            var newarr = new byte[inflatedSize];

            var stream = new ZlibCodec(Ionic.Zlib.CompressionMode.Decompress)
            {
                InputBuffer = arr,
                NextIn = 0,
                AvailableBytesIn = arr.Length,
                OutputBuffer = newarr,
                NextOut = 0,
                AvailableBytesOut = inflatedSize
            };

            stream.Inflate(FlushType.None);
            stream.Inflate(FlushType.Finish);
            stream.EndInflate();

            // Cannot use "using" here
            var pkt = new WorldPacket(GetOpcode(), newarr);
            pkt.SetReceiveTime(GetReceivedTime());
            return pkt;
        }

        public void WriteGuid(WowGuid64 guid)
        {
            WriteUInt64(guid.GetLowValue());
        }

        public void WritePackedGuid(WowGuid64 guid)
        {
            WritePackedUInt64(guid.Low);
        }

        public void WritePackedGuid128(WowGuid128 guid)
        {
            if (guid.IsEmpty())
            {
                WriteUInt8(0);
                WriteUInt8(0);
                return;
            }

            byte lowMask, highMask;
            byte[] lowPacked, highPacked;

            var loSize = PackUInt64(guid.GetLowValue(), out lowMask, out lowPacked);
            var hiSize = PackUInt64(guid.GetHighValue(), out highMask, out highPacked);

            WriteUInt8(lowMask);
            WriteUInt8(highMask);
            WriteBytes(lowPacked, loSize);
            WriteBytes(highPacked, hiSize);
        }

        public void WritePackedUInt64(ulong guid)
        {
            byte mask;
            byte[] packed;
            var packedSize = PackUInt64(guid, out mask, out packed);

            WriteUInt8(mask);
            WriteBytes(packed, packedSize);
        }

        uint PackUInt64(ulong value, out byte mask, out byte[] result)
        {
            uint resultSize = 0;
            mask = 0;
            result = new byte[8];

            for (byte i = 0; value != 0; ++i)
            {
                if ((value & 0xFF) != 0)
                {
                    mask |= (byte)(1 << i);
                    result[resultSize++] = (byte)(value & 0xFF);
                }

                value >>= 8;
            }

            return resultSize;
        }

        public void WriteBytes(WorldPacket data)
        {
            FlushBits();
            WriteBytes(data.GetData());
        }

        public uint GetOpcode() { return opcode; }
        public Opcode GetUniversalOpcode(bool isModern)
        {
            return Opcodes.GetUniversalOpcode(GetOpcode(), isModern ? Framework.Settings.ClientBuild : Framework.Settings.ServerBuild);
        }

        public long GetReceivedTime() { return m_receivedTime; }
        public void SetReceiveTime(long receivedTime) { m_receivedTime = receivedTime; }

        uint opcode;
        long m_receivedTime;
    }

    public class PacketHeader
    {
        public int Size;
        public byte[] Tag = new byte[12];

        public void Read(byte[] buffer)
        {
            Size = BitConverter.ToInt32(buffer, 0);
            Buffer.BlockCopy(buffer, 4, Tag, 0, 12);
        }

        public void Write(ByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt32(Size);
            byteBuffer.WriteBytes(Tag, 12);
        }

        public bool IsValidSize() { return Size < 0x40000; }
    }

    public class LegacyServerPacketHeader
    {
        public const int StructSize = sizeof(ushort) + sizeof(ushort);
        public ushort Size;
        public ushort Opcode;
        public void Read(byte[] buffer)
        {
            Size = Framework.Util.NetworkUtility.EndianConvert(BitConverter.ToUInt16(buffer, 0));
            Opcode = BitConverter.ToUInt16(buffer, sizeof(ushort));
        }
        public void Write(ByteBuffer byteBuffer)
        {
            byteBuffer.WriteUInt16(Size);
            byteBuffer.WriteUInt16(Opcode);
        }
    };

    public class LegacyClientPacketHeader
    {
        public const int StructSize = sizeof(ushort) + sizeof(uint);
        public ushort Size;
        public uint Opcode;
        public void Read(byte[] buffer)
        {
            Size = BitConverter.ToUInt16(buffer, 0);
            Opcode = BitConverter.ToUInt32(buffer, sizeof(ushort));
        }
        public void Write(ByteBuffer byteBuffer)
        {
            byteBuffer.WriteUInt16(Framework.Util.NetworkUtility.EndianConvert(Size));
            byteBuffer.WriteUInt32(Opcode);
        }
    };
}
