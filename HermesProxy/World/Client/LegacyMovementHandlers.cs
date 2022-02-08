using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using World;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        public static MovementFlag ConvertVanillaMovementFlags(MovementFlagVanilla flags)
        {
            MovementFlag newFlags = MovementFlag.None;

            if (flags.HasAnyFlag(MovementFlagVanilla.Forward))
                newFlags |= MovementFlag.Forward;
            if (flags.HasAnyFlag(MovementFlagVanilla.Backward))
                newFlags |= MovementFlag.Backward;
            if (flags.HasAnyFlag(MovementFlagVanilla.StrafeLeft))
                newFlags |= MovementFlag.StrafeLeft;
            if (flags.HasAnyFlag(MovementFlagVanilla.StrafeRight))
                newFlags |= MovementFlag.StrafeRight;
            if (flags.HasAnyFlag(MovementFlagVanilla.TurnLeft))
                newFlags |= MovementFlag.TurnLeft;
            if (flags.HasAnyFlag(MovementFlagVanilla.TurnRight))
                newFlags |= MovementFlag.TurnRight;
            if (flags.HasAnyFlag(MovementFlagVanilla.PitchUp))
                newFlags |= MovementFlag.PitchUp;
            if (flags.HasAnyFlag(MovementFlagVanilla.PitchDown))
                newFlags |= MovementFlag.PitchDown;
            if (flags.HasAnyFlag(MovementFlagVanilla.WalkMode))
                newFlags |= MovementFlag.WalkMode;
            if (flags.HasAnyFlag(MovementFlagVanilla.OnTransport))
                newFlags |= MovementFlag.OnTransport;
            if (flags.HasAnyFlag(MovementFlagVanilla.Levitating))
                newFlags |= MovementFlag.DisableGravity;
            if (flags.HasAnyFlag(MovementFlagVanilla.Root))
                newFlags |= MovementFlag.Root;
            if (flags.HasAnyFlag(MovementFlagVanilla.Falling))
                newFlags |= MovementFlag.Falling;
            if (flags.HasAnyFlag(MovementFlagVanilla.FallingFar))
                newFlags |= MovementFlag.FallingFar;
            if (flags.HasAnyFlag(MovementFlagVanilla.Swimming))
                newFlags |= MovementFlag.Swimming;
            if (flags.HasAnyFlag(MovementFlagVanilla.SplineEnabled))
                newFlags |= MovementFlag.SplineEnabled;
            if (flags.HasAnyFlag(MovementFlagVanilla.CanFly))
                newFlags |= MovementFlag.CanFly;
            if (flags.HasAnyFlag(MovementFlagVanilla.Flying))
                newFlags |= MovementFlag.Flying;
            if (flags.HasAnyFlag(MovementFlagVanilla.SplineElevation))
                newFlags |= MovementFlag.SplineElevation;
            if (flags.HasAnyFlag(MovementFlagVanilla.Waterwalking))
                newFlags |= MovementFlag.Waterwalking;
            if (flags.HasAnyFlag(MovementFlagVanilla.CanSafeFall))
                newFlags |= MovementFlag.CanSafeFall;
            if (flags.HasAnyFlag(MovementFlagVanilla.Hover))
                newFlags |= MovementFlag.Hover;

            return newFlags;
        }

        public static MovementFlag ConvertTBCMovementFlags(MovementFlagTBC flags)
        {
            MovementFlag newFlags = MovementFlag.None;

            if (flags.HasAnyFlag(MovementFlagTBC.Forward))
                newFlags |= MovementFlag.Forward;
            if (flags.HasAnyFlag(MovementFlagTBC.Backward))
                newFlags |= MovementFlag.Backward;
            if (flags.HasAnyFlag(MovementFlagTBC.StrafeLeft))
                newFlags |= MovementFlag.StrafeLeft;
            if (flags.HasAnyFlag(MovementFlagTBC.StrafeRight))
                newFlags |= MovementFlag.StrafeRight;
            if (flags.HasAnyFlag(MovementFlagTBC.TurnLeft))
                newFlags |= MovementFlag.TurnLeft;
            if (flags.HasAnyFlag(MovementFlagTBC.TurnRight))
                newFlags |= MovementFlag.TurnRight;
            if (flags.HasAnyFlag(MovementFlagTBC.PitchUp))
                newFlags |= MovementFlag.PitchUp;
            if (flags.HasAnyFlag(MovementFlagTBC.PitchDown))
                newFlags |= MovementFlag.PitchDown;
            if (flags.HasAnyFlag(MovementFlagTBC.WalkMode))
                newFlags |= MovementFlag.WalkMode;
            if (flags.HasAnyFlag(MovementFlagTBC.OnTransport))
                newFlags |= MovementFlag.OnTransport;
            if (flags.HasAnyFlag(MovementFlagTBC.DisableGravity))
                newFlags |= MovementFlag.DisableGravity;
            if (flags.HasAnyFlag(MovementFlagTBC.Root))
                newFlags |= MovementFlag.Root;
            if (flags.HasAnyFlag(MovementFlagTBC.Falling))
                newFlags |= MovementFlag.Falling;
            if (flags.HasAnyFlag(MovementFlagTBC.FallingFar))
                newFlags |= MovementFlag.FallingFar;
            if (flags.HasAnyFlag(MovementFlagTBC.Swimming))
                newFlags |= MovementFlag.Swimming;
            if (flags.HasAnyFlag(MovementFlagTBC.SplineEnabled))
                newFlags |= MovementFlag.SplineEnabled;
            if (flags.HasAnyFlag(MovementFlagTBC.CanFly))
                newFlags |= MovementFlag.CanFly;
            if (flags.HasAnyFlag(MovementFlagTBC.Flying) || flags.HasAnyFlag(MovementFlagTBC.Flying2))
                newFlags |= MovementFlag.Flying;
            if (flags.HasAnyFlag(MovementFlagTBC.SplineElevation))
                newFlags |= MovementFlag.SplineElevation;
            if (flags.HasAnyFlag(MovementFlagTBC.Waterwalking))
                newFlags |= MovementFlag.Waterwalking;
            if (flags.HasAnyFlag(MovementFlagTBC.CanSafeFall))
                newFlags |= MovementFlag.CanSafeFall;
            if (flags.HasAnyFlag(MovementFlagTBC.Hover))
                newFlags |= MovementFlag.Hover;

            return newFlags;
        }
        private static MovementInfo ReadMovementInfo(WorldPacket packet, WowGuid guid)
        {
            var info = new MovementInfo();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                info.Flags = packet.ReadUInt32();
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                info.Flags = (uint)ConvertTBCMovementFlags((MovementFlagTBC)packet.ReadUInt32());
            else
                info.Flags = (uint)ConvertVanillaMovementFlags((MovementFlagVanilla)packet.ReadUInt32());

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                info.FlagsExtra = packet.ReadUInt16();
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                info.FlagsExtra = packet.ReadUInt8();

            info.MoveTime = packet.ReadUInt32();

            info.Position = packet.ReadVector3();
            info.Orientation = packet.ReadFloat();

            if (info.Flags.HasAnyFlag(MovementFlag.OnTransport))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    info.TransportGuid = packet.ReadPackedGuid();
                else
                    info.TransportGuid = packet.ReadGuid();

                info.TransportOffset = packet.ReadVector4();
                info.TransportTime = packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    info.TransportSeat = packet.ReadInt8();

                if (info.FlagsExtra.HasAnyFlag(MovementFlagExtra.InterpolateMove))
                    info.TransportTime2 = packet.ReadUInt32();
            }

            if (info.Flags.HasAnyFlag(MovementFlag.Swimming | MovementFlag.Flying) ||
                info.FlagsExtra.HasAnyFlag(MovementFlagExtra.AlwaysAllowPitching))
                info.SwimPitch = packet.ReadFloat();

            info.FallTime = packet.ReadUInt32();
            if (info.Flags.HasAnyFlag(MovementFlag.Falling))
            {
                info.JumpVerticalSpeed = packet.ReadFloat();
                info.JumpSinAngle = packet.ReadFloat();
                info.JumpCosAngle = packet.ReadFloat();
                info.JumpHorizontalSpeed = packet.ReadFloat();
            }

            if (info.Flags.HasAnyFlag(MovementFlag.SplineElevation))
                info.SplineElevation = packet.ReadFloat();

            return info;
        }

    }
}
