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

namespace HermesProxy.World.Server.Packets
{
    class AuctionHelloResponse : ServerPacket
    {
        public AuctionHelloResponse() : base(Opcode.SMSG_AUCTION_HELLO_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteUInt32(AuctionHouseID);
            _worldPacket.WriteBit(OpenForBusiness);
            _worldPacket.FlushBits();
        }
        public WowGuid128 Guid;
        public uint AuctionHouseID;
        public bool OpenForBusiness = true;
    }

    class AuctionListBidderItems : ClientPacket
    {
        public AuctionListBidderItems(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Auctioneer = _worldPacket.ReadPackedGuid128();
            Offset = _worldPacket.ReadUInt32();

            uint auctionIDCount = _worldPacket.ReadBits<uint>(7);
            _worldPacket.ResetBitPos();

            for (var i = 0; i < auctionIDCount; ++i)
                AuctionItemIDs[i] = _worldPacket.ReadUInt32();
        }

        public WowGuid128 Auctioneer;
        public uint Offset;
        public List<uint> AuctionItemIDs = new();
    }

    class AuctionListOwnerItems : ClientPacket
    {
        public AuctionListOwnerItems(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Auctioneer = _worldPacket.ReadPackedGuid128();
            Offset = _worldPacket.ReadUInt32();
        }

        public WowGuid128 Auctioneer;
        public uint Offset;
    }

    class AuctionListItems: ClientPacket
    {
        public AuctionListItems(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Offset = _worldPacket.ReadUInt32();
            Auctioneer = _worldPacket.ReadPackedGuid128();

            MinLevel = _worldPacket.ReadUInt8();
            MaxLevel = _worldPacket.ReadUInt8();
            Quality = _worldPacket.ReadInt32();
            var sortCount = _worldPacket.ReadUInt8();
            var knownPetsCount = _worldPacket.ReadUInt32();
            MaxPetLevel = _worldPacket.ReadUInt8();

            for (int i = 0; i < knownPetsCount; ++i)
                KnownPets.Add(_worldPacket.ReadUInt8());

            uint nameLength = _worldPacket.ReadBits<uint>(8);
            Name = _worldPacket.ReadString(nameLength);

            uint classFiltersCount = _worldPacket.ReadBits<uint>(3);

            OnlyUsable = _worldPacket.HasBit();
            ExactMatch = _worldPacket.HasBit();
            _worldPacket.ResetBitPos();

            for (int i = 0; i < classFiltersCount; ++i)
            {
                ClassFilter classFilter = new ClassFilter();
                classFilter.ItemClass = _worldPacket.ReadInt32();

                uint subClassFiltersCount = _worldPacket.ReadBits<uint>(5);
                for (uint j = 0; j < subClassFiltersCount; ++j)
                {
                    SubClassFilter filter = new SubClassFilter();
                    filter.ItemSubclass = _worldPacket.ReadInt32();
                    filter.InvTypeMask = _worldPacket.ReadUInt32();
                    classFilter.SubClassFilters.Add(filter);
                }

                ClassFilters.Add(classFilter);
            }

            var size = _worldPacket.ReadUInt32();
            var data = _worldPacket.ReadBytes(size);
            var sorts = new WorldPacket(_worldPacket.GetOpcode(), data);
            for (var i = 0; i < sortCount; ++i)
            {
                AuctionSort sort = new AuctionSort();
                sort.Type = sorts.ReadUInt8();
                sort.Direction = sorts.ReadUInt8();
                Sorts.Add(sort);
            }
        }

        public uint Offset;
        public WowGuid128 Auctioneer;
        public byte MinLevel;
        public byte MaxLevel;
        public int Quality;
        public byte MaxPetLevel;
        public List<byte> KnownPets = new();
        public string Name;
        public bool OnlyUsable;
        public bool ExactMatch;
        public List<ClassFilter> ClassFilters = new List<ClassFilter>();
        public List<AuctionSort> Sorts = new();
    }

    public struct AuctionSort
    {
        public byte Type;
        public byte Direction;
    }

    public class ClassFilter
    {
        public int ItemClass;
        public List<SubClassFilter> SubClassFilters = new();
    }
    public struct SubClassFilter
    {
        public int ItemSubclass;
        public uint InvTypeMask;
    }

    public class AuctionListMyItemsResult : ServerPacket
    {
        public AuctionListMyItemsResult(Opcode opcode) : base(opcode) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Items.Count);
            _worldPacket.WriteInt32(TotalItemsCount);
            _worldPacket.WriteUInt32(DesiredDelay);

