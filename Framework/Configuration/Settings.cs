using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public static class Settings
    {
        static readonly Configuration Conf = new Configuration();
        public static readonly string ClientSeed = Conf.GetString("ClientSeed", "179D3DC3235629D07113A9B3867F97A7");
        public static readonly ClientVersionBuild ClientBuild = Conf.GetEnum("ClientBuild", ClientVersionBuild.V2_5_2_40892);
        public static readonly ClientVersionBuild ServerBuild = Conf.GetEnum("ServerBuild", ClientVersionBuild.V2_4_3_8606);
        public static readonly string ServerAddress = Conf.GetString("ServerAddress", "127.0.0.1");
        public static readonly int RestPort = Conf.GetInt("RestPort", 8081);
        public static readonly int BNetPort = Conf.GetInt("BNetPort", 1119);
        public static readonly int RealmPort = Conf.GetInt("RealmPort", 8085);
        public static readonly int InstancePort = Conf.GetInt("InstancePort", 8086);
        public static readonly bool DebugOutput = Conf.GetBoolean("DebutOutput", false);
    }
}
