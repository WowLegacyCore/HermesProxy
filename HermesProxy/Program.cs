using System.Threading;

using HermesProxy.Framework.Logging;
using HermesProxy.Network.BattleNet;

namespace HermesProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the Logger
            Log.Start();

            Log.Print(LogType.Debug, $"Client Version: {Settings.ClientBuild}");
            Log.Print(LogType.Debug, $"Server Version: {Settings.ServerBuild}");

            // Disabled until we actually need it.
            //GameData.LoadEverything();

            BattlenetServer.Start(Settings.ServerAddress);

            while (true)
            {
                Thread.Sleep(1);
            }
        }
    }
}
