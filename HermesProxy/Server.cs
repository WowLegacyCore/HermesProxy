using BNetServer.Networking;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.World;
using HermesProxy.World.Server;
using System;
using System.Globalization;
using BNetServer;

namespace HermesProxy
{
    partial class Server
    {
        static void Main()
        {
            //Set Culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Log.Print(LogType.Server, "Starting Hermes Proxy...");
            Log.Print(LogType.Server, $"Version {GetVersionInformation()}");
            Log.Start();
            
            GameData.LoadEverything();

            string bindIp = Framework.Settings.ExternalAddress;

            var restSocketServer = new SocketManager<BnetRestApiSession>();
            int restPort = Framework.Settings.RestPort;
            if (restPort < 0 || restPort > 0xFFFF)
            {
                Log.Print(LogType.Network, $"Specified login service port ({restPort}) out of allowed range (1-65535), defaulting to 8081");
                restPort = 8081;
            }

            Log.Print(LogType.Server, "Starting REST service...");
            if (!restSocketServer.StartNetwork(bindIp, restPort))
            {
                Log.Print(LogType.Server, "Failed to initialize Rest Socket Server");
                ExitNow();
            }

            Log.Print(LogType.Server, "Starting Login service...");
            LoginServiceManager.Instance.Initialize();

            var sessionSocketServer = new SocketManager<BnetTcpSession>();
            // Start the listening port (acceptor) for auth connections
            int bnPort = Framework.Settings.BNetPort;
            if (bnPort < 0 || bnPort > 0xFFFF)
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({bnPort}) out of allowed range (1-65535)");
                ExitNow();
            }

            Log.Print(LogType.Server, $"BNet Listening on {bindIp}:{bnPort}...");
            if (!sessionSocketServer.StartNetwork(bindIp, bnPort))
            {
                Log.Print(LogType.Network, "Failed to start BNet socket manager");
                ExitNow();
            }

            // Launch the worldserver listener socket
            int worldPort = Framework.Settings.RealmPort;
            if (worldPort < 0 || worldPort > 0xFFFF)
            {
                Log.Print(LogType.Server, $"Specified Realm port ({worldPort}) out of allowed range (1-65535)");
                ExitNow();
            }

            int instancePort = Framework.Settings.InstancePort;
            if (instancePort < 0 || instancePort > 0xFFFF)
            {
                Log.Print(LogType.Server, $"Specified Instance port ({instancePort}) out of allowed range (1-65535)");
                ExitNow();
            }

            if (worldPort == instancePort)
            {
                Log.Print(LogType.Server, $"Realm and Instance sockets cannot use the same port");
                ExitNow();
            }

            Log.Print(LogType.Server, $"World Listening on {bindIp}:{worldPort}...");
            var worldSocketMgr = new WorldSocketManager();
            if (!worldSocketMgr.StartNetwork(bindIp, worldPort))
            {
                Log.Print(LogType.Network, "Failed to start World socket manager");
                ExitNow();
            }
        }

        static void ExitNow()
        {
            Console.WriteLine("Halting process...");
            System.Threading.Thread.Sleep(10_000);
            Environment.Exit(-1);
        }

        private static readonly string? _buildTag;
        private static string GetVersionInformation()
        {
            string version = $"{GitVersionInformation.CommitDate} {_buildTag}{GitVersionInformation.MajorMinorPatch}";
            if (GitVersionInformation.CommitsSinceVersionSource != "0")
                version += $"+{GitVersionInformation.CommitsSinceVersionSource}({GitVersionInformation.ShortSha})";
            if (GitVersionInformation.UncommittedChanges != "0")
                version += " dirty";
            return version;
        }
    }
}
