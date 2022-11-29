using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
        [PacketHandler(Opcode.CMSG_MOVE_DISMISS_VEHICLE)]
        [PacketHandler(Opcode.CMSG_MOVE_FALL_LAND)]
        [PacketHandler(Opcode.CMSG_MOVE_FALL_RESET)]
        [PacketHandler(Opcode.CMSG_MOVE_HEARTBEAT)]
        [PacketHandler(Opcode.CMSG_MOVE_JUMP)]
        [PacketHandler(Opcode.CMSG_MOVE_REMOVE_MOVEMENT_FORCES)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_FACING)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_FLY)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_PITCH)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_RUN_MODE)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_WALK_MODE)]
        [PacketHandler(Opcode.CMSG_MOVE_START_ASCEND)]
        [PacketHandler(Opcode.CMSG_MOVE_START_BACKWARD)]
        [PacketHandler(Opcode.CMSG_MOVE_START_DESCEND)]
        [PacketHandler(Opcode.CMSG_MOVE_START_FORWARD)]
        [PacketHandler(Opcode.CMSG_MOVE_START_PITCH_DOWN)]
        [PacketHandler(Opcode.CMSG_MOVE_START_PITCH_UP)]
        [PacketHandler(Opcode.CMSG_MOVE_START_SWIM)]
        [PacketHandler(Opcode.CMSG_MOVE_START_TURN_LEFT)]
        [PacketHandler(Opcode.CMSG_MOVE_START_TURN_RIGHT)]
        [PacketHandler(Opcode.CMSG_MOVE_START_STRAFE_LEFT)]
        [PacketHandler(Opcode.CMSG_MOVE_START_STRAFE_RIGHT)]
        [PacketHandler(Opcode.CMSG_MOVE_STOP)]
        [PacketHandler(Opcode.CMSG_MOVE_STOP_ASCEND)]
        [PacketHandler(Opcode.CMSG_MOVE_STOP_PITCH)]
        [PacketHandler(Opcode.CMSG_MOVE_STOP_STRAFE)]
        [PacketHandler(Opcode.CMSG_MOVE_STOP_SWIM)]
        [PacketHandler(Opcode.CMSG_MOVE_STOP_TURN)]
        [PacketHandler(Opcode.CMSG_MOVE_DOUBLE_JUMP)]
        void HandlePlayerMove(ClientPlayerMovement movement)
        {
            string opcodeName = movement.GetUniversalOpcode().ToString();
            opcodeName = opcodeName.Replace("CMSG", "MSG");
            uint opcode = Opcodes.GetOpcodeValueForVersion(opcodeName, Framework.Settings.ServerBuild);
            if (opcode == 0)
                opcode = Opcodes.GetOpcodeValueForVersion("MSG_MOVE_SET_FACING", Framework.Settings.ServerBuild);

            WorldPacket packet = new WorldPacket(opcode);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(movement.Guid.To64());
            movement.MoveInfo.WriteMovementInfoLegacy(packet);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_TELEPORT_ACK)]
        void HandleMoveTeleportAck(MoveTeleportAck teleport)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_TELEPORT_ACK);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(teleport.MoverGUID.To64());
            else
                packet.WriteGuid(teleport.MoverGUID.To64());
            packet.WriteUInt32(teleport.MoveCounter);
            packet.WriteUInt32(teleport.MoveTime);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_WORLD_PORT_RESPONSE)]
        void HandleWorldPortResponse(WorldPortResponse teleport)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_WORLDPORT_ACK);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK)]
        void HandleMoveForceSpeedChangeAck(MovementSpeedAck speed)
        {
            var opcode = speed.GetUniversalOpcode();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180)
                && opcode is Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK
                          or Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)
                return; // This is probably an ack by our swim to fly speed change for vanilla

            WorldPacket packet = new WorldPacket(opcode);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(speed.MoverGUID.To64());
            else
                packet.WriteGuid(speed.MoverGUID.To64());
            packet.WriteUInt32(speed.Ack.MoveCounter);
            speed.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
            packet.WriteFloat(speed.Speed);
            SendPacketToServer(packet);
        }

        MovementFlagModern GetFlagForAckOpcode(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.CMSG_MOVE_FEATHER_FALL_ACK:
                    return MovementFlagModern.CanSafeFall;
                case Opcode.CMSG_MOVE_HOVER_ACK:
                    return MovementFlagModern.Hover;
                case Opcode.CMSG_MOVE_SET_CAN_FLY_ACK:
                    return MovementFlagModern.CanFly;
                case Opcode.CMSG_MOVE_WATER_WALK_ACK:
                    return MovementFlagModern.Waterwalking;
            }
            return MovementFlagModern.None;
        }

        [PacketHandler(Opcode.CMSG_MOVE_FEATHER_FALL_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_HOVER_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_CAN_FLY_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_WATER_WALK_ACK)]
        void HandleMoveForceAck1(MovementAckMessage movementAck)
        {
            WorldPacket packet = new WorldPacket(movementAck.GetUniversalOpcode());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(movementAck.MoverGUID.To64());
            else
                packet.WriteGuid(movementAck.MoverGUID.To64());
            packet.WriteUInt32(movementAck.Ack.MoveCounter);
            movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
            packet.WriteInt32(movementAck.Ack.MoveInfo.Flags.HasAnyFlag(GetFlagForAckOpcode(movementAck.GetUniversalOpcode())) ? 1 : 0);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_FORCE_ROOT_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_FORCE_UNROOT_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK)]
        [PacketHandler(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK)]
        void HandleMoveForceAck2(MovementAckMessage movementAck)
        {
            WorldPacket packet = new WorldPacket(movementAck.GetUniversalOpcode());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(movementAck.MoverGUID.To64());
            else
                packet.WriteGuid(movementAck.MoverGUID.To64());
            packet.WriteUInt32(movementAck.Ack.MoveCounter);
            movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(packet);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_ACTIVE_MOVER)]
        void HandleMoveSetActiveMover(SetActiveMover move)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
            packet.WriteGuid(move.MoverGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE)]
        void HandleMoveInitActiveMoverComplete(InitActiveMoverComplete move)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
            packet.WriteGuid(GetSession().GameState.CurrentPlayerGuid.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_SPLINE_DONE)]
        void HandleMoveSplineDone(MoveSplineDone movement)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_SPLINE_DONE);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(movement.Guid.To64());
            movement.MoveInfo.WriteMovementInfoLegacy(packet);
            packet.WriteInt32(movement.SplineID);
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteFloat(0); // Spline Type
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_TIME_SKIPPED)]
        void HandleMoveSplineDone(MoveTimeSkipped movement)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MOVE_TIME_SKIPPED);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WritePackedGuid(movement.MoverGUID.To64());
            else
                packet.WriteGuid(movement.MoverGUID.To64());
            packet.WriteUInt32(movement.TimeSkipped);
            SendPacketToServer(packet);
        }
    }
}
