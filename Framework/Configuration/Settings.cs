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
    }
}
