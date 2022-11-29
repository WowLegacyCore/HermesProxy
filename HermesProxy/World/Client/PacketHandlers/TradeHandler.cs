using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using static HermesProxy.World.Server.Packets.TradeUpdated;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_TRADE_STATUS)]
        void HandleTradeStatus(WorldPacket packet)
        {
            TradeStatusPkt trade = new()
            {
                Status = (TradeStatus)packet.ReadUInt32()
            };
            switch (trade.Status)
            {
                case TradeStatus.Proposed:
                    trade.Partner = packet.ReadGuid().To128(GetSession().GameState);
                    trade.PartnerAccount = GetSession().GetGameAccountGuidForPlayer(trade.Partner);
                    break;
                case TradeStatus.Initiated:
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        trade.Id = packet.ReadUInt32();
                    break;
                case TradeStatus.Failed:
                    trade.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
                    trade.FailureForYou = packet.ReadBool();
                    trade.ItemID = packet.ReadUInt32();
                    break;
                case TradeStatus.WrongRealm:
                case TradeStatus.NotOnTaplist:
                    trade.TradeSlot = packet.ReadUInt8();
                    break;
            }
            SendPacketToClient(trade);
        }

        [PacketHandler(Opcode.SMSG_TRADE_STATUS_EXTENDED)]
        void HandleTradeStatusExtended(WorldPacket packet)
        {
            TradeUpdated trade = new()
            {
                WhichPlayer = packet.ReadUInt8()
            };
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                trade.Id = packet.ReadUInt32();
            trade.ClientStateIndex = packet.ReadUInt32();
            trade.CurrentStateIndex = packet.ReadUInt32();
            trade.Gold = packet.ReadUInt32();
            trade.ProposedEnchantment = packet.ReadInt32();
            while (packet.CanRead())
            {
                TradeItem item = new()
                {
                    Unwrapped = new UnwrappedTradeItem(),
                    Slot = packet.ReadUInt8()
                };
                item.Item.ItemID = packet.ReadUInt32();
                packet.ReadUInt32(); // Item Display ID
                item.StackCount = packet.ReadInt32();
                packet.ReadUInt32(); // Is Wrapped
                item.GiftCreator = packet.ReadGuid().To128(GetSession().GameState);
                item.Unwrapped.EnchantID = packet.ReadInt32();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    for (var i = 0; i < 3; ++i)
                        packet.ReadUInt32(); // Item Enchantment Id
                }
                item.Unwrapped.Creator = packet.ReadGuid().To128(GetSession().GameState);
                item.Unwrapped.Charges = packet.ReadInt32();
                item.Item.RandomPropertiesSeed = packet.ReadUInt32();
                item.Item.RandomPropertiesID = packet.ReadUInt32();
                item.Unwrapped.Lock = packet.ReadUInt32() != 0;
                item.Unwrapped.MaxDurability = packet.ReadUInt32();
                item.Unwrapped.Durability = packet.ReadUInt32();
                trade.Items.Add(item);
            }
            SendPacketToClient(trade);
        }
    }
}
