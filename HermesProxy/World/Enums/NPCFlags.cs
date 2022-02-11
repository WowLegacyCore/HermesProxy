using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum NPCFlagsVanilla
    {
        None                  = 0x00000000,
        Gossip                = 0x00000001,       // 100%
        QuestGiver            = 0x00000002,       // 100%
        Vendor                = 0x00000004,       // 100%
        FlightMaster          = 0x00000008,       // 100%
        Trainer               = 0x00000010,       // 100%
        SpiritHealer          = 0x00000020,       // guessed
        SpiritGuide           = 0x00000040,       // guessed
        Innkeeper             = 0x00000080,       // 100%
        Banker                = 0x00000100,       // 100%
        Petitioner            = 0x00000200,       // 100% 0xC0000 = guild petitions
        TabardDesigner        = 0x00000400,       // 100%
        BattleMaster          = 0x00000800,       // 100%
        Auctioneer            = 0x00001000,       // 100%
        StableMaster          = 0x00002000,       // 100%
        Repair                = 0x00004000,       // 100%
    };

    [Flags]
    public enum NPCFlags : uint
    {
        None                = 0x00000000,
        Gossip              = 0x00000001,     // 100%
        QuestGiver          = 0x00000002,     // 100%
        Unk1                = 0x00000004,
        Unk2                = 0x00000008,
        Trainer             = 0x00000010,     // 100%
        TrainerClass        = 0x00000020,     // 100%
        TrainerProfession   = 0x00000040,     // 100%
        Vendor              = 0x00000080,     // 100%
        VendorAmmo          = 0x00000100,     // 100%, General Goods Vendor
        VendorFood          = 0x00000200,     // 100%
        VendorPoison        = 0x00000400,     // Guessed
        VendorReagent       = 0x00000800,     // 100%
        Repair              = 0x00001000,     // 100%
        FlightMaster        = 0x00002000,     // 100%
        SpiritHealer        = 0x00004000,     // Guessed
        SpiritGuide         = 0x00008000,     // Guessed
        Innkeeper           = 0x00010000,     // 100%
        Banker              = 0x00020000,     // 100%
        Petitioner          = 0x00040000,     // 100% 0xc0000 = Guild Petitions, 0x40000 = Arena Team Petitions
        TabardDesigner      = 0x00080000,     // 100%
        BattleMaster        = 0x00100000,     // 100%
        Auctioneer          = 0x00200000,     // 100%
        StableMaster        = 0x00400000,     // 100%
        GuildBanker         = 0x00800000,     //
        SpellClick          = 0x01000000,     //
        PlayerVehicle       = 0x02000000,     // Players With Mounts That Have Vehicle Data Should Have It Set
        Mailbox             = 0x04000000,     // Mailbox
        ArtifactPowerRespec = 0x08000000,     // Artifact Powers Reset
        Transmogrifier      = 0x10000000,     // Transmogrification
        VaultKeeper         = 0x20000000,     // Void Storage
        WildBattlePet       = 0x40000000,     // Pet That Player Can Fight (Battle Pet)
        BlackMarket         = 0x80000000,     // Black Market
    }
}
