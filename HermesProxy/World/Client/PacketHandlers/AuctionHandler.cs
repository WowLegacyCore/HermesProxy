using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.MSG_AUCTION_HELLO)]
        void HandleAuctionHello(WorldPacket packet)
        {
            AuctionHelloResponse auction = new AuctionHelloResponse();
            auction.Guid = packet.ReadGuid().To128(GetSession().GameState);
            GetSession().GameState.CurrentInteractedWithNPC = auction.Guid;
            auction.AuctionHouseID = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                auction.OpenForBusiness = packet.ReadBool();
            SendPacketToClient(auction);

            // Have to send this again here, or server does not reply for some reason.
            WorldPacket packet2 = new WorldPacket(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS);
            packet2.WriteGuid(auction.Guid.To64());
            packet2.WriteUInt32(0);
            SendPacketToServer(packet2);
        }

        AuctionItem ReadAuctionItem(WorldPacket packet)
        {
            AuctionItem item = new AuctionItem();
            item.AuctionID = packet.ReadUInt32();
            item.Item = new();
            item.Item.ItemID = packet.ReadUInt32();

            byte enchantmentCount;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                enchantmentCount = 7;
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                enchantmentCount = 6;
            else
                enchantmentCount = 1;

            for (byte j = 0; j < enchantmentCount; ++j)
            {
                ItemEnchantData enchant = new ItemEnchantData();
                enchant.Slot = j;
                enchant.ID = packet.ReadUInt32();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    enchant.Expiration = packet.ReadUInt32();
                    enchant.Charges = packet.ReadInt32();
                }
                if (enchant.ID != 0)
                    item.Enchantments.Add(enchant);
            }

            item.Item.RandomPropertiesID = packet.ReadUInt32();
            item.Item.RandomPropertiesSeed = packet.ReadUInt32();
            item.Count = packet.ReadInt32();
            item.Charges = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
               item.Flags = packet.ReadUInt32();

            item.Owner = packet.ReadGuid().To128(GetSession().GameState);
            item.OwnerAccountID = GetSession().GetGameAccountGuidForPlayer(item.Owner);
            item.MinBid = packet.ReadUInt32();
            item.MinIncrement = packet.ReadUInt32();
            item.BuyoutPrice = packet.ReadUInt32();
            item.DurationLeft = packet.ReadInt32();
            item.Bidder = packet.ReadGuid().To128(GetSession().GameState);
            item.BidAmount = packet.ReadUInt32();

            if (item.Item.ItemID == 0)
                item.Item = null;

            return item;
        }

        [PacketHandler(Opcode.SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT)]
        [PacketHandler(Opcode.SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT)]
        void HandleAuctionListMyItemsResult(WorldPacket packet)
        {
            AuctionListMyItemsResult auction = new AuctionListMyItemsResult(packet.GetUniversalOpcode(false));
            uint count = packet.ReadUInt32();
            for (uint i = 0; i < count; i++)
            {
                AuctionItem item = ReadAuctionItem(packet);
                auction.Items.Add(item);
            }
            auction.TotalItemsCount = packet.ReadInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
                auction.DesiredDelay = packet.ReadUInt32();
            SendPacketToClient(auction);
        }

        [PacketHandler(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT)]
        void HandleAuctionListItemsResult(WorldPacket packet)
        {
            AuctionListItemsResult auction = new AuctionListItemsResult();
            uint count = packet.ReadUInt32();
            for (uint i = 0; i < count; i++)
            {
                AuctionItem item = ReadAuctionItem(packet);
                item.CensorServerSideInfo = true;
                auction.Items.Add(item);
            }
            auction.TotalItemsCount = packet.ReadInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
                auction.DesiredDelay = packet.ReadUInt32();
            SendPacketToClient(auction);
        }

        [PacketHandler(Opcode.SMSG_AUCTION_COMMAND_RESULT)]
        void HandleAuctionCommandResult(WorldPacket packet)
        {
            AuctionCommandResult auction = new AuctionCommandResult();
            auction.AuctionID = packet.ReadUInt32();
            auction.Command = (AuctionHouseAction)packet.ReadUInt32();
            auction.ErrorCode = (AuctionHouseError)packet.ReadUInt32();
            switch (auction.ErrorCode)
            {
                case AuctionHouseError.Ok:
                    if (auction.Command == AuctionHouseAction.Bid)
                       auction.MinIncrement = packet.ReadUInt32();
                    break;
                case AuctionHouseError.Inventory:
                    auction.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
                    break;
                case AuctionHouseError.HigherBid:
                    auction.Guid = packet.ReadGuid().To128(GetSession().GameState);
                    auction.Money = packet.ReadUInt32();
                    auction.MinIncrement = packet.ReadUInt32();
                    break;
            }
            SendPacketToClient(auction);
        }

        [PacketHandler(Opcode.SMSG_AUCTION_OWNER_NOTIFICATION)]
        void HandleAuctionOwnerNotification(WorldPacket packet)
        {
            AuctionOwnerNotification info = new AuctionOwnerNotification();
            info.AuctionID = packet.ReadUInt32();
            info.BidAmount = packet.ReadUInt32();
            uint minIncrement = packet.ReadUInt32();
            WowGuid buyer = packet.ReadGuid();
            info.Item.ItemID = packet.ReadUInt32();
            info.Item.RandomPropertiesID = packet.ReadUInt32();

            float mailDelay;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                mailDelay = packet.ReadFloat();
            else
                mailDelay = 3600;

            if (buyer.IsEmpty())
            {
                // BidAmount != 0 -> Your auction of X sold.
                // BidAmount == 0 -> Your auction of X has expired.
                AuctionClosedNotification auction = new AuctionClosedNotification();
                auction.Info = info;
                auction.Sold = info.BidAmount != 0;
                auction.ProceedsMailDelay = mailDelay;
                SendPacketToClient(auction);
            }
            else
            {
                // A buyer has been found for your auction of X.
                AuctionOwnerBidNotification auction = new AuctionOwnerBidNotification();
                auction.Info = info;
                auction.MinIncrement = minIncrement;
                auction.Bidder = buyer.To128(GetSession().GameState);
                SendPacketToClient(auction);
            }
        }

        [PacketHandler(Opcode.SMSG_AUCTION_BIDDER_NOTIFICATION)]
        void HandleAuctionBidderNotification(WorldPacket packet)
        {
            AuctionBidderNotification info = new AuctionBidderNotification();
            uint auctionHouseId = packet.ReadUInt32();
            info.AuctionID = packet.ReadUInt32();
            info.Bidder = packet.ReadGuid().To128(GetSession().GameState);
            uint bidAmount = packet.ReadUInt32();
            uint minIncrement = packet.ReadUInt32();
            info.Item.ItemID = packet.ReadUInt32();
            info.Item.RandomPropertiesID = packet.ReadUInt32();

            if (bidAmount == 0)
            {
                // You won an auction for X.
                AuctionWonNotification auction = new AuctionWonNotification();
                auction.Info = info;
                SendPacketToClient(auction);
            }
            else
            {
                // You have been outbid on X.
                AuctionOutbidNotification auction = new AuctionOutbidNotification();
                auction.Info = info;
                auction.BidAmount = bidAmount;
                auction.MinIncrement = minIncrement;
                SendPacketToClient(auction);
            }
        }
    }
}
