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
        void HandleMovementMessages(WorldPacket packet)
        {
            MoveUpdate moveUpdate = new MoveUpdate();
            moveUpdate.MoverGUID = packet.ReadPackedGuid().To128();
            moveUpdate.MoveInfo = new();
            moveUpdate.MoveInfo.ReadMovementInfoLegacy(packet);
            moveUpdate.MoveInfo.Flags = (uint)(((MovementFlagWotLK)moveUpdate.MoveInfo.Flags).CastFlags<MovementFlagModern>());
            SendPacketToClient(moveUpdate);
        }

        [PacketHandler(Opcode.MSG_MOVE_KNOCK_BACK)]
        void HandleMoveKnockBack(WorldPacket packet)
        {
            MoveUpdateKnockBack knockback = new MoveUpdateKnockBack();
            knockback.MoverGUID = packet.ReadPackedGuid().To128();
            knockback.MoveInfo = new();
            knockback.MoveInfo.ReadMovementInfoLegacy(packet);
            knockback.MoveInfo.Flags = (uint)(((MovementFlagWotLK)knockback.MoveInfo.Flags).CastFlags<MovementFlagModern>());
            knockback.MoveInfo.JumpSinAngle = packet.ReadFloat();
            knockback.MoveInfo.JumpCosAngle = packet.ReadFloat();
            knockback.MoveInfo.JumpHorizontalSpeed = packet.ReadFloat();
            knockback.MoveInfo.JumpVerticalSpeed = packet.ReadFloat();
            SendPacketToClient(knockback);
        }

        [PacketHandler(Opcode.SMSG_MOVE_KNOCK_BACK)]
        void HandleMoveForceKnockBack(WorldPacket packet)
        {
            MoveKnockBack knockback = new MoveKnockBack();
            knockback.MoverGUID = packet.ReadPackedGuid().To128();
            knockback.MoveCounter = packet.ReadUInt32();
            knockback.Direction = packet.ReadVector2();
            knockback.HorizontalSpeed = packet.ReadFloat();
            knockback.VerticalSpeed = packet.ReadFloat();
            SendPacketToClient(knockback);
        }

        [PacketHandler(Opcode.MSG_MOVE_TELEPORT_ACK)]
        void HandleMoveTeleportAck(WorldPacket packet)
        {
            MoveTeleport teleport = new MoveTeleport();
            teleport.MoverGUID = packet.ReadPackedGuid().To128();
            teleport.MoveCounter = packet.ReadUInt32();
            MovementInfo moveInfo = new();
            moveInfo.ReadMovementInfoLegacy(packet);
            teleport.Position = moveInfo.Position;
            teleport.Orientation = moveInfo.Orientation;
            teleport.TransportGUID = moveInfo.TransportGuid;
            if (moveInfo.TransportSeat > 0)
            {
                teleport.Vehicle = new();
                teleport.Vehicle.VehicleSeatIndex = moveInfo.TransportSeat;
            }
            SendPacketToClient(teleport);
        }

        [PacketHandler(Opcode.SMSG_TRANSFER_PENDING)]
        void HandleTransferPending(WorldPacket packet)
        {
            TransferPending transfer = new TransferPending();
            transfer.MapID = packet.ReadInt32();
            transfer.OldMapPosition = Vector3.Zero;
            SendPacketToClient(transfer);
        }

        [PacketHandler(Opcode.SMSG_TRANSFER_ABORTED)]
        void HandleTransferAborted(WorldPacket packet)
        {
            TransferAborted transfer = new TransferAborted();
            transfer.MapID = packet.ReadUInt32();
            transfer.Reason = (TransferAbortReason)packet.ReadUInt8();
            transfer.Arg = packet.ReadUInt8();
            SendPacketToClient(transfer);
        }

        [PacketHandler(Opcode.SMSG_NEW_WORLD)]
        void HandleNewWorld(WorldPacket packet)
        {
            NewWorld teleport = new NewWorld();
            Global.CurrentSessionData.GameState.CurrentMapId = teleport.MapID = packet.ReadUInt32();
            teleport.Position = packet.ReadVector3();
            teleport.Orientation = packet.ReadFloat();
            teleport.Reason = 4;
            SendPacketToClient(teleport);
        }

        // for server controlled units
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_PITCH_RATE)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_RUN_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_TURN_RATE)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WALK_BACK_SPEED)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WALK_SPEED)]
        void HandleMoveSplineSetSpeed(WorldPacket packet)
        {
            MoveSplineSetSpeed speed = new MoveSplineSetSpeed(packet.GetUniversalOpcode(false));
            speed.MoverGUID = packet.ReadPackedGuid().To128();
            speed.Speed = packet.ReadFloat();
            SendPacketToClient(speed);
        }

        // for own player
        [PacketHandler(Opcode.SMSG_FORCE_WALK_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_RUN_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_RUN_BACK_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_SWIM_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_TURN_RATE_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_FLIGHT_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_FLIGHT_BACK_SPEED_CHANGE)]
        [PacketHandler(Opcode.SMSG_FORCE_PITCH_RATE_CHANGE)]
        void HandleMoveForceSpeedChange(WorldPacket packet)
        {
            string opcodeName = packet.GetUniversalOpcode(false).ToString().Replace("SMSG_FORCE_", "SMSG_MOVE_SET_").Replace("_CHANGE", "");
            Opcode universalOpcode = Opcodes.GetUniversalOpcode(opcodeName);

            MoveSetSpeed speed = new MoveSetSpeed(universalOpcode);
            speed.MoverGUID = packet.ReadPackedGuid().To128();
            speed.MoveCounter = packet.ReadUInt32();
            speed.Speed = packet.ReadFloat();
            SendPacketToClient(speed);
        }

        // for other players
        [PacketHandler(Opcode.MSG_MOVE_SET_FLIGHT_BACK_SPEED)]
        [PacketHandler(Opcode.MSG_MOVE_SET_FLIGHT_SPEED)]
        [PacketHandler(Opcode.MSG_MOVE_SET_PITCH_RATE)]
        [PacketHandler(Opcode.MSG_MOVE_SET_RUN_BACK_SPEED)]
        [PacketHandler(Opcode.MSG_MOVE_SET_RUN_SPEED)]
        [PacketHandler(Opcode.MSG_MOVE_SET_SWIM_BACK_SPEED)]
        [PacketHandler(Opcode.MSG_MOVE_SET_SWIM_SPEED)]
        [PacketHandler(Opcode.MSG_MOVE_SET_TURN_RATE)]
        [PacketHandler(Opcode.MSG_MOVE_SET_WALK_SPEED)]
        void HandleMoveUpdateSpeed(WorldPacket packet)
        {
            string opcodeName = packet.GetUniversalOpcode(false).ToString().Replace("MSG_MOVE_SET", "SMSG_MOVE_UPDATE");
            Opcode universalOpcode = Opcodes.GetUniversalOpcode(opcodeName);

            MoveUpdateSpeed speed = new MoveUpdateSpeed(universalOpcode);
            speed.MoverGUID = packet.ReadPackedGuid().To128();
            speed.MoveInfo = new MovementInfo();
            speed.MoveInfo.ReadMovementInfoLegacy(packet);
            speed.MoveInfo.Flags = (uint)(((MovementFlagWotLK)speed.MoveInfo.Flags).CastFlags<MovementFlagModern>());
            speed.Speed = packet.ReadFloat();
            SendPacketToClient(speed);
        }

        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_ROOT)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_UNROOT)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_ENABLE_GRAVITY)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_DISABLE_GRAVITY)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FEATHER_FALL)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_NORMAL_FALL)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_HOVER)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_UNSET_HOVER)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WATER_WALK)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_LAND_WALK)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_START_SWIM)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_STOP_SWIM)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_RUN_MODE)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WALK_MODE)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FLYING)]
        [PacketHandler(Opcode.SMSG_MOVE_SPLINE_UNSET_FLYING)]
        void HandleSplineMovementMessages(WorldPacket packet)
        {
            MoveSplineSetFlag spline = new MoveSplineSetFlag(packet.GetUniversalOpcode(false));
            spline.MoverGUID = packet.ReadPackedGuid().To128();
            SendPacketToClient(spline);
        }

        [PacketHandler(Opcode.SMSG_MOVE_FORCE_ROOT)]
        [PacketHandler(Opcode.SMSG_MOVE_FORCE_UNROOT)]
        [PacketHandler(Opcode.SMSG_MOVE_SET_WATER_WALK)]
        [PacketHandler(Opcode.SMSG_MOVE_SET_LAND_WALK)]
        [PacketHandler(Opcode.SMSG_MOVE_SET_HOVERING)]
        [PacketHandler(Opcode.SMSG_MOVE_UNSET_HOVERING)]
        [PacketHandler(Opcode.SMSG_MOVE_SET_CAN_FLY)]
        [PacketHandler(Opcode.SMSG_MOVE_UNSET_CAN_FLY)]
        [PacketHandler(Opcode.SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
        [PacketHandler(Opcode.SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
        [PacketHandler(Opcode.SMSG_MOVE_DISABLE_GRAVITY)]
        [PacketHandler(Opcode.SMSG_MOVE_ENABLE_GRAVITY)]
        [PacketHandler(Opcode.SMSG_MOVE_SET_FEATHER_FALL)]
        [PacketHandler(Opcode.SMSG_MOVE_SET_NORMAL_FALL)]
        void HandleMoveForceFlagChange(WorldPacket packet)
        {
            MoveSetFlag flag = new MoveSetFlag(packet.GetUniversalOpcode(false));
            flag.MoverGUID = packet.ReadPackedGuid().To128();
            flag.MoveCounter = packet.ReadUInt32();
            SendPacketToClient(flag);
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

                if (size < 21)
                    return;

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
