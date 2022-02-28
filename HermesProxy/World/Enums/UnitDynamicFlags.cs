using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum UnitDynamicFlagsLegacy : uint
    {
        None                  = 0x0000,
        Lootable              = 0x0001,
        TrackUnit             = 0x0002,
        Tapped                = 0x0004,
        TappedByPlayer        = 0x0008,
        EmpathyInfo           = 0x0010,
        AppearDead            = 0x0020,
        ReferAFriendLinked    = 0x0040,
    }

    [Flags]
    public enum UnitDynamicFlagsModern : uint
    {
        None                  = 0x0000,
        HideModel             = 0x0002,
        Lootable              = 0x0004,
        TrackUnit             = 0x0008,
        Tapped                = 0x0010,
        EmpathyInfo           = 0x0020,
        AppearDead            = 0x0040,
        ReferAFriendLinked    = 0x0080,
    }
}
