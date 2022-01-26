using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.Enums
{
    [Flags]
    public enum RealmFlags
    {
        None         = 0x00,
        Invalid      = 0x01,
        Offline      = 0x02,
        SpecifyBuild = 0x04,                         // client will show realm version in RealmList screen in form "RealmName (major.minor.revision.build)"
        Unk1         = 0x08,
        Unk2         = 0x10,
        NewPlayers   = 0x20,
        Recommended  = 0x40,
        Full         = 0x80
    };
}
