/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class SetProficiency : ServerPacket
    {
        public SetProficiency() : base(Opcode.SMSG_SET_PROFICIENCY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(ProficiencyMask);
            _worldPacket.WriteUInt8(ProficiencyClass);
        }

        public uint ProficiencyMask;
        public byte ProficiencyClass;
    }

    public class BuyBackItem : ClientPacket
    {
        public BuyBackItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VendorGUID = _worldPacket.ReadPackedGuid128();
            Slot = _worldPacket.ReadUInt32();
        }

        public WowGuid128 VendorGUID;
        public uint Slot;
    }

    public class BuyItem : ClientPacket
    {
        public BuyItem(WorldPacket packet) : base(packet)
        {
            Item = new ItemInstance();
        }

        public override void Read()
        {
            VendorGUID = _worldPacket.ReadPackedGuid128();
            ContainerGUID = _worldPacket.ReadPackedGuid128();
            Quantity = _worldPacket.ReadUInt32();
            Slot = _worldPacket.ReadUInt32();
            BagSlot = _worldPacket.ReadUInt32();
            Item.Read(_worldPacket);
            ItemType = (ItemVendorType)_worldPacket.ReadBits<int>(3);
        }

        public WowGuid128 VendorGUID;
        public ItemInstance Item;
        public uint Slot;
        public uint BagSlot;
        public ItemVendorType ItemType;
        public uint Quantity;
        public WowGuid128 ContainerGUID;
    }

    public class BuySucceeded : ServerPacket
    {
        public BuySucceeded() : base(Opcode.SMSG_BUY_SUCCEEDED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(VendorGUID);
            _worldPacket.WriteUInt32(Slot);
            _worldPacket.WriteInt32(NewQuantity);
            _worldPacket.WriteUInt32(QuantityBought);
        }

        public WowGuid128 VendorGUID;
        public uint Slot;
        public int NewQuantity;
        public uint QuantityBought;
    }

    public class BuyFailed : ServerPacket
    {
        public BuyFailed() : base(Opcode.SMSG_BUY_FAILED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(VendorGUID);
            _worldPacket.WriteUInt32(Slot);
            _worldPacket.WriteUInt8((byte)Reason);
        }

        public WowGuid128 VendorGUID;
        public uint Slot;
        public BuyResult Reason = BuyResult.CantFindItem;
    }

    class ItemPushResult : ServerPacket
    {
        public ItemPushResult() : base(Opcode.SMSG_ITEM_PUSH_RESULT) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PlayerGUID);
            _worldPacket.WriteUInt8(Slot);
            _worldPacket.WriteInt32(SlotInBag);
            _worldPacket.WriteInt32(QuestLogItemID);
            _worldPacket.WriteUInt32(Quantity);
            _worldPacket.WriteUInt32(QuantityInInventory);
            _worldPacket.WriteInt32(DungeonEncounterID);
            _worldPacket.WriteInt32(BattlePetSpeciesID);
            _worldPacket.WriteInt32(BattlePetBreedID);
            _worldPacket.WriteUInt32(BattlePetBreedQuality);
            _worldPacket.WriteInt32(BattlePetLevel);
            _worldPacket.WritePackedGuid128(ItemGUID);
            _worldPacket.WriteBit(Pushed);
            _worldPacket.WriteBit(Created);
            _worldPacket.WriteBits((uint)DisplayText, 3);
            _worldPacket.WriteBit(IsBonusRoll);
            _worldPacket.WriteBit(IsEncounterLoot);
            _worldPacket.FlushBits();

            Item.Write(_worldPacket);
        }

        public WowGuid128 PlayerGUID;
        public byte Slot;
        public int SlotInBag;
        public ItemInstance Item = new();
        public int QuestLogItemID;// Item ID used for updating quest progress
                                  // only set if different than real ID (similar to CreatureTemplate.KillCredit)
        public uint Quantity;
        public uint QuantityInInventory;
        public int DungeonEncounterID;
        public int BattlePetSpeciesID;
        public int BattlePetBreedID;
        public uint BattlePetBreedQuality;
        public int BattlePetLevel;
        public WowGuid128 ItemGUID;
        public bool Pushed;
        public DisplayType DisplayText;
        public bool Created;
        public bool IsBonusRoll;
        public bool IsEncounterLoot;

        public enum DisplayType
        {
            Hidden = 0,
            Received = 1,
            EncounterLoot = 2,
            Loot = 3,
        }
    }

    public class SellItem : ClientPacket
    {
        public SellItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VendorGUID = _worldPacket.ReadPackedGuid128();
            ItemGUID = _worldPacket.ReadPackedGuid128();
            Amount = _worldPacket.ReadUInt32();
        }

        public WowGuid128 VendorGUID;
        public WowGuid128 ItemGUID;
        public uint Amount;
    }

    public class SellResponse : ServerPacket
    {
        public SellResponse() : base(Opcode.SMSG_SELL_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(VendorGUID);
            _worldPacket.WritePackedGuid128(ItemGUID);
            _worldPacket.WriteUInt8(Reason);
        }

        public WowGuid128 VendorGUID;
        public WowGuid128 ItemGUID;
        public byte Reason;
    }

    public class SplitItem : ClientPacket
    {
        public SplitItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Inv = new InvUpdate(_worldPacket);
            FromPackSlot = _worldPacket.ReadUInt8();
            FromSlot = _worldPacket.ReadUInt8();
            ToPackSlot = _worldPacket.ReadUInt8();
            ToSlot = _worldPacket.ReadUInt8();
            Quantity = _worldPacket.ReadInt32();
        }

        public byte ToSlot;
        public byte ToPackSlot;
        public byte FromPackSlot;
        public int Quantity;
        public InvUpdate Inv;
        public byte FromSlot;
    }

    public class SwapInvItem : ClientPacket
    {
        public SwapInvItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Inv = new InvUpdate(_worldPacket);
            Slot2 = _worldPacket.ReadUInt8();
            Slot1 = _worldPacket.ReadUInt8();
        }

        public InvUpdate Inv;
        public byte Slot1; // Source Slot
        public byte Slot2; // Destination Slot
    }

    public class SwapItem : ClientPacket
    {
        public SwapItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Inv = new InvUpdate(_worldPacket);
            ContainerSlotB = _worldPacket.ReadUInt8();
            ContainerSlotA = _worldPacket.ReadUInt8();
            SlotB = _worldPacket.ReadUInt8();
            SlotA = _worldPacket.ReadUInt8();
        }

        public InvUpdate Inv;
        public byte SlotA;
        public byte ContainerSlotB;
        public byte SlotB;
        public byte ContainerSlotA;
    }

    public class AutoEquipItem : ClientPacket
    {
        public AutoEquipItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Inv = new InvUpdate(_worldPacket);
            PackSlot = _worldPacket.ReadUInt8();
            Slot = _worldPacket.ReadUInt8();
        }

        public byte Slot;
        public InvUpdate Inv;
        public byte PackSlot;
    }

    class AutoEquipItemSlot : ClientPacket
    {
        public AutoEquipItemSlot(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Inv = new InvUpdate(_worldPacket);
            Item = _worldPacket.ReadPackedGuid128();
            ItemDstSlot = _worldPacket.ReadUInt8();
        }

        public WowGuid128 Item;
        public byte ItemDstSlot;
        public InvUpdate Inv;
    }

    public struct InvUpdate
    {
        public InvUpdate(WorldPacket data)
        {
            Items = new List<InvItem>();
            int size = data.ReadBits<int>(2);
            data.ResetBitPos();
            for (int i = 0; i < size; ++i)
            {
                var item = new InvItem
                {
                    ContainerSlot = data.ReadUInt8(),
                    Slot = data.ReadUInt8()
                };
                Items.Add(item);
            }
        }

        public List<InvItem> Items;

        public struct InvItem
        {
            public byte ContainerSlot;
            public byte Slot;
        }
    }

    public class DestroyItem : ClientPacket
    {
        public DestroyItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Count = _worldPacket.ReadUInt32();
            ContainerId = _worldPacket.ReadUInt8();
            SlotNum = _worldPacket.ReadUInt8();
        }

        public uint Count;
        public byte SlotNum;
        public byte ContainerId;
    }

    public class ItemInstance
    {
        public uint ItemID;
        public uint RandomPropertiesSeed;
        public uint RandomPropertiesID;
        public ItemBonuses ItemBonus;
        public ItemModList Modifications = new();

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(ItemID);
            data.WriteUInt32(RandomPropertiesSeed);
            data.WriteUInt32(RandomPropertiesID);

            data.WriteBit(ItemBonus != null);
            data.FlushBits();

            Modifications.Write(data);

            if (ItemBonus != null)
                ItemBonus.Write(data);
        }

        public void Read(WorldPacket data)
        {
            ItemID = data.ReadUInt32();
            RandomPropertiesSeed = data.ReadUInt32();
            RandomPropertiesID = data.ReadUInt32();

            if (data.HasBit())
                ItemBonus = new();
            data.ResetBitPos();

            Modifications.Read(data);

            if (ItemBonus != null)
                ItemBonus.Read(data);
        }
    }

    public class ItemBonuses
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8((byte)Context);
            data.WriteInt32(BonusListIDs.Count);
            foreach (uint bonusID in BonusListIDs)
                data.WriteUInt32(bonusID);
        }

        public void Read(WorldPacket data)
        {
            Context = (ItemContext)data.ReadUInt8();
            uint bonusListIdSize = data.ReadUInt32();

            BonusListIDs = new List<uint>();
            for (uint i = 0u; i < bonusListIdSize; ++i)
            {
                uint bonusId = data.ReadUInt32();
                BonusListIDs.Add(bonusId);
            }
        }

        public ItemContext Context;
        public List<uint> BonusListIDs = new();
    }

    public class ItemMod
    {
        public uint Value;
        public ItemModifier Type;

        public ItemMod()
        {
            Type = ItemModifier.Max;
        }
        public ItemMod(uint value, ItemModifier type)
        {
            Value = value;
            Type = type;
        }

        public void Read(WorldPacket data)
        {
            Value = data.ReadUInt32();
            Type = (ItemModifier)data.ReadUInt8();
        }

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Value);
            data.WriteUInt8((byte)Type);
        }
    }

    public class ItemModList
    {
        public Array<ItemMod> Values = new((int)ItemModifier.Max);

        public void Read(WorldPacket data)
        {
            var itemModListCount = data.ReadBits<uint>(6);
            data.ResetBitPos();

            for (var i = 0; i < itemModListCount; ++i)
            {
                var itemMod = new ItemMod();
                itemMod.Read(data);
                Values[i] = itemMod;
            }
        }

        public void Write(WorldPacket data)
        {
            data.WriteBits(Values.Count, 6);
            data.FlushBits();

            foreach (ItemMod itemMod in Values)
                itemMod.Write(data);
        }
    }

    class ReadItem : ClientPacket
    {
        public ReadItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PackSlot = _worldPacket.ReadUInt8();
            Slot = _worldPacket.ReadUInt8();
        }

        public byte PackSlot;
        public byte Slot;
    }

    class ReadItemResultFailed : ServerPacket
    {
        public ReadItemResultFailed() : base(Opcode.SMSG_READ_ITEM_RESULT_FAILED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ItemGUID);
            _worldPacket.WriteUInt32(Delay);
            _worldPacket.WriteBits(Subcode, 2);
            _worldPacket.FlushBits();
        }

        public WowGuid128 ItemGUID;
        public uint Delay;
        public byte Subcode;
    }

    class ReadItemResultOK : ServerPacket
    {
        public ReadItemResultOK() : base(Opcode.SMSG_READ_ITEM_RESULT_OK) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ItemGUID);
        }

        public WowGuid128 ItemGUID;
    }

    public class InventoryChangeFailure : ServerPacket
    {
        public InventoryChangeFailure() : base(Opcode.SMSG_INVENTORY_CHANGE_FAILURE) { }

        public override void Write()
        {
            _worldPacket.WriteInt8((sbyte)BagResult);
            _worldPacket.WritePackedGuid128(Item[0]);
            _worldPacket.WritePackedGuid128(Item[1]);
            _worldPacket.WriteUInt8(ContainerBSlot); // bag type subclass, used with EQUIP_ERR_EVENT_AUTOEQUIP_BIND_CONFIRM and EQUIP_ERR_WRONG_BAG_TYPE_2

            switch (BagResult)
            {
                case InventoryResult.CantEquipLevel:
                case InventoryResult.PurchaseLevelTooLow:
                    _worldPacket.WriteInt32(Level);
                    break;
                case InventoryResult.EventAutoEquipBindConfirm:
                    _worldPacket.WritePackedGuid128(SrcContainer);
                    _worldPacket.WriteInt32(SrcSlot);
                    _worldPacket.WritePackedGuid128(DstContainer);
                    break;
                case InventoryResult.ItemMaxLimitCategoryCountExceeded:
                case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
                case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
                    _worldPacket.WriteInt32(LimitCategory);
                    break;
            }
        }

        public InventoryResult BagResult;
        public byte ContainerBSlot;
        public WowGuid128 SrcContainer;
        public WowGuid128 DstContainer;
        public int SrcSlot;
        public int LimitCategory;
        public int Level;
        public WowGuid128[] Item = new WowGuid128[2];
    }

    public class RepairItem : ClientPacket
    {
        public RepairItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VendorGUID = _worldPacket.ReadPackedGuid128();
            ItemGUID = _worldPacket.ReadPackedGuid128();
            UseGuildBank = _worldPacket.HasBit();
        }

        public WowGuid128 VendorGUID;
        public WowGuid128 ItemGUID;
        public bool UseGuildBank;
    }

    class SocketGems : ClientPacket
    {
        public SocketGems(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            ItemGuid = _worldPacket.ReadPackedGuid128();
            for (int i = 0; i < ItemConst.MaxGemSockets; ++i)
                Gems[i] = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 ItemGuid;
        public WowGuid128[] Gems = new WowGuid128[ItemConst.MaxGemSockets];
    }

    class SocketGemsSuccess : ServerPacket
    {
        public SocketGemsSuccess() : base(Opcode.SMSG_SOCKET_GEMS_SUCCESS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ItemGuid);
        }

        public WowGuid128 ItemGuid;
    }

    class DurabilityDamageDeath : ServerPacket
    {
        public DurabilityDamageDeath() : base(Opcode.SMSG_DURABILITY_DAMAGE_DEATH) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Percent);
        }

        public uint Percent;
    }

    class ItemCooldown : ServerPacket
    {
        public ItemCooldown() : base(Opcode.SMSG_ITEM_COOLDOWN) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ItemGuid);
            _worldPacket.WriteUInt32(SpellID);
            _worldPacket.WriteUInt32(Cooldown);
        }

        public WowGuid128 ItemGuid;
        public uint SpellID;
        public uint Cooldown;
    }

    public class OpenItem : ClientPacket
    {
        public OpenItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PackSlot = _worldPacket.ReadUInt8();
            Slot = _worldPacket.ReadUInt8();
        }

        public byte PackSlot;
        public byte Slot;
    }

    public class SetAmmo : ClientPacket
    {
        public SetAmmo(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            ItemId = _worldPacket.ReadUInt32();
        }

        public uint ItemId;
    }

    class ItemEnchantTimeUpdate : ServerPacket
    {
        public ItemEnchantTimeUpdate() : base(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ItemGuid);
            _worldPacket.WriteUInt32(DurationLeft);
            _worldPacket.WriteUInt32(Slot);
            _worldPacket.WritePackedGuid128(OwnerGuid);
        }

        public WowGuid128 ItemGuid;
        public uint DurationLeft;
        public uint Slot;
        public WowGuid128 OwnerGuid;
    }

    class EnchantmentLog : ServerPacket
    {
        public EnchantmentLog() : base(Opcode.SMSG_ENCHANTMENT_LOG) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(Caster);
            _worldPacket.WritePackedGuid128(ItemGUID);
            _worldPacket.WriteInt32(ItemID);
            _worldPacket.WriteInt32(Enchantment);
            _worldPacket.WriteInt32(EnchantSlot);
        }
        public WowGuid128 Owner;
        public WowGuid128 Caster;
        public WowGuid128 ItemGUID;
        public int ItemID;
        public int Enchantment;
        public int EnchantSlot = 1;
    }

    public class CancelTempEnchantment : ClientPacket
    {
        public CancelTempEnchantment(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            EnchantmentSlot = _worldPacket.ReadUInt32();
        }

        public uint EnchantmentSlot;
    }

    public class WrapItem : ClientPacket
    {
        public WrapItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            _worldPacket.ReadUInt8(); // Unknown Value. Usually 128
            GiftBag = _worldPacket.ReadUInt8();
            GiftSlot = _worldPacket.ReadUInt8();
            ItemBag = _worldPacket.ReadUInt8();
            ItemSlot = _worldPacket.ReadUInt8();
        }
        public byte GiftBag;
        public byte GiftSlot;
        public byte ItemBag;
        public byte ItemSlot;
    }
}
