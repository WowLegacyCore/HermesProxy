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
        public bool Pushed = true;
        public DisplayType DisplayText;
        public bool Created;
        public bool IsBonusRoll;
        public bool IsEncounterLoot;

        public enum DisplayType
        {
            Hidden = 0,
            Normal = 1,
            EncounterLoot = 2
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
}
