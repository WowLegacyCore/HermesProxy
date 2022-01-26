using System.Threading;

using HermesProxy.Framework.Logging;
using HermesProxy.Network.Auth;
using HermesProxy.Network.BattleNet;

namespace HermesProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Start();

            Log.Print(LogType.Server, $"ClientVersion: {Settings.ClientBuild}");
            Log.Print(LogType.Server, $"ServerVersion: {Settings.ServerBuild}");

            // Disabled until we actually need it.
            //GameData.LoadEverything();

            var authThread = new Thread(() =>
            {
                var authClient = new AuthClient();
                authClient.ConnectToAuthServer();
            });
            authThread.Start();

            // BattlenetServer.Start(Settings.ServerAddress);

            while (true)
            {
                Thread.Sleep(1);
            }
        }
    }
}
