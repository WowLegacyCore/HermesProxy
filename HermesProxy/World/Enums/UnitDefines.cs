using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum Race
    {
        None                = 0,
        Human               = 1,
        Orc                 = 2,
        Dwarf               = 3,
        NightElf            = 4,
        Undead              = 5, // Scourge
        Tauren              = 6,
        Gnome               = 7,
        Troll               = 8,
        Goblin              = 9,
        BloodElf            = 10,
        Draenei             = 11,
        FelOrc              = 12,
        Naga                = 13,
        Broken              = 14,
        Skeleton            = 15,
        Vrykul              = 16,
        Tuskarr             = 17,
        ForestTroll         = 18,
        Taunka              = 19,
        NorthrendSkeleton   = 20,
        IceTroll            = 21,
        Worgen              = 22,
        Gilnean             = 23, // Human
        PandarenNeutral     = 24,
        PandarenAlliance    = 25,
        PandarenHorde       = 26,
        Nightborne          = 27,
        HighmountainTauren  = 28,
        VoidElf             = 29,
        LightforgedDraenei  = 30,
        ZandalariTroll      = 31,
        KulTiran            = 32,
        ThinHuman           = 33,
        DarkIronDwarf       = 34,
        Vulpera             = 35,
        MagharOrc           = 36,
        Mechagnome          = 37
    }

    public enum Class
    {
        None        = 0,
        Warrior     = 1,
        Paladin     = 2,
        Hunter      = 3,
        Rogue       = 4,
        Priest      = 5,
        Deathknight = 6,
        Shaman      = 7,
        Mage        = 8,
        Warlock     = 9,
        Monk        = 10,
        Druid       = 11,
        DemonHunter = 12,
        Max         = 13,

        ClassMaskAllPlayable = ((1 << (Warrior - 1)) | (1 << (Paladin - 1)) | (1 << (Hunter - 1)) |
            (1 << (Rogue - 1)) | (1 << (Priest - 1)) | (1 << (Deathknight - 1)) | (1 << (Shaman - 1)) |
            (1 << (Mage - 1)) | (1 << (Warlock - 1)) | (1 << (Monk - 1)) | (1 << (Druid - 1)) | (1 << (DemonHunter - 1))),

        ClassMaskAllCreatures = ((1 << (Warrior - 1)) | (1 << (Paladin - 1)) | (1 << (Rogue - 1)) | (1 << (Mage - 1))),

        ClassMaskWandUsers = ((1 << (Priest - 1)) | (1 << (Mage - 1)) | (1 << (Warlock - 1)))
    }

    public enum Gender
    {
        Male   = 0,
        Female = 1,
        None   = 2
    }

    public enum ReactStates
    {
        Passive    = 0,
        Defensive  = 1,
        Aggressive = 2,
        Assist     = 3
    }
}
