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

using System.IO;
using System.IO.Compression;

namespace HermesProxy.Framework.IO
{
    public static class ZLib
    {
        public static byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            using (ZLibStream zlib = new ZLibStream(ms, CompressionMode.Compress))
            {
                zlib.Write(data, 0, data.Length);
                zlib.Flush();

                return ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data, uint unpackedSize)
        {
            using (MemoryStream msIn = new MemoryStream(data))
            using (ZLibStream zlib = new ZLibStream(msIn, CompressionMode.Decompress))
            using (MemoryStream msOut = new MemoryStream())
            {
                zlib.CopyTo(msOut);
                return msOut.ToArray();
            }
        }

        //public static byte[] Compress(byte[] data)
        //{
        //    using (var writer = new PacketWriter())
        //    {
        //        writer.WriteUInt8(0x78);
        //        writer.WriteUInt8(0x9c);

        //        using (var ms = new MemoryStream())
        //        using (var deflateStream = new DeflateStream(ms, CompressionMode.Compress))
        //        {
        //            deflateStream.Write(data, 0, data.Length);
        //            deflateStream.Flush();

        //            writer.WriteBytes(ms.ToArray());
        //        }

        //        writer.WriteBytes(BitConverter.GetBytes(Adler32_Default(data)).Reverse().ToArray());

        //        return writer.GetData();
        //    }
        //}

        //public static byte[] Decompress(byte[] data, uint unpackedSize)
        //{
        //    var decompressData = new byte[unpackedSize];
        //    using (var deflateStream = new DeflateStream(new MemoryStream(data, 2, data.Length - 6), CompressionMode.Decompress))
        //    {
        //        var decompressed = new MemoryStream();
        //        deflateStream.CopyTo(decompressed);

        //        decompressed.Seek(0, SeekOrigin.Begin);
        //        for (int i = 0; i < unpackedSize; i++)
        //            decompressData[i] = (byte)decompressed.ReadByte();
        //    }

        //    return decompressData;
        //}

        //public static uint Adler32_Default(byte[] data)
        //{
        //    var a = 1u;
        //    var b = 0u;

        //    for (var i = 0; i < data.Length; i++)
        //    {
        //        a = (a + data[i]) % 0xFFF1;
        //        b = (b + a) % 0xFFF1;
        //    }
        //    return (b << 16) + a;
        //}
    }
}
