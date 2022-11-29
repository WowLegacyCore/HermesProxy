using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        [PacketHandler(Opcode.CMSG_AUCTION_HELLO_REQUEST)]
        void HandleAuctionHelloRequest(InteractWithNPC interact)
        {
            WorldPacket packet = new(Opcode.MSG_AUCTION_HELLO);
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS)]
        void HandleAuctionListBidderItems(AuctionListBidderItems auction)
        {
            WorldPacket packet = new(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS);
            packet.WriteGuid(auction.Auctioneer.To64());
            packet.WriteUInt32(auction.Offset);
            packet.WriteInt32(auction.AuctionItemIDs.Count);
            foreach (var itemId in auction.AuctionItemIDs)
                packet.WriteUInt32(itemId);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS)]
        void HandleAuctionListOwnerItems(AuctionListOwnerItems auction)
        {
            WorldPacket packet = new(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS);
            packet.WriteGuid(auction.Auctioneer.To64());
            packet.WriteUInt32(auction.Offset);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUCTION_LIST_ITEMS)]
        void HandleAuctionListItems(AuctionListItems auction)
        {
            WorldPacket packet = new(Opcode.CMSG_AUCTION_LIST_ITEMS);
            packet.WriteGuid(auction.Auctioneer.To64());
            packet.WriteUInt32(auction.Offset);
            packet.WriteCString(auction.Name);
            packet.WriteUInt8(auction.MinLevel);
            packet.WriteUInt8(auction.MaxLevel);

            if (auction.ClassFilters.Count > 0)
            {
                if (auction.ClassFilters[0].SubClassFilters.Count == 1)
                {
                    packet.WriteInt32(ModernToLegacyInventorySlotType(auction.ClassFilters[0].SubClassFilters[0].InvTypeMask));
                    packet.WriteInt32(auction.ClassFilters[0].ItemClass);
                    packet.WriteInt32(auction.ClassFilters[0].SubClassFilters[0].ItemSubclass);
                }
                else
                {
                    packet.WriteInt32(-1); // Inventory slotId (head, chest, one-hand etc...)
                    packet.WriteInt32(auction.ClassFilters[0].ItemClass);
                    packet.WriteInt32(-1); // auctionSubCategory
                }
            }
            else
            {
                packet.WriteInt32(-1); // Inventory slotId (head, chest, one-hand etc...)
                packet.WriteInt32(-1); // auctionMainCategory
                packet.WriteInt32(-1); // auctionSubCategory
            }

            packet.WriteInt32(auction.Quality);
            packet.WriteBool(auction.OnlyUsable);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteBool(auction.ExactMatch);
                packet.WriteUInt8((byte)auction.Sorts.Count);

                foreach (var sort in auction.Sorts)
                {
                    packet.WriteUInt8(sort.Type);
                    packet.WriteUInt8(sort.Direction);
                }
            }

            SendPacketToServer(packet);

            int ModernToLegacyInventorySlotType(uint modernInventoryFlag)
            {
                // Modern client can technically search for multiple inventory types at the same time
                // We just get the first bit and just search for this type

                if (modernInventoryFlag == uint.MaxValue)
                    return -1;

                for (int i = 0; i < 32; i++)
                {
                    if ((modernInventoryFlag & (1 << i)) > 0)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        [PacketHandler(Opcode.CMSG_AUCTION_SELL_ITEM)]
        void HandleAuctionSellItem(AuctionSellItem auction)
        {
            uint expireTime = auction.ExpireTime;

            // auction durations were increased in tbc
            // server ignores packet if you send wrong duration
            if (LegacyVersion.ExpansionVersion <= 1 &&
                ModernVersion.ExpansionVersion > 1)
            {
                switch (expireTime)
                {
                    case 1 * 12 * 60: // 720
                    {
                        expireTime = 1 * 2 * 60; // 120
                        break;
                    }
                    case 2 * 12 * 60: // 1440
                    {
                        expireTime = 4 * 2 * 60; // 480
                        break;
                    }
                    case 4 * 12 * 60: // 2880
                    {
                        expireTime = 12 * 2 * 60; // 1440
                        break;
                    }
                }
            }
            else if (LegacyVersion.ExpansionVersion > 1 &&
                     ModernVersion.ExpansionVersion <= 1)
            {
                switch (expireTime)
                {
                    case 1 * 2 * 60:
                    {
                        expireTime = 1 * 12 * 60;
                        break;
                    }
                    case 4 * 2 * 60:
                    {
                        expireTime = 2 * 12 * 60;
                        break;
                    }
                    case 12 * 2 * 60:
                    {
                        expireTime = 4 * 12 * 60;
                        break;
                    }
                }
            }

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_2_2a_10505))
            {
                foreach (var item in auction.Items)
                {
                    WorldPacket packet = new(Opcode.CMSG_AUCTION_SELL_ITEM);
                    packet.WriteGuid(auction.Auctioneer.To64());
                    packet.WriteGuid(item.Guid.To64());
                    packet.WriteUInt32((uint)auction.MinBid);
                    packet.WriteUInt32((uint)auction.BuyoutPrice);
                    packet.WriteUInt32(expireTime);
                    SendPacketToServer(packet);
                }
            }
            else
            {
                WorldPacket packet = new(Opcode.CMSG_AUCTION_SELL_ITEM);
                packet.WriteGuid(auction.Auctioneer.To64());
                packet.WriteInt32(auction.Items.Count);
                foreach (var item in auction.Items)
                {
                    packet.WriteGuid(item.Guid.To64());
                    packet.WriteUInt32(item.UseCount);
                }
                packet.WriteUInt32((uint)auction.MinBid);
                packet.WriteUInt32((uint)auction.BuyoutPrice);
                packet.WriteUInt32(expireTime);
                SendPacketToServer(packet);
            }
        }

        [PacketHandler(Opcode.CMSG_AUCTION_REMOVE_ITEM)]
        void HandleAuctionRemoveItem(AuctionRemoveItem auction)
        {
            WorldPacket packet = new(Opcode.CMSG_AUCTION_REMOVE_ITEM);
            packet.WriteGuid(auction.Auctioneer.To64());
            packet.WriteUInt32(auction.AuctionID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUCTION_PLACE_BID)]
        void HandleAuctionPlaceBId(AuctionPlaceBid auction)
        {
            WorldPacket packet = new(Opcode.CMSG_AUCTION_PLACE_BID);
            packet.WriteGuid(auction.Auctioneer.To64());
            packet.WriteUInt32(auction.AuctionID);
            packet.WriteUInt32((uint)auction.BidAmount);
            SendPacketToServer(packet);
        }
    }
}
