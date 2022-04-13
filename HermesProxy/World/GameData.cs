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
