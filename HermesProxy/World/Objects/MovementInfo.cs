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
        public uint FlagsExtra2;
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
        public sbyte TransportSeat = -1;
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

        public void ReadMovementInfoLegacy(WorldPacket packet, GameSessionData gameState)
        {
            MovementInfo info = this;

            bool hasPitch;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                MovementFlagWotLK flags = (MovementFlagWotLK)packet.ReadUInt32();
                info.Flags = (uint)flags;
                info.FlagsExtra = packet.ReadUInt16();
                hasPitch = flags.HasAnyFlag(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying) || info.FlagsExtra.HasAnyFlag(MovementFlagExtra.AlwaysAllowPitching);
            }
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                MovementFlagTBC flags = (MovementFlagTBC)packet.ReadUInt32();
                info.Flags = (uint)flags.CastFlags<MovementFlagWotLK>();
                info.FlagsExtra = packet.ReadUInt8();
                hasPitch = flags.HasAnyFlag(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2);
            }
            else
            {
                MovementFlagVanilla flags = (MovementFlagVanilla)packet.ReadUInt32();
                info.Flags = (uint)flags.CastFlags<MovementFlagWotLK>();
                hasPitch = flags.HasAnyFlag(MovementFlagVanilla.Swimming);
                Hover = flags.HasAnyFlag(MovementFlagVanilla.FixedZ);
            }

            info.MoveTime = packet.ReadUInt32();

            info.Position = packet.ReadVector3();
            info.Orientation = packet.ReadFloat();

            if (info.Flags.HasAnyFlag(MovementFlagWotLK.OnTransport))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    info.TransportGuid = packet.ReadPackedGuid().To128(gameState);
                else
                    info.TransportGuid = packet.ReadGuid().To128(gameState);

                info.TransportOffset = packet.ReadVector4();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    info.TransportTime = packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    info.TransportSeat = packet.ReadInt8();

                if (info.FlagsExtra.HasAnyFlag(MovementFlagExtra.InterpolateMove))
                    info.TransportTime2 = packet.ReadUInt32();
            }

            if (hasPitch)
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

        public void WriteMovementInfoLegacy(WorldPacket data)
        {
            MovementInfo info = this;

            uint flags;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                flags = (uint)(((MovementFlagModern)info.Flags).CastFlags<MovementFlagWotLK>());
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                flags = (uint)(((MovementFlagModern)info.Flags).CastFlags<MovementFlagTBC>());
            else
                flags = (uint)(((MovementFlagModern)info.Flags).CastFlags<MovementFlagVanilla>());

            if (info.TransportGuid != null)
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    flags |= (uint)MovementFlagWotLK.OnTransport;
                else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    flags |= (uint)MovementFlagTBC.OnTransport;
                else
                    flags |= (uint)MovementFlagVanilla.OnTransport;
            }
            
            data.WriteUInt32(flags);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                data.WriteUInt16((ushort)info.FlagsExtra);
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                data.WriteUInt8((byte)info.FlagsExtra);

            data.WriteUInt32(info.MoveTime);
            data.WriteVector3(info.Position);
            data.WriteFloat(info.Orientation);

            bool hasTransport;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                hasTransport = flags.HasAnyFlag(MovementFlagWotLK.OnTransport);
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                hasTransport = flags.HasAnyFlag(MovementFlagTBC.OnTransport);
            else
                hasTransport = flags.HasAnyFlag(MovementFlagVanilla.OnTransport);

            if (hasTransport)
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    data.WritePackedGuid(info.TransportGuid.To64());
                else
                    data.WriteGuid(info.TransportGuid.To64());

                data.WriteVector4(info.TransportOffset);

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    data.WriteUInt32(info.TransportTime);

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    data.WriteInt8(info.TransportSeat);

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) &&
                    info.FlagsExtra.HasAnyFlag(MovementFlagExtra.InterpolateMove))
                    data.WriteUInt32(info.TransportTime2);
            }

            bool hasSwimPitch;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                hasSwimPitch = flags.HasAnyFlag(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying) || info.FlagsExtra.HasAnyFlag(MovementFlagExtra.AlwaysAllowPitching);
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                hasSwimPitch = flags.HasAnyFlag(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2);
            else
                hasSwimPitch = flags.HasAnyFlag(MovementFlagVanilla.Swimming);

            if (hasSwimPitch)
                data.WriteFloat(info.SwimPitch);

            data.WriteUInt32(info.FallTime);

            bool hasFallDirection;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                hasFallDirection = flags.HasAnyFlag(MovementFlagWotLK.Falling);
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                hasFallDirection = flags.HasAnyFlag(MovementFlagTBC.Falling);
            else
                hasFallDirection = flags.HasAnyFlag(MovementFlagVanilla.Falling);

            if (hasFallDirection)
            {
                data.WriteFloat(info.JumpVerticalSpeed);
                data.WriteFloat(info.JumpSinAngle);
                data.WriteFloat(info.JumpCosAngle);
                data.WriteFloat(info.JumpHorizontalSpeed);
            }

            bool hasSplineElevation;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                hasSplineElevation = flags.HasAnyFlag(MovementFlagWotLK.SplineElevation);
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                hasSplineElevation = flags.HasAnyFlag(MovementFlagTBC.SplineElevation);
            else
                hasSplineElevation = flags.HasAnyFlag(MovementFlagVanilla.SplineElevation);

            if (hasSplineElevation)
                data.WriteFloat(info.SplineElevation);
        }

        public void ReadMovementInfoModern(WorldPacket data)
        {
            var moveInfo = this;

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                moveInfo.Flags = data.ReadUInt32();
                moveInfo.FlagsExtra = data.ReadUInt32();
                moveInfo.FlagsExtra2 = data.ReadUInt32();
            }

            moveInfo.MoveTime = data.ReadUInt32();
            moveInfo.Position = data.ReadVector3();
            moveInfo.Orientation = data.ReadFloat();

            moveInfo.SwimPitch = data.ReadFloat();
            moveInfo.SplineElevation = data.ReadFloat();

            uint removeMovementForcesCount = data.ReadUInt32();

            uint moveIndex = data.ReadUInt32();

            for (uint i = 0; i < removeMovementForcesCount; ++i)
            {
                data.ReadPackedGuid128();
            }

            // ResetBitReader

            if (!ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                moveInfo.Flags = data.ReadBits<uint>(30);
                moveInfo.FlagsExtra = data.ReadBits<uint>(18);
            }

            bool hasTransport = data.HasBit();
            bool hasFall = data.HasBit();
            bool hasSpline = data.HasBit(); // todo 6.x read this infos

            data.ReadBit(); // HeightChangeFailed
            data.ReadBit(); // RemoteTimeValid
            bool hasInertia = ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3) ? data.HasBit() : false;

            if (hasTransport)
                ReadTransportInfoModern(data);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                if (hasInertia)
                {
                    data.ReadPackedGuid128();
                    data.ReadVector3(); // Force
                    data.ReadUInt32(); // Lifetime
                }
            }

            if (hasFall)
            {
                moveInfo.FallTime = data.ReadUInt32();
                moveInfo.JumpVerticalSpeed = data.ReadFloat();

                // ResetBitReader

                bool hasFallDirection = data.HasBit();
                if (hasFallDirection)
                {
                    moveInfo.JumpSinAngle = data.ReadFloat();
                    moveInfo.JumpCosAngle = data.ReadFloat();
                    moveInfo.JumpHorizontalSpeed = data.ReadFloat();
                }
            }
        }

        public void ReadTransportInfoModern(WorldPacket data)
        {
            var moveInfo = this;
            moveInfo.TransportGuid = data.ReadPackedGuid128();
            moveInfo.TransportOffset = data.ReadVector4();
            moveInfo.TransportSeat = data.ReadInt8();           // VehicleSeatIndex
            moveInfo.TransportTime = data.ReadUInt32();         // MoveTime

            bool hasPrevTime = data.HasBit();
            bool hasVehicleId = data.HasBit();

            if (hasPrevTime)
                moveInfo.TransportTime2 = data.ReadUInt32();    // PrevMoveTime

            if (hasVehicleId)
                moveInfo.VehicleId = data.ReadUInt32();         // VehicleRecID
        }

        public void WriteMovementInfoModern(WorldPacket data, WowGuid128 guid)
        {
            MovementInfo moveInfo = this;
            bool hasFallDirection = moveInfo.Flags.HasAnyFlag(MovementFlagModern.Falling | MovementFlagModern.FallingFar);
            bool hasFall = hasFallDirection || moveInfo.FallTime != 0;

            data.WritePackedGuid128(guid);                                  // MoverGUID

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                data.WriteUInt32(Flags);
                data.WriteUInt32(FlagsExtra);
                data.WriteUInt32(FlagsExtra2);
            }

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

            if (!ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                data.WriteBits(moveInfo.Flags, 30);
                data.WriteBits(moveInfo.FlagsExtra, 18);
            }
                
            data.WriteBit(moveInfo.TransportGuid != null);                 // HasTransport
            data.WriteBit(hasFall);                                        // HasFall
            data.WriteBit(HasSplineData);                                  // HasSpline - marks that the unit uses spline movement
            data.WriteBit(false);                                          // HeightChangeFailed
            data.WriteBit(false);                                          // RemoteTimeValid
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
                data.WriteBit(false);                                      // HasInertia
            data.FlushBits();

            if (moveInfo.TransportGuid != null)
                WriteTransportInfoModern(data);

            /*
            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                if (Inertia != null)
                {
                    data.WritePackedGuid128(Inertia.Guid);
                    data.WriteVector3(Inertia.Force);
                    data.WriteUInt32(Inertia.Lifetime);
                }
            }
            */

            if (hasFall)
            {
                data.WriteUInt32(moveInfo.FallTime);                              // Time
                data.WriteFloat(moveInfo.JumpVerticalSpeed);                      // JumpVelocity
                data.WriteBit(hasFallDirection);
                data.FlushBits();

                if (hasFallDirection)
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

            data.WritePackedGuid128(moveInfo.TransportGuid);
            data.WriteFloat(moveInfo.TransportOffset.X);
            data.WriteFloat(moveInfo.TransportOffset.Y);
            data.WriteFloat(moveInfo.TransportOffset.Z);
            data.WriteFloat(moveInfo.TransportOffset.W);
            data.WriteInt8(moveInfo.TransportSeat);
            data.WriteUInt32(moveInfo.TransportTime);

            data.WriteBit(hasPrevTime);
            data.WriteBit(hasVehicleId);
            data.FlushBits();

            if (hasPrevTime)
                data.WriteUInt32(0); // PrevMoveTime

            if (hasVehicleId)
                data.WriteUInt32(moveInfo.VehicleId);
        }
    }
}
