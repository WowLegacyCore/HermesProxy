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
using System;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Server.Packets
{
    public class InteractWithNPC : ClientPacket
    {
        public InteractWithNPC(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            CreatureGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 CreatureGUID;
    }

    public class GossipMessagePkt : ServerPacket
    {
        public GossipMessagePkt() : base(Opcode.SMSG_GOSSIP_MESSAGE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(GossipGUID);
            _worldPacket.WriteInt32(GossipID);
            _worldPacket.WriteInt32(FriendshipFactionID);
            _worldPacket.WriteInt32(TextID);

            _worldPacket.WriteInt32(GossipOptions.Count);
            _worldPacket.WriteInt32(GossipQuests.Count);

            foreach (ClientGossipOption options in GossipOptions)
            {
                _worldPacket.WriteInt32(options.OptionIndex);
                _worldPacket.WriteUInt8(options.OptionIcon);
                _worldPacket.WriteUInt8(options.OptionFlags);
                _worldPacket.WriteInt32(options.OptionCost);

                _worldPacket.WriteBits(options.Text.GetByteCount(), 12);
                _worldPacket.WriteBits(options.Confirm.GetByteCount(), 12);
                _worldPacket.WriteBits((byte)options.Status, 2);
                _worldPacket.WriteBit(options.SpellID.HasValue);
                _worldPacket.FlushBits();

                options.Treasure.Write(_worldPacket);

                _worldPacket.WriteString(options.Text);
                _worldPacket.WriteString(options.Confirm);

                if (options.SpellID.HasValue)
                    _worldPacket.WriteInt32(options.SpellID.Value);
            }

            foreach (ClientGossipQuest text in GossipQuests)
                text.Write(_worldPacket);
        }

        public List<ClientGossipOption> GossipOptions = new();
        public int FriendshipFactionID;
        public WowGuid128 GossipGUID;
        public List<ClientGossipQuest> GossipQuests = new();
        public int TextID;
        public int GossipID;
    }

    public class ClientGossipOption
    {
        public int OptionIndex;
        public byte OptionIcon;
        public byte OptionFlags;
        public int OptionCost;
        public GossipOptionStatus Status;
        public string Text;
        public string Confirm;
        public TreasureLootList Treasure = new();
        public int? SpellID;
    }

    public class TreasureLootList
    {
        public List<TreasureItem> Items = new();

        public void Write(WorldPacket data)
        {
            data.WriteInt32(Items.Count);
            foreach (TreasureItem treasureItem in Items)
                treasureItem.Write(data);
        }
    }

    public struct TreasureItem
    {
        public GossipOptionRewardType Type;
        public int ID;
        public int Quantity;

        public void Write(WorldPacket data)
        {
            data.WriteBits((byte)Type, 1);
            data.WriteInt32(ID);
            data.WriteInt32(Quantity);
        }
    }

    public class ClientGossipQuest
    {
        public uint QuestID;
        public uint ContentTuningID;
        public int QuestType;
        public int QuestLevel;
        public int QuestMaxLevel = 255;
        public bool Repeatable;
        public string QuestTitle;
        public uint QuestFlags = 8;
        public uint QuestFlagsEx;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(QuestID);
            data.WriteUInt32(ContentTuningID);
            data.WriteInt32(QuestType);
            data.WriteInt32(QuestLevel);
            data.WriteInt32(QuestMaxLevel);
            data.WriteUInt32(QuestFlags);
            data.WriteUInt32(QuestFlagsEx);

            data.WriteBit(Repeatable);
            data.WriteBits(QuestTitle.GetByteCount(), 9);
            data.FlushBits();

            data.WriteString(QuestTitle);
        }
    }

    public class GossipSelectOption : ClientPacket
    {
        public GossipSelectOption(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            GossipUnit = _worldPacket.ReadPackedGuid128();
            GossipID = _worldPacket.ReadUInt32();
            GossipIndex = _worldPacket.ReadUInt32();

            uint length = _worldPacket.ReadBits<uint>(8);
            PromotionCode = _worldPacket.ReadString(length);
        }

        public WowGuid128 GossipUnit;
        public uint GossipIndex;
        public uint GossipID;
        public string PromotionCode;
    }

    public class GossipComplete : ServerPacket
    {
        public GossipComplete() : base(Opcode.SMSG_GOSSIP_COMPLETE) { }

        public override void Write() { }
    }

    public class BinderConfirm : ServerPacket
    {
        public BinderConfirm() : base(Opcode.SMSG_BINDER_CONFIRM) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
        }

        public WowGuid128 Guid;
    }

    public class VendorInventory : ServerPacket
    {
        public VendorInventory() : base(Opcode.SMSG_VENDOR_INVENTORY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(VendorGUID);
            _worldPacket.WriteUInt8(Reason);
            _worldPacket.WriteInt32(Items.Count);

            foreach (VendorItem item in Items)
                item.Write(_worldPacket);
        }

        public byte Reason = 0;
        public List<VendorItem> Items = new();
        public WowGuid128 VendorGUID;
    }

    public class VendorItem
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(Slot);
            data.WriteInt32(Type);
            data.WriteInt32(Quantity);
            data.WriteUInt64(Price);
            data.WriteInt32(Durability);
            data.WriteUInt32(StackCount);
            data.WriteInt32(ExtendedCostID);
            data.WriteInt32(PlayerConditionFailed);
            Item.Write(data);
            data.WriteBit(DoNotFilterOnVendor);
            data.WriteBit(Refundable);
            data.FlushBits();
        }

        public int Slot;
        public int Type = 1;
        public ItemInstance Item = new();
        public int Quantity = -1;
        public ulong Price;
        public int Durability;
        public uint StackCount;
        public int ExtendedCostID;
        public int PlayerConditionFailed;
        public bool DoNotFilterOnVendor;
        public bool Refundable;
    }

    public class ShowBank : ServerPacket
    {
        public ShowBank() : base(Opcode.SMSG_SHOW_BANK, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
        }

        public WowGuid128 Guid;
    }

    public class BuyBankSlot : ClientPacket
    {
        public BuyBankSlot(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
    }

    public class TrainerList : ServerPacket
    {
        public TrainerList() : base(Opcode.SMSG_TRAINER_LIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(TrainerGUID);
            _worldPacket.WriteInt32(TrainerType);
            _worldPacket.WriteUInt32(TrainerID);

            _worldPacket.WriteInt32(Spells.Count);
            foreach (TrainerListSpell spell in Spells)
            {
                _worldPacket.WriteUInt32(spell.SpellID);
                _worldPacket.WriteUInt32(spell.MoneyCost);
                _worldPacket.WriteUInt32(spell.ReqSkillLine);
                _worldPacket.WriteUInt32(spell.ReqSkillRank);

                for (uint i = 0; i < 3; ++i)
                    _worldPacket.WriteUInt32(spell.ReqAbility[i]);

                _worldPacket.WriteUInt8((byte)spell.Usable);
                _worldPacket.WriteUInt8(spell.ReqLevel);
            }

            _worldPacket.WriteBits(Greeting.GetByteCount(), 11);
            _worldPacket.FlushBits();
            _worldPacket.WriteString(Greeting);
        }

        public WowGuid128 TrainerGUID;
        public int TrainerType;
        public uint TrainerID = 1;
        public List<TrainerListSpell> Spells = new();
        public string Greeting;
    }

    public class TrainerListSpell
    {
        public uint SpellID;
        public uint MoneyCost;
        public uint ReqSkillLine;
        public uint ReqSkillRank;
        public uint[] ReqAbility = new uint[3];
        public TrainerSpellStateModern Usable;
        public byte ReqLevel;
    }

    class TrainerBuySpell : ClientPacket
    {
        public TrainerBuySpell(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TrainerGUID = _worldPacket.ReadPackedGuid128();
            TrainerID = _worldPacket.ReadUInt32();
            SpellID = _worldPacket.ReadUInt32();
        }

        public WowGuid128 TrainerGUID;
        public uint TrainerID;
        public uint SpellID;
    }

    class TrainerBuyFailed : ServerPacket
    {
        public TrainerBuyFailed() : base(Opcode.SMSG_TRAINER_BUY_FAILED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(TrainerGUID);
            _worldPacket.WriteUInt32(SpellID);
            _worldPacket.WriteUInt32(TrainerFailedReason);
        }

        public WowGuid128 TrainerGUID;
        public uint SpellID;
        public uint TrainerFailedReason;
    }

    class RespecWipeConfirm : ServerPacket
    {
        public RespecWipeConfirm() : base(Opcode.SMSG_RESPEC_WIPE_CONFIRM) { }

        public override void Write()
        {
            _worldPacket.WriteInt8((sbyte)RespecType);
            _worldPacket.WriteUInt32(Cost);
            _worldPacket.WritePackedGuid128(TrainerGUID);
        }

        public SpecResetType RespecType = SpecResetType.Talents;
        public uint Cost;
        public WowGuid128 TrainerGUID;
    }

    class ConfirmRespecWipe : ClientPacket
    {
        public ConfirmRespecWipe(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TrainerGUID = _worldPacket.ReadPackedGuid128();
            RespecType = (SpecResetType)_worldPacket.ReadUInt8();
        }

        public WowGuid128 TrainerGUID;
        public SpecResetType RespecType;
    }
}
