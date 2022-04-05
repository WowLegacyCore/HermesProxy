using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum LootMethod
    {
        FreeForAll = 0,
        MasterLoot = 2,
        GroupLoot = 3,
        PersonalLoot = 5
    }

    public enum LootType
    {
        None = 0,
        Corpse = 1,
        Pickpocketing = 2,
        Fishing = 3,
        Disenchanting = 4,
        // Ignored Always By Client
        Skinning = 6,
        Prospecting = 7,
        Milling = 8,
    }

    public enum LootError
    {
        DidntKill = 0,    // You don't have permission to loot that corpse.
        TooFar = 4,    // You are too far away to loot that corpse.
        BadFacing = 5,    // You must be facing the corpse to loot it.
        Locked = 6,    // Someone is already looting that corpse.
        NotStanding = 8,    // You need to be standing up to loot something!
        Stunned = 9,    // You can't loot anything while stunned!
        PlayerNotFound = 10,   // Player not found
        PlayTimeExceeded = 11,   // Maximum play time exceeded
        MasterInvFull = 12,   // That player's inventory is full
        MasterUniqueItem = 13,   // Player has too many of that item already
        MasterOther = 14,   // Can't assign item to that player
        AlreadPickPocketed = 15,   // Your target has already had its pockets picked
        NotWhileShapeShifted = 16,    // You can't do that while shapeshifted.
        NoLoot = 17    // There is no loot.
    }

    // type of Loot Item in Loot View
    public enum LootSlotTypeLegacy : uint
    {
        AllowLoot = 0,
        RollOngoing = 1,
        Master = 2,
        Locked = 3,
        Owner = 4
    }
    public enum LootSlotTypeModern : uint
    {
        AllowLoot = 0,                     // Player Can Loot The Item.
        RollOngoing = 1,                   // Roll Is Ongoing. Player Cannot Loot.
        Locked = 2,                        // Item Is Shown In Red. Player Cannot Loot.
        Master = 3,                        // Item Can Only Be Distributed By Group Loot Master.
        Owner = 4                          // Ignore Binding Confirmation And Etc, For Single Player Looting
    }
    public enum RollMask
    {
        Pass = 0x01,
        Need = 0x02,
        Greed = 0x04,
        Disenchant = 0x08,

        AllNoDisenchant = 0x07,
        AllMask = 0x0f
    }

    public enum RollType
    {
        Pass = 0,
        Need = 1,
        Greed = 2,
        Disenchant = 3,
        NotEmitedYet = 4,
        NotValid = 5,

        MaxTypes = 4,
    }
}
