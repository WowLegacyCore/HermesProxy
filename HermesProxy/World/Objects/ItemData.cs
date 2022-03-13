using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class ItemEnchantment
    {
        public int? ID;
        public uint? Duration;
        public ushort? Charges;
        public ushort? Inactive;
    }
    public class ItemData
    {
        public WowGuid128 Owner;
        public WowGuid128 ContainedIn;
        public WowGuid128 Creator;
        public WowGuid128 GiftCreator;
        public uint? StackCount;
        public uint? Duration;
        public int?[] SpellCharges = new int?[5];
        public uint? Flags;
        public ItemEnchantment[] Enchantment = new ItemEnchantment[13];
        public uint? PropertySeed;
        public uint? RandomProperty;
        public uint? Durability;
        public uint? MaxDurability;
        public uint? CreatePlayedTime;
        public uint? ModifiersMask;
        public int? Context;
        public ulong? ArtifactXP;
        public uint? ItemAppearanceModID;

        // Dynamic Fields
        public bool HasGemsUpdate;
    }
}
