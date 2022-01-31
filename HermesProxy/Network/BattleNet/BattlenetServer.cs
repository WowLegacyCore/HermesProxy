using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using HermesProxy.Framework.Logging;
using HermesProxy.Network.BattleNet.Services;
using HermesProxy.Network.BattleNet.Session;

namespace HermesProxy.Network.BattleNet
{
    public static class BattlenetServer
    {
        /// <summary>
        /// Initializes and starts the TCP listeners with the IP and Ports given.
        /// </summary>
        public static void Start(string ip)
        {
            ServiceHandler.Initialize();

            var battlenetListener = new TcpListener(IPAddress.Parse(ip), 8000);
            battlenetListener.Start();
            Log.Print(LogType.Server, $"Started Battlenet Server on {ip}:1119");

            var restListener = new TcpListener(IPAddress.Parse(ip), 8081);
            restListener.Start();
            Log.Print(LogType.Server, $"Started Rest Server on {ip}:8081");

            var cert = new X509Certificate2("bnetserver.cert.pfx");

            var battlenetThread = new Thread(async () =>
            {
                while (true)
                {
                    if (battlenetListener.Pending())
                    {
                        var bnetSession = new BattlenetSession(battlenetListener.AcceptSocket(), cert);
                        await bnetSession.HandleIncomingConnection();
                    }
            
                    Thread.Sleep(1);
                }
            });
            battlenetThread.Start();

            var restThread = new Thread(async () =>
            {
                while (true)
                {
                    if (restListener.Pending())
                    {
                        var restSession = new RestSession(restListener.AcceptSocket(), cert);
                        await restSession.HandleIncomingConnection();
                    }

                    Thread.Sleep(1);
                }
            });
            restThread.Start();
        }
    }
}
