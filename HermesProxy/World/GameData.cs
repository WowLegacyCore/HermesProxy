using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World
{
    public static class GameData
    {
        // From CSV
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new();
        public static Dictionary<uint, ItemDisplayData> ItemDisplayDataStore = new();
        public static Dictionary<uint, Battleground> Battlegrounds = new();
        public static Dictionary<uint, ChatChannel> ChatChannels = new();
        public static Dictionary<uint, Dictionary<uint, byte>> ItemEffects = new();
        public static Dictionary<uint, uint> ItemEnchantVisuals = new();
        public static Dictionary<uint, uint> SpellVisuals = new();
        public static Dictionary<uint, uint> LearnSpells = new();
        public static Dictionary<uint, uint> TotemSpells = new();
        public static Dictionary<uint, uint> Gems = new();
        public static Dictionary<uint, float> UnitDisplayScales = new();
        public static Dictionary<uint, uint> TransportPeriods = new();
        public static Dictionary<uint, string> AreaNames = new();
        public static HashSet<uint> DispellSpells = new();
        public static HashSet<uint> StackableAuras = new();
        public static HashSet<uint> MountAuras = new();
        public static HashSet<uint> NextMeleeSpells = new();
        public static HashSet<uint> AutoRepeatSpells = new();
        public static Dictionary<uint, TaxiPath> TaxiPaths = new();
        public static int[,] TaxiNodesGraph = new int[250,250];
        public static Dictionary<uint /*questId*/, uint /*questBit*/> QuestBits = new();

        // From Server
        public static Dictionary<uint, ItemTemplate> ItemTemplates = new();
        public static Dictionary<uint, CreatureTemplate> CreatureTemplates = new();
        public static Dictionary<uint, QuestTemplate> QuestTemplates = new();
        public static Dictionary<uint, string> ItemNames = new();

        #region GettersAndSetters
        public static void StoreItemName(uint entry, string name)
        {
            if (ItemNames.ContainsKey(entry))
                ItemNames[entry] = name;
            else
                ItemNames.Add(entry, name);
        }

        public static string GetItemName(uint entry)
        {
            if (ItemNames.TryGetValue(entry, out string data))
                return data;

            ItemTemplate template = GetItemTemplate(entry);
            if (template != null)
                return template.Name[0];

            return "";
        }

        public static void StoreItemTemplate(uint entry, ItemTemplate template)
        {
            if (ItemTemplates.ContainsKey(entry))
                ItemTemplates[entry] = template;
            else
                ItemTemplates.Add(entry, template);
        }

        public static ItemTemplate GetItemTemplate(uint entry)
        {
            if (ItemTemplates.TryGetValue(entry, out ItemTemplate data))
                return data;
            return null;
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
            if (QuestTemplates.TryGetValue(entry, out QuestTemplate data))
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

        public static uint? GetUniqueQuestBit(uint questId)
        {
            if (!QuestBits.TryGetValue(questId, out var result))
                return null;

            return result;
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
            if (CreatureTemplates.TryGetValue(entry, out CreatureTemplate data))
                return data;
            return null;
        }

        public static ItemDisplayData GetItemDisplayData(uint entry)
        {
            if (ItemDisplayDataStore.TryGetValue(entry, out ItemDisplayData data))
                return data;
            return null;
        }

        public static uint GetItemIdWithDisplayId(uint displayId)
        {
            foreach (var item in ItemDisplayDataStore)
            {
                if (item.Value.DisplayId == displayId)
                    return item.Key;
            }
            return 0;
        }

        public static void SaveItemEffectSlot(uint itemId, uint spellId, byte slot)
        {
            if (ItemEffects.ContainsKey(itemId))
            {
                if (ItemEffects[itemId].ContainsKey(spellId))
                    ItemEffects[itemId][spellId] = slot;
                else
                    ItemEffects[itemId].Add(spellId, slot);
            }
            else
            {
                Dictionary<uint, byte> dict = new();
                dict.Add(spellId, slot);
                ItemEffects.Add(itemId, dict);
            }
        }

        public static byte GetItemEffectSlot(uint itemId, uint spellId)
        {
            if (ItemEffects.ContainsKey(itemId) &&
                ItemEffects[itemId].ContainsKey(spellId))
                return ItemEffects[itemId][spellId];
            return 0;
        }

        public static uint GetItemEnchantVisual(uint enchantId)
        {
            if (ItemEnchantVisuals.TryGetValue(enchantId, out uint visualId))
                return visualId;
            return 0;
        }

        public static uint GetSpellVisual(uint spellId)
        {
            if (SpellVisuals.TryGetValue(spellId, out uint visual))
                return visual;
            return 0;
        }

        public static int GetTotemSlotForSpell(uint spellId)
        {
            if (TotemSpells.TryGetValue(spellId, out uint slot))
                return (int)slot;
            return -1;
        }

        public static uint GetRealSpell(uint learnSpellId)
        {
            if (LearnSpells.TryGetValue(learnSpellId, out uint realSpellId))
                return realSpellId;
            return learnSpellId;
        }

        public static uint GetGemFromEnchantId(uint enchantId)
        {
            if (Gems.TryGetValue(enchantId, out uint itemId))
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
            if (UnitDisplayScales.TryGetValue(displayId, out float scale))
                return scale;
            return 1.0f;
        }

        public static uint GetTransportPeriod(uint entry)
        {
            if (TransportPeriods.TryGetValue(entry, out uint period))
                return period;
            return 0;
        }

        public static string GetAreaName(uint id)
        {
            if (AreaNames.TryGetValue(id, out string name))
                return name;
            return "";
        }

        public static uint GetBattlegroundIdFromMapId(uint mapId)
        {
            foreach (var bg in Battlegrounds)
            {
                if (bg.Value.MapIds.Contains(mapId))
                    return bg.Key;
            }
            return 0;
        }

        public static uint GetMapIdFromBattlegroundId(uint bgId)
        {
            if (Battlegrounds.TryGetValue(bgId, out Battleground bg))
                return bg.MapIds[0];
            return 0;
        }

        public static uint GetChatChannelIdFromName(string name)
        {
            foreach (var channel in ChatChannels)
            {
                if (name.Contains(channel.Value.Name))
                    return channel.Key;
            }
            return 0;
        }

        public static List<ChatChannel> GetChatChannelsWithFlags(ChannelFlags flags)
        {
            List<ChatChannel> channels = new();
            foreach (var channel in ChatChannels)
            {
                if ((channel.Value.Flags & flags) == flags)
                    channels.Add(channel.Value);
            }
            return channels;
        }

        public static bool IsAllianceRace(Race raceId)
        {
            switch (raceId)
            {
                case Race.Human:
                case Race.Dwarf:
                case Race.NightElf:
                case Race.Gnome:
                case Race.Draenei:
                case Race.Worgen:
                    return true;
            }
            return false;
        }

        public static BroadcastText GetBroadcastText(uint entry)
        {
            if (BroadcastTextStore.TryGetValue(entry, out BroadcastText data))
                return data;
            return null;
        }

        public static uint GetBroadcastTextId(string maleText, string femaleText, uint language, ushort[] emoteDelays, ushort[] emotes)
        {
            foreach (var itr in BroadcastTextStore)
            {
                if (((!string.IsNullOrEmpty(maleText) && itr.Value.MaleText == maleText) ||
                     (!string.IsNullOrEmpty(femaleText) && itr.Value.FemaleText == femaleText)) &&
                    itr.Value.Language == language &&
                    Enumerable.SequenceEqual(itr.Value.EmoteDelays, emoteDelays) &&
                    Enumerable.SequenceEqual(itr.Value.Emotes, emotes))
                {
                    return itr.Key;
                }
            }

            BroadcastText broadcastText = new()
            {
                Entry = BroadcastTextStore.Keys.Last() + 1,
                MaleText = maleText,
                FemaleText = femaleText,
                Language = language,
                EmoteDelays = emoteDelays,
                Emotes = emotes
            };
            BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
            return broadcastText.Entry;
        }
        #endregion
        #region Loading
        // Loading code
        public static void LoadEverything()
        {
            Log.Print(LogType.Storage, "Loading data files...");
            LoadBroadcastTexts();
            LoadItemTemplates();
            LoadBattlegrounds();
            LoadChatChannels();
            LoadItemEnchantVisuals();
            LoadSpellVisuals();
            LoadLearnSpells();
            LoadTotemSpells();
            LoadGems();
            LoadUnitDisplayScales();
            LoadTransports();
            LoadAreaNames();
            LoadDispellSpells();
            LoadStackableAuras();
            LoadMountAuras();
            LoadMeleeSpells();
            LoadAutoRepeatSpells();
            LoadTaxiPaths();
            LoadTaxiPathNodesGraph();
            LoadQuestBits();
            LoadHotfixes();
            Log.Print(LogType.Storage, "Finished loading data.");
        }

        public static void LoadBroadcastTexts()
        {
            var path = Path.Combine("CSV", $"BroadcastTexts{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    BroadcastText broadcastText = new()
                    {
                        Entry = uint.Parse(fields[0]),
                        MaleText = fields[1].TrimEnd().Replace("\0", "").Replace("~", "\n"),
                        FemaleText = fields[2].TrimEnd().Replace("\0", "").Replace("~", "\n"),
                        Language = uint.Parse(fields[3])
                    };
                    broadcastText.Emotes[0] = ushort.Parse(fields[4]);
                    broadcastText.Emotes[1] = ushort.Parse(fields[5]);
                    broadcastText.Emotes[2] = ushort.Parse(fields[6]);
                    broadcastText.EmoteDelays[0] = ushort.Parse(fields[7]);
                    broadcastText.EmoteDelays[1] = ushort.Parse(fields[8]);
                    broadcastText.EmoteDelays[2] = ushort.Parse(fields[9]);
                    BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
                }
            }
        }

        public static void LoadItemTemplates()
        {
            var path = Path.Combine("CSV", $"Items{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    ItemDisplayData item = new()
                    {
                        Entry = uint.Parse(fields[0]),
                        DisplayId = uint.Parse(fields[1]),
                        InventoryType = byte.Parse(fields[2])
                    };
                    ItemDisplayDataStore.Add(item.Entry, item);
                }
            }
        }

        public static void LoadBattlegrounds()
        {
            var path = Path.Combine("CSV", "Battlegrounds.csv");
            using (TextFieldParser csvParser = new(path))
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

                    Battleground bg = new();
                    uint bgId = uint.Parse(fields[0]);
                    bg.IsArena = byte.Parse(fields[1]) != 0;
                    for (int i = 0; i < 6; i++)
                    {
                        uint mapId = uint.Parse(fields[2 + i]);
                        if (mapId != 0)
                            bg.MapIds.Add(mapId);
                    }
                    System.Diagnostics.Trace.Assert(bg.MapIds.Count != 0);
                    Battlegrounds.Add(bgId, bg);
                }
            }
        }

        public static void LoadChatChannels()
        {
            var path = Path.Combine("CSV", "ChatChannels.csv");
            using (TextFieldParser csvParser = new(path))
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

                    ChatChannel channel = new()
                    {
                        Id = uint.Parse(fields[0]),
                        Flags = (ChannelFlags)uint.Parse(fields[1]),
                        Name = fields[2]
                    };
                    ChatChannels.Add(channel.Id, channel);
                }
            }
        }

        public static void LoadItemEnchantVisuals()
        {
            var path = Path.Combine("CSV", $"ItemEnchantVisuals{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint enchantId = uint.Parse(fields[0]);
                    uint visualId = uint.Parse(fields[1]);
                    ItemEnchantVisuals.Add(enchantId, visualId);
                }
            }
        }

        public static void LoadSpellVisuals()
        {
            var path = Path.Combine("CSV", $"SpellVisuals{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    uint visualId = uint.Parse(fields[1]);
                    SpellVisuals.Add(spellId, visualId);
                }
            }
        }

        public static void LoadLearnSpells()
        {
            var path = Path.Combine("CSV", "LearnSpells.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint learnSpellId = uint.Parse(fields[0]);
                    uint realSpellId = uint.Parse(fields[1]);
                    if (!LearnSpells.ContainsKey(learnSpellId))
                        LearnSpells.Add(learnSpellId, realSpellId);
                }
            }
        }

        public static void LoadTotemSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"TotemSpells.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    uint totemSlot = uint.Parse(fields[1]);
                    TotemSpells.Add(spellId, totemSlot);
                }
            }
        }

        public static void LoadGems()
        {
            if (ModernVersion.ExpansionVersion <= 1)
                return;

            var path = Path.Combine("CSV", $"Gems{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint enchantId = uint.Parse(fields[0]);
                    uint itemId = uint.Parse(fields[1]);
                    Gems.Add(enchantId, itemId);
                }
            }
        }

        public static void LoadUnitDisplayScales()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", "UnitDisplayScales.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint displayId = uint.Parse(fields[0]);
                    float scale = float.Parse(fields[1]);
                    UnitDisplayScales.Add(displayId, scale);
                }
            }
        }

        public static void LoadTransports()
        {
            var path = Path.Combine("CSV", $"Transports{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint entry = uint.Parse(fields[0]);
                    uint period = uint.Parse(fields[1]);
                    TransportPeriods.Add(entry, period);
                }
            }
        }

        public static void LoadAreaNames()
        {
            var path = Path.Combine("CSV", $"AreaNames.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint id = uint.Parse(fields[0]);
                    string name = fields[1];
                    AreaNames.Add(id, name);
                }
            }
        }

        public static void LoadDispellSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", "DispellSpells.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    DispellSpells.Add(spellId);
                }
            }
        }

        public static void LoadStackableAuras()
        {
            if (LegacyVersion.ExpansionVersion > 2)
                return;

            var path = Path.Combine("CSV", $"StackableAuras{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    StackableAuras.Add(spellId);
                }
            }
        }

        public static void LoadMountAuras()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"MountAuras.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    MountAuras.Add(spellId);
                }
            }
        }

        public static void LoadMeleeSpells()
        {
            var path = Path.Combine("CSV", $"MeleeSpells{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    NextMeleeSpells.Add(spellId);
                }
            }
        }

        public static void LoadAutoRepeatSpells()
        {
            var path = Path.Combine("CSV", $"AutoRepeatSpells{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint spellId = uint.Parse(fields[0]);
                    AutoRepeatSpells.Add(spellId);
                }
            }
        }
        public static void LoadTaxiPaths()
        {
            var path = Path.Combine("CSV", $"TaxiPath{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    TaxiPath taxiPath = new()
                    {
                        Id = uint.Parse(fields[0]),
                        From = uint.Parse(fields[1]),
                        To = uint.Parse(fields[2]),
                        Cost = int.Parse(fields[3])
                    };
                    TaxiPaths.Add(counter, taxiPath);
                    counter++;
                }
            }
        }
        public static void LoadTaxiPathNodesGraph()
        {
            // Load TaxiNodes (used in calculating first and last parts of path)
            Dictionary<uint, TaxiNode> TaxiNodes = new();
            var pathNodes = Path.Combine("CSV", $"TaxiNodes{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(pathNodes))
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

                    TaxiNode taxiNode = new()
                    {
                        Id = uint.Parse(fields[0]),
                        mapId = uint.Parse(fields[1]),
                        x = float.Parse(fields[2]),
                        y = float.Parse(fields[3]),
                        z = float.Parse(fields[4])
                    };
                    TaxiNodes.Add(taxiNode.Id, taxiNode);
                }
            }
            // Load TaxiPathNode (used in calculating rest of path)
            Dictionary<uint, TaxiPathNode> TaxiPathNodes = new();
            var pathPathNodes = Path.Combine("CSV", $"TaxiPathNode{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(pathPathNodes))
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

                    TaxiPathNode taxiPathNode = new()
                    {
                        Id = uint.Parse(fields[0]),
                        pathId = uint.Parse(fields[1]),
                        nodeIndex = uint.Parse(fields[2]),
                        mapId = uint.Parse(fields[3]),
                        x = float.Parse(fields[4]),
                        y = float.Parse(fields[5]),
                        z = float.Parse(fields[6]),
                        flags = uint.Parse(fields[7]),
                        delay = uint.Parse(fields[8])
                    };
                    TaxiPathNodes.Add(taxiPathNode.Id, taxiPathNode);
                }
            }
            // calculate distances between nodes
            for (uint i = 0; i < TaxiPaths.Count; i++)
            {
                if (TaxiPaths.ContainsKey(i))
                {
                    float dist = 0.0f;
                    TaxiPath taxiPath = TaxiPaths[i];
                    TaxiNode nodeFrom = TaxiNodes[TaxiPaths[i].From];
                    TaxiNode nodeTo = TaxiNodes[TaxiPaths[i].To];

                    if (nodeFrom.x == 0 && nodeFrom.x == 0 && nodeFrom.z == 0)
                        continue;
                    if (nodeTo.x == 0 && nodeTo.x == 0 && nodeTo.z == 0)
                        continue;

                    // save all node ids of this path
                    HashSet<uint> pathNodeList = new();
                    foreach (var itr in TaxiPathNodes)
                    {
                        TaxiPathNode pNode = itr.Value;
                        if (pNode.pathId != taxiPath.Id)
                            continue;
                        pathNodeList.Add(pNode.Id);
                    }
                    // sort ids by node index
                    IEnumerable<uint> query = pathNodeList.OrderBy(node => TaxiPathNodes[node].nodeIndex);
                    uint curNode = 0;
                    foreach (var itr in query)
                    {
                        TaxiPathNode pNode = TaxiPathNodes[itr];
                        // calculate distance from start node
                        if (pNode.nodeIndex == 0)
                        {
                            dist += (float)Math.Sqrt(Math.Pow(nodeFrom.x - pNode.x, 2) + Math.Pow(nodeFrom.y - pNode.y, 2));
                            continue;
                        }
                        // set previous node
                        if (curNode == 0)
                        {
                            curNode = pNode.Id;
                            continue;
                        }
                        // calculate distance to previous node
                        if (curNode != 0)
                        {
                            TaxiPathNode prevNode = TaxiPathNodes[curNode];
                            curNode = pNode.Id;
                            if (prevNode.mapId != pNode.mapId)
                                continue;

                            dist += (float)Math.Sqrt(Math.Pow(prevNode.x - pNode.x, 2) + Math.Pow(prevNode.y - pNode.y, 2));
                        }
                    }
                    // calculate distance to last node
                    if (curNode != 0) // should not happen
                    {
                        TaxiPathNode lastNode = TaxiPathNodes[curNode];
                        dist += (float)Math.Sqrt(Math.Pow(nodeTo.x - lastNode.x, 2) + Math.Pow(nodeTo.y - lastNode.y, 2));
                    }
                    TaxiNodesGraph[TaxiPaths[i].From, TaxiPaths[i].To] = dist > 0 ? (int)dist : 0;
                }
            }
        }

        public static void LoadQuestBits()
        {
            var path = Path.Combine("CSV", $"QuestV2_{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
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

                    uint questId = uint.Parse(fields[0]);
                    if (fields[1].StartsWith("-"))
                        continue; // Some bits have a negative index, is this an error from WDBX?
                    uint uniqueBitFlag = uint.Parse(fields[1]);
                    QuestBits.Add(questId, uniqueBitFlag);
                }
            }
        }

        #endregion
        #region HotFixes
        // Stores
        public const uint HotfixAreaTriggerBegin = 100000;
        public const uint HotfixSkillLineBegin = 110000;
        public const uint HotfixSkillRaceClassInfoBegin = 120000;
        public const uint HotfixSkillLineAbilityBegin = 130000;
        public const uint HotfixSpellBegin = 140000;
        public const uint HotfixSpellNameBegin = 150000;
        public const uint HotfixSpellLevelsBegin = 160000;
        public const uint HotfixSpellAuraOptionsBegin = 170000;
        public const uint HotfixSpellMiscBegin = 180000;
        public const uint HotfixSpellEffectBegin = 190000;
        public const uint HotfixSpellXSpellVisualBegin = 200000;
        public const uint HotfixItemSparseBegin = 210000;
        public const uint HotfixCreatureDisplayInfoBegin = 220000;
        public const uint HotfixCreatureDisplayInfoExtraBegin = 230000;
        public const uint HotfixCreatureDisplayInfoOptionBegin = 240000;
        public static Dictionary<uint, HotfixRecord> Hotfixes = new();
        public static void LoadHotfixes()
        {
            LoadAreaTriggerHotfixes();
            LoadSkillLineHotfixes();
            LoadSkillRaceClassInfoHotfixes();
            LoadSkillLineAbilityHotfixes();
            LoadSpellHotfixes();
            LoadSpellNameHotfixes();
            LoadSpellLevelsHotfixes();
            LoadSpellAuraOptionsHotfixes();
            LoadSpellMiscHotfixes();
            LoadSpellEffectHotfixes();
            LoadSpellXSpellVisualHotfixes();
            LoadItemSparseHotfixes();
            LoadCreatureDisplayInfoHotfixes();
            LoadCreatureDisplayInfoExtraHotfixes();
            LoadCreatureDisplayInfoOptionHotfixes();
        }

        public static void LoadAreaTriggerHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"AreaTrigger{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    AreaTrigger at = new()
                    {
                        Message = fields[0],
                        PositionX = float.Parse(fields[1]),
                        PositionY = float.Parse(fields[2]),
                        PositionZ = float.Parse(fields[3]),
                        Id = uint.Parse(fields[4]),
                        MapId = ushort.Parse(fields[5]),
                        PhaseUseFlags = byte.Parse(fields[6]),
                        PhaseId = ushort.Parse(fields[7]),
                        PhaseGroupId = ushort.Parse(fields[8]),
                        Radius = float.Parse(fields[9]),
                        BoxLength = float.Parse(fields[10]),
                        BoxWidth = float.Parse(fields[11]),
                        BoxHeight = float.Parse(fields[12]),
                        BoxYaw = float.Parse(fields[13]),
                        ShapeType = byte.Parse(fields[14]),
                        ShapeId = ushort.Parse(fields[15]),
                        ActionSetId = ushort.Parse(fields[16]),
                        Flags = byte.Parse(fields[17])
                    };

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.AreaTrigger,
                        HotfixId = HotfixAreaTriggerBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = at.Id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(at.Message);
                    record.HotfixContent.WriteFloat(at.PositionX);
                    record.HotfixContent.WriteFloat(at.PositionY);
                    record.HotfixContent.WriteFloat(at.PositionZ);
                    record.HotfixContent.WriteUInt32(at.Id);
                    record.HotfixContent.WriteUInt16(at.MapId);
                    record.HotfixContent.WriteUInt8(at.PhaseUseFlags);
                    record.HotfixContent.WriteUInt16(at.PhaseId);
                    record.HotfixContent.WriteUInt16(at.PhaseGroupId);
                    record.HotfixContent.WriteFloat(at.Radius);
                    record.HotfixContent.WriteFloat(at.BoxLength);
                    record.HotfixContent.WriteFloat(at.BoxWidth);
                    record.HotfixContent.WriteFloat(at.BoxHeight);
                    record.HotfixContent.WriteFloat(at.BoxYaw);
                    record.HotfixContent.WriteUInt8(at.ShapeType);
                    record.HotfixContent.WriteUInt16(at.ShapeId);
                    record.HotfixContent.WriteUInt16(at.ActionSetId);
                    record.HotfixContent.WriteUInt8(at.Flags);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSkillLineHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillLine{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    string displayName = fields[0];
                    string alternateVerb = fields[1];
                    string description = fields[2];
                    string hordeDisplayName = fields[3];
                    string neutralDisplayName = fields[4];
                    uint id = uint.Parse(fields[5]);
                    byte categoryID = byte.Parse(fields[6]);
                    uint spellIconFileID = uint.Parse(fields[7]);
                    byte canLink = byte.Parse(fields[8]);
                    uint parentSkillLineID = uint.Parse(fields[9]);
                    uint parentTierIndex = uint.Parse(fields[10]);
                    ushort flags = ushort.Parse(fields[11]);
                    uint spellBookSpellID = uint.Parse(fields[12]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SkillLine,
                        HotfixId = HotfixSkillLineBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(displayName);
                    record.HotfixContent.WriteCString(alternateVerb);
                    record.HotfixContent.WriteCString(description);
                    record.HotfixContent.WriteCString(hordeDisplayName);
                    record.HotfixContent.WriteCString(neutralDisplayName);
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt8(categoryID);
                    record.HotfixContent.WriteUInt32(spellIconFileID);
                    record.HotfixContent.WriteUInt8(canLink);
                    record.HotfixContent.WriteUInt32(parentSkillLineID);
                    record.HotfixContent.WriteUInt32(parentTierIndex);
                    record.HotfixContent.WriteUInt16(flags);
                    record.HotfixContent.WriteUInt32(spellBookSpellID);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSkillRaceClassInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillRaceClassInfo{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    ulong raceMask = ulong.Parse(fields[1]);
                    ushort skillId = ushort.Parse(fields[2]);
                    uint classMask = uint.Parse(fields[3]);
                    ushort flags = ushort.Parse(fields[4]);
                    byte availability = byte.Parse(fields[5]);
                    byte minLevel = byte.Parse(fields[6]);
                    ushort skillTierId = ushort.Parse(fields[7]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SkillRaceClassInfo,
                        HotfixId = HotfixSkillRaceClassInfoBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt64(raceMask);
                    record.HotfixContent.WriteUInt16(skillId);
                    record.HotfixContent.WriteUInt32(classMask);
                    record.HotfixContent.WriteUInt16(flags);
                    record.HotfixContent.WriteUInt8(availability);
                    record.HotfixContent.WriteUInt8(minLevel);
                    record.HotfixContent.WriteUInt16(skillTierId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSkillLineAbilityHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillLineAbility{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    ulong raceMask = ulong.Parse(fields[0]);
                    uint id = uint.Parse(fields[1]);
                    ushort skillId = ushort.Parse(fields[2]);
                    uint spellId = uint.Parse(fields[3]);
                    ushort minSkillLineRank = ushort.Parse(fields[4]);
                    uint classMask = uint.Parse(fields[5]);
                    uint supercedesSpellId = uint.Parse(fields[6]);
                    byte acquireMethod = byte.Parse(fields[7]);
                    ushort trivialSkillLineRankHigh = ushort.Parse(fields[8]);
                    ushort trivialSkillLineRankLow = ushort.Parse(fields[9]);
                    byte flags = byte.Parse(fields[10]);
                    byte numSkillUps = byte.Parse(fields[11]);
                    ushort uniqueBit = ushort.Parse(fields[12]);
                    ushort tradeSkillCategoryId = ushort.Parse(fields[13]);
                    ushort skillUpSkillLineId = ushort.Parse(fields[14]);
                    uint characterPoints1 = uint.Parse(fields[15]);
                    uint characterPoints2 = uint.Parse(fields[16]);


                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SkillLineAbility,
                        HotfixId = HotfixSkillLineAbilityBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt64(raceMask);
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt16(skillId);
                    record.HotfixContent.WriteUInt32(spellId);
                    record.HotfixContent.WriteUInt16(minSkillLineRank);
                    record.HotfixContent.WriteUInt32(classMask);
                    record.HotfixContent.WriteUInt32(supercedesSpellId);
                    record.HotfixContent.WriteUInt8(acquireMethod);
                    record.HotfixContent.WriteUInt16(trivialSkillLineRankHigh);
                    record.HotfixContent.WriteUInt16(trivialSkillLineRankLow);
                    record.HotfixContent.WriteUInt8(flags);
                    record.HotfixContent.WriteUInt8(numSkillUps);
                    record.HotfixContent.WriteUInt16(uniqueBit);
                    record.HotfixContent.WriteUInt16(tradeSkillCategoryId);
                    record.HotfixContent.WriteUInt16(skillUpSkillLineId);
                    record.HotfixContent.WriteUInt32(characterPoints1);
                    record.HotfixContent.WriteUInt32(characterPoints2);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"Spell{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    string nameSubText = fields[1];
                    string description = fields[2];
                    string auraDescription = fields[3];

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.Spell,
                        HotfixId = HotfixSpellBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(nameSubText);
                    record.HotfixContent.WriteCString(description);
                    record.HotfixContent.WriteCString(auraDescription);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellNameHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellName{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    string name = fields[1];

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SpellName,
                        HotfixId = HotfixSpellNameBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(name);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellLevelsHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellLevels{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    byte difficultyId = byte.Parse(fields[1]);
                    ushort baseLevel = ushort.Parse(fields[2]);
                    ushort maxLevel = ushort.Parse(fields[3]);
                    ushort spellLevel = ushort.Parse(fields[4]);
                    byte maxPassiveAuraLevel = byte.Parse(fields[5]);
                    uint spellId = uint.Parse(fields[6]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SpellLevels,
                        HotfixId = HotfixSpellLevelsBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt16(baseLevel);
                    record.HotfixContent.WriteUInt16(maxLevel);
                    record.HotfixContent.WriteUInt16(spellLevel);
                    record.HotfixContent.WriteUInt8(maxPassiveAuraLevel);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellAuraOptionsHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellAuraOptions{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    byte difficultyId = byte.Parse(fields[1]);
                    uint cumulatievAura = uint.Parse(fields[2]);
                    uint procCategoryRecovery = uint.Parse(fields[3]);
                    byte procChance = byte.Parse(fields[4]);
                    uint procCharges = uint.Parse(fields[5]);
                    ushort spellProcsPerMinuteId = ushort.Parse(fields[6]);
                    uint procTypeMask0 = uint.Parse(fields[7]);
                    uint procTypeMask1 = uint.Parse(fields[8]);
                    uint spellId = uint.Parse(fields[9]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SpellAuraOptions,
                        HotfixId = HotfixSpellAuraOptionsBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt32(cumulatievAura);
                    record.HotfixContent.WriteUInt32(procCategoryRecovery);
                    record.HotfixContent.WriteUInt8(procChance);
                    record.HotfixContent.WriteUInt32(procCharges);
                    record.HotfixContent.WriteUInt16(spellProcsPerMinuteId);
                    record.HotfixContent.WriteUInt32(procTypeMask0);
                    record.HotfixContent.WriteUInt32(procTypeMask1);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellMiscHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellMisc{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    byte difficultyId = byte.Parse(fields[1]);
                    ushort castingTimeIndex = ushort.Parse(fields[2]);
                    ushort durationIndex = ushort.Parse(fields[3]);
                    ushort rangeIndex = ushort.Parse(fields[4]);
                    byte schoolMask = byte.Parse(fields[5]);
                    float speed = float.Parse(fields[6]);
                    float launchDelay = float.Parse(fields[7]);
                    float minDuration = float.Parse(fields[8]);
                    uint spellIconFileDataId = uint.Parse(fields[9]);
                    uint activeIconFileDataId = uint.Parse(fields[10]);
                    uint attributes1 = uint.Parse(fields[11]);
                    uint attributes2 = uint.Parse(fields[12]);
                    uint attributes3 = uint.Parse(fields[13]);
                    uint attributes4 = uint.Parse(fields[14]);
                    uint attributes5 = uint.Parse(fields[15]);
                    uint attributes6 = uint.Parse(fields[16]);
                    uint attributes7 = uint.Parse(fields[17]);
                    uint attributes8 = uint.Parse(fields[18]);
                    uint attributes9 = uint.Parse(fields[19]);
                    uint attributes10 = uint.Parse(fields[20]);
                    uint attributes11 = uint.Parse(fields[21]);
                    uint attributes12 = uint.Parse(fields[22]);
                    uint attributes13 = uint.Parse(fields[23]);
                    uint attributes14 = uint.Parse(fields[24]);
                    uint spellId = uint.Parse(fields[25]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SpellMisc,
                        HotfixId = HotfixSpellMiscBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt16(castingTimeIndex);
                    record.HotfixContent.WriteUInt16(durationIndex);
                    record.HotfixContent.WriteUInt16(rangeIndex);
                    record.HotfixContent.WriteUInt8(schoolMask);
                    record.HotfixContent.WriteFloat(speed);
                    record.HotfixContent.WriteFloat(launchDelay);
                    record.HotfixContent.WriteFloat(minDuration);
                    record.HotfixContent.WriteUInt32(spellIconFileDataId);
                    record.HotfixContent.WriteUInt32(activeIconFileDataId);
                    record.HotfixContent.WriteUInt32(attributes1);
                    record.HotfixContent.WriteUInt32(attributes2);
                    record.HotfixContent.WriteUInt32(attributes3);
                    record.HotfixContent.WriteUInt32(attributes4);
                    record.HotfixContent.WriteUInt32(attributes5);
                    record.HotfixContent.WriteUInt32(attributes6);
                    record.HotfixContent.WriteUInt32(attributes7);
                    record.HotfixContent.WriteUInt32(attributes8);
                    record.HotfixContent.WriteUInt32(attributes9);
                    record.HotfixContent.WriteUInt32(attributes10);
                    record.HotfixContent.WriteUInt32(attributes11);
                    record.HotfixContent.WriteUInt32(attributes12);
                    record.HotfixContent.WriteUInt32(attributes13);
                    record.HotfixContent.WriteUInt32(attributes14);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellEffectHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellEffect{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    uint difficultyId = uint.Parse(fields[1]);
                    uint effectIndex = uint.Parse(fields[2]);
                    uint effect = uint.Parse(fields[3]);
                    float effectAmplitude = float.Parse(fields[4]);
                    uint effectAttributes = uint.Parse(fields[5]);
                    short effectAura = short.Parse(fields[6]);
                    int effectAuraPeriod = int.Parse(fields[7]);
                    int effectBasePoints = int.Parse(fields[8]);
                    float effectBonusCoefficient = float.Parse(fields[9]);
                    float effectChainAmplitude = float.Parse(fields[10]);
                    int effectChainTargets = int.Parse(fields[11]);
                    int effectDieSides = int.Parse(fields[12]);
                    int effectItemType = int.Parse(fields[13]);
                    int effectMechanic = int.Parse(fields[14]);
                    float effectPointsPerResource = float.Parse(fields[15]);
                    float effectPosFacing = float.Parse(fields[16]);
                    float effectRealPointsPerLevel = float.Parse(fields[17]);
                    int EffectTriggerSpell = int.Parse(fields[18]);
                    float bonusCoefficientFromAP = float.Parse(fields[19]);
                    float pvpMultiplier = float.Parse(fields[20]);
                    float coefficient = float.Parse(fields[21]);
                    float variance = float.Parse(fields[22]);
                    float resourceCoefficient = float.Parse(fields[23]);
                    float groupSizeBasePointsCoefficient = float.Parse(fields[24]);
                    int effectMiscValue1 = int.Parse(fields[25]);
                    int effectMiscValue2 = int.Parse(fields[26]);
                    uint effectRadiusIndex1 = uint.Parse(fields[27]);
                    uint effectRadiusIndex2 = uint.Parse(fields[28]);
                    int effectSpellClassMask1 = int.Parse(fields[29]);
                    int effectSpellClassMask2 = int.Parse(fields[30]);
                    int effectSpellClassMask3 = int.Parse(fields[31]);
                    int effectSpellClassMask4 = int.Parse(fields[32]);
                    short implicitTarget1 = short.Parse(fields[33]);
                    short implicitTarget2 = short.Parse(fields[34]);
                    uint spellId = uint.Parse(fields[35]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SpellEffect,
                        HotfixId = HotfixSpellEffectBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(difficultyId);
                    record.HotfixContent.WriteUInt32(effectIndex);
                    record.HotfixContent.WriteUInt32(effect);
                    record.HotfixContent.WriteFloat(effectAmplitude);
                    record.HotfixContent.WriteUInt32(effectAttributes);
                    record.HotfixContent.WriteInt16(effectAura);
                    record.HotfixContent.WriteInt32(effectAuraPeriod);
                    record.HotfixContent.WriteInt32(effectBasePoints);
                    record.HotfixContent.WriteFloat(effectBonusCoefficient);
                    record.HotfixContent.WriteFloat(effectChainAmplitude);
                    record.HotfixContent.WriteInt32(effectChainTargets);
                    record.HotfixContent.WriteInt32(effectDieSides);
                    record.HotfixContent.WriteInt32(effectItemType);
                    record.HotfixContent.WriteInt32(effectMechanic);
                    record.HotfixContent.WriteFloat(effectPointsPerResource);
                    record.HotfixContent.WriteFloat(effectPosFacing);
                    record.HotfixContent.WriteFloat(effectRealPointsPerLevel);
                    record.HotfixContent.WriteInt32(EffectTriggerSpell);
                    record.HotfixContent.WriteFloat(bonusCoefficientFromAP);
                    record.HotfixContent.WriteFloat(pvpMultiplier);
                    record.HotfixContent.WriteFloat(coefficient);
                    record.HotfixContent.WriteFloat(variance);
                    record.HotfixContent.WriteFloat(resourceCoefficient);
                    record.HotfixContent.WriteFloat(groupSizeBasePointsCoefficient);
                    record.HotfixContent.WriteInt32(effectMiscValue1);
                    record.HotfixContent.WriteInt32(effectMiscValue2);
                    record.HotfixContent.WriteUInt32(effectRadiusIndex1);
                    record.HotfixContent.WriteUInt32(effectRadiusIndex2);
                    record.HotfixContent.WriteInt32(effectSpellClassMask1);
                    record.HotfixContent.WriteInt32(effectSpellClassMask2);
                    record.HotfixContent.WriteInt32(effectSpellClassMask3);
                    record.HotfixContent.WriteInt32(effectSpellClassMask4);
                    record.HotfixContent.WriteInt16(implicitTarget1);
                    record.HotfixContent.WriteInt16(implicitTarget2);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellXSpellVisualHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    byte difficultyId = byte.Parse(fields[1]);
                    uint spellVisualId = uint.Parse(fields[2]);
                    float probability = float.Parse(fields[3]);
                    byte flags = byte.Parse(fields[4]);
                    byte priority = byte.Parse(fields[5]);
                    int spellIconFileId = int.Parse(fields[6]);
                    int activeIconFileId = int.Parse(fields[7]);
                    ushort viewerUnitConditionId = ushort.Parse(fields[8]);
                    uint viewerPlayerConditionId = uint.Parse(fields[9]);
                    ushort casterUnitConditionId = ushort.Parse(fields[10]);
                    uint casterPlayerConditionId = uint.Parse(fields[11]);
                    uint spellId = uint.Parse(fields[12]);

                    if (SpellVisuals.ContainsKey(spellId))
                        SpellVisuals[spellId] = id;
                    else
                        SpellVisuals.Add(spellId, id);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.SpellXSpellVisual,
                        HotfixId = HotfixSpellXSpellVisualBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt32(spellVisualId);
                    record.HotfixContent.WriteFloat(probability);
                    record.HotfixContent.WriteUInt8(flags);
                    record.HotfixContent.WriteUInt8(priority);
                    record.HotfixContent.WriteInt32(spellIconFileId);
                    record.HotfixContent.WriteInt32(activeIconFileId);
                    record.HotfixContent.WriteUInt16(viewerUnitConditionId);
                    record.HotfixContent.WriteUInt32(viewerPlayerConditionId);
                    record.HotfixContent.WriteUInt16(casterUnitConditionId);
                    record.HotfixContent.WriteUInt32(casterPlayerConditionId);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadItemSparseHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemSparse{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    long allowableRace = long.Parse(fields[1]);
                    string description = fields[2];
                    string name4 = fields[3];
                    string name3 = fields[4];
                    string name2 = fields[5];
                    string name1 = fields[6];
                    float dmgVariance = float.Parse(fields[7]);
                    uint durationInInventory = uint.Parse(fields[8]);
                    float qualityModifier = float.Parse(fields[9]);
                    uint bagFamily = uint.Parse(fields[10]);
                    float rangeMod = float.Parse(fields[11]);
                    float statPercentageOfSocket1 = float.Parse(fields[12]);
                    float statPercentageOfSocket2 = float.Parse(fields[13]);
                    float statPercentageOfSocket3 = float.Parse(fields[14]);
                    float statPercentageOfSocket4 = float.Parse(fields[15]);
                    float statPercentageOfSocket5 = float.Parse(fields[16]);
                    float statPercentageOfSocket6 = float.Parse(fields[17]);
                    float statPercentageOfSocket7 = float.Parse(fields[18]);
                    float statPercentageOfSocket8 = float.Parse(fields[19]);
                    float statPercentageOfSocket9 = float.Parse(fields[20]);
                    float statPercentageOfSocket10 = float.Parse(fields[21]);
                    int statPercentEditor1 = int.Parse(fields[22]);
                    int statPercentEditor2 = int.Parse(fields[23]);
                    int statPercentEditor3 = int.Parse(fields[24]);
                    int statPercentEditor4 = int.Parse(fields[25]);
                    int statPercentEditor5 = int.Parse(fields[26]);
                    int statPercentEditor6 = int.Parse(fields[27]);
                    int statPercentEditor7 = int.Parse(fields[28]);
                    int statPercentEditor8 = int.Parse(fields[29]);
                    int statPercentEditor9 = int.Parse(fields[30]);
                    int statPercentEditor10 = int.Parse(fields[31]);
                    int stackable = int.Parse(fields[32]);
                    int maxCount = int.Parse(fields[33]);
                    uint requiredAbility = uint.Parse(fields[34]);
                    uint sellPrice = uint.Parse(fields[35]);
                    uint buyPrice = uint.Parse(fields[36]);
                    uint vendorStackCount = uint.Parse(fields[37]);
                    float priceVariance = float.Parse(fields[38]);
                    float priceRandomValue = float.Parse(fields[39]);
                    int flags1 = int.Parse(fields[40]);
                    int flags2 = int.Parse(fields[41]);
                    int flags3 = int.Parse(fields[42]);
                    int flags4 = int.Parse(fields[43]);
                    int oppositeFactionItemId = int.Parse(fields[44]);
                    uint maxDurability = uint.Parse(fields[45]);
                    ushort itemNameDescriptionId = ushort.Parse(fields[46]);
                    ushort requiredTransmogHoliday = ushort.Parse(fields[47]);
                    ushort requiredHoliday = ushort.Parse(fields[48]);
                    ushort limitCategory = ushort.Parse(fields[49]);
                    ushort gemProperties = ushort.Parse(fields[50]);
                    ushort socketMatchEnchantmentId = ushort.Parse(fields[51]);
                    ushort totemCategoryId = ushort.Parse(fields[52]);
                    ushort instanceBound = ushort.Parse(fields[53]);
                    ushort zoneBound1 = ushort.Parse(fields[54]);
                    ushort zoneBound2 = ushort.Parse(fields[55]);
                    ushort itemSet = ushort.Parse(fields[56]);
                    ushort lockId = ushort.Parse(fields[57]);
                    ushort startQuestId = ushort.Parse(fields[58]);
                    ushort pageText = ushort.Parse(fields[59]);
                    ushort delay = ushort.Parse(fields[60]);
                    ushort requiredReputationId = ushort.Parse(fields[61]);
                    ushort requiredSkillRank = ushort.Parse(fields[62]);
                    ushort requiredSkill = ushort.Parse(fields[63]);
                    ushort itemLevel = ushort.Parse(fields[64]);
                    short allowableClass = short.Parse(fields[65]);
                    ushort itemRandomSuffixGroupId = ushort.Parse(fields[66]);
                    ushort randomProperty = ushort.Parse(fields[67]);
                    ushort damageMin1 = ushort.Parse(fields[68]);
                    ushort damageMin2 = ushort.Parse(fields[69]);
                    ushort damageMin3 = ushort.Parse(fields[70]);
                    ushort damageMin4 = ushort.Parse(fields[71]);
                    ushort damageMin5 = ushort.Parse(fields[72]);
                    ushort damageMax1 = ushort.Parse(fields[73]);
                    ushort damageMax2 = ushort.Parse(fields[74]);
                    ushort damageMax3 = ushort.Parse(fields[75]);
                    ushort damageMax4 = ushort.Parse(fields[76]);
                    ushort damageMax5 = ushort.Parse(fields[77]);
                    short armor = short.Parse(fields[78]);
                    short holyResistance = short.Parse(fields[79]);
                    short fireResistance = short.Parse(fields[80]);
                    short natureResistance = short.Parse(fields[81]);
                    short frostResistance = short.Parse(fields[82]);
                    short shadowResistance = short.Parse(fields[83]);
                    short arcaneResistance = short.Parse(fields[84]);
                    ushort scalingStatDistributionId = ushort.Parse(fields[85]);
                    byte expansionId = byte.Parse(fields[86]);
                    byte artifactId = byte.Parse(fields[87]);
                    byte spellWeight = byte.Parse(fields[88]);
                    byte spellWeightCategory = byte.Parse(fields[89]);
                    byte socketType1 = byte.Parse(fields[90]);
                    byte socketType2 = byte.Parse(fields[91]);
                    byte socketType3 = byte.Parse(fields[92]);
                    byte sheatheType = byte.Parse(fields[93]);
                    byte material = byte.Parse(fields[94]);
                    byte pageMaterial = byte.Parse(fields[95]);
                    byte pageLanguage = byte.Parse(fields[96]);
                    byte bonding = byte.Parse(fields[97]);
                    byte damageType = byte.Parse(fields[98]);
                    sbyte statType1 = sbyte.Parse(fields[99]);
                    sbyte statType2 = sbyte.Parse(fields[100]);
                    sbyte statType3 = sbyte.Parse(fields[101]);
                    sbyte statType4 = sbyte.Parse(fields[102]);
                    sbyte statType5 = sbyte.Parse(fields[103]);
                    sbyte statType6 = sbyte.Parse(fields[104]);
                    sbyte statType7 = sbyte.Parse(fields[105]);
                    sbyte statType8 = sbyte.Parse(fields[106]);
                    sbyte statType9 = sbyte.Parse(fields[107]);
                    sbyte statType10 = sbyte.Parse(fields[108]);
                    byte containerSlots = byte.Parse(fields[109]);
                    byte requiredReputationRank = byte.Parse(fields[110]);
                    byte requiredCityRank = byte.Parse(fields[111]);
                    byte requiredHonorRank = byte.Parse(fields[112]);
                    byte inventoryType = byte.Parse(fields[113]);
                    byte overallQualityId = byte.Parse(fields[114]);
                    byte ammoType = byte.Parse(fields[115]);
                    sbyte statValue1 = sbyte.Parse(fields[116]);
                    sbyte statValue2 = sbyte.Parse(fields[117]);
                    sbyte statValue3 = sbyte.Parse(fields[118]);
                    sbyte statValue4 = sbyte.Parse(fields[119]);
                    sbyte statValue5 = sbyte.Parse(fields[120]);
                    sbyte statValue6 = sbyte.Parse(fields[121]);
                    sbyte statValue7 = sbyte.Parse(fields[122]);
                    sbyte statValue8 = sbyte.Parse(fields[123]);
                    sbyte statValue9 = sbyte.Parse(fields[124]);
                    sbyte statValue10 = sbyte.Parse(fields[125]);
                    sbyte requiredLevel = sbyte.Parse(fields[126]);

                    HotfixRecord record = new()
                    {
                        Status = HotfixStatus.Valid,
                        TableHash = DB2Hash.ItemSparse,
                        HotfixId = HotfixItemSparseBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteInt64(allowableRace);
                    record.HotfixContent.WriteCString(description);
                    record.HotfixContent.WriteCString(name4);
                    record.HotfixContent.WriteCString(name3);
                    record.HotfixContent.WriteCString(name2);
                    record.HotfixContent.WriteCString(name1);
                    record.HotfixContent.WriteFloat(dmgVariance);
                    record.HotfixContent.WriteUInt32(durationInInventory);
                    record.HotfixContent.WriteFloat(qualityModifier);
                    record.HotfixContent.WriteUInt32(bagFamily);
                    record.HotfixContent.WriteFloat(rangeMod);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket1);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket2);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket3);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket4);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket5);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket6);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket7);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket8);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket9);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket10);
                    record.HotfixContent.WriteInt32(statPercentEditor1);
                    record.HotfixContent.WriteInt32(statPercentEditor2);
                    record.HotfixContent.WriteInt32(statPercentEditor3);
                    record.HotfixContent.WriteInt32(statPercentEditor4);
                    record.HotfixContent.WriteInt32(statPercentEditor5);
                    record.HotfixContent.WriteInt32(statPercentEditor6);
                    record.HotfixContent.WriteInt32(statPercentEditor7);
                    record.HotfixContent.WriteInt32(statPercentEditor8);
                    record.HotfixContent.WriteInt32(statPercentEditor9);
                    record.HotfixContent.WriteInt32(statPercentEditor10);
                    record.HotfixContent.WriteInt32(stackable);
                    record.HotfixContent.WriteInt32(maxCount);
                    record.HotfixContent.WriteUInt32(requiredAbility);
                    record.HotfixContent.WriteUInt32(sellPrice);
                    record.HotfixContent.WriteUInt32(buyPrice);
                    record.HotfixContent.WriteUInt32(vendorStackCount);
                    record.HotfixContent.WriteFloat(priceVariance);
                    record.HotfixContent.WriteFloat(priceRandomValue);
                    record.HotfixContent.WriteInt32(flags1);
                    record.HotfixContent.WriteInt32(flags2);
                    record.HotfixContent.WriteInt32(flags3);
                    record.HotfixContent.WriteInt32(flags4);
                    record.HotfixContent.WriteInt32(oppositeFactionItemId);
                    record.HotfixContent.WriteUInt32(maxDurability);
                    record.HotfixContent.WriteUInt16(itemNameDescriptionId);
                    record.HotfixContent.WriteUInt16(requiredTransmogHoliday);
                    record.HotfixContent.WriteUInt16(requiredHoliday);
                    record.HotfixContent.WriteUInt16(limitCategory);
                    record.HotfixContent.WriteUInt16(gemProperties);
                    record.HotfixContent.WriteUInt16(socketMatchEnchantmentId);
                    record.HotfixContent.WriteUInt16(totemCategoryId);
                    record.HotfixContent.WriteUInt16(instanceBound);
                    record.HotfixContent.WriteUInt16(zoneBound1);
                    record.HotfixContent.WriteUInt16(zoneBound2);
                    record.HotfixContent.WriteUInt16(itemSet);
                    record.HotfixContent.WriteUInt16(lockId);
                    record.HotfixContent.WriteUInt16(startQuestId);
                    record.HotfixContent.WriteUInt16(pageText);
                    record.HotfixContent.WriteUInt16(delay);
                    record.HotfixContent.WriteUInt16(requiredReputationId);
                    record.HotfixContent.WriteUInt16(requiredSkillRank);
                    record.HotfixContent.WriteUInt16(requiredSkill);
                    record.HotfixContent.WriteUInt16(itemLevel);
                    record.HotfixContent.WriteInt16(allowableClass);
                    record.HotfixContent.WriteUInt16(itemRandomSuffixGroupId);
                    record.HotfixContent.WriteUInt16(randomProperty);
                    record.HotfixContent.WriteUInt16(damageMin1);
                    record.HotfixContent.WriteUInt16(damageMin2);
                    record.HotfixContent.WriteUInt16(damageMin3);
                    record.HotfixContent.WriteUInt16(damageMin4);
                    record.HotfixContent.WriteUInt16(damageMin5);
                    record.HotfixContent.WriteUInt16(damageMax1);
                    record.HotfixContent.WriteUInt16(damageMax2);
                    record.HotfixContent.WriteUInt16(damageMax3);
                    record.HotfixContent.WriteUInt16(damageMax4);
                    record.HotfixContent.WriteUInt16(damageMax5);
                    record.HotfixContent.WriteInt16(armor);
                    record.HotfixContent.WriteInt16(holyResistance);
                    record.HotfixContent.WriteInt16(fireResistance);
                    record.HotfixContent.WriteInt16(natureResistance);
                    record.HotfixContent.WriteInt16(frostResistance);
                    record.HotfixContent.WriteInt16(shadowResistance);
                    record.HotfixContent.WriteInt16(arcaneResistance);
                    record.HotfixContent.WriteUInt16(scalingStatDistributionId);
                    record.HotfixContent.WriteUInt8(expansionId);
                    record.HotfixContent.WriteUInt8(artifactId);
                    record.HotfixContent.WriteUInt8(spellWeight);
                    record.HotfixContent.WriteUInt8(spellWeightCategory);
                    record.HotfixContent.WriteUInt8(socketType1);
                    record.HotfixContent.WriteUInt8(socketType2);
                    record.HotfixContent.WriteUInt8(socketType3);
                    record.HotfixContent.WriteUInt8(sheatheType);
                    record.HotfixContent.WriteUInt8(material);
                    record.HotfixContent.WriteUInt8(pageMaterial);
                    record.HotfixContent.WriteUInt8(pageLanguage);
                    record.HotfixContent.WriteUInt8(bonding);
                    record.HotfixContent.WriteUInt8(damageType);
                    record.HotfixContent.WriteInt8(statType1);
                    record.HotfixContent.WriteInt8(statType2);
                    record.HotfixContent.WriteInt8(statType3);
                    record.HotfixContent.WriteInt8(statType4);
                    record.HotfixContent.WriteInt8(statType5);
                    record.HotfixContent.WriteInt8(statType6);
                    record.HotfixContent.WriteInt8(statType7);
                    record.HotfixContent.WriteInt8(statType8);
                    record.HotfixContent.WriteInt8(statType9);
                    record.HotfixContent.WriteInt8(statType10);
                    record.HotfixContent.WriteUInt8(containerSlots);
                    record.HotfixContent.WriteUInt8(requiredReputationRank);
                    record.HotfixContent.WriteUInt8(requiredCityRank);
                    record.HotfixContent.WriteUInt8(requiredHonorRank);
                    record.HotfixContent.WriteUInt8(inventoryType);
                    record.HotfixContent.WriteUInt8(overallQualityId);
                    record.HotfixContent.WriteUInt8(ammoType);
                    record.HotfixContent.WriteInt8(statValue1);
                    record.HotfixContent.WriteInt8(statValue2);
                    record.HotfixContent.WriteInt8(statValue3);
                    record.HotfixContent.WriteInt8(statValue4);
                    record.HotfixContent.WriteInt8(statValue5);
                    record.HotfixContent.WriteInt8(statValue6);
                    record.HotfixContent.WriteInt8(statValue7);
                    record.HotfixContent.WriteInt8(statValue8);
                    record.HotfixContent.WriteInt8(statValue9);
                    record.HotfixContent.WriteInt8(statValue10);
                    record.HotfixContent.WriteInt8(requiredLevel);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadCreatureDisplayInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfo{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    ushort modelId = ushort.Parse(fields[1]);
                    ushort soundId = ushort.Parse(fields[2]);
                    sbyte sizeClass = sbyte.Parse(fields[3]);
                    float creatureModelScale = float.Parse(fields[4]);
                    byte creatureModelAlpha = byte.Parse(fields[5]);
                    byte bloodId = byte.Parse(fields[6]);
                    int extendedDisplayInfoId = int.Parse(fields[7]);
                    ushort nPCSoundId = ushort.Parse(fields[8]);
                    ushort particleColorId = ushort.Parse(fields[9]);
                    int portraitCreatureDisplayInfoId = int.Parse(fields[10]);
                    int portraitTextureFileDataId = int.Parse(fields[11]);
                    ushort objectEffectPackageId = ushort.Parse(fields[12]);
                    ushort animReplacementSetId = ushort.Parse(fields[13]);
                    byte flags = byte.Parse(fields[14]);
                    int stateSpellVisualKitId = int.Parse(fields[15]);
                    float playerOverrideScale = float.Parse(fields[16]);
                    float petInstanceScale = float.Parse(fields[17]);
                    sbyte unarmedWeaponType = sbyte.Parse(fields[18]);
                    int mountPoofSpellVisualKitId = int.Parse(fields[19]);
                    int dissolveEffectId = int.Parse(fields[20]);
                    sbyte gender = sbyte.Parse(fields[21]);
                    int dissolveOutEffectId = int.Parse(fields[22]);
                    sbyte creatureModelMinLod = sbyte.Parse(fields[23]);
                    int textureVariationFileDataId1 = int.Parse(fields[24]);
                    int textureVariationFileDataId2 = int.Parse(fields[25]);
                    int textureVariationFileDataId3 = int.Parse(fields[26]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.CreatureDisplayInfo,
                        HotfixId = HotfixCreatureDisplayInfoBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt16(modelId);
                    record.HotfixContent.WriteUInt16(soundId);
                    record.HotfixContent.WriteInt8(sizeClass);
                    record.HotfixContent.WriteFloat(creatureModelScale);
                    record.HotfixContent.WriteUInt8(creatureModelAlpha);
                    record.HotfixContent.WriteUInt8(bloodId);
                    record.HotfixContent.WriteInt32(extendedDisplayInfoId);
                    record.HotfixContent.WriteUInt16(nPCSoundId);
                    record.HotfixContent.WriteUInt16(particleColorId);
                    record.HotfixContent.WriteInt32(portraitCreatureDisplayInfoId);
                    record.HotfixContent.WriteInt32(portraitTextureFileDataId);
                    record.HotfixContent.WriteUInt16(objectEffectPackageId);
                    record.HotfixContent.WriteUInt16(animReplacementSetId);
                    record.HotfixContent.WriteUInt8(flags);
                    record.HotfixContent.WriteInt32(stateSpellVisualKitId);
                    record.HotfixContent.WriteFloat(playerOverrideScale);
                    record.HotfixContent.WriteFloat(petInstanceScale);
                    record.HotfixContent.WriteInt8(unarmedWeaponType);
                    record.HotfixContent.WriteInt32(mountPoofSpellVisualKitId);
                    record.HotfixContent.WriteInt32(dissolveEffectId);
                    record.HotfixContent.WriteInt8(gender);
                    record.HotfixContent.WriteInt32(dissolveOutEffectId);
                    record.HotfixContent.WriteInt8(creatureModelMinLod);
                    record.HotfixContent.WriteInt32(textureVariationFileDataId1);
                    record.HotfixContent.WriteInt32(textureVariationFileDataId2);
                    record.HotfixContent.WriteInt32(textureVariationFileDataId3);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadCreatureDisplayInfoExtraHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoExtra{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    sbyte displayRaceId = sbyte.Parse(fields[1]);
                    sbyte displaySexId = sbyte.Parse(fields[2]);
                    sbyte displayClassId = sbyte.Parse(fields[3]);
                    sbyte skinId = sbyte.Parse(fields[4]);
                    sbyte faceId = sbyte.Parse(fields[5]);
                    sbyte hairStyleId = sbyte.Parse(fields[6]);
                    sbyte hairColorId = sbyte.Parse(fields[7]);
                    sbyte facialHairId = sbyte.Parse(fields[8]);
                    sbyte flags = sbyte.Parse(fields[9]);
                    int bakeMaterialResourcesId = int.Parse(fields[10]);
                    int hDBakeMaterialResourcesId = int.Parse(fields[11]);
                    byte customDisplayOption1 = byte.Parse(fields[12]);
                    byte customDisplayOption2 = byte.Parse(fields[13]);
                    byte customDisplayOption3 = byte.Parse(fields[14]);

                    HotfixRecord record = new()
                    {
                        TableHash = DB2Hash.CreatureDisplayInfoExtra,
                        HotfixId = HotfixCreatureDisplayInfoExtraBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteInt8(displayRaceId);
                    record.HotfixContent.WriteInt8(displaySexId);
                    record.HotfixContent.WriteInt8(displayClassId);
                    record.HotfixContent.WriteInt8(skinId);
                    record.HotfixContent.WriteInt8(faceId);
                    record.HotfixContent.WriteInt8(hairStyleId);
                    record.HotfixContent.WriteInt8(hairColorId);
                    record.HotfixContent.WriteInt8(facialHairId);
                    record.HotfixContent.WriteInt8(flags);
                    record.HotfixContent.WriteInt32(bakeMaterialResourcesId);
                    record.HotfixContent.WriteInt32(hDBakeMaterialResourcesId);
                    record.HotfixContent.WriteUInt8(customDisplayOption1);
                    record.HotfixContent.WriteUInt8(customDisplayOption2);
                    record.HotfixContent.WriteUInt8(customDisplayOption3);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadCreatureDisplayInfoOptionHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoOption{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = uint.Parse(fields[0]);
                    int chrCustomizationOptionId = int.Parse(fields[1]);
                    int chrCustomizationChoiceId = int.Parse(fields[2]);
                    int creatureDisplayInfoExtraId = int.Parse(fields[3]);

                    HotfixRecord record = new()
                    {
                        Status = HotfixStatus.Valid,
                        TableHash = DB2Hash.CreatureDisplayInfoOption,
                        HotfixId = HotfixCreatureDisplayInfoOptionBegin + counter
                    };
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteInt32(chrCustomizationOptionId);
                    record.HotfixContent.WriteInt32(chrCustomizationChoiceId);
                    record.HotfixContent.WriteInt32(creatureDisplayInfoExtraId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        #endregion
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
    public class ItemDisplayData
    {
        public uint Entry;
        public uint DisplayId;
        public byte InventoryType;
    }
    public class Battleground
    {
        public bool IsArena;
        public List<uint> MapIds = new();
    }
    public class TaxiPath
    {
        public uint Id;
        public uint From;
        public uint To;
        public int Cost;
    }
    public class TaxiNode
    {
        public uint Id;
        public uint mapId;
        public float x, y, z;
    }
    public class TaxiPathNode
    {
        public uint Id;
        public uint pathId;
        public uint nodeIndex;
        public uint mapId;
        public float x, y, z;
        public uint flags;
        public uint delay;
    }
    public class ChatChannel
    {
        public uint Id;
        public ChannelFlags Flags;
        public string Name;
    }

    // Hotfix structures
    public class AreaTrigger
    {
        public string Message;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public uint Id;
        public ushort MapId;
        public byte PhaseUseFlags;
        public ushort PhaseId;
        public ushort PhaseGroupId;
        public float Radius;
        public float BoxLength;
        public float BoxWidth;
        public float BoxHeight;
        public float BoxYaw;
        public byte ShapeType;
        public ushort ShapeId;
        public ushort ActionSetId;
        public byte Flags;
    }
}
