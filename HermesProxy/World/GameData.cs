using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World
{
    public static partial class GameData
    {
        // From CSV
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new SortedDictionary<uint, BroadcastText>();
        public static Dictionary<uint, uint> ItemDisplayIdStore = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> ItemDisplayIdToFileDataIdStore = new Dictionary<uint, uint>();
        public static Dictionary<uint, ItemSpellsData> ItemSpellsDataStore = new Dictionary<uint, ItemSpellsData>();
        public static Dictionary<uint, ItemRecord> ItemRecordsStore = new Dictionary<uint, ItemRecord>();
        public static Dictionary<uint, ItemSparseRecord> ItemSparseRecordsStore = new Dictionary<uint, ItemSparseRecord>();
        public static Dictionary<uint, ItemAppearance> ItemAppearanceStore = new Dictionary<uint, ItemAppearance>();
        public static Dictionary<uint, ItemModifiedAppearance> ItemModifiedAppearanceStore = new Dictionary<uint, ItemModifiedAppearance>();
        public static Dictionary<uint, ItemEffect> ItemEffectStore = new Dictionary<uint, ItemEffect>();
        public static Dictionary<uint, Battleground> Battlegrounds = new Dictionary<uint, Battleground>();
        public static Dictionary<uint, ChatChannel> ChatChannels = new Dictionary<uint, ChatChannel>();
        public static Dictionary<uint, Dictionary<uint, byte>> ItemEffects = new Dictionary<uint, Dictionary<uint, byte>>();
        public static Dictionary<uint, uint> ItemEnchantVisuals = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> SpellVisuals = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> LearnSpells = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> TotemSpells = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> Gems = new Dictionary<uint, uint>();
        public static Dictionary<uint, CreatureDisplayInfo> CreatureDisplayInfos = new Dictionary<uint, CreatureDisplayInfo>();
        public static Dictionary<uint, CreatureModelCollisionHeight> CreatureModelCollisionHeights = new Dictionary<uint, CreatureModelCollisionHeight>();
        public static Dictionary<uint, uint> TransportPeriods = new Dictionary<uint, uint>();
        public static Dictionary<uint, string> AreaNames = new Dictionary<uint, string>();
        public static Dictionary<uint, uint> RaceFaction = new Dictionary<uint, uint>();
        public static HashSet<uint> DispellSpells = new HashSet<uint>();
        public static Dictionary<uint, List<float>> SpellEffectPoints = new();
        public static HashSet<uint> StackableAuras = new HashSet<uint>();
        public static HashSet<uint> MountAuras = new HashSet<uint>();
        public static HashSet<uint> NextMeleeSpells = new HashSet<uint>();
        public static HashSet<uint> AutoRepeatSpells = new HashSet<uint>();
        public static HashSet<uint> AuraSpells = new HashSet<uint>();
        public static Dictionary<uint, TaxiPath> TaxiPaths = new Dictionary<uint, TaxiPath>();
        public static int[,] TaxiNodesGraph = new int[250,250];
        public static Dictionary<uint /*questId*/, uint /*questBit*/> QuestBits = new Dictionary<uint, uint>();

        // From Server
        public static Dictionary<uint, ItemTemplate> ItemTemplates = new Dictionary<uint, ItemTemplate>();
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
            ItemTemplate data;
            if (ItemTemplates.TryGetValue(entry, out data))
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
            CreatureTemplate data;
            if (CreatureTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static uint GetItemDisplayId(uint entry)
        {
            uint displayId;
            if (ItemDisplayIdStore.TryGetValue(entry, out displayId))
                return displayId;
            return 0;
        }

        public static uint GetItemIdWithDisplayId(uint displayId)
        {
            foreach (var item in ItemDisplayIdStore)
            {
                if (item.Value == displayId)
                    return item.Key;
            }
            return 0;
        }

        public static ItemAppearance GetItemAppearanceByDisplayId(uint displayId)
        {
            foreach (var item in ItemAppearanceStore)
            {
                if (item.Value.ItemDisplayInfoID == (int)displayId)
                    return item.Value;
            }
            return null;
        }

        public static ItemAppearance GetItemAppearanceByItemId(uint itemId)
        {
            ItemModifiedAppearance modAppearance = GetItemModifiedAppearanceByItemId(itemId);
            if (modAppearance == null)
                return null;

            ItemAppearance data;
            if (ItemAppearanceStore.TryGetValue((uint)modAppearance.ItemAppearanceID, out data))
                return data;
            return null;
        }

        public static uint GetItemIconFileDataIdByDisplayId(uint displayId)
        {
            uint fileDataId;
            if (ItemDisplayIdToFileDataIdStore.TryGetValue(displayId, out fileDataId))
                return fileDataId;
            return 0;
        }

        public static ItemModifiedAppearance GetItemModifiedAppearanceByDisplayId(uint displayId)
        {
            ItemAppearance appearance = GetItemAppearanceByDisplayId(displayId);
            if (appearance != null)
            {
                foreach (var item in ItemModifiedAppearanceStore)
                {
                    if (item.Value.ItemAppearanceID == appearance.Id)
                        return item.Value;
                }
            }
            return null;
        }

        public static ItemModifiedAppearance GetItemModifiedAppearanceByItemId(uint itemId)
        {
            foreach (var item in ItemModifiedAppearanceStore)
            {
                if (item.Value.ItemID == (int)itemId)
                    return item.Value;
            }
            return null;
        }

        public static ItemEffect GetItemEffectByItemId(uint itemId, byte slot)
        {
            foreach (var item in ItemEffectStore)
            {
                if (item.Value.ParentItemID == itemId && item.Value.LegacySlotIndex == slot)
                    return item.Value;
            }
            return null;
        }

        public static uint GetFirstFreeId(System.Collections.IDictionary dict, uint after = 0)
        {
            uint firstEntry = 0;
            foreach (var item in dict)
            {
                var type = item.GetType();
                var key = type.GetProperty("Key");
                var keyObj = key.GetValue(item, null);

                if (after > 0 && (uint)keyObj <= after)
                    continue;

                firstEntry = (uint)keyObj;
                break;
            }
            while (dict.Contains(firstEntry))
                firstEntry++;

            return firstEntry;
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
                Dictionary<uint, byte> dict = new Dictionary<uint, byte>();
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
            uint visualId;
            if (ItemEnchantVisuals.TryGetValue(enchantId, out visualId))
                return visualId;
            return 0;
        }

        public static uint GetSpellVisual(uint spellId)
        {
            uint visual;
            if (SpellVisuals.TryGetValue(spellId, out visual))
                return visual;
            return 0;
        }

        public static int GetTotemSlotForSpell(uint spellId)
        {
            uint slot;
            if (TotemSpells.TryGetValue(spellId, out slot))
                return (int)slot;
            return -1;
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

        public static float GetUnitCompleteDisplayScale(uint displayId)
        {
            var displayData = GetDisplayInfo(displayId);
            if (displayData.ModelId == 0)
                return 1.0f;

            var modelData = GetModelData(displayId);
            return displayData.DisplayScale * modelData.ModelScale;
        }
        
        public static CreatureDisplayInfo GetDisplayInfo(uint displayId)
        {
            if (CreatureDisplayInfos.TryGetValue(displayId, out var info))
                return info;
            return new CreatureDisplayInfo(0, 1.0f);
        }

        public static CreatureModelCollisionHeight GetModelData(uint modelId)
        {
            if (CreatureModelCollisionHeights.TryGetValue(modelId, out var info))
                return info;
            return new CreatureModelCollisionHeight(1.0f, 0, 0);
        }

        public static uint GetTransportPeriod(uint entry)
        {
            uint period;
            if (TransportPeriods.TryGetValue(entry, out period))
                return period;
            return 0;
        }

        public static string GetAreaName(uint id)
        {
            string name;
            if (AreaNames.TryGetValue(id, out name))
                return name;
            return "";
        }

        public static uint GetFactionForRace(uint race)
        {
            uint faction;
            if (RaceFaction.TryGetValue(race, out faction))
                return faction;
            return 1;
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
            List<ChatChannel> channels = new List<ChatChannel>();
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

        public static bool IsHordeRace(Race raceId)
        {
            switch (raceId)
            {
                case Race.Orc:
                case Race.Undead:
                case Race.Tauren:
                case Race.Troll:
                case Race.BloodElf:
                case Race.Goblin:
                    return true;
            }
            return false;
        }

        /// returns 0 when unknown
        public static int GetFactionByRace(Race race)
        {
            if (IsAllianceRace(race))
                return 1;
            if (IsHordeRace(race)) 
                return 2;
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
        #endregion
        #region Loading
        // Loading code
        public static void LoadEverything()
        {
            Log.Print(LogType.Storage, "Loading data files...");
            LoadBroadcastTexts();
            LoadItemDisplayIds();
            LoadItemRecords();
            LoadItemSparseRecords();
            LoadItemAppearance();
            LoadItemModifiedAppearance();
            LoadItemEffect();
            LoadItemSpellsData();
            LoadItemDisplayIdToFileDataId();
            LoadBattlegrounds();
            LoadChatChannels();
            LoadItemEnchantVisuals();
            LoadSpellVisuals();
            LoadLearnSpells();
            LoadTotemSpells();
            LoadGems();
            LoadCreatureDisplayInfo();
            LoadCreatureModelCollisionHeights();
            LoadTransports();
            LoadAreaNames();
            LoadRaceFaction();
            LoadDispellSpells();
            LoadSpellEffectPoints();
            LoadStackableAuras();
            LoadMountAuras();
            LoadMeleeSpells();
            LoadAutoRepeatSpells();
            LoadAuraSpells();
            LoadTaxiPaths();
            LoadTaxiPathNodesGraph();
            LoadQuestBits();
            LoadHotfixes();
            Log.Print(LogType.Storage, "Finished loading data.");
        }

        public static void LoadBroadcastTexts()
        {
            var path = Path.Combine("CSV", $"BroadcastTexts{LegacyVersion.ExpansionVersion}.csv");
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

        public static void LoadItemDisplayIds()
        {
            var path = Path.Combine("CSV", $"ItemIdToDisplayId{ModernVersion.ExpansionVersion}.csv");
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
                    uint displayId = UInt32.Parse(fields[1]);
                    ItemDisplayIdStore.Add(entry, displayId);
                }
            }
        }

        public static void LoadItemRecords()
        {
            var path = Path.Combine("CSV", $"Item{ModernVersion.ExpansionVersion}.csv");
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

                    ItemRecord row = new();
                    row.Id = Int32.Parse(fields[0]);
                    row.ClassId = Byte.Parse(fields[1]);
                    row.SubclassId = Byte.Parse(fields[2]);
                    row.Material = Byte.Parse(fields[3]);
                    row.InventoryType = SByte.Parse(fields[4]);
                    row.RequiredLevel = Int32.Parse(fields[5]);
                    row.SheatheType = Byte.Parse(fields[6]);
                    row.RandomProperty = UInt16.Parse(fields[7]);
                    row.ItemRandomSuffixGroupId = UInt16.Parse(fields[8]);
                    row.SoundOverrideSubclassId = SByte.Parse(fields[9]);
                    row.ScalingStatDistributionId = UInt16.Parse(fields[10]);
                    row.IconFileDataId = Int32.Parse(fields[11]);
                    row.ItemGroupSoundsId = Byte.Parse(fields[12]);
                    row.ContentTuningId = Int32.Parse(fields[13]);
                    row.MaxDurability = UInt32.Parse(fields[14]);
                    row.AmmoType = Byte.Parse(fields[15]);
                    row.DamageType[0] = Byte.Parse(fields[16]);
                    row.DamageType[1] = Byte.Parse(fields[17]);
                    row.DamageType[2] = Byte.Parse(fields[18]);
                    row.DamageType[3] = Byte.Parse(fields[19]);
                    row.DamageType[4] = Byte.Parse(fields[20]);
                    row.Resistances[0] = Int16.Parse(fields[21]);
                    row.Resistances[1] = Int16.Parse(fields[22]);
                    row.Resistances[2] = Int16.Parse(fields[23]);
                    row.Resistances[3] = Int16.Parse(fields[24]);
                    row.Resistances[4] = Int16.Parse(fields[25]);
                    row.Resistances[5] = Int16.Parse(fields[26]);
                    row.Resistances[6] = Int16.Parse(fields[27]);
                    row.MinDamage[0] = UInt16.Parse(fields[28]);
                    row.MinDamage[1] = UInt16.Parse(fields[29]);
                    row.MinDamage[2] = UInt16.Parse(fields[30]);
                    row.MinDamage[3] = UInt16.Parse(fields[31]);
                    row.MinDamage[4] = UInt16.Parse(fields[32]);
                    row.MaxDamage[0] = UInt16.Parse(fields[33]);
                    row.MaxDamage[1] = UInt16.Parse(fields[34]);
                    row.MaxDamage[2] = UInt16.Parse(fields[35]);
                    row.MaxDamage[3] = UInt16.Parse(fields[36]);
                    row.MaxDamage[4] = UInt16.Parse(fields[37]);
                    ItemRecordsStore.Add((uint)row.Id, row);
                }
            }
        }

        public static void LoadItemSparseRecords()
        {
            var path = Path.Combine("CSV", $"ItemSparse{ModernVersion.ExpansionVersion}.csv");
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

                    ItemSparseRecord row = new();
                    row.Id = Int32.Parse(fields[0]);
                    row.AllowableRace = Int64.Parse(fields[1]);
                    row.Description = fields[2];
                    row.Name4 = fields[3];
                    row.Name3 = fields[4];
                    row.Name2 = fields[5];
                    row.Name1 = fields[6];
                    row.DmgVariance = Single.Parse(fields[7]);
                    row.DurationInInventory = UInt32.Parse(fields[8]);
                    row.QualityModifier = Single.Parse(fields[9]);
                    row.BagFamily = UInt32.Parse(fields[10]);
                    row.RangeMod = Single.Parse(fields[11]);
                    row.StatPercentageOfSocket[0] = Single.Parse(fields[12]);
                    row.StatPercentageOfSocket[1] = Single.Parse(fields[13]);
                    row.StatPercentageOfSocket[2] = Single.Parse(fields[14]);
                    row.StatPercentageOfSocket[3] = Single.Parse(fields[15]);
                    row.StatPercentageOfSocket[4] = Single.Parse(fields[16]);
                    row.StatPercentageOfSocket[5] = Single.Parse(fields[17]);
                    row.StatPercentageOfSocket[6] = Single.Parse(fields[18]);
                    row.StatPercentageOfSocket[7] = Single.Parse(fields[19]);
                    row.StatPercentageOfSocket[8] = Single.Parse(fields[20]);
                    row.StatPercentageOfSocket[9] = Single.Parse(fields[21]);
                    row.StatPercentEditor[0] = Int32.Parse(fields[22]);
                    row.StatPercentEditor[1] = Int32.Parse(fields[23]);
                    row.StatPercentEditor[2] = Int32.Parse(fields[24]);
                    row.StatPercentEditor[3] = Int32.Parse(fields[25]);
                    row.StatPercentEditor[4] = Int32.Parse(fields[26]);
                    row.StatPercentEditor[5] = Int32.Parse(fields[27]);
                    row.StatPercentEditor[6] = Int32.Parse(fields[28]);
                    row.StatPercentEditor[7] = Int32.Parse(fields[29]);
                    row.StatPercentEditor[8] = Int32.Parse(fields[30]);
                    row.StatPercentEditor[9] = Int32.Parse(fields[31]);
                    row.Stackable = Int32.Parse(fields[32]);
                    row.MaxCount = Int32.Parse(fields[33]);
                    row.RequiredAbility = UInt32.Parse(fields[34]);
                    row.SellPrice = UInt32.Parse(fields[35]);
                    row.BuyPrice = UInt32.Parse(fields[36]);
                    row.VendorStackCount = UInt32.Parse(fields[37]);
                    row.PriceVariance = Single.Parse(fields[38]);
                    row.PriceRandomValue = Single.Parse(fields[39]);
                    row.Flags[0] = UInt32.Parse(fields[40]);
                    row.Flags[1] = UInt32.Parse(fields[41]);
                    row.Flags[2] = UInt32.Parse(fields[42]);
                    row.Flags[3] = UInt32.Parse(fields[43]);
                    row.OppositeFactionItemId = Int32.Parse(fields[44]);
                    row.MaxDurability = UInt32.Parse(fields[45]);
                    row.ItemNameDescriptionId = UInt16.Parse(fields[46]);
                    row.RequiredTransmogHoliday = UInt16.Parse(fields[47]);
                    row.RequiredHoliday = UInt16.Parse(fields[48]);
                    row.LimitCategory = UInt16.Parse(fields[49]);
                    row.GemProperties = UInt16.Parse(fields[50]);
                    row.SocketMatchEnchantmentId = UInt16.Parse(fields[51]);
                    row.TotemCategoryId = UInt16.Parse(fields[52]);
                    row.InstanceBound = UInt16.Parse(fields[53]);
                    row.ZoneBound[0] = UInt16.Parse(fields[54]);
                    row.ZoneBound[1] = UInt16.Parse(fields[55]);
                    row.ItemSet = UInt16.Parse(fields[56]);
                    row.LockId = UInt16.Parse(fields[57]);
                    row.StartQuestId = UInt16.Parse(fields[58]);
                    row.PageText = UInt16.Parse(fields[59]);
                    row.Delay = UInt16.Parse(fields[60]);
                    row.RequiredReputationId = UInt16.Parse(fields[61]);
                    row.RequiredSkillRank = UInt16.Parse(fields[62]);
                    row.RequiredSkill = UInt16.Parse(fields[63]);
                    row.ItemLevel = UInt16.Parse(fields[64]);
                    row.AllowableClass = Int16.Parse(fields[65]);
                    row.ItemRandomSuffixGroupId = UInt16.Parse(fields[66]);
                    row.RandomProperty = UInt16.Parse(fields[67]);
                    row.MinDamage[0] = UInt16.Parse(fields[68]);
                    row.MinDamage[1] = UInt16.Parse(fields[69]);
                    row.MinDamage[2] = UInt16.Parse(fields[70]);
                    row.MinDamage[3] = UInt16.Parse(fields[71]);
                    row.MinDamage[4] = UInt16.Parse(fields[72]);
                    row.MaxDamage[0] = UInt16.Parse(fields[73]);
                    row.MaxDamage[1] = UInt16.Parse(fields[74]);
                    row.MaxDamage[2] = UInt16.Parse(fields[75]);
                    row.MaxDamage[3] = UInt16.Parse(fields[76]);
                    row.MaxDamage[4] = UInt16.Parse(fields[77]);
                    row.Resistances[0] = Int16.Parse(fields[78]);
                    row.Resistances[1] = Int16.Parse(fields[79]);
                    row.Resistances[2] = Int16.Parse(fields[80]);
                    row.Resistances[3] = Int16.Parse(fields[81]);
                    row.Resistances[4] = Int16.Parse(fields[82]);
                    row.Resistances[5] = Int16.Parse(fields[83]);
                    row.Resistances[6] = Int16.Parse(fields[84]);
                    row.ScalingStatDistributionId = UInt16.Parse(fields[85]);
                    row.ExpansionId = Byte.Parse(fields[86]);
                    row.ArtifactId = Byte.Parse(fields[87]);
                    row.SpellWeight = Byte.Parse(fields[88]);
                    row.SpellWeightCategory = Byte.Parse(fields[89]);
                    row.SocketType[0] = Byte.Parse(fields[90]);
                    row.SocketType[1] = Byte.Parse(fields[91]);
                    row.SocketType[2] = Byte.Parse(fields[92]);
                    row.SheatheType = Byte.Parse(fields[93]);
                    row.Material = Byte.Parse(fields[94]);
                    row.PageMaterial = Byte.Parse(fields[95]);
                    row.PageLanguage = Byte.Parse(fields[96]);
                    row.Bonding = Byte.Parse(fields[97]);
                    row.DamageType = Byte.Parse(fields[98]);
                    row.StatType[0] = SByte.Parse(fields[99]);
                    row.StatType[1] = SByte.Parse(fields[100]);
                    row.StatType[2] = SByte.Parse(fields[101]);
                    row.StatType[3] = SByte.Parse(fields[102]);
                    row.StatType[4] = SByte.Parse(fields[103]);
                    row.StatType[5] = SByte.Parse(fields[104]);
                    row.StatType[6] = SByte.Parse(fields[105]);
                    row.StatType[7] = SByte.Parse(fields[106]);
                    row.StatType[8] = SByte.Parse(fields[107]);
                    row.StatType[9] = SByte.Parse(fields[108]);
                    row.ContainerSlots = Byte.Parse(fields[109]);
                    row.RequiredReputationRank = Byte.Parse(fields[110]);
                    row.RequiredCityRank = Byte.Parse(fields[111]);
                    row.RequiredHonorRank = Byte.Parse(fields[112]);
                    row.InventoryType = Byte.Parse(fields[113]);
                    row.OverallQualityId = Byte.Parse(fields[114]);
                    row.AmmoType = Byte.Parse(fields[115]);
                    row.StatValue[0] = SByte.Parse(fields[116]);
                    row.StatValue[1] = SByte.Parse(fields[117]);
                    row.StatValue[2] = SByte.Parse(fields[118]);
                    row.StatValue[3] = SByte.Parse(fields[119]);
                    row.StatValue[4] = SByte.Parse(fields[120]);
                    row.StatValue[5] = SByte.Parse(fields[121]);
                    row.StatValue[6] = SByte.Parse(fields[122]);
                    row.StatValue[7] = SByte.Parse(fields[123]);
                    row.StatValue[8] = SByte.Parse(fields[124]);
                    row.StatValue[9] = SByte.Parse(fields[125]);
                    row.RequiredLevel = SByte.Parse(fields[126]);
                    ItemSparseRecordsStore.Add((uint)row.Id, row);
                }
            }
        }

        public static void LoadItemAppearance()
        {
            var path = Path.Combine("CSV", $"ItemAppearance{ModernVersion.ExpansionVersion}.csv");
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

                    ItemAppearance appearance = new ItemAppearance();
                    appearance.Id = Int32.Parse(fields[0]);
                    appearance.DisplayType = Byte.Parse(fields[1]);
                    appearance.ItemDisplayInfoID = Int32.Parse(fields[2]);
                    appearance.DefaultIconFileDataID = Int32.Parse(fields[3]);
                    appearance.UiOrder = Int32.Parse(fields[4]);
                    ItemAppearanceStore.Add((uint)appearance.Id, appearance);
                }
            }
        }

        public static void LoadItemModifiedAppearance()
        {
            var path = Path.Combine("CSV", $"ItemModifiedAppearance{ModernVersion.ExpansionVersion}.csv");
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

                    ItemModifiedAppearance modifiedAppearance = new ItemModifiedAppearance();
                    modifiedAppearance.Id = Int32.Parse(fields[0]);
                    modifiedAppearance.ItemID = Int32.Parse(fields[1]);
                    modifiedAppearance.ItemAppearanceModifierID = Int32.Parse(fields[2]);
                    modifiedAppearance.ItemAppearanceID = Int32.Parse(fields[3]);
                    modifiedAppearance.OrderIndex = Int32.Parse(fields[4]);
                    modifiedAppearance.TransmogSourceTypeEnum = Int32.Parse(fields[5]);
                    ItemModifiedAppearanceStore.Add((uint)modifiedAppearance.Id, modifiedAppearance);
                }
            }
        }

        public static void LoadItemEffect()
        {
            var path = Path.Combine("CSV", $"ItemEffect{ModernVersion.ExpansionVersion}.csv");
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

                    ItemEffect effect = new ItemEffect();
                    effect.Id = Int32.Parse(fields[0]);
                    effect.LegacySlotIndex = Byte.Parse(fields[1]);
                    effect.TriggerType = SByte.Parse(fields[2]);
                    effect.Charges = Int16.Parse(fields[3]);
                    effect.CoolDownMSec = Int32.Parse(fields[4]);
                    effect.CategoryCoolDownMSec = Int32.Parse(fields[5]);
                    effect.SpellCategoryID = UInt16.Parse(fields[6]);
                    effect.SpellID = Int32.Parse(fields[7]);
                    effect.ChrSpecializationID = UInt16.Parse(fields[8]);
                    effect.ParentItemID = Int32.Parse(fields[9]);
                    ItemEffectStore.Add((uint)effect.Id, effect);
                }
            }
        }

        public static void LoadItemSpellsData()
        {
            var path = Path.Combine("CSV", $"ItemSpellsData{ModernVersion.ExpansionVersion}.csv");
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

                    ItemSpellsData data = new ItemSpellsData();
                    data.Id = Int32.Parse(fields[0]);
                    data.Category = Int32.Parse(fields[1]);
                    data.RecoveryTime = Int32.Parse(fields[2]);
                    data.CategoryRecoveryTime = Int32.Parse(fields[3]);
                    ItemSpellsDataStore.Add((uint)data.Id, data);
                }
            }
        }

        public static void LoadItemDisplayIdToFileDataId()
        {
            var path = Path.Combine("CSV", $"ItemDisplayIdToFileDataId{ModernVersion.ExpansionVersion}.csv");
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
                    uint fileDataId = UInt32.Parse(fields[1]);
                    ItemDisplayIdToFileDataIdStore.Add(displayId, fileDataId);
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

        public static void LoadChatChannels()
        {
            var path = Path.Combine("CSV", "ChatChannels.csv");
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

                    ChatChannel channel = new ChatChannel();
                    channel.Id = UInt32.Parse(fields[0]);
                    channel.Flags = (ChannelFlags)UInt32.Parse(fields[1]);
                    channel.Name = fields[2];
                    ChatChannels.Add(channel.Id, channel);
                }
            }
        }

        public static void LoadItemEnchantVisuals()
        {
            var path = Path.Combine("CSV", $"ItemEnchantVisuals{ModernVersion.ExpansionVersion}.csv");
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
                    uint visualId = UInt32.Parse(fields[1]);
                    ItemEnchantVisuals.Add(enchantId, visualId);
                }
            }
        }

        public static void LoadSpellVisuals()
        {
            var path = Path.Combine("CSV", $"SpellVisuals{ModernVersion.ExpansionVersion}.csv");
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

        public static void LoadTotemSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"TotemSpells.csv");
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
                    uint totemSlot = UInt32.Parse(fields[1]);
                    TotemSpells.Add(spellId, totemSlot);
                }
            }
        }

        public static void LoadGems()
        {
            if (ModernVersion.ExpansionVersion <= 1)
                return;

            var path = Path.Combine("CSV", $"Gems{ModernVersion.ExpansionVersion}.csv");
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

        public static void LoadCreatureDisplayInfo()
        {
            var path = Path.Combine("CSV", "CreatureDisplayInfo.csv");
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
                    uint modelId = UInt32.Parse(fields[1]);
                    float scale = Single.Parse(fields[2]);
                    CreatureDisplayInfos.Add(displayId, new CreatureDisplayInfo(modelId, scale));
                }
            }
        }

        public static void LoadCreatureModelCollisionHeights()
        {
            var path = Path.Combine("CSV", $"CreatureModelCollisionHeightsModern{LegacyVersion.ExpansionVersion}.csv");
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

                    uint modelId = UInt32.Parse(fields[0]);
                    float modelScale = Single.Parse(fields[1]);
                    float collisionHeight = Single.Parse(fields[2]);
                    float collisionHeightMounted = Single.Parse(fields[3]);
                    CreatureModelCollisionHeights.Add(modelId, new CreatureModelCollisionHeight(modelScale, collisionHeight, collisionHeightMounted));
                }
            }
        }

        public static void LoadTransports()
        {
            var path = Path.Combine("CSV", $"Transports{LegacyVersion.ExpansionVersion}.csv");
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

        public static void LoadAreaNames()
        {
            var path = Path.Combine("CSV", $"AreaNames.csv");
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

                    uint id = UInt32.Parse(fields[0]);
                    string name = fields[1];
                    AreaNames.Add(id, name);
                }
            }
        }

        public static void LoadRaceFaction()
        {
            var path = Path.Combine("CSV", $"RaceFaction.csv");
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

                    uint id = UInt32.Parse(fields[0]);
                    uint faction = UInt32.Parse(fields[1]);
                    RaceFaction.Add(id, faction);
                }
            }
        }

        public static void LoadDispellSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
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

        public static void LoadSpellEffectPoints()
        {
            var path = Path.Combine("CSV", $"SpellEffectPoints{LegacyVersion.ExpansionVersion}.csv");
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

                    // Those basePoints are usually incremented by 1, only few test spell have another value there (baseDice)
                    int basePointsEff1 = int.Parse(fields[2]);
                    if (basePointsEff1 != 0)
                        basePointsEff1 += 1;

                    int basePointsEff2 = int.Parse(fields[3]);
                    if (basePointsEff2 != 0)
                        basePointsEff2 += 1;

                    int basePointsEff3 = int.Parse(fields[4]);
                    if (basePointsEff3 != 0)
                        basePointsEff3 += 1;

                    SpellEffectPoints.Add(spellId, new List<float>{ basePointsEff1, basePointsEff2, basePointsEff3 });
                }
            }
        }

        public static void LoadStackableAuras()
        {
            if (LegacyVersion.ExpansionVersion > 2)
                return;

            var path = Path.Combine("CSV", $"StackableAuras{LegacyVersion.ExpansionVersion}.csv");
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

        public static void LoadMountAuras()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"MountAuras.csv");
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
                    MountAuras.Add(spellId);
                }
            }
        }

        public static void LoadMeleeSpells()
        {
            var path = Path.Combine("CSV", $"MeleeSpells{ModernVersion.ExpansionVersion}.csv");
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
                    NextMeleeSpells.Add(spellId);
                }
            }
        }

        public static void LoadAutoRepeatSpells()
        {
            var path = Path.Combine("CSV", $"AutoRepeatSpells{ModernVersion.ExpansionVersion}.csv");
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
                    AutoRepeatSpells.Add(spellId);
                }
            }
        }
        public static void LoadAuraSpells()
        {
            var path = Path.Combine("CSV", $"AuraSpells{LegacyVersion.ExpansionVersion}.csv");
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
                    AuraSpells.Add(spellId);
                }
            }
        }
        public static void LoadTaxiPaths()
        {
            var path = Path.Combine("CSV", $"TaxiPath{ModernVersion.ExpansionVersion}.csv");
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
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    TaxiPath taxiPath = new TaxiPath();
                    taxiPath.Id = UInt32.Parse(fields[0]);
                    taxiPath.From = UInt32.Parse(fields[1]);
                    taxiPath.To = UInt32.Parse(fields[2]);
                    taxiPath.Cost = Int32.Parse(fields[3]);
                    TaxiPaths.Add(counter, taxiPath);
                    counter++;
                }
            }
        }
        public static void LoadTaxiPathNodesGraph()
        {
            // Load TaxiNodes (used in calculating first and last parts of path)
            Dictionary<uint, TaxiNode> TaxiNodes = new Dictionary<uint, TaxiNode>();
            var pathNodes = Path.Combine("CSV", $"TaxiNodes{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(pathNodes))
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

                    TaxiNode taxiNode = new TaxiNode();
                    taxiNode.Id = UInt32.Parse(fields[0]);
                    taxiNode.mapId = UInt32.Parse(fields[1]);
                    taxiNode.x = float.Parse(fields[2]);
                    taxiNode.y = float.Parse(fields[3]);
                    taxiNode.z = float.Parse(fields[4]);
                    TaxiNodes.Add(taxiNode.Id, taxiNode);
                }
            }
            // Load TaxiPathNode (used in calculating rest of path)
            Dictionary<uint, TaxiPathNode> TaxiPathNodes = new Dictionary<uint, TaxiPathNode>();
            var pathPathNodes = Path.Combine("CSV", $"TaxiPathNode{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(pathPathNodes))
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

                    TaxiPathNode taxiPathNode = new TaxiPathNode();
                    taxiPathNode.Id = UInt32.Parse(fields[0]);
                    taxiPathNode.pathId = UInt32.Parse(fields[1]);
                    taxiPathNode.nodeIndex = UInt32.Parse(fields[2]);
                    taxiPathNode.mapId = UInt32.Parse(fields[3]);
                    taxiPathNode.x = float.Parse(fields[4]);
                    taxiPathNode.y = float.Parse(fields[5]);
                    taxiPathNode.z = float.Parse(fields[6]);
                    taxiPathNode.flags = UInt32.Parse(fields[7]);
                    taxiPathNode.delay = UInt32.Parse(fields[8]);
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
                    HashSet<uint> pathNodeList = new HashSet<uint>();
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

                    uint questId = UInt32.Parse(fields[0]);
                    if (fields[1].StartsWith("-"))
                        continue; // Some bits have a negative index, is this an error from WDBX?
                    uint uniqueBitFlag = UInt32.Parse(fields[1]);
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
        public const uint HotfixItemBegin = 210000;
        public const uint HotfixItemSparseBegin = 220000;
        public const uint HotfixItemAppearanceBegin = 230000;
        public const uint HotfixItemModifiedAppearanceBegin = 240000;
        public const uint HotfixItemEffectBegin = 250000;
        public const uint HotfixItemDisplayInfoBegin = 260000;
        public const uint HotfixCreatureDisplayInfoBegin = 270000;
        public const uint HotfixCreatureDisplayInfoExtraBegin = 280000;
        public const uint HotfixCreatureDisplayInfoOptionBegin = 290000;
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
            LoadSpellXSpellVisualHotfixes();
            LoadItemSparseHotfixes();
            LoadItemHotfixes();
            LoadItemEffectHotfixes();
            LoadItemDisplayInfoHotfixes();
            LoadCreatureDisplayInfoHotfixes();
            LoadCreatureDisplayInfoExtraHotfixes();
            LoadCreatureDisplayInfoOptionHotfixes();
        }
        
        public static void LoadAreaTriggerHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"AreaTrigger{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SkillLine{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SkillRaceClassInfo{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SkillLineAbility{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"Spell{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellName{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellLevels{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellAuraOptions{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellMisc{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"SpellEffect{ModernVersion.ExpansionVersion}.csv");
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
        public static void LoadSpellXSpellVisualHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{ModernVersion.ExpansionVersion}.csv");
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
                    uint spellVisualId = UInt32.Parse(fields[2]);
                    float probability = Single.Parse(fields[3]);
                    byte flags = Byte.Parse(fields[4]);
                    byte priority = Byte.Parse(fields[5]);
                    int spellIconFileId = Int32.Parse(fields[6]);
                    int activeIconFileId = Int32.Parse(fields[7]);
                    ushort viewerUnitConditionId = UInt16.Parse(fields[8]);
                    uint viewerPlayerConditionId = UInt32.Parse(fields[9]);
                    ushort casterUnitConditionId = UInt16.Parse(fields[10]);
                    uint casterPlayerConditionId = UInt32.Parse(fields[11]);
                    uint spellId = UInt32.Parse(fields[12]);

                    if (SpellVisuals.ContainsKey(spellId))
                        SpellVisuals[spellId] = id;
                    else
                        SpellVisuals.Add(spellId, id);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellXSpellVisual;
                    record.HotfixId = HotfixSpellXSpellVisualBegin + counter;
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

        public static void WriteItemSparseHotfix(ItemTemplate item, Framework.IO.ByteBuffer buffer)
        {
            int[] StatValues = new int[10];
            for (int i = 0; i < item.StatsCount; i++)
            {
                StatValues[i] = item.StatValues[i];
                if (StatValues[i] > 127)
                    StatValues[i] = 127;
                if (StatValues[i] < -127)
                    StatValues[i] = -127;
            }

            buffer.WriteInt64(item.AllowedRaces);
            buffer.WriteCString(item.Description);
            buffer.WriteCString(item.Name[3]);
            buffer.WriteCString(item.Name[2]);
            buffer.WriteCString(item.Name[1]);
            buffer.WriteCString(item.Name[0]);
            buffer.WriteFloat(1);
            buffer.WriteUInt32(item.Duration);
            buffer.WriteFloat(0);
            buffer.WriteUInt32(item.BagFamily);
            buffer.WriteFloat(item.RangedMod);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(item.MaxStackSize);
            buffer.WriteInt32(item.MaxCount);
            buffer.WriteUInt32(item.RequiredSpell);
            buffer.WriteUInt32(item.SellPrice);
            buffer.WriteUInt32(item.BuyPrice);
            buffer.WriteUInt32(item.BuyCount);
            buffer.WriteFloat(1);
            buffer.WriteFloat(1);
            buffer.WriteUInt32(item.Flags);
            buffer.WriteUInt32(item.FlagsExtra);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteUInt32(item.MaxDurability);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16((ushort)item.HolidayID);
            buffer.WriteUInt16((ushort)item.ItemLimitCategory);
            buffer.WriteUInt16((ushort)item.GemProperties);
            buffer.WriteUInt16((ushort)item.SocketBonus);
            buffer.WriteUInt16((ushort)item.TotemCategory);
            buffer.WriteUInt16((ushort)item.MapID);
            buffer.WriteUInt16((ushort)item.AreaID);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16((ushort)item.ItemSet);
            buffer.WriteUInt16((ushort)item.LockId);
            buffer.WriteUInt16((ushort)item.StartQuestId);
            buffer.WriteUInt16((ushort)item.PageText);
            buffer.WriteUInt16((ushort)item.Delay);
            buffer.WriteUInt16((ushort)item.RequiredRepFaction);
            buffer.WriteUInt16((ushort)item.RequiredSkillLevel);
            buffer.WriteUInt16((ushort)item.RequiredSkillId);
            buffer.WriteUInt16((ushort)item.ItemLevel);
            buffer.WriteInt16((short)item.AllowedClasses);
            buffer.WriteUInt16((ushort)item.RandomSuffix);
            buffer.WriteUInt16((ushort)item.RandomProperty);
            buffer.WriteUInt16((ushort)item.DamageMins[0]);
            buffer.WriteUInt16((ushort)item.DamageMins[1]);
            buffer.WriteUInt16((ushort)item.DamageMins[2]);
            buffer.WriteUInt16((ushort)item.DamageMins[3]);
            buffer.WriteUInt16((ushort)item.DamageMins[4]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
            buffer.WriteInt16((short)item.Armor);
            buffer.WriteInt16((short)item.HolyResistance);
            buffer.WriteInt16((short)item.FireResistance);
            buffer.WriteInt16((short)item.NatureResistance);
            buffer.WriteInt16((short)item.FrostResistance);
            buffer.WriteInt16((short)item.ShadowResistance);
            buffer.WriteInt16((short)item.ArcaneResistance);
            buffer.WriteUInt16((ushort)item.ScalingStatDistribution);
            buffer.WriteUInt8(254);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8((byte)item.ItemSocketColors[0]);
            buffer.WriteUInt8((byte)item.ItemSocketColors[1]);
            buffer.WriteUInt8((byte)item.ItemSocketColors[2]);
            buffer.WriteUInt8((byte)item.SheathType);
            buffer.WriteUInt8((byte)item.Material);
            buffer.WriteUInt8((byte)item.PageMaterial);
            buffer.WriteUInt8((byte)item.Language);
            buffer.WriteUInt8((byte)item.Bonding);
            buffer.WriteUInt8((byte)item.DamageTypes[0]);
            buffer.WriteInt8((sbyte)item.StatTypes[0]);
            buffer.WriteInt8((sbyte)item.StatTypes[1]);
            buffer.WriteInt8((sbyte)item.StatTypes[2]);
            buffer.WriteInt8((sbyte)item.StatTypes[3]);
            buffer.WriteInt8((sbyte)item.StatTypes[4]);
            buffer.WriteInt8((sbyte)item.StatTypes[5]);
            buffer.WriteInt8((sbyte)item.StatTypes[6]);
            buffer.WriteInt8((sbyte)item.StatTypes[7]);
            buffer.WriteInt8((sbyte)item.StatTypes[8]);
            buffer.WriteInt8((sbyte)item.StatTypes[9]);
            buffer.WriteUInt8((byte)item.ContainerSlots);
            buffer.WriteUInt8((byte)item.RequiredRepValue);
            buffer.WriteUInt8((byte)item.RequiredCityRank);
            buffer.WriteUInt8((byte)item.RequiredHonorRank);
            buffer.WriteUInt8((byte)item.InventoryType);
            buffer.WriteUInt8((byte)item.Quality);
            buffer.WriteUInt8((byte)item.AmmoType);
            buffer.WriteInt8((sbyte)StatValues[0]);
            buffer.WriteInt8((sbyte)StatValues[1]);
            buffer.WriteInt8((sbyte)StatValues[2]);
            buffer.WriteInt8((sbyte)StatValues[3]);
            buffer.WriteInt8((sbyte)StatValues[4]);
            buffer.WriteInt8((sbyte)StatValues[5]);
            buffer.WriteInt8((sbyte)StatValues[6]);
            buffer.WriteInt8((sbyte)StatValues[7]);
            buffer.WriteInt8((sbyte)StatValues[8]);
            buffer.WriteInt8((sbyte)StatValues[9]);
            buffer.WriteInt8((sbyte)item.RequiredLevel);
        }

        public static void WriteItemSparseHotfix(ItemSparseRecord row, Framework.IO.ByteBuffer buffer)
        {
            int[] StatValues = new int[10];
            for (int i = 0; i < 10; i++)
            {
                StatValues[i] = (int)row.StatValue[i];
                if (StatValues[i] > 127)
                    StatValues[i] = 127;
                if (StatValues[i] < -127)
                    StatValues[i] = -127;
            }

            buffer.WriteInt64(row.AllowableRace);
            buffer.WriteCString(row.Description);
            buffer.WriteCString(row.Name4);
            buffer.WriteCString(row.Name3);
            buffer.WriteCString(row.Name2);
            buffer.WriteCString(row.Name1);
            buffer.WriteFloat(row.DmgVariance);
            buffer.WriteUInt32(row.DurationInInventory);
            buffer.WriteFloat(row.QualityModifier);
            buffer.WriteUInt32(row.BagFamily);
            buffer.WriteFloat(row.RangeMod);
            buffer.WriteFloat(row.StatPercentageOfSocket[0]);
            buffer.WriteFloat(row.StatPercentageOfSocket[1]);
            buffer.WriteFloat(row.StatPercentageOfSocket[2]);
            buffer.WriteFloat(row.StatPercentageOfSocket[3]);
            buffer.WriteFloat(row.StatPercentageOfSocket[4]);
            buffer.WriteFloat(row.StatPercentageOfSocket[5]);
            buffer.WriteFloat(row.StatPercentageOfSocket[6]);
            buffer.WriteFloat(row.StatPercentageOfSocket[7]);
            buffer.WriteFloat(row.StatPercentageOfSocket[8]);
            buffer.WriteFloat(row.StatPercentageOfSocket[9]);
            buffer.WriteInt32(row.StatPercentEditor[0]);
            buffer.WriteInt32(row.StatPercentEditor[1]);
            buffer.WriteInt32(row.StatPercentEditor[2]);
            buffer.WriteInt32(row.StatPercentEditor[3]);
            buffer.WriteInt32(row.StatPercentEditor[4]);
            buffer.WriteInt32(row.StatPercentEditor[5]);
            buffer.WriteInt32(row.StatPercentEditor[6]);
            buffer.WriteInt32(row.StatPercentEditor[7]);
            buffer.WriteInt32(row.StatPercentEditor[8]);
            buffer.WriteInt32(row.StatPercentEditor[9]);
            buffer.WriteInt32(row.Stackable);
            buffer.WriteInt32(row.MaxCount);
            buffer.WriteUInt32(row.RequiredAbility);
            buffer.WriteUInt32(row.SellPrice);
            buffer.WriteUInt32(row.BuyPrice);
            buffer.WriteUInt32(row.VendorStackCount);
            buffer.WriteFloat(row.PriceVariance);
            buffer.WriteFloat(row.PriceRandomValue);
            buffer.WriteUInt32(row.Flags[0]);
            buffer.WriteUInt32(row.Flags[1]);
            buffer.WriteUInt32(row.Flags[2]);
            buffer.WriteUInt32(row.Flags[3]);
            buffer.WriteInt32(row.OppositeFactionItemId);
            buffer.WriteUInt32(row.MaxDurability);
            buffer.WriteUInt16(row.ItemNameDescriptionId);
            buffer.WriteUInt16(row.RequiredTransmogHoliday);
            buffer.WriteUInt16(row.RequiredHoliday);
            buffer.WriteUInt16(row.LimitCategory);
            buffer.WriteUInt16(row.GemProperties);
            buffer.WriteUInt16(row.SocketMatchEnchantmentId);
            buffer.WriteUInt16(row.TotemCategoryId);
            buffer.WriteUInt16(row.InstanceBound);
            buffer.WriteUInt16(row.ZoneBound[0]);
            buffer.WriteUInt16(row.ZoneBound[1]);
            buffer.WriteUInt16(row.ItemSet);
            buffer.WriteUInt16(row.LockId);
            buffer.WriteUInt16(row.StartQuestId);
            buffer.WriteUInt16(row.PageText);
            buffer.WriteUInt16(row.Delay);
            buffer.WriteUInt16(row.RequiredReputationId);
            buffer.WriteUInt16(row.RequiredSkillRank);
            buffer.WriteUInt16(row.RequiredSkill);
            buffer.WriteUInt16(row.ItemLevel);
            buffer.WriteInt16(row.AllowableClass);
            buffer.WriteUInt16(row.ItemRandomSuffixGroupId);
            buffer.WriteUInt16(row.RandomProperty);
            buffer.WriteUInt16(row.MinDamage[0]);
            buffer.WriteUInt16(row.MinDamage[1]);
            buffer.WriteUInt16(row.MinDamage[2]);
            buffer.WriteUInt16(row.MinDamage[3]);
            buffer.WriteUInt16(row.MinDamage[4]);
            buffer.WriteUInt16(row.MaxDamage[0]);
            buffer.WriteUInt16(row.MaxDamage[1]);
            buffer.WriteUInt16(row.MaxDamage[2]);
            buffer.WriteUInt16(row.MaxDamage[3]);
            buffer.WriteUInt16(row.MaxDamage[4]);
            buffer.WriteInt16(row.Resistances[0]);
            buffer.WriteInt16(row.Resistances[1]);
            buffer.WriteInt16(row.Resistances[2]);
            buffer.WriteInt16(row.Resistances[3]);
            buffer.WriteInt16(row.Resistances[4]);
            buffer.WriteInt16(row.Resistances[5]);
            buffer.WriteInt16(row.Resistances[6]);
            buffer.WriteUInt16(row.ScalingStatDistributionId);
            buffer.WriteUInt8(row.ExpansionId);
            buffer.WriteUInt8(row.ArtifactId);
            buffer.WriteUInt8(row.SpellWeight);
            buffer.WriteUInt8(row.SpellWeightCategory);
            buffer.WriteUInt8(row.SocketType[0]);
            buffer.WriteUInt8(row.SocketType[1]);
            buffer.WriteUInt8(row.SocketType[2]);
            buffer.WriteUInt8(row.SheatheType);
            buffer.WriteUInt8(row.Material);
            buffer.WriteUInt8(row.PageMaterial);
            buffer.WriteUInt8(row.PageLanguage);
            buffer.WriteUInt8(row.Bonding);
            buffer.WriteUInt8(row.DamageType);
            buffer.WriteInt8(row.StatType[0]);
            buffer.WriteInt8(row.StatType[1]);
            buffer.WriteInt8(row.StatType[2]);
            buffer.WriteInt8(row.StatType[3]);
            buffer.WriteInt8(row.StatType[4]);
            buffer.WriteInt8(row.StatType[5]);
            buffer.WriteInt8(row.StatType[6]);
            buffer.WriteInt8(row.StatType[7]);
            buffer.WriteInt8(row.StatType[8]);
            buffer.WriteInt8(row.StatType[9]);
            buffer.WriteUInt8(row.ContainerSlots);
            buffer.WriteUInt8(row.RequiredReputationRank);
            buffer.WriteUInt8(row.RequiredCityRank);
            buffer.WriteUInt8(row.RequiredHonorRank);
            buffer.WriteUInt8(row.InventoryType);
            buffer.WriteUInt8(row.OverallQualityId);
            buffer.WriteUInt8(row.AmmoType);
            buffer.WriteInt8((sbyte)StatValues[0]);
            buffer.WriteInt8((sbyte)StatValues[1]);
            buffer.WriteInt8((sbyte)StatValues[2]);
            buffer.WriteInt8((sbyte)StatValues[3]);
            buffer.WriteInt8((sbyte)StatValues[4]);
            buffer.WriteInt8((sbyte)StatValues[5]);
            buffer.WriteInt8((sbyte)StatValues[6]);
            buffer.WriteInt8((sbyte)StatValues[7]);
            buffer.WriteInt8((sbyte)StatValues[8]);
            buffer.WriteInt8((sbyte)StatValues[9]);
            buffer.WriteInt8(row.RequiredLevel);
        }
        public static void LoadItemHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"Item{ModernVersion.ExpansionVersion}.csv");
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
                    byte ClassID = Byte.Parse(fields[1]);
                    byte SubclassID = Byte.Parse(fields[2]);
                    byte Material = Byte.Parse(fields[3]);
                    sbyte InventoryType = SByte.Parse(fields[4]);
                    uint RequiredLevel = UInt32.Parse(fields[5]);
                    byte SheatheType = Byte.Parse(fields[6]);
                    ushort RandomSelect = UInt16.Parse(fields[7]);
                    ushort ItemRandomSuffixGroupID = UInt16.Parse(fields[8]);
                    sbyte Sound_override_subclassID = SByte.Parse(fields[9]);
                    ushort ScalingStatDistributionID = UInt16.Parse(fields[10]);
                    int IconFileDataID = Int32.Parse(fields[11]);
                    byte ItemGroupSoundsID = Byte.Parse(fields[12]);
                    int ContentTuningID = Int32.Parse(fields[13]);
                    uint MaxDurability = UInt32.Parse(fields[14]);
                    byte AmmunitionType = Byte.Parse(fields[15]);
                    byte DamageType1 = Byte.Parse(fields[16]);
                    byte DamageType2 = Byte.Parse(fields[17]);
                    byte DamageType3 = Byte.Parse(fields[18]);
                    byte DamageType4 = Byte.Parse(fields[19]);
                    byte DamageType5 = Byte.Parse(fields[20]);
                    short Resistances1 = Int16.Parse(fields[21]);
                    short Resistances2 = Int16.Parse(fields[22]);
                    short Resistances3 = Int16.Parse(fields[23]);
                    short Resistances4 = Int16.Parse(fields[24]);
                    short Resistances5 = Int16.Parse(fields[25]);
                    short Resistances6 = Int16.Parse(fields[26]);
                    short Resistances7 = Int16.Parse(fields[27]);
                    ushort MinDamage1 = UInt16.Parse(fields[28]);
                    ushort MinDamage2 = UInt16.Parse(fields[29]);
                    ushort MinDamage3 = UInt16.Parse(fields[30]);
                    ushort MinDamage4 = UInt16.Parse(fields[31]);
                    ushort MinDamage5 = UInt16.Parse(fields[32]);
                    ushort MaxDamage1 = UInt16.Parse(fields[33]);
                    ushort MaxDamage2 = UInt16.Parse(fields[34]);
                    ushort MaxDamage3 = UInt16.Parse(fields[35]);
                    ushort MaxDamage4 = UInt16.Parse(fields[36]);
                    ushort MaxDamage5 = UInt16.Parse(fields[37]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.Item;
                    record.HotfixId = HotfixItemBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteUInt8(ClassID);
                    record.HotfixContent.WriteUInt8(SubclassID);
                    record.HotfixContent.WriteUInt8(Material);
                    record.HotfixContent.WriteInt8(InventoryType);
                    record.HotfixContent.WriteUInt32(RequiredLevel);
                    record.HotfixContent.WriteUInt8(SheatheType);
                    record.HotfixContent.WriteUInt16(RandomSelect);
                    record.HotfixContent.WriteUInt16(ItemRandomSuffixGroupID);
                    record.HotfixContent.WriteInt8(Sound_override_subclassID);
                    record.HotfixContent.WriteUInt16(ScalingStatDistributionID);
                    record.HotfixContent.WriteInt32(IconFileDataID);
                    record.HotfixContent.WriteUInt8(ItemGroupSoundsID);
                    record.HotfixContent.WriteInt32(ContentTuningID);
                    record.HotfixContent.WriteUInt32(MaxDurability);
                    record.HotfixContent.WriteUInt8(AmmunitionType);
                    record.HotfixContent.WriteUInt8(DamageType1);
                    record.HotfixContent.WriteUInt8(DamageType2);
                    record.HotfixContent.WriteUInt8(DamageType3);
                    record.HotfixContent.WriteUInt8(DamageType4);
                    record.HotfixContent.WriteUInt8(DamageType5);
                    record.HotfixContent.WriteInt16(Resistances1);
                    record.HotfixContent.WriteInt16(Resistances2);
                    record.HotfixContent.WriteInt16(Resistances3);
                    record.HotfixContent.WriteInt16(Resistances4);
                    record.HotfixContent.WriteInt16(Resistances5);
                    record.HotfixContent.WriteInt16(Resistances6);
                    record.HotfixContent.WriteInt16(Resistances7);
                    record.HotfixContent.WriteUInt16(MinDamage1);
                    record.HotfixContent.WriteUInt16(MinDamage2);
                    record.HotfixContent.WriteUInt16(MinDamage3);
                    record.HotfixContent.WriteUInt16(MinDamage4);
                    record.HotfixContent.WriteUInt16(MinDamage5);
                    record.HotfixContent.WriteUInt16(MaxDamage1);
                    record.HotfixContent.WriteUInt16(MaxDamage2);
                    record.HotfixContent.WriteUInt16(MaxDamage3);
                    record.HotfixContent.WriteUInt16(MaxDamage4);
                    record.HotfixContent.WriteUInt16(MaxDamage5);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }

        public static void WriteItemHotfix(ItemTemplate item, Framework.IO.ByteBuffer buffer)
        {
            int fileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);

            buffer.WriteUInt8((byte)item.Class);
            buffer.WriteUInt8((byte)item.SubClass);
            buffer.WriteUInt8((byte)item.Material);
            buffer.WriteInt8((sbyte)item.InventoryType);
            buffer.WriteInt32((int)item.RequiredLevel);
            buffer.WriteUInt8((byte)item.SheathType);
            buffer.WriteUInt16((ushort)item.RandomProperty);
            buffer.WriteUInt16((ushort)item.RandomSuffix);
            buffer.WriteInt8(-1);
            buffer.WriteUInt16(0);
            buffer.WriteInt32(fileDataId);
            buffer.WriteUInt8(0);
            buffer.WriteInt32(0);
            buffer.WriteUInt32(item.MaxDurability);
            buffer.WriteUInt8((byte)item.AmmoType);
            buffer.WriteUInt8((byte)item.DamageTypes[0]);
            buffer.WriteUInt8((byte)item.DamageTypes[1]);
            buffer.WriteUInt8((byte)item.DamageTypes[2]);
            buffer.WriteUInt8((byte)item.DamageTypes[3]);
            buffer.WriteUInt8((byte)item.DamageTypes[4]);
            buffer.WriteInt16((short)item.Armor);
            buffer.WriteInt16((short)item.HolyResistance);
            buffer.WriteInt16((short)item.FireResistance);
            buffer.WriteInt16((short)item.NatureResistance);
            buffer.WriteInt16((short)item.FrostResistance);
            buffer.WriteInt16((short)item.ShadowResistance);
            buffer.WriteInt16((short)item.ArcaneResistance);
            buffer.WriteUInt16((ushort)item.DamageMins[0]);
            buffer.WriteUInt16((ushort)item.DamageMins[1]);
            buffer.WriteUInt16((ushort)item.DamageMins[2]);
            buffer.WriteUInt16((ushort)item.DamageMins[3]);
            buffer.WriteUInt16((ushort)item.DamageMins[4]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
        }

        public static void WriteItemHotfix(ItemRecord row, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8(row.ClassId);
            buffer.WriteUInt8(row.SubclassId);
            buffer.WriteUInt8(row.Material);
            buffer.WriteInt8(row.InventoryType);
            buffer.WriteInt32(row.RequiredLevel);
            buffer.WriteUInt8(row.SheatheType);
            buffer.WriteUInt16(row.RandomProperty);
            buffer.WriteUInt16(row.ItemRandomSuffixGroupId);
            buffer.WriteInt8(row.SoundOverrideSubclassId);
            buffer.WriteUInt16(row.ScalingStatDistributionId);
            buffer.WriteInt32(row.IconFileDataId);
            buffer.WriteUInt8(row.ItemGroupSoundsId);
            buffer.WriteInt32(row.ContentTuningId);
            buffer.WriteUInt32(row.MaxDurability);
            buffer.WriteUInt8(row.AmmoType);
            buffer.WriteUInt8(row.DamageType[0]);
            buffer.WriteUInt8(row.DamageType[1]);
            buffer.WriteUInt8(row.DamageType[2]);
            buffer.WriteUInt8(row.DamageType[3]);
            buffer.WriteUInt8(row.DamageType[4]);
            buffer.WriteInt16(row.Resistances[0]);
            buffer.WriteInt16(row.Resistances[1]);
            buffer.WriteInt16(row.Resistances[2]);
            buffer.WriteInt16(row.Resistances[3]);
            buffer.WriteInt16(row.Resistances[4]);
            buffer.WriteInt16(row.Resistances[5]);
            buffer.WriteInt16(row.Resistances[6]);
            buffer.WriteUInt16(row.MinDamage[0]);
            buffer.WriteUInt16(row.MinDamage[1]);
            buffer.WriteUInt16(row.MinDamage[2]);
            buffer.WriteUInt16(row.MinDamage[3]);
            buffer.WriteUInt16(row.MinDamage[4]);
            buffer.WriteUInt16(row.MaxDamage[0]);
            buffer.WriteUInt16(row.MaxDamage[1]);
            buffer.WriteUInt16(row.MaxDamage[2]);
            buffer.WriteUInt16(row.MaxDamage[3]);
            buffer.WriteUInt16(row.MaxDamage[4]);
        }

        public static void WriteItemAppearanceHotfix(ItemAppearance appearance, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8(appearance.DisplayType);
            buffer.WriteInt32(appearance.ItemDisplayInfoID);
            buffer.WriteInt32(appearance.DefaultIconFileDataID);
            buffer.WriteInt32(appearance.UiOrder);
        }

        public static void WriteItemModifiedAppearanceHotfix(ItemModifiedAppearance modAppearance, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteInt32(modAppearance.Id);
            buffer.WriteInt32(modAppearance.ItemID);
            buffer.WriteInt32(modAppearance.ItemAppearanceModifierID);
            buffer.WriteInt32(modAppearance.ItemAppearanceID);
            buffer.WriteInt32(modAppearance.OrderIndex);
            buffer.WriteInt32(modAppearance.TransmogSourceTypeEnum);
        }

        public static void WriteItemEffectHotfix(ItemEffect effect, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8(effect.LegacySlotIndex);
            buffer.WriteInt8(effect.TriggerType);
            buffer.WriteInt16(effect.Charges);
            buffer.WriteInt32(effect.CoolDownMSec);
            buffer.WriteInt32(effect.CategoryCoolDownMSec);
            buffer.WriteUInt16(effect.SpellCategoryID);
            buffer.WriteInt32(effect.SpellID);
            buffer.WriteUInt16(effect.ChrSpecializationID);
            buffer.WriteInt32(effect.ParentItemID);
        }

        public static List<HotfixRecord> FindHotfixesByRecordIdAndTable(uint id, DB2Hash table, uint startId = 0)
        {
            return Hotfixes.Values.Where(hotfix => hotfix.HotfixId >= startId && hotfix.TableHash == table && hotfix.RecordId == id).ToList();
        }

        public static void UpdateHotfix(object obj, bool remove = false)
        {
            void DoStuff(uint recordId, DB2Hash table, Action<ByteBuffer> writer)
            {
                List<HotfixRecord> oldRecords = FindHotfixesByRecordIdAndTable(recordId, table, HotfixItemBegin);
                if (oldRecords.Count == 0)
                {
                    // We have a new entry
                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = table;
                    record.HotfixId = GetFirstFreeId(Hotfixes, HotfixItemBegin);
                    record.UniqueId = record.HotfixId;
                    record.RecordId = recordId;
                    writer(record.HotfixContent);
                    Hotfixes.Add(record.HotfixId, record);
                }
                else
                {
                    IEnumerable<HotfixRecord> oldRecordsToBeInvalided = oldRecords.SkipLast(1);
                    foreach (var record in oldRecordsToBeInvalided) // TODO maybe just delete these?
                    {
                        record.Status = HotfixStatus.Invalid;
                        record.HotfixContent = new();
                        Log.Print(LogType.Storage, $"Got duplicate record for record {record.RecordId} in {record.TableHash}");
                    }

                    HotfixRecord recordToOverwrite = oldRecords.Last();
                    recordToOverwrite.HotfixContent = new();
                    writer(recordToOverwrite.HotfixContent);
                    Hotfixes[recordToOverwrite.HotfixId] = recordToOverwrite;
                }
            }
            
            if (obj is ItemRecord)
            {
                ItemRecord item = (ItemRecord)obj;
                DoStuff((uint)item.Id, DB2Hash.Item, writer: (hotfixContentTargetBuffer) => WriteItemHotfix(item, hotfixContentTargetBuffer));
                
            }
            if (obj is ItemSparseRecord)
            {
                ItemSparseRecord itemSparse = (ItemSparseRecord)obj;
                DoStuff((uint)itemSparse.Id, DB2Hash.ItemSparse, writer: (hotfixContentTargetBuffer) => WriteItemSparseHotfix(itemSparse, hotfixContentTargetBuffer));
            }
            if (obj is ItemEffect)
            {
                ItemEffect effect = (ItemEffect)obj;
                DoStuff((uint)effect.Id, DB2Hash.ItemEffect, writer: (hotfixContentTargetBuffer) => WriteItemEffectHotfix(effect, hotfixContentTargetBuffer));
            }
            if (obj is ItemAppearance)
            {
                ItemAppearance appearance = (ItemAppearance)obj;
                DoStuff((uint)appearance.Id, DB2Hash.ItemAppearance, writer: (hotfixContentTargetBuffer) => WriteItemAppearanceHotfix(appearance, hotfixContentTargetBuffer));
            }
            if (obj is ItemModifiedAppearance)
            {
                ItemModifiedAppearance modAppearance = (ItemModifiedAppearance)obj;
                DoStuff((uint)modAppearance.Id, DB2Hash.ItemModifiedAppearance, writer: (hotfixContentTargetBuffer) => WriteItemModifiedAppearanceHotfix(modAppearance, hotfixContentTargetBuffer));
            }
        }

        public static Server.Packets.HotFixMessage? GenerateItemUpdateIfNeeded(ItemTemplate item)
        {
            ItemRecord row;
            ItemRecordsStore.TryGetValue(item.Entry, out row);
            if (row != null)
            {
                int iconFileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
                if (row.ClassId != (byte)item.Class ||
                    row.SubclassId != (byte)item.SubClass ||
                    row.Material != (byte)item.Material ||
                    row.InventoryType != (sbyte)item.InventoryType ||
                    row.RequiredLevel != (int)item.RequiredLevel ||
                    row.SheatheType != (byte)item.SheathType ||
                    row.RandomProperty != (ushort)item.RandomProperty ||
                    row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix ||
                    row.IconFileDataId != iconFileDataId && iconFileDataId != 0 ||
                    row.MaxDurability != item.MaxDurability ||
                    row.AmmoType != (byte)item.AmmoType ||
                    row.DamageType[0] != (byte)item.DamageTypes[0] ||
                    row.DamageType[1] != (byte)item.DamageTypes[1] ||
                    row.DamageType[2] != (byte)item.DamageTypes[2] ||
                    row.DamageType[3] != (byte)item.DamageTypes[3] ||
                    row.DamageType[4] != (byte)item.DamageTypes[4] ||
                    //row.MinDamage[0] != (ushort)item.DamageMins[0] ||
                    //row.MinDamage[1] != (ushort)item.DamageMins[1] ||
                    //row.MinDamage[2] != (ushort)item.DamageMins[2] ||
                    //row.MinDamage[3] != (ushort)item.DamageMins[3] ||
                    //row.MinDamage[4] != (ushort)item.DamageMins[4] ||
                    //row.MaxDamage[0] != (ushort)item.DamageMaxs[0] ||
                    //row.MaxDamage[1] != (ushort)item.DamageMaxs[1] ||
                    //row.MaxDamage[2] != (ushort)item.DamageMaxs[2] ||
                    //row.MaxDamage[3] != (ushort)item.DamageMaxs[3] ||
                    //row.MaxDamage[4] != (ushort)item.DamageMaxs[4] ||
                    //row.Resistances[0] != (short)item.Armor ||
                    row.Resistances[1] != (short)item.HolyResistance ||
                    row.Resistances[2] != (short)item.FireResistance ||
                    row.Resistances[3] != (short)item.NatureResistance ||
                    row.Resistances[4] != (short)item.FrostResistance ||
                    row.Resistances[5] != (short)item.ShadowResistance ||
                    row.Resistances[6] != (short)item.ArcaneResistance)
                {
                    Log.Print(LogType.Storage, $"Item #{item.Entry} needs to be updated.");

                    string msg;
                    if (row.ClassId != (byte)item.Class)
                        Log.Print(LogType.Storage, $"ClassId {row.ClassId} vs {item.Class}");
                    if (row.SubclassId != (byte)item.SubClass)
                        Log.Print(LogType.Storage, $"SubclassId {row.SubclassId} vs {item.SubClass}");
                    if (row.Material != (byte)item.Material)
                        Log.Print(LogType.Storage, $"Material {row.Material} vs {item.Material}");
                    if (row.InventoryType != (sbyte)item.InventoryType)
                        Log.Print(LogType.Storage, $"InventoryType {row.InventoryType} vs {item.InventoryType}");
                    if (row.RequiredLevel != (int)item.RequiredLevel)
                        Log.Print(LogType.Storage, $"RequiredLevel {row.RequiredLevel} vs {item.RequiredLevel}");
                    if (row.SheatheType != (byte)item.SheathType)
                        Log.Print(LogType.Storage, $"SheatheType {row.SheatheType} vs {item.SheathType}");
                    if (row.RandomProperty != (ushort)item.RandomProperty)
                        Log.Print(LogType.Storage, $"RandomProperty {row.RandomProperty} vs {item.RandomProperty}");
                    if (row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix)
                        Log.Print(LogType.Storage, $"ItemRandomSuffixGroupId {row.ItemRandomSuffixGroupId} vs {item.RandomSuffix}");
                    if (row.IconFileDataId != iconFileDataId)
                        Log.Print(LogType.Storage, $"IconFileDataId {row.IconFileDataId} vs {iconFileDataId}");
                    if (row.MaxDurability != item.MaxDurability)
                        Log.Print(LogType.Storage, $"MaxDurability {row.MaxDurability} vs {item.MaxDurability}");
                    if (row.AmmoType != (byte)item.AmmoType)
                        Log.Print(LogType.Storage, $"AmmoType {row.AmmoType} vs {item.AmmoType}");
                    for (int i = 0; i < 5; i++)
                    {
                        if (row.DamageType[i] != (byte)item.DamageTypes[i])
                            Log.Print(LogType.Storage, $"DamageType[{i}] {row.DamageType[i]} vs {item.DamageTypes[i]}");
                    }
                    if (row.Resistances[1] != (short)item.HolyResistance)
                        Log.Print(LogType.Storage, $"Resistances[1] {row.Resistances[1]} vs {item.HolyResistance}");
                    if (row.Resistances[2] != (short)item.FireResistance)
                        Log.Print(LogType.Storage, $"Resistances[2] {row.Resistances[2]} vs {item.FireResistance}");
                    if (row.Resistances[3] != (short)item.NatureResistance)
                        Log.Print(LogType.Storage, $"Resistances[3] {row.Resistances[3]} vs {item.NatureResistance}");
                    if (row.Resistances[4] != (short)item.FrostResistance)
                        Log.Print(LogType.Storage, $"Resistances[4] {row.Resistances[4]} vs {item.FrostResistance}");
                    if (row.Resistances[5] != (short)item.ShadowResistance)
                        Log.Print(LogType.Storage, $"Resistances[5] {row.Resistances[5]} vs {item.ShadowResistance}");
                    if (row.Resistances[6] != (short)item.ArcaneResistance)
                        Log.Print(LogType.Storage, $"Resistances[6] {row.Resistances[6]} vs {item.ArcaneResistance}");

                    // something is different so update current data
                    UpdateItemRecord(row, item);
                    UpdateHotfix(row);                    
                    return GenerateHotFixMessage(row);
                }
            }
            else
            {
                // item is missing so add new record
                //Log.Print(LogType.Storage, $"Item #{item.Entry} needs to be created.");
                row = AddItemRecord(item);
                if (row == null)
                    return null;

                UpdateHotfix(row);
                return GenerateHotFixMessage(row);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemSparseUpdateIfNeeded(ItemTemplate item)
        {
            ItemSparseRecord row;
            ItemSparseRecordsStore.TryGetValue(item.Entry, out row);
            if (row != null)
            {
                if (//row.AllowableRace != item.AllowedRaces ||
                    !row.Description.Equals(item.Description) ||
                    !row.Name4.Equals(item.Name[3]) ||
                    !row.Name3.Equals(item.Name[2]) ||
                    !row.Name2.Equals(item.Name[1]) ||
                    !row.Name1.Equals(item.Name[0]) ||
                    row.DurationInInventory != item.Duration ||
                    row.BagFamily != item.BagFamily ||
                    row.RangeMod != item.RangedMod ||
                    //row.Stackable != item.MaxStackSize ||
                    //row.MaxCount != item.MaxCount ||
                    row.RequiredAbility != item.RequiredSpell ||
                    row.SellPrice != item.SellPrice ||
                    row.BuyPrice != item.BuyPrice ||
                    //row.Flags[0] != item.Flags ||
                    //row.Flags[1] != item.FlagsExtra ||
                    row.MaxDurability != item.MaxDurability ||
                    row.RequiredHoliday != (ushort)item.HolidayID ||
                    row.LimitCategory != (ushort)item.ItemLimitCategory ||
                    row.GemProperties != (ushort)item.GemProperties ||
                    row.SocketMatchEnchantmentId != (ushort)item.SocketBonus ||
                    row.TotemCategoryId != (ushort)item.TotemCategory ||
                    row.InstanceBound != (ushort)item.MapID ||
                    row.ZoneBound[0] != (ushort)item.AreaID ||
                    row.ItemSet != (ushort)item.ItemSet ||
                    row.LockId != (ushort)item.LockId ||
                    row.StartQuestId != (ushort)item.StartQuestId ||
                    row.PageText != (ushort)item.PageText ||
                    row.Delay != (ushort)item.Delay ||
                    row.RequiredReputationId != (ushort)item.RequiredRepFaction ||
                    row.RequiredSkillRank != (ushort)item.RequiredSkillLevel ||
                    row.RequiredSkill != (ushort)item.RequiredSkillId ||
                    row.ItemLevel != (ushort)item.ItemLevel ||
                    //row.AllowableClass != (short)item.AllowedClasses ||
                    row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix ||
                    row.RandomProperty != (ushort)item.RandomProperty ||
                    //row.MinDamage[0] != (ushort)item.DamageMins[0] ||
                    //row.MinDamage[1] != (ushort)item.DamageMins[1] ||
                    //row.MinDamage[2] != (ushort)item.DamageMins[2] ||
                    //row.MinDamage[3] != (ushort)item.DamageMins[3] ||
                    //row.MinDamage[4] != (ushort)item.DamageMins[4] ||
                    //row.MaxDamage[0] != (ushort)item.DamageMaxs[0] ||
                    //row.MaxDamage[1] != (ushort)item.DamageMaxs[1] ||
                    //row.MaxDamage[2] != (ushort)item.DamageMaxs[2] ||
                    //row.MaxDamage[3] != (ushort)item.DamageMaxs[3] ||
                    //row.MaxDamage[4] != (ushort)item.DamageMaxs[4] ||
                    //row.Resistances[0] != (short)item.Armor ||
                    row.Resistances[1] != (short)item.HolyResistance ||
                    row.Resistances[2] != (short)item.FireResistance ||
                    row.Resistances[3] != (short)item.NatureResistance ||
                    row.Resistances[4] != (short)item.FrostResistance ||
                    row.Resistances[5] != (short)item.ShadowResistance ||
                    row.Resistances[6] != (short)item.ArcaneResistance ||
                    row.ScalingStatDistributionId != (ushort)item.ScalingStatDistribution ||
                    row.SocketType[0] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[0]) ||
                    row.SocketType[1] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[1]) ||
                    row.SocketType[2] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[2]) ||
                    row.SheatheType != (byte)item.SheathType ||
                    row.Material != (byte)item.Material ||
                    row.PageMaterial != (byte)item.PageMaterial ||
                    row.PageLanguage != (byte)item.Language ||
                    row.Bonding != (byte)item.Bonding ||
                    row.DamageType != (byte)item.DamageTypes[0] ||
                    row.StatType[0] != (sbyte)item.StatTypes[0] && (row.StatValue[0] != 0 || item.StatValues[0] != 0) ||
                    row.StatType[1] != (sbyte)item.StatTypes[1] && (row.StatValue[1] != 0 || item.StatValues[1] != 0) ||
                    row.StatType[2] != (sbyte)item.StatTypes[2] && (row.StatValue[2] != 0 || item.StatValues[2] != 0) ||
                    row.StatType[3] != (sbyte)item.StatTypes[3] && (row.StatValue[3] != 0 || item.StatValues[3] != 0) ||
                    row.StatType[4] != (sbyte)item.StatTypes[4] && (row.StatValue[4] != 0 || item.StatValues[4] != 0) ||
                    row.StatType[5] != (sbyte)item.StatTypes[5] && (row.StatValue[5] != 0 || item.StatValues[5] != 0) ||
                    row.StatType[6] != (sbyte)item.StatTypes[6] && (row.StatValue[6] != 0 || item.StatValues[6] != 0) ||
                    row.StatType[7] != (sbyte)item.StatTypes[7] && (row.StatValue[7] != 0 || item.StatValues[7] != 0) ||
                    row.StatType[8] != (sbyte)item.StatTypes[8] && (row.StatValue[8] != 0 || item.StatValues[8] != 0) ||
                    row.StatType[9] != (sbyte)item.StatTypes[9] && (row.StatValue[9] != 0 || item.StatValues[9] != 0) ||
                    row.ContainerSlots != (byte)item.ContainerSlots ||
                    row.RequiredReputationRank != (byte)item.RequiredRepValue ||
                    row.RequiredCityRank != (byte)item.RequiredCityRank ||
                    row.RequiredHonorRank != (byte)item.RequiredHonorRank ||
                    row.InventoryType != (byte)item.InventoryType ||
                    row.OverallQualityId != (byte)item.Quality ||
                    row.AmmoType != (byte)item.AmmoType ||
                    row.StatValue[0] != (sbyte)item.StatValues[0] ||
                    row.StatValue[1] != (sbyte)item.StatValues[1] ||
                    row.StatValue[2] != (sbyte)item.StatValues[2] ||
                    row.StatValue[3] != (sbyte)item.StatValues[3] ||
                    row.StatValue[4] != (sbyte)item.StatValues[4] ||
                    row.StatValue[5] != (sbyte)item.StatValues[5] ||
                    row.StatValue[6] != (sbyte)item.StatValues[6] ||
                    row.StatValue[7] != (sbyte)item.StatValues[7] ||
                    row.StatValue[8] != (sbyte)item.StatValues[8] ||
                    row.StatValue[9] != (sbyte)item.StatValues[9] ||
                    row.RequiredLevel != (sbyte)item.RequiredLevel)
                {
                    Log.Print(LogType.Storage, $"ItemSparse #{item.Entry} needs to be updated.");

                    if (!row.Description.Equals(item.Description))
                        Log.Print(LogType.Storage, $"Description \"{row.Description}\" vs \"{item.Description}\"");
                    if (!row.Name4.Equals(item.Name[3]))
                        Log.Print(LogType.Storage, $"Name4 \"{row.Name4}\" vs \"{item.Name[3]}\"");
                    if (!row.Name3.Equals(item.Name[2]))
                        Log.Print(LogType.Storage, $"Name3 \"{row.Name3}\" vs \"{item.Name[2]}\"");
                    if (!row.Name2.Equals(item.Name[1]))
                        Log.Print(LogType.Storage, $"Name2 \"{row.Name2}\" vs \"{item.Name[1]}\"");
                    if (!row.Name1.Equals(item.Name[0]))
                        Log.Print(LogType.Storage, $"Name1 \"{row.Name1}\" vs \"{item.Name[0]}\"");
                    if (row.DurationInInventory != item.Duration)
                        Log.Print(LogType.Storage, $"DurationInInventory {row.DurationInInventory} vs {item.Duration}");
                    if (row.BagFamily != item.BagFamily)
                        Log.Print(LogType.Storage, $"BagFamily {row.BagFamily} vs {item.BagFamily}");
                    if (row.RangeMod != item.RangedMod)
                        Log.Print(LogType.Storage, $"RangeMod {row.RangeMod} vs {item.RangedMod}");
                    if (row.RequiredAbility != item.RequiredSpell)
                        Log.Print(LogType.Storage, $"RequiredAbility {row.RequiredAbility} vs {item.RequiredSpell}");
                    if (row.SellPrice != item.SellPrice)
                        Log.Print(LogType.Storage, $"SellPrice {row.SellPrice} vs {item.SellPrice}");
                    if (row.BuyPrice != item.BuyPrice)
                        Log.Print(LogType.Storage, $"BuyPrice {row.BuyPrice} vs {item.BuyPrice}");
                    if (row.MaxDurability != item.MaxDurability)
                        Log.Print(LogType.Storage, $"MaxDurability {row.MaxDurability} vs {item.MaxDurability}");
                    if (row.RequiredHoliday != (ushort)item.HolidayID)
                        Log.Print(LogType.Storage, $"RequiredHoliday {row.RequiredHoliday} vs {item.HolidayID}");
                    if (row.LimitCategory != (ushort)item.ItemLimitCategory)
                        Log.Print(LogType.Storage, $"LimitCategory {row.LimitCategory} vs {item.ItemLimitCategory}");
                    if (row.GemProperties != (ushort)item.GemProperties)
                        Log.Print(LogType.Storage, $"GemProperties {row.GemProperties} vs {item.GemProperties}");
                    if (row.SocketMatchEnchantmentId != (ushort)item.SocketBonus)
                        Log.Print(LogType.Storage, $"SocketMatchEnchantmentId {row.SocketMatchEnchantmentId} vs {item.SocketBonus}");
                    if (row.TotemCategoryId != (ushort)item.TotemCategory)
                        Log.Print(LogType.Storage, $"TotemCategoryId {row.TotemCategoryId} vs {item.TotemCategory}");
                    if (row.InstanceBound != (ushort)item.MapID)
                        Log.Print(LogType.Storage, $"InstanceBound {row.InstanceBound} vs {item.MapID}");
                    if (row.ZoneBound[0] != (ushort)item.AreaID)
                        Log.Print(LogType.Storage, $"ZoneBound[0] {row.ZoneBound[0]} vs {item.AreaID}");
                    if (row.ItemSet != (ushort)item.ItemSet)
                        Log.Print(LogType.Storage, $"ItemSet {row.ItemSet} vs {item.ItemSet}");
                    if (row.LockId != (ushort)item.LockId)
                        Log.Print(LogType.Storage, $"LockId {row.LockId} vs {item.LockId}");
                    if (row.StartQuestId != (ushort)item.StartQuestId)
                        Log.Print(LogType.Storage, $"StartQuestId {row.StartQuestId} vs {item.StartQuestId}");
                    if (row.PageText != (ushort)item.PageText)
                        Log.Print(LogType.Storage, $"PageText {row.PageText} vs {item.PageText}");
                    if (row.Delay != (ushort)item.Delay)
                        Log.Print(LogType.Storage, $"Delay {row.Delay} vs {item.Delay}");
                    if (row.RequiredReputationId != (ushort)item.RequiredRepFaction)
                        Log.Print(LogType.Storage, $"RequiredReputationId {row.RequiredReputationId} vs {item.RequiredRepFaction}");
                    if (row.RequiredSkillRank != (ushort)item.RequiredSkillLevel)
                        Log.Print(LogType.Storage, $"RequiredSkillRank {row.RequiredSkillRank} vs {item.RequiredSkillLevel}");
                    if (row.RequiredSkill != (ushort)item.RequiredSkillId)
                        Log.Print(LogType.Storage, $"RequiredSkill {row.RequiredSkill} vs {item.RequiredSkillId}");
                    if (row.ItemLevel != (ushort)item.ItemLevel)
                        Log.Print(LogType.Storage, $"ItemLevel {row.ItemLevel} vs {item.ItemLevel}");
                    if (row.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix)
                        Log.Print(LogType.Storage, $"ItemRandomSuffixGroupId {row.ItemRandomSuffixGroupId} vs {item.RandomSuffix}");
                    if (row.RandomProperty != (ushort)item.RandomProperty)
                        Log.Print(LogType.Storage, $"RandomProperty {row.RandomProperty} vs {item.RandomProperty}");
                    if (row.Resistances[1] != (short)item.HolyResistance)
                        Log.Print(LogType.Storage, $"Resistances[1] {row.Resistances[1]} vs {item.HolyResistance}");
                    if (row.Resistances[2] != (short)item.FireResistance)
                        Log.Print(LogType.Storage, $"Resistances[2] {row.Resistances[2]} vs {item.FireResistance}");
                    if (row.Resistances[3] != (short)item.NatureResistance)
                        Log.Print(LogType.Storage, $"Resistances[3]  {row.Resistances[3]} vs {item.NatureResistance}");
                    if (row.Resistances[4] != (short)item.FrostResistance)
                        Log.Print(LogType.Storage, $"Resistances[4] {row.Resistances[4]} vs {item.FrostResistance}");
                    if (row.Resistances[5] != (short)item.ShadowResistance)
                        Log.Print(LogType.Storage, $"Resistances[5] {row.Resistances[5]} vs {item.ShadowResistance}");
                    if (row.Resistances[6] != (short)item.ArcaneResistance)
                        Log.Print(LogType.Storage, $"Resistances[6] {row.Resistances[6]} vs {item.ArcaneResistance}");
                    if (row.ScalingStatDistributionId != (ushort)item.ScalingStatDistribution)
                        Log.Print(LogType.Storage, $"ScalingStatDistributionId {row.ScalingStatDistributionId} vs {item.ScalingStatDistribution}");
                    for (int i = 0; i < 3; i++)
                    {
                        if (row.SocketType[i] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[i]))
                            Log.Print(LogType.Storage, $"SocketType[{i}] {row.SocketType[i]} vs {ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[i])}");
                    }
                    if (row.SheatheType != (byte)item.SheathType)
                        Log.Print(LogType.Storage, $"SheatheType {row.SheatheType} vs {item.SheathType}");
                    if (row.Material != (byte)item.Material)
                        Log.Print(LogType.Storage, $"Material {row.Material} vs {item.Material}");
                    if (row.PageMaterial != (byte)item.PageMaterial)
                        Log.Print(LogType.Storage, $"PageMaterial {row.PageMaterial} vs {item.PageMaterial}");
                    if (row.PageLanguage != (byte)item.Language)
                        Log.Print(LogType.Storage, $"PageLanguage {row.PageLanguage} vs {item.Language}");
                    if (row.Bonding != (byte)item.Bonding)
                        Log.Print(LogType.Storage, $"Bonding {row.Bonding} vs {item.Bonding}");
                    if (row.DamageType != (byte)item.DamageTypes[0])
                        Log.Print(LogType.Storage, $"DamageType {row.DamageType} vs {item.DamageTypes[0]}");
                    for (int i = 0; i < 10; i++)
                    {
                        if (row.StatType[i] != (sbyte)item.StatTypes[i] && (row.StatValue[i] != 0 || item.StatValues[i] != 0))
                            Log.Print(LogType.Storage, $"StatType[{i}] {row.StatType[i]} vs {item.StatTypes[i]}");
                    }
                    if (row.ContainerSlots != (byte)item.ContainerSlots)
                        Log.Print(LogType.Storage, $"ContainerSlots {row.ContainerSlots} vs {item.ContainerSlots}");
                    if (row.RequiredReputationRank != (byte)item.RequiredRepValue)
                        Log.Print(LogType.Storage, $"RequiredReputationRank {row.RequiredReputationRank} vs {item.RequiredRepValue}");
                    if (row.RequiredCityRank != (byte)item.RequiredCityRank)
                        Log.Print(LogType.Storage, $"RequiredCityRank {row.RequiredCityRank} vs {item.RequiredCityRank}");
                    if (row.RequiredHonorRank != (byte)item.RequiredHonorRank)
                        Log.Print(LogType.Storage, $"RequiredHonorRank {row.RequiredHonorRank} vs {item.RequiredHonorRank}");
                    if (row.InventoryType != (byte)item.InventoryType)
                        Log.Print(LogType.Storage, $"InventoryType {row.InventoryType} vs {item.InventoryType}");
                    if (row.OverallQualityId != (byte)item.Quality)
                        Log.Print(LogType.Storage, $"OverallQualityId {row.OverallQualityId} vs {item.Quality}");
                    if (row.AmmoType != (byte)item.AmmoType)
                        Log.Print(LogType.Storage, $"AmmoType {row.AmmoType} vs {item.AmmoType}");
                    for (int i = 0; i < 10; i++)
                    {
                        if (row.StatValue[0] != (sbyte)item.StatValues[0])
                            Log.Print(LogType.Storage, $"StatValue[{i}] {row.StatValue[i]} vs {item.StatValues[i]}");
                    }
                    if (row.RequiredLevel != (sbyte)item.RequiredLevel)
                        Log.Print(LogType.Storage, $"RequiredLevel {row.RequiredLevel} vs {item.RequiredLevel}");

                    // something is different so update current data
                    UpdateItemSparseRecord(row, item);

                    // sending db reply for existing itemsparse entry crashes game, so prepare hotfix for next login
                    UpdateHotfix(row);
                    return null;
                }
            }
            else
            {
                // item is missing so add new record
                //Log.Print(LogType.Storage, $"ItemSparse #{item.Entry} needs to be created.");
                row = AddItemSparseRecord(item);
                if (row == null)
                    return null;

                UpdateHotfix(row);
                return GenerateHotFixMessage(row);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemEffectUpdateIfNeeded(ItemTemplate item, byte slot)
        {
            ItemEffect effect = GetItemEffectByItemId(item.Entry, slot);
            if (effect != null)
            {
                // compare to spell data
                bool wrongCategory = false;
                bool wrongCooldown = false;
                bool wrongCatCooldown = false;
                if (item.TriggeredSpellIds[slot] > 0)
                {
                    ItemSpellsData data;
                    ItemSpellsDataStore.TryGetValue((uint)item.TriggeredSpellIds[slot], out data);
                    if (data != null)
                    {
                        // category
                        if (effect.SpellCategoryID != item.TriggeredSpellCategories[slot])
                            wrongCategory = data.Category != item.TriggeredSpellCategories[slot];
                        // cooldown
                        if (Math.Abs(effect.CoolDownMSec - item.TriggeredSpellCooldowns[slot]) > 1)
                            wrongCooldown = data.RecoveryTime != item.TriggeredSpellCooldowns[slot];
                        // category cooldown
                        if (Math.Abs(effect.CategoryCoolDownMSec - item.TriggeredSpellCategoryCooldowns[slot]) > 1)
                            wrongCatCooldown = data.CategoryRecoveryTime != item.TriggeredSpellCategoryCooldowns[slot];
                    }
                }
                if (effect.TriggerType != item.TriggeredSpellTypes[slot] ||
                    effect.Charges != item.TriggeredSpellCharges[slot] ||
                    wrongCooldown ||
                    wrongCatCooldown ||
                    wrongCategory ||
                    effect.SpellID != item.TriggeredSpellIds[slot])
                {
                    if (item.TriggeredSpellIds[slot] > 0)
                    {
                        Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be updated.");

                        if (effect.TriggerType != item.TriggeredSpellTypes[slot])
                            Log.Print(LogType.Storage, $"TriggerType {effect.TriggerType} vs {item.TriggeredSpellTypes[slot]}");
                        if (effect.Charges != item.TriggeredSpellCharges[slot])
                            Log.Print(LogType.Storage, $"Charges {effect.Charges} vs {item.TriggeredSpellCharges[slot]}");
                        if (wrongCooldown)
                            Log.Print(LogType.Storage, $"CoolDownMSec {effect.CoolDownMSec} vs {item.TriggeredSpellCooldowns[slot]}");
                        if (wrongCatCooldown)
                            Log.Print(LogType.Storage, $"CategoryCoolDownMSec {effect.CategoryCoolDownMSec} vs {item.TriggeredSpellCategoryCooldowns[slot]}");
                        if (wrongCategory)
                            Log.Print(LogType.Storage, $"SpellCategoryId {effect.SpellCategoryID} vs {item.TriggeredSpellCategories[slot]}");
                        if (effect.SpellID != item.TriggeredSpellIds[slot])
                            Log.Print(LogType.Storage, $"SpellId {effect.SpellID} vs {item.TriggeredSpellIds[slot]}");

                        effect.TriggerType = (sbyte)item.TriggeredSpellTypes[slot];
                        effect.Charges = (short)item.TriggeredSpellCharges[slot];
                        effect.CoolDownMSec = wrongCooldown ? item.TriggeredSpellCooldowns[slot] : -1;
                        effect.CategoryCoolDownMSec = wrongCatCooldown ? item.TriggeredSpellCategoryCooldowns[slot] : -1;
                        effect.SpellCategoryID = wrongCategory ? (ushort)item.TriggeredSpellCategories[slot] : (ushort)0;
                        effect.SpellID = item.TriggeredSpellIds[slot];

                        // there is a spell so update current data
                        UpdateItemEffectRecord(effect, item);
                        UpdateHotfix(effect);
                        return GenerateHotFixMessage(effect);
                    }
                    else
                    {
                        // there is no spell so remove the record
                        //Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be deleted.");
                        RemoveItemEffectRecord(effect);
                        UpdateHotfix(effect, true);
                        return GenerateHotFixMessage(effect, true);
                    }
                }
            }
            else if (item.TriggeredSpellIds[slot] > 0)
            {
                // there is a spell so add new record
                //Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be created.");
                effect = AddItemEffectRecord(item, slot);
                if (effect == null)
                    return null;

                UpdateHotfix(effect);
                return GenerateHotFixMessage(effect);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemAppearanceUpdateIfNeeded(ItemTemplate item)
        {
            ItemAppearance appearance = GetItemAppearanceByDisplayId(item.DisplayID);
            if (appearance != null)
            {
                // never can happen, should not edit existing ItemAppearance as can affect other items

                /*if (appearance.ItemDisplayInfoID != item.DisplayID)
                {
                    Log.Print(LogType.Storage, $"ItemAppearance for item #{item.Entry}, DisplayID #{item.DisplayID} needs to be updated.");
                    Log.Print(LogType.Storage, $"ItemDisplayID {appearance.ItemDisplayInfoID} vs {item.DisplayID}");

                    // something is different so update current data
                    UpdateItemAppearanceRecord(appearance, item);
                    UpdateItemAppearanceHotfix(appearance);
                    return GenerateDbReply(appearance);
                }*/
            }
            else
            {
                // item appearance is missing so add new record
                //Log.Print(LogType.Storage, $"ItemAppearance for item #{item.Entry}, DisplayID #{item.DisplayID} needs to be created.");
                appearance = AddItemAppearanceRecord(item);
                if (appearance == null)
                    return null;

                UpdateHotfix(appearance);
                return GenerateHotFixMessage(appearance);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateItemModifiedAppearanceUpdateIfNeeded(ItemTemplate item)
        {
            ItemModifiedAppearance modAppearance = GetItemModifiedAppearanceByItemId(item.Entry);
            if (modAppearance != null)
            {
                ItemAppearance appearance;
                ItemAppearanceStore.TryGetValue((uint)modAppearance.ItemAppearanceID, out appearance);
                if (appearance == null || appearance.ItemDisplayInfoID != item.DisplayID)
                {
                    Log.Print(LogType.Storage, $"ItemModifiedAppearance #{modAppearance.Id} for item #{item.Entry} needs to be updated.");
                    
                    if (appearance == null)
                        Log.Print(LogType.Storage, $"ItemAppearance #{modAppearance.ItemAppearanceID} missing.");
                    else if (appearance.ItemDisplayInfoID != item.DisplayID)
                        Log.Print(LogType.Storage, $"DisplayID {appearance.ItemDisplayInfoID} vs {item.DisplayID}");

                    // something is different so update current data
                    UpdateItemModifiedAppearanceRecord(modAppearance, item);
                    UpdateHotfix(modAppearance);
                    return GenerateHotFixMessage(modAppearance);
                }
            }
            else
            {
                // item modified appearance is missing so add new record
                //Log.Print(LogType.Storage, $"ItemModifiedAppearance for item #{item.Entry} needs to be created.");
                modAppearance = AddItemModifiedAppearanceRecord(item);
                if (modAppearance == null)
                    return null;

                UpdateHotfix(modAppearance);
                return GenerateHotFixMessage(modAppearance);
            }
            return null;
        }

        public static Server.Packets.HotFixMessage? GenerateHotFixMessage(object obj, bool remove = false)
        {
            Server.Packets.HotFixMessage reply = new();

            if (obj == null)
            {
                Log.Print(LogType.Error, $"DBReply for NULL object requested!");
                return null;
            }
            System.Type type = obj.GetType();
            //Log.Print(LogType.Storage, $"DBReply generating for {type}");
            if (obj is ItemRecord)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)((ItemRecord)obj).Id, DB2Hash.Item);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemSparseRecord)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)((ItemSparseRecord)obj).Id, DB2Hash.ItemSparse);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemEffect)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)((ItemEffect)obj).Id, DB2Hash.ItemEffect);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemAppearance)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)((ItemAppearance)obj).Id, DB2Hash.ItemAppearance);
                reply.Hotfixes.AddRange(records);
            }
            else if (obj is ItemModifiedAppearance)
            {
                var records = FindHotfixesByRecordIdAndTable((uint)((ItemModifiedAppearance)obj).Id, DB2Hash.ItemModifiedAppearance);
                reply.Hotfixes.AddRange(records);
            }
            else
            {
                Log.Print(LogType.Error, $"Unsupported DBReply requested! ({type})");
                return null;
            }
            return reply;
        }

        public static ItemRecord AddItemRecord(ItemTemplate item)
        {
            ItemRecord record = new();
            record.Id = (int)item.Entry;
            UpdateItemRecord(record, item);
            ItemRecordsStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"Item #{record.Id} created.");
            return record;
        }

        public static void UpdateItemRecord(ItemRecord row, ItemTemplate item)
        {
            row.ClassId = (byte)item.Class;
            row.SubclassId = (byte)item.SubClass;
            row.Material = (byte)item.Material;
            row.InventoryType = (sbyte)item.InventoryType;
            row.RequiredLevel = (int)item.RequiredLevel;
            row.SheatheType = (byte)item.SheathType;
            row.RandomProperty = (ushort)item.RandomProperty;
            row.ItemRandomSuffixGroupId = (ushort)item.RandomSuffix;
            row.SoundOverrideSubclassId = -1;
            row.ScalingStatDistributionId = 0;
            row.IconFileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
            row.ItemGroupSoundsId = 0;
            row.ContentTuningId = 0;
            row.MaxDurability = item.MaxDurability;
            row.AmmoType = (byte)item.AmmoType;
            row.DamageType[0] = (byte)item.DamageTypes[0];
            row.DamageType[1] = (byte)item.DamageTypes[1];
            row.DamageType[2] = (byte)item.DamageTypes[2];
            row.DamageType[3] = (byte)item.DamageTypes[3];
            row.DamageType[4] = (byte)item.DamageTypes[4];
            row.Resistances[0] = (short)item.Armor;
            row.Resistances[1] = (short)item.HolyResistance;
            row.Resistances[2] = (short)item.FireResistance;
            row.Resistances[3] = (short)item.NatureResistance;
            row.Resistances[4] = (short)item.FrostResistance;
            row.Resistances[5] = (short)item.ShadowResistance;
            row.Resistances[6] = (short)item.ArcaneResistance;
            row.MinDamage[0] = (ushort)item.DamageMins[0];
            row.MinDamage[1] = (ushort)item.DamageMins[1];
            row.MinDamage[2] = (ushort)item.DamageMins[2];
            row.MinDamage[3] = (ushort)item.DamageMins[3];
            row.MinDamage[4] = (ushort)item.DamageMins[4];
            row.MaxDamage[0] = (ushort)item.DamageMaxs[0];
            row.MaxDamage[1] = (ushort)item.DamageMaxs[1];
            row.MaxDamage[2] = (ushort)item.DamageMaxs[2];
            row.MaxDamage[3] = (ushort)item.DamageMaxs[3];
            row.MaxDamage[4] = (ushort)item.DamageMaxs[4];

            if (ItemRecordsStore.ContainsKey(item.Entry))
            {
                ItemRecordsStore[item.Entry] = row;
                //Log.Print(LogType.Storage, $"Item #{row.Id} updated.");
            }
        }

        public static ItemSparseRecord AddItemSparseRecord(ItemTemplate item)
        {
            ItemSparseRecord record = new();
            record.Id = (int)item.Entry;
            UpdateItemSparseRecord(record, item);
            ItemSparseRecordsStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemSparse #{record.Id} created.");
            return record;
        }

        public static void UpdateItemSparseRecord(ItemSparseRecord row, ItemTemplate item)
        {
            int[] StatValues = new int[10];
            for (int i = 0; i < item.StatsCount; i++)
            {
                StatValues[i] = item.StatValues[i];
                if (StatValues[i] > 127)
                    StatValues[i] = 127;
                if (StatValues[i] < -127)
                    StatValues[i] = -127;
            }

            row.AllowableRace = item.AllowedRaces;
            row.Description = item.Description;
            row.Name4 = item.Name[3];
            row.Name3 = item.Name[2];
            row.Name2 = item.Name[1];
            row.Name1 = item.Name[0];
            row.DurationInInventory = item.Duration;
            row.BagFamily = item.BagFamily;
            row.RangeMod = item.RangedMod;
            row.Stackable = item.MaxStackSize;
            row.MaxCount = item.MaxCount;
            row.RequiredAbility = item.RequiredSpell;
            row.SellPrice = item.SellPrice;
            row.BuyPrice = item.BuyPrice;
            row.Flags[0] = item.Flags;
            row.Flags[1] = item.FlagsExtra;
            row.MaxDurability = item.MaxDurability;
            row.RequiredHoliday = (ushort)item.HolidayID;
            row.LimitCategory = (ushort)item.ItemLimitCategory;
            row.GemProperties = (ushort)item.GemProperties;
            row.SocketMatchEnchantmentId = (ushort)item.SocketBonus;
            row.TotemCategoryId = (ushort)item.TotemCategory;
            row.InstanceBound = (ushort)item.MapID;
            row.ZoneBound[0] = (ushort)item.AreaID;
            row.ItemSet = (ushort)item.ItemSet;
            row.LockId = (ushort)item.LockId;
            row.StartQuestId = (ushort)item.StartQuestId;
            row.PageText = (ushort)item.PageText;
            row.Delay = (ushort)item.Delay;
            row.RequiredReputationId = (ushort)item.RequiredRepFaction;
            row.RequiredSkillRank = (ushort)item.RequiredSkillLevel;
            row.RequiredSkill = (ushort)item.RequiredSkillId;
            row.ItemLevel = (ushort)item.ItemLevel;
            row.AllowableClass = (short)item.AllowedClasses;
            row.ItemRandomSuffixGroupId = (ushort)item.RandomSuffix;
            row.RandomProperty = (ushort)item.RandomProperty;
            row.MinDamage[0] = (ushort)item.DamageMins[0];
            row.MinDamage[1] = (ushort)item.DamageMins[1];
            row.MinDamage[2] = (ushort)item.DamageMins[2];
            row.MinDamage[3] = (ushort)item.DamageMins[3];
            row.MinDamage[4] = (ushort)item.DamageMins[4];
            row.MaxDamage[0] = (ushort)item.DamageMaxs[0];
            row.MaxDamage[1] = (ushort)item.DamageMaxs[1];
            row.MaxDamage[2] = (ushort)item.DamageMaxs[2];
            row.MaxDamage[3] = (ushort)item.DamageMaxs[3];
            row.MaxDamage[4] = (ushort)item.DamageMaxs[4];
            row.Resistances[0] = (short)item.Armor;
            row.Resistances[1] = (short)item.HolyResistance;
            row.Resistances[2] = (short)item.FireResistance;
            row.Resistances[3] = (short)item.NatureResistance;
            row.Resistances[4] = (short)item.FrostResistance;
            row.Resistances[5] = (short)item.ShadowResistance;
            row.Resistances[6] = (short)item.ArcaneResistance;
            row.ScalingStatDistributionId = (ushort)item.ScalingStatDistribution;
            row.SocketType[0] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[0]);
            row.SocketType[1] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[1]);
            row.SocketType[2] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[2]);
            row.SheatheType = (byte)item.SheathType;
            row.Material = (byte)item.Material;
            row.PageMaterial = (byte)item.PageMaterial;
            row.PageLanguage = (byte)item.Language;
            row.Bonding = (byte)item.Bonding;
            row.DamageType = (byte)item.DamageTypes[0];
            row.StatType[0] = (sbyte)item.StatTypes[0];
            row.StatType[1] = (sbyte)item.StatTypes[1];
            row.StatType[2] = (sbyte)item.StatTypes[2];
            row.StatType[3] = (sbyte)item.StatTypes[3];
            row.StatType[4] = (sbyte)item.StatTypes[4];
            row.StatType[5] = (sbyte)item.StatTypes[5];
            row.StatType[6] = (sbyte)item.StatTypes[6];
            row.StatType[7] = (sbyte)item.StatTypes[7];
            row.StatType[8] = (sbyte)item.StatTypes[8];
            row.StatType[9] = (sbyte)item.StatTypes[9];
            row.ContainerSlots = (byte)item.ContainerSlots;
            row.RequiredReputationRank = (byte)item.RequiredRepValue;
            row.RequiredCityRank = (byte)item.RequiredCityRank;
            row.RequiredHonorRank = (byte)item.RequiredHonorRank;
            row.InventoryType = (byte)item.InventoryType;
            row.OverallQualityId = (byte)item.Quality;
            row.AmmoType = (byte)item.AmmoType;
            row.StatValue[0] = (sbyte)StatValues[0];
            row.StatValue[1] = (sbyte)StatValues[1];
            row.StatValue[2] = (sbyte)StatValues[2];
            row.StatValue[3] = (sbyte)StatValues[3];
            row.StatValue[4] = (sbyte)StatValues[4];
            row.StatValue[5] = (sbyte)StatValues[5];
            row.StatValue[6] = (sbyte)StatValues[6];
            row.StatValue[7] = (sbyte)StatValues[7];
            row.StatValue[8] = (sbyte)StatValues[8];
            row.StatValue[9] = (sbyte)StatValues[9];
            row.RequiredLevel = (sbyte)item.RequiredLevel;

            if (ItemSparseRecordsStore.ContainsKey(item.Entry))
            {
                ItemSparseRecordsStore[item.Entry] = row;
                //Log.Print(LogType.Storage, $"ItemSparse #{row.Id} updated.");
            }
        }

        public static ItemEffect AddItemEffectRecord(ItemTemplate item, byte slot)
        {
            ItemEffect record = new();
            record.Id = (int)GetFirstFreeId(ItemEffectStore);
            record.LegacySlotIndex = slot;
            UpdateItemEffectRecord(record, item);
            ItemEffectStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemEffect #{record.Id} created for item #{item.Entry} slot #{slot}.");
            return record;
        }

        public static void UpdateItemEffectRecord(ItemEffect effect, ItemTemplate item)
        {
            byte i = effect.LegacySlotIndex;
            effect.TriggerType = (sbyte)item.TriggeredSpellTypes[i];
            effect.Charges = (short)item.TriggeredSpellCharges[i];
            effect.CoolDownMSec = item.TriggeredSpellCooldowns[i];
            effect.CategoryCoolDownMSec = item.TriggeredSpellCategoryCooldowns[i];
            effect.SpellCategoryID = (ushort)item.TriggeredSpellCategories[i];
            effect.SpellID = item.TriggeredSpellIds[i];
            effect.ChrSpecializationID = 0;
            effect.ParentItemID = (int)item.Entry;

            if (ItemEffectStore.ContainsKey((uint)effect.Id))
            {
                ItemEffectStore[(uint)effect.Id] = effect;
                //Log.Print(LogType.Storage, $"ItemEffect #{effect.Id} updated for item #{item.Entry} slot #{i}.");
            }
        }

        public static void RemoveItemEffectRecord(ItemEffect effect)
        {
            ItemEffectStore.Remove((uint)effect.Id);
            Log.Print(LogType.Storage, $"ItemEffect #{effect.Id} removed for item #{effect.ParentItemID} slot #{effect.LegacySlotIndex}.");
        }

        public static ItemAppearance AddItemAppearanceRecord(ItemTemplate item)
        {
            ItemAppearance record = new();
            record.Id = (int)GetFirstFreeId(ItemAppearanceStore);
            UpdateItemAppearanceRecord(record, item);
            ItemAppearanceStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemAppearance #{record.Id} created for DisplayID #{item.DisplayID}.");
            return record;
        }

        public static void UpdateItemAppearanceRecord(ItemAppearance appearance, ItemTemplate item)
        {
            int fileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);

            appearance.DisplayType = 11; // todo find out
            appearance.ItemDisplayInfoID = (int)item.DisplayID;
            appearance.DefaultIconFileDataID = fileDataId;
            appearance.UiOrder = 0;

            if (ItemAppearanceStore.ContainsKey((uint)appearance.Id))
            {
                ItemAppearanceStore[(uint)appearance.Id] = appearance;
                //Log.Print(LogType.Storage, $"ItemAppearance #{appearance.Id} updated for DisplayID #{item.DisplayID}.");
            }
        }

        public static ItemModifiedAppearance AddItemModifiedAppearanceRecord(ItemTemplate item)
        {
            ItemModifiedAppearance record = new();
            record.Id = (int)GetFirstFreeId(ItemModifiedAppearanceStore);
            UpdateItemModifiedAppearanceRecord(record, item);
            if (record.ItemID != item.Entry)
            {
                Log.Print(LogType.Error, $"ItemModifiedAppearance #{record.Id} create failed for item #{record.ItemID}.");
                return null;
            }
            ItemModifiedAppearanceStore.Add((uint)record.Id, record);
            Log.Print(LogType.Storage, $"ItemModifiedAppearance #{record.Id} created for item #{record.ItemID}.");
            return record;
        }

        public static void UpdateItemModifiedAppearanceRecord(ItemModifiedAppearance modAppearance, ItemTemplate item)
        {
            ItemAppearance appearance = GetItemAppearanceByDisplayId(item.DisplayID);
            if (appearance == null) // should not happen
            {
                Log.Print(LogType.Error, $"ItemModifiedAppearance #{modAppearance.Id} update failed: no ItemAppearance for DisplayID #{item.DisplayID}");
                return;
            }    

            modAppearance.ItemID = (int)item.Entry;
            modAppearance.ItemAppearanceModifierID = 0;
            modAppearance.ItemAppearanceID = appearance.Id;
            modAppearance.OrderIndex = 0;
            modAppearance.TransmogSourceTypeEnum = 0;

            if (ItemModifiedAppearanceStore.ContainsKey((uint)modAppearance.Id))
            {
                ItemModifiedAppearanceStore[(uint)modAppearance.Id] = modAppearance;
                //Log.Print(LogType.Storage, $"ItemModifiedAppearance #{modAppearance.Id} updated for item #{item.Entry}.");
            }
        }

        public static bool ItemCanHaveModel(ItemTemplate item)
        {
            // weapons
            if (item.Class == 2)
                return true;

            // armor (except necklaces, rings, trinkets, relics)
            if (item.Class == 4)
            {
                if (item.SubClass != 7 &&
                    item.SubClass != 8 &&
                    item.SubClass != 9 &&
                    item.InventoryType != 0 &&
                    item.InventoryType != 2 &&
                    item.InventoryType != 11 &&
                    item.InventoryType != 12 &&
                    item.InventoryType != 18 &&
                    item.InventoryType != 28)
                    return true;
            }

            // quivers
            if (item.Class == 11 && item.SubClass == 2)
                return true;

            return false;
        }

        public static void LoadCreatureDisplayInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfo{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoExtra{ModernVersion.ExpansionVersion}.csv");
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
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoOption{ModernVersion.ExpansionVersion}.csv");
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
        public static void LoadItemEffectHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemEffect{ModernVersion.ExpansionVersion}.csv");
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

                    uint id = uint.Parse(fields[0]);
                    byte legacySlotIndex = byte.Parse(fields[1]);
                    byte triggerType = byte.Parse(fields[2]);
                    short charges = short.Parse(fields[3]);
                    int coolDownMSec = int.Parse(fields[4]);
                    int categoryCoolDownMSec = int.Parse(fields[5]);
                    short spellCategoryId = short.Parse(fields[6]);
                    int spellId = int.Parse(fields[7]);
                    short chrSpecializationId = short.Parse(fields[8]);
                    int parentItemId = int.Parse(fields[9]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.ItemEffect;
                    record.HotfixId = HotfixItemEffectBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteUInt8(legacySlotIndex);
                    record.HotfixContent.WriteUInt8(triggerType);
                    record.HotfixContent.WriteInt16(charges);
                    record.HotfixContent.WriteInt32(coolDownMSec);
                    record.HotfixContent.WriteInt32(categoryCoolDownMSec);
                    record.HotfixContent.WriteInt16(spellCategoryId);
                    record.HotfixContent.WriteInt32(spellId);
                    record.HotfixContent.WriteInt16(chrSpecializationId);
                    record.HotfixContent.WriteInt32(parentItemId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }

        public static void LoadItemDisplayInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemDisplayInfo{ModernVersion.ExpansionVersion}.csv");
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

                    uint id = uint.Parse(fields[0]);
                    int itemVisual = int.Parse(fields[1]);
                    int particleColorID = int.Parse(fields[2]);
                    uint itemRangedDisplayInfoID = uint.Parse(fields[3]);
                    uint overrideSwooshSoundKitID = uint.Parse(fields[4]);
                    int sheatheTransformMatrixID = int.Parse(fields[5]);
                    int stateSpellVisualKitID = int.Parse(fields[6]);
                    int sheathedSpellVisualKitID = int.Parse(fields[7]);
                    uint unsheathedSpellVisualKitID = uint.Parse(fields[8]);
                    int flags = int.Parse(fields[9]);
                    uint modelResourcesID1 = uint.Parse(fields[10]);
                    uint modelResourcesID2 = uint.Parse(fields[11]);
                    int modelMaterialResourcesID1 = int.Parse(fields[12]);
                    int modelMaterialResourcesID2 = int.Parse(fields[13]);
                    int modelType1 = int.Parse(fields[14]);
                    int modelType2 = int.Parse(fields[15]);
                    int geosetGroup1 = int.Parse(fields[16]);
                    int geosetGroup2 = int.Parse(fields[17]);
                    int geosetGroup3 = int.Parse(fields[18]);
                    int geosetGroup4 = int.Parse(fields[19]);
                    int geosetGroup5 = int.Parse(fields[20]);
                    int geosetGroup6 = int.Parse(fields[21]);
                    int attachmentGeosetGroup1 = int.Parse(fields[22]);
                    int attachmentGeosetGroup2 = int.Parse(fields[23]);
                    int attachmentGeosetGroup3 = int.Parse(fields[24]);
                    int attachmentGeosetGroup4 = int.Parse(fields[25]);
                    int attachmentGeosetGroup5 = int.Parse(fields[26]);
                    int attachmentGeosetGroup6 = int.Parse(fields[27]);
                    int helmetGeosetVis1 = int.Parse(fields[28]);
                    int helmetGeosetVis2 = int.Parse(fields[29]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.ItemDisplayInfo;
                    record.HotfixId = HotfixItemDisplayInfoBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteInt32(itemVisual);
                    record.HotfixContent.WriteInt32(particleColorID);
                    record.HotfixContent.WriteUInt32(itemRangedDisplayInfoID);
                    record.HotfixContent.WriteUInt32(overrideSwooshSoundKitID);
                    record.HotfixContent.WriteInt32(sheatheTransformMatrixID);
                    record.HotfixContent.WriteInt32(stateSpellVisualKitID);
                    record.HotfixContent.WriteInt32(sheathedSpellVisualKitID);
                    record.HotfixContent.WriteUInt32(unsheathedSpellVisualKitID);
                    record.HotfixContent.WriteInt32(flags);
                    record.HotfixContent.WriteUInt32(modelResourcesID1);
                    record.HotfixContent.WriteUInt32(modelResourcesID2);
                    record.HotfixContent.WriteInt32(modelMaterialResourcesID1);
                    record.HotfixContent.WriteInt32(modelMaterialResourcesID2);
                    record.HotfixContent.WriteInt32(modelType1);
                    record.HotfixContent.WriteInt32(modelType2);
                    record.HotfixContent.WriteInt32(geosetGroup1);
                    record.HotfixContent.WriteInt32(geosetGroup2);
                    record.HotfixContent.WriteInt32(geosetGroup3);
                    record.HotfixContent.WriteInt32(geosetGroup4);
                    record.HotfixContent.WriteInt32(geosetGroup5);
                    record.HotfixContent.WriteInt32(geosetGroup6);
                    record.HotfixContent.WriteInt32(attachmentGeosetGroup1);
                    record.HotfixContent.WriteInt32(attachmentGeosetGroup2);
                    record.HotfixContent.WriteInt32(attachmentGeosetGroup3);
                    record.HotfixContent.WriteInt32(attachmentGeosetGroup4);
                    record.HotfixContent.WriteInt32(attachmentGeosetGroup5);
                    record.HotfixContent.WriteInt32(attachmentGeosetGroup6);
                    record.HotfixContent.WriteInt32(helmetGeosetVis1);
                    record.HotfixContent.WriteInt32(helmetGeosetVis2);
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

    public class ItemRecord
    {
        public int Id;
        public byte ClassId;
        public byte SubclassId;
        public byte Material;
        public sbyte InventoryType;
        public int RequiredLevel;
        public byte SheatheType;
        public ushort RandomProperty;
        public ushort ItemRandomSuffixGroupId;
        public sbyte SoundOverrideSubclassId;
        public ushort ScalingStatDistributionId;
        public int IconFileDataId;
        public byte ItemGroupSoundsId;
        public int ContentTuningId;
        public uint MaxDurability;
        public byte AmmoType;
        public byte[] DamageType = new byte[5];
        public short[] Resistances = new short[7];
        public ushort[] MinDamage = new ushort[5];
        public ushort[] MaxDamage = new ushort[5];
    }

    public class ItemSparseRecord
    {
        public int Id;
        public long AllowableRace;
        public string Description;
        public string Name4;
        public string Name3;
        public string Name2;
        public string Name1;
        public float DmgVariance = 1;
        public uint DurationInInventory;
        public float QualityModifier;
        public uint BagFamily;
        public float RangeMod;
        public float[] StatPercentageOfSocket = new float[10];
        public int[] StatPercentEditor = new int[10];
        public int Stackable;
        public int MaxCount;
        public uint RequiredAbility;
        public uint SellPrice;
        public uint BuyPrice;
        public uint VendorStackCount = 1;
        public float PriceVariance = 1;
        public float PriceRandomValue = 1;
        public uint[] Flags = new uint[4];
        public int OppositeFactionItemId;
        public uint MaxDurability;
        public ushort ItemNameDescriptionId;
        public ushort RequiredTransmogHoliday;
        public ushort RequiredHoliday;
        public ushort LimitCategory;
        public ushort GemProperties;
        public ushort SocketMatchEnchantmentId;
        public ushort TotemCategoryId;
        public ushort InstanceBound;
        public ushort[] ZoneBound = new ushort[2];
        public ushort ItemSet;
        public ushort LockId;
        public ushort StartQuestId;
        public ushort PageText;
        public ushort Delay;
        public ushort RequiredReputationId;
        public ushort RequiredSkillRank;
        public ushort RequiredSkill;
        public ushort ItemLevel;
        public short AllowableClass;
        public ushort ItemRandomSuffixGroupId;
        public ushort RandomProperty;
        public ushort[] MinDamage = new ushort[5];
        public ushort[] MaxDamage = new ushort[5];
        public short[] Resistances = new short[7];
        public ushort ScalingStatDistributionId;
        public byte ExpansionId = 254;
        public byte ArtifactId;
        public byte SpellWeight;
        public byte SpellWeightCategory;
        public byte[] SocketType = new byte[3];
        public byte SheatheType;
        public byte Material;
        public byte PageMaterial;
        public byte PageLanguage;
        public byte Bonding;
        public byte DamageType;
        public sbyte[] StatType = new sbyte[10];
        public byte ContainerSlots;
        public byte RequiredReputationRank;
        public byte RequiredCityRank;
        public byte RequiredHonorRank;
        public byte InventoryType;
        public byte OverallQualityId;
        public byte AmmoType;
        public sbyte[] StatValue = new sbyte[10];
        public sbyte RequiredLevel;
    }

    public class ItemAppearance
    {
        public int Id;
        public byte DisplayType;
        public int ItemDisplayInfoID;
        public int DefaultIconFileDataID;
        public int UiOrder;
    }
    public class ItemModifiedAppearance
    {
        public int Id;
        public int ItemID;
        public int ItemAppearanceModifierID;
        public int ItemAppearanceID;
        public int OrderIndex;
        public int TransmogSourceTypeEnum;
    }
    public class ItemEffect
    {
        public int Id;
        public byte LegacySlotIndex;
        public sbyte TriggerType;
        public short Charges;
        public int CoolDownMSec;
        public int CategoryCoolDownMSec;
        public ushort SpellCategoryID;
        public int SpellID;
        public ushort ChrSpecializationID;
        public int ParentItemID;
    }
    public class ItemSpellsData
    {
        public int Id;
        public int Category;
        public int RecoveryTime;
        public int CategoryRecoveryTime;
    }
    public class ItemDisplayData
    {
        public int Id;
        public int IconFileDataId;
        public int ModelResourcesId_1;
        public int ModelResourcesId_2;
        public int ModelMaterialResourcesId_1;
        public int ModelMaterialResourcesId_2;
    }
    public class Battleground
    {
        public bool IsArena;
        public List<uint> MapIds = new List<uint>();
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

    public record CreatureDisplayInfo(uint ModelId, float DisplayScale);
    public record CreatureModelCollisionHeight(float ModelScale, float Height, float MountHeight);

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
