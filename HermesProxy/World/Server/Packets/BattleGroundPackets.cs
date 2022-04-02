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
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class SeasonInfo : ServerPacket
    {
        public SeasonInfo() : base(Opcode.SMSG_SEASON_INFO) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(MythicPlusSeasonID);
            _worldPacket.WriteInt32(CurrentSeason);
            _worldPacket.WriteInt32(PreviousSeason);
            _worldPacket.WriteInt32(ConquestWeeklyProgressCurrencyID);
            _worldPacket.WriteInt32(PvpSeasonID);
            _worldPacket.WriteBit(WeeklyRewardChestsEnabled);
            _worldPacket.FlushBits();
        }

        public int MythicPlusSeasonID;
        public int PreviousSeason;
        public int CurrentSeason;
        public int PvpSeasonID;
        public int ConquestWeeklyProgressCurrencyID;
        public bool WeeklyRewardChestsEnabled;
    }

    class BattlefieldList : ServerPacket
    {
        public BattlefieldList() : base(Opcode.SMSG_BATTLEFIELD_LIST)
        {
            MinLevel = LegacyVersion.GetMaxLevel();
            MaxLevel = LegacyVersion.GetMaxLevel();
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(BattlemasterGuid);
            _worldPacket.WriteInt32(Verification);
            _worldPacket.WriteUInt32(BattlemasterListID);
            _worldPacket.WriteUInt8(MinLevel);
            _worldPacket.WriteUInt8(MaxLevel);
            _worldPacket.WriteInt32(BattlefieldInstances.Count);

            foreach (var field in BattlefieldInstances)
                _worldPacket.WriteInt32(field);

            _worldPacket.WriteBit(PvpAnywhere);
            _worldPacket.WriteBit(HasRandomWinToday);
            _worldPacket.FlushBits();
        }

        public WowGuid128 BattlemasterGuid;
        public int Verification;
        public uint BattlemasterListID;
        public byte MinLevel;
        public byte MaxLevel;
        public List<int> BattlefieldInstances = new();
        public bool PvpAnywhere;
        public bool HasRandomWinToday;
    }

    class BattlemasterJoin : ClientPacket
    {
        public BattlemasterJoin(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            long queueId = _worldPacket.ReadInt64();
            BattlefieldListId = (uint)(queueId & ~0x1F10000000000000);
            Roles = _worldPacket.ReadUInt8();
            BlacklistMap[0] = _worldPacket.ReadInt32();
            BlacklistMap[1] = _worldPacket.ReadInt32();
            BattlemasterGuid = _worldPacket.ReadPackedGuid128();
            Verification = _worldPacket.ReadInt32();
            BattlefieldInstanceID = _worldPacket.ReadInt32();
            JoinAsGroup = _worldPacket.HasBit();

        }

        public uint BattlefieldListId;
        public byte Roles;
        public int[] BlacklistMap = new int[2];
        public WowGuid128 BattlemasterGuid;
        public int Verification;
        public int BattlefieldInstanceID;
        public bool JoinAsGroup;
    }

    public class BattlefieldStatusNeedConfirmation : ServerPacket
    {
        public BattlefieldStatusNeedConfirmation() : base(Opcode.SMSG_BATTLEFIELD_STATUS_NEED_CONFIRMATION) { }

        public override void Write()
        {
            Hdr.Write(_worldPacket);
            _worldPacket.WriteUInt32(Mapid);
            _worldPacket.WriteUInt32(Timeout);
            _worldPacket.WriteUInt8(Role);
        }

        public BattlefieldStatusHeader Hdr = new();
        public uint Mapid;
        public uint Timeout;
        public byte Role;
    }

    public class BattlefieldStatusQueued : ServerPacket
    {
        public BattlefieldStatusQueued() : base(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED) { }

        public override void Write()
        {
            Hdr.Write(_worldPacket);
            _worldPacket.WriteUInt32(AverageWaitTime);
            _worldPacket.WriteUInt32(WaitTime);
            _worldPacket.WriteBit(AsGroup);
            _worldPacket.WriteBit(EligibleForMatchmaking);
            _worldPacket.FlushBits();
        }

        public BattlefieldStatusHeader Hdr = new();
        public uint AverageWaitTime;
        public uint WaitTime;
        public bool AsGroup;
        public bool EligibleForMatchmaking = true;
    }

    public class BattlefieldStatusFailed : ServerPacket
    {
        public BattlefieldStatusFailed() : base(Opcode.SMSG_BATTLEFIELD_STATUS_FAILED) { }

        public override void Write()
        {
            Ticket.Write(_worldPacket);

            ulong queueID = BattlefieldListId | 0x1F10000000000000;
            _worldPacket.WriteUInt64(queueID);
            _worldPacket.WriteInt32(Reason);
            _worldPacket.WritePackedGuid128(ClientID);
        }

        public RideTicket Ticket = new();
        public byte Unk;
        public ulong BattlefieldListId;
        public WowGuid128 ClientID = WowGuid128.Empty;
        public int Reason;
    }

    public class BattlefieldStatusHeader
    {
        public void Write(WorldPacket data)
        {
            Ticket.Write(data);
            data.WriteInt32(BattlefieldListIDs.Count);
            data.WriteUInt8(RangeMin);
            data.WriteUInt8(RangeMax);
            data.WriteUInt8(ArenaTeamSize);
            data.WriteUInt32(InstanceID);

            foreach (ulong bgId in BattlefieldListIDs)
            {
                ulong queueID = bgId | 0x1F10000000000000;
                data.WriteUInt64(queueID);
            }

            data.WriteBit(IsArena);
            data.WriteBit(TournamentRules);
            data.FlushBits();
        }

        public RideTicket Ticket = new();
        public List<uint> BattlefieldListIDs = new();
        public byte RangeMin;
        public byte RangeMax = 70;
        public byte ArenaTeamSize;
        public uint InstanceID;
        public bool IsArena;
        public bool TournamentRules;
    }

    public class RideTicket
    {
        public void Read(WorldPacket data)
        {
            RequesterGuid = data.ReadPackedGuid128();
            Id = data.ReadUInt32();
            Type = (RideType)data.ReadUInt32();
            Time = data.ReadInt64();
        }

        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(RequesterGuid);
            data.WriteUInt32(Id);
            data.WriteUInt32((uint)Type);
            data.WriteInt64(Time);
        }

        public WowGuid128 RequesterGuid = WowGuid128.Empty;
        public uint Id;
        public RideType Type;
        public long Time;
    }

    public enum RideType
    {
        None = 0,
        Battlegrounds = 1,
        Lfg = 2
    }

    class BattlefieldPort : ClientPacket
    {
        public BattlefieldPort(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Ticket.Read(_worldPacket);
            AcceptedInvite = _worldPacket.HasBit();
        }

        public RideTicket Ticket = new();
        public bool AcceptedInvite;
    }

    public class BattlefieldStatusActive : ServerPacket
    {
        public BattlefieldStatusActive() : base(Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE) { }

        public override void Write()
        {
            Hdr.Write(_worldPacket);
            _worldPacket.WriteUInt32(Mapid);
            _worldPacket.WriteUInt32(ShutdownTimer);
            _worldPacket.WriteUInt32(StartTimer);
            _worldPacket.WriteBit(ArenaFaction != 0);
            _worldPacket.WriteBit(LeftEarly);
            _worldPacket.FlushBits();
        }

        public BattlefieldStatusHeader Hdr = new();
        public uint Mapid;
        public uint ShutdownTimer;
        public uint StartTimer;
        public byte ArenaFaction;
        public bool LeftEarly;
    }

    public class BattlegroundInit : ServerPacket
    {
        public BattlegroundInit() : base(Opcode.SMSG_BATTLEGROUND_INIT) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Milliseconds);
            _worldPacket.WriteUInt16(BattlegroundPoints);
        }

        public uint Milliseconds;
        public ushort BattlegroundPoints;
    }

    class RequestBattlefieldStatus : ClientPacket
    {
        public RequestBattlefieldStatus(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class PVPLogDataRequest : ClientPacket
    {
        public PVPLogDataRequest(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class PVPMatchStatisticsMessage : ServerPacket
    {
        public PVPMatchStatisticsMessage() : base(Opcode.SMSG_PVP_MATCH_STATISTICS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Ratings != null);
            _worldPacket.WriteBit(ArenaTeams != null);
            _worldPacket.WriteBit(Winner != null);
            _worldPacket.WriteInt32(Statistics.Count);

            foreach (var count in PlayerCount)
                _worldPacket.WriteInt8(count);

            if (Ratings != null)
                Ratings.Write(_worldPacket);

            if (Winner != null)
                _worldPacket.WriteUInt8((byte)Winner);

            foreach (var player in Statistics)
                player.Write(_worldPacket);
        }

        public RatingData Ratings;
        public ArenaTeamsInfo ArenaTeams;
        public byte? Winner;
        public List<PVPMatchPlayerStatistics> Statistics = new();
        public sbyte[] PlayerCount = new sbyte[2];

        public class ArenaTeamsInfo
        {
            public void Write(WorldPacket data)
            {
                foreach (var str in Names)
                    data.WriteBits(str.GetByteCount(), 7);
                data.FlushBits();

                for (int i = 0; i < 2; i++)
                {
                    data.WritePackedGuid128(Guids[i]);
                    data.WriteString(Names[i]);
                }
            }

            public WowGuid128[] Guids = new WowGuid128[2];
            public string[] Names = new string[2];
        }

        public class RatingData
        {
            public void Write(WorldPacket data)
            {
                foreach (var id in Prematch)
                    data.WriteUInt32(id);

                foreach (var id in Postmatch)
                    data.WriteUInt32(id);

                foreach (var id in PrematchMMR)
                    data.WriteUInt32(id);
            }

            public uint[] Prematch = new uint[2];
            public uint[] Postmatch = new uint[2];
            public uint[] PrematchMMR = new uint[2];
        }

        public class HonorData
        {
            public void Write(WorldPacket data)
            {
                data.WriteUInt32(HonorKills);
                data.WriteUInt32(Deaths);
                data.WriteUInt32(ContributionPoints);
            }

            public uint HonorKills;
            public uint Deaths;
            public uint ContributionPoints;
        }

        public class PVPMatchPlayerStatistics
        {
            public void Write(WorldPacket data)
            {
                data.WritePackedGuid128(PlayerGUID);
                data.WriteUInt32(Kills);
                data.WriteUInt32(DamageDone);
                data.WriteUInt32(HealingDone);
                data.WriteInt32(Stats.Count);
                data.WriteInt32(PrimaryTalentTree);
                data.WriteUInt32((uint)Sex);
                data.WriteUInt32((uint)PlayerRace);
                data.WriteUInt32((uint)PlayerClass);
                data.WriteInt32(CreatureID);
                data.WriteInt32(HonorLevel);
                data.WriteInt32(Role);

                foreach (var pvpStat in Stats)
                    data.WriteUInt32(pvpStat);

                data.WriteBit(Faction);
                data.WriteBit(IsInWorld);
                data.WriteBit(Honor != null);
                data.WriteBit(PreMatchRating.HasValue);
                data.WriteBit(RatingChange.HasValue);
                data.WriteBit(PreMatchMMR.HasValue);
                data.WriteBit(MmrChange.HasValue);
                data.FlushBits();

                if (Honor != null)
                    Honor.Write(data);

                if (PreMatchRating.HasValue)
                    data.WriteUInt32(PreMatchRating.Value);

                if (RatingChange.HasValue)
                    data.WriteInt32(RatingChange.Value);

                if (PreMatchMMR.HasValue)
                    data.WriteUInt32(PreMatchMMR.Value);

                if (MmrChange.HasValue)
                    data.WriteInt32(MmrChange.Value);
            }

            public WowGuid128 PlayerGUID;
            public uint Kills;
            public bool Faction;
            public bool IsInWorld = true;
            public HonorData Honor;
            public uint DamageDone;
            public uint HealingDone;
            public uint? PreMatchRating;
            public int? RatingChange;
            public uint? PreMatchMMR;
            public int? MmrChange;
            public List<uint> Stats = new();
            public int PrimaryTalentTree;
            public Gender Sex;
            public Race PlayerRace;
            public Class PlayerClass;
            public int CreatureID;
            public int HonorLevel = 1;
            public int Role;
        }
    }

    class BattlefieldLeave : ClientPacket
    {
        public BattlefieldLeave(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class BattlegroundPlayerPositions : ServerPacket
    {
        public BattlegroundPlayerPositions() : base(Opcode.SMSG_BATTLEGROUND_PLAYER_POSITIONS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(FlagCarriers.Count);
            foreach (var pos in FlagCarriers)
                pos.Write(_worldPacket);
        }

        public List<BattlegroundPlayerPosition> FlagCarriers = new();
    }

    public struct BattlegroundPlayerPosition
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(Guid);
            data.WriteVector2(Pos);
            data.WriteInt8(IconID);
            data.WriteInt8(ArenaSlot);
        }

        public WowGuid128 Guid;
        public Vector2 Pos;
        public sbyte IconID;
        public sbyte ArenaSlot;
    }

    class BattlegroundPlayerLeftOrJoined : ServerPacket
    {
        public BattlegroundPlayerLeftOrJoined(Opcode opcode) : base(opcode, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
        }

        public WowGuid128 Guid;
    }

    public class AreaSpiritHealerTime : ServerPacket
    {
        public AreaSpiritHealerTime() : base(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(HealerGuid);
            _worldPacket.WriteUInt32(TimeLeft);
        }

        public WowGuid128 HealerGuid;
        public uint TimeLeft;
    }

    class PvPCredit : ServerPacket
    {
        public PvPCredit() : base(Opcode.SMSG_PVP_CREDIT) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(OriginalHonor);
            _worldPacket.WriteInt32(Honor);
            _worldPacket.WritePackedGuid128(Target);
            _worldPacket.WriteUInt32(Rank);
        }

        public int OriginalHonor;
        public int Honor;
        public WowGuid128 Target;
        public uint Rank;
    }

    class PlayerSkinned : ServerPacket
    {
        public PlayerSkinned() : base(Opcode.SMSG_PLAYER_SKINNED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBit(FreeRepop);
            _worldPacket.FlushBits();
        }

        public bool FreeRepop;
    }
}
