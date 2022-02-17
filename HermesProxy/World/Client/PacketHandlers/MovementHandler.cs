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
        private static MovementInfo ReadMovementInfo(WorldPacket packet, WowGuid guid)
        {
            var info = new MovementInfo();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                info.Flags = packet.ReadUInt32();
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                info.Flags = (uint)(((MovementFlagTBC)packet.ReadUInt32()).CastFlags<MovementFlagWotLK>());
            else
                info.Flags = (uint)(((MovementFlagVanilla)packet.ReadUInt32()).CastFlags<MovementFlagWotLK>());

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                info.FlagsExtra = packet.ReadUInt16();
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                info.FlagsExtra = packet.ReadUInt8();

            info.MoveTime = packet.ReadUInt32();

            info.Position = packet.ReadVector3();
            info.Orientation = packet.ReadFloat();

            if (info.Flags.HasAnyFlag(MovementFlagWotLK.OnTransport))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    info.TransportGuid = packet.ReadPackedGuid().To128();
                else
                    info.TransportGuid = packet.ReadGuid().To128();

                info.TransportOffset = packet.ReadVector4();
                info.TransportTime = packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    info.TransportSeat = packet.ReadInt8();

                if (info.FlagsExtra.HasAnyFlag(MovementFlagExtra.InterpolateMove))
                    info.TransportTime2 = packet.ReadUInt32();
            }

            if (info.Flags.HasAnyFlag(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying) ||
                info.FlagsExtra.HasAnyFlag(MovementFlagExtra.AlwaysAllowPitching))
                info.SwimPitch = packet.ReadFloat();

            info.FallTime = packet.ReadUInt32();
            if (info.Flags.HasAnyFlag(MovementFlagWotLK.Falling))
            {
                info.JumpVerticalSpeed = packet.ReadFloat();
                info.JumpSinAngle = packet.ReadFloat();
                info.JumpCosAngle = packet.ReadFloat();
                info.JumpHorizontalSpeed = packet.ReadFloat();
            }

            if (info.Flags.HasAnyFlag(MovementFlagWotLK.SplineElevation))
                info.SplineElevation = packet.ReadFloat();

            return info;
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
