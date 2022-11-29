using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_SET_PROFICIENCY)]
        void HandleSetProficiency(WorldPacket packet)
        {
            SetProficiency proficiency = new()
            {
                ProficiencyClass = packet.ReadUInt8(),
                ProficiencyMask = packet.ReadUInt32()
            };
            SendPacketToClient(proficiency);
        }
        [PacketHandler(Opcode.SMSG_BUY_SUCCEEDED)]
        void HandleBuySucceeded(WorldPacket packet)
        {
            BuySucceeded buy = new()
            {
                VendorGUID = packet.ReadGuid().To128(GetSession().GameState),
                Slot = packet.ReadUInt32(),
                NewQuantity = packet.ReadInt32(),
                QuantityBought = packet.ReadUInt32()
            };
            SendPacketToClient(buy);
        }
        [PacketHandler(Opcode.SMSG_ITEM_PUSH_RESULT)]
        void HandleItemPushResult(WorldPacket packet)
        {
            ItemPushResult item = new()
            {
                PlayerGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            bool fromNPC = packet.ReadUInt32() == 1;
            item.Created = packet.ReadUInt32() == 1;
            bool showInChat = packet.ReadUInt32() == 1;

            if (fromNPC && !item.Created)
            {
                item.DisplayText = ItemPushResult.DisplayType.Received;
                item.Pushed = true;
            }
            else if (!showInChat)
                item.DisplayText = ItemPushResult.DisplayType.Hidden;
            else
                item.DisplayText = ItemPushResult.DisplayType.Loot;

            item.Slot = packet.ReadUInt8();
            item.SlotInBag = packet.ReadInt32();
            item.Item.ItemID = packet.ReadUInt32();
            item.Item.RandomPropertiesSeed = packet.ReadUInt32();
            item.Item.RandomPropertiesID = packet.ReadUInt32();
            item.Quantity = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                item.QuantityInInventory = packet.ReadUInt32();
            else
            {
                uint currentCount = 0;
                QuestObjective objective = GameData.GetQuestObjectiveForItem(item.Item.ItemID);
                if (objective != null)
                {
                    var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
                    int questsCount = LegacyVersion.GetQuestLogSize();
                    for (int i = 0; i < questsCount; i++)
                    {
                        QuestLog logEntry = ReadQuestLogEntry(i, null, updateFields);
                        if (logEntry == null || logEntry.QuestID == null)
                            continue;
                        if (logEntry.QuestID != objective.QuestID)
                            continue;
                        if (logEntry.ObjectiveProgress[objective.StorageIndex] == null)
                            continue;

                        currentCount = (uint)logEntry.ObjectiveProgress[objective.StorageIndex];
                        break;
                    }
                }
                item.QuantityInInventory = item.Quantity + currentCount;
            }

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
            ReadItemResultOK read = new()
            {
                ItemGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(read);
        }
        [PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_FAILED)]
        void HandleReadItemResultFailed(WorldPacket packet)
        {
            ReadItemResultFailed read = new()
            {
                ItemGUID = packet.ReadGuid().To128(GetSession().GameState),
                Subcode = 2
            };
            SendPacketToClient(read);
        }
        [PacketHandler(Opcode.SMSG_BUY_FAILED)]
        void HandleBuyFailed(WorldPacket packet)
        {
            BuyFailed fail = new()
            {
                VendorGUID = packet.ReadGuid().To128(GetSession().GameState),
                Slot = packet.ReadUInt32(),
                Reason = (BuyResult)packet.ReadUInt8()
            };
            SendPacketToClient(fail);
        }
        [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleInventoryChangeFailureVanilla(WorldPacket packet)
        {
            InventoryChangeFailure failure = new()
            {
                BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8())
            };
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

            if (GetSession().GameState.CurrentClientNormalCast != null &&
               !GetSession().GameState.CurrentClientNormalCast.HasStarted &&
                GetSession().GameState.CurrentClientNormalCast.ItemGUID == failure.Item[0])
            {
                GetSession().InstanceSocket.SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, false);
                GetSession().GameState.CurrentClientNormalCast = null;
            }
        }
        [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.V2_0_1_6180)]
        void HandleInventoryChangeFailure(WorldPacket packet)
        {
            InventoryChangeFailure failure = new()
            {
                BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8())
            };
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

            if (GetSession().GameState.CurrentClientNormalCast != null &&
               !GetSession().GameState.CurrentClientNormalCast.HasStarted &&
                GetSession().GameState.CurrentClientNormalCast.ItemGUID == failure.Item[0])
            {
                GetSession().InstanceSocket.SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, false);
                GetSession().GameState.CurrentClientNormalCast = null;
            }
        }
        [PacketHandler(Opcode.SMSG_DURABILITY_DAMAGE_DEATH)]
        void HandleDurabilityDamageDeath(WorldPacket packet)
        {
            DurabilityDamageDeath death = new()
            {
                Percent = 10
            };
            SendPacketToClient(death);
        }
        [PacketHandler(Opcode.SMSG_ITEM_COOLDOWN)]
        void HandleItemCooldown(WorldPacket packet)
        {
            ItemCooldown item = new()
            {
                ItemGuid = packet.ReadGuid().To128(GetSession().GameState),
                SpellID = packet.ReadUInt32(),
                Cooldown = 30000
            };
            SendPacketToClient(item);
        }
        [PacketHandler(Opcode.SMSG_SELL_RESPONSE)]
        void HandleSellResponse(WorldPacket packet)
        {
            SellResponse sell = new()
            {
                VendorGUID = packet.ReadGuid().To128(GetSession().GameState),
                ItemGUID = packet.ReadGuid().To128(GetSession().GameState),
                Reason = packet.ReadUInt8()
            };
            SendPacketToClient(sell);
        }
        [PacketHandler(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE)]
        void HandleItemEnchantTimeUpdate(WorldPacket packet)
        {
            ItemEnchantTimeUpdate enchant = new()
            {
                ItemGuid = packet.ReadGuid().To128(GetSession().GameState),
                Slot = packet.ReadUInt32(),
                DurationLeft = packet.ReadUInt32(),
                OwnerGuid = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(enchant);
        }

        [PacketHandler(Opcode.SMSG_ENCHANTMENT_LOG)]
        void HandleEnchantmentLog(WorldPacket packet)
        {
            EnchantmentLog enchantment = new();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                enchantment.Owner = packet.ReadPackedGuid().To128(GetSession().GameState);
                enchantment.Caster = packet.ReadPackedGuid().To128(GetSession().GameState);
            }
            else
            {
                enchantment.Owner = packet.ReadGuid().To128(GetSession().GameState);
                enchantment.Caster = packet.ReadGuid().To128(GetSession().GameState);
            }
            enchantment.ItemID = packet.ReadInt32();
            var session = GetSession().GameState;

            for (int i = 0; i < 23; i++)
            {
                if (session.GetItemId(session.GetInventorySlotItem(i).To128(session)).Equals((uint)enchantment.ItemID))
                {
                    enchantment.ItemGUID = session.GetInventorySlotItem(i).To128(session);
                    break;
                }
            }
            if (enchantment.ItemGUID == null)
                return;

            enchantment.Enchantment = packet.ReadInt32();
            SendPacketToClient(enchantment);
        }
    }
}
