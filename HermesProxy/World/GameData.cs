using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World
{
    public static class GameData
    {
        // From CSV
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new SortedDictionary<uint, BroadcastText>();
        public static Dictionary<uint, ItemTemplate> ItemTemplateStore = new Dictionary<uint, ItemTemplate>();
        public static Dictionary<uint, Battleground> Battlegrounds = new Dictionary<uint, Battleground>();
        public static Dictionary<uint, uint> SpellVisuals = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> LearnSpells = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> Gems = new Dictionary<uint, uint>();
        public static Dictionary<uint, float> UnitDisplayScales = new Dictionary<uint, float>();
        public static Dictionary<uint, uint> TransportPeriods = new Dictionary<uint, uint>();
        public static HashSet<uint> DispellSpells = new HashSet<uint>();
        public static HashSet<uint> StackableAuras = new HashSet<uint>();

        // From Server
        public static Dictionary<uint, CreatureTemplate> CreatureTemplates = new Dictionary<uint, CreatureTemplate>();
        public static Dictionary<uint, QuestTemplate> QuestTemplates = new Dictionary<uint, QuestTemplate>();
        public static Dictionary<uint, string> ItemNames = new Dictionary<uint, string>();

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

        public static uint GetTransportPeriod(uint entry)
        {
            uint period;
            if (TransportPeriods.TryGetValue(entry, out period))
                return period;
            return 0;
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
            Battleground bg;
            if (Battlegrounds.TryGetValue(bgId, out bg))
                return bg.MapIds[0];
            return 0;
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
        #endregion
        #region Loading
        // Loading code
        public static void LoadEverything()
        {
            Log.Print(LogType.Storage, "Loading data files...");
            LoadBroadcastTexts();
            LoadItemTemplates();
            LoadBattlegrounds();
            LoadSpellVisuals();
            LoadLearnSpells();
            LoadGems();
            LoadUnitDisplayScales();
            LoadTransports();
            LoadDispellSpells();
            LoadStackableAuras();
            LoadHotfixes();
            Log.Print(LogType.Storage, "Finished loading data.");
        }

        public static void LoadBroadcastTexts()
        {
            var path = Path.Combine("CSV", $"BroadcastTexts{LegacyVersion.GetExpansionVersion()}.csv");
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
            var path = Path.Combine("CSV", $"Items{ModernVersion.GetExpansionVersion()}.csv");
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

        public static void LoadBattlegrounds()
        {
            var path = Path.Combine("CSV", "Battlegrounds.csv");
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

                    Battleground bg = new Battleground();
                    uint bgId = UInt32.Parse(fields[0]);
                    bg.IsArena = Byte.Parse(fields[1]) != 0;
                    for (int i = 0; i < 6; i++)
                    {
                        uint mapId = UInt32.Parse(fields[2 + i]);
                        if (mapId != 0)
                            bg.MapIds.Add(mapId);
                    }
                    System.Diagnostics.Trace.Assert(bg.MapIds.Count != 0);
                    Battlegrounds.Add(bgId, bg);
                }
            }
        }

        public static void LoadSpellVisuals()
        {
            var path = Path.Combine("CSV", $"SpellVisuals{ModernVersion.GetExpansionVersion()}.csv");
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
            var path = Path.Combine("CSV", "LearnSpells.csv");
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

            var path = Path.Combine("CSV", $"Gems{ModernVersion.GetExpansionVersion()}.csv");
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

            var path = Path.Combine("CSV", "UnitDisplayScales.csv");
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

        public static void LoadTransports()
        {
            var path = Path.Combine("CSV", $"Transports{LegacyVersion.GetExpansionVersion()}.csv");
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

                    uint entry = UInt32.Parse(fields[0]);
                    uint period = UInt32.Parse(fields[1]);
                    TransportPeriods.Add(entry, period);
                }
            }
        }

        public static void LoadDispellSpells()
        {
            if (LegacyVersion.GetExpansionVersion() > 1)
                return;

            var path = Path.Combine("CSV", "DispellSpells.csv");
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
                    DispellSpells.Add(spellId);
                }
            }
        }

        public static void LoadStackableAuras()
        {
            if (LegacyVersion.GetExpansionVersion() > 2)
                return;

            var path = Path.Combine("CSV", $"StackableAuras{LegacyVersion.GetExpansionVersion()}.csv");
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
                    StackableAuras.Add(spellId);
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
        public const uint HotfixItemSparseBegin = 200000;
        public const uint HotfixCreatureDisplayInfoBegin = 210000;
        public const uint HotfixCreatureDisplayInfoExtraBegin = 220000;
        public const uint HotfixCreatureDisplayInfoOptionBegin = 230000;
        public static Dictionary<uint, HotfixRecord> Hotfixes = new Dictionary<uint, HotfixRecord>();
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
            LoadItemSparseHotfixes();
            LoadCreatureDisplayInfoHotfixes();
            LoadCreatureDisplayInfoExtraHotfixes();
            LoadCreatureDisplayInfoOptionHotfixes();
        }
        
        public static void LoadAreaTriggerHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"AreaTrigger{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    AreaTrigger at = new AreaTrigger();
                    at.Message = fields[0];
                    at.PositionX = float.Parse(fields[1]);
                    at.PositionY = float.Parse(fields[2]);
                    at.PositionZ = float.Parse(fields[3]);
                    at.Id = UInt32.Parse(fields[4]);
                    at.MapId = UInt16.Parse(fields[5]);
                    at.PhaseUseFlags = Byte.Parse(fields[6]);
                    at.PhaseId = UInt16.Parse(fields[7]);
                    at.PhaseGroupId = UInt16.Parse(fields[8]);
                    at.Radius = float.Parse(fields[9]);
                    at.BoxLength = float.Parse(fields[10]);
                    at.BoxWidth = float.Parse(fields[11]);
                    at.BoxHeight = float.Parse(fields[12]);
                    at.BoxYaw = float.Parse(fields[13]);
                    at.ShapeType = Byte.Parse(fields[14]);
                    at.ShapeId = UInt16.Parse(fields[15]);
                    at.ActionSetId = UInt16.Parse(fields[16]);
                    at.Flags = Byte.Parse(fields[17]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.AreaTrigger;
                    record.HotfixId = HotfixAreaTriggerBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SkillLine{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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
                    uint id = UInt32.Parse(fields[5]);
                    byte categoryID = Byte.Parse(fields[6]);
                    uint spellIconFileID = UInt32.Parse(fields[7]);
                    byte canLink = Byte.Parse(fields[8]);
                    uint parentSkillLineID = UInt32.Parse(fields[9]);
                    uint parentTierIndex = UInt32.Parse(fields[10]);
                    ushort flags = UInt16.Parse(fields[11]);
                    uint spellBookSpellID = UInt32.Parse(fields[12]);
                    
                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SkillLine;
                    record.HotfixId = HotfixSkillLineBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SkillRaceClassInfo{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    ulong raceMask = UInt64.Parse(fields[1]);
                    ushort skillId = UInt16.Parse(fields[2]);
                    uint classMask = UInt32.Parse(fields[3]);
                    ushort flags = UInt16.Parse(fields[4]);
                    byte availability = Byte.Parse(fields[5]);
                    byte minLevel = Byte.Parse(fields[6]);
                    ushort skillTierId = UInt16.Parse(fields[7]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SkillRaceClassInfo;
                    record.HotfixId = HotfixSkillRaceClassInfoBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SkillLineAbility{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    ulong raceMask = UInt64.Parse(fields[0]);
                    uint id = UInt32.Parse(fields[1]);
                    ushort skillId = UInt16.Parse(fields[2]);
                    uint spellId = UInt32.Parse(fields[3]);
                    ushort minSkillLineRank = UInt16.Parse(fields[4]);
                    uint classMask = UInt32.Parse(fields[5]);
                    uint supercedesSpellId = UInt32.Parse(fields[6]);
                    byte acquireMethod = Byte.Parse(fields[7]);
                    ushort trivialSkillLineRankHigh = UInt16.Parse(fields[8]);
                    ushort trivialSkillLineRankLow = UInt16.Parse(fields[9]);
                    byte flags = Byte.Parse(fields[10]);
                    byte numSkillUps = Byte.Parse(fields[11]);
                    ushort uniqueBit = UInt16.Parse(fields[12]);
                    ushort tradeSkillCategoryId = UInt16.Parse(fields[13]);
                    ushort skillUpSkillLineId = UInt16.Parse(fields[14]);
                    uint characterPoints1 = UInt32.Parse(fields[15]);
                    uint characterPoints2 = UInt32.Parse(fields[16]);


                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SkillLineAbility;
                    record.HotfixId = HotfixSkillLineAbilityBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"Spell{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    string nameSubText = fields[1];
                    string description = fields[2];
                    string auraDescription = fields[3];

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.Spell;
                    record.HotfixId = HotfixSpellBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellName{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    string name = fields[1];

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellName;
                    record.HotfixId = HotfixSpellNameBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellLevels{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    ushort baseLevel = UInt16.Parse(fields[2]);
                    ushort maxLevel = UInt16.Parse(fields[3]);
                    ushort spellLevel = UInt16.Parse(fields[4]);
                    byte maxPassiveAuraLevel = Byte.Parse(fields[5]);
                    uint spellId = UInt32.Parse(fields[6]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellLevels;
                    record.HotfixId = HotfixSpellLevelsBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellAuraOptions{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    uint cumulatievAura = UInt32.Parse(fields[2]);
                    uint procCategoryRecovery = UInt32.Parse(fields[3]);
                    byte procChance = Byte.Parse(fields[4]);
                    uint procCharges = UInt32.Parse(fields[5]);
                    ushort spellProcsPerMinuteId = UInt16.Parse(fields[6]);
                    uint procTypeMask0 = UInt32.Parse(fields[7]);
                    uint procTypeMask1 = UInt32.Parse(fields[8]);
                    uint spellId = UInt32.Parse(fields[9]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellAuraOptions;
                    record.HotfixId = HotfixSpellAuraOptionsBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellMisc{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    ushort castingTimeIndex = UInt16.Parse(fields[2]);
                    ushort durationIndex = UInt16.Parse(fields[3]);
                    ushort rangeIndex = UInt16.Parse(fields[4]);
                    byte schoolMask = Byte.Parse(fields[5]);
                    float speed = Single.Parse(fields[6]);
                    float launchDelay = Single.Parse(fields[7]);
                    float minDuration = Single.Parse(fields[8]);
                    uint spellIconFileDataId = UInt32.Parse(fields[9]);
                    uint activeIconFileDataId = UInt32.Parse(fields[10]);
                    uint attributes1 = UInt32.Parse(fields[11]);
                    uint attributes2 = UInt32.Parse(fields[12]);
                    uint attributes3 = UInt32.Parse(fields[13]);
                    uint attributes4 = UInt32.Parse(fields[14]);
                    uint attributes5 = UInt32.Parse(fields[15]);
                    uint attributes6 = UInt32.Parse(fields[16]);
                    uint attributes7 = UInt32.Parse(fields[17]);
                    uint attributes8 = UInt32.Parse(fields[18]);
                    uint attributes9 = UInt32.Parse(fields[19]);
                    uint attributes10 = UInt32.Parse(fields[20]);
                    uint attributes11 = UInt32.Parse(fields[21]);
                    uint attributes12 = UInt32.Parse(fields[22]);
                    uint attributes13 = UInt32.Parse(fields[23]);
                    uint attributes14 = UInt32.Parse(fields[24]);
                    uint spellId = UInt32.Parse(fields[25]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellMisc;
                    record.HotfixId = HotfixSpellMiscBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellEffect{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    uint difficultyId = UInt32.Parse(fields[1]);
                    uint effectIndex = UInt32.Parse(fields[2]);
                    uint effect = UInt32.Parse(fields[3]);
                    float effectAmplitude = Single.Parse(fields[4]);
                    uint effectAttributes = UInt32.Parse(fields[5]);
                    short effectAura = Int16.Parse(fields[6]);
                    int effectAuraPeriod = Int32.Parse(fields[7]);
                    int effectBasePoints = Int32.Parse(fields[8]);
                    float effectBonusCoefficient = Single.Parse(fields[9]);
                    float effectChainAmplitude = Single.Parse(fields[10]);
                    int effectChainTargets = Int32.Parse(fields[11]);
                    int effectDieSides = Int32.Parse(fields[12]);
                    int effectItemType = Int32.Parse(fields[13]);
                    int effectMechanic = Int32.Parse(fields[14]);
                    float effectPointsPerResource = Single.Parse(fields[15]);
                    float effectPosFacing = Single.Parse(fields[16]);
                    float effectRealPointsPerLevel = Single.Parse(fields[17]);
                    int EffectTriggerSpell = Int32.Parse(fields[18]);
                    float bonusCoefficientFromAP = Single.Parse(fields[19]);
                    float pvpMultiplier = Single.Parse(fields[20]);
                    float coefficient = Single.Parse(fields[21]);
                    float variance = Single.Parse(fields[22]);
                    float resourceCoefficient = Single.Parse(fields[23]);
                    float groupSizeBasePointsCoefficient = Single.Parse(fields[24]);
                    int effectMiscValue1 = Int32.Parse(fields[25]);
                    int effectMiscValue2 = Int32.Parse(fields[26]);
                    uint effectRadiusIndex1 = UInt32.Parse(fields[27]);
                    uint effectRadiusIndex2 = UInt32.Parse(fields[28]);
                    int effectSpellClassMask1 = Int32.Parse(fields[29]);
                    int effectSpellClassMask2 = Int32.Parse(fields[30]);
                    int effectSpellClassMask3 = Int32.Parse(fields[31]);
                    int effectSpellClassMask4 = Int32.Parse(fields[32]);
                    short implicitTarget1 = Int16.Parse(fields[33]);
                    short implicitTarget2 = Int16.Parse(fields[34]);
                    uint spellId = UInt32.Parse(fields[35]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellEffect;
                    record.HotfixId = HotfixSpellEffectBegin + counter;
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
        public static void LoadItemSparseHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemSparse{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    long allowableRace = Int64.Parse(fields[1]);
                    string description = fields[2];
                    string name4 = fields[3];
                    string name3 = fields[4];
                    string name2 = fields[5];
                    string name1 = fields[6];
                    float dmgVariance = Single.Parse(fields[7]);
                    uint durationInInventory = UInt32.Parse(fields[8]);
                    float qualityModifier = Single.Parse(fields[9]);
                    uint bagFamily = UInt32.Parse(fields[10]);
                    float rangeMod = Single.Parse(fields[11]);
                    float statPercentageOfSocket1 = Single.Parse(fields[12]);
                    float statPercentageOfSocket2 = Single.Parse(fields[13]);
                    float statPercentageOfSocket3 = Single.Parse(fields[14]);
                    float statPercentageOfSocket4 = Single.Parse(fields[15]);
                    float statPercentageOfSocket5 = Single.Parse(fields[16]);
                    float statPercentageOfSocket6 = Single.Parse(fields[17]);
                    float statPercentageOfSocket7 = Single.Parse(fields[18]);
                    float statPercentageOfSocket8 = Single.Parse(fields[19]);
                    float statPercentageOfSocket9 = Single.Parse(fields[20]);
                    float statPercentageOfSocket10 = Single.Parse(fields[21]);
                    int statPercentEditor1 = Int32.Parse(fields[22]);
                    int statPercentEditor2 = Int32.Parse(fields[23]);
                    int statPercentEditor3 = Int32.Parse(fields[24]);
                    int statPercentEditor4 = Int32.Parse(fields[25]);
                    int statPercentEditor5 = Int32.Parse(fields[26]);
                    int statPercentEditor6 = Int32.Parse(fields[27]);
                    int statPercentEditor7 = Int32.Parse(fields[28]);
                    int statPercentEditor8 = Int32.Parse(fields[29]);
                    int statPercentEditor9 = Int32.Parse(fields[30]);
                    int statPercentEditor10 = Int32.Parse(fields[31]);
                    int stackable = Int32.Parse(fields[32]);
                    int maxCount = Int32.Parse(fields[33]);
                    uint requiredAbility = UInt32.Parse(fields[34]);
                    uint sellPrice = UInt32.Parse(fields[35]);
                    uint buyPrice = UInt32.Parse(fields[36]);
                    uint vendorStackCount = UInt32.Parse(fields[37]);
                    float priceVariance = Single.Parse(fields[38]);
                    float priceRandomValue = Single.Parse(fields[39]);
                    int flags1 = Int32.Parse(fields[40]);
                    int flags2 = Int32.Parse(fields[41]);
                    int flags3 = Int32.Parse(fields[42]);
                    int flags4 = Int32.Parse(fields[43]);
                    int oppositeFactionItemId = Int32.Parse(fields[44]);
                    uint maxDurability = UInt32.Parse(fields[45]);
                    ushort itemNameDescriptionId = UInt16.Parse(fields[46]);
                    ushort requiredTransmogHoliday = UInt16.Parse(fields[47]);
                    ushort requiredHoliday = UInt16.Parse(fields[48]);
                    ushort limitCategory = UInt16.Parse(fields[49]);
                    ushort gemProperties = UInt16.Parse(fields[50]);
                    ushort socketMatchEnchantmentId = UInt16.Parse(fields[51]);
                    ushort totemCategoryId = UInt16.Parse(fields[52]);
                    ushort instanceBound = UInt16.Parse(fields[53]);
                    ushort zoneBound1 = UInt16.Parse(fields[54]);
                    ushort zoneBound2 = UInt16.Parse(fields[55]);
                    ushort itemSet = UInt16.Parse(fields[56]);
                    ushort lockId = UInt16.Parse(fields[57]);
                    ushort startQuestId = UInt16.Parse(fields[58]);
                    ushort pageText = UInt16.Parse(fields[59]);
                    ushort delay = UInt16.Parse(fields[60]);
                    ushort requiredReputationId = UInt16.Parse(fields[61]);
                    ushort requiredSkillRank = UInt16.Parse(fields[62]);
                    ushort requiredSkill = UInt16.Parse(fields[63]);
                    ushort itemLevel = UInt16.Parse(fields[64]);
                    short allowableClass = Int16.Parse(fields[65]);
                    ushort itemRandomSuffixGroupId = UInt16.Parse(fields[66]);
                    ushort randomProperty = UInt16.Parse(fields[67]);
                    ushort damageMin1 = UInt16.Parse(fields[68]);
                    ushort damageMin2 = UInt16.Parse(fields[69]);
                    ushort damageMin3 = UInt16.Parse(fields[70]);
                    ushort damageMin4 = UInt16.Parse(fields[71]);
                    ushort damageMin5 = UInt16.Parse(fields[72]);
                    ushort damageMax1 = UInt16.Parse(fields[73]);
                    ushort damageMax2 = UInt16.Parse(fields[74]);
                    ushort damageMax3 = UInt16.Parse(fields[75]);
                    ushort damageMax4 = UInt16.Parse(fields[76]);
                    ushort damageMax5 = UInt16.Parse(fields[77]);
                    short armor = Int16.Parse(fields[78]);
                    short holyResistance = Int16.Parse(fields[79]);
                    short fireResistance = Int16.Parse(fields[80]);
                    short natureResistance = Int16.Parse(fields[81]);
                    short frostResistance = Int16.Parse(fields[82]);
                    short shadowResistance = Int16.Parse(fields[83]);
                    short arcaneResistance = Int16.Parse(fields[84]);
                    ushort scalingStatDistributionId = UInt16.Parse(fields[85]);
                    byte expansionId = Byte.Parse(fields[86]);
                    byte artifactId = Byte.Parse(fields[87]);
                    byte spellWeight = Byte.Parse(fields[88]);
                    byte spellWeightCategory = Byte.Parse(fields[89]);
                    byte socketType1 = Byte.Parse(fields[90]);
                    byte socketType2 = Byte.Parse(fields[91]);
                    byte socketType3 = Byte.Parse(fields[92]);
                    byte sheatheType = Byte.Parse(fields[93]);
                    byte material = Byte.Parse(fields[94]);
                    byte pageMaterial = Byte.Parse(fields[95]);
                    byte pageLanguage = Byte.Parse(fields[96]);
                    byte bonding = Byte.Parse(fields[97]);
                    byte damageType = Byte.Parse(fields[98]);
                    sbyte statType1 = SByte.Parse(fields[99]);
                    sbyte statType2 = SByte.Parse(fields[100]);
                    sbyte statType3 = SByte.Parse(fields[101]);
                    sbyte statType4 = SByte.Parse(fields[102]);
                    sbyte statType5 = SByte.Parse(fields[103]);
                    sbyte statType6 = SByte.Parse(fields[104]);
                    sbyte statType7 = SByte.Parse(fields[105]);
                    sbyte statType8 = SByte.Parse(fields[106]);
                    sbyte statType9 = SByte.Parse(fields[107]);
                    sbyte statType10 = SByte.Parse(fields[108]);
                    byte containerSlots = Byte.Parse(fields[109]);
                    byte requiredReputationRank = Byte.Parse(fields[110]);
                    byte requiredCityRank = Byte.Parse(fields[111]);
                    byte requiredHonorRank = Byte.Parse(fields[112]);
                    byte inventoryType = Byte.Parse(fields[113]);
                    byte overallQualityId = Byte.Parse(fields[114]);
                    byte ammoType = Byte.Parse(fields[115]);
                    sbyte statValue1 = SByte.Parse(fields[116]);
                    sbyte statValue2 = SByte.Parse(fields[117]);
                    sbyte statValue3 = SByte.Parse(fields[118]);
                    sbyte statValue4 = SByte.Parse(fields[119]);
                    sbyte statValue5 = SByte.Parse(fields[120]);
                    sbyte statValue6 = SByte.Parse(fields[121]);
                    sbyte statValue7 = SByte.Parse(fields[122]);
                    sbyte statValue8 = SByte.Parse(fields[123]);
                    sbyte statValue9 = SByte.Parse(fields[124]);
                    sbyte statValue10 = SByte.Parse(fields[125]);
                    sbyte requiredLevel = SByte.Parse(fields[126]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.ItemSparse;
                    record.HotfixId = HotfixItemSparseBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfo{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    ushort modelId = UInt16.Parse(fields[1]);
                    ushort soundId = UInt16.Parse(fields[2]);
                    sbyte sizeClass = SByte.Parse(fields[3]);
                    float creatureModelScale = Single.Parse(fields[4]);
                    byte creatureModelAlpha = Byte.Parse(fields[5]);
                    byte bloodId = Byte.Parse(fields[6]);
                    int extendedDisplayInfoId = Int32.Parse(fields[7]);
                    ushort nPCSoundId = UInt16.Parse(fields[8]);
                    ushort particleColorId = UInt16.Parse(fields[9]);
                    int portraitCreatureDisplayInfoId = Int32.Parse(fields[10]);
                    int portraitTextureFileDataId = Int32.Parse(fields[11]);
                    ushort objectEffectPackageId = UInt16.Parse(fields[12]);
                    ushort animReplacementSetId = UInt16.Parse(fields[13]);
                    byte flags = Byte.Parse(fields[14]);
                    int stateSpellVisualKitId = Int32.Parse(fields[15]);
                    float playerOverrideScale = Single.Parse(fields[16]);
                    float petInstanceScale = Single.Parse(fields[17]);
                    sbyte unarmedWeaponType = SByte.Parse(fields[18]);
                    int mountPoofSpellVisualKitId = Int32.Parse(fields[19]);
                    int dissolveEffectId = Int32.Parse(fields[20]);
                    sbyte gender = SByte.Parse(fields[21]);
                    int dissolveOutEffectId = Int32.Parse(fields[22]);
                    sbyte creatureModelMinLod = SByte.Parse(fields[23]);
                    int textureVariationFileDataId1 = Int32.Parse(fields[24]);
                    int textureVariationFileDataId2 = Int32.Parse(fields[25]);
                    int textureVariationFileDataId3 = Int32.Parse(fields[26]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.CreatureDisplayInfo;
                    record.HotfixId = HotfixCreatureDisplayInfoBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoExtra{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    sbyte displayRaceId = SByte.Parse(fields[1]);
                    sbyte displaySexId = SByte.Parse(fields[2]);
                    sbyte displayClassId = SByte.Parse(fields[3]);
                    sbyte skinId = SByte.Parse(fields[4]);
                    sbyte faceId = SByte.Parse(fields[5]);
                    sbyte hairStyleId = SByte.Parse(fields[6]);
                    sbyte hairColorId = SByte.Parse(fields[7]);
                    sbyte facialHairId = SByte.Parse(fields[8]);
                    sbyte flags = SByte.Parse(fields[9]);
                    int bakeMaterialResourcesId = Int32.Parse(fields[10]);
                    int hDBakeMaterialResourcesId = Int32.Parse(fields[11]);
                    byte customDisplayOption1 = Byte.Parse(fields[12]);
                    byte customDisplayOption2 = Byte.Parse(fields[13]);
                    byte customDisplayOption3 = Byte.Parse(fields[14]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.CreatureDisplayInfoExtra;
                    record.HotfixId = HotfixCreatureDisplayInfoExtraBegin + counter;
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
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoOption{ModernVersion.GetExpansionVersion()}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
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

                    uint id = UInt32.Parse(fields[0]);
                    int chrCustomizationOptionId = Int32.Parse(fields[1]);
                    int chrCustomizationChoiceId = Int32.Parse(fields[2]);
                    int creatureDisplayInfoExtraId = Int32.Parse(fields[3]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.CreatureDisplayInfoOption;
                    record.HotfixId = HotfixCreatureDisplayInfoOptionBegin + counter;
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
    public class ItemTemplate
    {
        public uint Entry;
        public uint DisplayId;
        public byte InventoryType;
    }
    public class Battleground
    {
        public bool IsArena;
        public List<uint> MapIds = new List<uint>();
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
