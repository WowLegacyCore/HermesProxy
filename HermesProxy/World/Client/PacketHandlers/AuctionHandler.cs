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
            uint auctionId = packet.ReadUInt32();
            uint bidAmount = packet.ReadUInt32();
            uint minIncrement = packet.ReadUInt32();
            WowGuid buyer = packet.ReadGuid();
            uint itemId = packet.ReadUInt32();
            uint randomPropertyId = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadFloat(); // Time Left

            string name = GameData.GetItemName(itemId);
            if (String.IsNullOrEmpty(name))
            {
                WorldPacket query = new WorldPacket(Opcode.CMSG_ITEM_NAME_QUERY);
                query.WriteUInt32(itemId);
                query.WriteGuid(WowGuid64.Empty);
                SendPacket(query);
                return;
            }

            if (buyer.IsEmpty())
            {
                string message;
                if (bidAmount == 0)
                    message = $"Your auction of {name} has expired.";
                else
                    message = $"Your auction of {name} sold.";

                ChatPkt chat = new ChatPkt(GetSession(), ChatMessageTypeModern.System, message);
                SendPacketToClient(chat);
            }
        }

        [PacketHandler(Opcode.SMSG_AUCTION_BIDDER_NOTIFICATION)]
        void HandleAuctionBidderNotification(WorldPacket packet)
        {
            uint auctionHouseId = packet.ReadUInt32();
            uint auctionId = packet.ReadUInt32();
            WowGuid buyer = packet.ReadGuid();
            uint bidAmount = packet.ReadUInt32();
            uint minIncrement = packet.ReadUInt32();
            uint itemId = packet.ReadUInt32();
            uint randomPropertyId = packet.ReadUInt32();

            string name = GameData.GetItemName(itemId);
            if (String.IsNullOrEmpty(name))
            {
                WorldPacket query = new WorldPacket(Opcode.CMSG_ITEM_NAME_QUERY);
                query.WriteUInt32(itemId);
                query.WriteGuid(WowGuid64.Empty);
                SendPacket(query);
                return;
            }

            string message;
            if (bidAmount == 0)
                message = $"You won an auction for {name}.";
            else
                message = $"You have been outbid on {name}.";

            ChatPkt chat = new ChatPkt(GetSession(), ChatMessageTypeModern.System, message);
            SendPacketToClient(chat);
        }
    }
}
