﻿using HermesProxy.Enums;
using HermesProxy.World.Enums;
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
            GetSession().GameState.LastLootTargetGuid = packet.ReadGuid();
            loot.Owner = GetSession().GameState.LastLootTargetGuid.To128(GetSession().GameState);
            loot.LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid();
            loot.AcquireReason = (LootType)packet.ReadUInt8();
            if (loot.AcquireReason == LootType.None)
            {
                loot.FailureReason = (LootError)packet.ReadUInt8();
                return;
            }
            loot.LootMethod = GetSession().GameState.GetCurrentLootMethod();

            loot.Coins = packet.ReadUInt32();

            var itemsCount = packet.ReadUInt8();
            for (var i = 0; i < itemsCount; ++i)
            {
                LootItemData lootItem = new()
                {
                    LootListID = packet.ReadUInt8()
                };
                lootItem.Loot.ItemID = packet.ReadUInt32();
                lootItem.Quantity = packet.ReadUInt32();
                packet.ReadUInt32(); // DisplayID
                lootItem.Loot.RandomPropertiesSeed = packet.ReadUInt32();
                lootItem.Loot.RandomPropertiesID = packet.ReadUInt32();
                var uiType = (LootSlotTypeLegacy)packet.ReadUInt8();
                lootItem.UIType = (LootSlotTypeModern)Enum.Parse(typeof(LootSlotTypeModern), uiType.ToString());
                loot.Items.Add(lootItem);
            }
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_RELEASE)]
        void HandleLootRelease(WorldPacket packet)
        {
            LootReleaseResponse loot = new();
            WowGuid64 owner = packet.ReadGuid();
            loot.Owner = owner.To128(GetSession().GameState);
            loot.LootObj = owner.ToLootGuid();
            packet.ReadBool(); // unk
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_REMOVED)]
        void HandleLootRemoved(WorldPacket packet)
        {
            LootRemoved loot = new()
            {
                Owner = GetSession().GameState.LastLootTargetGuid.To128(GetSession().GameState),
                LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid(),
                LootListID = packet.ReadUInt8()
            };
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_MONEY_NOTIFY)]
        void HandleLootMoneyNotify(WorldPacket packet)
        {
            LootMoneyNotify loot = new()
            {
                Money = packet.ReadUInt32()
            };
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                loot.SoleLooter = packet.ReadBool();
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_CLEAR_MONEY)]
        void HandleLootCelarMoney(WorldPacket packet)
        {
            CoinRemoved loot = new()
            {
                LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid()
            };
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_START_ROLL)]
        void HandleLootStartRoll(WorldPacket packet)
        {
            StartLootRoll loot = new();
            WowGuid64 owner = packet.ReadGuid();
            loot.LootObj = owner.ToLootGuid();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                loot.MapID = packet.ReadUInt32();
            else
                loot.MapID = (uint)GetSession().GameState.CurrentMapId;
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Item.Loot.ItemID = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                loot.Item.Quantity = packet.ReadUInt32();
            else
                loot.Item.Quantity = 1;
            loot.RollTime = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                loot.ValidRolls = (RollMask)packet.ReadUInt8();
            else
                loot.ValidRolls = RollMask.AllNoDisenchant;
            SendPacketToClient(loot);

            if (GetSession().GameState.IsPassingOnLoot)
            {
                WorldPacket packet2 = new(Opcode.CMSG_LOOT_ROLL);
                packet2.WriteGuid(owner);
                packet2.WriteUInt32(loot.Item.LootListID);
                packet2.WriteUInt8((byte)RollType.Pass);
                SendPacketToServer(packet2);
            }
        }

        [PacketHandler(Opcode.SMSG_LOOT_ROLL)]
        void HandleLootRoll(WorldPacket packet)
        {
            LootRollBroadcast loot = new();
            WowGuid64 owner = packet.ReadGuid();
            loot.LootObj = owner.ToLootGuid();
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Player = packet.ReadGuid().To128(GetSession().GameState);
            loot.Item.Loot.ItemID = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
            loot.Item.Quantity = 1;
            loot.Roll = packet.ReadUInt8();

            byte rollType = packet.ReadUInt8();
            if (loot.Roll == 128 && rollType == 128)
                loot.RollType = RollType.Pass;
            else if (loot.Roll == 0 && rollType == 0)
                loot.RollType = RollType.Need;
            else
                loot.RollType = (RollType) rollType;

            if (loot.Roll == 128)
                loot.Roll = 0;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                loot.Autopassed = packet.ReadBool();

            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_ROLL_WON)]
        void HandleLootRollWon(WorldPacket packet)
        {
            LootRollWon loot = new()
            {
                LootObj = packet.ReadGuid().ToLootGuid()
            };
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Item.Loot.ItemID = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
            loot.Item.Quantity = 1;
            loot.Winner = packet.ReadGuid().To128(GetSession().GameState);
            loot.Roll = packet.ReadUInt8();
            loot.RollType = (RollType)packet.ReadUInt8();
            if (loot.RollType == RollType.Need)
                loot.MainSpec = 128;
            SendPacketToClient(loot);

            LootRollsComplete complete = new()
            {
                LootObj = loot.LootObj,
                LootListID = loot.Item.LootListID
            };
            SendPacketToClient(complete);
        }

        [PacketHandler(Opcode.SMSG_LOOT_ALL_PASSED)]
        void HandleLootAllPassed(WorldPacket packet)
        {
            LootAllPassed loot = new()
            {
                LootObj = packet.ReadGuid().ToLootGuid()
            };
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Item.Loot.ItemID = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
            loot.Item.Quantity = 1;
            SendPacketToClient(loot);

            LootRollsComplete complete = new()
            {
                LootObj = loot.LootObj,
                LootListID = loot.Item.LootListID
            };
            SendPacketToClient(complete);
        }

        [PacketHandler(Opcode.SMSG_LOOT_MASTER_LIST)]
        void HandleLootMasterList(WorldPacket packet)
        {
            if (GetSession().GameState.LastLootTargetGuid == null)
                return;

            LootList list = new()
            {
                Owner = GetSession().GameState.LastLootTargetGuid.To128(GetSession().GameState),
                LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid(),
                Master = GetSession().GameState.CurrentPlayerGuid
            };
            SendPacketToClient(list);

            MasterLootCandidateList loot = new()
            {
                LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid()
            };
            byte count = packet.ReadUInt8();
            for (byte i = 0; i < count; i++)
            {
                WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
                loot.Players.Add(guid);
            }
            SendPacketToClient(loot);
        }
    }
}
