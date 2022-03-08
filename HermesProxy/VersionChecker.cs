using Framework;
using HermesProxy.World.Enums;
using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Framework.Logging;
using HermesProxy.World.Objects;

namespace HermesProxy
{
    public class UpdateFieldInfo
    {
        public int Value;
        public string Name;
        public int Size;
        public UpdateFieldType Format;
    }
    public static class LegacyVersion
    {
        static LegacyVersion()
        {
            Build = Settings.ServerBuild;

            UpdateFieldDictionary = new Dictionary<Type, SortedList<int, UpdateFieldInfo>>();
            UpdateFieldNameDictionary = new Dictionary<Type, Dictionary<string, int>>();
            if (!LoadUFDictionariesInto(UpdateFieldDictionary, UpdateFieldNameDictionary))
                Log.Print(LogType.Error, "Could not load update fields for current server version.");
        }

        private static readonly Dictionary<Type, SortedList<int, UpdateFieldInfo>> UpdateFieldDictionary;
        private static readonly Dictionary<Type, Dictionary<string, int>> UpdateFieldNameDictionary;

        private static bool LoadUFDictionariesInto(Dictionary<Type, SortedList<int, UpdateFieldInfo>> dicts,
            Dictionary<Type, Dictionary<string, int>> nameToValueDict)
        {
            Type[] enumTypes =
            {
                typeof(ObjectField), typeof(ItemField), typeof(ContainerField), typeof(AzeriteEmpoweredItemField), typeof(AzeriteItemField), typeof(UnitField),
                typeof(PlayerField), typeof(ActivePlayerField), typeof(GameObjectField), typeof(DynamicObjectField),
                typeof(CorpseField), typeof(AreaTriggerField), typeof(SceneObjectField), typeof(ConversationField),
                typeof(ObjectDynamicField), typeof(ItemDynamicField), typeof(ContainerDynamicField), typeof(AzeriteEmpoweredItemDynamicField), typeof(AzeriteItemDynamicField), typeof(UnitDynamicField),
                typeof(PlayerDynamicField), typeof(ActivePlayerDynamicField), typeof(GameObjectDynamicField), typeof(DynamicObjectDynamicField),
                typeof(CorpseDynamicField), typeof(AreaTriggerDynamicField), typeof(SceneObjectDynamicField), typeof(ConversationDynamicField)
            };

            bool loaded = false;
            foreach (Type enumType in enumTypes)
            {
                string vTypeString =
                    $"HermesProxy.World.Enums.{Build.ToString()}.{enumType.Name}";
                Type vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                if (vEnumType == null)
                {
                    vTypeString =
                        $"HermesProxy.World.Enums.{Build.ToString()}.{enumType.Name}";
                    vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                    if (vEnumType == null)
                        continue;   // versions prior to 4.3.0 do not have AreaTriggerField
                }

                Array vValues = Enum.GetValues(vEnumType);
                var vNames = Enum.GetNames(vEnumType);

                var result = new SortedList<int, UpdateFieldInfo>(vValues.Length);
                var namesResult = new Dictionary<string, int>(vNames.Length);

                for (int i = 0; i < vValues.Length; ++i)
                {
                    var format = enumType.GetMember(vNames[i])
                        .SelectMany(member => member.GetCustomAttributes(typeof(UpdateFieldAttribute), false))
                        .Where(attribute => ((UpdateFieldAttribute)attribute).Version <= Build)
                        .OrderByDescending(attribute => ((UpdateFieldAttribute)attribute).Version)
                        .Select(attribute => ((UpdateFieldAttribute)attribute).UFAttribute)
                        .DefaultIfEmpty(UpdateFieldType.Default).First();

                    result.Add((int)vValues.GetValue(i), new UpdateFieldInfo() { Value = (int)vValues.GetValue(i), Name = vNames[i], Size = 0, Format = format });
                    namesResult.Add(vNames[i], (int)vValues.GetValue(i));
                }

                for (var i = 0; i < result.Count - 1; ++i)
                    result.Values[i].Size = result.Keys[i + 1] - result.Keys[i];

                dicts.Add(enumType, result);
                nameToValueDict.Add(enumType, namesResult);
                loaded = true;
            }

            return loaded;
        }

        public static int GetUpdateField<T>(T field) // where T: System.Enum // C# 7.3
        {
            Dictionary<string, int> byNamesDict;
            if (UpdateFieldNameDictionary.TryGetValue(typeof(T), out byNamesDict))
            {
                int fieldValue;
                if (byNamesDict.TryGetValue(field.ToString(), out fieldValue))
                    return fieldValue;
            }

            return -1;
        }

