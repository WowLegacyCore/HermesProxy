using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public enum SpellCastSource
    {
        Player  = 2,
        Normal  = 3,
        Item    = 4,
        Passive = 7,
        Pet     = 9,
        Aura    = 13,
        Spell   = 16,
    }
}
