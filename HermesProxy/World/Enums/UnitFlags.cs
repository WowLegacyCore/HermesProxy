using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum UnitFlagsVanilla : uint
    {
        None                = 0x00000000,
        ServerControlled    = 0x00000001,           // Movement checks disabled, likely paired with loss of client control packet.
        Spawning            = 0x00000002,           // not attackable
        RemoveClientControl = 0x00000004,
        PlayerControlled    = 0x00000008,           // players, pets, totems, guardians, companions, charms, any units associated with players
        PetRename           = 0x00000010,           // Old pet rename: moved to UNIT_FIELD_BYTES_2,2 in TBC+
        PetAbandon          = 0x00000020,           // Old pet abandon: moved to UNIT_FIELD_BYTES_2,2 in TBC+
        Unk6                = 0x00000040,
        NotAttackable1      = 0x00000080,           // ?? (UNIT_FLAG_PLAYER_CONTROLLED | UNIT_FLAG_NOT_ATTACKABLE_1) is NON_PVP_ATTACKABLE
        ImmuneToPc          = 0x00000100,           // Target is immune to players
        ImmuneToNpc         = 0x00000200,           // Target is immune to creatures
        Looting             = 0x00000400,           // loot animation
        PetInCombat         = 0x00000800,           // in combat?, 2.0.8
        Pvp                 = 0x00001000,
        Silenced            = 0x00002000,           // silenced, 2.1.1
        CannotSwim          = 0x00004000,
        CanSwim             = 0x00008000,
        NotAttackable2      = 0x00010000,           // removes attackable icon, if on yourself, cannot assist self but can cast TARGET_UNIT_CASTER spells - added by SPELL_AURA_MOD_UNATTACKABLE
        Pacified            = 0x00020000,
        Stunned             = 0x00040000,           // Unit is a subject to stun, turn and strafe movement disabled
        InCombat            = 0x00080000,
        TaxiFlight          = 0x00100000,           // Unit is on taxi, paired with a duplicate loss of client control packet (likely a legacy serverside hack). Disables any spellcasts not allowed in taxi flight client-side.
        Disarmed            = 0x00200000,           // disable melee spells casting..., "Required melee weapon" added to melee spells tooltip.
        Confused            = 0x00400000,           // Unit is a subject to confused movement, movement checks disabled, paired with loss of client control packet.
        Fleeing             = 0x00800000,           // Unit is a subject to fleeing movement, movement checks disabled, paired with loss of client control packet.
        Possessed           = 0x01000000,           // Unit is under remote control by another unit, movement checks disabled, paired with loss of client control packet. New master is allowed to use melee attack and can't select this unit via mouse in the world (as if it was own character).
        NotSelectable       = 0x02000000,
        Skinnable           = 0x04000000,
        AurasVisible        = 0x08000000,           // magic detect
        Unk28               = 0x10000000,
        PreventAnim         = 0x20000000,           // Prevent automatically playing emotes from parsing chat text, for example "lol" in /say, ending message with ? or !, or using /yell
        Sheathe             = 0x40000000,
        Immune              = 0x80000000,           // Immune to damage
    };

    [Flags]
    public enum UnitFlags : uint
    {
        None                = 0x00000000,
        ServerControlled    = 0x00000001,
        Spawning            = 0x00000002,
        RemoveClientControl = 0x00000004, // This is a legacy flag used to disable movement player's movement while controlling other units, SMSG_CLIENT_CONTROL replaces this functionality clientside now. CONFUSED and FLEEING flags have the same effect on client movement asDISABLE_MOVE_CONTROL in addition to preventing spell casts/autoattack (they all allow climbing steeper hills and emotes while moving)
        PlayerControlled    = 0x00000008,
        Rename              = 0x00000010,
        Preparation         = 0x00000020,
        Unk6                = 0x00000040,
        NotAttackable1      = 0x00000080,
        ImmuneToPc          = 0x00000100,
        ImmuneToNpc         = 0x00000200,
        Looting             = 0x00000400,
        PetInCombat         = 0x00000800,
        Pvp                 = 0x00001000,
        Silenced            = 0x00002000,
        CannotSwim          = 0x00004000,
        CanSwim             = 0x00008000,
        NotAttackable2      = 0x00010000,
        Pacified            = 0x00020000,
        Stunned             = 0x00040000,
        InCombat            = 0x00080000,
        TaxiFlight          = 0x00100000,
        Disarmed            = 0x00200000,
        Confused            = 0x00400000,
        Fleeing             = 0x00800000,
        Possessed           = 0x01000000,
        NotSelectable       = 0x02000000,
        Skinnable           = 0x04000000,
        Mount               = 0x08000000,
        Unk28               = 0x10000000,
        PreventAnim         = 0x20000000,
        Sheathe             = 0x40000000,
        Immune              = 0x80000000
    }

    [Flags]
    public enum UnitFlags2 : uint
    {
        FeignDeath                      = 0x00000001,
        HideBody                        = 0x00000002,   // TITLE Hide Body DESCRIPTION Hide unit model (show only player equip)
        IgnoreReputation                = 0x00000004,
        ComprehendLang                  = 0x00000008,
        MirrorImage                     = 0x00000010,
        DontFadeIn                      = 0x00000020,   // TITLE Don't Fade In DESCRIPTION Unit model instantly appears when summoned (does not fade in)
        ForceMovement                   = 0x00000040,
        DisarmOffhand                   = 0x00000080,
        DisablePredStats                = 0x00000100,   // Player has disabled predicted stats (Used by raid frames)
        AllowChangingTalents            = 0x00000200,   // Allows changing talents outside rest area
        DisarmRanged                    = 0x00000400,   // this does not disable ranged weapon display (maybe additional flag needed?)
        RegeneratePower                 = 0x00000800,
        RestrictPartyInteraction        = 0x00001000,   // Restrict interaction to party or raid
        PreventSpellClick               = 0x00002000,   // Prevent spellclick
        InteractWhileHostile            = 0x00004000,   // TITLE Interact while Hostile
        CannotTurn                      = 0x00008000,   // TITLE Cannot Turn
        Unk2                            = 0x00010000,
        PlayDeathAnim                   = 0x00020000,   // Plays special death animation upon death
        AllowCheatSpells                = 0x00040000,   // Allows casting spells with AttributesEx7 & SPELL_ATTR7_IS_CHEAT_SPELL
        SuppressHighlight               = 0x00080000,   // TITLE Suppress highlight when targeted or moused over
        TreatAsRaidUnit                 = 0x00100000,   // TITLE Treat as Raid Unit For Helpful Spells (Instances ONLY)
        LargeAOI                        = 0x00200000,   // TITLE Large (AOI)
        GiganticAOI                     = 0x00400000,   // TITLE Gigantic (AOI)
        NoActions                       = 0x00800000,
        AIOnlySwimIfTargetSwim          = 0x01000000,   // TITLE AI will only swim if target swims
        NoCombatLogWithNPCs             = 0x02000000,   // TITLE Don't generate combat log when engaged with NPC's
        UntargetableByClient            = 0x04000000,   // TITLE Untargetable By Client
        AttackerIgnoresMinimumRanges    = 0x08000000,   // TITLE Attacker Ignores Minimum Ranges
        UninteractibleIfHostile         = 0x10000000,   // TITLE Uninteractible If Hostile
        Unused11                        = 0x20000000,
        InfiniteAOI                     = 0x40000000,   // TITLE Infinite (AOI)
        Unused13                        = 0x80000000,
    }
}
