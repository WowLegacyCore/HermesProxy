using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Objects
{
    public static class ObjectTypeConverter
    {
        private static readonly Dictionary<ObjectTypeLegacy, ObjectType> ConvDictLegacy = new()
        {
            { ObjectTypeLegacy.Object,                 ObjectType.Object },
            { ObjectTypeLegacy.Item,                   ObjectType.Item },
            { ObjectTypeLegacy.Container,              ObjectType.Container },
            { ObjectTypeLegacy.Unit,                   ObjectType.Unit },
            { ObjectTypeLegacy.Player,                 ObjectType.Player },
            { ObjectTypeLegacy.GameObject,             ObjectType.GameObject },
            { ObjectTypeLegacy.DynamicObject,          ObjectType.DynamicObject },
            { ObjectTypeLegacy.Corpse,                 ObjectType.Corpse },
            { ObjectTypeLegacy.AreaTrigger,            ObjectType.AreaTrigger },
            { ObjectTypeLegacy.SceneObject,            ObjectType.SceneObject },
            { ObjectTypeLegacy.Conversation,           ObjectType.Conversation }
        };

        public static ObjectType Convert(ObjectTypeLegacy type)
        {
            if (!ConvDictLegacy.ContainsKey(type))
                throw new ArgumentOutOfRangeException("0x" + type.ToString("X"));
            return ConvDictLegacy[type];
        }

        public static ObjectTypeLegacy ConvertToLegacy(ObjectType type)
        {
            foreach (var itr in ConvDictLegacy)
            {
                if (itr.Value == type)
                    return itr.Key;
            }
            throw new ArgumentOutOfRangeException("0x" + type.ToString("X"));
        }

        private static readonly Dictionary<ObjectType801, ObjectType> ConvDict801 = new()
        {
            { ObjectType801.Object,                 ObjectType.Object },
            { ObjectType801.Item,                   ObjectType.Item },
            { ObjectType801.Container,              ObjectType.Container },
            { ObjectType801.AzeriteEmpoweredItem,   ObjectType.AzeriteEmpoweredItem },
            { ObjectType801.AzeriteItem,            ObjectType.AzeriteItem },
            { ObjectType801.Unit,                   ObjectType.Unit },
            { ObjectType801.Player,                 ObjectType.Player },
            { ObjectType801.ActivePlayer,           ObjectType.ActivePlayer },
            { ObjectType801.GameObject,             ObjectType.GameObject },
            { ObjectType801.DynamicObject,          ObjectType.DynamicObject },
            { ObjectType801.Corpse,                 ObjectType.Corpse },
            { ObjectType801.AreaTrigger,            ObjectType.AreaTrigger },
            { ObjectType801.SceneObject,            ObjectType.SceneObject },
            { ObjectType801.Conversation,           ObjectType.Conversation }
        };

        public static ObjectType Convert(ObjectType801 type)
        {
            if (!ConvDict801.ContainsKey(type))
                throw new ArgumentOutOfRangeException("0x" + type.ToString("X"));
            return ConvDict801[type];
        }

        public static ObjectType801 ConvertTo801(ObjectType type)
        {
            foreach (var itr in ConvDict801)
            {
                if (itr.Value == type)
                    return itr.Key;
            }
            throw new ArgumentOutOfRangeException("0x" + type.ToString("X"));
        }

        private static readonly Dictionary<ObjectTypeBCC, ObjectType> ConvDictBCC = new()
        {
            { ObjectTypeBCC.Object,                 ObjectType.Object },
            { ObjectTypeBCC.Item,                   ObjectType.Item },
            { ObjectTypeBCC.Container,              ObjectType.Container },
            { ObjectTypeBCC.Unit,                   ObjectType.Unit },
            { ObjectTypeBCC.Player,                 ObjectType.Player },
            { ObjectTypeBCC.ActivePlayer,           ObjectType.ActivePlayer },
            { ObjectTypeBCC.GameObject,             ObjectType.GameObject },
            { ObjectTypeBCC.DynamicObject,          ObjectType.DynamicObject },
            { ObjectTypeBCC.Corpse,                 ObjectType.Corpse },
            { ObjectTypeBCC.AreaTrigger,            ObjectType.AreaTrigger },
            { ObjectTypeBCC.SceneObject,            ObjectType.SceneObject },
            { ObjectTypeBCC.Conversation,           ObjectType.Conversation }
        };

        public static ObjectType Convert(ObjectTypeBCC type)
        {
            if (!ConvDictBCC.ContainsKey(type))
                throw new ArgumentOutOfRangeException("0x" + type.ToString("X"));
            return ConvDictBCC[type];
        }

        public static ObjectTypeBCC ConvertToBCC(ObjectType type)
        {
            foreach (var itr in ConvDictBCC)
            {
                if (itr.Value == type)
                    return itr.Key;
            }
            throw new ArgumentOutOfRangeException("0x" + type.ToString("X"));
        }
    }
}
