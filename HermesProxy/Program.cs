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
            //GameData.LoadEverything();

            new BnetServer().Run();
            new WorldServer().Run();
        }
    }
}