        public static string GetUpdateFieldName<T>(int field) // where T: System.Enum // C# 7.3
        {
            SortedList<int, UpdateFieldInfo> infoDict;
            if (UpdateFieldDictionary.TryGetValue(typeof(T), out infoDict))
            {
                if (infoDict.Count != 0)
                {
                    var index = infoDict.BinarySearch(field);
                    if (index >= 0)
                        return infoDict.Values[index].Name;

                    index = ~index - 1;
                    var start = infoDict.Keys[index];
                    return infoDict.Values[index].Name + " + " + (field - start);
                }
            }

            return field.ToString(CultureInfo.InvariantCulture);
        }

        public static UpdateFieldInfo GetUpdateFieldInfo<T>(int field) // where T: System.Enum // C# 7.3
        {
            SortedList<int, UpdateFieldInfo> infoDict;
            if (UpdateFieldDictionary.TryGetValue(typeof(T), out infoDict))
            {
                if (infoDict.Count != 0)
                {
                    var index = infoDict.BinarySearch(field);
                    if (index >= 0)
                        return infoDict.Values[index];

                    return infoDict.Values[~index - 1];
                }
            }

            return null;
        }

        public static ClientVersionBuild Build { get; private set; }

        public static int BuildInt => (int)Build;

        public static string VersionString => Build.ToString();

        public static byte GetExpansionVersion()
        {
            string str = VersionString;
            str = str.Replace("V", "");
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        public static byte GetMajorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        public static byte GetMinorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }

        public static bool InVersion(ClientVersionBuild build1, ClientVersionBuild build2)
        {
            return AddedInVersion(build1) && RemovedInVersion(build2);
        }

        public static bool AddedInVersion(ClientVersionBuild build)
        {
            return Build >= build;
        }

        public static bool RemovedInVersion(ClientVersionBuild build)
        {
            return Build < build;
        }

        public static int GetPowersCount()
        {
            if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                return 5;

            return 7;
        }

