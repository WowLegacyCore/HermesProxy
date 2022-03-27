using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Framework.Logging;
using HermesProxy.World.Objects;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World
{
    public static class GameData
    {
        // From CSV
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new SortedDictionary<uint, BroadcastText>();
        public static Dictionary<uint, ItemTemplate> ItemTemplateStore = new Dictionary<uint, ItemTemplate>();
        public static Dictionary<uint, uint> SpellVisuals = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> LearnSpells = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> Gems = new Dictionary<uint, uint>();
        public static Dictionary<uint, float> UnitDisplayScales = new Dictionary<uint, float>();

        // From Server
        public static Dictionary<uint, CreatureTemplate> CreatureTemplates = new Dictionary<uint, CreatureTemplate>();
        public static Dictionary<uint, QuestTemplate> QuestTemplates = new Dictionary<uint, QuestTemplate>();
        public static Dictionary<uint, string> ItemNames = new Dictionary<uint, string>();

        public static void StoreItemName(uint entry, string name)
        {
            if (ItemNames.ContainsKey(entry))
                ItemNames[entry] = name;
            else
                ItemNames.Add(entry, name);
        }
        public static string GetItemName(uint entry)
        {
            string data;
            if (ItemNames.TryGetValue(entry, out data))
                return data;
            return "";
        }
        public static void StoreQuestTemplate(uint entry, QuestTemplate template)
        {
            if (QuestTemplates.ContainsKey(entry))
                QuestTemplates[entry] = template;
            else
                QuestTemplates.Add(entry, template);
        }
        public static QuestTemplate GetQuestTemplate(uint entry)
        {
            QuestTemplate data;
            if (QuestTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }
        public static QuestObjective GetQuestObjectiveForItem(uint entry)
        {
            foreach (var quest in QuestTemplates)
            {
                foreach (var objective in quest.Value.Objectives)
                {
                    if (objective.ObjectID == entry &&
                        objective.Type == QuestObjectiveType.Item)
                        return objective;
                }
            }
            return null;
        }
        public static void StoreCreatureTemplate(uint entry, CreatureTemplate template)
        {
            if (CreatureTemplates.ContainsKey(entry))
                CreatureTemplates[entry] = template;
            else
                CreatureTemplates.Add(entry, template);
        }
        public static CreatureTemplate GetCreatureTemplate(uint entry)
        {
            CreatureTemplate data;
            if (CreatureTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }
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
        public static uint GetRealSpell(uint learnSpellId)
        {
            uint realSpellId;
            if (LearnSpells.TryGetValue(learnSpellId, out realSpellId))
                return realSpellId;
            return learnSpellId;
        }
        public static uint GetGemFromEnchantId(uint enchantId)
        {
            uint itemId;
            if (Gems.TryGetValue(enchantId, out itemId))
                return itemId;
            return 0;
        }
        public static uint GetEnchantIdFromGem(uint itemId)
        {
            foreach (var itr in Gems)
            {
                if (itr.Value == itemId)
                    return itr.Key;
            }
            return 0;
        }
        public static float GetUnitDisplayScale(uint displayId)
        {
            float scale;
            if (UnitDisplayScales.TryGetValue(displayId, out scale))
                return scale;
            return 1.0f;
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
                if (((!String.IsNullOrEmpty(maleText) && itr.Value.MaleText == maleText) ||
                     (!String.IsNullOrEmpty(femaleText) && itr.Value.FemaleText == femaleText)) &&
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
            Log.Print(LogType.Storage, "Loading data files...");
            LoadBroadcastTexts();
            LoadItemTemplates();
            LoadSpellVisuals();
            LoadLearnSpells();
            LoadGems();
            LoadUnitDisplayScales();
            Log.Print(LogType.Storage, "Finished loading data.");
        }
        public static void LoadBroadcastTexts()
        {
            var path = $"CSV\\BroadcastTexts{ModernVersion.GetExpansionVersion()}.csv";
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
                    broadcastText.MaleText = fields[1].TrimEnd().Replace("\0", "").Replace("~", "\n");
                    broadcastText.FemaleText = fields[2].TrimEnd().Replace("\0", "").Replace("~", "\n");
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
            var path = $"CSV\\Items{ModernVersion.GetExpansionVersion()}.csv";
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
            var path = $"CSV\\SpellVisuals{ModernVersion.GetExpansionVersion()}.csv";
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

        public static void LoadLearnSpells()
        {
            var path = $"CSV\\LearnSpells.csv";
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

                    uint learnSpellId = UInt32.Parse(fields[0]);
                    uint realSpellId = UInt32.Parse(fields[1]);
                    if (!LearnSpells.ContainsKey(learnSpellId))
                        LearnSpells.Add(learnSpellId, realSpellId);
                }
            }
        }

        public static void LoadGems()
        {
            if (ModernVersion.GetExpansionVersion() <= 1)
                return;

            var path = $"CSV\\Gems{ModernVersion.GetExpansionVersion()}.csv";
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

                    uint enchantId = UInt32.Parse(fields[0]);
                    uint itemId = UInt32.Parse(fields[1]);
                    Gems.Add(enchantId, itemId);
                }
            }
        }

        public static void LoadUnitDisplayScales()
        {
            if (LegacyVersion.GetExpansionVersion() > 1)
                return;

            var path = $"CSV\\UnitDisplayScales.csv";
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

                    uint displayId = UInt32.Parse(fields[0]);
                    float scale = Single.Parse(fields[1]);
                    UnitDisplayScales.Add(displayId, scale);
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
