using BNetServer.Networking;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.World;
using HermesProxy.World.Server;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using BNetServer;
using Framework;

namespace HermesProxy
{
    partial class Server
    {
        static void Main()
        {
            //Set Culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Log.Print(LogType.Server, "Starting Hermes Proxy...");
            Log.Print(LogType.Server, $"Version {GetVersionInformation()}");
            Log.Start();

            if (!Settings.VerifyConfig())
            {
                Console.WriteLine("The verification of the config failed");
                Environment.Exit(1);
            }
            Log.DebugLogEnabled = Settings.DebugOutput;

            GameData.LoadEverything();

            var bindIp = NetworkUtils.ResolveOrDirectIp(Settings.ExternalAddress);
            if (!IPAddress.IsLoopback(bindIp))
                bindIp = IPAddress.Any; // If we are not listening on localhost we have to expose our services

            // LoginServiceManager holds our external IPs so that other player can connect to our Hermes instance
            LoginServiceManager.Instance.Initialize();

            // 1. Start the listener for binary bnet RPC service connections
            var bnetSocketServer = StartServer<BnetTcpSession>(new IPEndPoint(bindIp, Settings.BNetPort));

            // 2. Start the listener for http(s) bnet RPC service like auth/"realm" connections
            var restSocketServer = StartServer<BnetRestApiSession>(new IPEndPoint(bindIp, Settings.RestPort));

            // 3. Start the listener for realm connections
            var realmSocketServer = StartServer<RealmSocket>(new IPEndPoint(bindIp, Settings.RealmPort));

            // 4. Start the listener for world connections
            var worldSocketServer = StartServer<WorldSocket>(new IPEndPoint(bindIp, Settings.InstancePort));

            while (restSocketServer.IsListening || bnetSocketServer.IsListening || realmSocketServer.IsListening || worldSocketServer.IsListening)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

            Console.WriteLine($"(restSocketServer.IsListening: {restSocketServer.IsListening}");
            Console.WriteLine($"(bnetSocketServer.IsListening: {bnetSocketServer.IsListening}");
            Console.WriteLine($"(realmSocketServer.IsListening: {realmSocketServer.IsListening}");
            Console.WriteLine($"(worldSocketServer.IsListening: {worldSocketServer.IsListening}");

            ExitNow();
        }

        private static SocketManager<TSocketType> StartServer<TSocketType>(IPEndPoint bindIp) where TSocketType : ISocket
        {
            var socketManager = new SocketManager<TSocketType>();

            Log.Print(LogType.Server, $"Starting {typeof(TSocketType).Name} service on {bindIp}...");
            if (!socketManager.StartNetwork(bindIp.Address.ToString(), bindIp.Port))
            {
                throw new Exception($"Failed to start {typeof(TSocketType).Name} service");
            }

            Thread.Sleep(50); // Lets wait until the thread has been logged

            return socketManager;
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
