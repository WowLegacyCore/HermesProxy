using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum GossipOption
    {
        None                  = 0,                    //Unit_Npc_Flag_None                (0)
        Gossip                = 1,                    //Unit_Npc_Flag_Gossip              (1)
        Questgiver            = 2,                    //Unit_Npc_Flag_Questgiver          (2)
        Vendor                = 3,                    //Unit_Npc_Flag_Vendor              (128)
        Taxivendor            = 4,                    //Unit_Npc_Flag_Taxivendor          (8192)
        Trainer               = 5,                    //Unit_Npc_Flag_Trainer             (16)
        Spirithealer          = 6,                    //Unit_Npc_Flag_Spirithealer        (16384)
        Spiritguide           = 7,                    //Unit_Npc_Flag_Spiritguide         (32768)
        Innkeeper             = 8,                    //Unit_Npc_Flag_Innkeeper           (65536)
        Banker                = 9,                    //Unit_Npc_Flag_Banker              (131072)
        Petitioner            = 10,                   //Unit_Npc_Flag_Petitioner          (262144)
        Tabarddesigner        = 11,                   //Unit_Npc_Flag_Tabarddesigner      (524288)
        Battlefield           = 12,                   //Unit_Npc_Flag_Battlefieldperson   (1048576)
        Auctioneer            = 13,                   //Unit_Npc_Flag_Auctioneer          (2097152)
        Stablepet             = 14,                   //Unit_Npc_Flag_Stable              (4194304)
        Armorer               = 15,                   //Unit_Npc_Flag_Armorer             (4096)
        Unlearntalents        = 16,                   //Unit_Npc_Flag_Trainer             (16) (Bonus Option For Trainer)
        Unlearnpettalents_Old = 17,                   // deprecated
        Learndualspec         = 18,                   //Unit_Npc_Flag_Trainer             (16) (Bonus Option For Trainer)
        Outdoorpvp            = 19,                   //Added By Code (Option For Outdoor Pvp Creatures)
        Transmogrifier        = 20,                   //UNIT_NPC_FLAG_TRANSMOGRIFIER
        Max
    }

    public enum GossipOptionIcon
    {
        Chat      = 0,                    // White Chat Bubble
        Vendor    = 1,                    // Brown Bag
        Taxi      = 2,                    // Flightmarker (Paperplane)
        Trainer   = 3,                    // Brown Book (Trainer)
        Interact1 = 4,                    // Golden Interaction Wheel
        Interact2 = 5,                    // Golden Interaction Wheel
        MoneyBag  = 6,                    // Brown Bag (With Gold Coin In Lower Corner)
        Talk      = 7,                    // White Chat Bubble (With "..." Inside)
        Tabard    = 8,                    // White Tabard
        Battle    = 9,                    // Two Crossed Swords
        Dot       = 10,                   // Yellow Dot/Point
        Chat11    = 11,                   // White Chat Bubble
        Chat12    = 12,                   // White Chat Bubble
        Chat13    = 13,                   // White Chat Bubble
        Unk14     = 14,                   // Invalid - Do Not Use
        Unk15     = 15,                   // Invalid - Do Not Use
        Chat16    = 16,                   // White Chat Bubble
        Chat17    = 17,                   // White Chat Bubble
        Chat18    = 18,                   // White Chat Bubble
        Chat19    = 19,                   // White Chat Bubble
        Chat20    = 20,                   // White Chat Bubble
        Chat21    = 21,                   // transmogrifier?
        Max
    }
    public enum GossipOptionStatus
    {
        Available = 0,
        Unavailable = 1,
        Locked = 2,
        AlreadyComplete = 3
    }

    public enum GossipOptionRewardType
    {
        Item = 0,
        Currency = 1
    }
}
