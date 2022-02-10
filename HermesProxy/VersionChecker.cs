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
    }
}
