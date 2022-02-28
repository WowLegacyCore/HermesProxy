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
        public LootSlotType UIType;
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
}
