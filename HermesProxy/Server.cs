// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using BNetServer.Networking;
using Framework.Cryptography;
using Framework.Logging;
using Framework.Networking;
using System;
using System.Globalization;
using System.Timers;

namespace HermesProxy
{
    class Server
    {
        static void Main()
        {
            //Set Culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Console.WriteLine("Hello from Hermes Proxy!");

            string bindIp = "0.0.0.0";

            var restSocketServer = new SocketManager<RestSession>();
            int restPort = 8081;
            if (restPort < 0 || restPort > 0xFFFF)
            {
                Log.Print(LogType.Network, $"Specified login service port ({restPort}) out of allowed range (1-65535), defaulting to 8081");
                restPort = 8081;
            }

            Console.WriteLine("Starting REST service...");
            if (!restSocketServer.StartNetwork(bindIp, restPort))
            {
                Log.Print(LogType.Server, "Failed to initialize Rest Socket Server");
                ExitNow();
            }

            Console.WriteLine("Starting Realm manager...");
            Global.RealmMgr.Initialize();

            Console.WriteLine("Starting Login service...");
            Global.LoginServiceMgr.Initialize();

            var sessionSocketServer = new SocketManager<Session>();
            // Start the listening port (acceptor) for auth connections
            int bnPort = 1119;
            if (bnPort < 0 || bnPort > 0xFFFF)
            {
                Log.Print(LogType.Server, $"Specified battle.net port ({bnPort}) out of allowed range (1-65535)");
                ExitNow();
            }

            Console.WriteLine($"Listening on {bindIp}:{bnPort}...");
            if (!sessionSocketServer.StartNetwork(bindIp, bnPort))
            {
                Log.Print(LogType.Network, "Failed to start BnetServer Network");
                ExitNow();
            }
        }

        static void ExitNow()
        {
            Console.WriteLine("Halting process...");
            System.Threading.Thread.Sleep(10000);
            Environment.Exit(-1);
        }
    }
}