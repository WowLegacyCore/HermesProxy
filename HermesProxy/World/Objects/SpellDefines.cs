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

    [Flags]
    public enum AuraFlagsVanilla : ushort
    {
        None         = 0x00,
        Cancelable   = 0x01,
        EffectIndex2 = 0x02,
        EffectIndex1 = 0x04,
        EffectIndex0 = 0x08,
    };

    [Flags]
    public enum AuraFlagsTBC : ushort
    {
        None          = 0x00,
        Positive      = 0x01,
        Negative      = 0x02,
        Passive       = 0x04,     // Pre-WotLK: debuffs can't be queried using this flag. Unused in UI since 1.10.0, new meaning unknown (still the same?)
        Unk4          = 0x08,     // Pre-WotLK: unused in UI
        Cancelable    = 0x10,
        NotCancelable = 0x20,
    };

    [Flags]
    public enum AuraFlagsWotLK : ushort
    {
        None         = 0x00,
        EffectIndex0 = 0x01,
        EffectIndex1 = 0x02,
        EffectIndex2 = 0x04,
        NoCaster     = 0x08,
        Positive     = 0x10,
        Duration     = 0x20,
        Unk2         = 0x40,
        Negative     = 0x80
    };

    [Flags]
    public enum AuraFlagsModern : ushort
    {
        None       = 0x00,
        NoCaster   = 0x01,
        Cancelable = 0x02,
        Duration   = 0x04,
        Scalable   = 0x08,
        Negative   = 0x10,
        Unk20      = 0x20,
        Unk40      = 0x40,
        Unk80      = 0x80,
        Positive   = 0x100,
        Passive    = 0x200
    }
}
