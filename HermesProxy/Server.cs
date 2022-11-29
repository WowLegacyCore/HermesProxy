using BNetServer.Networking;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.World;
using HermesProxy.World.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BNetServer;
using Framework;

namespace HermesProxy
{
    partial class Server
    {
        static void Main(string[] args)
        {
            //Set Culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

#if !DEBUG
            if (!args.Any(a => a.Trim().Contains("--no-version-check")))
                CheckForUpdate().Wait();
#endif

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
            Thread.Sleep(10_000);
            Environment.Exit(-1);
        }

        private static async Task CheckForUpdate()
        {
            const string hermesGitHubRepo = "WowLegacyCore/HermesProxy";

            try
            {
                if (GitVersionInformation.CommitsSinceVersionSource != "0" || GitVersionInformation.UncommittedChanges != "0")
                    return; // we are probably in a test branch

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("User-Agent", "curl/7.0.0"); // otherwise we get blocked
                var response = await client.GetAsync($"https://api.github.com/repos/{hermesGitHubRepo}/releases/latest");
                response.EnsureSuccessStatusCode();

                string rawJson = await response.Content.ReadAsStringAsync();
                var parsedJson = JsonSerializer.Deserialize<Dictionary<string, object>>(rawJson);

                string commitDateStr = parsedJson!["created_at"].ToString();
                DateTime commitDate = DateTime.Parse(commitDateStr!, CultureInfo.InvariantCulture).ToUniversalTime();;

                string myCommitDateStr = GitVersionInformation.CommitDate;
                DateTime myCommitDate = DateTime.Parse(myCommitDateStr, CultureInfo.InvariantCulture).ToUniversalTime();;

                if (commitDate > myCommitDate)
                {
                    Console.WriteLine("------------------------");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"HermesProxy update available v{GitVersionInformation.Major}.{GitVersionInformation.Minor} => {parsedJson!["tag_name"]} ({commitDate:yyyy-MM-dd})");
                    Console.WriteLine("Please download new version from https://github.com/WowLegacyCore/HermesProxy/releases/latest");
                    Console.ResetColor();
                    Console.WriteLine("------------------------");
                    Console.WriteLine();
                    Thread.Sleep(10_000);
                }
            }
            catch
            {
                // ignore
            }
        }

        private static readonly string? _buildTag;
        private static string GetVersionInformation()
        {
            var commitDate = DateTime.Parse(GitVersionInformation.CommitDate, CultureInfo.InvariantCulture).ToUniversalTime();

            string version = $"{commitDate:yyyy-MM-dd} {_buildTag}{GitVersionInformation.MajorMinorPatch}";
            if (GitVersionInformation.CommitsSinceVersionSource != "0")
                version += $"+{GitVersionInformation.CommitsSinceVersionSource}({GitVersionInformation.ShortSha})";
            if (GitVersionInformation.UncommittedChanges != "0")
                version += " dirty";
            return version;
        }
    }
}
