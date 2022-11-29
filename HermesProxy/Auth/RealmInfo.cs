using Framework.Constants;

namespace HermesProxy.Auth
{
    public class RealmInfo
    {
        public uint ID;
        public RealmType Type;
        public byte IsLocked;
        public RealmFlags Flags;
        public string Name;
        public string Address;
        public ushort Port;
        public float Population;
        public byte CharacterCount;
        public byte Timezone;
        public byte VersionMajor;
        public byte VersionMinor;
        public byte VersonBugfix;
        public ushort Build;

        public override string ToString()
        {
            return $"{ID,-5} {Type,-5} {IsLocked,-8} {Flags,-10} {Name,-15} {Address,-15} {Port,-10} {Build,-10}";
        }
    }
}
