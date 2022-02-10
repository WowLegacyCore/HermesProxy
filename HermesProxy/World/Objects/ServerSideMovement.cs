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
        public float StartPositionX;
        public float StartPositionY;
        public float StartPositionZ;
        public Vector3 EndPosition;
        public float FinalOrientation;
        public Vector3 FinalFacingSpot;
        public WowGuid FinalFacingGuid;
        public WowGuid TransportGuid = WowGuid64.Empty;
        public uint TransportId;
        public string TransportType = "";
        public sbyte TransportSeat;
        public List<Vector3> SplinePoints = null;
    }
}
