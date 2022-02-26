/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class ClientPlayerMovement : ClientPacket
    {
        public ClientPlayerMovement(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128(); ;
            MoveInfo = new MovementInfo();
            MoveInfo.ReadMovementInfoModern(_worldPacket);
        }

        public WowGuid128 Guid;
        public MovementInfo MoveInfo;
    }
    public class MoveUpdate : ServerPacket
    {
        public MoveUpdate() : base(Opcode.SMSG_MOVE_UPDATE, ConnectionType.Instance) { }

        public override void Write()
        {
            MoveInfo.WriteMovementInfoModern(_worldPacket, MoverGUID);
        }

        public WowGuid128 MoverGUID;
        public MovementInfo MoveInfo;
    }

    public class MonsterMove : ServerPacket
    {
        public MonsterMove(WowGuid128 guid, ServerSideMovement moveSpline) : base(Opcode.SMSG_ON_MONSTER_MOVE, ConnectionType.Instance)
        {
            if (moveSpline.SplineFlags.HasFlag(SplineFlagModern.UncompressedPath))
            {
                if (!moveSpline.SplineFlags.HasFlag(SplineFlagModern.Cyclic))
                {
                    foreach (var point in moveSpline.SplinePoints)
                        Points.Add(point);

                    if (moveSpline.EndPosition != Vector3.Zero)
                        Points.Add(moveSpline.EndPosition);
                }
                else
                {
                    if (moveSpline.EndPosition != Vector3.Zero)
                        Points.Add(moveSpline.EndPosition);

                    foreach (var point in moveSpline.SplinePoints)
                        Points.Add(point);
                }
            }
            else if (moveSpline.EndPosition != Vector3.Zero)
            {
                Points.Add(moveSpline.EndPosition);

                if (moveSpline.SplinePoints.Count > 0)
                {
                    Vector3 middle = (moveSpline.StartPosition + moveSpline.EndPosition) / 2.0f;

                    // first and last points already appended
                    for (int i = 0; i < moveSpline.SplinePoints.Count; ++i)
                        PackedDeltas.Add(middle - moveSpline.SplinePoints[i]);
                }
            }
            MoverGUID = guid;
            MoveSpline = moveSpline;
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
            _worldPacket.WriteVector3(MoveSpline.StartPosition);

            _worldPacket.WriteUInt32(MoveSpline.SplineId);
            _worldPacket.WriteVector3(Vector3.Zero); // Destination
            _worldPacket.WriteBit(false); // CrzTeleport
            _worldPacket.WriteBits(Points.Count == 0 ? 2 : 0, 3); // StopDistanceTolerance

            _worldPacket.WriteUInt32((uint)MoveSpline.SplineFlags);
            _worldPacket.WriteInt32(0); // Elapsed
            _worldPacket.WriteUInt32(MoveSpline.SplineTimeFull);
            _worldPacket.WriteUInt32(0); // FadeObjectTime
            _worldPacket.WriteUInt8(MoveSpline.SplineMode);
            _worldPacket.WritePackedGuid128(MoveSpline.TransportGuid != null ? MoveSpline.TransportGuid : WowGuid128.Empty);
            _worldPacket.WriteInt8(MoveSpline.TransportSeat);
            _worldPacket.WriteBits((byte)MoveSpline.SplineType, 2);
            _worldPacket.WriteBits(Points.Count, 16);
            _worldPacket.WriteBit(false); // VehicleExitVoluntary ;
            _worldPacket.WriteBit(false); // Interpolate
            _worldPacket.WriteBits(PackedDeltas.Count, 16);
            _worldPacket.WriteBit(false); // SplineFilter.HasValue
            _worldPacket.WriteBit(false); // SpellEffectExtraData.HasValue
            _worldPacket.WriteBit(false); // JumpExtraData.HasValue
            _worldPacket.FlushBits();

            //if (SplineFilter.HasValue)
            //    SplineFilter.Value.Write(data);

            switch (MoveSpline.SplineType)
            {
                case SplineTypeModern.FacingSpot:
                    _worldPacket.WriteVector3(MoveSpline.FinalFacingSpot);
                    break;
                case SplineTypeModern.FacingTarget:
                    _worldPacket.WriteFloat(MoveSpline.FinalOrientation);
                    _worldPacket.WritePackedGuid128(MoveSpline.FinalFacingGuid);
                    break;
                case SplineTypeModern.FacingAngle:
                    _worldPacket.WriteFloat(MoveSpline.FinalOrientation);
                    break;
            }

            foreach (Vector3 pos in Points)
                _worldPacket.WriteVector3(pos);

            foreach (Vector3 pos in PackedDeltas)
                _worldPacket.WritePackXYZ(pos);

            /*
            if (SpellEffectExtraData.HasValue)
                SpellEffectExtraData.Value.Write(data);

            if (JumpExtraData.HasValue)
                JumpExtraData.Value.Write(data);
            */
        }

        public WowGuid128 MoverGUID;
        public ServerSideMovement MoveSpline;
        public List<Vector3> Points = new();
        public List<Vector3> PackedDeltas = new();
    }

    class MoveTeleportAck : ClientPacket
    {
        public MoveTeleportAck(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            MoverGUID = _worldPacket.ReadPackedGuid128();
            MoveCounter = _worldPacket.ReadUInt32();
            MoveTime = _worldPacket.ReadUInt32();
        }

        public WowGuid128 MoverGUID;
        public uint MoveCounter;
        public uint MoveTime;
    }

    public class MoveTeleport : ServerPacket
    {
        public MoveTeleport() : base(Opcode.SMSG_MOVE_TELEPORT, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
            _worldPacket.WriteUInt32(MoveCounter);
            _worldPacket.WriteVector3(Position);
            _worldPacket.WriteFloat(Orientation);
            _worldPacket.WriteUInt8(PreloadWorld);

            _worldPacket.WriteBit(TransportGUID != null);
            _worldPacket.WriteBit(Vehicle != null);
            _worldPacket.FlushBits();

            if (Vehicle != null)
            {
                _worldPacket.WriteInt8(Vehicle.VehicleSeatIndex);
                _worldPacket.WriteBit(Vehicle.VehicleExitVoluntary);
                _worldPacket.WriteBit(Vehicle.VehicleExitTeleport);
                _worldPacket.FlushBits();
            }

            if (TransportGUID != null)
                _worldPacket.WritePackedGuid128(TransportGUID);
        }

        public Vector3 Position;
        public VehicleTeleport Vehicle;
        public uint MoveCounter;
        public WowGuid128 MoverGUID;
        public WowGuid128 TransportGUID;
        public float Orientation;
        public byte PreloadWorld;
    }

    public class VehicleTeleport
    {
        public sbyte VehicleSeatIndex;
        public bool VehicleExitVoluntary;
        public bool VehicleExitTeleport;
    }

    public class TransferPending : ServerPacket
    {
        public TransferPending() : base(Opcode.SMSG_TRANSFER_PENDING) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(MapID);
            _worldPacket.WriteVector3(OldMapPosition);
            _worldPacket.WriteBit(Ship != null);
            _worldPacket.WriteBit(TransferSpellID.HasValue);

            if (Ship != null)
            {
                _worldPacket.WriteUInt32(Ship.Id);
                _worldPacket.WriteInt32(Ship.OriginMapID);
            }

            if (TransferSpellID.HasValue)
                _worldPacket.WriteInt32(TransferSpellID.Value);

            _worldPacket.FlushBits();
        }

        public int MapID = -1;
        public Vector3 OldMapPosition;
        public ShipTransferPending Ship;
        public int? TransferSpellID;

        public class ShipTransferPending
        {
            public uint Id;              // gameobject_template.entry of the transport the player is teleporting on
            public int OriginMapID;     // Map id the player is currently on (before teleport)
        }
    }

    public class TransferAborted : ServerPacket
    {
        public TransferAborted() : base(Opcode.SMSG_TRANSFER_ABORTED) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteUInt8(Arg);
            _worldPacket.WriteUInt32(MapDifficultyXConditionID);
            _worldPacket.WriteBits(Reason, 5);
            _worldPacket.FlushBits();
        }

        public uint MapID;
        public byte Arg;
        public uint MapDifficultyXConditionID;
        public TransferAbortReason Reason;
    }

    public class NewWorld : ServerPacket
    {
        public NewWorld() : base(Opcode.SMSG_NEW_WORLD) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteVector3(Position);
            _worldPacket.WriteFloat(Orientation);
            _worldPacket.WriteUInt32(Reason);
            _worldPacket.WriteVector3(MovementOffset);
        }

        public uint MapID;
        public uint Reason;
        public Vector3 Position = new();
        public float Orientation;
        public Vector3 MovementOffset;    // Adjusts all pending movement events by this offset
    }

    public class WorldPortResponse : ClientPacket
    {
        public WorldPortResponse(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    // for server controlled units
    public class MoveSplineSetSpeed : ServerPacket
    {
        public MoveSplineSetSpeed(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
            _worldPacket.WriteFloat(Speed);
        }

        public WowGuid128 MoverGUID;
        public float Speed = 1.0f;
    }

    // for own player
    public class MoveSetSpeed : ServerPacket
    {
        public MoveSetSpeed(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
            _worldPacket.WriteUInt32(MoveCounter);
            _worldPacket.WriteFloat(Speed);
        }

        public WowGuid128 MoverGUID;
        public uint MoveCounter = 0;
        public float Speed = 1.0f;
    }

    public class MovementSpeedAck : ClientPacket
    {
        public MovementSpeedAck(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            MoverGUID = _worldPacket.ReadPackedGuid128();
            Ack.Read(_worldPacket);
            Speed = _worldPacket.ReadFloat();
        }

        public WowGuid128 MoverGUID;
        public MovementAck Ack;
        public float Speed;
    }

    public struct MovementAck
    {
        public void Read(WorldPacket data)
        {
            MoveInfo = new();
            MoveInfo.ReadMovementInfoModern(data);
            MoveCounter = data.ReadUInt32();
        }

        public MovementInfo MoveInfo;
        public uint MoveCounter;
    }

    // for other players
    public class MoveUpdateSpeed : ServerPacket
    {
        public MoveUpdateSpeed(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            MoveInfo.WriteMovementInfoModern(_worldPacket, MoverGUID);
            _worldPacket.WriteFloat(Speed);
        }

        public WowGuid128 MoverGUID;
        public MovementInfo MoveInfo;
        public float Speed = 1.0f;
    }

    public class MoveSplineSetFlag : ServerPacket
    {
        public MoveSplineSetFlag(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
        }

        public WowGuid128 MoverGUID;
    }

    public class MoveSetFlag : ServerPacket
    {
        public MoveSetFlag(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
            _worldPacket.WriteUInt32(MoveCounter);
        }

        public WowGuid128 MoverGUID;
        public uint MoveCounter = 0;
    }

    public class MovementAckMessage : ClientPacket
    {
        public MovementAckMessage(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            MoverGUID = _worldPacket.ReadPackedGuid128();
            Ack.Read(_worldPacket);
        }

        public WowGuid128 MoverGUID;
        public MovementAck Ack;
    }

    class MoveKnockBack : ServerPacket
    {
        public MoveKnockBack() : base(Opcode.SMSG_MOVE_KNOCK_BACK, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(MoverGUID);
            _worldPacket.WriteUInt32(MoveCounter);
            _worldPacket.WriteVector2(Direction);
            _worldPacket.WriteFloat(HorizontalSpeed);
            _worldPacket.WriteFloat(VerticalSpeed);
        }

        public WowGuid128 MoverGUID;
        public uint MoveCounter;
        public Vector2 Direction;
        public float HorizontalSpeed;
        public float VerticalSpeed;
    }

    public class MoveUpdateKnockBack : ServerPacket
    {
        public MoveUpdateKnockBack() : base(Opcode.SMSG_MOVE_UPDATE_KNOCK_BACK) { }

        public override void Write()
        {
            MoveInfo.WriteMovementInfoModern(_worldPacket, MoverGUID);
        }

        public WowGuid128 MoverGUID;
        public MovementInfo MoveInfo;
    }

    class SuspendToken : ServerPacket
    {
        public SuspendToken() : base(Opcode.SMSG_SUSPEND_TOKEN, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SequenceIndex);
            _worldPacket.WriteBits(Reason, 2);
            _worldPacket.FlushBits();
        }

        public uint SequenceIndex = 1;
        public uint Reason = 1;
    }

    class ResumeToken : ServerPacket
    {
        public ResumeToken() : base(Opcode.SMSG_RESUME_TOKEN, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SequenceIndex);
            _worldPacket.WriteBits(Reason, 2);
            _worldPacket.FlushBits();
        }

        public uint SequenceIndex = 1;
        public uint Reason = 1;
    }
}
