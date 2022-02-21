using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

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
    }
}
