using Framework.GameMath;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using World;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
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

            for (var i = 0; i < count; i++)
            {
                UpdateType type = (UpdateType)packet.ReadUInt8();
                PrintString($"Update Type = {type}", i);

                switch (type)
                {
                    case UpdateType.Values:
                    {
                        WowGuid64 guid = packet.ReadPackedGuid();
                        PrintString($"Guid = {guid.ToString()}", i);
                        ReadValuesUpdateBlock(packet, guid, i);
                        break;
                    }
                    case UpdateType.Movement:
                    {
                        var guid = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901) ? packet.ReadPackedGuid() : packet.ReadGuid();
                        PrintString($"Guid = {guid.ToString()}", i);
                        var moves = ReadMovementUpdateBlock(packet, guid, i);
                        break;
                    }
                    case UpdateType.CreateObject1:
                    case UpdateType.CreateObject2:
                    {
                        var guid = packet.ReadPackedGuid();
                        PrintString($"Guid = {guid.ToString()}", i);
                        ReadCreateObjectBlock(packet, guid, i);
                        break;
                    }
                    case UpdateType.NearObjects:
                    {
                        ReadObjectsBlock(packet, i);
                        break;
                    }
                    case UpdateType.FarObjects:
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

        private void ReadCreateObjectBlock(WorldPacket packet, WowGuid guid, object index)
        {
            ObjectType objType = ObjectTypeConverter.Convert((ObjectTypeLegacy)packet.ReadUInt8());
            var moves = ReadMovementUpdateBlock(packet, guid, index);

            BitArray updateMaskArray = null;
            var updates = ReadValuesUpdateBlockOnCreate(packet, objType, index, out updateMaskArray);
        }

        public Dictionary<int, UpdateField> ReadValuesUpdateBlockOnCreate(WorldPacket packet, ObjectType type, object index, out BitArray outUpdateMaskArray)
        {
            return ReadValuesUpdateBlock(packet, type, index, true, null, out outUpdateMaskArray);
        }

        public void ReadValuesUpdateBlock(WorldPacket packet, WowGuid guid, int index)
        {
            BitArray updateMaskArray = null;
            ReadValuesUpdateBlock(packet, guid.GetObjectType(), index, false, null, out updateMaskArray);
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

        private static MovementInfo ReadMovementUpdateBlock(WorldPacket packet, WowGuid guid, object index)
        {
            var moveInfo = new MovementInfo();

            UpdateFlag flags;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                flags = (UpdateFlag)packet.ReadUInt16();
            else
                flags = (UpdateFlag)packet.ReadUInt8();

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

                if (moveFlags.HasAnyFlag(MovementFlag.SplineEnabled))
                {
                    ServerSideMovement monsterMove = new ServerSideMovement();

                    if (moveInfo.TransportGuid != null)
                        monsterMove.TransportGuid = moveInfo.TransportGuid;
                    monsterMove.TransportSeat = moveInfo.TransportSeat;

                    // Temp solution
                    // TODO: Make Enums version friendly
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    {
                        SplineFlag splineFlags = (SplineFlag)packet.ReadUInt32();
                        monsterMove.SplineFlags = (uint)splineFlags;

                        if (splineFlags.HasAnyFlag(SplineFlag.FinalTarget))
                            monsterMove.FinalFacingGuid = packet.ReadGuid();
                        else if (splineFlags.HasAnyFlag(SplineFlag.FinalOrientation))
                            monsterMove.FinalOrientation = packet.ReadFloat();
                        else if (splineFlags.HasAnyFlag(SplineFlag.FinalPoint))
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
                    }
                    else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        SplineFlagTBC splineFlags = (SplineFlagTBC)packet.ReadUInt32();
                        monsterMove.SplineFlags = (uint)splineFlags;

                        if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalTarget))
                            monsterMove.FinalFacingGuid = packet.ReadGuid();
                        else if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalOrientation))
                            monsterMove.FinalOrientation = packet.ReadFloat();
                        else if (splineFlags.HasAnyFlag(SplineFlagTBC.FinalPoint))
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
                    }
                    else
                    {
                        SplineFlagVanilla splineFlags = (SplineFlagVanilla)packet.ReadUInt32();
                        monsterMove.SplineFlags = (uint)splineFlags;

                        if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalTarget))
                            monsterMove.FinalFacingGuid = packet.ReadGuid();
                        else if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalOrientation))
                            monsterMove.FinalOrientation = packet.ReadFloat();
                        else if (splineFlags.HasAnyFlag(SplineFlagVanilla.FinalPoint))
                            monsterMove.FinalFacingSpot = packet.ReadVector3();
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
                        packet.ReadUInt8(); // Spline Mode

                    monsterMove.EndPosition = packet.ReadVector3();
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
                packet.ReadPackedGuid();

            if (flags.HasAnyFlag(UpdateFlag.Transport))
                moveInfo.TransportPathTimer = packet.ReadUInt32();

            if (flags.HasAnyFlag(UpdateFlag.Vehicle))
            {
                moveInfo.VehicleId = packet.ReadUInt32();
                moveInfo.VehicleOrientation = packet.ReadFloat();
            }

            if (flags.HasAnyFlag(UpdateFlag.GORotation))
                moveInfo.Rotation = packet.ReadPackedQuaternion();

            return moveInfo;
        }
    }
}
