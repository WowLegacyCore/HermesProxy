using Framework.Constants;
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
        [PacketHandler(Opcode.CMSG_BUY_ITEM)]
        void HandleBuyItem(BuyItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_ITEM);
            packet.WriteGuid(item.VendorGUID.To64());
            packet.WriteUInt32(item.Item.ItemID);
            uint quantity = item.Quantity / Global.CurrentSessionData.GameState.GetItemBuyCount(item.Item.ItemID);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                packet.WriteUInt32(item.Slot);
                packet.WriteUInt32(quantity);
            }
            else
                packet.WriteUInt8((byte)quantity);
            packet.WriteUInt8((byte)item.BagSlot);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SELL_ITEM)]
        void HandleSellItem(SellItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SELL_ITEM);
            packet.WriteGuid(item.VendorGUID.To64());
            packet.WriteGuid(item.ItemGUID.To64());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192)) // not sure when this was changed exactly
                packet.WriteUInt32(item.Amount);
            else
                packet.WriteUInt8((byte)item.Amount);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SPLIT_ITEM)]
        void HandleSplitItem(SplitItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SPLIT_ITEM);
            packet.WriteUInt8(item.FromPackSlot);
            packet.WriteUInt8(item.FromSlot);
            packet.WriteUInt8(item.ToPackSlot);
            packet.WriteUInt8(item.ToSlot);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WriteInt32(item.Quantity);
            else
                packet.WriteUInt8((byte)item.Quantity);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SWAP_INV_ITEM)]
        void HandleSwapInvItem(SwapInvItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SWAP_INV_ITEM);
            packet.WriteUInt8(item.Slot1);
            packet.WriteUInt8(item.Slot2);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SWAP_ITEM)]
        void HandleSwapItem(SwapItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SWAP_ITEM);
            packet.WriteUInt8(item.ContainerSlotB);
            packet.WriteUInt8(item.SlotB);
            packet.WriteUInt8(item.ContainerSlotA);
            packet.WriteUInt8(item.SlotA);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_DESTROY_ITEM)]
        void HandleDestroyItem(DestroyItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_DESTROY_ITEM);
            packet.WriteUInt8(item.ContainerId);
            packet.WriteUInt8(item.SlotNum);
            packet.WriteUInt32(item.Count);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUTO_EQUIP_ITEM)]
        void HandleAutoEquipItem(AutoEquipItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTO_EQUIP_ITEM);
            packet.WriteUInt8(item.PackSlot);
            packet.WriteUInt8(item.Slot);
            SendPacketToServer(packet);
        }
    }
}
