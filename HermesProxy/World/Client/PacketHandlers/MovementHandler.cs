using Framework.GameMath;
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
        [PacketHandler(Opcode.MSG_MOVE_START_FORWARD)]
        [PacketHandler(Opcode.MSG_MOVE_START_BACKWARD)]
        [PacketHandler(Opcode.MSG_MOVE_STOP)]
        [PacketHandler(Opcode.MSG_MOVE_START_STRAFE_LEFT)]
        [PacketHandler(Opcode.MSG_MOVE_START_STRAFE_RIGHT)]
        [PacketHandler(Opcode.MSG_MOVE_STOP_STRAFE)]
        [PacketHandler(Opcode.MSG_MOVE_START_ASCEND)]
        [PacketHandler(Opcode.MSG_MOVE_START_DESCEND)]
        [PacketHandler(Opcode.MSG_MOVE_STOP_ASCEND)]
        [PacketHandler(Opcode.MSG_MOVE_JUMP)]
        [PacketHandler(Opcode.MSG_MOVE_START_TURN_LEFT)]
        [PacketHandler(Opcode.MSG_MOVE_START_TURN_RIGHT)]
        [PacketHandler(Opcode.MSG_MOVE_STOP_TURN)]
        [PacketHandler(Opcode.MSG_MOVE_START_PITCH_UP)]
        [PacketHandler(Opcode.MSG_MOVE_START_PITCH_DOWN)]
        [PacketHandler(Opcode.MSG_MOVE_STOP_PITCH)]
        [PacketHandler(Opcode.MSG_MOVE_SET_RUN_MODE)]
        [PacketHandler(Opcode.MSG_MOVE_SET_WALK_MODE)]
        [PacketHandler(Opcode.MSG_MOVE_TELEPORT)]
        [PacketHandler(Opcode.MSG_MOVE_SET_FACING)]
        [PacketHandler(Opcode.MSG_MOVE_SET_PITCH)]
        [PacketHandler(Opcode.MSG_MOVE_TOGGLE_COLLISION_CHEAT)]
        [PacketHandler(Opcode.MSG_MOVE_GRAVITY_CHNG)]
        [PacketHandler(Opcode.MSG_MOVE_ROOT)]
        [PacketHandler(Opcode.MSG_MOVE_UNROOT)]
        [PacketHandler(Opcode.MSG_MOVE_START_SWIM)]
        [PacketHandler(Opcode.MSG_MOVE_STOP_SWIM)]
        [PacketHandler(Opcode.MSG_MOVE_START_SWIM_CHEAT)]
        [PacketHandler(Opcode.MSG_MOVE_STOP_SWIM_CHEAT)]
        [PacketHandler(Opcode.MSG_MOVE_HEARTBEAT)]
        [PacketHandler(Opcode.MSG_MOVE_FALL_LAND)]
        [PacketHandler(Opcode.MSG_MOVE_UPDATE_CAN_FLY)]
        [PacketHandler(Opcode.MSG_MOVE_UPDATE_CAN_TRANSITION_BETWEEN_SWIM_AND_FLY)]
        [PacketHandler(Opcode.MSG_MOVE_HOVER)]
        [PacketHandler(Opcode.MSG_MOVE_FEATHER_FALL)]
        [PacketHandler(Opcode.MSG_MOVE_WATER_WALK)]
        [PacketHandler(Opcode.CMSG_MOVE_FALL_RESET)]
        [PacketHandler(Opcode.CMSG_MOVE_SET_FLY)]
        [PacketHandler(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
        [PacketHandler(Opcode.CMSG_MOVE_NOT_ACTIVE_MOVER)]
        [PacketHandler(Opcode.CMSG_DISMISS_CONTROLLED_VEHICLE)]
        void HandleMovementMessages(WorldPacket packet)
        {
            MoveUpdate moveUpdate = new MoveUpdate();
            moveUpdate.Guid = packet.ReadPackedGuid().To128();
            moveUpdate.MoveInfo = new();
            moveUpdate.MoveInfo.ReadMovementInfoLegacy(packet);
            moveUpdate.MoveInfo.Flags = (uint)(((MovementFlagWotLK)moveUpdate.MoveInfo.Flags).CastFlags<MovementFlagModern>());
            SendPacketToClient(moveUpdate);
        }

        [PacketHandler(Opcode.SMSG_COMPRESSED_MOVES)]
        void HandleCompressedMoves(WorldPacket packet)
        {
            var uncompressedSize = packet.ReadInt32();

            WorldPacket pkt = packet.Inflate(uncompressedSize);

            while (pkt.CanRead())
            {
                var size = pkt.ReadUInt8();
                var opc = pkt.ReadUInt16();
                var data = pkt.ReadBytes((uint)(size - 2));

                var pkt2 = new WorldPacket(opc, data);
                HandleMonsterMove(pkt2);
            }
        }

        [PacketHandler(Opcode.SMSG_ON_MONSTER_MOVE)]
        void HandleMonsterMove(WorldPacket packet)
        {
            WowGuid128 guid = packet.ReadPackedGuid().To128();
            ServerSideMovement moveSpline = new();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767)) // no idea when this was added exactly
                packet.ReadBool(); // "Toggle AnimTierInTrans"

            moveSpline.StartPosition = packet.ReadVector3();

            moveSpline.SplineId = packet.ReadUInt32();

            SplineTypeLegacy type = (SplineTypeLegacy)packet.ReadUInt8();

            switch (type)
            {
                case SplineTypeLegacy.FacingSpot:
                {
                    moveSpline.SplineType = SplineTypeModern.FacingSpot;
                    moveSpline.FinalFacingSpot = packet.ReadVector3();
                    break;
                }
                case SplineTypeLegacy.FacingTarget:
                {
                    moveSpline.SplineType = SplineTypeModern.FacingTarget;
                    moveSpline.FinalFacingGuid = packet.ReadGuid().To128();
                    break;
                }
                case SplineTypeLegacy.FacingAngle:
                {
                    moveSpline.SplineType = SplineTypeModern.FacingAngle;
                    moveSpline.FinalOrientation = packet.ReadFloat();
                    break;
                }
                case SplineTypeLegacy.Stop:
                {
                    moveSpline.SplineType = SplineTypeModern.None;
                    MonsterMove moveStop = new MonsterMove(guid, moveSpline);
                    SendPacketToClient(moveStop);
                    return;
                }
            }

            bool hasAnimTier;
            bool hasTrajectory;
            bool hasCatmullRom;
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                var splineFlags = (SplineFlagVanilla)packet.ReadUInt32();
                hasAnimTier = false;
                hasTrajectory = false;
                hasCatmullRom = splineFlags.HasAnyFlag(SplineFlagVanilla.Flying);
                moveSpline.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
            }
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                var splineFlags = (SplineFlagTBC)packet.ReadUInt32();
                hasAnimTier = false;
                hasTrajectory = false;
                hasCatmullRom = splineFlags.HasAnyFlag(SplineFlagTBC.Flying);
                moveSpline.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
            }
            else
            {
                var splineFlags = (SplineFlagWotLK)packet.ReadUInt32();
                hasAnimTier = splineFlags.HasAnyFlag(SplineFlagWotLK.AnimationTier);
                hasTrajectory = splineFlags.HasAnyFlag(SplineFlagWotLK.Trajectory);
                hasCatmullRom = splineFlags.HasAnyFlag(SplineFlagWotLK.Flying | SplineFlagWotLK.CatmullRom);
                moveSpline.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
            }

            if (hasAnimTier)
            {
                packet.ReadUInt8(); // Animation State
                packet.ReadInt32(); // Async-time in ms
            }

            moveSpline.SplineTimeFull = packet.ReadUInt32();

            if (hasTrajectory)
            {
                packet.ReadFloat(); // Vertical Speed
                packet.ReadInt32(); // Async-time in ms
            }

            moveSpline.SplineCount = packet.ReadUInt32();

            if (hasCatmullRom)
            {
                for (var i = 0; i < moveSpline.SplineCount; i++)
                {
                    Vector3 vec = packet.ReadVector3();
                    if (moveSpline != null)
                        moveSpline.SplinePoints.Add(vec);
                }
            }
            else
            {
                moveSpline.EndPosition = packet.ReadVector3();

                Vector3 mid = (moveSpline.StartPosition + moveSpline.EndPosition) * 0.5f;

                for (var i = 1; i < moveSpline.SplineCount; i++)
                {
                    var vec = packet.ReadPackedVector3();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        vec = mid - vec;
                    else
                        vec = moveSpline.EndPosition - vec;

                    moveSpline.SplinePoints.Add(vec);
                }
            }

            MonsterMove monsterMove = new MonsterMove(guid, moveSpline);
            SendPacketToClient(monsterMove);
        }
    }
}
