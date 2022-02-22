using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_GOSSIP_MESSAGE)]
        void HandleGossipmessage(WorldPacket packet)
        {
            GossipMessagePkt gossip = new GossipMessagePkt();
            gossip.GossipGUID = packet.ReadGuid().To128();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                gossip.GossipID = packet.ReadInt32();
            else
                gossip.GossipID = (int)gossip.GossipGUID.GetEntry();

            gossip.TextID = packet.ReadInt32();

            uint optionsCount = packet.ReadUInt32();

            for (uint i = 0; i < optionsCount; i++)
            {
                ClientGossipOption option = new ClientGossipOption();
                option.OptionIndex = packet.ReadInt32();
                option.OptionIcon = packet.ReadUInt8();
                bool hasBox = packet.ReadBool();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    option.OptionCost = packet.ReadInt32();

                option.Text = packet.ReadCString();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    option.Confirm = packet.ReadCString();
                gossip.GossipOptions.Add(option);
            }

            uint questsCount = packet.ReadUInt32();

            for (uint i = 0; i < questsCount; i++)
            {
                ClientGossipQuest quest = new ClientGossipQuest();
                quest.QuestID = packet.ReadUInt32();
                quest.QuestType = packet.ReadInt32();
                quest.QuestLevel = packet.ReadInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    quest.QuestFlags = packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    quest.Repeatable = packet.ReadBool();

                quest.QuestTitle = packet.ReadCString();
                gossip.GossipQuests.Add(quest);
            }

            SendPacketToClient(gossip);
        }

        [PacketHandler(Opcode.SMSG_GOSSIP_COMPLETE)]
        void HandleGossipComplete(WorldPacket packet)
        {
            GossipComplete gossip = new GossipComplete();
            SendPacketToClient(gossip);
        }

        [PacketHandler(Opcode.SMSG_BINDER_CONFIRM)]
        void HandleBinderConfirm(WorldPacket packet)
        {
            BinderConfirm confirm = new BinderConfirm();
            confirm.Guid = packet.ReadGuid().To128();
            SendPacketToClient(confirm);
        }

        [PacketHandler(Opcode.SMSG_VENDOR_INVENTORY)]
        void HandleVendorInventory(WorldPacket packet)
        {
            VendorInventory vendor = new VendorInventory();
            vendor.VendorGUID = packet.ReadGuid().To128();
            byte itemsCount = packet.ReadUInt8();

            if (itemsCount == 0)
            {
                vendor.Reason = packet.ReadUInt8();
                SendPacketToClient(vendor);
                return;
            }

            for (byte i = 0; i < itemsCount; i++)
            {
                VendorItem vendorItem = new();
                vendorItem.Slot = packet.ReadInt32();
                vendorItem.Item.ItemID = packet.ReadUInt32();
                packet.ReadUInt32(); // Display Id
                vendorItem.Quantity = packet.ReadInt32();
                vendorItem.Price = packet.ReadUInt32();
                vendorItem.Durability = packet.ReadInt32();
                vendorItem.StackCount = packet.ReadUInt32();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    vendorItem.ExtendedCostID = packet.ReadInt32();
                Global.CurrentSessionData.GameState.SetItemBuyCount(vendorItem.Item.ItemID, vendorItem.StackCount);
                vendor.Items.Add(vendorItem);
            }

            SendPacketToClient(vendor);
        }
    }
}
