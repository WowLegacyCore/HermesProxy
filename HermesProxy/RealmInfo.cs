using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy
{
    public class RealmInfo
    {
        public ushort ID;
        public byte Type;
        public byte IsLocked;
        public Enums.RealmFlags Flags;
        public string Name;
        public string Address;
        public int Port;
        public float Population;
        public byte CharacterCount;
        public byte Timezone;
        public byte VersionMajor;
        public byte VersionMinor;
        public byte VersonBugfix;
        public ushort Build;

        public override string ToString()
        {
            return String.Format("{0,-5} {1,-5} {2,-8} {3,-10} {4,-15} {5,-15} {6,-10} {7,-10}", ID, Type, IsLocked, Flags, Name, Address, Port, Build);
        }
    }
}
