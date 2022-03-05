using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum PetTalk
    {
        SpecialSpell = 0,
        Attack = 1
    }

    public enum CommandStates
    {
        Stay = 0,
        Follow = 1,
        Attack = 2,
        Abandon = 3,
        MoveTo = 4
    }

    [Flags]
    public enum PetModeFlags
    {
        Unknown1       = 0x0010000,
        Unknown2       = 0x0020000,
        Unknown3       = 0x0040000,
        Unknown4       = 0x0080000,
        Unknown5       = 0x0100000,
        Unknown6       = 0x0200000,
        Unknown7       = 0x0400000,
        Unknown8       = 0x0800000,
        Unknown9       = 0x1000000,
        Unknown10      = 0x2000000,
        Unknown11      = 0x4000000,
        DisableActions = 0x8000000
    }
}
