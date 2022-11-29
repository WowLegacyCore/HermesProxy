﻿using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_UPDATE_INSTANCE_OWNERSHIP)]
        void HandleUpdateInstanceOwnership(WorldPacket packet)
        {
            UpdateInstanceOwnership instance = new()
            {
                IOwnInstance = packet.ReadUInt32()
            };
            SendPacketToClient(instance);
        }

        [PacketHandler(Opcode.SMSG_INSTANCE_RESET)]
        void HandleInstanceReset(WorldPacket packet)
        {
            InstanceReset reset = new()
            {
                MapID = packet.ReadUInt32()
            };
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_INSTANCE_RESET_FAILED)]
        void HandleInstanceResetFailed(WorldPacket packet)
        {
            InstanceResetFailed reset = new()
            {
                ResetFailedReason = (ResetFailedReason)packet.ReadUInt32(),
                MapID = packet.ReadUInt32()
            };
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_RESET_FAILED_NOTIFY)]
        void HandleResetFailedNotify(WorldPacket packet)
        {
            ResetFailedNotify reset = new();
            packet.ReadUInt32(); // Map ID
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_RAID_INSTANCE_INFO)]
        void HandleRaidInstanceInfo(WorldPacket packet)
        {
            RaidInstanceInfo infos = new();
            int count = packet.ReadInt32();
            for (var i = 0; i < count; ++i)
            {
                InstanceLock instance = new()
                {
                    MapID = packet.ReadUInt32()
                };

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    instance.DifficultyID = (Difficulty)packet.ReadUInt32();
                else
                {
                    if (ModernVersion.ExpansionVersion == 1)
                        instance.DifficultyID = Difficulty.Raid40;
                    else
                        instance.DifficultyID = Difficulty.Raid25N;
                }

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                {
                    instance.InstanceID = packet.ReadUInt64();
                    instance.Locked = packet.ReadBool();
                    instance.Extended = packet.ReadBool();
                    instance.TimeRemaining = packet.ReadInt32();
                }
                else
                {
                    instance.TimeRemaining = packet.ReadInt32();
                    instance.InstanceID = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        packet.ReadUInt32(); // Counter
                }
                infos.LockList.Add(instance);
            }
            SendPacketToClient(infos);
        }

        [PacketHandler(Opcode.SMSG_INSTANCE_SAVE_CREATED)]
        void HandleInstanceSaveCreated(WorldPacket packet)
        {
            InstanceSaveCreated save = new()
            {
                Gm = packet.ReadUInt32() != 0
            };
            SendPacketToClient(save);
        }

        [PacketHandler(Opcode.SMSG_RAID_GROUP_ONLY)]
        void HandleRaidGroupOnly(WorldPacket packet)
        {
            RaidGroupOnly save = new()
            {
                Delay = packet.ReadInt32(),
                Reason = (RaidGroupReason)packet.ReadUInt32()
            };
            SendPacketToClient(save);
        }

        [PacketHandler(Opcode.SMSG_RAID_INSTANCE_MESSAGE)]
        void HandleRaidInstanceMessage(WorldPacket packet)
        {
            RaidInstanceMessage instance = new()
            {
                Type = (InstanceResetWarningType)packet.ReadUInt32(),
                MapID = packet.ReadUInt32()
            };

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                instance.DifficultyID = (Difficulty)packet.ReadUInt32();
            else
            {
                if (ModernVersion.ExpansionVersion == 1)
                    instance.DifficultyID = Difficulty.Raid40;
                else
                    instance.DifficultyID = Difficulty.Raid25N;
            }

            packet.ReadUInt32(); // time

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) &&
                instance.Type == InstanceResetWarningType.Welcome)
            {
                instance.Locked = packet.ReadBool();
                instance.Extended = packet.ReadBool();
            }

            SendPacketToClient(instance);
        }
    }
}
