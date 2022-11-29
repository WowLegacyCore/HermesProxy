using Framework;
using HermesProxy.World.Enums;
using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Framework.Logging;

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

            ExpansionVersion = GetExpansionVersion();
            MajorVersion = GetMajorPatchVersion();
            MinorVersion = GetMinorPatchVersion();

            UpdateFieldDictionary = new Dictionary<Type, SortedList<int, UpdateFieldInfo>>();
            UpdateFieldNameDictionary = new Dictionary<Type, Dictionary<string, int>>();
            if (!LoadUFDictionariesInto(UpdateFieldDictionary, UpdateFieldNameDictionary))
                Log.Print(LogType.Error, "Could not load update fields for current legacy version.");
            if (!LoadOpcodeDictionaries())
                Log.Print(LogType.Error, "Could not load opcodes for current legacy version.");
        }

        private static readonly Dictionary<uint, Opcode> CurrentToUniversalOpcodeDictionary = new();
        private static readonly Dictionary<Opcode, uint> UniversalToCurrentOpcodeDictionary = new();

        private static bool LoadOpcodeDictionaries()
        {
            Type enumType = Opcodes.GetOpcodesEnumForVersion(Build);
            if (enumType == null)
                return false;

            foreach (var item in Enum.GetValues(enumType))
            {
                string oldOpcodeName = Enum.GetName(enumType, item);
                Opcode universalOpcode = Opcodes.GetUniversalOpcode(oldOpcodeName);
                if (universalOpcode == Opcode.MSG_NULL_ACTION &&
                    oldOpcodeName != "MSG_NULL_ACTION")
                {
                    Log.Print(LogType.Error, $"Opcode {oldOpcodeName} is missing from the universal opcode enum!");
                    continue;
                }

                CurrentToUniversalOpcodeDictionary.Add((uint)item, universalOpcode);
                UniversalToCurrentOpcodeDictionary.Add(universalOpcode, (uint)item);
            }

            if (CurrentToUniversalOpcodeDictionary.Count < 1)
                return false;

            Log.Print(LogType.Server, $"Loaded {CurrentToUniversalOpcodeDictionary.Count} legacy opcodes.");
            return true;
        }

        public static Opcode GetUniversalOpcode(uint opcode)
        {
            Opcode universalOpcode;
            if (CurrentToUniversalOpcodeDictionary.TryGetValue(opcode, out universalOpcode))
                return universalOpcode;
            return Opcode.MSG_NULL_ACTION;
        }

        public static uint GetCurrentOpcode(Opcode universalOpcode)
        {
            uint opcode;
            if (UniversalToCurrentOpcodeDictionary.TryGetValue(universalOpcode, out opcode))
                return opcode;
            return 0;
        }

        private static readonly Dictionary<Type, SortedList<int, UpdateFieldInfo>> UpdateFieldDictionary;
        private static readonly Dictionary<Type, Dictionary<string, int>> UpdateFieldNameDictionary;

        public static ClientVersionBuild GetUpdateFieldsDefiningBuild()
        {
            return GetUpdateFieldsDefiningBuild(Build);
        }

        public static ClientVersionBuild GetUpdateFieldsDefiningBuild(ClientVersionBuild version)
        {
            switch (version)
            {
                case ClientVersionBuild.V1_12_1_5875:
                    return ClientVersionBuild.V1_12_1_5875;
                case ClientVersionBuild.V2_4_3_8606:
                    return ClientVersionBuild.V2_4_3_8606;
                case ClientVersionBuild.V3_3_5a_12340:
                    return ClientVersionBuild.V3_3_5a_12340;
            }
            return ClientVersionBuild.Zero;
        }

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

            ClientVersionBuild ufDefiningBuild = GetUpdateFieldsDefiningBuild(Build);
            System.Diagnostics.Trace.Assert(ufDefiningBuild != ClientVersionBuild.Zero);

            bool loaded = false;
            foreach (Type enumType in enumTypes)
            {
                string vTypeString =
                    $"HermesProxy.World.Enums.{ufDefiningBuild.ToString()}.{enumType.Name}";
                Type vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                if (vEnumType == null)
                {
                    vTypeString =
                        $"HermesProxy.World.Enums.{ufDefiningBuild.ToString()}.{enumType.Name}";
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

        public static byte ExpansionVersion { get; private set; }
        public static byte MajorVersion { get; private set; }
        public static byte MinorVersion { get; private set; }

        public static ClientVersionBuild Build { get; private set; }

        public static int BuildInt => (int)Build;

        public static string VersionString => Build.ToString();

        private static byte GetExpansionVersion()
        {
            string str = VersionString;
            str = str.Replace("V", "");
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        private static byte GetMajorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        private static byte GetMinorPatchVersion()
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

        public static byte GetMaxLevel()
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

        public static InventoryResult ConvertInventoryResult(uint result)
        {
            if (RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return (InventoryResult)Enum.Parse(typeof(InventoryResult), ((InventoryResultVanilla)result).ToString());
            else
                return (InventoryResult)Enum.Parse(typeof(InventoryResult), ((InventoryResultTBC)result).ToString());

            return (InventoryResult)result;
        }

        public static int GetQuestLogSize()
        {
            return AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 25 : 20;  // 2.0.0.5849 Alpha
        }

        public static int GetAuraSlotsCount()
        {
            return AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 56 : 48;
        }
    }

    public static class ModernVersion
    {
        static ModernVersion()
        {
            Build = Settings.ClientBuild;

            ExpansionVersion = GetExpansionVersion();
            MajorVersion = GetMajorPatchVersion();
            MinorVersion = GetMinorPatchVersion();

            UpdateFieldDictionary = new Dictionary<Type, SortedList<int, UpdateFieldInfo>>();
            UpdateFieldNameDictionary = new Dictionary<Type, Dictionary<string, int>>();

            if (!LoadUFDictionariesInto(UpdateFieldDictionary, UpdateFieldNameDictionary))
                Log.Print(LogType.Error, "Could not load update fields for current modern version.");
            if (!LoadOpcodeDictionaries())
                Log.Print(LogType.Error, "Could not load opcodes for current modern version.");
        }

        private static readonly Dictionary<uint, Opcode> CurrentToUniversalOpcodeDictionary = new();
        private static readonly Dictionary<Opcode, uint> UniversalToCurrentOpcodeDictionary = new();

        private static bool LoadOpcodeDictionaries()
        {
            Type enumType = Opcodes.GetOpcodesEnumForVersion(Build);
            if (enumType == null)
                return false;

            foreach (var item in Enum.GetValues(enumType))
            {
                string oldOpcodeName = Enum.GetName(enumType, item);
                Opcode universalOpcode = Opcodes.GetUniversalOpcode(oldOpcodeName);
                if (universalOpcode == Opcode.MSG_NULL_ACTION &&
                    oldOpcodeName != "MSG_NULL_ACTION")
                {
                    Log.Print(LogType.Error, $"Opcode {oldOpcodeName} is missing from the universal opcode enum!");
                    continue;
                }

                CurrentToUniversalOpcodeDictionary.Add((uint)item, universalOpcode);
                UniversalToCurrentOpcodeDictionary.Add(universalOpcode, (uint)item);
            }

            if (CurrentToUniversalOpcodeDictionary.Count < 1)
                return false;

            Log.Print(LogType.Server, $"Loaded {CurrentToUniversalOpcodeDictionary.Count} modern opcodes.");
            return true;
        }

        public static Opcode GetUniversalOpcode(uint opcode)
        {
            Opcode universalOpcode;
            if (CurrentToUniversalOpcodeDictionary.TryGetValue(opcode, out universalOpcode))
                return universalOpcode;
            return Opcode.MSG_NULL_ACTION;
        }

        public static uint GetCurrentOpcode(Opcode universalOpcode)
        {
            uint opcode;
            if (UniversalToCurrentOpcodeDictionary.TryGetValue(universalOpcode, out opcode))
                return opcode;
            return 0;
        }

        private static readonly Dictionary<Type, SortedList<int, UpdateFieldInfo>> UpdateFieldDictionary;
        private static readonly Dictionary<Type, Dictionary<string, int>> UpdateFieldNameDictionary;

        public static ClientVersionBuild GetUpdateFieldsDefiningBuild()
        {
            return GetUpdateFieldsDefiningBuild(Build);
        }

        public static ClientVersionBuild GetUpdateFieldsDefiningBuild(ClientVersionBuild version)
        {
            switch (version)
            {
                case ClientVersionBuild.V1_14_0_39802:
                case ClientVersionBuild.V1_14_0_39958:
                case ClientVersionBuild.V1_14_0_40140:
                case ClientVersionBuild.V1_14_0_40179:
                case ClientVersionBuild.V1_14_0_40237:
                case ClientVersionBuild.V1_14_0_40347:
                case ClientVersionBuild.V1_14_0_40441:
                case ClientVersionBuild.V1_14_0_40618:
                    return ClientVersionBuild.V1_14_0_40237;
                case ClientVersionBuild.V1_14_1_40487:
                case ClientVersionBuild.V1_14_1_40594:
                case ClientVersionBuild.V1_14_1_40666:
                case ClientVersionBuild.V1_14_1_40688:
                case ClientVersionBuild.V1_14_1_40800:
                case ClientVersionBuild.V1_14_1_40818:
                case ClientVersionBuild.V1_14_1_40926:
                case ClientVersionBuild.V1_14_1_40962:
                case ClientVersionBuild.V1_14_1_41009:
                case ClientVersionBuild.V1_14_1_41030:
                case ClientVersionBuild.V1_14_1_41077:
                case ClientVersionBuild.V1_14_1_41137:
                case ClientVersionBuild.V1_14_1_41243:
                case ClientVersionBuild.V1_14_1_41511:
                case ClientVersionBuild.V1_14_1_41794:
                case ClientVersionBuild.V1_14_1_42032:
                    return ClientVersionBuild.V1_14_1_40688;
                case ClientVersionBuild.V2_5_2_39570:
                case ClientVersionBuild.V2_5_2_39618:
                case ClientVersionBuild.V2_5_2_39926:
                case ClientVersionBuild.V2_5_2_40011:
                case ClientVersionBuild.V2_5_2_40045:
                case ClientVersionBuild.V2_5_2_40203:
                case ClientVersionBuild.V2_5_2_40260:
                case ClientVersionBuild.V2_5_2_40422:
                case ClientVersionBuild.V2_5_2_40488:
                case ClientVersionBuild.V2_5_2_40617:
                case ClientVersionBuild.V2_5_2_40892:
                case ClientVersionBuild.V2_5_2_41446:
                case ClientVersionBuild.V2_5_2_41510:
                    return ClientVersionBuild.V2_5_2_39570;
            }
            return ClientVersionBuild.Zero;
        }

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

            ClientVersionBuild ufDefiningBuild = GetUpdateFieldsDefiningBuild(Build);
            System.Diagnostics.Trace.Assert(ufDefiningBuild != ClientVersionBuild.Zero);

            bool loaded = false;
            foreach (Type enumType in enumTypes)
            {
                string vTypeString =
                    $"HermesProxy.World.Enums.{ufDefiningBuild.ToString()}.{enumType.Name}";
                Type vEnumType = Assembly.GetExecutingAssembly().GetType(vTypeString);
                if (vEnumType == null)
                {
                    vTypeString =
                        $"HermesProxy.World.Enums.{ufDefiningBuild.ToString()}.{enumType.Name}";
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

        public static byte ExpansionVersion { get; private set; }
        public static byte MajorVersion { get; private set; }
        public static byte MinorVersion { get; private set; }

        public static ClientVersionBuild Build { get; private set; }

        public static int BuildInt => (int)Build;

        public static string VersionString => Build.ToString();

        private static byte GetExpansionVersion()
        {
            string str = VersionString;
            str = str.Replace("V", "");
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        private static byte GetMajorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }
        private static byte GetMinorPatchVersion()
        {
            string str = VersionString;
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(str.IndexOf('_') + 1);
            str = str.Substring(0, str.IndexOf("_"));
            return (byte)UInt32.Parse(str);
        }

        public static bool AddedInVersion(byte expansion, byte major, byte minor)
        {
            if (ExpansionVersion < expansion)
                return false;

            if (ExpansionVersion > expansion)
                return true;

            if (MajorVersion < major)
                return false;

            if (MajorVersion > major)
                return true;

            return MinorVersion >= minor;
        }

        public static bool AddedInVersion(byte retailExpansion, byte retailMajor, byte retailMinor, byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
        {
            if (ExpansionVersion == 1)
                return AddedInVersion(classicEraExpansion, classicEraMajor, classicEraMinor);
            else if (ExpansionVersion == 2 || ExpansionVersion == 3)
                return AddedInVersion(classicExpansion, classicMajor, classicMinor);

            return AddedInVersion(retailExpansion, retailMajor, retailMinor);
        }

        public static bool AddedInClassicVersion(byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
        {
            if (ExpansionVersion == 1)
                return AddedInVersion(classicEraExpansion, classicEraMajor, classicEraMinor);
            else if (ExpansionVersion == 2 || ExpansionVersion == 3)
                return AddedInVersion(classicExpansion, classicMajor, classicMinor);

            return false;
        }

        public static bool IsVersion(byte expansion, byte major, byte minor)
        {
            return ExpansionVersion == expansion && MajorVersion == major && MinorVersion == minor;
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

        public static bool IsClassicVersionBuild()
        {
            return ExpansionVersion == 1 && MajorVersion >= 13 ||
                   ExpansionVersion == 2 && MajorVersion >= 5 ||
                   ExpansionVersion == 3 && MajorVersion >= 4;
        }

        public static int GetAccountDataCount()
        {
            if (ExpansionVersion == 1 && MajorVersion >= 14)
            {
                if (AddedInVersion(1, 14, 1))
                    return 13;
                else
                    return 10;
            }
            else if (ExpansionVersion == 2 && MajorVersion >= 5 ||
                     ExpansionVersion == 3 && MajorVersion >= 4)
            {
                if (AddedInVersion(2, 5, 3))
                    return 13;
            }
            else if (!IsClassicVersionBuild())
            {
                if (AddedInVersion(9, 2, 0))
                    return 13;
                else if (AddedInVersion(9, 1, 5))
                    return 12;
            }

            return 8;
        }

        public static int GetPowerCountForClientVersion()
        {
            if (IsClassicVersionBuild())
            {
                if (AddedInClassicVersion(1, 14, 1, 2, 5, 3))
                    return 7;

                return 6;
            }
            else
            {
                if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    return 5;
                if (RemovedInVersion(ClientVersionBuild.V4_0_6_13596))
                    return 7;
                if (RemovedInVersion(ClientVersionBuild.V6_0_2_19033))
                    return 5;
                if (RemovedInVersion(ClientVersionBuild.V9_1_5_40772))
                    return 6;

                return 7;
            }
        }

        public static uint GetGameObjectStateAnimId()
        {
            if (IsVersion(1, 14, 0) || IsVersion(2, 5, 2))
                return 1556;
            if (IsVersion(1, 14, 1))
                return 1618;
            return 0;
        }

        public static byte AdjustInventorySlot(byte slot)
        {
            byte offset = 0;
            if (slot >= World.Enums.Classic.InventorySlots.BankItemStart && slot < World.Enums.Classic.InventorySlots.BankItemEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Enums.Classic.InventorySlots.BankItemStart - World.Enums.Vanilla.InventorySlots.BankItemStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Enums.Classic.InventorySlots.BankItemStart - World.Enums.TBC.InventorySlots.BankItemStart;
                else
                    offset = World.Enums.Classic.InventorySlots.BankItemStart - World.Enums.WotLK.InventorySlots.BankItemStart;
            }
            else if (slot >= World.Enums.Classic.InventorySlots.BankBagStart && slot < World.Enums.Classic.InventorySlots.BankBagEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Enums.Classic.InventorySlots.BankBagStart - World.Enums.Vanilla.InventorySlots.BankBagStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Enums.Classic.InventorySlots.BankBagStart - World.Enums.TBC.InventorySlots.BankBagStart;
                else
                    offset = World.Enums.Classic.InventorySlots.BankBagStart - World.Enums.WotLK.InventorySlots.BankBagStart;
            }
            else if (slot >= World.Enums.Classic.InventorySlots.BuyBackStart && slot < World.Enums.Classic.InventorySlots.BuyBackEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Enums.Classic.InventorySlots.BuyBackStart - World.Enums.Vanilla.InventorySlots.BuyBackStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Enums.Classic.InventorySlots.BuyBackStart - World.Enums.TBC.InventorySlots.BuyBackStart;
                else
                    offset = World.Enums.Classic.InventorySlots.BuyBackStart - World.Enums.WotLK.InventorySlots.BuyBackStart;
            }
            else if (slot >= World.Enums.Classic.InventorySlots.KeyringStart && slot < World.Enums.Classic.InventorySlots.KeyringEnd)
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    offset = World.Enums.Classic.InventorySlots.KeyringStart - World.Enums.Vanilla.InventorySlots.KeyringStart;
                else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    offset = World.Enums.Classic.InventorySlots.KeyringStart - World.Enums.TBC.InventorySlots.KeyringStart;
                else
                    offset = World.Enums.Classic.InventorySlots.KeyringStart - World.Enums.WotLK.InventorySlots.KeyringStart;
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

                if (oldFlags.HasAnyFlag(AuraFlagsTBC.NotCancelable))
                    newFlags |= AuraFlagsModern.Negative;
                else if (oldFlags.HasAnyFlag(AuraFlagsTBC.Cancelable))
                    newFlags |= (AuraFlagsModern.Positive | AuraFlagsModern.Cancelable);
                else if (slot >= 40)
                    newFlags |= AuraFlagsModern.Negative;

                if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex0))
                    activeFlags |= 1;
                if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex1))
                    activeFlags |= 2;
                if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex2))
                    activeFlags |= 4;
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

        public static uint GetArenaTeamSizeFromIndex(uint index)
        {
            switch (index)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 5;
            }
            return 0;
        }

        public static uint GetArenaTeamIndexFromSize(uint size)
        {
            switch (size)
            {
                case 2:
                    return 0;
                case 3:
                    return 1;
                case 5:
                    return 2;
            }
            return 0;
        }
    }
}
