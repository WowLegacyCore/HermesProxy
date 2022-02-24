using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public enum SpellMissInfo
    {
        None = 0,
        Miss = 1,
        Resist = 2,
        Dodge = 3,
        Parry = 4,
        Block = 5,
        Evade = 6,
        Immune = 7,
        Immune2 = 8, // One Of These 2 Is MissTempimmune
        Deflect = 9,
        Absorb = 10,
        Reflect = 11
    }
    public enum SpellCastTargetFlags
    {
        None           = 0x00000000,
        Unused1        = 0x00000001,               // Not Used
        Unit           = 0x00000002,               // Pguid
        UnitRaid       = 0x00000004,               // Not Sent, Used To Validate Target (If Raid Member)
        UnitParty      = 0x00000008,               // Not Sent, Used To Validate Target (If Party Member)
        Item           = 0x00000010,               // Pguid
        SourceLocation = 0x00000020,               // Pguid, 3 Float
        DestLocation   = 0x00000040,               // Pguid, 3 Float
        UnitEnemy      = 0x00000080,               // Not Sent, Used To Validate Target (If Enemy)
        UnitAlly       = 0x00000100,               // Not Sent, Used To Validate Target (If Ally)
        CorpseEnemy    = 0x00000200,               // Pguid
        UnitDead       = 0x00000400,               // Not Sent, Used To Validate Target (If Dead Creature)
        GameObject     = 0x00000800,               // Pguid, Used With TargetGameobjectTarget
        TradeItem      = 0x00001000,               // Pguid
        String         = 0x00002000,               // String
        GameobjectItem = 0x00004000,               // Not Sent, Used With TargetGameobjectItemTarget
        CorpseAlly     = 0x00008000,               // Pguid
        UnitMinipet    = 0x00010000,               // Pguid, Used To Validate Target (If Non Combat Pet)
        GlyphSlot      = 0x00020000,               // Used In Glyph Spells
        DestTarget     = 0x00040000,               // Sometimes Appears With DestTarget Spells (May Appear Or Not For A Given Spell)
        ExtraTargets   = 0x00080000,               // Uint32 Counter, Loop { Vec3 - Screen Position (?), Guid }, Not Used So Far
        UnitPassenger  = 0x00100000,               // Guessed, Used To Validate Target (If Vehicle Passenger)\
        Unk400000      = 0x00400000,
        Unk1000000     = 0X01000000,
        Unk4000000     = 0X04000000,
        Unk10000000    = 0X10000000,
        Unk40000000    = 0X40000000,

        UnitMask = Unit | UnitRaid | UnitParty | UnitEnemy | UnitAlly | UnitDead | UnitMinipet | UnitPassenger,
        GameobjectMask = GameObject | GameobjectItem,
        CorpseMask = CorpseAlly | CorpseEnemy,
        ItemMask = TradeItem | Item | GameobjectItem
    }
    [Flags]
    public enum CastFlag : uint
    {
        None           = 0x00000000,
        PendingCast    = 0x00000001, // 4.x NoCombatLog
        HasTrajectory  = 0x00000002,
        Unknown2       = 0x00000004,
        Unknown3       = 0x00000008,
        Unknown4       = 0x00000010,
        Projectile     = 0x00000020,
        Unknown5       = 0x00000040,
        Unknown6       = 0x00000080,
        Unknown7       = 0x00000100,
        Unknown8       = 0x00000200,
        Unknown9       = 0x00000400,
        PredictedPower = 0x00000800,
        Unknown10      = 0x00001000,
        Unknown11      = 0x00002000,
        Unknown12      = 0x00004000,
        Unknown13      = 0x00008000,
        Unknown14      = 0x00010000,
        AdjustMissile  = 0x00020000, // 4.x
        NoGcd          = 0x00040000,
        VisualChain    = 0x00080000, // 4.x
        Unknown18      = 0x00100000,
        RuneInfo       = 0x00200000, // 4.x PredictedRunes
        Unknown19      = 0x00400000,
        Unknown20      = 0x00800000,
        Unknown21      = 0x01000000,
        Unknown22      = 0x02000000,
        Immunity       = 0x04000000, // 4.x
        Unknown23      = 0x08000000,
        Unknown24      = 0x10000000,
        Unknown25      = 0x20000000,
        HealPrediction = 0x40000000, // 4.x
        Unknown27      = 0x80000000
    }
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