        public static int GetMaxLevel()
        {
            if (RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return 60;
            else if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                return 70;
            else
                return 80;
        }

        public static HitInfo ConvertHitInfoFlags(uint hitInfo)
        {
            if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                return ((HitInfoVanilla)hitInfo).CastFlags<HitInfo>();
            else
                return (HitInfo)hitInfo;
        }

        public static uint ConvertSpellCastResult(uint result)
        {
            if (AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                return (uint)Enum.Parse(typeof(SpellCastResultClassic), ((SpellCastResultWotLK)result).ToString());
            else if (AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                return (uint)Enum.Parse(typeof(SpellCastResultClassic), ((SpellCastResultTBC)result).ToString());
            else
                return (uint)Enum.Parse(typeof(SpellCastResultClassic), ((SpellCastResultVanilla)result).ToString());
        }

        public static QuestGiverStatusModern ConvertQuestGiverStatus(byte status)
        {
            if (AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                return (QuestGiverStatusModern)Enum.Parse(typeof(QuestGiverStatusModern), ((QuestGiverStatusWotLK)status).ToString());
            else if (AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                return (QuestGiverStatusModern)Enum.Parse(typeof(QuestGiverStatusModern), ((QuestGiverStatusTBC)status).ToString());
            else
                return (QuestGiverStatusModern)Enum.Parse(typeof(QuestGiverStatusModern), ((QuestGiverStatusVanilla)status).ToString());
        }
    }

    public static class ModernVersion
    {
        static ModernVersion()
        {
            Build = Settings.ClientBuild;

            UpdateFieldDictionary = new Dictionary<Type, SortedList<int, UpdateFieldInfo>>();
            UpdateFieldNameDictionary = new Dictionary<Type, Dictionary<string, int>>();
            if (!LoadUFDictionariesInto(UpdateFieldDictionary, UpdateFieldNameDictionary))
                Log.Print(LogType.Error, "Could not load update fields for current server version.");
        }

        private static readonly Dictionary<Type, SortedList<int, UpdateFieldInfo>> UpdateFieldDictionary;
        private static readonly Dictionary<Type, Dictionary<string, int>> UpdateFieldNameDictionary;

        private static bool LoadUFDictionariesInto(Dictionary<Type, SortedList<int, UpdateFieldInfo>> dicts,
            Dictionary<Type, Dictionary<string, int>> nameToValueDict)
        {
            Type[] enumTypes =
            {
                typeof(ObjectField), typeof(ItemField), typeof(ContainerField), typeof(AzeriteEmpoweredItemField), typeof(AzeriteItemField), typeof(UnitField),
                typeof(PlayerField), typeof(ActivePlayerField), typeof(GameObjectField), typeof(DynamicObjectField),
                typeof(CorpseField), typeof(AreaTriggerField), typeof(SceneObjectField), typeof(ConversationField),
                typeof(ObjectDynamicField), typeof(ItemDynamicField), typeof(ContainerDynamicField), typeof(AzeriteEmpoweredItemDynamicField), typeof(AzeriteItemDynamicField), typeof(UnitDynamicField),
                typeof(PlayerDynamicField), typeof(ActivePlayerDynamicField), typeof(GameObjectDynamicField), typeof(DynamicObjectDynamicField),
                typeof(CorpseDynamicField), typeof(AreaTriggerDynamicField), typeof(SceneObjectDynamicField), typeof(ConversationDynamicField)
            };

            bool loaded = false;
            foreach (Type enumType in enumTypes)
            {
                string vTypeString =
                    $"HermesProxy.World.Enums.{Build.ToString()}.{enumType.Name}";
                Type vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                if (vEnumType == null)
                {
                    vTypeString =
                        $"HermesProxy.World.Enums.{Build.ToString()}.{enumType.Name}";
                    vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                    if (vEnumType == null)
                        continue;   // versions prior to 4.3.0 do not have AreaTriggerField
                }

                Array vValues = Enum.GetValues(vEnumType);
                var vNames = Enum.GetNames(vEnumType);

                var result = new SortedList<int, UpdateFieldInfo>(vValues.Length);
                var namesResult = new Dictionary<string, int>(vNames.Length);

                for (int i = 0; i < vValues.Length; ++i)
                {
                    var format = enumType.GetMember(vNames[i])
                        .SelectMany(member => member.GetCustomAttributes(typeof(UpdateFieldAttribute), false))
                        .Where(attribute => ((UpdateFieldAttribute)attribute).Version <= Build)
                        .OrderByDescending(attribute => ((UpdateFieldAttribute)attribute).Version)
                        .Select(attribute => ((UpdateFieldAttribute)attribute).UFAttribute)
                        .DefaultIfEmpty(UpdateFieldType.Default).First();

                    result.Add((int)vValues.GetValue(i), new UpdateFieldInfo() { Value = (int)vValues.GetValue(i), Name = vNames[i], Size = 0, Format = format });
                    namesResult.Add(vNames[i], (int)vValues.GetValue(i));
                }

                for (var i = 0; i < result.Count - 1; ++i)
                    result.Values[i].Size = result.Keys[i + 1] - result.Keys[i];

                dicts.Add(enumType, result);
                nameToValueDict.Add(enumType, namesResult);
                loaded = true;
            }

            return loaded;
        }

        public static int GetUpdateField<T>(T field) // where T: System.Enum // C# 7.3
        {
            Dictionary<string, int> byNamesDict;
            if (UpdateFieldNameDictionary.TryGetValue(typeof(T), out byNamesDict))
            {
                int fieldValue;
                if (byNamesDict.TryGetValue(field.ToString(), out fieldValue))
                    return fieldValue;
            }

            return -1;
        }

        public static string GetUpdateFieldName<T>(int field) // where T: System.Enum // C# 7.3
        {
            SortedList<int, UpdateFieldInfo> infoDict;
            if (UpdateFieldDictionary.TryGetValue(typeof(T), out infoDict))
            {
                if (infoDict.Count != 0)
                {
                    var index = infoDict.BinarySearch(field);
                    if (index >= 0)
                        return infoDict.Values[index].Name;

                    index = ~index - 1;
                    var start = infoDict.Keys[index];
                    return infoDict.Values[index].Name + " + " + (field - start);
                }
            }

            return field.ToString(CultureInfo.InvariantCulture);
        }

        public static UpdateFieldInfo GetUpdateFieldInfo<T>(int field) // where T: System.Enum // C# 7.3
        {
            SortedList<int, UpdateFieldInfo> infoDict;
            if (UpdateFieldDictionary.TryGetValue(typeof(T), out infoDict))
            {
                if (infoDict.Count != 0)
                {
                    var index = infoDict.BinarySearch(field);
                    if (index >= 0)
                        return infoDict.Values[index];

                    return infoDict.Values[~index - 1];
                }
            }

            return null;
        }

        public static ClientVersionBuild Build { get; private set; }

        public static int BuildInt => (int)Build;

        public static string VersionString => Build.ToString();

        public static byte GetExpansionVersion()
        {
            string str = VersionString;
            str = str.Replace("V", "");
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        public static byte GetMajorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        public static byte GetMinorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }

        public static bool InVersion(ClientVersionBuild build1, ClientVersionBuild build2)
        {
            return AddedInVersion(build1) && RemovedInVersion(build2);
        }

        public static bool AddedInVersion(ClientVersionBuild build)
        {
            return Build >= build;
        }

        public static bool RemovedInVersion(ClientVersionBuild build)
        {
            return Build < build;
        }

        public static int GetAccountDataCount()
        {
            int count = (GetExpansionVersion() == 1) ? 10 : 8;
            return count;
        }

        public static byte AdjustInventorySlot(byte slot)
        {
            byte offset = 0;
            if (slot >= World.Objects.Classic.InventorySlots.BankItemStart && slot < World.Objects.Classic.InventorySlots.BankItemEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Objects.Classic.InventorySlots.BankItemStart - World.Objects.Vanilla.InventorySlots.BankItemStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Objects.Classic.InventorySlots.BankItemStart - World.Objects.TBC.InventorySlots.BankItemStart;
                else
                    offset = World.Objects.Classic.InventorySlots.BankItemStart - World.Objects.WotLK.InventorySlots.BankItemStart;
            }
            else if (slot >= World.Objects.Classic.InventorySlots.BankBagStart && slot < World.Objects.Classic.InventorySlots.BankBagEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Objects.Classic.InventorySlots.BankBagStart - World.Objects.Vanilla.InventorySlots.BankBagStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Objects.Classic.InventorySlots.BankBagStart - World.Objects.TBC.InventorySlots.BankBagStart;
                else
                    offset = World.Objects.Classic.InventorySlots.BankBagStart - World.Objects.WotLK.InventorySlots.BankBagStart;
            }
            else if (slot >= World.Objects.Classic.InventorySlots.BuyBackStart && slot < World.Objects.Classic.InventorySlots.BuyBackEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Objects.Classic.InventorySlots.BuyBackStart - World.Objects.Vanilla.InventorySlots.BuyBackStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Objects.Classic.InventorySlots.BuyBackStart - World.Objects.TBC.InventorySlots.BuyBackStart;
                else
                    offset = World.Objects.Classic.InventorySlots.BuyBackStart - World.Objects.WotLK.InventorySlots.BuyBackStart;
            }
            else if (slot >= World.Objects.Classic.InventorySlots.KeyringStart && slot < World.Objects.Classic.InventorySlots.KeyringEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Objects.Classic.InventorySlots.KeyringStart - World.Objects.Vanilla.InventorySlots.KeyringStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Objects.Classic.InventorySlots.KeyringStart - World.Objects.TBC.InventorySlots.KeyringStart;
                else
                    offset = World.Objects.Classic.InventorySlots.KeyringStart - World.Objects.WotLK.InventorySlots.KeyringStart;
            }
            return (byte)(slot - offset);
        }
        public static void ConvertAuraFlags(ushort oldFlags, byte slot, out AuraFlagsModern newFlags, out uint activeFlags)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                activeFlags = 0;
                newFlags = AuraFlagsModern.None;

                if (slot >= 32)
                    newFlags |= AuraFlagsModern.Negative;
                else
                    newFlags |= AuraFlagsModern.Positive;

                if (oldFlags.HasAnyFlag(AuraFlagsVanilla.Cancelable))
                    newFlags |= AuraFlagsModern.Cancelable;

                if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex0))
                    activeFlags |= 1;
                if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex1))
                    activeFlags |= 2;
                if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex2))
                    activeFlags |= 4;
            }
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                activeFlags = 1;
                newFlags = AuraFlagsModern.None;

                if (oldFlags.HasAnyFlag(AuraFlagsTBC.Negative))
                    newFlags |= AuraFlagsModern.Negative;
                else if (oldFlags.HasAnyFlag(AuraFlagsTBC.Positive))
                    newFlags |= AuraFlagsModern.Positive; 
                else if (oldFlags.HasAnyFlag(AuraFlagsTBC.NotCancelable))
                    newFlags |= AuraFlagsModern.Negative;

                if (oldFlags.HasAnyFlag(AuraFlagsTBC.Cancelable))
                    newFlags |= AuraFlagsModern.Cancelable;

                if (oldFlags.HasAnyFlag(AuraFlagsTBC.Passive))
                    newFlags |= AuraFlagsModern.Passive;
            }
            else
            {
                activeFlags = 0;
                newFlags = AuraFlagsModern.None;

                if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Negative))
                    newFlags |= AuraFlagsModern.Negative;
                else if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Positive))
                    newFlags |= (AuraFlagsModern.Positive | AuraFlagsModern.Cancelable);

                if (oldFlags.HasAnyFlag(AuraFlagsWotLK.NoCaster))
                    newFlags |= AuraFlagsModern.NoCaster;
                if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Duration))
                    newFlags |= AuraFlagsModern.Duration;

                if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex0))
                    activeFlags |= 1;
                if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex1))
                    activeFlags |= 2;
                if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex2))
                    activeFlags |= 4;
            }
        }
    }
}
