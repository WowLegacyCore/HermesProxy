﻿//#define DEBUG_UPDATES

using Framework.GameMath;
using Framework.Logging;
using Framework.Util;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_DESTROY_OBJECT)]
        void HandleDestroyObject(WorldPacket packet)
        {
            WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
            GetSession().GameState.ObjectCacheMutex.WaitOne();
            GetSession().GameState.ObjectCacheLegacy.Remove(guid);
            GetSession().GameState.ObjectCacheModern.Remove(guid);
            GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
            GetSession().GameState.LastAuraCasterOnTarget.Remove(guid);

            UpdateObject updateObject = new UpdateObject(GetSession().GameState);
            updateObject.DestroyedGuids.Add(guid);
            SendPacketToClient(updateObject);
        }
        
        [PacketHandler(Opcode.SMSG_COMPRESSED_UPDATE_OBJECT)]
        void HandleCompressedUpdateObject(WorldPacket packet)
        {
            using (var packet2 = packet.Inflate(packet.ReadInt32()))
            {
                HandleUpdateObject(packet2);
            }
        }

        [PacketHandler(Opcode.SMSG_UPDATE_OBJECT)]
        void HandleUpdateObject(WorldPacket packet)
        {
            var count = packet.ReadUInt32();
            PrintString($"Updates Count = {count}");

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadBool(); // Has Transport

            HashSet<uint> missingItemTemplates = new HashSet<uint>();
            List<AuraUpdate> auraUpdates = new List<AuraUpdate>();
            UpdateObject updateObject = new UpdateObject(GetSession().GameState);

            for (var i = 0; i < count; i++)
            {
                UpdateTypeLegacy type = (UpdateTypeLegacy)packet.ReadUInt8();
                PrintString($"Update Type = {type}", i);

                switch (type)
                {
                    case UpdateTypeLegacy.Values:
                    {
                        var guid = packet.ReadPackedGuid().To128(GetSession().GameState);
                        PrintString($"Guid = {guid.ToString()}", i);
                        
                        ObjectUpdate updateData = new ObjectUpdate(guid, UpdateTypeModern.Values, GetSession());
                        AuraUpdate auraUpdate = new AuraUpdate(guid, false);
                        PowerUpdate powerUpdate = new PowerUpdate(guid);
                        ReadValuesUpdateBlock(packet, guid, updateData, auraUpdate, powerUpdate, i);

                        if (powerUpdate.Powers.Count != 0)
                            SendPacketToClient(powerUpdate);
                        updateObject.ObjectUpdates.Add(updateData);
                        if (auraUpdate.Auras.Count != 0)
                            auraUpdates.Add(auraUpdate);
                        break;
                    }
                    case UpdateTypeLegacy.Movement:
                    {
                        var guid = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901) ? packet.ReadPackedGuid() : packet.ReadGuid();
                        PrintString($"Guid = {guid.ToString()}", i);
                        ReadMovementUpdateBlock(packet, guid, null, i);
                        break;
                    }
                    case UpdateTypeLegacy.CreateObject1:
                    {
                        var oldGuid = packet.ReadPackedGuid();

                        // workaround for lack of dynamic guids on private servers
                        if (oldGuid.GetHighType() == HighGuidType.Creature || oldGuid.GetHighType() == HighGuidType.GameObject)
                        {
                            if (!GetSession().GameState.ObjectSpawnCount.ContainsKey(oldGuid))
                                GetSession().GameState.ObjectSpawnCount.Add(oldGuid, 0);
                            else if (oldGuid.GetHighType() == HighGuidType.GameObject && GetSession().GameState.DespawnedGameObjects.Contains(oldGuid))
                                    GetSession().GameState.IncrementObjectSpawnCounter(oldGuid);
                        }

                        var guid = oldGuid.To128(GetSession().GameState);
                        PrintString($"Guid = {guid.ToString()}", i);

                        // workaround for mind vision and mind control
                        // mangos sends a create object for own player upon interrupt
                        // instead of a values update like on official servers
                        // modern client seems to ignore create packets for existing objects
                        if (guid == GetSession().GameState.CurrentPlayerGuid &&
                            GetSession().GameState.IsInFarSight)
                        {
                            UpdateObject updateObject2 = new UpdateObject(GetSession().GameState);
                            ObjectUpdate updateData2 = new ObjectUpdate(guid, UpdateTypeModern.Values, GetSession());
                            updateData2.ActivePlayerData.FarsightObject = WowGuid128.Empty;
                            updateObject2.ObjectUpdates.Add(updateData2);
                            SendPacketToClient(updateObject2);
                        }

                        ObjectUpdate updateData = new ObjectUpdate(guid, UpdateTypeModern.CreateObject1, GetSession());
                        AuraUpdate auraUpdate = new AuraUpdate(guid, true);
                        ReadCreateObjectBlock(packet, guid, updateData, auraUpdate, i);

                        if (updateData.Guid == GetSession().GameState.CurrentPlayerGuid)
                        {
                            if (GetSession().GameState.CurrentPlayerStorage.CompletedQuests.NeedsToBeForceSent)
                            {
                                GetSession().GameState.CurrentPlayerStorage.CompletedQuests.WriteAllCompletedIntoArray(updateData.ActivePlayerData.QuestCompleted);
                                GetSession().GameState.CurrentPlayerStorage.CompletedQuests.NeedsToBeForceSent = false;
                            }
                        }

                        if (guid.IsItem() && updateData.ObjectData.EntryID != null &&
                           !GameData.ItemTemplates.ContainsKey((uint)updateData.ObjectData.EntryID))
                        {
                            missingItemTemplates.Add((uint)updateData.ObjectData.EntryID);
                        }

                        if (updateData.CreateData.MoveInfo != null || !guid.IsWorldObject() )
                        {
                            updateObject.ObjectUpdates.Add(updateData);
                            if (auraUpdate.Auras.Count != 0)
                                auraUpdates.Add(auraUpdate);
                        }
                        else
                            Log.Print(LogType.Error, $"Broken create1 without position for {guid}");
                        
                        break;
                    }
                    case UpdateTypeLegacy.CreateObject2:
                    {
                        var oldGuid = packet.ReadPackedGuid();

                        // workaround for lack of dynamic guids on private servers
                        if (oldGuid.GetHighType() == HighGuidType.Creature || oldGuid.GetHighType() == HighGuidType.GameObject)
                            GetSession().GameState.IncrementObjectSpawnCounter(oldGuid);

                        var guid = oldGuid.To128(GetSession().GameState);
                        PrintString($"Guid = {guid.ToString()}", i);

                        ObjectUpdate updateData = new ObjectUpdate(guid, UpdateTypeModern.CreateObject2, GetSession());
                        AuraUpdate auraUpdate = new AuraUpdate(guid, true);
                        ReadCreateObjectBlock(packet, guid, updateData, auraUpdate, i);

                        if (guid.IsItem() && updateData.ObjectData.EntryID != null &&
                           !GameData.ItemTemplates.ContainsKey((uint)updateData.ObjectData.EntryID))
                        {
                            missingItemTemplates.Add((uint)updateData.ObjectData.EntryID);
                        }

                        if (updateData.CreateData.MoveInfo != null || !guid.IsWorldObject())
                        {
                            updateObject.ObjectUpdates.Add(updateData);
                            if (auraUpdate.Auras.Count != 0)
                                auraUpdates.Add(auraUpdate);
                        }
                        else
                            Log.Print(LogType.Error, $"Broken create2 without position for {guid}");

                        break;
                    }
                    case UpdateTypeLegacy.NearObjects:
                    {
                        ReadNearObjectsBlock(packet, i);
                        break;
                    }
                    case UpdateTypeLegacy.FarObjects:
                    {
                        ReadFarObjectsBlock(packet, updateObject, i);
                        break;
                    }
                }
            }

            if (updateObject.ObjectUpdates.Count == 0 &&
                GetSession().GameState.IsWaitingForNewWorld)
                return;

            foreach (uint itemId in missingItemTemplates)
            {
                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                packet2.WriteUInt32(itemId);
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    packet2.WriteGuid(WowGuid64.Empty);
                SendPacketToServer(packet2);
            }

            int activePlayerUpdateIndex = -1;
            for (int i = 0; i < updateObject.ObjectUpdates.Count; i++)
            {
                if (updateObject.ObjectUpdates[i].CreateData != null &&
                    updateObject.ObjectUpdates[i].CreateData.ThisIsYou)
                {
                    activePlayerUpdateIndex = i;
                    break;
                }
            }

            // resend spell mods on player create
            if (activePlayerUpdateIndex >= 0)
            {
                if (GetSession().GameState.FlatSpellMods.Count > 0)
                {
                    SetSpellModifier spell = new SetSpellModifier(Opcode.SMSG_SET_FLAT_SPELL_MODIFIER);
                    foreach (var modItr in GetSession().GameState.FlatSpellMods)
                    {
                        SpellModifierInfo mod = new SpellModifierInfo();
                        mod.ModIndex = modItr.Key;
                        foreach (var dataItr in modItr.Value)
                        {
                            SpellModifierData data = new SpellModifierData();
                            data.ClassIndex = dataItr.Key;
                            data.ModifierValue = dataItr.Value;
                            mod.ModifierData.Add(data);
                        }
                        spell.Modifiers.Add(mod);
                    }
                    SendPacketToClient(spell);
                }
                if (GetSession().GameState.PctSpellMods.Count > 0)
                {
                    SetSpellModifier spell = new SetSpellModifier(Opcode.SMSG_SET_PCT_SPELL_MODIFIER);
                    foreach (var modItr in GetSession().GameState.PctSpellMods)
                    {
                        SpellModifierInfo mod = new SpellModifierInfo();
                        mod.ModIndex = modItr.Key;
                        foreach (var dataItr in modItr.Value)
                        {
                            SpellModifierData data = new SpellModifierData();
                            data.ClassIndex = dataItr.Key;
                            data.ModifierValue = dataItr.Value;
                            mod.ModifierData.Add(data);
                        }
                        spell.Modifiers.Add(mod);
                    }
                    SendPacketToClient(spell);
                }
            }

            if (activePlayerUpdateIndex > 0)
            {
                ObjectUpdate tmp = updateObject.ObjectUpdates[0];
                updateObject.ObjectUpdates[0] = updateObject.ObjectUpdates[activePlayerUpdateIndex];
                updateObject.ObjectUpdates[activePlayerUpdateIndex] = tmp;
            }

            // Fix flag carrier positions on map bugging out when player goes out of range
            if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch)
            {
                bool resetBgPlayerPositions = false;
                foreach (var guid in updateObject.OutOfRangeGuids)
                {
                    if (guid.IsPlayer() && GetSession().GameState.FlagCarrierGuids.Contains(guid))
                    {
                        resetBgPlayerPositions = true;
                        break;
                    }
                }
                if (resetBgPlayerPositions)
                {
                    BattlegroundPlayerPositions bglist = new BattlegroundPlayerPositions();
                    SendPacketToClient(bglist);
                }
            }

            if (updateObject.ObjectUpdates.Count != 0 ||
                updateObject.DestroyedGuids.Count != 0 ||
                updateObject.OutOfRangeGuids.Count != 0)
                SendPacketToClient(updateObject);

            foreach (var auraUpdate in auraUpdates)
                SendPacketToClient(auraUpdate);
        }

        public void ReadNearObjectsBlock(WorldPacket packet, object index)
        {
            var objCount = packet.ReadInt32();
            PrintString($"NearObjectsCount = {objCount}", index);
            for (var j = 0; j < objCount; j++)
            {
                var guid = packet.ReadPackedGuid();
                PrintString($"Guid = {objCount}", index, j);
            }
        }

        public void ReadFarObjectsBlock(WorldPacket packet, UpdateObject updateObject, object index)
        {
            var objCount = packet.ReadInt32();
            PrintString($"FarObjectsCount = {objCount}", index);
            for (var j = 0; j < objCount; j++)
            {
                var guid = packet.ReadPackedGuid().To128(GetSession().GameState);

                // workaround for freeze issue after hunter  'Eyes of the Beast' 
                if(guid == GetSession().GameState.CurrentPlayerGuid && GetSession().GameState.IsInFarSight) {
                    UpdateObject updateObject2 = new UpdateObject(GetSession().GameState);
                    ObjectUpdate updateData2 = new ObjectUpdate(guid, UpdateTypeModern.Values, GetSession());
                    updateObject2.ObjectUpdates.Add(updateData2);
                    SendPacketToClient(updateObject2);
                } else {
                    PrintString($"Guid = {objCount}", index, j);
                    GetSession().GameState.ObjectCacheMutex.WaitOne();
                    GetSession().GameState.ObjectCacheLegacy.Remove(guid);
                    GetSession().GameState.ObjectCacheModern.Remove(guid);
                    GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
                    GetSession().GameState.LastAuraCasterOnTarget.Remove(guid);
                    updateObject.OutOfRangeGuids.Add(guid);
                }
                
            }
        }

        private void ReadCreateObjectBlock(WorldPacket packet, WowGuid128 guid, ObjectUpdate updateData, AuraUpdate auraUpdate, object index)
        {
            updateData.CreateData.ObjectType = ObjectTypeConverter.Convert((ObjectTypeLegacy)packet.ReadUInt8());
            GetSession().GameState.StoreOriginalObjectType(guid, updateData.CreateData.ObjectType);
            ReadMovementUpdateBlock(packet, guid, updateData, index);
            ReadValuesUpdateBlockOnCreate(packet, guid, updateData.CreateData.ObjectType, updateData, auraUpdate, index);
        }

        public void ReadValuesUpdateBlockOnCreate(WorldPacket packet, WowGuid128 guid, ObjectType type, ObjectUpdate updateData, AuraUpdate auraUpdate, object index)
        {
            BitArray updateMaskArray = null;
            var updates = ReadValuesUpdateBlock(packet, ref type, index, true, null, out updateMaskArray, out var actuallyChangedValuesMaskArray);
            StoreObjectUpdate(guid, type, updateMaskArray, updates, auraUpdate, null, true, updateData, actuallyChangedValuesMaskArray);
            GetSession().GameState.ObjectCacheMutex.WaitOne();
            if (!GetSession().GameState.ObjectCacheLegacy.ContainsKey(guid))
                GetSession().GameState.ObjectCacheLegacy.Add(guid, updates);
            else
                GetSession().GameState.ObjectCacheLegacy[guid] = updates;
            GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
        }

        public void ReadValuesUpdateBlock(WorldPacket packet, WowGuid128 guid, ObjectUpdate updateData, AuraUpdate auraUpdate, PowerUpdate powerUpdate, int index)
        {
            BitArray updateMaskArray = null;
            ObjectType type = GetSession().GameState.GetOriginalObjectType(guid);
            var updates = ReadValuesUpdateBlock(packet, ref type, index, false, GetSession().GameState.GetCachedObjectFieldsLegacy(guid), out updateMaskArray, out var actuallyChangedValuesMaskArray);
            StoreObjectUpdate(guid, type, updateMaskArray, updates, auraUpdate, powerUpdate, false, updateData, actuallyChangedValuesMaskArray);
        }

        private string GetIndexString(params object[] values)
        {
            var list = values.Flatten();

            return list.Where(value => value != null)
                .Aggregate(string.Empty, (current, value) =>
                {
                    var s = value is string ? "()" : "[]";
                    return current + (s[0] + value.ToString() + s[1] + ' ');
                });
        }

        private void PrintString(string txt, params object[] indexes)
        {
#if DEBUG_UPDATES
            Console.WriteLine("{0}{1}", GetIndexString(indexes), txt);
#endif
        }

        private T PrintValue<T>(string name, T obj, params object[] indexes)
        {
#if DEBUG_UPDATES
            Console.WriteLine("{0}{1}: {2}", GetIndexString(indexes), name, obj);
#endif
            return obj;
        }

        private Dictionary<int, UpdateField> ReadValuesUpdateBlock(WorldPacket packet, ref ObjectType type, object index, bool isCreating, Dictionary<int, UpdateField>? oldValues, out BitArray outUpdateMaskArray, out BitArray outActuallyChangedValuesMaskArray)
        {
            bool missingCreateObject = !isCreating && oldValues == null;
            var maskSize = packet.ReadUInt8();

            var updateMask = new int[maskSize];
            for (var i = 0; i < maskSize; i++)
                updateMask[i] = packet.ReadInt32();

            var mask = new BitArray(updateMask);
            outUpdateMaskArray = mask;
            outActuallyChangedValuesMaskArray = new BitArray(new int[maskSize]);
            var dict = oldValues ?? new Dictionary<int, UpdateField>();

            if (missingCreateObject)
            {
                switch (type)
                {
                    case ObjectType.Item:
                    {
                        if (mask.Count >= LegacyVersion.GetUpdateField(ItemField.ITEM_END))
                        {
                            // Container MaskSize = 8 (6.1.0 - 8.0.1) 5 (2.4.3 - 6.0.3)
                            if (maskSize == Convert.ToInt32((LegacyVersion.GetUpdateField(ContainerField.CONTAINER_END) + 32) / 32))
                                type = ObjectType.Container;
                        }
                        break;
                    }
                    case ObjectType.Player:
                    {
                        if (mask.Count >= LegacyVersion.GetUpdateField(PlayerField.PLAYER_END))
                        {
                            // ActivePlayer MaskSize = 184 (8.0.1)
                            if (maskSize == Convert.ToInt32((LegacyVersion.GetUpdateField(ActivePlayerField.ACTIVE_PLAYER_END) + 32) / 32))
                                type = ObjectType.ActivePlayer;
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case ObjectType.Item:
                    {
                        int ITEM_END = LegacyVersion.GetUpdateField(ItemField.ITEM_END);
                        if (mask.Length < ITEM_END)
                            mask.Length = ITEM_END;
                        break;
                    }
                    case ObjectType.Container:
                    {
                        int CONTAINER_END = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_END);
                        if (mask.Length < CONTAINER_END)
                            mask.Length = CONTAINER_END;
                        break;
                    }
                    case ObjectType.Unit:
                    {
                        int UNIT_END = LegacyVersion.GetUpdateField(UnitField.UNIT_END);
                        if (mask.Length < UNIT_END)
                            mask.Length = UNIT_END;
                        break;
                    }
                    case ObjectType.Player:
                    {
                        int PLAYER_END = LegacyVersion.GetUpdateField(PlayerField.PLAYER_END);
                        if (mask.Length < PLAYER_END)
                            mask.Length = PLAYER_END;
                        break;
                    }
                    case ObjectType.GameObject:
                    {
                        int GAMEOBJECT_END = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_END);
                        if (mask.Length < GAMEOBJECT_END)
                            mask.Length = GAMEOBJECT_END;
                        break;
                    }
                    case ObjectType.DynamicObject:
                    {
                        int DYNAMICOBJECT_END = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_END);
                        if (mask.Length < DYNAMICOBJECT_END)
                            mask.Length = DYNAMICOBJECT_END;
                        break;
                    }
                    case ObjectType.Corpse:
                    {
                        int CORPSE_END = LegacyVersion.GetUpdateField(CorpseField.CORPSE_END);
                        if (mask.Length < CORPSE_END)
                            mask.Length = CORPSE_END;
                        break;
                    }
                }
            }

            int objectEnd = LegacyVersion.GetUpdateField(ObjectField.OBJECT_END);
            for (var i = 0; i < mask.Count; ++i)
            {
                if (!mask[i])
                    continue;

                UpdateField blockVal = packet.ReadUpdateField();

                string key = "Block Value " + i;
                string value = blockVal.UInt32Value + "/" + blockVal.FloatValue;
                UpdateFieldInfo fieldInfo = null;

                if (i < objectEnd)
                {
                    fieldInfo = LegacyVersion.GetUpdateFieldInfo<ObjectField>(i);
                }
                else
                {
                    switch (type)
                    {
                        case ObjectType.Container:
                        {
                            if (i < LegacyVersion.GetUpdateField(ItemField.ITEM_END))
                                goto case ObjectType.Item;

                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<ContainerField>(i);
                            break;
                        }
                        case ObjectType.Item:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<ItemField>(i);
                            break;
                        }
                        case ObjectType.AzeriteEmpoweredItem:
                        {
                            if (i < LegacyVersion.GetUpdateField(ItemField.ITEM_END))
                                goto case ObjectType.Item;

                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<AzeriteEmpoweredItemField>(i);
                            break;
                        }
                        case ObjectType.AzeriteItem:
                        {
                            if (i < LegacyVersion.GetUpdateField(ItemField.ITEM_END))
                                goto case ObjectType.Item;

                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<AzeriteItemField>(i);
                            break;
                        }
                        case ObjectType.Player:
                        {
                            if (i < LegacyVersion.GetUpdateField(UnitField.UNIT_END) || i < LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_END))
                                goto case ObjectType.Unit;

                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<PlayerField>(i);
                            break;
                        }
                        case ObjectType.ActivePlayer:
                        {
                            if (i < LegacyVersion.GetUpdateField(PlayerField.PLAYER_END))
                                goto case ObjectType.Player;

                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<ActivePlayerField>(i);
                            break;
                        }
                        case ObjectType.Unit:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<UnitField>(i);
                            break;
                        }
                        case ObjectType.GameObject:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<GameObjectField>(i);
                            break;
                        }
                        case ObjectType.DynamicObject:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<DynamicObjectField>(i);
                            break;
                        }
                        case ObjectType.Corpse:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<CorpseField>(i);
                            break;
                        }
                        case ObjectType.AreaTrigger:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<AreaTriggerField>(i);
                            break;
                        }
                        case ObjectType.SceneObject:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<SceneObjectField>(i);
                            break;
                        }
                        case ObjectType.Conversation:
                        {
                            fieldInfo = LegacyVersion.GetUpdateFieldInfo<ConversationField>(i);
                            break;
                        }
                    }
                }
                int start = i;
                int size = 1;
                UpdateFieldType updateFieldType = UpdateFieldType.Default;
                if (fieldInfo != null)
                {
                    key = fieldInfo.Name;
                    size = fieldInfo.Size;
                    start = fieldInfo.Value;
                    updateFieldType = fieldInfo.Format;
                }

                List<UpdateField> fieldData = new List<UpdateField>();
                for (int k = start; k < i; ++k)
                {
                    UpdateField updateField;
                    if (oldValues == null || !oldValues.TryGetValue(k, out updateField))
                        updateField = new UpdateField(0);

                    fieldData.Add(updateField);
                }
                fieldData.Add(blockVal);
                for (int k = i - start + 1; k < size; ++k)
                {
                    int currentPosition = ++i;
                    UpdateField updateField;
                    if (mask[currentPosition])
                        updateField = packet.ReadUpdateField();
                    else if (oldValues == null || !oldValues.TryGetValue(currentPosition, out updateField))
                        updateField = new UpdateField(0);

                    fieldData.Add(updateField);
                }

                switch (updateFieldType)
                {
                    case UpdateFieldType.Guid:
                    {
                        var guidSize = LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033) ? 4 : 2;
                        var guidCount = size / guidSize;
                        for (var guidI = 0; guidI < guidCount; ++guidI)
                        {
                            bool hasGuidValue = false;
                            for (var guidPart = 0; guidPart < guidSize; ++guidPart)
                                if (mask[start + guidI * guidSize + guidPart])
                                    hasGuidValue = true;

                            if (!hasGuidValue)
                                continue;

                            if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
                            {
                                ulong guid = fieldData[guidI * guidSize + 1].UInt32Value;
                                guid <<= 32;
                                guid |= fieldData[guidI * guidSize + 0].UInt32Value;
                                if (isCreating && guid == 0)
                                    continue;

                                PrintValue(key + (guidCount > 1 ? " + " + guidI : ""), new WowGuid64(guid), index);
                            }
                            else
                            {
                                ulong low = (fieldData[guidI * guidSize + 1].UInt32Value << 32);
                                low <<= 32;
                                low |= fieldData[guidI * guidSize + 0].UInt32Value;
                                ulong high = fieldData[guidI * guidSize + 3].UInt32Value;
                                high <<= 32;
                                high |= fieldData[guidI * guidSize + 2].UInt32Value;
                                if (isCreating && (high == 0 && low == 0))
                                    continue;

                                PrintValue(key + (guidCount > 1 ? " + " + guidI : ""), new WowGuid128(low, high), index);
                            }
                        }
                        break;
                    }
                    case UpdateFieldType.Quaternion:
                    {
                        var quaternionCount = size / 4;
                        for (var quatI = 0; quatI < quaternionCount; ++quatI)
                        {
                            bool hasQuatValue = false;
                            for (var guidPart = 0; guidPart < 4; ++guidPart)
                                if (mask[start + quatI * 4 + guidPart])
                                    hasQuatValue = true;

                            if (!hasQuatValue)
                                continue;

                            PrintValue(key + (quaternionCount > 1 ? " + " + quatI : ""), new Quaternion(fieldData[quatI * 4 + 0].FloatValue, fieldData[quatI * 4 + 1].FloatValue,
                                fieldData[quatI * 4 + 2].FloatValue, fieldData[quatI * 4 + 3].FloatValue), index);
                        }
                        break;
                    }
                    case UpdateFieldType.PackedQuaternion:
                    {
                        var quaternionCount = size / 2;
                        for (var quatI = 0; quatI < quaternionCount; ++quatI)
                        {
                            bool hasQuatValue = false;
                            for (var guidPart = 0; guidPart < 2; ++guidPart)
                                if (mask[start + quatI * 2 + guidPart])
                                    hasQuatValue = true;

                            if (!hasQuatValue)
                                continue;

                            long quat = fieldData[quatI * 2 + 1].UInt32Value;
                            quat <<= 32;
                            quat |= fieldData[quatI * 2 + 0].UInt32Value;
                            PrintValue(key + (quaternionCount > 1 ? " + " + quatI : ""), new Quaternion(quat), index);
                        }
                        break;
                    }
                    case UpdateFieldType.Uint:
                    {
                        for (int k = 0; k < fieldData.Count; ++k)
                            if (mask[start + k] && (!isCreating || fieldData[k].UInt32Value != 0))
                                PrintValue(k > 0 ? key + " + " + k : key, fieldData[k].UInt32Value, index);
                        break;
                    }
                    case UpdateFieldType.Int:
                    {
                        for (int k = 0; k < fieldData.Count; ++k)
                            if (mask[start + k] && (!isCreating || fieldData[k].UInt32Value != 0))
                                PrintValue(k > 0 ? key + " + " + k : key, fieldData[k].Int32Value, index);
                        break;
                    }
                    case UpdateFieldType.Float:
                    {
                        for (int k = 0; k < fieldData.Count; ++k)
                            if (mask[start + k] && (!isCreating || fieldData[k].UInt32Value != 0))
                                PrintValue(k > 0 ? key + " + " + k : key, fieldData[k].FloatValue, index);
                        break;
                    }
                    case UpdateFieldType.Bytes:
                    {
                        for (int k = 0; k < fieldData.Count; ++k)
                        {
                            if (mask[start + k] && (!isCreating || fieldData[k].UInt32Value != 0))
                            {
                                byte[] intBytes = BitConverter.GetBytes(fieldData[k].UInt32Value);
                                PrintValue(k > 0 ? key + " + " + k : key, intBytes[0] + "/" + intBytes[1] + "/" + intBytes[2] + "/" + intBytes[3], index);
                            }
                        }
                        break;
                    }
                    case UpdateFieldType.Short:
                    {
                        for (int k = 0; k < fieldData.Count; ++k)
                        {
                            if (mask[start + k] && (!isCreating || fieldData[k].UInt32Value != 0))
                                PrintValue(k > 0 ? key + " + " + k : key, ((short)(fieldData[k].UInt32Value & 0xffff)) + "/" + ((short)(fieldData[k].UInt32Value >> 16)), index);
                        }
                        break;
                    }
                    case UpdateFieldType.Custom:
                    default:
                        for (int k = 0; k < fieldData.Count; ++k)
                            if (mask[start + k] && (!isCreating || fieldData[k].UInt32Value != 0))
                                PrintValue(k > 0 ? key + " + " + k : key, fieldData[k].UInt32Value + "/" + fieldData[k].FloatValue, index);
                        break;
                }

                for (int k = 0; k < fieldData.Count; ++k)
                {
                    if (!dict.ContainsKey(start + k))
                    {
                        outActuallyChangedValuesMaskArray.Set(start + k, true);
                        dict.Add(start + k, fieldData[k]);
                    }
                    else
                    {
                        if (dict[start + k] != fieldData[k])
                            outActuallyChangedValuesMaskArray.Set(start + k, true);
                        dict[start + k] = fieldData[k];
                    }
                }
            }

            return dict;
        }

        void ReadMovementUpdateBlock(WorldPacket packet, WowGuid guid, ObjectUpdate updateData, object index)
        {
            MovementInfo moveInfo = null ;

            UpdateFlag flags;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                flags = (UpdateFlag)packet.ReadUInt16();
            else
                flags = (UpdateFlag)packet.ReadUInt8();

            if (flags.HasAnyFlag(UpdateFlag.Self))
            {
                if (updateData != null)
                    updateData.CreateData.ThisIsYou = true;
                GetSession().GameState.CurrentPlayerCreateTime = packet.GetReceivedTime();
            }

            if (flags.HasAnyFlag(UpdateFlag.Living))
            {
                moveInfo = new MovementInfo();
                moveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
                var moveFlags = moveInfo.Flags;

                moveInfo.WalkSpeed = packet.ReadFloat();
                moveInfo.RunSpeed = packet.ReadFloat();
                moveInfo.RunBackSpeed = packet.ReadFloat();
                moveInfo.SwimSpeed = packet.ReadFloat();
                moveInfo.SwimBackSpeed = packet.ReadFloat();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    moveInfo.FlightSpeed = packet.ReadFloat();
                    moveInfo.FlightBackSpeed = packet.ReadFloat();
                }
                else
                { // Convenience in vanilla to use SwimSpeed as FlySpeed
                    moveInfo.FlightSpeed = moveInfo.SwimSpeed;
                    moveInfo.FlightBackSpeed = moveInfo.SwimBackSpeed;
                }
                moveInfo.TurnRate = packet.ReadFloat();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    moveInfo.PitchRate = packet.ReadFloat();

                if (moveFlags.HasAnyFlag(MovementFlagWotLK.SplineEnabled))
                {
                    moveInfo.HasSplineData = true;
                    ServerSideMovement monsterMove = new ServerSideMovement();

                    if (moveInfo.TransportGuid != null)
                        monsterMove.TransportGuid = moveInfo.TransportGuid;
                    monsterMove.TransportSeat = moveInfo.TransportSeat;

                    bool hasTaxiFlightFlags;
                    monsterMove.SplineFlags = SplineFlagModern.None;
                    monsterMove.SplineType = SplineTypeModern.None;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    {
                        SplineFlagWotLK splineFlags = (SplineFlagWotLK)packet.ReadUInt32();
                        monsterMove.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
                        hasTaxiFlightFlags = splineFlags == (SplineFlagWotLK.WalkMode | SplineFlagWotLK.Flying);

                        if (splineFlags.HasAnyFlag(SplineFlagWotLK.FinalTarget))
                        {
                            monsterMove.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
                            monsterMove.SplineType = SplineTypeModern.FacingTarget;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagWotLK.FinalOrientation))
                        {
                            monsterMove.FinalOrientation = packet.ReadFloat();
                            MovementInfo.ClampOrientation(ref monsterMove.FinalOrientation);
                            monsterMove.SplineType = SplineTypeModern.FacingAngle;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagWotLK.FinalPoint))
                        {
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
                            monsterMove.SplineType = SplineTypeModern.FacingSpot;
                        }
                    }
                    else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        SplineFlagTBC splineFlags = (SplineFlagTBC)packet.ReadUInt32();
                        monsterMove.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
                        hasTaxiFlightFlags = splineFlags == (SplineFlagTBC.Runmode | SplineFlagTBC.Flying);

                        if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalTarget))
                        {
                            monsterMove.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
                            monsterMove.SplineType = SplineTypeModern.FacingTarget;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalOrientation))
                        {
                            monsterMove.FinalOrientation = packet.ReadFloat();
                            MovementInfo.ClampOrientation(ref monsterMove.FinalOrientation);
                            monsterMove.SplineType = SplineTypeModern.FacingAngle;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalPoint))
                        {
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
                            monsterMove.SplineType = SplineTypeModern.FacingSpot;
                        }
                    }
                    else
                    {
                        SplineFlagVanilla splineFlags = (SplineFlagVanilla)packet.ReadUInt32();
                        monsterMove.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();
                        hasTaxiFlightFlags = splineFlags == (SplineFlagVanilla.Runmode | SplineFlagVanilla.Flying);

                        if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalTarget))
                        {
                            monsterMove.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
                            monsterMove.SplineType = SplineTypeModern.FacingTarget;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalOrientation))
                        {
                            monsterMove.FinalOrientation = packet.ReadFloat();
                            MovementInfo.ClampOrientation(ref monsterMove.FinalOrientation);
                            monsterMove.SplineType = SplineTypeModern.FacingAngle;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalPoint))
                        {
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
                            monsterMove.SplineType = SplineTypeModern.FacingSpot;
                        }
                    }

                    if (hasTaxiFlightFlags && guid.IsPlayer() &&
                        flags.HasAnyFlag(UpdateFlag.Self))
                    {
                        monsterMove.SplineFlags = SplineFlagModern.Flying |
                                                  SplineFlagModern.CatmullRom |
                                                  SplineFlagModern.CanSwim |
                                                  SplineFlagModern.UncompressedPath |
                                                  SplineFlagModern.Unknown5 |
                                                  SplineFlagModern.Steering |
                                                  SplineFlagModern.Unknown10;
                    }

                    monsterMove.SplineTime = packet.ReadUInt32();
                    monsterMove.SplineTimeFull = packet.ReadUInt32();
                    monsterMove.SplineId = packet.ReadUInt32();
                    
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    {
                        packet.ReadFloat(); // Spline Duration Multiplier
                        packet.ReadFloat(); // Spline Duration Multiplier Next
                        packet.ReadInt32(); // Spline Vertical Acceleration
                        packet.ReadInt32(); // Spline Start Time
                    }

                    var splineCount = packet.ReadUInt32();
                    monsterMove.SplineCount = splineCount;
                    monsterMove.SplinePoints = new List<Vector3>();

                    for (var i = 0; i < splineCount; i++)
                    {
                        Vector3 vec = packet.ReadVector3();
                        monsterMove.SplinePoints.Add(vec);
                    }

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                        monsterMove.SplineMode = packet.ReadUInt8();

                    monsterMove.EndPosition = packet.ReadVector3();

                    if (updateData != null)
                        updateData.CreateData.MoveSpline = monsterMove;
                }
            }
            else // !UpdateFlag.Living
            {
                if (flags.HasAnyFlag(UpdateFlag.GOPosition))
                {
                    moveInfo = new MovementInfo();
                    moveInfo.TransportGuid = packet.ReadPackedGuid().To128(GetSession().GameState);

                    moveInfo.Position = packet.ReadVector3();
                    moveInfo.TransportOffset = packet.ReadVector3();

                    moveInfo.Orientation = packet.ReadFloat();
                    moveInfo.TransportOrientation = moveInfo.Orientation;

                    moveInfo.CorpseOrientation = packet.ReadFloat();
                }
                else if (flags.HasAnyFlag(UpdateFlag.StationaryObject))
                {
                    moveInfo = new MovementInfo();
                    moveInfo.Position = packet.ReadVector3();
                    moveInfo.Orientation = packet.ReadFloat();
                }
            }

            if (flags.HasAnyFlag(UpdateFlag.LowGuid))
                packet.ReadUInt32();

            if (flags.HasAnyFlag(UpdateFlag.HighGuid))
                packet.ReadUInt32();

            if (flags.HasAnyFlag(UpdateFlag.AttackingTarget))
            {
                WowGuid64 attackGuid = packet.ReadPackedGuid();
                if (updateData != null)
                    updateData.CreateData.AutoAttackVictim = attackGuid.To128(GetSession().GameState);
            }

            if (flags.HasAnyFlag(UpdateFlag.Transport))
            {
                uint transportPathTimer = packet.ReadUInt32();
                if (moveInfo != null)
                    moveInfo.TransportPathTimer = transportPathTimer;
            }

            if (flags.HasAnyFlag(UpdateFlag.Vehicle))
            {
                uint vehicleId = packet.ReadUInt32();
                float vehicleOrientation = packet.ReadFloat();
                if (moveInfo != null)
                {
                    moveInfo.VehicleId = vehicleId;
                    moveInfo.VehicleOrientation = vehicleOrientation;
                }
            }

            if (flags.HasAnyFlag(UpdateFlag.GORotation))
            {
                var rotation = packet.ReadPackedQuaternion();
                if (moveInfo != null)
                    moveInfo.Rotation = rotation;
            }

            if (updateData != null && moveInfo != null)
            {
                moveInfo.Flags = (uint)(((MovementFlagWotLK)moveInfo.Flags).CastFlags<MovementFlagModern>());
                moveInfo.ValidateMovementInfo();
                updateData.CreateData.MoveInfo = moveInfo;
            }
        }

        private static WowGuid GetGuidValue<T>(Dictionary<int, UpdateField> UpdateFields, T field) where T : System.Enum
        {
            if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
            {
                var parts = UpdateFields.GetArray<T, uint>(field, 2);
                return new WowGuid64(MathFunctions.MakePair64(parts[0], parts[1]));
            }
            else
            {
                var parts = UpdateFields.GetArray<T, uint>(field, 4);
                return new WowGuid128(MathFunctions.MakePair64(parts[0], parts[1]), MathFunctions.MakePair64(parts[2], parts[3]));
            }
        }
        private static WowGuid GetGuidValue(Dictionary<int, UpdateField> UpdateFields, int field)
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

        public QuestLog ReadQuestLogEntry(int i, BitArray updateMaskArray, Dictionary<int, UpdateField> updates)
        {
            int PLAYER_QUEST_LOG_1_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_QUEST_LOG_1_1);
            int sizePerEntry = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) ? 4 : 3;
            int stateOffset = 1;
            int progressOffset = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) ? 2 : -1;
            int timerOffset = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) ? 3 : 2;
            QuestLog questLog = null;

            int index = PLAYER_QUEST_LOG_1_1 + i * sizePerEntry;
            if ((updateMaskArray != null && updateMaskArray[index]) ||
                (updateMaskArray == null && updates.ContainsKey(index)))
            {
                if (questLog == null)
                    questLog = new QuestLog();

                questLog.QuestID = updates[index].Int32Value;
            }
            if ((updateMaskArray != null && updateMaskArray[index + stateOffset]) ||
                (updateMaskArray == null && updates.ContainsKey(index + stateOffset)))
            {
                if (questLog == null)
                    questLog = new QuestLog();

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_4_0_8089))
                {
                    // first 3 bytes are objective progress, each counter is 6 bits long, total 4 counters
                    uint rawValue = updates[index + stateOffset].UInt32Value;
                    questLog.ObjectiveProgress[0] = (byte)(rawValue & 0x3F);
                    questLog.ObjectiveProgress[1] = (byte)((rawValue & (0x3F << 6)) >> 6);
                    questLog.ObjectiveProgress[2] = (byte)((rawValue & (0x3F << 12)) >> 12);
                    questLog.ObjectiveProgress[3] = (byte)((rawValue & (0x3F << 18)) >> 18);
                    questLog.StateFlags = ((rawValue >> 24) & 0xFF);
                }
                else
                    questLog.StateFlags = updates[index + stateOffset].UInt32Value;
            }
            if (progressOffset != -1 &&
               ((updateMaskArray != null && updateMaskArray[index + progressOffset]) ||
               (updateMaskArray == null && updates.ContainsKey(index + progressOffset))))
            {
                if (questLog == null)
                    questLog = new QuestLog();

                questLog.ObjectiveProgress[0] = (byte)(updates[index + progressOffset].UInt32Value & 0xFF);
                questLog.ObjectiveProgress[1] = (byte)((updates[index + progressOffset].UInt32Value >> 8) & 0xFF);
                questLog.ObjectiveProgress[2] = (byte)((updates[index + progressOffset].UInt32Value >> 16) & 0xFF);
                questLog.ObjectiveProgress[3] = (byte)((updates[index + progressOffset].UInt32Value >> 24) & 0xFF);
            }
            if ((updateMaskArray != null && updateMaskArray[index + timerOffset]) ||
                (updateMaskArray == null && updates.ContainsKey(index + timerOffset)))
            {
                if (questLog == null)
                    questLog = new QuestLog();

                questLog.EndTime = updates[index + timerOffset].UInt32Value;
            }

            return questLog;
        }

        public AuraDataInfo ReadAuraSlot(byte i, WowGuid128 guid, Dictionary<int, UpdateField> updates)
        {
            int UNIT_FIELD_AURA = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURA);
            int UNIT_FIELD_AURAFLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAFLAGS);
            int UNIT_FIELD_AURALEVELS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURALEVELS);
            int UNIT_FIELD_AURAAPPLICATIONS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAAPPLICATIONS);

            if (!updates.ContainsKey(UNIT_FIELD_AURA + i))
                return null;

            uint spellId = updates[UNIT_FIELD_AURA + i].UInt32Value;
            if (spellId == 0)
                return null;

            AuraDataInfo data = new AuraDataInfo();
            data.CastID = WowGuid128.Create(HighGuidType703.Cast, World.Enums.SpellCastSource.Aura, (uint)GetSession().GameState.CurrentMapId, (uint)spellId, guid.GetCounter());
            data.SpellID = spellId;
            data.SpellXSpellVisualID = GameData.GetSpellVisual(spellId);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                int flagsIndex = UNIT_FIELD_AURAFLAGS + i / 4;
                if (updates.ContainsKey(flagsIndex))
                {
                    ushort flags = (ushort)((updates[flagsIndex].UInt32Value >> ((i % 4) * 8)) & 0xFF);
                    ModernVersion.ConvertAuraFlags(flags, i, out data.Flags, out data.ActiveFlags);
                }
            }
            else
            {
                int flagsIndex = UNIT_FIELD_AURAFLAGS + i / 8;
                if (updates.ContainsKey(flagsIndex))
                {
                    ushort flags = (ushort)((updates[flagsIndex].UInt32Value >> ((i % 8) * 4)) & 0xF);
                    ModernVersion.ConvertAuraFlags(flags, i, out data.Flags, out data.ActiveFlags);
                }
            }

            int levelsIndex = UNIT_FIELD_AURALEVELS + i / 4;
            if (updates.ContainsKey(levelsIndex))
                data.CastLevel = (ushort)((updates[levelsIndex].UInt32Value >> ((i % 4) * 8)) & 0xFF);
            else
                data.CastLevel = 0;

            int stacksIndex = UNIT_FIELD_AURAAPPLICATIONS + i / 4;
            if (updates.ContainsKey(stacksIndex))
                data.Applications = (byte)((updates[stacksIndex].UInt32Value >> ((i % 4) * 8)) & 0xFF);
            else
                data.Applications = 0;

            if (GameData.StackableAuras.Contains(spellId))
                data.Applications++;

            if (GameData.SpellEffectPoints.TryGetValue(spellId, out var basePoints))
                data.Points = basePoints;

            return data;
        }

        public byte ReadPvPFlags(Dictionary<int, UpdateField> updates)
        {
            byte flags = 0;

            int UNIT_FIELD_FLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS);
            if (UNIT_FIELD_FLAGS >= 0 && updates.ContainsKey(UNIT_FIELD_FLAGS))
            {
                if (updates[UNIT_FIELD_FLAGS].UInt32Value.HasAnyFlag(UnitFlags.Pvp))
                    flags |= (byte)PvPFlags.PvP;
            }

            int PLAYER_FLAGS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FLAGS);
            if (PLAYER_FLAGS >= 0 && updates.ContainsKey(PLAYER_FLAGS))
            {
                if (updates[PLAYER_FLAGS].UInt32Value.HasAnyFlag(PlayerFlagsLegacy.FreeForAllPvP))
                    flags |= (byte)PvPFlags.FFAPvp;
                if (updates[PLAYER_FLAGS].UInt32Value.HasAnyFlag(PlayerFlagsLegacy.Sanctuary))
                    flags |= (byte)PvPFlags.Sanctuary;
            }

            return flags;
        }

        public void StoreObjectUpdate(WowGuid128 guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, AuraUpdate auraUpdate, PowerUpdate powerUpdate, bool isCreate, ObjectUpdate updateData, BitArray actuallyChangedValuesMaskArray)
        {
            StoreObjectUpdateInternal(guid, objectType, updateMaskArray, updates, auraUpdate, powerUpdate, isCreate, updateData);
            AfterStoreObjectUpdateHook(guid, objectType, updateMaskArray, updates, auraUpdate, powerUpdate, isCreate, updateData, actuallyChangedValuesMaskArray);
        }

        private void AfterStoreObjectUpdateHook(WowGuid128 guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, AuraUpdate auraUpdate, PowerUpdate powerUpdate, bool isCreate, ObjectUpdate updateData, BitArray changedValuesMask)
        {
            if (objectType == ObjectType.Player || objectType == ObjectType.ActivePlayer)
            {
                int UNIT_FIELD_NATIVEDISPLAYID = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NATIVEDISPLAYID);
                int UNIT_FIELD_MOUNTDISPLAYID = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MOUNTDISPLAYID);
                int OBJECT_FIELD_SCALE_X = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_SCALE_X);
                if (UNIT_FIELD_NATIVEDISPLAYID >= 0 && UNIT_FIELD_MOUNTDISPLAYID >= 0 && OBJECT_FIELD_SCALE_X >= 0)
                {
                    if (!changedValuesMask.Get(UNIT_FIELD_NATIVEDISPLAYID) && !changedValuesMask.Get(UNIT_FIELD_MOUNTDISPLAYID) && !changedValuesMask.Get(OBJECT_FIELD_SCALE_X))
                        return; // No need for an update

                    int nativeDisplayId = Session.GameState.GetLegacyFieldValueInt32(guid, UnitField.UNIT_FIELD_DISPLAYID);
                    int mountDisplayId = Session.GameState.GetLegacyFieldValueInt32(guid, UnitField.UNIT_FIELD_MOUNTDISPLAYID);
                    float rawScaleX = Session.GameState.GetLegacyFieldValueFloat(guid, ObjectField.OBJECT_FIELD_SCALE_X);

                    if (rawScaleX == 0.0f)
                        return;

                    var regularNativeDisplaySize = GameData.GetUnitCompleteDisplayScale((uint)nativeDisplayId);
                    var scale = rawScaleX / regularNativeDisplaySize;

                    var ourDisplayInfo = GameData.GetDisplayInfo((uint)nativeDisplayId);
                    var ourModel = GameData.GetModelData(ourDisplayInfo.ModelId);

                    float calculatedBaseHeight;
                    if (mountDisplayId != 0 && LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    { // in vanilla there were no mount collisions
                        var mountDisplayInfo = GameData.GetDisplayInfo((uint)mountDisplayId);
                        var mountModel = GameData.GetModelData(mountDisplayInfo.ModelId);
                        calculatedBaseHeight = mountModel.MountHeight * mountDisplayInfo.DisplayScale + (ourModel.Height * ourModel.ModelScale * ourDisplayInfo.DisplayScale * 0.5f);
                    }
                    else
                    {
                        calculatedBaseHeight = ourDisplayInfo.DisplayScale * ourModel.Height * ourModel.ModelScale;
                    }

                    if (calculatedBaseHeight == 0)
                        calculatedBaseHeight = mountDisplayId != 0 ? PlayerHeight.Mounted : PlayerHeight.Normal;

                    var heightScale = Math.Max(scale, regularNativeDisplaySize); // you HitBox cannot be smaller than displaySize in legacy clients
                    var scaledHeight = heightScale * calculatedBaseHeight;

                    var displayScale = regularNativeDisplaySize * scale;

                    var reason = changedValuesMask.Get(UNIT_FIELD_MOUNTDISPLAYID)
                        ? MoveSetCollisionHeight.UpdateCollisionHeightReason.Mount
                        : MoveSetCollisionHeight.UpdateCollisionHeightReason.Force;

                    MoveSetCollisionHeight height = new()
                    {
                        MoverGUID = guid,
                        Height = scaledHeight,
                        Scale = displayScale,
                        Reason = reason,
                        MountDisplayID = (uint) mountDisplayId,
                    };
                    SendPacketToClient(height, Opcode.SMSG_UPDATE_OBJECT);
                }
            }
        }
        
        private void StoreObjectUpdateInternal(WowGuid128 guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, AuraUpdate auraUpdate, PowerUpdate powerUpdate, bool isCreate, ObjectUpdate updateData)
        {
            // Object Fields
            int OBJECT_FIELD_GUID = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_GUID);
            if (OBJECT_FIELD_GUID >= 0 && updateMaskArray[OBJECT_FIELD_GUID])
            {
                updateData.ObjectData.Guid = GetGuidValue(updates, ObjectField.OBJECT_FIELD_GUID).To128(GetSession().GameState);
            }
            int OBJECT_FIELD_ENTRY = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
            if (OBJECT_FIELD_ENTRY >= 0 && updateMaskArray[OBJECT_FIELD_ENTRY])
            {
                updateData.ObjectData.EntryID = updates[OBJECT_FIELD_ENTRY].Int32Value;
            }
            int OBJECT_FIELD_SCALE_X = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_SCALE_X);
            if (OBJECT_FIELD_SCALE_X >= 0 && updateMaskArray[OBJECT_FIELD_SCALE_X])
            {
                updateData.ObjectData.Scale = updates[OBJECT_FIELD_SCALE_X].FloatValue;
            }

            // Item Fields
            if ((objectType == ObjectType.Item) ||
                (objectType == ObjectType.Container))
            {
                int ITEM_FIELD_OWNER = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_OWNER);
                if (ITEM_FIELD_OWNER >= 0 && updateMaskArray[ITEM_FIELD_OWNER])
                {
                    updateData.ItemData.Owner = GetGuidValue(updates, ItemField.ITEM_FIELD_OWNER).To128(GetSession().GameState);
                }
                int ITEM_FIELD_CONTAINED = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_CONTAINED);
                if (ITEM_FIELD_CONTAINED >= 0 && updateMaskArray[ITEM_FIELD_CONTAINED])
                {
                    updateData.ItemData.ContainedIn = GetGuidValue(updates, ItemField.ITEM_FIELD_CONTAINED).To128(GetSession().GameState);
                }
                int ITEM_FIELD_CREATOR = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_CREATOR);
                if (ITEM_FIELD_CREATOR >= 0 && updateMaskArray[ITEM_FIELD_CREATOR])
                {
                    updateData.ItemData.Creator = GetGuidValue(updates, ItemField.ITEM_FIELD_CREATOR).To128(GetSession().GameState);
                }
                int ITEM_FIELD_GIFTCREATOR = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_GIFTCREATOR);
                if (ITEM_FIELD_GIFTCREATOR >= 0 && updateMaskArray[ITEM_FIELD_GIFTCREATOR])
                {
                    updateData.ItemData.GiftCreator = GetGuidValue(updates, ItemField.ITEM_FIELD_GIFTCREATOR).To128(GetSession().GameState);
                }
                int ITEM_FIELD_STACK_COUNT = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_STACK_COUNT);
                if (ITEM_FIELD_STACK_COUNT >= 0 && updateMaskArray[ITEM_FIELD_STACK_COUNT])
                {
                    updateData.ItemData.StackCount = updates[ITEM_FIELD_STACK_COUNT].UInt32Value;
                }
                int ITEM_FIELD_DURATION = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_DURATION);
                if (ITEM_FIELD_DURATION >= 0 && updateMaskArray[ITEM_FIELD_DURATION])
                {
                    updateData.ItemData.Duration = updates[ITEM_FIELD_DURATION].UInt32Value;
                }
                int ITEM_FIELD_SPELL_CHARGES = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_SPELL_CHARGES);
                if (ITEM_FIELD_SPELL_CHARGES >= 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (updateMaskArray[ITEM_FIELD_SPELL_CHARGES + i])
                        {
                            updateData.ItemData.SpellCharges[i] = updates[ITEM_FIELD_SPELL_CHARGES + i].Int32Value;
                        }
                    }
                }
                int ITEM_FIELD_FLAGS = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_FLAGS);
                if (ITEM_FIELD_FLAGS >= 0 && updateMaskArray[ITEM_FIELD_FLAGS])
                {
                    updateData.ItemData.Flags = updates[ITEM_FIELD_FLAGS].UInt32Value;
                }
                int ITEM_FIELD_ENCHANTMENT = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_ENCHANTMENT);
                if (ITEM_FIELD_ENCHANTMENT >= 0)
                {
                    int sizePerEntry = 3;

                    Func<int, ItemEnchantment> ReadEnchantData = delegate (int slot)
                    {
                        ItemEnchantment enchantment = null;
                        int idIndex = ITEM_FIELD_ENCHANTMENT + slot * sizePerEntry;
                        int durationIndex = idIndex + 1;
                        int chargesIndex = durationIndex + 1;
                        if (updateMaskArray[idIndex])
                        {
                            if (enchantment == null)
                                enchantment = new ItemEnchantment();

                            enchantment.ID = updates[idIndex].Int32Value;
                        }
                        if (updateMaskArray[durationIndex])
                        {
                            if (enchantment == null)
                                enchantment = new ItemEnchantment();

                            enchantment.Duration = updates[durationIndex].UInt32Value;
                        }
                        if (updateMaskArray[chargesIndex])
                        {
                            if (enchantment == null)
                                enchantment = new ItemEnchantment();

                            enchantment.Charges = (ushort)updates[chargesIndex].UInt32Value;
                        }
                        return enchantment;
                    };

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Perm] = ReadEnchantData(Enums.Vanilla.EnchantmentSlot.Perm);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Temp] = ReadEnchantData(Enums.Vanilla.EnchantmentSlot.Temp);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop0] = ReadEnchantData(Enums.Vanilla.EnchantmentSlot.Prop0);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop1] = ReadEnchantData(Enums.Vanilla.EnchantmentSlot.Prop1);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop2] = ReadEnchantData(Enums.Vanilla.EnchantmentSlot.Prop2);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop3] = ReadEnchantData(Enums.Vanilla.EnchantmentSlot.Prop3);
                    }
                    else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    {
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Perm] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Perm);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Temp] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Temp);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Sock1] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Sock1);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Sock2] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Sock2);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Sock3] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Sock3);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Bonus] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Bonus);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop0] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Prop0);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop1] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Prop1);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop2] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Prop2);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop3] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Prop3);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop4] = ReadEnchantData(Enums.TBC.EnchantmentSlot.Prop4);

                    }
                    else
                    {
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Perm] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Perm);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Temp] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Temp);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Sock1] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Sock1);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Sock2] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Sock2);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Sock3] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Sock3);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Bonus] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Bonus);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prismatic] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Prismatic);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop0] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Prop0);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop1] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Prop1);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop2] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Prop2);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop3] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Prop3);
                        updateData.ItemData.Enchantment[Enums.Classic.EnchantmentSlot.Prop4] = ReadEnchantData(Enums.WotLK.EnchantmentSlot.Prop4);
                    }

                    uint?[] gems = new uint?[ItemConst.MaxGemSockets];
                    for (int i = 0; i < ItemConst.MaxGemSockets; i++)
                    {
                        int slot = Enums.Classic.EnchantmentSlot.Sock1 + i;
                        if (updateData.ItemData.Enchantment[slot] != null && updateData.ItemData.Enchantment[slot].ID != null)
                        {
                            uint itemId = GameData.GetGemFromEnchantId((uint)updateData.ItemData.Enchantment[slot].ID);
                            if (itemId != 0 || updateData.ItemData.Enchantment[slot].ID == 0)
                            {
                                gems[i] = itemId;
                                updateData.ItemData.HasGemsUpdate = true;
                            }
                        }
                    }
                    if (updateData.ItemData.HasGemsUpdate)
                        GetSession().GameState.SaveGemsForItem(guid, gems);
                }
                int ITEM_FIELD_PROPERTY_SEED = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_PROPERTY_SEED);
                if (ITEM_FIELD_PROPERTY_SEED >= 0 && updateMaskArray[ITEM_FIELD_PROPERTY_SEED])
                {
                    updateData.ItemData.PropertySeed = updates[ITEM_FIELD_PROPERTY_SEED].UInt32Value;
                }
                int ITEM_FIELD_RANDOM_PROPERTIES_ID = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_RANDOM_PROPERTIES_ID);
                if (ITEM_FIELD_RANDOM_PROPERTIES_ID >= 0 && updateMaskArray[ITEM_FIELD_RANDOM_PROPERTIES_ID])
                {
                    updateData.ItemData.RandomProperty = updates[ITEM_FIELD_RANDOM_PROPERTIES_ID].UInt32Value;
                }
                int ITEM_FIELD_DURABILITY = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_DURABILITY);
                if (ITEM_FIELD_DURABILITY >= 0 && updateMaskArray[ITEM_FIELD_DURABILITY])
                {
                    updateData.ItemData.Durability = updates[ITEM_FIELD_DURABILITY].UInt32Value;
                }
                int ITEM_FIELD_MAXDURABILITY = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_MAXDURABILITY);
                if (ITEM_FIELD_MAXDURABILITY >= 0 && updateMaskArray[ITEM_FIELD_MAXDURABILITY])
                {
                    updateData.ItemData.MaxDurability = updates[ITEM_FIELD_MAXDURABILITY].UInt32Value;
                }
            }

            // Container Fields
            if (objectType == ObjectType.Container)
            {
                int CONTAINER_FIELD_NUM_SLOTS = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_NUM_SLOTS);
                if (CONTAINER_FIELD_NUM_SLOTS >= 0 && updateMaskArray[CONTAINER_FIELD_NUM_SLOTS])
                {
                    updateData.ContainerData.NumSlots = updates[CONTAINER_FIELD_NUM_SLOTS].UInt32Value;
                }
                int CONTAINER_FIELD_SLOT_1 = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
                if (CONTAINER_FIELD_SLOT_1 >= 0)
                {
                    for (int i = 0; i < 36; i++)
                    {
                        if (updateMaskArray[CONTAINER_FIELD_SLOT_1 + i * 2])
                        {
                            updateData.ContainerData.Slots[i] = GetGuidValue(updates, CONTAINER_FIELD_SLOT_1 + i * 2).To128(GetSession().GameState);
                        }
                    }
                }
            }

            // Unit Fields
            if ((objectType == ObjectType.Unit) ||
                (objectType == ObjectType.Player) ||
                (objectType == ObjectType.ActivePlayer))
            {
                int UNIT_FIELD_CHARM = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHARM);
                if (UNIT_FIELD_CHARM >= 0 && updateMaskArray[UNIT_FIELD_CHARM])
                {
                    updateData.UnitData.Charm = GetGuidValue(updates, UnitField.UNIT_FIELD_CHARM).To128(GetSession().GameState);
                }
                int UNIT_FIELD_SUMMON = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMON);
                if (UNIT_FIELD_SUMMON >= 0 && updateMaskArray[UNIT_FIELD_SUMMON])
                {
                    updateData.UnitData.Summon = GetGuidValue(updates, UnitField.UNIT_FIELD_SUMMON).To128(GetSession().GameState);
                }
                int UNIT_FIELD_CHARMEDBY = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHARMEDBY);
                if (UNIT_FIELD_CHARMEDBY >= 0 && updateMaskArray[UNIT_FIELD_CHARMEDBY])
                {
                    updateData.UnitData.CharmedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_CHARMEDBY).To128(GetSession().GameState);
                }
                int UNIT_FIELD_SUMMONEDBY = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMONEDBY);
                if (UNIT_FIELD_SUMMONEDBY >= 0 && updateMaskArray[UNIT_FIELD_SUMMONEDBY])
                {
                    updateData.UnitData.SummonedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_SUMMONEDBY).To128(GetSession().GameState);
                }
                int UNIT_FIELD_CREATEDBY = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CREATEDBY);
                if (UNIT_FIELD_CREATEDBY >= 0 && updateMaskArray[UNIT_FIELD_CREATEDBY])
                {
                    updateData.UnitData.CreatedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_CREATEDBY).To128(GetSession().GameState);
                }
                int UNIT_FIELD_TARGET = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_TARGET);
                if (UNIT_FIELD_TARGET >= 0 && updateMaskArray[UNIT_FIELD_TARGET])
                {
                    updateData.UnitData.Target = GetGuidValue(updates, UnitField.UNIT_FIELD_TARGET).To128(GetSession().GameState);
                }
                int UNIT_FIELD_CHANNEL_OBJECT = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHANNEL_OBJECT);
                if (UNIT_FIELD_CHANNEL_OBJECT >= 0 && updateMaskArray[UNIT_FIELD_CHANNEL_OBJECT])
                {
                    updateData.UnitData.ChannelObject = GetGuidValue(updates, UnitField.UNIT_FIELD_CHANNEL_OBJECT).To128(GetSession().GameState);
                }
                int UNIT_FIELD_HEALTH = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_HEALTH);
                if (UNIT_FIELD_HEALTH >= 0 && updateMaskArray[UNIT_FIELD_HEALTH])
                {
                    updateData.UnitData.Health = updates[UNIT_FIELD_HEALTH].Int32Value;
                }
                int UNIT_FIELD_MAXHEALTH = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXHEALTH);
                if (UNIT_FIELD_MAXHEALTH >= 0 && updateMaskArray[UNIT_FIELD_MAXHEALTH])
                {
                    updateData.UnitData.MaxHealth = updates[UNIT_FIELD_MAXHEALTH].Int32Value;
                }
                int UNIT_FIELD_LEVEL = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_LEVEL);
                if (UNIT_FIELD_LEVEL >= 0 && updateMaskArray[UNIT_FIELD_LEVEL])
                {
                    updateData.UnitData.Level = updates[UNIT_FIELD_LEVEL].Int32Value;
                }
                int UNIT_FIELD_FACTIONTEMPLATE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FACTIONTEMPLATE);
                if (UNIT_FIELD_FACTIONTEMPLATE >= 0 && updateMaskArray[UNIT_FIELD_FACTIONTEMPLATE])
                {
                    updateData.UnitData.FactionTemplate = updates[UNIT_FIELD_FACTIONTEMPLATE].Int32Value;
                }

                int UNIT_FIELD_BYTES_0 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_0);
                if (UNIT_FIELD_BYTES_0 >= 0 && updateMaskArray[UNIT_FIELD_BYTES_0])
                {
                    updateData.UnitData.RaceId = (byte)(updates[UNIT_FIELD_BYTES_0].UInt32Value & 0xFF);
                    updateData.UnitData.ClassId = (byte)((updates[UNIT_FIELD_BYTES_0].UInt32Value >> 8) & 0xFF);
                    updateData.UnitData.SexId = (byte)((updates[UNIT_FIELD_BYTES_0].UInt32Value >> 16) & 0xFF);
                    updateData.UnitData.DisplayPower = (byte)((updates[UNIT_FIELD_BYTES_0].UInt32Value >> 24) & 0xFF);

                    if (guid.GetHighType() == HighGuidType.Pet && updateData.UnitData.DisplayPower == (uint)PowerType.Focus)
                        GetSession().GameState.HunterPetGuids.Add(guid);

                    if (objectType == ObjectType.Unit)
                        GetSession().GameState.StoreCreatureClass(guid.GetEntry(), (Class)updateData.UnitData.ClassId);
                    else
                        updateData.PlayerData.ArenaFaction = (byte)(GameData.IsAllianceRace((Race)updateData.UnitData.RaceId) ? 1 : 0);
                }

                int UNIT_FIELD_POWER1 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER1);
                if (UNIT_FIELD_POWER1 >= 0)
                {
                    for (int i = 0; i < LegacyVersion.GetPowersCount(); i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_POWER1 + i])
                        {
                            if (powerUpdate != null && 
                               (guid == GetSession().GameState.CurrentPlayerGuid || guid == GetSession().GameState.CurrentPetGuid))
                                powerUpdate.Powers.Add(new PowerUpdatePower(updates[UNIT_FIELD_POWER1 + i].Int32Value, (byte)i));

                            sbyte powerSlot;
                            if (GetSession().GameState.HunterPetGuids.Contains(guid))
                                powerSlot = ClassPowerTypes.GetPowerSlotForPet((PowerType)i);
                            else
                            {
                                Class classId;
                                if (updateData.UnitData.ClassId != null)
                                    classId = (Class)updateData.UnitData.ClassId;
                                else
                                    classId = GetSession().GameState.GetUnitClass(guid.To128(GetSession().GameState));
                                powerSlot = ClassPowerTypes.GetPowerSlotForClass(classId, (PowerType)i);
                            }
                                
                            if (powerSlot >= 0)
                                updateData.UnitData.Power[powerSlot] = updates[UNIT_FIELD_POWER1 + i].Int32Value;
                        }
                    }
                }
                int UNIT_FIELD_MAXPOWER1 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXPOWER1);
                if (UNIT_FIELD_MAXPOWER1 >= 0)
                {
                    for (int i = 0; i < LegacyVersion.GetPowersCount(); i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_MAXPOWER1 + i])
                        {
                            Class classId;
                            if (updateData.UnitData.ClassId != null)
                                classId = (Class)updateData.UnitData.ClassId;
                            else
                                classId = GetSession().GameState.GetUnitClass(guid.To128(GetSession().GameState));

                            sbyte powerSlot;
                            if (GetSession().GameState.HunterPetGuids.Contains(guid))
                                powerSlot = ClassPowerTypes.GetPowerSlotForPet((PowerType)i);
                            else
                                powerSlot = ClassPowerTypes.GetPowerSlotForClass(classId, (PowerType)i);

                            if (powerSlot >= 0)
                                updateData.UnitData.MaxPower[powerSlot] = updates[UNIT_FIELD_MAXPOWER1 + i].Int32Value;

                            if (i == (byte)PowerType.Energy)
                            {
                                powerSlot = ClassPowerTypes.GetPowerSlotForClass(classId, PowerType.ComboPoints);
                                if (powerSlot >= 0)
                                    updateData.UnitData.MaxPower[powerSlot] = 5;
                            }
                        }
                    }
                }
                int UNIT_VIRTUAL_ITEM_SLOT_DISPLAY = LegacyVersion.GetUpdateField(UnitField.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY);
                if (UNIT_VIRTUAL_ITEM_SLOT_DISPLAY >= 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (updateMaskArray[UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + i])
                        {
                            uint itemDisplayId = updates[UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + i].UInt32Value;
                            uint itemId = GameData.GetItemIdWithDisplayId(itemDisplayId);
                            if (itemId != 0)
                            {
                                VisibleItem visibleItem = new VisibleItem();
                                visibleItem.ItemID = (int)itemId;
                                updateData.UnitData.VirtualItems[i] = visibleItem;
                            }
                        }
                    }
                }
                int UNIT_VIRTUAL_ITEM_SLOT_ID = LegacyVersion.GetUpdateField(UnitField.UNIT_VIRTUAL_ITEM_SLOT_ID);
                if (UNIT_VIRTUAL_ITEM_SLOT_ID >= 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (updateMaskArray[UNIT_VIRTUAL_ITEM_SLOT_ID + i])
                        {
                            VisibleItem visibleItem = new VisibleItem();
                            visibleItem.ItemID = updates[UNIT_VIRTUAL_ITEM_SLOT_ID + i].Int32Value;
                            updateData.UnitData.VirtualItems[i] = visibleItem;
                        }
                    }
                }
                int UNIT_FIELD_FLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS);
                if (UNIT_FIELD_FLAGS >= 0 && updateMaskArray[UNIT_FIELD_FLAGS])
                {
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        UnitFlagsVanilla vanillaFlags = (UnitFlagsVanilla)updates[UNIT_FIELD_FLAGS].UInt32Value;
                        updateData.UnitData.Flags = (uint)(vanillaFlags.CastFlags<UnitFlags>());

                        if (vanillaFlags.HasAnyFlag(UnitFlagsVanilla.PetRename))
                        {
                            if (updateData.UnitData.PetFlags == null)
                                updateData.UnitData.PetFlags = (byte)PetFlags.CanBeRenamed;
                            else
                                updateData.UnitData.PetFlags |= (byte)PetFlags.CanBeRenamed;
                        }
                        if (vanillaFlags.HasAnyFlag(UnitFlagsVanilla.PetAbandon))
                        {
                            if (updateData.UnitData.PetFlags == null)
                                updateData.UnitData.PetFlags = (byte)PetFlags.CanBeAbandoned;
                            else
                                updateData.UnitData.PetFlags |= (byte)PetFlags.CanBeAbandoned;
                        }
                    }
                    else
                    {
                        updateData.UnitData.Flags = updates[UNIT_FIELD_FLAGS].UInt32Value;
                    }

                    // Here because of this bullshit in cmangos:
                    // https://github.com/cmangos/mangos-tbc/blob/fd093b33071b546545cc5973608304bccc5a041b/src/game/Entities/Object.cpp#L544
                    if (updateData.UnitData.Flags.HasAnyFlag(UnitFlags.ServerControlled) && isCreate &&
                        guid == GetSession().GameState.CurrentPlayerGuid && updateData.CreateData.MoveSpline == null)
                        updateData.UnitData.Flags &= ~(uint)UnitFlags.ServerControlled;

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) &&
                        updateData.UnitData.PvpFlags == null)
                        updateData.UnitData.PvpFlags = ReadPvPFlags(updates);
                }
                int UNIT_FIELD_FLAGS_2 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS_2);
                if (UNIT_FIELD_FLAGS_2 >= 0 && updateMaskArray[UNIT_FIELD_FLAGS_2])
                {
                    updateData.UnitData.Flags2 = updates[UNIT_FIELD_FLAGS_2].UInt32Value;
                }
                int UNIT_FIELD_AURASTATE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURASTATE);
                if (UNIT_FIELD_AURASTATE >= 0 && updateMaskArray[UNIT_FIELD_AURASTATE])
                {
                    updateData.UnitData.AuraState = updates[UNIT_FIELD_AURASTATE].UInt32Value;
                }
                int UNIT_FIELD_BASEATTACKTIME = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASEATTACKTIME);
                if (UNIT_FIELD_BASEATTACKTIME >= 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_BASEATTACKTIME + i])
                            updateData.UnitData.AttackRoundBaseTime[i] = updates[UNIT_FIELD_BASEATTACKTIME + i].UInt32Value;
                    }
                }
                int UNIT_FIELD_RANGEDATTACKTIME = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RANGEDATTACKTIME);
                if (UNIT_FIELD_RANGEDATTACKTIME >= 0 && updateMaskArray[UNIT_FIELD_RANGEDATTACKTIME])
                {
                    updateData.UnitData.RangedAttackRoundBaseTime = updates[UNIT_FIELD_RANGEDATTACKTIME].UInt32Value;
                }
                int UNIT_FIELD_BOUNDINGRADIUS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BOUNDINGRADIUS);
                if (UNIT_FIELD_BOUNDINGRADIUS >= 0 && updateMaskArray[UNIT_FIELD_BOUNDINGRADIUS])
                {
                    updateData.UnitData.BoundingRadius = updates[UNIT_FIELD_BOUNDINGRADIUS].FloatValue;
                }
                int UNIT_FIELD_COMBATREACH = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_COMBATREACH);
                if (UNIT_FIELD_COMBATREACH >= 0 && updateMaskArray[UNIT_FIELD_COMBATREACH])
                {
                    updateData.UnitData.CombatReach = updates[UNIT_FIELD_COMBATREACH].FloatValue;
                }
                int UNIT_FIELD_DISPLAYID = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_DISPLAYID);
                if (UNIT_FIELD_DISPLAYID >= 0 && updateMaskArray[UNIT_FIELD_DISPLAYID])
                {
                    updateData.UnitData.DisplayID = updates[UNIT_FIELD_DISPLAYID].Int32Value;

                    // in post vanilla versions, the client automatically multiplies the scale
                    // the server sends it by the default scale for this display id in the dbc
                    // this is not the case in 1.12, so we have to adjust the unit scale here
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                        updateData.UnitData.DisplayScale = 1.0f / GameData.GetUnitCompleteDisplayScale((uint)updateData.UnitData.DisplayID);
                }
                int UNIT_FIELD_NATIVEDISPLAYID = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NATIVEDISPLAYID);
                if (UNIT_FIELD_NATIVEDISPLAYID >= 0 && updateMaskArray[UNIT_FIELD_NATIVEDISPLAYID])
                {
                    updateData.UnitData.NativeDisplayID = updates[UNIT_FIELD_NATIVEDISPLAYID].Int32Value;
                }
                int UNIT_FIELD_MOUNTDISPLAYID = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MOUNTDISPLAYID);
                if (UNIT_FIELD_MOUNTDISPLAYID >= 0 && updateMaskArray[UNIT_FIELD_MOUNTDISPLAYID])
                {
                    updateData.UnitData.MountDisplayID = updates[UNIT_FIELD_MOUNTDISPLAYID].Int32Value;
                }
                int UNIT_FIELD_MINDAMAGE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MINDAMAGE);
                if (UNIT_FIELD_MINDAMAGE >= 0 && updateMaskArray[UNIT_FIELD_MINDAMAGE])
                {
                    updateData.UnitData.MinDamage = updates[UNIT_FIELD_MINDAMAGE].FloatValue;
                }
                int UNIT_FIELD_MAXDAMAGE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXDAMAGE);
                if (UNIT_FIELD_MAXDAMAGE >= 0 && updateMaskArray[UNIT_FIELD_MAXDAMAGE])
                {
                    updateData.UnitData.MaxDamage = updates[UNIT_FIELD_MAXDAMAGE].FloatValue;
                }
                int UNIT_FIELD_MINOFFHANDDAMAGE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MINOFFHANDDAMAGE);
                if (UNIT_FIELD_MINOFFHANDDAMAGE >= 0 && updateMaskArray[UNIT_FIELD_MINOFFHANDDAMAGE])
                {
                    updateData.UnitData.MinOffHandDamage = updates[UNIT_FIELD_MINOFFHANDDAMAGE].FloatValue;
                }
                int UNIT_FIELD_MAXOFFHANDDAMAGE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXOFFHANDDAMAGE);
                if (UNIT_FIELD_MAXOFFHANDDAMAGE >= 0 && updateMaskArray[UNIT_FIELD_MAXOFFHANDDAMAGE])
                {
                    updateData.UnitData.MaxOffHandDamage = updates[UNIT_FIELD_MAXOFFHANDDAMAGE].FloatValue;
                }
                int UNIT_FIELD_BYTES_1 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_1);
                if (UNIT_FIELD_BYTES_1 >= 0 && updateMaskArray[UNIT_FIELD_BYTES_1])
                {
                    updateData.UnitData.StandState = (byte)(updates[UNIT_FIELD_BYTES_1].UInt32Value & 0xFF);

                    byte petLoyaltyIndex = (byte)((updates[UNIT_FIELD_BYTES_1].UInt32Value >> 8) & 0xFF);
                    if (petLoyaltyIndex != 238)
                        updateData.UnitData.PetLoyaltyIndex = petLoyaltyIndex;

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                    {
                        updateData.UnitData.VisFlags = (byte)((updates[UNIT_FIELD_BYTES_1].UInt32Value >> 16) & 0xFF);
                        updateData.UnitData.AnimTier = (byte)((updates[UNIT_FIELD_BYTES_1].UInt32Value >> 24) & 0xFF);
                    }
                    else
                    {
                        updateData.UnitData.ShapeshiftForm = (byte)((updates[UNIT_FIELD_BYTES_1].UInt32Value >> 16) & 0xFF);
                        updateData.UnitData.VisFlags = (byte)((updates[UNIT_FIELD_BYTES_1].UInt32Value >> 24) & 0xFF);
                    }
                }
                int UNIT_FIELD_PETNUMBER = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PETNUMBER);
                if (UNIT_FIELD_PETNUMBER >= 0 && updateMaskArray[UNIT_FIELD_PETNUMBER])
                {
                    updateData.UnitData.PetNumber = updates[UNIT_FIELD_PETNUMBER].UInt32Value;
                }
                int UNIT_FIELD_PET_NAME_TIMESTAMP = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PET_NAME_TIMESTAMP);
                if (UNIT_FIELD_PET_NAME_TIMESTAMP >= 0 && updateMaskArray[UNIT_FIELD_PET_NAME_TIMESTAMP])
                {
                    updateData.UnitData.PetNameTimestamp = updates[UNIT_FIELD_PET_NAME_TIMESTAMP].UInt32Value;
                }
                int UNIT_FIELD_PETEXPERIENCE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PETEXPERIENCE);
                if (UNIT_FIELD_PETEXPERIENCE >= 0 && updateMaskArray[UNIT_FIELD_PETEXPERIENCE])
                {
                    updateData.UnitData.PetExperience = updates[UNIT_FIELD_PETEXPERIENCE].UInt32Value;
                }
                int UNIT_FIELD_PETNEXTLEVELEXP = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PETNEXTLEVELEXP);
                if (UNIT_FIELD_PETNEXTLEVELEXP >= 0 && updateMaskArray[UNIT_FIELD_PETNEXTLEVELEXP])
                {
                    updateData.UnitData.PetNextLevelExperience = updates[UNIT_FIELD_PETNEXTLEVELEXP].UInt32Value;
                }
                int UNIT_DYNAMIC_FLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_DYNAMIC_FLAGS);
                if (UNIT_DYNAMIC_FLAGS >= 0 && updateMaskArray[UNIT_DYNAMIC_FLAGS])
                {
                    UnitDynamicFlagsLegacy flags = (UnitDynamicFlagsLegacy)(updates[UNIT_DYNAMIC_FLAGS].UInt32Value);
                    if (flags.HasFlag(UnitDynamicFlagsLegacy.Tapped) && flags.HasFlag(UnitDynamicFlagsLegacy.TappedByPlayer))
                        flags &= ~(UnitDynamicFlagsLegacy.Tapped | UnitDynamicFlagsLegacy.TappedByPlayer);
                    updateData.ObjectData.DynamicFlags = (uint)flags.CastFlags<UnitDynamicFlagsModern>();

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        if (updateData.UnitData.Flags2 == null)
                            updateData.UnitData.Flags2 = (uint)UnitFlags2.RegeneratePower;
                        if (flags.HasAnyFlag(UnitDynamicFlagsLegacy.AppearDead))
                            updateData.UnitData.Flags2 |= (uint)UnitFlags2.FeignDeath;
                    }
                }
                int UNIT_CHANNEL_SPELL = LegacyVersion.GetUpdateField(UnitField.UNIT_CHANNEL_SPELL);
                if (UNIT_CHANNEL_SPELL >= 0 && updateMaskArray[UNIT_CHANNEL_SPELL])
                {
                    UnitChannel channel = new UnitChannel();
                    channel.SpellID = updates[UNIT_CHANNEL_SPELL].Int32Value;
                    channel.SpellXSpellVisualID = (int)GameData.GetSpellVisual((uint)channel.SpellID);
                    updateData.UnitData.ChannelData = channel;
                }
                int UNIT_MOD_CAST_SPEED = LegacyVersion.GetUpdateField(UnitField.UNIT_MOD_CAST_SPEED);
                if (UNIT_MOD_CAST_SPEED >= 0 && updateMaskArray[UNIT_MOD_CAST_SPEED])
                {
                    updateData.UnitData.ModCastSpeed = updates[UNIT_MOD_CAST_SPEED].FloatValue;
                }
                int UNIT_CREATED_BY_SPELL = LegacyVersion.GetUpdateField(UnitField.UNIT_CREATED_BY_SPELL);
                if (UNIT_CREATED_BY_SPELL >= 0 && updateMaskArray[UNIT_CREATED_BY_SPELL])
                {
                    updateData.UnitData.CreatedBySpell = updates[UNIT_CREATED_BY_SPELL].Int32Value;

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) &&
                        isCreate && updateData.UnitData.CreatedBy == GetSession().GameState.CurrentPlayerGuid)
                    {
                        int totemSlot = GameData.GetTotemSlotForSpell((uint)updateData.UnitData.CreatedBySpell);
                        if (totemSlot >= 0)
                        {
                            TotemCreated totem = new();
                            totem.Slot = (byte)totemSlot;
                            totem.Totem = guid;
                            totem.Duration = 120000;
                            totem.SpellId = (uint)updateData.UnitData.CreatedBySpell;
                            totem.CannotDismiss = true;
                            SendPacketToClient(totem);
                        }
                    }
                }
                int UNIT_NPC_FLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
                if (UNIT_NPC_FLAGS >= 0 && updateMaskArray[UNIT_NPC_FLAGS])
                {
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        NPCFlagsVanilla vanillaFlags = (NPCFlagsVanilla)updates[UNIT_NPC_FLAGS].UInt32Value;
                        updateData.UnitData.NpcFlags[0] = (uint)(vanillaFlags.CastFlags<NPCFlags>());
                    }
                    else
                    {
                        updateData.UnitData.NpcFlags[0] = updates[UNIT_NPC_FLAGS].UInt32Value;
                    }
                }
                int UNIT_NPC_EMOTESTATE = LegacyVersion.GetUpdateField(UnitField.UNIT_NPC_EMOTESTATE);
                if (UNIT_NPC_EMOTESTATE >= 0 && updateMaskArray[UNIT_NPC_EMOTESTATE])
                {
                    updateData.UnitData.EmoteState = updates[UNIT_NPC_EMOTESTATE].Int32Value;
                }
                int UNIT_TRAINING_POINTS = LegacyVersion.GetUpdateField(UnitField.UNIT_TRAINING_POINTS);
                if (UNIT_TRAINING_POINTS >= 0 && updateMaskArray[UNIT_TRAINING_POINTS])
                {
                    updateData.UnitData.TrainingPointsUsed = (ushort)(updates[UNIT_TRAINING_POINTS].UInt32Value & 0xFFFF);
                    updateData.UnitData.TrainingPointsTotal = (ushort)((updates[UNIT_TRAINING_POINTS].UInt32Value >> 16) & 0xFFFF);
                }
                int UNIT_FIELD_STAT0 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_STAT0);
                if (UNIT_FIELD_STAT0 >= 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_STAT0 + i])
                            updateData.UnitData.Stats[i] = updates[UNIT_FIELD_STAT0 + i].Int32Value;
                    }
                }
                int UNIT_FIELD_POSSTAT0 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POSSTAT0);
                if (UNIT_FIELD_POSSTAT0 >= 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_POSSTAT0 + i])
                            updateData.UnitData.StatPosBuff[i] = updates[UNIT_FIELD_POSSTAT0 + i].Int32Value;
                    }
                }
                int UNIT_FIELD_NEGSTAT0 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NEGSTAT0);
                if (UNIT_FIELD_NEGSTAT0 >= 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_NEGSTAT0 + i])
                            updateData.UnitData.StatNegBuff[i] = updates[UNIT_FIELD_NEGSTAT0 + i].Int32Value;
                    }
                }
                int UNIT_FIELD_RESISTANCES = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCES);
                if (UNIT_FIELD_RESISTANCES >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_RESISTANCES + i])
                            updateData.UnitData.Resistances[i] = updates[UNIT_FIELD_RESISTANCES + i].Int32Value;
                    }
                }
                int UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE);
                if (UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + i])
                            updateData.UnitData.ResistanceBuffModsPositive[i] = updates[UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + i].Int32Value;
                    }
                }
                int UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE);
                if (UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + i])
                            updateData.UnitData.ResistanceBuffModsNegative[i] = updates[UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + i].Int32Value;
                    }
                }
                int UNIT_FIELD_BASE_MANA = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASE_MANA);
                if (UNIT_FIELD_BASE_MANA >= 0 && updateMaskArray[UNIT_FIELD_BASE_MANA])
                {
                    updateData.UnitData.BaseMana = updates[UNIT_FIELD_BASE_MANA].Int32Value;
                }
                int UNIT_FIELD_BASE_HEALTH = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASE_HEALTH);
                if (UNIT_FIELD_BASE_HEALTH >= 0 && updateMaskArray[UNIT_FIELD_BASE_HEALTH])
                {
                    updateData.UnitData.BaseHealth = updates[UNIT_FIELD_BASE_HEALTH].Int32Value;
                }
                int UNIT_FIELD_BYTES_2 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_2);
                if (UNIT_FIELD_BYTES_2 >= 0 && updateMaskArray[UNIT_FIELD_BYTES_2])
                {
                    updateData.UnitData.SheatheState = (byte)(updates[UNIT_FIELD_BYTES_2].UInt32Value & 0xFF);

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                        updateData.UnitData.PvpFlags = (byte)((updates[UNIT_FIELD_BYTES_2].UInt32Value >> 8) & 0xFF);

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        updateData.UnitData.PetFlags = (byte)((updates[UNIT_FIELD_BYTES_2].UInt32Value >> 16) & 0xFF);

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                        updateData.UnitData.ShapeshiftForm = (byte)((updates[UNIT_FIELD_BYTES_2].UInt32Value >> 24) & 0xFF);
                }
                int UNIT_FIELD_ATTACK_POWER = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_ATTACK_POWER);
                if (UNIT_FIELD_ATTACK_POWER >= 0 && updateMaskArray[UNIT_FIELD_ATTACK_POWER])
                {
                    updateData.UnitData.AttackPower = updates[UNIT_FIELD_ATTACK_POWER].Int32Value;
                }
                int UNIT_FIELD_ATTACK_POWER_MODS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_ATTACK_POWER_MODS);
                if (UNIT_FIELD_ATTACK_POWER_MODS >= 0 && updateMaskArray[UNIT_FIELD_ATTACK_POWER_MODS])
                {
                    updateData.UnitData.AttackPowerModNeg = (updates[UNIT_FIELD_ATTACK_POWER_MODS].Int32Value & 0xFFFF);
                    updateData.UnitData.AttackPowerModPos = ((updates[UNIT_FIELD_ATTACK_POWER_MODS].Int32Value >> 16) & 0xFFFF);
                }
                int UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER);
                if (UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER >= 0 && updateMaskArray[UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER])
                {
                    updateData.UnitData.AttackPowerMultiplier = updates[UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER].FloatValue;
                }
                int UNIT_FIELD_MINRANGEDDAMAGE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MINRANGEDDAMAGE);
                if (UNIT_FIELD_MINRANGEDDAMAGE >= 0 && updateMaskArray[UNIT_FIELD_MINRANGEDDAMAGE])
                {
                    updateData.UnitData.MinRangedDamage = updates[UNIT_FIELD_MINRANGEDDAMAGE].FloatValue;
                }
                int UNIT_FIELD_MAXRANGEDDAMAGE = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXRANGEDDAMAGE);
                if (UNIT_FIELD_MAXRANGEDDAMAGE >= 0 && updateMaskArray[UNIT_FIELD_MAXRANGEDDAMAGE])
                {
                    updateData.UnitData.MaxRangedDamage = updates[UNIT_FIELD_MAXRANGEDDAMAGE].FloatValue;
                }
                int UNIT_FIELD_POWER_COST_MODIFIER = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER_COST_MODIFIER);
                if (UNIT_FIELD_POWER_COST_MODIFIER >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_POWER_COST_MODIFIER + i])
                            updateData.UnitData.PowerCostModifier[i] = updates[UNIT_FIELD_POWER_COST_MODIFIER + i].Int32Value;
                    }
                }
                int UNIT_FIELD_POWER_COST_MULTIPLIER = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER_COST_MULTIPLIER);
                if (UNIT_FIELD_POWER_COST_MULTIPLIER >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_POWER_COST_MULTIPLIER + i])
                            updateData.UnitData.PowerCostMultiplier[i] = updates[UNIT_FIELD_POWER_COST_MULTIPLIER + i].FloatValue;
                    }
                }
                int UNIT_FIELD_MAXHEALTHMODIFIER = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXHEALTHMODIFIER);
                if (UNIT_FIELD_MAXHEALTHMODIFIER >= 0 && updateMaskArray[UNIT_FIELD_MAXHEALTHMODIFIER])
                {
                    updateData.UnitData.MaxHealthModifier = updates[UNIT_FIELD_MAXHEALTHMODIFIER].FloatValue;
                }
                int UNIT_FIELD_AURA = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURA);
                int UNIT_FIELD_AURAFLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAFLAGS);
                int UNIT_FIELD_AURALEVELS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURALEVELS);
                int UNIT_FIELD_AURAAPPLICATIONS = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAAPPLICATIONS);
                if (UNIT_FIELD_AURA > 0 && UNIT_FIELD_AURAFLAGS > 0 && UNIT_FIELD_AURALEVELS > 0 && UNIT_FIELD_AURAAPPLICATIONS > 0)
                {
                    int aurasCount = LegacyVersion.GetAuraSlotsCount();
                    for (byte i = 0; i < aurasCount; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_AURA + i] ||
                            updateMaskArray[UNIT_FIELD_AURALEVELS + i / 4] ||
                            updateMaskArray[UNIT_FIELD_AURAAPPLICATIONS + i / 4])
                        {
                            AuraInfo aura = new AuraInfo();
                            aura.Slot = i;
                            aura.AuraData = ReadAuraSlot(i, guid, updates);
                            if (aura.AuraData != null)
                            {
                                int durationLeft;
                                int durationFull;
                                GetSession().GameState.GetAuraDuration(guid, i, out durationLeft, out durationFull);
                                if (durationLeft > 0 && durationFull > 0)
                                {
                                    aura.AuraData.Flags |= AuraFlagsModern.Duration;
                                    aura.AuraData.Duration = durationFull;
                                    aura.AuraData.Remaining = durationLeft;
                                }
                                aura.AuraData.CastUnit = GetSession().GameState.GetAuraCaster(guid, i, aura.AuraData.SpellID);
                            }
                            else if (updateMaskArray[UNIT_FIELD_AURA + i])
                            {
                                GetSession().GameState.ClearAuraDuration(guid, i);
                                GetSession().GameState.ClearAuraCaster(guid, i);
                            }
                            if (aura.AuraData != null || updateMaskArray[UNIT_FIELD_AURA + i])
                                auraUpdate.Auras.Add(aura);
                        }
                    }
                }
            }

            // Player Fields
            if ((objectType == ObjectType.Player) ||
                (objectType == ObjectType.ActivePlayer))
            {
                int PLAYER_DUEL_ARBITER = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DUEL_ARBITER);
                if (PLAYER_DUEL_ARBITER >= 0 && updateMaskArray[PLAYER_DUEL_ARBITER])
                {
                    updateData.PlayerData.DuelArbiter = GetGuidValue(updates, PlayerField.PLAYER_DUEL_ARBITER).To128(GetSession().GameState);
                }
                int PLAYER_FLAGS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FLAGS);
                if (PLAYER_FLAGS >= 0 && updateMaskArray[PLAYER_FLAGS])
                {
                    PlayerFlagsLegacy legacyFlags = (PlayerFlagsLegacy)updates[PLAYER_FLAGS].UInt32Value;
                    var flags = legacyFlags.CastFlags<PlayerFlags>();
                    if (updateData.Guid == GetSession().GameState.CurrentPlayerGuid)
                        GetSession().GameState.CurrentPlayerStorage.Settings.PatchFlags(ref flags); // Some patches like auto guild inv decline
                    updateData.PlayerData.PlayerFlags = (uint) flags;

                    if (updateData.PlayerData.PlayerFlagsEx == null)
                        updateData.PlayerData.PlayerFlagsEx = 0;
                    if (legacyFlags.HasAnyFlag(PlayerFlagsLegacy.HideHelm))
                        updateData.PlayerData.PlayerFlagsEx |= (uint)PlayerFlagsEx.HideHelm;
                    if (legacyFlags.HasAnyFlag(PlayerFlagsLegacy.HideCloak))
                        updateData.PlayerData.PlayerFlagsEx |= (uint)PlayerFlagsEx.HideCloak;

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) &&
                        updateData.UnitData.PvpFlags == null)
                        updateData.UnitData.PvpFlags = ReadPvPFlags(updates);
                }
                else if (updateData.Guid == GetSession().GameState.CurrentPlayerGuid && GetSession().GameState.CurrentPlayerStorage.Settings.NeedToForcePatchFlags)
                { // If we did not patch the PlayerFlags the first time, we need to force include the field
                    PlayerFlags flags = GetSession().GameState.CurrentPlayerStorage.Settings.CreateNewFlags();
                    updateData.PlayerData.PlayerFlags = (uint) flags;
                }

                int PLAYER_GUILDID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
                if (PLAYER_GUILDID >= 0 && updateMaskArray[PLAYER_GUILDID])
                {
                    GetSession().GameState.StorePlayerGuildId(guid, updates[PLAYER_GUILDID].UInt32Value);
                    updateData.UnitData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, updates[PLAYER_GUILDID].UInt32Value);
                }
                int PLAYER_GUILDRANK = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDRANK);
                if (PLAYER_GUILDRANK >= 0 && updateMaskArray[PLAYER_GUILDRANK])
                {
                    updateData.PlayerData.GuildLevel = 25;
                    updateData.PlayerData.GuildRankID = updates[PLAYER_GUILDRANK].UInt32Value;
                }
                int PLAYER_GUILD_TIMESTAMP = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILD_TIMESTAMP);
                if (PLAYER_GUILD_TIMESTAMP >= 0 && updateMaskArray[PLAYER_GUILD_TIMESTAMP])
                {
                    updateData.PlayerData.GuildTimeStamp = updates[PLAYER_GUILD_TIMESTAMP].Int32Value;
                }
                int PLAYER_QUEST_LOG_1_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_QUEST_LOG_1_1);
                if (PLAYER_QUEST_LOG_1_1 >= 0)
                {
                    int questsCount = LegacyVersion.GetQuestLogSize();
                    for (int i = 0; i < questsCount; i++)
                    {
                        updateData.PlayerData.QuestLog[i] = ReadQuestLogEntry(i, updateMaskArray, updates);
                    }
                }
                int PLAYER_CHOSEN_TITLE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_CHOSEN_TITLE);
                if (PLAYER_CHOSEN_TITLE >= 0 && updateMaskArray[PLAYER_CHOSEN_TITLE])
                {
                    updateData.PlayerData.ChosenTitle = updates[PLAYER_CHOSEN_TITLE].Int32Value;
                }
                int PLAYER_VISIBLE_ITEM_1_0 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_0);
                if (PLAYER_VISIBLE_ITEM_1_0 >= 0) // vanilla and tbc
                {
                    int offset = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 16 : 12;
                    for (int i = 0; i < 19; i++)
                    {
                        int itemIdIndex = PLAYER_VISIBLE_ITEM_1_0 + i * offset;
                        int enchantIdIndex = PLAYER_VISIBLE_ITEM_1_0 + 1 + i * offset;
                        if (updateMaskArray[itemIdIndex] || updateMaskArray[enchantIdIndex])
                        {
                            updateData.PlayerData.VisibleItems[i] = new VisibleItem();
                            if (updates.ContainsKey(itemIdIndex))
                                updateData.PlayerData.VisibleItems[i].ItemID = updates[itemIdIndex].Int32Value;
                            if (updates.ContainsKey(enchantIdIndex))
                                updateData.PlayerData.VisibleItems[i].ItemVisual = (ushort)GameData.GetItemEnchantVisual(updates[enchantIdIndex].UInt32Value);
                        }
                    }
                }
                int PLAYER_VISIBLE_ITEM_1_ENTRYID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_ENTRYID);
                if (PLAYER_VISIBLE_ITEM_1_ENTRYID >= 0) // wotlk
                {
                    int offset = 2;
                    for (int i = 0; i < 19; i++)
                    {
                        if (updateMaskArray[PLAYER_VISIBLE_ITEM_1_ENTRYID + i * offset])
                        {
                            updateData.PlayerData.VisibleItems[i] = new VisibleItem();
                            updateData.PlayerData.VisibleItems[i].ItemID = updates[PLAYER_VISIBLE_ITEM_1_ENTRYID + i * offset].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_INV_SLOT_HEAD = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_INV_SLOT_HEAD);
                if (PLAYER_FIELD_INV_SLOT_HEAD >= 0)
                {
                    for (int i = 0; i < 23; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_INV_SLOT_HEAD + i * 2])
                            updateData.ActivePlayerData.InvSlots[i] = GetGuidValue(updates, PLAYER_FIELD_INV_SLOT_HEAD + i * 2).To128(GetSession().GameState);
                    }
                }
                int PLAYER_FIELD_PACK_SLOT_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_PACK_SLOT_1);
                if (PLAYER_FIELD_PACK_SLOT_1 >= 0)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_PACK_SLOT_1 + i * 2])
                            updateData.ActivePlayerData.PackSlots[i] = GetGuidValue(updates, PLAYER_FIELD_PACK_SLOT_1 + i * 2).To128(GetSession().GameState);
                    }
                }
                int PLAYER_FIELD_BANK_SLOT_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BANK_SLOT_1);
                if (PLAYER_FIELD_BANK_SLOT_1 >= 0)
                {
                    int bankSlots = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 28 : 24; // 2.0.0.5965 Alpha 
                    for (int i = 0; i < bankSlots; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_BANK_SLOT_1 + i * 2])
                            updateData.ActivePlayerData.BankSlots[i] = GetGuidValue(updates, PLAYER_FIELD_BANK_SLOT_1 + i * 2).To128(GetSession().GameState);
                    }
                }
                int PLAYER_FIELD_BANKBAG_SLOT_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BANKBAG_SLOT_1);
                if (PLAYER_FIELD_BANKBAG_SLOT_1 >= 0)
                {
                    int bankBagSlots = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 7 : 6; // 2.0.0.5965 Alpha 
                    for (int i = 0; i < bankBagSlots; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_BANKBAG_SLOT_1 + i * 2])
                            updateData.ActivePlayerData.BankBagSlots[i] = GetGuidValue(updates, PLAYER_FIELD_BANKBAG_SLOT_1 + i * 2).To128(GetSession().GameState);
                    }
                }
                int PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_VENDORBUYBACK_SLOT_1);
                if (PLAYER_FIELD_VENDORBUYBACK_SLOT_1 >= 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_VENDORBUYBACK_SLOT_1 + i * 2])
                            updateData.ActivePlayerData.BuyBackSlots[i] = GetGuidValue(updates, PLAYER_FIELD_VENDORBUYBACK_SLOT_1 + i * 2).To128(GetSession().GameState);
                    }
                }
                int PLAYER_FIELD_KEYRING_SLOT_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_KEYRING_SLOT_1);
                if (PLAYER_FIELD_KEYRING_SLOT_1 >= 0)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_KEYRING_SLOT_1 + i * 2])
                            updateData.ActivePlayerData.KeyringSlots[i] = GetGuidValue(updates, PLAYER_FIELD_KEYRING_SLOT_1 + i * 2).To128(GetSession().GameState);
                    }
                }

                byte? skin = null;
                byte? face = null;
                byte? hairStyle = null;
                byte? hairColor = null;
                byte? facialHair = null;

                int PLAYER_BYTES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES);
                if (PLAYER_BYTES >= 0 && updateMaskArray[PLAYER_BYTES])
                {
                    skin = (byte)(updates[PLAYER_BYTES].UInt32Value & 0xFF);
                    face = (byte)((updates[PLAYER_BYTES].UInt32Value >> 8) & 0xFF);
                    hairStyle = (byte)((updates[PLAYER_BYTES].UInt32Value >> 16) & 0xFF);
                    hairColor = (byte)((updates[PLAYER_BYTES].UInt32Value >> 24) & 0xFF);
                }

                RestInfo restInfo = isCreate && guid == GetSession().GameState.CurrentPlayerGuid ? new RestInfo() : null;
                if (restInfo != null)
                    restInfo.StateID = (uint)RestState.Normal;

                int PLAYER_BYTES_2 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES_2);
                if (PLAYER_BYTES_2 >= 0 && updateMaskArray[PLAYER_BYTES_2])
                {
                    facialHair = (byte)(updates[PLAYER_BYTES_2].UInt32Value & 0xFF);
                    updateData.PlayerData.NumBankSlots = (byte)((updates[PLAYER_BYTES_2].UInt32Value >> 16) & 0xFF);

                    if (restInfo == null && guid == GetSession().GameState.CurrentPlayerGuid)
                        restInfo = new RestInfo();
                    if (restInfo != null)
                        restInfo.StateID = (byte)((updates[PLAYER_BYTES_2].UInt32Value >> 24) & 0xFF);
                }

                if (skin != null && face != null && hairStyle != null && hairColor != null && facialHair != null)
                {
                    Race raceId = Race.None;
                    Gender sexId = Gender.None;

                    if (updateData.UnitData.RaceId != null)
                        raceId = (Race)updateData.UnitData.RaceId;
                    if (updateData.UnitData.SexId != null)
                        sexId = (Gender)updateData.UnitData.SexId;

                    if (raceId == Race.None || sexId == Gender.None)
                    {
                        PlayerCache cache;
                        if (GetSession().GameState.CachedPlayers.TryGetValue(guid.To128(GetSession().GameState), out cache))
                        {
                            raceId = cache.RaceId;
                            sexId = cache.SexId;
                        }
                    }
                    
                    if (raceId != Race.None && sexId != Gender.None)
                    {
                        var customizations = CharacterCustomizations.ConvertLegacyCustomizationsToModern(raceId, sexId, (byte)skin, (byte)face, (byte)hairStyle, (byte)hairColor, (byte)facialHair);
                        for (int i = 0; i < 5; i++)
                        {
                            updateData.PlayerData.Customizations[i] = customizations[i];
                        }
                    }
                }

                int PLAYER_REST_STATE_EXPERIENCE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_REST_STATE_EXPERIENCE);
                if (PLAYER_REST_STATE_EXPERIENCE >= 0 && updateMaskArray[PLAYER_REST_STATE_EXPERIENCE])
                {
                    if (restInfo == null && guid == GetSession().GameState.CurrentPlayerGuid)
                        restInfo = new RestInfo();
                    if (restInfo != null)
                        restInfo.Threshold = updates[PLAYER_REST_STATE_EXPERIENCE].UInt32Value;
                }

                if (restInfo != null)
                    updateData.ActivePlayerData.RestInfo[(byte)RestType.XP] = restInfo;

                int PLAYER_BYTES_3 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES_3);
                if (PLAYER_BYTES_3 >= 0 && updateMaskArray[PLAYER_BYTES_3])
                {
                    ushort genderAndInebriation = (ushort)(updates[PLAYER_BYTES_3].UInt32Value & 0xFFFF);
                    updateData.PlayerData.NativeSex = (byte)(genderAndInebriation & 0x1);
                    updateData.PlayerData.Inebriation = (byte)(genderAndInebriation & 0xFFFE);
                    updateData.PlayerData.PvpTitle = (byte)((updates[PLAYER_BYTES_3].UInt32Value >> 16) & 0xFF); // city protector
                    updateData.PlayerData.PvPRank = (byte)((updates[PLAYER_BYTES_3].UInt32Value >> 24) & 0xFF); // honor rank
                }
                int PLAYER_DUEL_TEAM = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DUEL_TEAM);
                if (PLAYER_DUEL_TEAM >= 0 && updateMaskArray[PLAYER_DUEL_TEAM])
                {
                    updateData.PlayerData.DuelTeam = updates[PLAYER_DUEL_TEAM].UInt32Value;
                }
                int PLAYER_FARSIGHT = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FARSIGHT);
                if (PLAYER_FARSIGHT >= 0 && updateMaskArray[PLAYER_FARSIGHT])
                {
                    updateData.ActivePlayerData.FarsightObject = GetGuidValue(updates, PlayerField.PLAYER_FARSIGHT).To128(GetSession().GameState);
                }
                int PLAYER_FIELD_COMBO_TARGET = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_COMBO_TARGET);
                if (PLAYER_FIELD_COMBO_TARGET >= 0 && updateMaskArray[PLAYER_FIELD_COMBO_TARGET])
                {
                    updateData.ActivePlayerData.ComboTarget = GetGuidValue(updates, PlayerField.PLAYER_FIELD_COMBO_TARGET).To128(GetSession().GameState);
                }
                int PLAYER_FIELD_KNOWN_TITLES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_KNOWN_TITLES);
                if (PLAYER_FIELD_KNOWN_TITLES >= 0)
                {
                    int count = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? 3 : 2;
                    for (int i = 0; i < count; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_KNOWN_TITLES + i])
                            updateData.ActivePlayerData.KnownTitles[i] = updates[PLAYER_FIELD_KNOWN_TITLES + i].UInt32Value;
                    }
                }
                int PLAYER_XP = LegacyVersion.GetUpdateField(PlayerField.PLAYER_XP);
                if (PLAYER_XP >= 0 && updateMaskArray[PLAYER_XP])
                {
                    updateData.ActivePlayerData.XP = updates[PLAYER_XP].Int32Value;
                }
                int PLAYER_NEXT_LEVEL_XP = LegacyVersion.GetUpdateField(PlayerField.PLAYER_NEXT_LEVEL_XP);
                if (PLAYER_NEXT_LEVEL_XP >= 0 && updateMaskArray[PLAYER_NEXT_LEVEL_XP])
                {
                    updateData.ActivePlayerData.NextLevelXP = updates[PLAYER_NEXT_LEVEL_XP].Int32Value;
                }
                int PLAYER_SKILL_INFO_1_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SKILL_INFO_1_1);
                if (PLAYER_SKILL_INFO_1_1 >= 0)
                {
                    for (int i = 0; i < 128; i++)
                    {
                        int idIndex = PLAYER_SKILL_INFO_1_1 + i * 3;
                        if (updateMaskArray[idIndex])
                        {
                            updateData.ActivePlayerData.Skill.SkillLineID[i] = (ushort)(updates[idIndex].UInt32Value & 0xFFFF);
                            updateData.ActivePlayerData.Skill.SkillStep[i] = (ushort)((updates[idIndex].UInt32Value >> 16) & 0xFFFF);
                }
                        int valueIndex = idIndex + 1;
                        if (updateMaskArray[valueIndex])
                        {
                            updateData.ActivePlayerData.Skill.SkillRank[i] = (ushort)(updates[valueIndex].UInt32Value & 0xFFFF);
                            updateData.ActivePlayerData.Skill.SkillMaxRank[i] = (ushort)((updates[valueIndex].UInt32Value >> 16) & 0xFFFF);
                        }
                        int bonusIndex = valueIndex + 1;
                        if (updateMaskArray[bonusIndex])
                        {
                            updateData.ActivePlayerData.Skill.SkillTempBonus[i] = (short)(updates[bonusIndex].Int32Value & 0xFFFF);
                            updateData.ActivePlayerData.Skill.SkillPermBonus[i] = (ushort)((updates[bonusIndex].UInt32Value >> 16) & 0xFFFF);
                        }
                    }
                }
                int PLAYER_CHARACTER_POINTS1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_CHARACTER_POINTS1);
                if (PLAYER_CHARACTER_POINTS1 >= 0 && updateMaskArray[PLAYER_CHARACTER_POINTS1])
                {
                    updateData.ActivePlayerData.CharacterPoints = updates[PLAYER_CHARACTER_POINTS1].Int32Value;
                }
                int PLAYER_TRACK_CREATURES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_TRACK_CREATURES);
                if (PLAYER_TRACK_CREATURES >= 0 && updateMaskArray[PLAYER_TRACK_CREATURES])
                {
                    updateData.ActivePlayerData.TrackCreatureMask = updates[PLAYER_TRACK_CREATURES].UInt32Value;
                }
                int PLAYER_TRACK_RESOURCES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_TRACK_RESOURCES);
                if (PLAYER_TRACK_RESOURCES >= 0 && updateMaskArray[PLAYER_TRACK_RESOURCES])
                {
                    updateData.ActivePlayerData.TrackResourceMask[0] = updates[PLAYER_TRACK_RESOURCES].UInt32Value;
                }
                int PLAYER_BLOCK_PERCENTAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BLOCK_PERCENTAGE);
                if (PLAYER_BLOCK_PERCENTAGE >= 0 && updateMaskArray[PLAYER_BLOCK_PERCENTAGE])
                {
                    updateData.ActivePlayerData.BlockPercentage = updates[PLAYER_BLOCK_PERCENTAGE].FloatValue;
                }
                int PLAYER_DODGE_PERCENTAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DODGE_PERCENTAGE);
                if (PLAYER_DODGE_PERCENTAGE >= 0 && updateMaskArray[PLAYER_DODGE_PERCENTAGE])
                {
                    updateData.ActivePlayerData.DodgePercentage = updates[PLAYER_DODGE_PERCENTAGE].FloatValue;
                }
                int PLAYER_PARRY_PERCENTAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_PARRY_PERCENTAGE);
                if (PLAYER_PARRY_PERCENTAGE >= 0 && updateMaskArray[PLAYER_PARRY_PERCENTAGE])
                {
                    updateData.ActivePlayerData.ParryPercentage = updates[PLAYER_PARRY_PERCENTAGE].FloatValue;
                }
                int PLAYER_EXPERTISE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_EXPERTISE);
                if (PLAYER_EXPERTISE >= 0 && updateMaskArray[PLAYER_EXPERTISE])
                {
                    updateData.ActivePlayerData.MainhandExpertise = updates[PLAYER_EXPERTISE].Int32Value;
                }
                int PLAYER_OFFHAND_EXPERTISE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_OFFHAND_EXPERTISE);
                if (PLAYER_OFFHAND_EXPERTISE >= 0 && updateMaskArray[PLAYER_OFFHAND_EXPERTISE])
                {
                    updateData.ActivePlayerData.OffhandExpertise = updates[PLAYER_OFFHAND_EXPERTISE].Int32Value;
                }
                int PLAYER_CRIT_PERCENTAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_CRIT_PERCENTAGE);
                if (PLAYER_CRIT_PERCENTAGE >= 0 && updateMaskArray[PLAYER_CRIT_PERCENTAGE])
                {
                    updateData.ActivePlayerData.CritPercentage = updates[PLAYER_CRIT_PERCENTAGE].FloatValue;
                }
                int PLAYER_RANGED_CRIT_PERCENTAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_RANGED_CRIT_PERCENTAGE);
                if (PLAYER_RANGED_CRIT_PERCENTAGE >= 0 && updateMaskArray[PLAYER_RANGED_CRIT_PERCENTAGE])
                {
                    updateData.ActivePlayerData.RangedCritPercentage = updates[PLAYER_RANGED_CRIT_PERCENTAGE].FloatValue;
                }
                int PLAYER_OFFHAND_CRIT_PERCENTAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_OFFHAND_CRIT_PERCENTAGE);
                if (PLAYER_OFFHAND_CRIT_PERCENTAGE >= 0 && updateMaskArray[PLAYER_OFFHAND_CRIT_PERCENTAGE])
                {
                    updateData.ActivePlayerData.OffhandCritPercentage = updates[PLAYER_OFFHAND_CRIT_PERCENTAGE].FloatValue;
                }
                int PLAYER_SPELL_CRIT_PERCENTAGE1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SPELL_CRIT_PERCENTAGE1);
                if (PLAYER_SPELL_CRIT_PERCENTAGE1 >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[PLAYER_SPELL_CRIT_PERCENTAGE1 + i])
                            updateData.ActivePlayerData.SpellCritPercentage[i] = updates[PLAYER_SPELL_CRIT_PERCENTAGE1 + i].FloatValue;
                    }
                }
                int PLAYER_SHIELD_BLOCK = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SHIELD_BLOCK);
                if (PLAYER_SHIELD_BLOCK >= 0 && updateMaskArray[PLAYER_SHIELD_BLOCK])
                {
                    updateData.ActivePlayerData.ShieldBlock = updates[PLAYER_SHIELD_BLOCK].Int32Value;
                }
                int PLAYER_EXPLORED_ZONES_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_EXPLORED_ZONES_1);
                if (PLAYER_EXPLORED_ZONES_1 >= 0)
                {
                    int maxZones = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 128 : 64;
                    for (int i = 0; i < maxZones; i++)
                    {
                        if (updateMaskArray[PLAYER_EXPLORED_ZONES_1 + i])
                        {
                            if ((i & 1) != 0)
                            {
                                ulong oldValue = updateData.ActivePlayerData.ExploredZones[i / 2] != null ? (ulong)updateData.ActivePlayerData.ExploredZones[i / 2] : 0;
                                updateData.ActivePlayerData.ExploredZones[i / 2] = oldValue | ((ulong)updates[PLAYER_EXPLORED_ZONES_1 + i].UInt32Value << 32);
                            }
                            else
                                updateData.ActivePlayerData.ExploredZones[i / 2] = updates[PLAYER_EXPLORED_ZONES_1 + i].UInt32Value;
                        }
                    }
                }
                int PLAYER_FIELD_COINAGE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_COINAGE);
                if (PLAYER_FIELD_COINAGE >= 0 && updateMaskArray[PLAYER_FIELD_COINAGE])
                {
                    updateData.ActivePlayerData.Coinage = updates[PLAYER_FIELD_COINAGE].UInt32Value;
                }
                int PLAYER_FIELD_POSSTAT0 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_POSSTAT0);
                if (PLAYER_FIELD_POSSTAT0 >= 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_POSSTAT0 + i])
                        {
                            updateData.UnitData.StatPosBuff[i] = updates[PLAYER_FIELD_POSSTAT0 + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_NEGSTAT0 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_NEGSTAT0);
                if (PLAYER_FIELD_NEGSTAT0 >= 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_NEGSTAT0 + i])
                        {
                            updateData.UnitData.StatNegBuff[i] = updates[PLAYER_FIELD_NEGSTAT0 + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE);
                if (PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE + i])
                        {
                            updateData.UnitData.ResistanceBuffModsPositive[i] = updates[PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE);
                if (PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + i])
                        {
                            updateData.UnitData.ResistanceBuffModsNegative[i] = updates[PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_MOD_DAMAGE_DONE_POS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_DAMAGE_DONE_POS);
                if (PLAYER_FIELD_MOD_DAMAGE_DONE_POS >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_MOD_DAMAGE_DONE_POS + i])
                        {
                            updateData.ActivePlayerData.ModDamageDonePos[i] = updates[PLAYER_FIELD_MOD_DAMAGE_DONE_POS + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG);
                if (PLAYER_FIELD_MOD_DAMAGE_DONE_NEG >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + i])
                        {
                            updateData.ActivePlayerData.ModDamageDoneNeg[i] = updates[PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT);
                if (PLAYER_FIELD_MOD_DAMAGE_DONE_PCT >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + i])
                        {
                            updateData.ActivePlayerData.ModDamageDonePercent[i] = updates[PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + i].FloatValue;
                        }
                    }
                }
                int PLAYER_FIELD_MOD_HEALING_DONE_POS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_HEALING_DONE_POS);
                if (PLAYER_FIELD_MOD_HEALING_DONE_POS >= 0 && updateMaskArray[PLAYER_FIELD_MOD_HEALING_DONE_POS])
                {
                    updateData.ActivePlayerData.ModHealingDonePos = updates[PLAYER_FIELD_MOD_HEALING_DONE_POS].Int32Value;
                }
                int PLAYER_FIELD_MOD_TARGET_RESISTANCE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_TARGET_RESISTANCE);
                if (PLAYER_FIELD_MOD_TARGET_RESISTANCE >= 0 && updateMaskArray[PLAYER_FIELD_MOD_TARGET_RESISTANCE])
                {
                    updateData.ActivePlayerData.ModTargetResistance = updates[PLAYER_FIELD_MOD_TARGET_RESISTANCE].Int32Value;
                }
                int PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE);
                if (PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE >= 0 && updateMaskArray[PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE])
                {
                    updateData.ActivePlayerData.ModTargetPhysicalResistance = updates[PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE].Int32Value;
                }
                int PLAYER_FIELD_BYTES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES);
                if (PLAYER_FIELD_BYTES >= 0 && updateMaskArray[PLAYER_FIELD_BYTES])
                {
                    updateData.ActivePlayerData.LocalFlags = (byte)(updates[PLAYER_FIELD_BYTES].UInt32Value & 0xFF);

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        byte comboPoints = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 8) & 0xFF);
                        Class classId = Class.None;
                        if (updateData.UnitData.ClassId != null)
                            classId = (Class)updateData.UnitData.ClassId;
                        else
                            classId = GetSession().GameState.GetUnitClass(guid.To128(GetSession().GameState));
                        sbyte powerSlot = ClassPowerTypes.GetPowerSlotForClass(classId, PowerType.ComboPoints);
                        if (powerSlot >= 0)
                        {
                            if (powerUpdate != null && guid == GetSession().GameState.CurrentPlayerGuid)
                                powerUpdate.Powers.Add(new PowerUpdatePower(comboPoints, (byte)PowerType.ComboPoints));
                            updateData.UnitData.Power[powerSlot] = comboPoints;
                        }
                    }
                    else
                        updateData.ActivePlayerData.GrantableLevels = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 8) & 0xFF);
                    
                    updateData.ActivePlayerData.MultiActionBars = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 16) & 0xFF);
                    updateData.ActivePlayerData.LifetimeMaxRank = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 24) & 0xFF);
                }
                int PLAYER_AMMO_ID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_AMMO_ID);
                if (PLAYER_AMMO_ID >= 0 && updateMaskArray[PLAYER_AMMO_ID])
                {
                    updateData.ActivePlayerData.AmmoID = updates[PLAYER_AMMO_ID].UInt32Value;
                }
                int PLAYER_SELF_RES_SPELL = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SELF_RES_SPELL);
                if (PLAYER_SELF_RES_SPELL >= 0 && updateMaskArray[PLAYER_SELF_RES_SPELL])
                {
                    uint spellId = updates[PLAYER_SELF_RES_SPELL].UInt32Value;
                    updateData.ActivePlayerData.SelfResSpells = new List<uint>();
                    updateData.ActivePlayerData.SelfResSpells.Add(spellId);
                }
                int PLAYER_FIELD_PVP_MEDALS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_PVP_MEDALS);
                if (PLAYER_FIELD_PVP_MEDALS >= 0 && updateMaskArray[PLAYER_FIELD_PVP_MEDALS])
                {
                    updateData.ActivePlayerData.PvpMedals = updates[PLAYER_FIELD_PVP_MEDALS].UInt32Value;
                }
                int PLAYER_FIELD_BUYBACK_PRICE_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BUYBACK_PRICE_1);
                if (PLAYER_FIELD_BUYBACK_PRICE_1 >= 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_BUYBACK_PRICE_1 + i])
                        {
                            updateData.ActivePlayerData.BuybackPrice[i] = updates[PLAYER_FIELD_BUYBACK_PRICE_1 + i].UInt32Value;
                        }
                    }
                }
                int PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BUYBACK_TIMESTAMP_1);
                if (PLAYER_FIELD_BUYBACK_TIMESTAMP_1 >= 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_BUYBACK_TIMESTAMP_1 + i])
                        {
                            updateData.ActivePlayerData.BuybackTimestamp[i] = updates[PLAYER_FIELD_BUYBACK_TIMESTAMP_1 + i].UInt32Value;
                        }
                    }
                }
                int PLAYER_FIELD_SESSION_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_SESSION_KILLS);
                if (PLAYER_FIELD_SESSION_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_SESSION_KILLS]) // vanilla
                {
                    updateData.ActivePlayerData.TodayHonorableKills = (ushort)(updates[PLAYER_FIELD_SESSION_KILLS].UInt32Value & 0xFFFF);
                    updateData.ActivePlayerData.TodayDishonorableKills = (ushort)((updates[PLAYER_FIELD_SESSION_KILLS].UInt32Value >> 16) & 0xFFFF);
                }
                int PLAYER_FIELD_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_KILLS);
                if (PLAYER_FIELD_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_KILLS]) // tbc
                {
                    updateData.ActivePlayerData.TodayHonorableKills = (ushort)(updates[PLAYER_FIELD_KILLS].UInt32Value & 0xFFFF);
                    updateData.ActivePlayerData.YesterdayHonorableKills = (ushort)((updates[PLAYER_FIELD_KILLS].UInt32Value >> 16) & 0xFFFF);
                }
                int PLAYER_FIELD_YESTERDAY_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_YESTERDAY_KILLS);
                if (PLAYER_FIELD_YESTERDAY_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_YESTERDAY_KILLS]) // vanilla
                {
                    updateData.ActivePlayerData.YesterdayHonorableKills = (ushort)(updates[PLAYER_FIELD_YESTERDAY_KILLS].UInt32Value & 0xFFFF);
                    updateData.ActivePlayerData.YesterdayDishonorableKills = (ushort)((updates[PLAYER_FIELD_YESTERDAY_KILLS].UInt32Value >> 16) & 0xFFFF);
                }
                int PLAYER_FIELD_LAST_WEEK_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LAST_WEEK_KILLS);
                if (PLAYER_FIELD_LAST_WEEK_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_LAST_WEEK_KILLS]) // vanilla
                {
                    updateData.ActivePlayerData.LastWeekHonorableKills = (ushort)(updates[PLAYER_FIELD_LAST_WEEK_KILLS].UInt32Value & 0xFFFF);
                    updateData.ActivePlayerData.LastWeekDishonorableKills = (ushort)((updates[PLAYER_FIELD_LAST_WEEK_KILLS].UInt32Value >> 16) & 0xFFFF);
                }
                int PLAYER_FIELD_THIS_WEEK_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_THIS_WEEK_KILLS);
                if (PLAYER_FIELD_THIS_WEEK_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_THIS_WEEK_KILLS]) // vanilla
                {
                    updateData.ActivePlayerData.ThisWeekHonorableKills = (ushort)(updates[PLAYER_FIELD_THIS_WEEK_KILLS].UInt32Value & 0xFFFF);
                    updateData.ActivePlayerData.ThisWeekDishonorableKills = (ushort)((updates[PLAYER_FIELD_THIS_WEEK_KILLS].UInt32Value >> 16) & 0xFFFF);
                }
                int PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION); // vanilla
                if (PLAYER_FIELD_THIS_WEEK_CONTRIBUTION < 0)
                    PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_TODAY_CONTRIBUTION); // tbc
                if (PLAYER_FIELD_THIS_WEEK_CONTRIBUTION >= 0 && updateMaskArray[PLAYER_FIELD_THIS_WEEK_CONTRIBUTION])
                {
                    updateData.ActivePlayerData.ThisWeekContribution = updates[PLAYER_FIELD_THIS_WEEK_CONTRIBUTION].UInt32Value;
                }
                int PLAYER_FIELD_LIFETIME_HONORABLE_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS);
                if (PLAYER_FIELD_LIFETIME_HONORABLE_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_LIFETIME_HONORABLE_KILLS])
                {
                    updateData.ActivePlayerData.LifetimeHonorableKills = updates[PLAYER_FIELD_LIFETIME_HONORABLE_KILLS].UInt32Value;
                }
                int PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS);
                if (PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS >= 0 && updateMaskArray[PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS]) // vanilla
                {
                    updateData.ActivePlayerData.LifetimeDishonorableKills = updates[PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS].UInt32Value;
                }
                int PLAYER_FIELD_YESTERDAY_CONTRIBUTION = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_YESTERDAY_CONTRIBUTION);
                if (PLAYER_FIELD_YESTERDAY_CONTRIBUTION >= 0 && updateMaskArray[PLAYER_FIELD_YESTERDAY_CONTRIBUTION])
                {
                    updateData.ActivePlayerData.YesterdayContribution = updates[PLAYER_FIELD_YESTERDAY_CONTRIBUTION].UInt32Value;
                }
                int PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION);
                if (PLAYER_FIELD_LAST_WEEK_CONTRIBUTION >= 0 && updateMaskArray[PLAYER_FIELD_LAST_WEEK_CONTRIBUTION]) // vanilla
                {
                    updateData.ActivePlayerData.LastWeekContribution = updates[PLAYER_FIELD_LAST_WEEK_CONTRIBUTION].UInt32Value;
                }
                int PLAYER_FIELD_LAST_WEEK_RANK = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LAST_WEEK_RANK);
                if (PLAYER_FIELD_LAST_WEEK_RANK >= 0 && updateMaskArray[PLAYER_FIELD_LAST_WEEK_RANK]) // vanilla
                {
                    updateData.ActivePlayerData.LastWeekRank = updates[PLAYER_FIELD_LAST_WEEK_RANK].UInt32Value;
                }
                int PLAYER_FIELD_BYTES2 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES2);
                if (PLAYER_FIELD_BYTES2 >= 0 && updateMaskArray[PLAYER_FIELD_BYTES2])
                {
                    updateData.ActivePlayerData.PvPRankProgress = (byte)(updates[PLAYER_FIELD_BYTES2].UInt32Value & 0xFF);
                    updateData.ActivePlayerData.AuraVision = (byte)((updates[PLAYER_FIELD_BYTES2].UInt32Value >> 8) & 0xFF);
                }
                int PLAYER_FIELD_WATCHED_FACTION_INDEX = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_WATCHED_FACTION_INDEX);
                if (PLAYER_FIELD_WATCHED_FACTION_INDEX >= 0 && updateMaskArray[PLAYER_FIELD_WATCHED_FACTION_INDEX])
                {
                    updateData.ActivePlayerData.WatchedFactionIndex = updates[PLAYER_FIELD_WATCHED_FACTION_INDEX].Int32Value;
                }
                int PLAYER_FIELD_COMBAT_RATING_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_COMBAT_RATING_1);
                if (PLAYER_FIELD_COMBAT_RATING_1 >= 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_COMBAT_RATING_1 + i])
                        {
                            updateData.ActivePlayerData.CombatRatings[i] = updates[PLAYER_FIELD_COMBAT_RATING_1 + i].Int32Value;
                        }
                    }
                }
                int PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_ARENA_TEAM_INFO_1_1);
                if (PLAYER_FIELD_ARENA_TEAM_INFO_1_1 >= 0)
                {
                    int teamIdOffset = 0;
                    //int teamMemberOffset = 1;
                    int teamGamesWeekOffset = 2;
                    int teamGamesSeasonOffset = 3;
                    int teamWinsSeasonOffset = 4;
                    int teamPersonalRatingOffset = 5;
                    int sizePerEntry = 6;
                    for (int i = 0; i < 3; i++)
                    {
                        int startOffset = PLAYER_FIELD_ARENA_TEAM_INFO_1_1 + i * sizePerEntry;
                        
                        if (updateMaskArray[startOffset + teamIdOffset] &&
                            guid == GetSession().GameState.CurrentPlayerGuid)
                        {
                            uint teamId = GetSession().GameState.CurrentArenaTeamIds[i] = updates[startOffset + teamIdOffset].UInt32Value;

                            if (teamId != 0)
                            {
                                WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
                                packet.WriteUInt32(teamId);
                                SendPacketToServer(packet);

                                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
                                packet2.WriteUInt32(teamId);
                                SendPacketToServer(packet2);
                            }
                            else
                            {
                                ArenaTeamRosterResponse response = new ArenaTeamRosterResponse();
                                response.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex((uint)i);
                                SendPacketToClient(response);
                            }
                        }
                        
                        /*
                        if (updateMaskArray[startOffset + teamMemberOffset])
                        {
                            if (updateData.ActivePlayerData.PvpInfo[i] == null)
                                updateData.ActivePlayerData.PvpInfo[i] = new PVPInfo();

                            updateData.ActivePlayerData.PvpInfo[i].Captain = updates[startOffset + teamMemberOffset].Int32Value;
                        }
                        */
                        if (updateMaskArray[startOffset + teamGamesWeekOffset])
                        {
                            if (updateData.ActivePlayerData.PvpInfo[i] == null)
                                updateData.ActivePlayerData.PvpInfo[i] = new PVPInfo();

                            updateData.ActivePlayerData.PvpInfo[i].WeeklyPlayed = updates[startOffset + teamGamesWeekOffset].UInt32Value;
                        }
                        if (updateMaskArray[startOffset + teamGamesSeasonOffset])
                        {
                            if (updateData.ActivePlayerData.PvpInfo[i] == null)
                                updateData.ActivePlayerData.PvpInfo[i] = new PVPInfo();

                            updateData.ActivePlayerData.PvpInfo[i].SeasonPlayed = updates[startOffset + teamGamesSeasonOffset].UInt32Value;
                        }
                        if (updateMaskArray[startOffset + teamWinsSeasonOffset])
                        {
                            if (updateData.ActivePlayerData.PvpInfo[i] == null)
                                updateData.ActivePlayerData.PvpInfo[i] = new PVPInfo();

                            updateData.ActivePlayerData.PvpInfo[i].SeasonWon = updates[startOffset + teamWinsSeasonOffset].UInt32Value;
                        }
                        if (updateMaskArray[startOffset + teamPersonalRatingOffset])
                        {
                            if (updateData.ActivePlayerData.PvpInfo[i] == null)
                                updateData.ActivePlayerData.PvpInfo[i] = new PVPInfo();

                            updateData.ActivePlayerData.PvpInfo[i].Rating = updates[startOffset + teamPersonalRatingOffset].UInt32Value;
                        }
                    }
                }
                if (guid == GetSession().GameState.CurrentPlayerGuid && ModernVersion.ExpansionVersion > 1)
                {
                    int PLAYER_FIELD_HONOR_CURRENCY = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_HONOR_CURRENCY);
                    int PLAYER_FIELD_ARENA_CURRENCY = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_ARENA_CURRENCY);
                    if (PLAYER_FIELD_HONOR_CURRENCY >= 0 && PLAYER_FIELD_ARENA_CURRENCY >= 0 &&
                       (updateMaskArray[PLAYER_FIELD_HONOR_CURRENCY] || updateMaskArray[PLAYER_FIELD_ARENA_CURRENCY]))
                    {
                        SetupCurrency currencies = new SetupCurrency();
                        if (updates.ContainsKey(PLAYER_FIELD_ARENA_CURRENCY))
                        {
                            SetupCurrency.Record honor = new SetupCurrency.Record();
                            honor.Type = (uint)Currency.ArenaPoints;
                            honor.Quantity = updates[PLAYER_FIELD_ARENA_CURRENCY].UInt32Value;
                            currencies.Data.Add(honor);
                        }
                        if (updates.ContainsKey(PLAYER_FIELD_HONOR_CURRENCY))
                        {
                            SetupCurrency.Record honor = new SetupCurrency.Record();
                            honor.Type = (uint)Currency.HonorPoints;
                            honor.Quantity = updates[PLAYER_FIELD_HONOR_CURRENCY].UInt32Value;
                            currencies.Data.Add(honor);
                        }
                        SendPacketToClient(currencies);
                    }
                }
                int PLAYER_FIELD_MOD_MANA_REGEN = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_MANA_REGEN);
                if (PLAYER_FIELD_MOD_MANA_REGEN >= 0 && updateMaskArray[PLAYER_FIELD_MOD_MANA_REGEN])
                {
                    updateData.UnitData.ModPowerRegen[0] = updates[PLAYER_FIELD_MOD_MANA_REGEN].FloatValue;
                }
                int PLAYER_FIELD_MAX_LEVEL = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MAX_LEVEL);
                if (PLAYER_FIELD_MAX_LEVEL >= 0 && updateMaskArray[PLAYER_FIELD_MAX_LEVEL])
                {
                    updateData.ActivePlayerData.MaxLevel = updates[PLAYER_FIELD_MAX_LEVEL].Int32Value;
                }
                int PLAYER_FIELD_DAILY_QUESTS_1 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_DAILY_QUESTS_1);
                if (PLAYER_FIELD_DAILY_QUESTS_1 >= 0 && guid == GetSession().GameState.CurrentPlayerGuid)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        if (updateMaskArray[PLAYER_FIELD_DAILY_QUESTS_1 + i])
                        {
                            GetSession().GameState.SetDailyQuestSlot((uint)i, updates[PLAYER_FIELD_DAILY_QUESTS_1 + i].UInt32Value);
                            updateData.ActivePlayerData.HasDailyQuestsUpdate = true;
                        }
                    }
                }
            }

            // GameObject Fields
            if (objectType == ObjectType.GameObject)
            {
                int GAMEOBJECT_FIELD_CREATED_BY = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FIELD_CREATED_BY);
                if (GAMEOBJECT_FIELD_CREATED_BY >= 0 && updateMaskArray[GAMEOBJECT_FIELD_CREATED_BY])
                {
                    updateData.GameObjectData.CreatedBy = GetGuidValue(updates, GameObjectField.GAMEOBJECT_FIELD_CREATED_BY).To128(GetSession().GameState);
                }
                int GAMEOBJECT_DISPLAYID = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_DISPLAYID);
                if (GAMEOBJECT_DISPLAYID >= 0 && updateMaskArray[GAMEOBJECT_DISPLAYID])
                {
                    updateData.GameObjectData.DisplayID = updates[GAMEOBJECT_DISPLAYID].Int32Value;
                }
                int GAMEOBJECT_FLAGS = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FLAGS);
                if (GAMEOBJECT_FLAGS >= 0 && updateMaskArray[GAMEOBJECT_FLAGS])
                {
                    updateData.GameObjectData.Flags = updates[GAMEOBJECT_FLAGS].UInt32Value;
                }
                int GAMEOBJECT_ROTATION = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_ROTATION);
                if (GAMEOBJECT_ROTATION >= 0 && updateData.CreateData != null && updateData.CreateData.MoveInfo != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (updateMaskArray[GAMEOBJECT_ROTATION + i])
                            updateData.CreateData.MoveInfo.Rotation[i] = updates[GAMEOBJECT_ROTATION + i].FloatValue;
                    }

                    // Fix for invalid movement of Deeprun Tram, some carts were going through the wall (in the opposite direction)
                    // Entry IDs of Trams:
                    const int tramSouthEastmost = 176080;
                    const int tramNorthMiddle = 176081;
                    const int tramSouthMiddle = 176082;
                    const int tramSouthWestmost = 176083;
                    const int tramNorthWestmost = 176084;
                    const int tramNorthEastmost = 176085;
                    const int zangarmarshElevator = 183177;

                    switch (updateData.ObjectData.EntryID)
                    {
                        case tramSouthEastmost:
                        case tramNorthWestmost:
                        case tramNorthEastmost:
                        {
                            var rot = updateData.CreateData.MoveInfo.Rotation.AsEulerAngles();
                            rot.Yaw *= -1; // Rotate the cart content by 180°, so players who stand on the left side of the cart are actually on the left side
                            updateData.CreateData.MoveInfo.Rotation = rot.AsQuaternion();
                            break;
                        }
                    }

                    switch (updateData.ObjectData.EntryID)
                    {
                        case tramNorthMiddle:
                        case tramSouthMiddle:
                        case tramSouthWestmost:
                        case tramNorthEastmost:
                        {
                            // Quaternion to rotate the pivot point of the transport movement by 180°
                            updateData.GameObjectData.ParentRotation = new float?[] { -4.371139E-08f, 0, 1, 0 };
                            break;
                        }
                        case zangarmarshElevator:
                        {
                            // Super weird angle -88°
                            updateData.GameObjectData.ParentRotation = new float?[] { 0, 0, -0.69465846f, 0.7193397f };
                            break;
                        }
                    }
                }
                int GAMEOBJECT_STATE = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_STATE);
                if (GAMEOBJECT_STATE >= 0 && updateMaskArray[GAMEOBJECT_STATE])
                {
                    updateData.GameObjectData.State = (sbyte)updates[GAMEOBJECT_STATE].Int32Value;
                }
                int GAMEOBJECT_DYN_FLAGS = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_DYN_FLAGS);
                if (GAMEOBJECT_DYN_FLAGS >= 0 && updateMaskArray[GAMEOBJECT_DYN_FLAGS])
                {
                    uint oldValue = 0;
                    if (updateData.ObjectData.DynamicFlags != null)
                        oldValue = (uint)updateData.ObjectData.DynamicFlags;
                    else if (!guid.IsTransport())
                        oldValue = 4294901760;

                    GameObjectDynamicFlagsLegacy flags = (GameObjectDynamicFlagsLegacy)(updates[GAMEOBJECT_DYN_FLAGS].UInt32Value);
                    updateData.ObjectData.DynamicFlags = (oldValue | (uint)flags.CastFlags<GameObjectDynamicFlagsModern>());
                }
                int GAMEOBJECT_FACTION = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FACTION);
                if (GAMEOBJECT_FACTION >= 0 && updateMaskArray[GAMEOBJECT_FACTION])
                {
                    updateData.GameObjectData.FactionTemplate = updates[GAMEOBJECT_FACTION].Int32Value;
                }
                int GAMEOBJECT_TYPE_ID = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_TYPE_ID);
                if (GAMEOBJECT_TYPE_ID >= 0 && updateMaskArray[GAMEOBJECT_TYPE_ID])
                {
                    updateData.GameObjectData.TypeID = (sbyte)updates[GAMEOBJECT_TYPE_ID].Int32Value;
                }
                int GAMEOBJECT_LEVEL = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_LEVEL);
                if (GAMEOBJECT_LEVEL >= 0 && updateMaskArray[GAMEOBJECT_LEVEL])
                {
                    updateData.GameObjectData.Level = updates[GAMEOBJECT_LEVEL].Int32Value;
                }
                int GAMEOBJECT_ARTKIT = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_ARTKIT);
                if (GAMEOBJECT_ARTKIT >= 0 && updateMaskArray[GAMEOBJECT_ARTKIT])
                {
                    updateData.GameObjectData.ArtKit = (byte)updates[GAMEOBJECT_ARTKIT].UInt32Value;
                }
            }

            // DynamicObject Fields
            if (objectType == ObjectType.DynamicObject)
            {
                int DYNAMICOBJECT_CASTER = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_CASTER);
                if (DYNAMICOBJECT_CASTER >= 0 && updateMaskArray[DYNAMICOBJECT_CASTER])
                {
                    updateData.DynamicObjectData.Caster = GetGuidValue(updates, DynamicObjectField.DYNAMICOBJECT_CASTER).To128(GetSession().GameState);
                }
                int DYNAMICOBJECT_SPELLID = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_SPELLID);
                if (DYNAMICOBJECT_SPELLID >= 0 && updateMaskArray[DYNAMICOBJECT_SPELLID])
                {
                    updateData.DynamicObjectData.SpellID = updates[DYNAMICOBJECT_SPELLID].Int32Value;
                    updateData.DynamicObjectData.SpellXSpellVisualID = (int)GameData.GetSpellVisual((uint)updateData.DynamicObjectData.SpellID);
                }
                int DYNAMICOBJECT_RADIUS = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_RADIUS);
                if (DYNAMICOBJECT_RADIUS >= 0 && updateMaskArray[DYNAMICOBJECT_RADIUS])
                {
                    updateData.DynamicObjectData.Radius = updates[DYNAMICOBJECT_RADIUS].FloatValue;
                }
            }

            // Corpse Fields
            if (objectType == ObjectType.Corpse)
            {
                int CORPSE_FIELD_OWNER = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_OWNER);
                if (CORPSE_FIELD_OWNER >= 0 && updateMaskArray[CORPSE_FIELD_OWNER])
                {
                    updateData.CorpseData.Owner = GetGuidValue(updates, CorpseField.CORPSE_FIELD_OWNER).To128(GetSession().GameState);
                }
                int CORPSE_FIELD_DISPLAY_ID = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_DISPLAY_ID);
                if (CORPSE_FIELD_DISPLAY_ID >= 0 && updateMaskArray[CORPSE_FIELD_DISPLAY_ID])
                {
                    updateData.CorpseData.DisplayID = updates[CORPSE_FIELD_DISPLAY_ID].UInt32Value;
                }
                int CORPSE_FIELD_ITEM = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_ITEM);
                if (CORPSE_FIELD_ITEM >= 0)
                {
                    for (int i = 0; i < 19; i++)
                    {
                        if (updateMaskArray[CORPSE_FIELD_ITEM + i])
                            updateData.CorpseData.Items[i] = updates[CORPSE_FIELD_ITEM + i].UInt32Value;
                    }
                }
                int CORPSE_FIELD_BYTES_1 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_BYTES_1);
                if (CORPSE_FIELD_BYTES_1 >= 0 && updateMaskArray[CORPSE_FIELD_BYTES_1])
                {
                    updateData.CorpseData.RaceId = (byte)((updates[CORPSE_FIELD_BYTES_1].UInt32Value >> 8) & 0xFF);
                    updateData.CorpseData.SexId = (byte)((updates[CORPSE_FIELD_BYTES_1].UInt32Value >> 16) & 0xFF);
                    byte skin = (byte)((updates[CORPSE_FIELD_BYTES_1].UInt32Value >> 24) & 0xFF);

                    int CORPSE_FIELD_BYTES_2 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_BYTES_2);
                    if (CORPSE_FIELD_BYTES_2 >= 0 && updateMaskArray[CORPSE_FIELD_BYTES_2])
                    {
                        byte face = (byte)(updates[CORPSE_FIELD_BYTES_2].UInt32Value & 0xFF);
                        byte hairStyle = (byte)((updates[CORPSE_FIELD_BYTES_2].UInt32Value >> 8) & 0xFF);
                        byte hairColor = (byte)((updates[CORPSE_FIELD_BYTES_2].UInt32Value >> 16) & 0xFF);
                        byte facialHair = (byte)((updates[CORPSE_FIELD_BYTES_2].UInt32Value >> 24) & 0xFF);

                        var customizations = CharacterCustomizations.ConvertLegacyCustomizationsToModern((Race)updateData.CorpseData.RaceId, (Gender)updateData.CorpseData.SexId, (byte)skin, (byte)face, (byte)hairStyle, (byte)hairColor, (byte)facialHair);
                        for (int i = 0; i < 5; i++)
                        {
                            updateData.CorpseData.Customizations[i] = customizations[i];
                        }
                    }
                }
                int CORPSE_FIELD_GUILD = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_GUILD);
                if (CORPSE_FIELD_GUILD >= 0 && updateMaskArray[CORPSE_FIELD_GUILD])
                {
                    updateData.CorpseData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, updates[CORPSE_FIELD_GUILD].UInt32Value);
                }
                int CORPSE_FIELD_FLAGS = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_FLAGS);
                if (CORPSE_FIELD_FLAGS >= 0 && updateMaskArray[CORPSE_FIELD_FLAGS])
                {
                    updateData.CorpseData.Flags = updates[CORPSE_FIELD_FLAGS].UInt32Value;

                    // These flags have a different meaning in modern client.
                    if (updateData.CorpseData.Flags.HasAnyFlag(CorpseFlags.HideHelm))
                    {
                        updateData.CorpseData.Flags &= ~(uint)CorpseFlags.HideHelm;
                        updateData.CorpseData.Items[EquipmentSlot.Head] = null;
                    }
                    if (updateData.CorpseData.Flags.HasAnyFlag(CorpseFlags.HideCloak))
                    {
                        updateData.CorpseData.Flags &= ~(uint)CorpseFlags.HideCloak;
                        updateData.CorpseData.Items[EquipmentSlot.Cloak] = null;
                    }
                }
                int CORPSE_FIELD_DYNAMIC_FLAGS = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_DYNAMIC_FLAGS);
                if (CORPSE_FIELD_DYNAMIC_FLAGS >= 0 && updateMaskArray[CORPSE_FIELD_DYNAMIC_FLAGS])
                {
                    updateData.CorpseData.DynamicFlags = updates[CORPSE_FIELD_DYNAMIC_FLAGS].UInt32Value;
                }
            }
        }
    }
}
