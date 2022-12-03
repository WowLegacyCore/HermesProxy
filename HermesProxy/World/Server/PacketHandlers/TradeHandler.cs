using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_INITIATE_TRADE)]
        void HandleInitiateTrade(InitiateTrade trade)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_INITIATE_TRADE);
            packet.WriteGuid(trade.Guid.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_TRADE_GOLD)]
        void HandleSetTradeGold(SetTradeGold trade)
        {
            var tradeSession = GetSession().GameState.CurrentTrade;
            if (tradeSession == null)
            {
                Log.Print(LogType.Error, "Got CMSG_SET_TRADE_GOLD without trade session");
                return;
            }
            tradeSession.ClientStateIndex++;
            
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_TRADE_GOLD);
            packet.WriteInt32((int)trade.Coinage);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ACCEPT_TRADE)]
        void HandleAcceptTrade(AcceptTrade trade)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ACCEPT_TRADE);
            packet.WriteUInt32(trade.StateIndex);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BEGIN_TRADE)]
        [PacketHandler(Opcode.CMSG_BUSY_TRADE)]
        [PacketHandler(Opcode.CMSG_CANCEL_TRADE)]
        [PacketHandler(Opcode.CMSG_UNACCEPT_TRADE)]
        [PacketHandler(Opcode.CMSG_IGNORE_TRADE)]
        void HandleEmptyTradePacket(EmptyClientPacket trade)
        {
            WorldPacket packet = new WorldPacket(trade.GetUniversalOpcode());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CLEAR_TRADE_ITEM)]
        void HandleClearTradeItem(ClearTradeItem trade)
        {
            var tradeSession = GetSession().GameState.CurrentTrade;
            if (tradeSession == null)
            {
                Log.Print(LogType.Error, "Got CMSG_SET_TRADE_GOLD without trade session");
                return;
            }
            tradeSession.ClientStateIndex++;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_CLEAR_TRADE_ITEM);
            packet.WriteUInt8(trade.TradeSlot);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_TRADE_ITEM)]
        void HandleSetTradeItem(SetTradeItem trade)
        {
            var tradeSession = GetSession().GameState.CurrentTrade;
            if (tradeSession == null)
            {
                Log.Print(LogType.Error, "Got CMSG_SET_TRADE_GOLD without trade session");
                return;
            }
            tradeSession.ClientStateIndex++;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_TRADE_ITEM);
            packet.WriteUInt8(trade.TradeSlot);
            byte containerSlot = trade.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(trade.PackSlot) : trade.PackSlot;
            byte slot = trade.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(trade.ItemSlotInPack) : trade.ItemSlotInPack;
            packet.WriteUInt8(containerSlot);
            packet.WriteUInt8(slot);
            SendPacketToServer(packet);
        }
    }
}
