using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy
{
    public static class GameData
    {
        // Storages
        public static Dictionary<uint, BroadcastText> BroadcastTextStore = new Dictionary<uint, BroadcastText>();

        // Loading code
        public static void LoadEverything()
        {
            Console.WriteLine("[GameData] Loading database...");
            LoadBroadcastTexts();
            Console.WriteLine("[GameData] Finished loading database.");
        }
        public static void LoadBroadcastTexts()
        {
            // not implemented
        }
    }

    // Data structures
    public class BroadcastText
    {
        public uint Entry;
        public string MaleText;
        public string FemaleText;
    }
}
