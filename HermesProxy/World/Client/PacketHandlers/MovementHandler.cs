using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
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

    }
}
