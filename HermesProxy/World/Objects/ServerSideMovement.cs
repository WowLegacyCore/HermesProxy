using Framework.GameMath;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public sealed class ServerSideMovement
    {
        public SplineTypeModern SplineType;
        public uint SplineTime;
        public uint SplineTimeFull;
        public uint SplineId;
        public byte SplineMode;
        public SplineFlagModern SplineFlags;
        public uint SplineCount;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public float FinalOrientation;
        public Vector3 FinalFacingSpot;
        public WowGuid128 FinalFacingGuid;
        public WowGuid128 TransportGuid;
        public sbyte TransportSeat;
        public List<Vector3> SplinePoints = new();
    }
}
