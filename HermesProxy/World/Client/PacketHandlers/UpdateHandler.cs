using Framework.GameMath;
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

            UpdateObject updateObject = new UpdateObject();

            for (var i = 0; i < count; i++)
            {
                UpdateTypeLegacy type = (UpdateTypeLegacy)packet.ReadUInt8();
                PrintString($"Update Type = {type}", i);

                switch (type)
                {
                    case UpdateTypeLegacy.Values:
                    {
                        WowGuid64 guid = packet.ReadPackedGuid();
                        PrintString($"Guid = {guid.ToString()}", i);
                        ReadValuesUpdateBlock(packet, guid, i);
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
                        var guid = packet.ReadPackedGuid();
                        PrintString($"Guid = {guid.ToString()}", i);

                        ObjectUpdate updateData = new ObjectUpdate(guid.To128(), UpdateTypeModern.CreateObject1);
                        ReadCreateObjectBlock(packet, guid, updateData, i);
                        break;
                    }
                    case UpdateTypeLegacy.CreateObject2:
                    {
                        var guid = packet.ReadPackedGuid();
                        PrintString($"Guid = {guid.ToString()}", i);

                        ObjectUpdate updateData = new ObjectUpdate(guid.To128(), UpdateTypeModern.CreateObject2);
                        ReadCreateObjectBlock(packet, guid, updateData, i);
                        break;
                    }
                    case UpdateTypeLegacy.NearObjects:
                    {
                        ReadObjectsBlock(packet, i);
                        break;
                    }
                    case UpdateTypeLegacy.FarObjects:
                    {
                        ReadDestroyObjectsBlock(packet, i);
                        break;
                    }
                }
            }
        }

        public void ReadObjectsBlock(WorldPacket packet, object index)
        {
            var objCount = packet.ReadInt32();
            PrintString($"NearObjectsCount = {objCount}", index);
            for (var j = 0; j < objCount; j++)
            {
                var guid = packet.ReadPackedGuid();
                PrintString($"Guid = {objCount}", index, j);
            }
        }

        public void ReadDestroyObjectsBlock(WorldPacket packet, object index)
        {
            var objCount = packet.ReadInt32();
            PrintString($"FarObjectsCount = {objCount}", index);
            for (var j = 0; j < objCount; j++)
            {
                var guid = packet.ReadPackedGuid();
                PrintString($"Guid = {objCount}", index, j);
            }
        }

        private void ReadCreateObjectBlock(WorldPacket packet, WowGuid guid, ObjectUpdate updateData, object index)
        {
            updateData.CreateData.ObjectType = ObjectTypeConverter.Convert((ObjectTypeLegacy)packet.ReadUInt8());
            ReadMovementUpdateBlock(packet, guid, updateData, index);
            ReadValuesUpdateBlockOnCreate(packet, guid, updateData.CreateData.ObjectType, updateData, index);
        }

        public void ReadValuesUpdateBlockOnCreate(WorldPacket packet, WowGuid guid, ObjectType type, ObjectUpdate updateData, object index)
        {
            BitArray updateMaskArray = null;
            var updates = ReadValuesUpdateBlock(packet, type, index, true, null, out updateMaskArray);
            StoreObjectUpdate(guid, type, updateMaskArray, updates, true, updateData);
        }

        public void ReadValuesUpdateBlock(WorldPacket packet, WowGuid guid, int index)
        {
            BitArray updateMaskArray = null;
            var updates = ReadValuesUpdateBlock(packet, guid.GetObjectType(), index, false, null, out updateMaskArray);
            //StoreObjectUpdate
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

        private const bool DebugUpdates = false;
        private void PrintString(string txt, params object[] indexes)
        {
            if (DebugUpdates)
                Console.WriteLine("{0}{1}", GetIndexString(indexes), txt);
        }

        private T PrintValue<T>(string name, T obj, params object[] indexes)
        {
            if (DebugUpdates)
                Console.WriteLine("{0}{1}: {2}", GetIndexString(indexes), name, obj);
            return obj;
        }

        private Dictionary<int, UpdateField> ReadValuesUpdateBlock(WorldPacket packet, ObjectType type, object index, bool isCreating, Dictionary<int, UpdateField> oldValues, out BitArray outUpdateMaskArray)
        {
            bool skipDictionary = false;
            bool missingCreateObject = !isCreating && oldValues == null;
            var maskSize = packet.ReadUInt8();

            var updateMask = new int[maskSize];
            for (var i = 0; i < maskSize; i++)
                updateMask[i] = packet.ReadInt32();

            var mask = new BitArray(updateMask);
            outUpdateMaskArray = mask;
            var dict = new Dictionary<int, UpdateField>();

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

                if (!skipDictionary)
                    for (int k = 0; k < fieldData.Count; ++k)
                        if (!dict.ContainsKey(start + k))
                            dict.Add(start + k, fieldData[k]);
            }

            return dict;
        }

        private static void ReadMovementUpdateBlock(WorldPacket packet, WowGuid guid, ObjectUpdate updateData, object index)
        {
            var moveInfo = new MovementInfo();

            UpdateFlag flags;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                flags = (UpdateFlag)packet.ReadUInt16();
            else
                flags = (UpdateFlag)packet.ReadUInt8();

            if (flags.HasAnyFlag(UpdateFlag.Self) && updateData != null)
                updateData.CreateData.ThisIsYou = true;

            if (flags.HasAnyFlag(UpdateFlag.Living))
            {
                moveInfo = ReadMovementInfo(packet, guid);
                var moveFlags = moveInfo.Flags;

                var speeds = 6;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    speeds = 9;
                else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    speeds = 8;

                for (var i = 0; i < speeds; ++i)
                {
                    var speedType = (SpeedType)i;
                    var speed = packet.ReadFloat();

                    switch (speedType)
                    {
                        case SpeedType.Walk:
                        {
                            moveInfo.WalkSpeed = speed;
                            break;
                        }
                        case SpeedType.Run:
                        {
                            moveInfo.RunSpeed = speed;
                            break;
                        }
                        case SpeedType.RunBack:
                        {
                            moveInfo.RunBackSpeed = speed;
                            break;
                        }
                        case SpeedType.Swim:
                        {
                            moveInfo.SwimSpeed = speed;
                            break;
                        }
                        case SpeedType.SwimBack:
                        {
                            moveInfo.SwimBackSpeed = speed;
                            break;
                        }
                        case SpeedType.Turn:
                        {
                            moveInfo.TurnRate = speed;
                            break;
                        }
                        case SpeedType.Fly:
                        {
                            moveInfo.FlightSpeed = speed;
                            break;
                        }
                        case SpeedType.FlyBack:
                        {
                            moveInfo.FlightBackSpeed = speed;
                            break;
                        }
                        case SpeedType.Pitch:
                        {
                            moveInfo.PitchRate = speed;
                            break;
                        }
                    }
                }

                if (moveFlags.HasAnyFlag(MovementFlagWotLK.SplineEnabled))
                {
                    ServerSideMovement monsterMove = new ServerSideMovement();

                    if (moveInfo.TransportGuid != null)
                        monsterMove.TransportGuid = moveInfo.TransportGuid;
                    monsterMove.TransportSeat = moveInfo.TransportSeat;

                    monsterMove.SplineFlags = SplineFlagModern.None;
                    monsterMove.SplineType = SplineTypeModern.None;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    {
                        SplineFlagWotLK splineFlags = (SplineFlagWotLK)packet.ReadUInt32();
                        monsterMove.SplineFlags = splineFlags.CastFlags<SplineFlagModern>();

                        if (splineFlags.HasAnyFlag(SplineFlagWotLK.FinalTarget))
                        {
                            monsterMove.FinalFacingGuid = packet.ReadGuid();
                            monsterMove.SplineType = SplineTypeModern.FacingTarget;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagWotLK.FinalOrientation))
                        {
                            monsterMove.FinalOrientation = packet.ReadFloat();
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

                        if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalTarget))
                        {
                            monsterMove.FinalFacingGuid = packet.ReadGuid();
                            monsterMove.SplineType = SplineTypeModern.FacingTarget;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalOrientation))
                        {
                            monsterMove.FinalOrientation = packet.ReadFloat();
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

                        if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalTarget))
                        {
                            monsterMove.FinalFacingGuid = packet.ReadGuid();
                            monsterMove.SplineType = SplineTypeModern.FacingTarget;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalOrientation))
                        {
                            monsterMove.FinalOrientation = packet.ReadFloat();
                            monsterMove.SplineType = SplineTypeModern.FacingAngle;
                        }
                        else if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalPoint))
                        {
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
                            monsterMove.SplineType = SplineTypeModern.FacingSpot;
                        }
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
                    moveInfo.TransportGuid = packet.ReadPackedGuid();

                    moveInfo.Position = packet.ReadVector3();
                    moveInfo.TransportOffset.X = packet.ReadFloat();
                    moveInfo.TransportOffset.Y = packet.ReadFloat();
                    moveInfo.TransportOffset.Z = packet.ReadFloat();

                    moveInfo.Orientation = packet.ReadFloat();
                    moveInfo.TransportOffset.W = moveInfo.Orientation;

                    moveInfo.CorpseOrientation = packet.ReadFloat();
                }
                else if (flags.HasAnyFlag(UpdateFlag.StationaryObject))
                {
                    moveInfo.Position = packet.ReadVector3();
                    moveInfo.Orientation = packet.ReadFloat();
                }
            }

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V4_2_2_14545))
            {
                if (flags.HasAnyFlag(UpdateFlag.LowGuid))
                    packet.ReadUInt32();

                if (flags.HasAnyFlag(UpdateFlag.HighGuid))
                    packet.ReadUInt32();
            }

            if (flags.HasAnyFlag(UpdateFlag.AttackingTarget))
            {
                WowGuid64 attackGuid = packet.ReadPackedGuid();
                if (updateData != null)
                    updateData.CreateData.AutoAttackVictim = attackGuid.To128();
            }

            if (flags.HasAnyFlag(UpdateFlag.Transport))
                moveInfo.TransportPathTimer = packet.ReadUInt32();

            if (flags.HasAnyFlag(UpdateFlag.Vehicle))
            {
                moveInfo.VehicleId = packet.ReadUInt32();
                moveInfo.VehicleOrientation = packet.ReadFloat();
            }

            if (flags.HasAnyFlag(UpdateFlag.GORotation))
                moveInfo.Rotation = packet.ReadPackedQuaternion();

            if (updateData != null)
            {
                moveInfo.Flags = (uint)(((MovementFlagWotLK)moveInfo.Flags).CastFlags<MovementFlagModern>());
                updateData.CreateData.MoveInfo = moveInfo;
            }
        }

        private static WowGuid GetGuidValue<T>(Dictionary<int, UpdateField> UpdateFields, T field) where T : System.Enum
        {
            if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
            {
                var parts = UpdateFields.GetArray<T, uint>(field, 2);
                return new WowGuid64(Utilities.MAKE_PAIR64(parts[0], parts[1]));
            }
            else
            {
                var parts = UpdateFields.GetArray<T, uint>(field, 4);
                return new WowGuid128(Utilities.MAKE_PAIR64(parts[0], parts[1]), Utilities.MAKE_PAIR64(parts[2], parts[3]));
            }
        }

        public void StoreObjectUpdate(WowGuid guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, bool isCreate, ObjectUpdate updateData)
        {
            // Object Fields
            int OBJECT_FIELD_GUID = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_GUID);
            if (OBJECT_FIELD_GUID >= 0 && updateMaskArray[OBJECT_FIELD_GUID])
            {
                updateData.ObjectData.Guid = GetGuidValue(updates, ObjectField.OBJECT_FIELD_GUID).To128();
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

            // Unit Fields
            if ((objectType == ObjectType.Unit) ||
                (objectType == ObjectType.Player) ||
                (objectType == ObjectType.ActivePlayer))
            {
                int UNIT_FIELD_CHARM = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHARM);
                if (UNIT_FIELD_CHARM >= 0 && updateMaskArray[UNIT_FIELD_CHARM])
                {
                    updateData.UnitData.Charm = GetGuidValue(updates, UnitField.UNIT_FIELD_CHARM).To128();
                }
                int UNIT_FIELD_SUMMON = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMON);
                if (UNIT_FIELD_SUMMON >= 0 && updateMaskArray[UNIT_FIELD_SUMMON])
                {
                    updateData.UnitData.Summon = GetGuidValue(updates, UnitField.UNIT_FIELD_SUMMON).To128();
                }
                int UNIT_FIELD_CHARMEDBY = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHARMEDBY);
                if (UNIT_FIELD_CHARMEDBY >= 0 && updateMaskArray[UNIT_FIELD_CHARMEDBY])
                {
                    updateData.UnitData.CharmedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_CHARMEDBY).To128();
                }
                int UNIT_FIELD_SUMMONEDBY = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMONEDBY);
                if (UNIT_FIELD_SUMMONEDBY >= 0 && updateMaskArray[UNIT_FIELD_SUMMONEDBY])
                {
                    updateData.UnitData.SummonedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_SUMMONEDBY).To128();
                }
                int UNIT_FIELD_CREATEDBY = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CREATEDBY);
                if (UNIT_FIELD_CREATEDBY >= 0 && updateMaskArray[UNIT_FIELD_CREATEDBY])
                {
                    updateData.UnitData.CreatedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_CREATEDBY).To128();
                }
                int UNIT_FIELD_TARGET = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_TARGET);
                if (UNIT_FIELD_TARGET >= 0 && updateMaskArray[UNIT_FIELD_TARGET])
                {
                    updateData.UnitData.Target = GetGuidValue(updates, UnitField.UNIT_FIELD_TARGET).To128();
                }
                int UNIT_FIELD_HEALTH = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_HEALTH);
                if (UNIT_FIELD_HEALTH >= 0 && updateMaskArray[UNIT_FIELD_HEALTH])
                {
                    updateData.UnitData.Health = updates[UNIT_FIELD_HEALTH].Int32Value;
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

                Class classId = Class.None;
                int UNIT_FIELD_BYTES_0 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_0);
                if (UNIT_FIELD_BYTES_0 >= 0 && updateMaskArray[UNIT_FIELD_BYTES_0])
                {
                    updateData.UnitData.RaceId = (byte)(updates[UNIT_FIELD_BYTES_0].UInt32Value & 0xFF);
                    updateData.UnitData.ClassId = (byte)((updates[UNIT_FIELD_BYTES_0].UInt32Value >> 8) & 0xFF);
                    updateData.UnitData.SexId = (byte)((updates[UNIT_FIELD_BYTES_0].UInt32Value >> 16) & 0xFF);
                    updateData.UnitData.DisplayPower = (byte)((updates[UNIT_FIELD_BYTES_0].UInt32Value >> 24) & 0xFF);

                    classId = (Class)updateData.UnitData.ClassId;
                    if (objectType == ObjectType.Player || objectType == ObjectType.ActivePlayer)
                        updateData.UnitData.PlayerClassId = updateData.UnitData.ClassId;
                }

                int UNIT_FIELD_POWER1 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER1);
                if (UNIT_FIELD_POWER1 >= 0)
                {
                    for (int i = 0; i < LegacyVersion.GetPowersCount(); i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_POWER1 + i])
                        {
                            sbyte index = ClassPowerTypes.GetPowerSlotForClass(classId != Class.None ? classId : Global.CurrentSessionData.GameData.GetUnitClass(guid), (PowerType)i);
                            if (index >= 0)
                                updateData.UnitData.Power[i] = updates[UNIT_FIELD_POWER1 + i].Int32Value;
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
                            sbyte index = ClassPowerTypes.GetPowerSlotForClass(classId != Class.None ? classId : Global.CurrentSessionData.GameData.GetUnitClass(guid), (PowerType)i);
                            if (index >= 0)
                                updateData.UnitData.MaxPower[i] = updates[UNIT_FIELD_MAXPOWER1 + i].Int32Value;
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
                            updateData.UnitData.PetFlags |= (byte)PetFlags.CanBeRenamed;
                        if (vanillaFlags.HasAnyFlag(UnitFlagsVanilla.PetAbandon))
                            updateData.UnitData.PetFlags |= (byte)PetFlags.CanBeAbandoned;
                    }
                    else
                    {
                        updateData.UnitData.Flags = updates[UNIT_FIELD_FLAGS].UInt32Value;
                    }

                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) &&
                        updateData.UnitData.Flags.HasAnyFlag(UnitFlags.Pvp))
                        updateData.UnitData.PvpFlags |= (byte)PvPFlags.PvP;
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
                    updateData.UnitData.PetLoyaltyIndex = (byte)((updates[UNIT_FIELD_BYTES_1].UInt32Value >> 8) & 0xFF);

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
                    updateData.ObjectData.DynamicFlags = updates[UNIT_DYNAMIC_FLAGS].UInt32Value;
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
                }
                int UNIT_NPC_FLAGS = LegacyVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
                if (UNIT_NPC_FLAGS >= 0 && updateMaskArray[UNIT_NPC_FLAGS])
                {
                    if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        NPCFlagsVanilla vanillaFlags = (NPCFlagsVanilla)updates[UNIT_NPC_FLAGS].UInt32Value;
                        updateData.UnitData.NpcFlags = (uint)(vanillaFlags.CastFlags<NPCFlags>());
                    }
                    else
                    {
                        updateData.UnitData.NpcFlags = updates[UNIT_NPC_FLAGS].UInt32Value;
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
                int UNIT_FIELD_RESISTANCES = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCES);
                if (UNIT_FIELD_RESISTANCES >= 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (updateMaskArray[UNIT_FIELD_RESISTANCES + i])
                            updateData.UnitData.Resistances[i] = updates[UNIT_FIELD_RESISTANCES + i].Int32Value;
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
            }

            // Player Fields
            if ((objectType == ObjectType.Player) ||
                (objectType == ObjectType.ActivePlayer))
            {
                int PLAYER_DUEL_ARBITER = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DUEL_ARBITER);
                if (PLAYER_DUEL_ARBITER >= 0 && updateMaskArray[PLAYER_DUEL_ARBITER])
                {
                    updateData.PlayerData.DuelArbiter = GetGuidValue(updates, PlayerField.PLAYER_DUEL_ARBITER).To128();
                }
                int PLAYER_FLAGS = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FLAGS);
                if (PLAYER_FLAGS >= 0 && updateMaskArray[PLAYER_FLAGS])
                {
                    updateData.PlayerData.PlayerFlags = updates[PLAYER_FLAGS].UInt32Value;
                }
                int PLAYER_GUILDID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
                if (PLAYER_GUILDID >= 0 && updateMaskArray[PLAYER_GUILDID])
                {
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

                byte? restState = null;

                int PLAYER_BYTES_2 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES_2);
                if (PLAYER_BYTES_2 >= 0 && updateMaskArray[PLAYER_BYTES_2])
                {
                    facialHair = (byte)(updates[PLAYER_BYTES_2].UInt32Value & 0xFF);
                    updateData.PlayerData.NumBankSlots = (byte)((updates[PLAYER_BYTES_2].UInt32Value >> 16) & 0xFF);
                    restState = (byte)((updates[PLAYER_BYTES_2].UInt32Value >> 24) & 0xFF);
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
                        Global.PlayerCache cache;
                        if (Global.CurrentSessionData.GameData.CachedPlayers.TryGetValue(guid, out cache))
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
                    RestInfo restInfo = new RestInfo();
                    restInfo.StateID = restState;
                    restInfo.Threshold = updates[PLAYER_REST_STATE_EXPERIENCE].UInt32Value;
                }

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
            }
        }
    }
}
