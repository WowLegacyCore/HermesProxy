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
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
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
}
