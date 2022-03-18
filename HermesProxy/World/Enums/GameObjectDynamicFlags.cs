using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum GameObjectDynamicFlagsLegacy : uint
    {
        Activate          = 0x001,               // enables interaction with GO
        Animate           = 0x002,               // possibly more distinct animation of GO
        NoInteract        = 0x004,               // appears to disable interaction (not fully verified)
        Sparkle           = 0x008,               // makes GO sparkle
        Stopped           = 0x010                // Transport is stopped
    };

    [Flags]
    public enum GameObjectDynamicFlagsModern : uint
    {
        HideModel         = 0x002,               // Object model is not shown with this flag
        Activate          = 0x004,               // enables interaction with GO
        Animate           = 0x008,               // possibly more distinct animation of GO
        Depleted          = 0x010,               // can no longer be interacted with (and for gathering nodes it forces "open" visual state)
        Sparkle           = 0x020,               // makes GO sparkle
        Stopped           = 0x040,               // Transport is stopped
        NoInteract        = 0x080,
        InvertedMovement  = 0x100,               // GAMEOBJECT_TYPE_TRANSPORT only
        LoHighlight       = 0x200,               // Allows object highlight when GO_DYNFLAG_LO_ACTIVATE or GO_DYNFLAG_LO_SPARKLE are set, not only when player is on quest determined by Data fields
    };
}
