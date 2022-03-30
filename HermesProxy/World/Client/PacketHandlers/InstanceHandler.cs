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
    }
}
