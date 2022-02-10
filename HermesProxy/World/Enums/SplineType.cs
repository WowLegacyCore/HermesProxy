using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum SplineTypeLegacy
    {
        Normal       = 0,
        Stop         = 1,
        FacingSpot   = 2,
        FacingTarget = 3,
        FacingAngle  = 4
    }

    public enum SplineTypeModern
    {
        None         = 0,
        FacingSpot   = 1,
        FacingTarget = 2,
        FacingAngle  = 3
    }
}
