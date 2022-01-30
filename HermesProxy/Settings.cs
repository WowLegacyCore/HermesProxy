using HermesProxy.Enums;

namespace HermesProxy
{
    public static class Settings
    {
        static readonly Configuration Conf = new();

        public static readonly ClientVersionBuild ClientBuild = Conf.GetEnum("ClientBuild", ClientVersionBuild.V2_5_2_40892);
        public static readonly ClientVersionBuild ServerBuild = Conf.GetEnum("ServerBuild", ClientVersionBuild.V2_4_3_8606);
        public static byte GetServerExpansionVersion()
        {
            string str = ServerBuild.ToString();
            str = str.Replace("V", "");
            str = str[..str.IndexOf("_")];
            return (byte)uint.Parse(str);
        }
        public static byte GetServerMajorPatchVersion()
        {
            string str = ServerBuild.ToString();
            str = str[(str.IndexOf('_') + 1)..];
            str = str[..str.IndexOf("_")];
            return (byte)uint.Parse(str);
        }
        public static byte GetServerMinorPatchVersion()
        {
            string str = ServerBuild.ToString();
            str = str[(str.IndexOf('_') + 1)..];
            str = str[(str.IndexOf('_') + 1)..];
            str = str[..str.IndexOf("_")];
            return (byte)uint.Parse(str);
        }
        public static readonly string ServerAddress = Conf.GetString("ServerAddress", "127.0.0.1");
    }
}
