using Framework.GameMath;
using System;
using System.IO;
using System.Text;

namespace Framework.IO.Packet
{
    public class PacketWriter : BinaryWriter
    {
        byte _bitPosition = 8;
        byte _bitValue = 0;

        public uint Size => (uint)BaseStream.Length;

        public PacketWriter(MemoryStream output) : base(output) { }

        public PacketWriter() : base(new MemoryStream()) { }

        #region Base Writers
        public void WriteBool(bool data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteInt8(sbyte data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteUInt8(byte data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteBytes(byte[] data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteBytes(byte[] data, int size)
        {
            FlushBits();
            base.Write(data, 0, size);
        }
        public void WriteInt16(short data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteUInt16(ushort data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteInt32(int data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteUInt32(uint data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteInt64(long data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteUInt64(ulong data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteFloat(float data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteDouble(double data)
        {
            FlushBits();
            base.Write(data);
        }
        public void WriteString(string value)
        {
            if (value == string.Empty || value == null)
                return;

            byte[] sBytes = Encoding.UTF8.GetBytes(value);
            WriteBytes(sBytes);
        }
        // public void WriteObjectGuid(ObjectGuid guid)
        // {
        //     if (guid.IsEmpty)
        //     {
        //         WriteUInt8(0);
        //         WriteUInt8(0);
        //         return;
        //     }
        // 
        //     var loSize = PackUInt64(guid.Low, out var lowMask, out var lowPacked);
        //     var hiSize = PackUInt64(guid.High, out var highMask, out var highPacked);
        // 
        //     WriteUInt8(lowMask);
        //     WriteUInt8(highMask);
        //     WriteBytes(lowPacked, loSize);
        //     WriteBytes(highPacked, hiSize);
        // }
        int PackUInt64(ulong value, out byte mask, out byte[] result)
        {
            int resultSize = 0;
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
        public void WriteVector3(Vector3 vector)
        {
            WriteFloat(vector.X);
            WriteFloat(vector.Y);
            WriteFloat(vector.Z);
        }

        // public void WritePackedTime(long time)
        // {
        //     var now = Time.UnixTimeToDateTime(time);
        //     WriteUInt32(Convert.ToUInt32((now.Year - 2000) << 24 | (now.Month - 1) << 20 | (now.Day - 1) << 14 | (int)now.DayOfWeek << 11 | now.Hour << 6 | now.Minute));
        // }

        public void WritePackedTime()
        {
            DateTime now = DateTime.Now;
            WriteUInt32(Convert.ToUInt32((now.Year - 2000) << 24 | (now.Month - 1) << 20 | (now.Day - 1) << 14 | (int)now.DayOfWeek << 11 | now.Hour << 6 | now.Minute));
        }

        public void Write(Position pos)
        {
            WriteFloat(pos.X);
            WriteFloat(pos.Y);
            WriteFloat(pos.Z);
            WriteFloat(pos.Orientation);
        }

        #endregion

        #region Bit Writers
        public bool WriteBit(bool bit)
        {
            --_bitPosition;

            if (bit)
                _bitValue |= (byte)(1 << _bitPosition);

            if (_bitPosition == 0)
            {
                Write(_bitValue);

                _bitPosition = 8;
                _bitValue = 0;
            }

            return bit;
        }

        public bool HasUnfinishedBitPack() => _bitPosition != 8;

        public void WriteBits(uint bit, int count)
        {
            for (int i = count - 1; i >= 0; --i)
                WriteBit(((bit >> i) & 1) != 0);
        }

        public void WriteBits(int bit, int count)
        {
            for (int i = count - 1; i >= 0; --i)
                WriteBit(((bit >> i) & 1) != 0);
        }

        public void FlushBits()
        {
            if (_bitPosition == 8)
                return;

            Write(_bitValue);

            _bitValue = 0;
            _bitPosition = 8;
        }
        #endregion

        public byte[] GetData()
        {
            var stream = BaseStream as MemoryStream;
            return stream.ToArray() ?? Array.Empty<byte>();
        }
    }
}
