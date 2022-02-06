using Framework;
using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy
{
    public static class LegacyVersion
    {
        static LegacyVersion()
        {
            Build = Settings.ServerBuild;
        }
        public static ClientVersionBuild Build { get; private set; }

        public static int BuildInt => (int)Build;

        public static string VersionString => Build.ToString();

        public static bool InVersion(ClientVersionBuild build1, ClientVersionBuild build2)
        {
            return AddedInVersion(build1) && RemovedInVersion(build2);
        }

        public static bool AddedInVersion(ClientVersionBuild build)
        {
            return Build >= build;
        }

        public static bool RemovedInVersion(ClientVersionBuild build)
        {
            return Build < build;
        }
    }
}
