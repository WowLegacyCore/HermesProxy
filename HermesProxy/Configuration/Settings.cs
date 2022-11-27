using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Framework.Logging;

namespace Framework
{
    public static class Settings
    {
        static readonly Configuration Conf = Configuration.LoadDefaultConfiguration();
        public static readonly byte[] ClientSeed = Conf.GetByteArray("ClientSeed", "179D3DC3235629D07113A9B3867F97A7".ParseAsByteArray());
        public static readonly ClientVersionBuild ClientBuild = Conf.GetEnum("ClientBuild", ClientVersionBuild.V2_5_2_40892);
        public static readonly ClientVersionBuild ServerBuild = Conf.GetEnum("ServerBuild", ClientVersionBuild.V2_4_3_8606);
        public static readonly string ServerAddress = Dns.GetHostAddresses(Conf.GetString("ServerAddress", "127.0.0.1")).First().ToString();
        public static readonly int ServerPort = Conf.GetInt("ServerPort", 3724);
        public static readonly string ReportedOS = Conf.GetString("ReportedOS", "OSX");
        public static readonly string ReportedPlatform = Conf.GetString("ReportedPlatform", "x86");
        public static readonly string ExternalAddress = Conf.GetString("ExternalAddress", "127.0.0.1");
        public static readonly int RestPort = Conf.GetInt("RestPort", 8081);
        public static readonly int BNetPort = Conf.GetInt("BNetPort", 1119);
        public static readonly int RealmPort = Conf.GetInt("RealmPort", 8084);
        public static readonly int InstancePort = Conf.GetInt("InstancePort", 8086);
        public static readonly bool DebugOutput = Conf.GetBoolean("DebugOutput", false);
        public static readonly bool PacketsLog = Conf.GetBoolean("PacketsLog", true);
        public static readonly bool RememberLastCharacter = Conf.GetBoolean("RememberLastCharacter", true);

        public static bool VerifyConfig()
        {
            
            if (ClientSeed.Length != 16)
            {
                Log.Print(LogType.Server, "ClientSeed must have byte length of 16 (32 characters)");
                return false;
            }
            
            if (IsValidPortNumber(RestPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({RestPort}) out of allowed range (1-65535)");
                return false;
            }

            if (IsValidPortNumber(ServerPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({BNetPort}) out of allowed range (1-65535)");
                return false;
            }

            if (IsValidPortNumber(BNetPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({BNetPort}) out of allowed range (1-65535)");
                return false;
            }

            if (IsValidPortNumber(RealmPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({RealmPort}) out of allowed range (1-65535)");
                return false;
            }

            if (IsValidPortNumber(InstancePort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({InstancePort}) out of allowed range (1-65535)");
                return false;
            }

            bool IsValidPortNumber(int someNumber)
            {
                return someNumber > IPEndPoint.MinPort && someNumber < IPEndPoint.MaxPort;
            }

            return true;
        }
    }
}
