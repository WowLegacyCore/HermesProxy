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
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class LootUnit : ClientPacket
    {
        public LootUnit(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Unit = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Unit;
    }

    public class LootResponse : ServerPacket
    {
        public LootResponse() : base(Opcode.SMSG_LOOT_RESPONSE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt8((byte)FailureReason);
            _worldPacket.WriteUInt8((byte)AcquireReason);
            _worldPacket.WriteUInt8((byte)LootMethod);
            _worldPacket.WriteUInt8(Threshold);
            _worldPacket.WriteUInt32(Coins);
            _worldPacket.WriteInt32(Items.Count);
            _worldPacket.WriteInt32(Currencies.Count);
            _worldPacket.WriteBit(Acquired);
            _worldPacket.WriteBit(AELooting);
            _worldPacket.FlushBits();

            foreach (LootItemData item in Items)
                item.Write(_worldPacket);

            foreach (LootCurrency currency in Currencies)
            {
                _worldPacket.WriteUInt32(currency.CurrencyID);
                _worldPacket.WriteUInt32(currency.Quantity);
                _worldPacket.WriteUInt8(currency.LootListID);
                _worldPacket.WriteBits(currency.UIType, 3);
                _worldPacket.FlushBits();
            }
        }

        public WowGuid128 Owner;
        public WowGuid128 LootObj;
        public LootError FailureReason = LootError.NoLoot; // Most common value
        public LootType AcquireReason;
        public LootMethod LootMethod;
        public byte Threshold = 2; // Most common value, 2 = Uncommon
        public uint Coins;
        public List<LootItemData> Items = new();
        public List<LootCurrency> Currencies = new();
        public bool Acquired = true;
        public bool AELooting;
    }

    public class LootItemData
    {
        public void Write(WorldPacket data)
        {
            data.WriteBits(Type, 2);
            data.WriteBits(UIType, 3);
            data.WriteBit(CanTradeToTapList);
            data.FlushBits();
            Loot.Write(data); // WorldPackets::Item::ItemInstance
            data.WriteUInt32(Quantity);
            data.WriteUInt8(LootItemType);
            data.WriteUInt8(LootListID);
        }

        public byte Type;
        public LootSlotTypeModern UIType;
        public uint Quantity;
        public byte LootItemType;
        public byte LootListID;
        public bool CanTradeToTapList;
        public ItemInstance Loot = new();
    }

    public struct LootCurrency
    {
        public uint CurrencyID;
        public uint Quantity;
        public byte LootListID;
        public byte UIType;
    }

    class LootRelease : ClientPacket
    {
        public LootRelease(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Owner = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Owner;
    }

    class LootReleaseResponse : ServerPacket
    {
        public LootReleaseResponse() : base(Opcode.SMSG_LOOT_RELEASE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WritePackedGuid128(Owner);
        }

        public WowGuid128 LootObj;
        public WowGuid128 Owner;
    }

    class LootMoney : ClientPacket
    {
        public LootMoney(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class LootMoneyNotify : ServerPacket
    {
        public LootMoneyNotify() : base(Opcode.SMSG_LOOT_MONEY_NOTIFY) { }

        public override void Write()
        {
            _worldPacket.WriteUInt64(Money);
            _worldPacket.WriteUInt64(MoneyMod);
            _worldPacket.WriteBit(SoleLooter);
            _worldPacket.FlushBits();
        }

        public ulong Money;
        public ulong MoneyMod;
        public bool SoleLooter;
    }

    class CoinRemoved : ServerPacket
    {
        public CoinRemoved() : base(Opcode.SMSG_COIN_REMOVED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
        }

        public WowGuid128 LootObj;
    }

    class LootItemPkt : ClientPacket
    {
        public LootItemPkt(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint Count = _worldPacket.ReadUInt32();

            for (uint i = 0; i < Count; ++i)
            {
                var loot = new LootRequest()
                {
                    LootObj = _worldPacket.ReadPackedGuid128(),
                    LootListID = _worldPacket.ReadUInt8()
                };

                Loot.Add(loot);
            }
        }

        public List<LootRequest> Loot = new();
    }
    public struct LootRequest
    {
        public WowGuid128 LootObj;
        public byte LootListID;
    }

    class LootRemoved : ServerPacket
    {
        public LootRemoved() : base(Opcode.SMSG_LOOT_REMOVED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt8(LootListID);
        }

        public WowGuid128 Owner;
        public WowGuid128 LootObj;
        public byte LootListID;
    }

    class SetLootMethod : ClientPacket
    {
        public SetLootMethod(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
            LootMethod = (LootMethod)_worldPacket.ReadUInt8();
            LootMasterGUID = _worldPacket.ReadPackedGuid128();
            LootThreshold = _worldPacket.ReadUInt32();
        }

        public sbyte PartyIndex;
        public LootMethod LootMethod;
        public WowGuid128 LootMasterGUID;
        public uint LootThreshold;
    }

    class OptOutOfLoot : ClientPacket
    {
        public OptOutOfLoot(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PassOnLoot = _worldPacket.HasBit();
        }

        public bool PassOnLoot;
    }

    class StartLootRoll : ServerPacket
    {
        public StartLootRoll() : base(Opcode.SMSG_LOOT_START_ROLL) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteUInt32(RollTime);
            _worldPacket.WriteUInt8((byte)ValidRolls);
            _worldPacket.WriteUInt8((byte)Method);
            Item.Write(_worldPacket);
        }

        public WowGuid128 LootObj;
        public uint MapID;
        public uint RollTime;
        public LootMethod Method = LootMethod.GroupLoot;
        public RollMask ValidRolls;
        public LootItemData Item = new();
    }

    class LootRoll : ClientPacket
    {
        public LootRoll(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            LootObj = _worldPacket.ReadPackedGuid128();
            LootListID = _worldPacket.ReadUInt8();
            RollType = (RollType)_worldPacket.ReadUInt8();
        }

        public WowGuid128 LootObj;
        public byte LootListID;
        public RollType RollType;
    }

    class LootRollBroadcast : ServerPacket
    {
        public LootRollBroadcast() : base(Opcode.SMSG_LOOT_ROLL) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WritePackedGuid128(Player);
            _worldPacket.WriteInt32(Roll);
            _worldPacket.WriteUInt8((byte)RollType);
            Item.Write(_worldPacket);
            _worldPacket.WriteBit(Autopassed);
            _worldPacket.FlushBits();
        }

        public WowGuid128 LootObj;
        public WowGuid128 Player;
        public int Roll;
        public RollType RollType;
        public LootItemData Item = new();
        public bool Autopassed = false;    // Triggers message |HlootHistory:%d|h[Loot]|h: You automatically passed on: %s because you cannot loot that item.
    }

    class LootRollWon : ServerPacket
    {
        public LootRollWon() : base(Opcode.SMSG_LOOT_ROLL_WON) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WritePackedGuid128(Winner);
            _worldPacket.WriteInt32(Roll);
            _worldPacket.WriteUInt8((byte)RollType);
            Item.Write(_worldPacket);
            _worldPacket.WriteUInt8(MainSpec);
        }

        public WowGuid128 LootObj;
        public WowGuid128 Winner;
        public int Roll;
        public RollType RollType;
        public LootItemData Item = new();
        public byte MainSpec;
    }

    class LootAllPassed : ServerPacket
    {
        public LootAllPassed() : base(Opcode.SMSG_LOOT_ALL_PASSED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            Item.Write(_worldPacket);
        }

        public WowGuid128 LootObj;
        public LootItemData Item = new();
    }

    class LootRollsComplete : ServerPacket
    {
        public LootRollsComplete() : base(Opcode.SMSG_LOOT_ROLLS_COMPLETE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteUInt8(LootListID);
        }

        public WowGuid128 LootObj;
        public byte LootListID;
    }

    class LootMasterGive : ClientPacket
    {
        public LootMasterGive(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint Count = _worldPacket.ReadUInt32();
            TargetGUID = _worldPacket.ReadPackedGuid128();

            for (int i = 0; i < Count; ++i)
            {
                LootRequest lootRequest = new()
                {
                    LootObj = _worldPacket.ReadPackedGuid128(),
                    LootListID = _worldPacket.ReadUInt8()
                };
                Loot.Add(lootRequest);
            }
        }

        public WowGuid128 TargetGUID;
        public List<LootRequest> Loot = new();
    }

    class MasterLootCandidateList : ServerPacket
    {
        public MasterLootCandidateList() : base(Opcode.SMSG_LOOT_MASTER_LIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(LootObj);
            _worldPacket.WriteInt32(Players.Count);
            foreach (var guid in Players)
                _worldPacket.WritePackedGuid128(guid);
        }

        public WowGuid128 LootObj;
        public List<WowGuid128> Players = new();
    }

    class LootList : ServerPacket
    {
        public LootList() : base(Opcode.SMSG_LOOT_LIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(LootObj);

            _worldPacket.WriteBit(Master != null);
            _worldPacket.WriteBit(RoundRobinWinner != null);
            _worldPacket.FlushBits();

            if (Master != null)
                _worldPacket.WritePackedGuid128(Master);

            if (RoundRobinWinner != null)
                _worldPacket.WritePackedGuid128(RoundRobinWinner);
        }

        public WowGuid128 Owner;
        public WowGuid128 LootObj;
        public WowGuid128 Master;
        public WowGuid128 RoundRobinWinner;
    }
}
