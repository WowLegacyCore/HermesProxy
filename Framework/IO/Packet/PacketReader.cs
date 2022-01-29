using System;
using System.IO;
using System.Text;
using Framework.GameMath;

namespace Framework.IO.Packet
{
    public class PacketReader : BinaryReader
    {
        byte _bitPosition = 8;
        byte _bitValue = 0;

        public PacketReader(MemoryStream input) : base(input) { }
        public PacketReader(byte[] data) : base(new MemoryStream(data)) { }

        /// <summary>
        /// Checks if the stream is still able to read data.
        /// </summary>
        public bool IsOpen => BaseStream.CanRead;
        /// <summary>
        /// Remaining bytes on the stream.
        /// </summary>
        public uint RemainingBytes => BaseStream?.Remaining() ?? 0u;

        public byte[] GetData()
        {
            var data = new byte[BaseStream.Length];
            var pos = BaseStream.Position;
            BaseStream.Seek(0, SeekOrigin.Begin);
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)BaseStream.ReadByte();

            BaseStream.Seek(pos, SeekOrigin.Begin);
            return data;
        }

        #region Read Methods
        public sbyte ReadInt8()
        {
            return base.ReadSByte();
        }

        public override short ReadInt16()
        {
            return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            return base.ReadInt64();
        }

        public byte ReadUInt8()
        {
            return base.ReadByte();
        }

        public override ushort ReadUInt16()
        {
            return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            return base.ReadUInt64();
        }

        public float ReadFloat()
        {
            return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            return base.ReadDouble();
        }
        public string ReadString(int length)
        {
            var returnString = base.ReadChars(length);

            return new string(returnString);
        }
        public void Skip(uint count) => BaseStream.Position += count;
        public ulong ReadPacketUInt64(byte length)
        {
            if (length == 0)
                return 0;

            var guid = 0ul;

            for (var i = 0; i < 8; i++)
                if ((1 << i & length) != 0)
                    guid |= (ulong)ReadUInt8() << (i * 8);

            return guid;
        }

        // public ObjectGuid ReadObjectGuid()
        // {
        //     var loLength = ReadUInt8();
        //     var hiLength = ReadUInt8();
        //     var low = ReadPacketUInt64(loLength);
        //     var hi = ReadPacketUInt64(hiLength);
        //     return new ObjectGuid(hi, low);
        // }

        public string ReadCString()
        {
            var builder = new StringBuilder();

            while (true)
            {
                var letter = ReadByte();
                if (letter == 0)
                    break;

                builder.Append((char)letter);
            }

            return builder.ToString();
        }

        public Position ReadPosition()
        {
            return new Position(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Vector3 ReadVector3()
        {
            return new(ReadFloat(), ReadFloat(), ReadFloat());
        }

        #endregion
        #region Bit Readers
        public bool ReadBit()
        {
            if (_bitPosition == 8)
            {
                _bitValue = ReadUInt8();
                _bitPosition = 0;
            }

            int returnValue = _bitValue;
            _bitValue = (byte)(2 * returnValue);
            ++_bitPosition;

            return Convert.ToBoolean(returnValue >> 7);
        }

        public void ResetBitPos()
        {
            if (_bitPosition > 7)
                return;

            _bitPosition = 8;
            _bitValue = 0;
        }

        public T ReadBits<T>(int bitCount)
        {
            int value = 0;

            for (var i = bitCount - 1; i >= 0; --i)
                if (ReadBit())
                    value |= (1 << i);

            return (T)Convert.ChangeType(value, typeof(T));
        }
        #endregion
    }
}
