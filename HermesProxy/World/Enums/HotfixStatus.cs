using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum HotfixStatus : byte
    {
        Valid         = 1,
        RecordRemoved = 2,
        Invalid       = 3,
        NotPublic     = 4,
    }
}
