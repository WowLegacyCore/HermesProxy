using System;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum HitInfoVanilla : uint
    {
        None          = 0x00000000,
        Unk0          = 0x00000001,
        AffectsVictim = 0x00000002,
        OffHand       = 0x00000004,
        Unk3          = 0x00000008,
        Miss          = 0x00000010,
        FullAbsorb    = 0x00000020, // plays absorb sound
        FullResist    = 0x00000040, // resisted atleast some damage
        CriticalHit   = 0x00000080,
        Block         = 0x00000800,
        Glancing      = 0x00004000,
        Crushing      = 0x00008000,
        NoAnimation   = 0x00010000,
        NoHitSound    = 0x00080000
    };

    [Flags]
    public enum HitInfo : uint
    {
        None          = 0x00000000,
        Unk0          = 0x00000001, // unused - debug flag, probably debugging visuals, no effect in non-ptr client
        AffectsVictim = 0x00000002,
        OffHand       = 0x00000004,
        Unk3          = 0x00000008, // unused (3.3.5a)
        Miss          = 0x00000010,
        FullAbsorb    = 0x00000020,
        PartialAbsorb = 0x00000040,
        FullResist    = 0x00000080,
        PartialResist = 0x00000100,
        CriticalHit   = 0x00000200,
        Unk10         = 0x00000400,
        Unk11         = 0x00000800,
        Unk12         = 0x00001000,
        Block         = 0x00002000,
        Unk14         = 0x00004000, // set only if meleespellid is present//  no world text when victim is hit for 0 dmg(HideWorldTextForNoDamage?)
        Unk15         = 0x00008000, // player victim?// something related to blod sprut visual (BloodSpurtInBack?)
        Glancing      = 0x00010000,
        Crushing      = 0x00020000,
        NoAnimation   = 0x00040000, // set always for melee spells and when no hit animation should be displayed
        Unk19         = 0x00080000,
        Unk20         = 0x00100000,
        NoHitSound    = 0x00200000, // unused (3.3.5a)
        Unk22         = 0x00400000,
        RageGain      = 0x00800000,
        FakeDamage    = 0x01000000, // enables damage animation even if no damage done, set only if no damage
        Unk25         = 0x02000000,
        Unk26         = 0x04000000
    };
}
