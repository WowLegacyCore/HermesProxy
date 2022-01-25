using System;

namespace HermesProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ClientVersion: " + Settings.ClientBuild.ToString());
            Console.WriteLine("ServerVersion: " + Settings.ServerBuild.ToString());

            // Disabled until we actually need it.
            //GameData.LoadEverything(Settings.DatabaseConnectionString);

            new BnetServer().Run();
            new WorldServer().Run();
        }
    }
}
