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
        [PacketHandler(Opcode.SMSG_LOOT_RESPONSE)]
        void HandleLootResponse(WorldPacket packet)
        {
            LootResponse loot = new();
            loot.Owner = packet.ReadGuid().To128();
            Global.CurrentSessionData.GameState.LastLootTargetGuid = loot.Owner;
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)Global.CurrentSessionData.GameState.CurrentMapId, loot.Owner.GetEntry(), loot.Owner.GetLow());
            loot.AcquireReason = (LootType)packet.ReadUInt8();
            if (loot.AcquireReason == LootType.None)
            {
                loot.FailureReason = (LootError)packet.ReadUInt8();
                return;
            }

            loot.Coins = packet.ReadUInt32();

            var itemsCount = packet.ReadUInt8();
            for (var i = 0; i < itemsCount; ++i)
            {
                LootItemData lootItem = new();
                lootItem.LootListID = packet.ReadUInt8();
                lootItem.Loot.ItemID = packet.ReadUInt32();
                lootItem.Quantity = packet.ReadUInt32();
                packet.ReadUInt32(); // DisplayID
                lootItem.Loot.RandomPropertiesSeed = packet.ReadUInt32();
                lootItem.Loot.RandomPropertiesID = packet.ReadUInt32();
                lootItem.UIType = (LootSlotType)packet.ReadUInt8();
                loot.Items.Add(lootItem);
            }
            SendPacketToClient(loot);
        }
        [PacketHandler(Opcode.SMSG_LOOT_RELEASE)]
        void HandleLootRelease(WorldPacket packet)
        {
            LootReleaseResponse loot = new();
            loot.Owner = packet.ReadGuid().To128();
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)Global.CurrentSessionData.GameState.CurrentMapId, loot.Owner.GetEntry(), loot.Owner.GetLow());
            packet.ReadBool(); // unk
            SendPacketToClient(loot);
        }
        [PacketHandler(Opcode.SMSG_LOOT_REMOVED)]
        void HandleLootRemoved(WorldPacket packet)
        {
            LootRemoved loot = new();
            loot.Owner = Global.CurrentSessionData.GameState.LastLootTargetGuid;
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)Global.CurrentSessionData.GameState.CurrentMapId, loot.Owner.GetEntry(), loot.Owner.GetLow());
            loot.LootListID = packet.ReadUInt8();
            SendPacketToClient(loot);
        }
        [PacketHandler(Opcode.SMSG_LOOT_CLEAR_MONEY)]
        void HandleLootCelarMoney(WorldPacket packet)
        {
            CoinRemoved loot = new();
            WowGuid128 owner = Global.CurrentSessionData.GameState.LastLootTargetGuid;
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)Global.CurrentSessionData.GameState.CurrentMapId, owner.GetEntry(), owner.GetLow());
            SendPacketToClient(loot);
        }
    }
}
