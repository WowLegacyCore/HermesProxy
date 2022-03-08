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
            uint quantity = item.Quantity / GetSession().GameState.GetItemBuyCount(item.Item.ItemID);
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
            byte containerSlot1 = item.FromPackSlot != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.FromPackSlot) : item.FromPackSlot;
            byte slot1 = item.FromPackSlot == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.FromSlot) : item.FromSlot;
            byte containerSlot2 = item.ToPackSlot != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.ToPackSlot) : item.ToPackSlot;
            byte slot2 = item.ToPackSlot == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.ToSlot) : item.ToSlot;
            packet.WriteUInt8(containerSlot1);
            packet.WriteUInt8(slot1);
            packet.WriteUInt8(containerSlot2);
            packet.WriteUInt8(slot2);
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
            byte slot1 = ModernVersion.AdjustInventorySlot(item.Slot1);
            byte slot2 = ModernVersion.AdjustInventorySlot(item.Slot2);
            packet.WriteUInt8(slot1);
            packet.WriteUInt8(slot2);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SWAP_ITEM)]
        void HandleSwapItem(SwapItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SWAP_ITEM);
            byte containerSlotB = item.ContainerSlotB != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.ContainerSlotB) : item.ContainerSlotB;
            byte slotB = item.ContainerSlotB == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.SlotB) : item.SlotB;
            byte containerSlotA = item.ContainerSlotA != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.ContainerSlotA) : item.ContainerSlotA;
            byte slotA = item.ContainerSlotA == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.SlotA) : item.SlotA;
            packet.WriteUInt8(containerSlotB);
            packet.WriteUInt8(slotB);
            packet.WriteUInt8(containerSlotA);
            packet.WriteUInt8(slotA);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_DESTROY_ITEM)]
        void HandleDestroyItem(DestroyItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_DESTROY_ITEM);
            byte containerSlot = item.ContainerId != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.ContainerId) : item.ContainerId;
            byte slot = item.ContainerId == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.SlotNum) : item.SlotNum;
            packet.WriteUInt8(containerSlot);
            packet.WriteUInt8(slot);
            packet.WriteUInt32(item.Count);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUTO_EQUIP_ITEM)]
        void HandleAutoEquipItem(AutoEquipItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTO_EQUIP_ITEM);
            byte containerSlot = item.PackSlot != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.PackSlot) : item.PackSlot;
            byte slot = item.PackSlot == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.Slot) : item.Slot;
            packet.WriteUInt8(containerSlot);
            packet.WriteUInt8(slot);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_READ_ITEM)]
        void HandleReadItem(ReadItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_READ_ITEM);
            byte containerSlot = item.PackSlot != Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.PackSlot) : item.PackSlot;
            byte slot = item.PackSlot == Objects.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(item.Slot) : item.Slot;
            packet.WriteUInt8(containerSlot);
            packet.WriteUInt8(slot);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_BUY_BACK_ITEM)]
        void HandleBuyBackItem(BuyBackItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_BACK_ITEM);
            packet.WriteGuid(item.VendorGUID.To64());
            byte slot = ModernVersion.AdjustInventorySlot((byte)item.Slot);
            packet.WriteUInt32(slot);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_REPAIR_ITEM)]
        void HandleRepairItem(RepairItem item)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_REPAIR_ITEM);
            packet.WriteGuid(item.VendorGUID.To64());
            packet.WriteGuid(item.ItemGUID.To64());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteBool(item.UseGuildBank);
            SendPacketToServer(packet);
        }
    }
}
