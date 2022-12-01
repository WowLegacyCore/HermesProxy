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
using Framework.GameMath;
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
                if (GameObjectData.StateAnimID == null)
                    GameObjectData.StateAnimID = ModernVersion.GetGameObjectStateAnimId();
                if (Guid.GetHighType() == HighGuidType.Transport)
                {
                    uint period = GameData.GetTransportPeriod((uint)ObjectData.EntryID);
                    if (period != 0)
                    {
                        if (GameObjectData.Level == null)
                            GameObjectData.Level = (int)period;
                        if (ObjectData.DynamicFlags == null)
                            ObjectData.DynamicFlags = (((uint)(((float)(CreateData.MoveInfo.TransportPathTimer % period) / (float)period) * System.UInt16.MaxValue)) << 16);
                        GameObjectData.Flags = 1048616;
                    }
                    else if (ObjectData.DynamicFlags == null)
                        ObjectData.DynamicFlags = ((CreateData.MoveInfo.TransportPathTimer % System.UInt16.MaxValue) << 16);
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
                if (CorpseData.FactionTemplate == null && CorpseData.Owner != null)
                {
                    int ownerFaction = GlobalSession.GameState.GetLegacyFieldValueInt32(CorpseData.Owner, UnitField.UNIT_FIELD_FACTIONTEMPLATE);
                    if (ownerFaction != 0)
                        CorpseData.FactionTemplate = ownerFaction;
                }
            }
            if (UnitData != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (UnitData.ModPowerRegen[i] == null)
                        UnitData.ModPowerRegen[i] = 1;
                }
                if (UnitData.Flags2 == null)
                    UnitData.Flags2 = 2048;
                if (UnitData.DisplayScale == null)
                    UnitData.DisplayScale = 1;
                if (UnitData.NativeXDisplayScale == null)
                    UnitData.NativeXDisplayScale = 1;
                if (UnitData.ModCastHaste == null)
                    UnitData.ModCastHaste = 1;
                if (UnitData.ModHaste == null)
                    UnitData.ModHaste = 1;
                if (UnitData.ModRangedHaste == null)
                    UnitData.ModRangedHaste = 1;
                if (UnitData.ModHasteRegen == null)
                    UnitData.ModHasteRegen = 1;
                if (UnitData.ModTimeRate == null)
                    UnitData.ModTimeRate = 1;
                if (UnitData.HoverHeight == null)
                    UnitData.HoverHeight = 1;
                if (UnitData.ScaleDuration == null)
                    UnitData.ScaleDuration = 100;
                if (UnitData.LookAtControllerID == null)
                    UnitData.LookAtControllerID = -1;
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
                if (PlayerData.VirtualPlayerRealm == null)
                    PlayerData.VirtualPlayerRealm = GlobalSession.RealmId.GetAddress();
                if (PlayerData.HonorLevel == null)
                    PlayerData.HonorLevel = 1;
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
                if (ActivePlayerData.ModHealingPercent == null)
                    ActivePlayerData.ModHealingPercent = 1;
                if (ActivePlayerData.ModHealingDonePercent == null)
                    ActivePlayerData.ModHealingDonePercent = 1;
                if (ActivePlayerData.ModPeriodicHealingDonePercent == null)
                    ActivePlayerData.ModPeriodicHealingDonePercent = 1;
                for (int i = 0; i < 3; i++)
                {
                    if (ActivePlayerData.WeaponDmgMultipliers[i] == null)
                        ActivePlayerData.WeaponDmgMultipliers[i] = 1;
                    if (ActivePlayerData.WeaponAtkSpeedMultipliers[i] == null)
                        ActivePlayerData.WeaponAtkSpeedMultipliers[i] = 1;
                }
                if (ActivePlayerData.ModSpellPowerPercent == null)
                    ActivePlayerData.ModSpellPowerPercent = 1;
                if (ActivePlayerData.NumBackpackSlots == null)
                    ActivePlayerData.NumBackpackSlots = 16;
                if (ActivePlayerData.MultiActionBars == null)
                    ActivePlayerData.MultiActionBars = 7;
                if (ActivePlayerData.MaxLevel == null)
                    ActivePlayerData.MaxLevel = LegacyVersion.GetMaxLevel();
                if (ActivePlayerData.ModPetHaste == null)
                    ActivePlayerData.ModPetHaste = 1;
                if (ActivePlayerData.HonorNextLevel == null)
                    ActivePlayerData.HonorNextLevel = 5500;
                if (ActivePlayerData.PvPTierMaxFromWins == null)
                    ActivePlayerData.PvPTierMaxFromWins = 4294967295;
                if (ActivePlayerData.PvPLastWeeksTierMaxFromWins == null)
                    ActivePlayerData.PvPLastWeeksTierMaxFromWins = 4294967295;
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
                        Objects.Version.V1_14_0_40237.ObjectUpdateBuilder builder = new Objects.Version.V1_14_0_40237.ObjectUpdateBuilder(update, _gameState);
                        builder.WriteToPacket(data);
                        break;
                    }
                    case ClientVersionBuild.V1_14_1_40688:
                    {
                        Objects.Version.V1_14_1_40688.ObjectUpdateBuilder builder = new Objects.Version.V1_14_1_40688.ObjectUpdateBuilder(update, _gameState);
                        builder.WriteToPacket(data);
                        break;
                    }
                    case ClientVersionBuild.V2_5_2_39570:
                    {
                        Objects.Version.V2_5_2_39570.ObjectUpdateBuilder builder = new Objects.Version.V2_5_2_39570.ObjectUpdateBuilder(update, _gameState);
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

        GameSessionData _gameState;
        public uint NumObjUpdates;
        public ushort MapID;
        public byte[] Data;

        public List<WowGuid128> OutOfRangeGuids = new List<WowGuid128>();
        public List<WowGuid128> DestroyedGuids = new List<WowGuid128>();
        public List<ObjectUpdate> ObjectUpdates = new List<ObjectUpdate>();
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

    public class ObjectUpdateFailed : ClientPacket
    {
        public ObjectUpdateFailed(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            ObjectGuid = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 ObjectGuid;
    }
}
