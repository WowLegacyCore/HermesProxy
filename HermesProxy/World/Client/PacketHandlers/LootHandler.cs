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
            GetSession().GameState.LastLootTargetGuid = loot.Owner;
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, loot.Owner.GetEntry(), loot.Owner.GetCounter());
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
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, loot.Owner.GetEntry(), loot.Owner.GetCounter());
            packet.ReadBool(); // unk
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_REMOVED)]
        void HandleLootRemoved(WorldPacket packet)
        {
            LootRemoved loot = new();
            loot.Owner = GetSession().GameState.LastLootTargetGuid;
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, loot.Owner.GetEntry(), loot.Owner.GetCounter());
            loot.LootListID = packet.ReadUInt8();
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_MONEY_NOTIFY)]
        void HandleLootMoneyNotify(WorldPacket packet)
        {
            LootMoneyNotify loot = new();
            loot.Money = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                loot.SoleLooter = packet.ReadBool();
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_CLEAR_MONEY)]
        void HandleLootCelarMoney(WorldPacket packet)
        {
            CoinRemoved loot = new();
            WowGuid128 owner = GetSession().GameState.LastLootTargetGuid;
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, owner.GetEntry(), owner.GetCounter());
            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_START_ROLL)]
        void HandleLootStartRoll(WorldPacket packet)
        {
            StartLootRoll loot = new StartLootRoll();
            WowGuid64 owner = packet.ReadGuid();
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, owner.GetEntry(), owner.GetCounter());
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
                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_LOOT_ROLL);
                packet2.WriteGuid(owner);
                packet2.WriteUInt32(loot.Item.LootListID);
                packet2.WriteUInt8((byte)RollType.Pass);
                SendPacketToServer(packet2);
            }
        }

        [PacketHandler(Opcode.SMSG_LOOT_ROLL)]
        void HandleLootRoll(WorldPacket packet)
        {
            LootRollBroadcast loot = new LootRollBroadcast();
            WowGuid64 owner = packet.ReadGuid();
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, owner.GetEntry(), owner.GetCounter());
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Player = packet.ReadGuid().To128();
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
                loot.RollType = RollType.Greed;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                loot.Autopassed = packet.ReadBool();

            SendPacketToClient(loot);
        }

        [PacketHandler(Opcode.SMSG_LOOT_ROLL_WON)]
        void HandleLootRollWon(WorldPacket packet)
        {
            LootRollWon loot = new LootRollWon();
            WowGuid64 owner = packet.ReadGuid();
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, owner.GetEntry(), owner.GetCounter());
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Item.Loot.ItemID = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
            loot.Item.Quantity = 1;
            loot.Winner = packet.ReadGuid().To128();
            loot.Roll = packet.ReadUInt8();
            loot.RollType = (RollType)packet.ReadUInt8();
            if (loot.RollType == RollType.Need)
                loot.MainSpec = 128;
            SendPacketToClient(loot);

            LootRollsComplete complete = new LootRollsComplete();
            complete.LootObj = loot.LootObj;
            complete.LootListID = loot.Item.LootListID;
            SendPacketToClient(complete);
        }

        [PacketHandler(Opcode.SMSG_LOOT_ALL_PASSED)]
        void HandleLootAllPassed(WorldPacket packet)
        {
            LootAllPassed loot = new LootAllPassed();
            WowGuid64 owner = packet.ReadGuid();
            loot.LootObj = WowGuid128.Create(HighGuidType703.LootObject, (uint)GetSession().GameState.CurrentMapId, owner.GetEntry(), owner.GetCounter());
            loot.Item.LootListID = (byte)packet.ReadUInt32();
            loot.Item.Loot.ItemID = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
            loot.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
            loot.Item.Quantity = 1;
            SendPacketToClient(loot);

            LootRollsComplete complete = new LootRollsComplete();
            complete.LootObj = loot.LootObj;
            complete.LootListID = loot.Item.LootListID;
            SendPacketToClient(complete);
        }
    }
}
