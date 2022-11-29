/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class CreateObjectData
    {
        public ObjectType ObjectType;
        public MovementInfo MoveInfo;
        public ServerSideMovement MoveSpline;
        public bool NoBirthAnim;
        public bool EnablePortals;
        public bool PlayHoverAnim;
        public bool ThisIsYou;
        public WowGuid128 AutoAttackVictim;
    }
    public class ObjectUpdate
    {
        public ObjectUpdate(WowGuid128 guid, UpdateTypeModern type, GlobalSessionData globalSession)
        {
            Type = type;
            Guid = guid;
            GlobalSession = globalSession;
            ObjectData = new ObjectData();

            switch (type)
            {
                case UpdateTypeModern.CreateObject1:
                case UpdateTypeModern.CreateObject2:
                    CreateData = new CreateObjectData();
                    break;
            }

            switch (guid.GetObjectType())
            {
                case ObjectType.Item:
                case ObjectType.Container:
                    ItemData = new ItemData();
                    ContainerData = new ContainerData();
                    break;
                case ObjectType.Unit:
                    UnitData = new UnitData();
                    break;
                case ObjectType.Player:
                case ObjectType.ActivePlayer:
                    UnitData = new UnitData();
                    PlayerData = new PlayerData();
                    ActivePlayerData = new ActivePlayerData();
                    break;
                case ObjectType.GameObject:
                    GameObjectData = new GameObjectData();
                    break;
                case ObjectType.DynamicObject:
                    DynamicObjectData = new DynamicObjectData();
                    break;
                case ObjectType.Corpse:
                    CorpseData = new CorpseData();
                    break;
            }
        }

        public UpdateTypeModern Type;
        public WowGuid128 Guid;
        public GlobalSessionData GlobalSession;
        public CreateObjectData CreateData;
        public ObjectData ObjectData;
        public ItemData ItemData;
        public ContainerData ContainerData;
        public UnitData UnitData;
        public PlayerData PlayerData;
        public ActivePlayerData ActivePlayerData;
        public GameObjectData GameObjectData;
        public DynamicObjectData DynamicObjectData;
        public CorpseData CorpseData;

        public void InitializePlaceholders()
        {
            if (CreateData == null)
                return;

            if (CreateData.MoveInfo != null)
            {
                if (CreateData.MoveInfo.WalkSpeed == 0)
                    CreateData.MoveInfo.WalkSpeed = 2.5f;
                if (CreateData.MoveInfo.RunSpeed == 0)
                    CreateData.MoveInfo.RunSpeed = 7;
                if (CreateData.MoveInfo.RunBackSpeed == 0)
                    CreateData.MoveInfo.RunBackSpeed = 4.5f;
                if (CreateData.MoveInfo.SwimSpeed == 0)
                    CreateData.MoveInfo.SwimSpeed = 4.722222f;
                if (CreateData.MoveInfo.SwimBackSpeed == 0)
                    CreateData.MoveInfo.SwimBackSpeed = 2.5f;
                if (CreateData.MoveInfo.FlightSpeed == 0)
                    CreateData.MoveInfo.FlightSpeed = 7;
                if (CreateData.MoveInfo.FlightBackSpeed == 0)
                    CreateData.MoveInfo.FlightBackSpeed = 4.5f;
                if (CreateData.MoveInfo.TurnRate == 0)
                    CreateData.MoveInfo.TurnRate = 3.141594f;
                if (CreateData.MoveInfo.PitchRate == 0)
                    CreateData.MoveInfo.PitchRate = CreateData.MoveInfo.TurnRate;
                if (CreateData.MoveInfo.Flags.HasAnyFlag(MovementFlagModern.WalkMode) && (CreateData.MoveSpline != null))
                    CreateData.MoveInfo.Flags &= ~(uint)MovementFlagModern.WalkMode;
                if (CreateData.MoveInfo.FlagsExtra == 0)
                    CreateData.MoveInfo.FlagsExtra = 512;
                if (CreateData.MoveInfo.Orientation < 0)
                    CreateData.MoveInfo.Orientation += (float)(Math.PI * 2f);
                if (CreateData.MoveInfo.Orientation > (float)(Math.PI * 2f))
                    CreateData.MoveInfo.Orientation -= (float)(Math.PI * 2f);
            }
            if (CreateData.MoveSpline != null)
            {
                if (CreateData.MoveSpline.SplineFlags == 0)
                    CreateData.MoveSpline.SplineFlags = (SplineFlagModern)2432696320;
            }
            if (GameObjectData != null)
            {
                if ((GameObjectData.PercentHealth == null) &&
                    (GameObjectData.State != null || GameObjectData.TypeID != null || GameObjectData.ArtKit != null))
                    GameObjectData.PercentHealth = 255;
                if (GameObjectData.ParentRotation[3] == null)
                    GameObjectData.ParentRotation[3] = 1;
                GameObjectData.StateAnimID ??= ModernVersion.GetGameObjectStateAnimId();
                if (Guid.GetHighType() == HighGuidType.Transport)
                {
                    uint period = GameData.GetTransportPeriod((uint)ObjectData.EntryID);
                    if (period != 0)
                    {
                        GameObjectData.Level ??= (int)period;
                        ObjectData.DynamicFlags ??= (((uint)(((float)(CreateData.MoveInfo.TransportPathTimer % period) / (float)period) * ushort.MaxValue)) << 16);
                        GameObjectData.Flags = 1048616;
                    }
                    else ObjectData.DynamicFlags ??= ((CreateData.MoveInfo.TransportPathTimer % ushort.MaxValue) << 16);
                }
            }
            if (CorpseData != null)
            {
                if (CorpseData.ClassId == null)
                {
                    if (CorpseData.Owner != null)
                        CorpseData.ClassId = (byte)GlobalSession.GameState.GetUnitClass(CorpseData.Owner);
                    else
                        CorpseData.ClassId = 1;
                }
            }
            if (UnitData != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (UnitData.ModPowerRegen[i] == null)
                        UnitData.ModPowerRegen[i] = 1;
                }
                UnitData.Flags2 ??= 2048;
                UnitData.DisplayScale ??= 1;
                UnitData.NativeXDisplayScale ??= 1;
                UnitData.ModCastHaste ??= 1;
                UnitData.ModHaste ??= 1;
                UnitData.ModRangedHaste ??= 1;
                UnitData.ModHasteRegen ??= 1;
                UnitData.ModTimeRate ??= 1;
                UnitData.HoverHeight ??= 1;
                UnitData.ScaleDuration ??= 100;
                UnitData.LookAtControllerID ??= -1;
                if (UnitData.ChannelObject == null &&
                    Guid == GlobalSession.GameState.CurrentPlayerGuid)
                    UnitData.ChannelObject = WowGuid128.Empty;
            }
            if (PlayerData != null)
            {
                if (PlayerData.WowAccount == null)
                {
                    if (CreateData.ThisIsYou == true)
                        PlayerData.WowAccount = WowGuid128.Create(HighGuidType703.WowAccount, GlobalSession.GameAccountInfo.Id);
                    else
                        PlayerData.WowAccount = WowGuid128.Create(HighGuidType703.WowAccount, Guid.GetCounter());
                }
                PlayerData.VirtualPlayerRealm ??= GlobalSession.RealmId.GetAddress();
                PlayerData.HonorLevel ??= 1;
                if (PlayerData.AvgItemLevel[3] == null)
                    PlayerData.AvgItemLevel[3] = 1;
            }
            if (ActivePlayerData != null)
            {
                if (ActivePlayerData.RestInfo[0] == null)
                    ActivePlayerData.RestInfo[0] = new RestInfo();
                if (ActivePlayerData.RestInfo[0].Threshold == null)
                    ActivePlayerData.RestInfo[0].Threshold = 1;
                if (ActivePlayerData.RestInfo[0].StateID == null)
                    ActivePlayerData.RestInfo[0].StateID = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (ActivePlayerData.ModDamageDonePercent[i] == null)
                        ActivePlayerData.ModDamageDonePercent[i] = 1;
                }
                ActivePlayerData.ModHealingPercent ??= 1;
                ActivePlayerData.ModHealingDonePercent ??= 1;
                ActivePlayerData.ModPeriodicHealingDonePercent ??= 1;
                for (int i = 0; i < 3; i++)
                {
                    if (ActivePlayerData.WeaponDmgMultipliers[i] == null)
                        ActivePlayerData.WeaponDmgMultipliers[i] = 1;
                    if (ActivePlayerData.WeaponAtkSpeedMultipliers[i] == null)
                        ActivePlayerData.WeaponAtkSpeedMultipliers[i] = 1;
                }
                ActivePlayerData.ModSpellPowerPercent ??= 1;
                ActivePlayerData.NumBackpackSlots ??= 16;
                ActivePlayerData.MultiActionBars ??= 7;
                ActivePlayerData.MaxLevel ??= LegacyVersion.GetMaxLevel();
                ActivePlayerData.ModPetHaste ??= 1;
                ActivePlayerData.HonorNextLevel ??= 5500;
                ActivePlayerData.PvPTierMaxFromWins ??= 4294967295;
                ActivePlayerData.PvPLastWeeksTierMaxFromWins ??= 4294967295;
            }
        }
    }

    public class UpdateObject : ServerPacket
    {
        public UpdateObject(GameSessionData gameState) : base(Opcode.SMSG_UPDATE_OBJECT, ConnectionType.Instance)
        {
            _gameState = gameState;
        }

        public override void Write()
        {
            NumObjUpdates = (uint)ObjectUpdates.Count;
            MapID = (ushort)_gameState.CurrentMapId;

            _worldPacket.WriteUInt32(NumObjUpdates);
            _worldPacket.WriteUInt16(MapID);

            WorldPacket buffer = new();
            if (buffer.WriteBit(!OutOfRangeGuids.Empty() || !DestroyedGuids.Empty()))
            {
                buffer.WriteUInt16((ushort)DestroyedGuids.Count);
                buffer.WriteInt32(DestroyedGuids.Count + OutOfRangeGuids.Count);

                foreach (var destroyGuid in DestroyedGuids)
                    buffer.WritePackedGuid128(destroyGuid);

                foreach (var outOfRangeGuid in OutOfRangeGuids)
                    buffer.WritePackedGuid128(outOfRangeGuid);
            }

            WorldPacket data = new();
            foreach (var update in ObjectUpdates)
            {
                update.InitializePlaceholders();
                switch (ModernVersion.GetUpdateFieldsDefiningBuild())
                {
                    case ClientVersionBuild.V1_14_0_40237:
                    {
                        Objects.Version.V1_14_0_40237.ObjectUpdateBuilder builder = new(update, _gameState);
                        builder.WriteToPacket(data);
                        break;
                    }
                    case ClientVersionBuild.V1_14_1_40688:
                    {
                        Objects.Version.V1_14_1_40688.ObjectUpdateBuilder builder = new(update, _gameState);
                        builder.WriteToPacket(data);
                        break;
                    }
                    case ClientVersionBuild.V2_5_2_39570:
                    {
                        Objects.Version.V2_5_2_39570.ObjectUpdateBuilder builder = new(update, _gameState);
                        builder.WriteToPacket(data);
                        break;
                    }
                    default:
                        throw new System.ArgumentOutOfRangeException("No object update builder defined for current build.");
                }
            }

            var bytes = data.GetData();
            buffer.WriteInt32(bytes.Length);
            buffer.WriteBytes(bytes);
            Data = buffer.GetData();

            _worldPacket.WriteBytes(Data);
        }

        readonly GameSessionData _gameState;
        public uint NumObjUpdates;
        public ushort MapID;
        public byte[] Data;

        public List<WowGuid128> OutOfRangeGuids = new();
        public List<WowGuid128> DestroyedGuids = new();
        public List<ObjectUpdate> ObjectUpdates = new();
    }

    public class PowerUpdate : ServerPacket
    {
        public PowerUpdate(WowGuid128 guid) : base(Opcode.SMSG_POWER_UPDATE)
        {
            Guid = guid;
            Powers = new List<PowerUpdatePower>();
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteInt32(Powers.Count);
            foreach (var power in Powers)
            {
                _worldPacket.WriteInt32(power.Power);
                _worldPacket.WriteUInt8(power.PowerType);
            }
        }

        public WowGuid128 Guid;
        public List<PowerUpdatePower> Powers;
    }

    public struct PowerUpdatePower
    {
        public PowerUpdatePower(int power, byte powerType)
        {
            Power = power;
            PowerType = powerType;
        }

        public int Power;
        public byte PowerType;
    }
}
