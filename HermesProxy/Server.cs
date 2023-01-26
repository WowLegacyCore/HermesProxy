using BNetServer.Networking;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.World;
using HermesProxy.World.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BNetServer;
using Framework;
using HermesProxy.Configuration;

namespace HermesProxy
{
    partial class Server
    {
        public static void ServerMain(CommandLineArguments args)
        {
#if !DEBUG
            try
            {
                if (!args.DisableVersionCheck)
                    CheckForUpdate().WaitAsync(TimeSpan.FromSeconds(15)).Wait(); // Max wait 15 sec, maybe there is some wierd network error
            }
            catch { /* ignore */ }
#endif

            Log.Print(LogType.Server, "Starting Hermes Proxy...");
            Log.Print(LogType.Server, $"Version {GetVersionInformation()}");
            Log.Start();

            if (Environment.CurrentDirectory != Path.GetDirectoryName(AppContext.BaseDirectory))
            {
                Log.Print(LogType.Storage, "Switching working directory");
                Log.Print(LogType.Storage, $"Old: {Environment.CurrentDirectory}");
                Environment.CurrentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory)!;
                Log.Print(LogType.Storage, $"New: {Environment.CurrentDirectory}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            ConfigurationParser config;
            try
            {
                config = ConfigurationParser.ParseFromFile(args.ConfigFileLocation, args.OverwrittenConfigValues);
            }
            catch (FileNotFoundException)
            {
                Log.Print(LogType.Error, "Config loading failed");
                return;
            }
            if (!Settings.LoadAndVerifyFrom(config))
            {
                Log.Print(LogType.Error, "The verification of the config failed");
                return;
            }
            Log.DebugLogEnabled = Settings.DebugOutput;
            Log.Print(LogType.Debug, "Debug logging enabled");

            if (!AesGcm.IsSupported)
            {
                Log.Print(LogType.Error, "AesGcm is not supported on your platform");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Log.Print(LogType.Error, "Since you are on MacOS, you can install openssl@3 via homebrew");
                    Log.Print(LogType.Error, "Run this:      brew install openssl@3");
                    Log.Print(LogType.Error, "Start Hermes:  DYLD_LIBRARY_PATH=/opt/homebrew/opt/openssl@3/lib ./HermesProxy");
                }
                return;
            }
            
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
