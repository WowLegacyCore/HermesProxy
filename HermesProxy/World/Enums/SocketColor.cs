using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum SocketColorLegacy : byte
    {
        None = 0,
        Meta = 1,
        Red = 2,
        Yellow = 4,
        Blue = 8
    }

    public enum SocketColorModern : byte
    {
        None = 0,
        Meta = 1,
        Red = 2,
        Yellow = 3,
        Blue = 4,
    }
}
