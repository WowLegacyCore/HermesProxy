using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.Configuration;

namespace Framework
{
    public static class Settings
    {
        public static byte[] ClientSeed;
        public static ClientVersionBuild ClientBuild;
        public static ClientVersionBuild ServerBuild;
        public static string ServerAddress;
        public static int ServerPort;
        public static string ReportedOS;
        public static string ReportedPlatform;
        public static string ExternalAddress;
        public static int RestPort;
        public static int BNetPort;
        public static int RealmPort;
        public static int InstancePort;
        public static bool DebugOutput;
        public static bool PacketsLog;
        public static int ServerSpellDelay;
        public static int ClientSpellDelay;

        public static bool LoadAndVerifyFrom(ConfigurationParser config)
        {
            ClientSeed = config.GetByteArray("ClientSeed", "179D3DC3235629D07113A9B3867F97A7".ParseAsByteArray());
            ClientBuild = config.GetEnum("ClientBuild", ClientVersionBuild.V2_5_2_40892);
            ServerBuild = config.GetEnum("ServerBuild", ClientVersionBuild.V2_4_3_8606);
            ServerAddress = config.GetString("ServerAddress", "127.0.0.1");
            ServerPort = config.GetInt("ServerPort", 3724);
            ReportedOS = config.GetString("ReportedOS", "OSX");
            ReportedPlatform = config.GetString("ReportedPlatform", "x86");
            ExternalAddress = config.GetString("ExternalAddress", "127.0.0.1");
            RestPort = config.GetInt("RestPort", 8081);
            BNetPort = config.GetInt("BNetPort", 1119);
            RealmPort = config.GetInt("RealmPort", 8084);
            InstancePort = config.GetInt("InstancePort", 8086);
            DebugOutput = config.GetBoolean("DebugOutput", false);
            PacketsLog = config.GetBoolean("PacketsLog", true);
            ServerSpellDelay = config.GetInt("ServerSpellDelay", 0);
            ClientSpellDelay = config.GetInt("ClientSpellDelay", 0);

            return VerifyConfig();
        }
        
        private static bool VerifyConfig()
        {
            
            if (ClientSeed.Length != 16)
            {
                Log.Print(LogType.Server, "ClientSeed must have byte length of 16 (32 characters)");
                return false;
            }
            
            if (!IsValidPortNumber(RestPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({RestPort}) out of allowed range (1-65535)");
                return false;
            }

            if (!IsValidPortNumber(ServerPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({BNetPort}) out of allowed range (1-65535)");
                return false;
            }

            if (!IsValidPortNumber(BNetPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({BNetPort}) out of allowed range (1-65535)");
                return false;
            }

            if (!IsValidPortNumber(RealmPort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({RealmPort}) out of allowed range (1-65535)");
                return false;
            }

            if (!IsValidPortNumber(InstancePort))
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({InstancePort}) out of allowed range (1-65535)");
                return false;
            }

            if (ServerSpellDelay < 0)
            {
                Log.Print(LogType.Server, "ServerSpellDelay must be larger than or equal to 0");
                return false;
            }

            if (ClientSpellDelay < 0)
            {
                Log.Print(LogType.Server, "ClientSpellDelay must be larger than or equal to 0");
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
