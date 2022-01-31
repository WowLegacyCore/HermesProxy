using System.IO;
using System.IO.Compression;

namespace HermesProxy.Framework.Util
{
    internal class ZLib
    {
        public static byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZLibStream zlib = new ZLibStream(ms, CompressionMode.Compress))
                {
                    zlib.Write(data, 0, data.Length);
                    zlib.Flush();
                }
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
    }
}
