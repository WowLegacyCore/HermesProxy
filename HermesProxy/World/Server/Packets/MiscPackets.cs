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
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class BindPointUpdate : ServerPacket
    {
        public BindPointUpdate() : base(Opcode.SMSG_BIND_POINT_UPDATE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteVector3(BindPosition);
            _worldPacket.WriteUInt32(BindMapID);
            _worldPacket.WriteUInt32(BindAreaID);
        }

        public uint BindMapID = 0xFFFFFFFF;
        public Vector3 BindPosition;
        public uint BindAreaID;
    }

    public class PlayerBound : ServerPacket
    {
        public PlayerBound() : base(Opcode.SMSG_PLAYER_BOUND) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(BinderGUID);
            _worldPacket.WriteUInt32(AreaID);
        }

        public WowGuid128 BinderGUID;
        public uint AreaID;
    }

    public class ServerTimeOffset : ServerPacket
    {
        public ServerTimeOffset() : base(Opcode.SMSG_SERVER_TIME_OFFSET) { }

        public override void Write()
        {
            _worldPacket.WriteInt64(Time);
        }

        public long Time;
    }

    public class TutorialFlags : ServerPacket
    {
        public TutorialFlags() : base(Opcode.SMSG_TUTORIAL_FLAGS) { }

        public override void Write()
        {
            for (byte i = 0; i < (int)Tutorials.Max; ++i)
                _worldPacket.WriteUInt32(TutorialData[i]);
        }

        public uint[] TutorialData = new uint[(int)Tutorials.Max];
    }

    public class CorpseReclaimDelay : ServerPacket
    {
        public CorpseReclaimDelay() : base(Opcode.SMSG_CORPSE_RECLAIM_DELAY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Remaining);
        }

        public uint Remaining;
    }

    public class SetupCurrency : ServerPacket
    {
        public SetupCurrency() : base(Opcode.SMSG_SETUP_CURRENCY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Data.Count);

            foreach (Record data in Data)
            {
                _worldPacket.WriteUInt32(data.Type);
                _worldPacket.WriteUInt32(data.Quantity);

                _worldPacket.WriteBit(data.WeeklyQuantity.HasValue);
                _worldPacket.WriteBit(data.MaxWeeklyQuantity.HasValue);
                _worldPacket.WriteBit(data.TrackedQuantity.HasValue);
                _worldPacket.WriteBit(data.MaxQuantity.HasValue);
                _worldPacket.WriteBit(data.Unused901.HasValue);
                _worldPacket.WriteBits(data.Flags, 5);
                _worldPacket.FlushBits();

                if (data.WeeklyQuantity.HasValue)
                    _worldPacket.WriteUInt32(data.WeeklyQuantity.Value);
                if (data.MaxWeeklyQuantity.HasValue)
                    _worldPacket.WriteUInt32(data.MaxWeeklyQuantity.Value);
                if (data.TrackedQuantity.HasValue)
                    _worldPacket.WriteUInt32(data.TrackedQuantity.Value);
                if (data.MaxQuantity.HasValue)
                    _worldPacket.WriteInt32(data.MaxQuantity.Value);
                if (data.Unused901.HasValue)
                    _worldPacket.WriteInt32(data.Unused901.Value);
            }
        }

        public List<Record> Data = new();

        public struct Record
        {
            public uint Type;
            public uint Quantity;
            public uint? WeeklyQuantity;       // Currency count obtained this Week.  
            public uint? MaxWeeklyQuantity;    // Weekly Currency cap.
            public uint? TrackedQuantity;
            public int? MaxQuantity;
            public int? Unused901;
            public byte Flags;                      // 0 = none, 
        }
    }

    class AllAccountCriteria : ServerPacket
    {
        public AllAccountCriteria() : base(Opcode.SMSG_ALL_ACCOUNT_CRITERIA, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Progress.Count);
            foreach (var progress in Progress)
                progress.Write(_worldPacket);
        }

        public List<CriteriaProgressPkt> Progress = new();
    }

    public struct CriteriaProgressPkt
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Id);
            data.WriteUInt64(Quantity);
            data.WritePackedGuid128(Player);
            data.WritePackedTime(Date);
            data.WriteUInt32(TimeFromStart);
            data.WriteUInt32(TimeFromCreate);
            data.WriteBits(Flags, 4);
            data.WriteBit(RafAcceptanceID.HasValue);
            data.FlushBits();

            if (RafAcceptanceID.HasValue)
                data.WriteUInt64(RafAcceptanceID.Value);
        }

        public uint Id;
        public ulong Quantity;
        public WowGuid128 Player;
        public uint Flags;
        public long Date;
        public uint TimeFromStart;
        public uint TimeFromCreate;
        public ulong? RafAcceptanceID;
    }

    public class TimeSyncRequest : ServerPacket
    {
        public TimeSyncRequest() : base(Opcode.SMSG_TIME_SYNC_REQUEST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(SequenceIndex);
        }

        public uint SequenceIndex;
    }

    public class TimeSyncResponse : ClientPacket
    {
        public TimeSyncResponse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SequenceIndex = _worldPacket.ReadUInt32();
            ClientTime = _worldPacket.ReadUInt32();
        }

        public uint ClientTime; // Client ticks in ms
        public uint SequenceIndex; // Same index as in request
    }

    public class WeatherPkt : ServerPacket
    {
        public WeatherPkt() : base(Opcode.SMSG_WEATHER, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32((uint)WeatherID);
            _worldPacket.WriteFloat(Intensity);
            _worldPacket.WriteBit(Abrupt);

            _worldPacket.FlushBits();
        }

        public bool Abrupt;
        public float Intensity;
        public WeatherState WeatherID;
    }

    class StartLightningStorm : ServerPacket
    {
        public StartLightningStorm() : base(Opcode.SMSG_START_LIGHTNING_STORM, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(LightningStormId);
        }

        public uint LightningStormId;
    }

    public class LoginSetTimeSpeed : ServerPacket
    {
        public LoginSetTimeSpeed() : base(Opcode.SMSG_LOGIN_SET_TIME_SPEED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(ServerTime);
            _worldPacket.WriteUInt32(GameTime);
            _worldPacket.WriteFloat(NewSpeed);
            _worldPacket.WriteInt32(ServerTimeHolidayOffset);
            _worldPacket.WriteInt32(GameTimeHolidayOffset);
        }

        public uint ServerTime;
        public uint GameTime;
        public float NewSpeed;
        public int ServerTimeHolidayOffset;
        public int GameTimeHolidayOffset;
    }

    class AreaTriggerPkt : ClientPacket
    {
        public AreaTriggerPkt(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            AreaTriggerID = _worldPacket.ReadUInt32();
            Entered = _worldPacket.HasBit();
            FromClient = _worldPacket.HasBit();
        }

        public uint AreaTriggerID;
        public bool Entered;
        public bool FromClient;
    }

    class AreaTriggerMessage : ServerPacket
    {
        public AreaTriggerMessage() : base(Opcode.SMSG_AREA_TRIGGER_MESSAGE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(AreaTriggerID);
        }

        public uint AreaTriggerID = 0;
    }

    public class SetSelection : ClientPacket
    {
        public SetSelection(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TargetGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 TargetGUID;
    }

    public class WorldServerInfo : ServerPacket
    {
        public WorldServerInfo() : base(Opcode.SMSG_WORLD_SERVER_INFO, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(DifficultyID);
            _worldPacket.WriteUInt8(IsTournamentRealm);
            _worldPacket.WriteBit(XRealmPvpAlert);
            _worldPacket.WriteBit(RestrictedAccountMaxLevel.HasValue);
            _worldPacket.WriteBit(RestrictedAccountMaxMoney.HasValue);
            _worldPacket.WriteBit(InstanceGroupSize.HasValue);
            _worldPacket.FlushBits();

            if (RestrictedAccountMaxLevel.HasValue)
                _worldPacket.WriteUInt32(RestrictedAccountMaxLevel.Value);

            if (RestrictedAccountMaxMoney.HasValue)
                _worldPacket.WriteUInt64(RestrictedAccountMaxMoney.Value);

            if (InstanceGroupSize.HasValue)
                _worldPacket.WriteUInt32(InstanceGroupSize.Value);
        }

        public uint DifficultyID;
        public byte IsTournamentRealm;
        public bool XRealmPvpAlert;
        public uint? RestrictedAccountMaxLevel;
        public ulong? RestrictedAccountMaxMoney;
        public uint? InstanceGroupSize;
    }
}
