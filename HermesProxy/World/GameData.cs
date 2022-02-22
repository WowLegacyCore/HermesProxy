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
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new SortedDictionary<uint, BroadcastText>();
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

        public static int GetTransportPeriod(int entry)
        {
            switch (entry)
            {
                case 20808:
                    return 350822;
                case 164871:
                    return 356287;
                case 175080:
                    return 303466;
                case 176231:
                    return 329315;
                case 176244:
                    return 316253;
                case 176310:
                    return 295580;
                case 176495:
                    return 335297;
                case 177233:
                    return 317044;
                case 181056:
                    return 1208095;
            }
            return 0;
        }

        public static BroadcastText GetBroadcastText(uint entry)
        {
            BroadcastText data;
            if (BroadcastTextStore.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static uint GetBroadcastTextId(string maleText, string femaleText, uint language, ushort[] emoteDelays, ushort[] emotes)
        {
            foreach (var itr in BroadcastTextStore)
            {
                if (itr.Value.MaleText == maleText &&
                    itr.Value.FemaleText == femaleText &&
                    itr.Value.Language == language &&
                    Enumerable.SequenceEqual(itr.Value.EmoteDelays, emoteDelays) &&
                    Enumerable.SequenceEqual(itr.Value.Emotes, emotes))
                {
                    return itr.Key;
                }
            }

            BroadcastText broadcastText = new();
            broadcastText.Entry = BroadcastTextStore.Keys.Last() + 1;
            broadcastText.MaleText = maleText;
            broadcastText.FemaleText = femaleText;
            broadcastText.Language = language;
            broadcastText.EmoteDelays = emoteDelays;
            broadcastText.Emotes = emotes;
            BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
            return broadcastText.Entry;
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
            var path = $"CSV\\BroadcastTexts{Settings.GetClientExpansionVersion()}.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    BroadcastText broadcastText = new BroadcastText();
                    broadcastText.Entry = UInt32.Parse(fields[0]);
                    broadcastText.MaleText = fields[1].Replace("~", "\n");
                    broadcastText.FemaleText = fields[2].Replace("~", "\n");
                    broadcastText.Language = UInt32.Parse(fields[3]);
                    broadcastText.Emotes[0] = UInt16.Parse(fields[4]);
                    broadcastText.Emotes[1] = UInt16.Parse(fields[5]);
                    broadcastText.Emotes[2] = UInt16.Parse(fields[6]);
                    broadcastText.EmoteDelays[0] = UInt16.Parse(fields[7]);
                    broadcastText.EmoteDelays[1] = UInt16.Parse(fields[8]);
                    broadcastText.EmoteDelays[2] = UInt16.Parse(fields[9]);
                    BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
                }
            }
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
        public uint Language;
        public ushort[] Emotes = new ushort[3];
        public ushort[] EmoteDelays = new ushort[3];
    }
    public class ItemTemplate
    {
        public uint Entry;
        public uint DisplayId;
        public byte InventoryType;
    }
}
