using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class ContainerData
    {
        public WowGuid128[] Slots = new WowGuid128[36];
        public uint? NumSlots;
    }
}
