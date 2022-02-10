using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Util
{
    public static class Utilities
    {
        public static ulong MAKE_PAIR64(uint l, uint h)
        {
            return (l | (ulong)h << 32);
        }

        public static uint PAIR64_HIPART(ulong x)
        {
            return (uint)((x >> 32) & 0x00000000FFFFFFFF);
        }

        public static uint PAIR64_LOPART(ulong x)
        {
            return (uint)(x & 0x00000000FFFFFFFF);
        }
    }
}
