using Framework.GameMath;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public sealed class MovementInfo
    {
        public const float DEFAULT_WALK_SPEED = 2.5f;
        public const float DEFAULT_RUN_SPEED = 7.0f;
        public const float DEFAULT_RUN_BACK_SPEED = 4.5f;
        public const float DEFAULT_SWIM_SPEED = 4.72222f;
        public const float DEFAULT_SWIM_BACK_SPEED = 2.5f;
        public const float DEFAULT_FLY_SPEED = 7.0f;
        public const float DEFAULT_FLY_BACK_SPEED = 4.5f;
        public const float DEFAULT_TURN_RATE = 3.141593f;
        public const float DEFAULT_PITCH_RATE = 3.141593f;

        public uint Flags;
        public uint FlagsExtra;
        public uint MoveTime;
        public float SwimPitch;
        public uint FallTime;
        public float JumpHorizontalSpeed;
        public float JumpVerticalSpeed;
        public float JumpCosAngle;
        public float JumpSinAngle;
        public float SplineElevation;
        public bool HasSplineData;
        public Vector3 Position;
        public float Orientation;
        public float CorpseOrientation;
        public WowGuid128 TransportGuid;
        public Vector4 TransportOffset;
        public uint TransportTime;
        public uint TransportTime2;
        public sbyte TransportSeat;
        public Quaternion Rotation;
        public float WalkSpeed;
        public float RunSpeed;
        public float RunBackSpeed;
        public float SwimSpeed;
        public float SwimBackSpeed;
        public float FlightSpeed;
        public float FlightBackSpeed;
        public float TurnRate;
        public float PitchRate;
        public bool Hover;
        public float VehicleOrientation;
        public uint VehicleId; // Not exactly related to movement but it is read in ReadMovementUpdateBlock
        public uint TransportPathTimer; // only set for transports

        public MovementInfo CopyFromMe()
        {
            MovementInfo copy = new MovementInfo();
            copy.Flags = this.Flags;
            copy.FlagsExtra = this.FlagsExtra;
            copy.SwimPitch = this.SwimPitch;
            copy.FallTime = this.FallTime;
            copy.JumpHorizontalSpeed = this.JumpHorizontalSpeed;
            copy.JumpVerticalSpeed = this.JumpVerticalSpeed;
            copy.JumpCosAngle = this.JumpCosAngle;
            copy.JumpSinAngle = this.JumpSinAngle;
            copy.SplineElevation = this.SplineElevation;
            copy.HasSplineData = this.HasSplineData;
            copy.Position = this.Position;
            copy.Orientation = this.Orientation;
            copy.CorpseOrientation = this.CorpseOrientation;
            copy.TransportGuid = this.TransportGuid;
            copy.TransportOffset = this.TransportOffset;
            copy.TransportTime = this.TransportTime;
            copy.TransportTime2 = this.TransportTime2;
            copy.TransportSeat = this.TransportSeat;
            copy.Rotation = this.Rotation;
            copy.WalkSpeed = this.WalkSpeed;
            copy.RunSpeed = this.RunSpeed;
            copy.RunBackSpeed = this.RunBackSpeed;
            copy.SwimSpeed = this.SwimSpeed;
            copy.SwimBackSpeed = this.SwimBackSpeed;
            copy.FlightSpeed = this.FlightSpeed;
            copy.FlightBackSpeed = this.FlightBackSpeed;
            copy.TurnRate = this.TurnRate;
            copy.PitchRate = this.PitchRate;
            copy.Hover = this.Hover;
            copy.VehicleId = this.VehicleId;
            copy.VehicleOrientation = this.VehicleOrientation;
            copy.TransportPathTimer = this.TransportPathTimer;
            return copy;
        }

        public void ReadMovementInfoLegacy(WorldPacket packet)
        {
            MovementInfo info = this;

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
        }

        public void WriteMovementInfoModern(WorldPacket data, WowGuid128 guid)
        {
            MovementInfo moveInfo = this;
            bool hasFallDirection = moveInfo.Flags.HasAnyFlag(MovementFlagModern.Falling | MovementFlagModern.FallingFar);
            bool hasFall = hasFallDirection || moveInfo.FallTime != 0;

            data.WritePackedGuid128(guid);                                  // MoverGUID

            data.WriteUInt32(moveInfo.MoveTime);                            // MoveTime
            data.WriteFloat(moveInfo.Position.X);
            data.WriteFloat(moveInfo.Position.Y);
            data.WriteFloat(moveInfo.Position.Z);
            data.WriteFloat(moveInfo.Orientation);

            data.WriteFloat(moveInfo.SwimPitch);                            // Pitch
            data.WriteFloat(moveInfo.SplineElevation);                      // StepUpStartElevation

            data.WriteUInt32(0);                                            // RemoveForcesIDs.size()
            data.WriteUInt32(0);                                            // MoveIndex

            //for (public uint i = 0; i < RemoveForcesIDs.Count; ++i)
            //    *data << ObjectGuid(RemoveForcesIDs);

            data.WriteBits(moveInfo.Flags, 30);
            data.WriteBits(moveInfo.FlagsExtra, 18);
            data.WriteBit(moveInfo.TransportGuid != null);                 // HasTransport
            data.WriteBit(hasFall);                                        // HasFall
            data.WriteBit(HasSplineData);                                  // HasSpline - marks that the unit uses spline movement
            data.WriteBit(false);                                          // HeightChangeFailed
            data.WriteBit(false);                                          // RemoteTimeValid

            if (moveInfo.TransportGuid != null)
                WriteTransportInfoModern(data);

            if (hasFall)
            {
                data.WriteUInt32(moveInfo.FallTime);                              // Time
                data.WriteFloat(moveInfo.JumpVerticalSpeed);                      // JumpVelocity

                if (data.WriteBit(hasFallDirection))
                {
                    data.WriteFloat(moveInfo.JumpSinAngle);                       // Direction
                    data.WriteFloat(moveInfo.JumpCosAngle);
                    data.WriteFloat(moveInfo.JumpHorizontalSpeed);                // Speed
                }
            }
        }
        public void WriteTransportInfoModern(WorldPacket data)
        {
            MovementInfo moveInfo = this;
            bool hasPrevTime = false;
            bool hasVehicleId = moveInfo.VehicleId != 0;

            data.WritePackedGuid128(moveInfo.TransportGuid.To128()); // Transport Guid
            data.WriteFloat(moveInfo.TransportOffset.X);
            data.WriteFloat(moveInfo.TransportOffset.Y);
            data.WriteFloat(moveInfo.TransportOffset.Z);
            data.WriteFloat(moveInfo.TransportOffset.W);
            data.WriteInt8(moveInfo.TransportSeat);                  // VehicleSeatIndex
            data.WriteUInt32(moveInfo.TransportTime);                // MoveTime

            data.WriteBit(hasPrevTime);
            data.WriteBit(hasVehicleId);
            data.FlushBits();

            if (hasPrevTime)
                data.WriteUInt32(0);                                 // PrevMoveTime

            if (hasVehicleId)
                data.WriteUInt32(moveInfo.VehicleId);                // VehicleRecID
        }
    }
}
