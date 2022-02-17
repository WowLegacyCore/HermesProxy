using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class DynamicObjectData
    {
        public WowGuid128 Caster;
        public uint? Type;
        public int? SpellXSpellVisualID;
        public int? SpellID;
        public float? Radius;
        public uint? CastTime;
    }
}