            foreach (AuctionItem item in Items)
                item.Write(_worldPacket);
        }

        public List<AuctionItem> Items = new();
        public int TotalItemsCount;
        public uint DesiredDelay = 300;
    }

    public class AuctionListItemsResult : ServerPacket
    {
        public AuctionListItemsResult() : base(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Items.Count);
            _worldPacket.WriteInt32(TotalItemsCount);
            _worldPacket.WriteUInt32(DesiredDelay);

            if (Items.Count > 0)
                _worldPacket.WriteBool(OnlyUsable);

            foreach (AuctionItem item in Items)
                item.Write(_worldPacket);
        }

        public List<AuctionItem> Items = new();
        public int TotalItemsCount;
        public uint DesiredDelay = 300;
        public bool OnlyUsable;
    }

    public class AuctionItem
    {
        public void Write(WorldPacket data)
        {
            data.WriteBit(Item != null);
            data.WriteBits(Enchantments.Count, 4);
            data.WriteBits(Gems.Count, 2);
            data.WriteBit(MinBid.HasValue);
            data.WriteBit(MinIncrement.HasValue);
            data.WriteBit(BuyoutPrice.HasValue);
            data.WriteBit(UnitPrice.HasValue);
            data.WriteBit(CensorServerSideInfo);
            data.WriteBit(CensorBidInfo);
            data.WriteBit(AuctionBucketKey != null);
            data.WriteBit(Creator != null);
            if (!CensorBidInfo)
            {
                data.WriteBit(Bidder != null);
                data.WriteBit(BidAmount.HasValue);
            }

            data.FlushBits();

            if (Item != null)
                Item.Write(data);

            data.WriteInt32(Count);
            data.WriteInt32(Charges);
            data.WriteUInt32(Flags);
            data.WriteUInt32(AuctionID);
            data.WritePackedGuid128(Owner);
            data.WriteInt32(DurationLeft);
            data.WriteUInt8(DeleteReason);

            foreach (ItemEnchantData enchant in Enchantments)
                enchant.Write(data);

            if (MinBid.HasValue)
                data.WriteUInt64(MinBid.Value);

            if (MinIncrement.HasValue)
                data.WriteUInt64(MinIncrement.Value);

            if (BuyoutPrice.HasValue)
                data.WriteUInt64(BuyoutPrice.Value);

            if (UnitPrice.HasValue)
                data.WriteUInt64(UnitPrice.Value);

            if (!CensorServerSideInfo)
            {
                data.WritePackedGuid128(ItemGuid);
                data.WritePackedGuid128(OwnerAccountID);
                data.WriteUInt32(EndTime);
            }

            if (Creator != null)
                data.WritePackedGuid128(Creator);

            if (!CensorBidInfo)
            {
                if (Bidder != null)
                    data.WritePackedGuid128(Bidder);

                if (BidAmount.HasValue)
                    data.WriteUInt64(BidAmount.Value);
            }

            foreach (ItemGemData gem in Gems)
                gem.Write(data);

            if (AuctionBucketKey != null)
                AuctionBucketKey.Write(data);
        }

        public ItemInstance Item;
        public int Count;
        public int Charges;
        public List<ItemEnchantData> Enchantments = new();
        public uint Flags = 196608;
        public uint AuctionID;
        public WowGuid128 Owner;
        public ulong? MinBid;
        public ulong? MinIncrement;
        public ulong? BuyoutPrice;
        public ulong? UnitPrice;
        public int DurationLeft;
        public byte DeleteReason;
        public bool CensorServerSideInfo;
        public bool CensorBidInfo;
        public WowGuid128 ItemGuid = WowGuid128.Empty;
        public WowGuid128 OwnerAccountID;
        public uint EndTime;
        public WowGuid128 Creator;
        public WowGuid128 Bidder;
        public ulong? BidAmount;
        public List<ItemGemData> Gems = new();
        public AuctionBucketKey AuctionBucketKey;
    }

    public class AuctionBucketKey
    {
        public AuctionBucketKey() { }

        public AuctionBucketKey(WorldPacket data)
        {
            data.ResetBitPos();
            ItemID = data.ReadBits<uint>(20);

            if (data.HasBit())
                BattlePetSpeciesID = new();

            ItemLevel = data.ReadBits<ushort>(11);

            if (data.HasBit())
                SuffixItemNameDescriptionID = new();

            if (BattlePetSpeciesID.HasValue)
                BattlePetSpeciesID = data.ReadUInt16();

            if (SuffixItemNameDescriptionID.HasValue)
                SuffixItemNameDescriptionID = data.ReadUInt16();
        }

        public void Write(WorldPacket data)
        {
            data.WriteBits(ItemID, 20);
            data.WriteBit(BattlePetSpeciesID.HasValue);
            data.WriteBits(ItemLevel, 11);
            data.WriteBit(SuffixItemNameDescriptionID.HasValue);
            data.FlushBits();

            if (BattlePetSpeciesID.HasValue)
                data.WriteUInt16(BattlePetSpeciesID.Value);

            if (SuffixItemNameDescriptionID.HasValue)
                data.WriteUInt16(SuffixItemNameDescriptionID.Value);
        }

        public uint ItemID;
        public ushort ItemLevel;
        public ushort? BattlePetSpeciesID = new();
        public ushort? SuffixItemNameDescriptionID = new();
    }

    public class ItemEnchantData
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(ID);
            data.WriteUInt32(Expiration);
            data.WriteInt32(Charges);
            data.WriteUInt8(Slot);
        }

        public uint ID;
        public uint Expiration;
        public int Charges;
        public byte Slot;
    }

    class AuctionSellItem : ClientPacket
    {
        public AuctionSellItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Auctioneer = _worldPacket.ReadPackedGuid128();
            MinBid = _worldPacket.ReadUInt64();
            BuyoutPrice = _worldPacket.ReadUInt64();
            ExpireTime = _worldPacket.ReadUInt32();

            uint itemCount = _worldPacket.ReadBits<uint>(6);

            for (var i = 0; i < itemCount; ++i)
                Items.Add(new AuctionItemForSale(_worldPacket));
        }

        public ulong BuyoutPrice;
        public WowGuid128 Auctioneer;
        public ulong MinBid;
        public uint ExpireTime;
        public List<AuctionItemForSale> Items = new();
    }

    public struct AuctionItemForSale
    {
        public AuctionItemForSale(WorldPacket data)
        {
            Guid = data.ReadPackedGuid128();
            UseCount = data.ReadUInt32();
        }

        public WowGuid128 Guid;
        public uint UseCount;
    }

    class AuctionRemoveItem : ClientPacket
    {
        public AuctionRemoveItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Auctioneer = _worldPacket.ReadPackedGuid128();
            AuctionID = _worldPacket.ReadUInt32();
            if (_worldPacket.HasBit())
                TaintedBy = new();

            if (TaintedBy != null)
                TaintedBy.Read(_worldPacket);
        }

        public WowGuid128 Auctioneer;
        public uint AuctionID;
        public AddOnInfo TaintedBy;
    }

    public class AddOnInfo
    {
        public void Read(WorldPacket data)
        {
            data.ResetBitPos();

            uint nameLength = data.ReadBits<uint>(10);
            uint versionLength = data.ReadBits<uint>(10);
            Loaded = data.HasBit();
            Disabled = data.HasBit();
            if (nameLength > 1)
            {
                Name = data.ReadString(nameLength - 1);
                data.ReadUInt8(); // null terminator
            }
            if (versionLength > 1)
            {
                Version = data.ReadString(versionLength - 1);
                data.ReadUInt8(); // null terminator
            }
        }

        public string Name;
        public string Version;
        public bool Loaded;
        public bool Disabled;
    }

    class AuctionPlaceBid : ClientPacket
    {
        public WowGuid128 Auctioneer;
        public ulong BidAmount;
        public uint AuctionID;
        public AddOnInfo TaintedBy;

        public AuctionPlaceBid(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Auctioneer = _worldPacket.ReadPackedGuid128();
            AuctionID = _worldPacket.ReadUInt32();
            BidAmount = _worldPacket.ReadUInt64();
            if (_worldPacket.HasBit())
                TaintedBy = new();

            if (TaintedBy != null)
                TaintedBy.Read(_worldPacket);
        }
    }

    class AuctionCommandResult : ServerPacket
    {
        public AuctionCommandResult() : base(Opcode.SMSG_AUCTION_COMMAND_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(AuctionID);
            _worldPacket.WriteInt32((int)Command);
            _worldPacket.WriteInt32((int)ErrorCode);
            _worldPacket.WriteInt32((int)BagResult);
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteUInt64(MinIncrement);
            _worldPacket.WriteUInt64(Money);
            _worldPacket.WriteUInt32(DesiredDelay);
        }

        public uint AuctionID;                              //< the id of the auction that triggered this notification
        public AuctionHouseAction Command;                  //< the type of action that triggered this notification. Possible values are @ref AuctionAction
        public AuctionHouseError ErrorCode;                 //< the error code that was generated when trying to perform the action. Possible values are @ref AuctionError
        public InventoryResult BagResult;                   //< the bid error. Possible values are @ref AuctionError
        public WowGuid128 Guid = WowGuid128.Empty;          //< the GUID of the bidder for this auction.
        public ulong MinIncrement;                          //< the sum of outbid is (1% of current bid) * 5, if the bid is too small, then this value is 1 copper.
        public ulong Money;                                 //< the amount of money that the player bid in copper
        public uint DesiredDelay;
    }
}
