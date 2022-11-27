using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_UPDATE_INSTANCE_OWNERSHIP)]
        void HandleUpdateInstanceOwnership(WorldPacket packet)
        {
            UpdateInstanceOwnership instance = new UpdateInstanceOwnership();
            instance.IOwnInstance = packet.ReadUInt32();
            SendPacketToClient(instance);
        }

        [PacketHandler(Opcode.SMSG_INSTANCE_RESET)]
        void HandleInstanceReset(WorldPacket packet)
        {
            InstanceReset reset = new InstanceReset();
            reset.MapID = packet.ReadUInt32();
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_INSTANCE_RESET_FAILED)]
        void HandleInstanceResetFailed(WorldPacket packet)
        {
            InstanceResetFailed reset = new InstanceResetFailed();
            reset.ResetFailedReason = (ResetFailedReason)packet.ReadUInt32();
            reset.MapID = packet.ReadUInt32();
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_RESET_FAILED_NOTIFY)]
        void HandleResetFailedNotify(WorldPacket packet)
        {
            ResetFailedNotify reset = new ResetFailedNotify();
            packet.ReadUInt32(); // Map ID
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_RAID_INSTANCE_INFO)]
        void HandleRaidInstanceInfo(WorldPacket packet)
        {
            RaidInstanceInfo infos = new RaidInstanceInfo();
            int count = packet.ReadInt32();
            for (var i = 0; i < count; ++i)
            {
                InstanceLock instance = new InstanceLock();
                instance.MapID = packet.ReadUInt32();

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
            InstanceSaveCreated save = new InstanceSaveCreated();
            save.Gm = packet.ReadUInt32() != 0;
            SendPacketToClient(save);
        }

        [PacketHandler(Opcode.SMSG_RAID_GROUP_ONLY)]
        void HandleRaidGroupOnly(WorldPacket packet)
        {
            RaidGroupOnly save = new RaidGroupOnly();
            save.Delay = packet.ReadInt32();
            save.Reason = (RaidGroupReason)packet.ReadUInt32();
            SendPacketToClient(save);
        }

        [PacketHandler(Opcode.SMSG_RAID_INSTANCE_MESSAGE)]
        void HandleRaidInstanceMessage(WorldPacket packet)
        {
            RaidInstanceMessage instance = new RaidInstanceMessage();
            instance.Type = (InstanceResetWarningType)packet.ReadUInt32();
            instance.MapID = packet.ReadUInt32();

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
