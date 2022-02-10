using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Framework.Logging;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World
{
    public static class GameData
    {
        // Storages
        public static Dictionary<uint, BroadcastText> BroadcastTextStore = new Dictionary<uint, BroadcastText>();
        public static Dictionary<uint, ItemTemplate> ItemTemplateStore = new Dictionary<uint, ItemTemplate>();
        public static Dictionary<uint, uint> SpellVisuals = new Dictionary<uint, uint>();

        public static ItemTemplate GetItemTemplate(uint entry)
        {
            ItemTemplate data;
            if (ItemTemplateStore.TryGetValue(entry, out data))
                return data;
            return null;
        }
        public static uint GetItemIdWithDisplayId(uint displayId)
        {
            foreach (var item in ItemTemplateStore)
            {
                if (item.Value.DisplayId == displayId)
                    return item.Key;
            }
            return 0;
        }
        public static uint GetSpellVisual(uint spellId)
        {
            uint visual;
            if (SpellVisuals.TryGetValue(spellId, out visual))
                return visual;
            return 0;
        }

        // Loading code
        public static void LoadEverything()
        {
            Log.Print(LogType.Storage, "[GameData] Loading database...");
            LoadBroadcastTexts();
            LoadItemTemplates();
            LoadSpellVisuals();
            Log.Print(LogType.Storage, "[GameData] Finished loading database.");
        }
        public static void LoadBroadcastTexts()
        {
            // not implemented
        }
        public static void LoadItemTemplates()
        {
            var path = $"CSV\\Items{Settings.GetClientExpansionVersion()}.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    ItemTemplate item = new ItemTemplate();
                    item.Entry = UInt32.Parse(fields[0]);
                    item.DisplayId = UInt32.Parse(fields[1]);
                    item.InventoryType = Byte.Parse(fields[2]);
                    ItemTemplateStore.Add(item.Entry, item);
                }
            }
        }
        public static void LoadSpellVisuals()
        {
            var path = $"CSV\\SpellVisuals{Settings.GetClientExpansionVersion()}.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    uint visualId = UInt32.Parse(fields[1]);
                    SpellVisuals.Add(spellId, visualId);
                }
            }
        }
    }

    // Data structures
    public class BroadcastText
    {
        public uint Entry;
        public string MaleText;
        public string FemaleText;
    }
    public class ItemTemplate
    {
        public uint Entry;
        public uint DisplayId;
        public byte InventoryType;
    }
}
