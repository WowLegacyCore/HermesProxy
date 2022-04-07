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
        [PacketHandler(Opcode.SMSG_SET_PROFICIENCY)]
        void HandleSetProficiency(WorldPacket packet)
        {
            SetProficiency proficiency = new SetProficiency();
            proficiency.ProficiencyClass = packet.ReadUInt8();
            proficiency.ProficiencyMask = packet.ReadUInt32();
            SendPacketToClient(proficiency);
        }
        [PacketHandler(Opcode.SMSG_BUY_SUCCEEDED)]
        void HandleBuySucceeded(WorldPacket packet)
        {
            BuySucceeded buy = new BuySucceeded();
            buy.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
            buy.Slot = packet.ReadUInt32();
            buy.NewQuantity = packet.ReadInt32();
            buy.QuantityBought = packet.ReadUInt32();
            SendPacketToClient(buy);
        }
        [PacketHandler(Opcode.SMSG_ITEM_PUSH_RESULT)]
        void HandleItemPushResult(WorldPacket packet)
        {
            ItemPushResult item = new ItemPushResult();
            item.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
            if (packet.ReadUInt32() == 1)
                item.DisplayText = ItemPushResult.DisplayType.Normal;
            else
                item.DisplayText = ItemPushResult.DisplayType.EncounterLoot;
            item.Created = packet.ReadUInt32() == 1;
            if (packet.ReadUInt32() == 0)
                item.DisplayText = ItemPushResult.DisplayType.Hidden;
            item.Slot = packet.ReadUInt8();
            item.SlotInBag = packet.ReadInt32();
            item.Item.ItemID = packet.ReadUInt32();
            item.Item.RandomPropertiesSeed = packet.ReadUInt32();
            item.Item.RandomPropertiesID = packet.ReadUInt32();
            item.Quantity = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                item.QuantityInInventory = packet.ReadUInt32();
            else
                item.QuantityInInventory = item.Quantity;

            if (item.Slot == Enums.Classic.InventorySlots.Bag0 && item.SlotInBag >= 0 &&
                item.PlayerGUID == GetSession().GameState.CurrentPlayerGuid)
                item.ItemGUID = GetSession().GameState.GetInventorySlotItem(item.SlotInBag).To128(GetSession().GameState);
            else
                item.ItemGUID = WowGuid128.Empty;

            SendPacketToClient(item);
        }
        [PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_OK)]
        void HandleReadItemResultOk(WorldPacket packet)
        {
            ReadItemResultOK read = new ReadItemResultOK();
            read.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
            SendPacketToClient(read);
        }
        [PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_FAILED)]
        void HandleReadItemResultFailed(WorldPacket packet)
        {
            ReadItemResultFailed read = new ReadItemResultFailed();
            read.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
            read.Subcode = 2;
            SendPacketToClient(read);
        }
        [PacketHandler(Opcode.SMSG_BUY_FAILED)]
        void HandleBuyFailed(WorldPacket packet)
        {
            BuyFailed fail = new BuyFailed();
            fail.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
            fail.Slot = packet.ReadUInt32();
            fail.Reason = (BuyResult)packet.ReadUInt8();
            SendPacketToClient(fail);
        }
        [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleInventoryChangeFailureVanilla(WorldPacket packet)
        {
            InventoryChangeFailure failure = new();
            failure.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8());
            if (failure.BagResult == InventoryResult.Ok)
                return;

            switch (failure.BagResult)
            {
                case InventoryResult.CantEquipLevel:
                    failure.Level = packet.ReadInt32();
                    break;
            }

            failure.Item[0] = packet.ReadGuid().To128(GetSession().GameState);
            failure.Item[1] = packet.ReadGuid().To128(GetSession().GameState);
            failure.ContainerBSlot = packet.ReadUInt8();

            SendPacketToClient(failure);
        }
        [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.V2_0_1_6180)]
        void HandleInventoryChangeFailure(WorldPacket packet)
        {
            InventoryChangeFailure failure = new();
            failure.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8());
            if (failure.BagResult == InventoryResult.Ok)
                return;

            failure.Item[0] = packet.ReadGuid().To128(GetSession().GameState);
            failure.Item[1] = packet.ReadGuid().To128(GetSession().GameState);
            failure.ContainerBSlot = packet.ReadUInt8();

            switch (failure.BagResult)
            {
                case InventoryResult.CantEquipLevel:
                case InventoryResult.PurchaseLevelTooLow:
                    failure.Level = packet.ReadInt32();
                    break;
                case InventoryResult.EventAutoEquipBindConfirm:
                    failure.SrcContainer = packet.ReadGuid().To128(GetSession().GameState);
                    failure.SrcSlot = packet.ReadInt32();
                    failure.DstContainer = packet.ReadGuid().To128(GetSession().GameState);
                    break;
                case InventoryResult.ItemMaxLimitCategoryCountExceeded:
                case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
                case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
                    failure.LimitCategory = packet.ReadInt32();
                    break;
            }
            SendPacketToClient(failure);
        }
        [PacketHandler(Opcode.SMSG_DURABILITY_DAMAGE_DEATH)]
        void HandleDurabilityDamageDeath(WorldPacket packet)
        {
            DurabilityDamageDeath death = new DurabilityDamageDeath();
            death.Percent = 10;
            SendPacketToClient(death);
        }
        [PacketHandler(Opcode.SMSG_ITEM_COOLDOWN)]
        void HandleItemCooldown(WorldPacket packet)
        {
            ItemCooldown item = new ItemCooldown();
            item.ItemGuid = packet.ReadGuid().To128(GetSession().GameState);
            item.SpellID = packet.ReadUInt32();
            item.Cooldown = 30000;
            SendPacketToClient(item);
        }
    }
}
