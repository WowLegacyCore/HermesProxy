using HermesProxy.Enums;
using HermesProxy.World.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HermesProxy.World.Objects
{
    public static class UpdateFieldExtensions
    {
        private static TypeCode GetTypeCodeOfReturnValue<TK>()
        {
            var type = typeof(TK);
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.Double:
                    return typeCode;
                default:
                {
                    typeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(type));
                    switch (typeCode)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int32:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return typeCode;
                        default:
                            break;
                    }
                    break;
                }
            }
            throw new ArgumentException($"Type must be one of int, uint, float or its nullable counterpart but was {type.Name}");
        }

        /// <summary>
        /// Grabs a value from a dictionary of UpdateFields
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (int, uint or float and their nullable counterparts)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="updateField">The update field we want</param>
        /// <returns></returns>
        public static TK GetValue<T, TK>(this Dictionary<int, UpdateField> dict, T updateField) // where T: System.Enum // C# 7.3
        {
            UpdateField uf;
            if (dict != null && dict.TryGetValue(LegacyVersion.GetUpdateField(updateField), out uf))
            {
                var type = GetTypeCodeOfReturnValue<TK>();
                switch (type)
                {
                    case TypeCode.UInt32:
                        return (TK)(object)uf.UInt32Value;
                    case TypeCode.Int32:
                        return (TK)(object)(int)uf.UInt32Value;
                    case TypeCode.Single:
                        return (TK)(object)uf.FloatValue;
                    case TypeCode.Double:
                        return (TK)(object)(double)uf.FloatValue;
                    default:
                        break;
                }
            }

            return default(TK);
        }

        /// <summary>
        /// Grabs a value list from a dictionary of dynamic UpdateFields
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (UnitDynamicField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (int, uint or float and their nullable counterparts)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="updateField">The update field we want</param>
        /// <returns></returns>
        public static IEnumerable<TK> GetValue<T, TK>(this Dictionary<int, List<UpdateField>> dict, T updateField) // where T: System.Enum // C# 7.3
        {
            List<UpdateField> ufs;
            if (dict != null && dict.TryGetValue(LegacyVersion.GetUpdateField(updateField), out ufs))
            {
                var type = GetTypeCodeOfReturnValue<TK>();
                switch (type)
                {
                    case TypeCode.UInt32:
                        return ufs.Select(uf => (TK)(object)uf.UInt32Value);
                    case TypeCode.Int32:
                        return ufs.Select(uf => (TK)(object)(int)uf.UInt32Value);
                    case TypeCode.Single:
                        return ufs.Select(uf => (TK)(object)uf.FloatValue);
                    case TypeCode.Double:
                        return ufs.Select(uf => (TK)(object)(double)uf.FloatValue);
                    default:
                        break;
                }
            }

            return Enumerable.Empty<TK>();
        }

        /// <summary>
        /// Grabs N (consecutive) values from a dictionary of UpdateFields
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (int, uint or float and their nullable counterparts)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="firstUpdateField">The first update field of the sequence</param>
        /// <param name="count">Number of values to retrieve</param>
        /// <returns></returns>
        public static TK[] GetArray<T, TK>(this Dictionary<int, UpdateField> dict, T firstUpdateField, int count) where T: System.Enum // C# 7.3
        {
            return GetArray<TK>(dict, LegacyVersion.GetUpdateField(firstUpdateField), count);
        }
        public static TK[] GetArray<TK>(this Dictionary<int, UpdateField> dict, int firstUpdateField, int count)
        {
            var result = new TK[count];
            var type = GetTypeCodeOfReturnValue<TK>();
            for (var i = 0; i < count; i++)
            {
                UpdateField uf;
                if (dict != null && dict.TryGetValue(firstUpdateField + i, out uf))
                {
                    switch (type)
                    {
                        case TypeCode.UInt32:
                            result[i] = (TK)(object)uf.UInt32Value;
                            break;
                        case TypeCode.Int32:
                            result[i] = (TK)(object)(int)uf.UInt32Value;
                            break;
                        case TypeCode.Single:
                            result[i] = (TK)(object)uf.FloatValue;
                            break;
                        case TypeCode.Double:
                            result[i] = (TK)(object)(double)uf.FloatValue;
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }
        public static WowGuid GetGuidValue(this Dictionary<int, UpdateField> UpdateFields, int field)
        {
            if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
            {
                var parts = UpdateFields.GetArray<uint>(field, 2);
                return new WowGuid64(MathFunctions.MakePair64(parts[0], parts[1]));
            }
            else
            {
                var parts = UpdateFields.GetArray<uint>(field, 4);
                return new WowGuid128(MathFunctions.MakePair64(parts[0], parts[1]), MathFunctions.MakePair64(parts[2], parts[3]));
            }
        }

        /// <summary>
        /// Grabs a value from a dictionary of UpdateFields and converts it to an enum val
        /// </summary>
        /// <typeparam name="T">The type of UpdateField (ObjectField, UnitField, ...)</typeparam>
        /// <typeparam name="TK">The type of the value (a NULLABLE enum)</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="updateField">The update field we want</param>
        /// <returns></returns>
        public static TK GetEnum<T, TK>(this Dictionary<int, UpdateField> dict, T updateField) // where T: System.Enum // C# 7.3
        {
            // typeof (TK) is a nullable type (ObjectField?)
            // typeof (TK).GetGenericArguments()[0] is the non nullable equivalent (ObjectField)
            // we need to convert our int from UpdateFields to the enum type

            try
            {
                UpdateField uf;
                if (dict != null && dict.TryGetValue(LegacyVersion.GetUpdateField(updateField), out uf))
                    return (TK)Enum.Parse(typeof(TK).GetGenericArguments()[0], uf.UInt32Value.ToString(CultureInfo.InvariantCulture));
            }
            catch (OverflowException) // Data wrongly parsed can result in very wtfy values
            {
                return default(TK);
            }

            return default(TK);
        }
    }
}
