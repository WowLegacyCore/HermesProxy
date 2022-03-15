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
    public class GuildCommandResult : ServerPacket
    {
        public GuildCommandResult() : base(Opcode.SMSG_GUILD_COMMAND_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32((uint)Result);
            _worldPacket.WriteUInt32((uint)Command);

            _worldPacket.WriteBits(Name.GetByteCount(), 8);
            _worldPacket.WriteString(Name);
        }

        public string Name;
        public GuildCommandError Result;
        public GuildCommandType Command;
    }

    public class QueryGuildInfo : ClientPacket
    {
        public QueryGuildInfo(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            GuildGuid = _worldPacket.ReadPackedGuid128();
            PlayerGuid = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 GuildGuid;
        public WowGuid128 PlayerGuid;
    }

    public class QueryGuildInfoResponse : ServerPacket
    {
        public QueryGuildInfoResponse() : base(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(GuildGUID);
            _worldPacket.WritePackedGuid128(PlayerGuid);
            _worldPacket.WriteBit(HasGuildInfo);
            _worldPacket.FlushBits();

            if (HasGuildInfo)
            {
                _worldPacket.WritePackedGuid128(Info.GuildGuid);
                _worldPacket.WriteUInt32(Info.VirtualRealmAddress);
                _worldPacket.WriteInt32(Info.Ranks.Count);
                _worldPacket.WriteUInt32(Info.EmblemStyle);
                _worldPacket.WriteUInt32(Info.EmblemColor);
                _worldPacket.WriteUInt32(Info.BorderStyle);
                _worldPacket.WriteUInt32(Info.BorderColor);
                _worldPacket.WriteUInt32(Info.BackgroundColor);
                _worldPacket.WriteBits(Info.GuildName.GetByteCount(), 7);
                _worldPacket.FlushBits();

                foreach (var rank in Info.Ranks)
                {
                    _worldPacket.WriteUInt32(rank.RankID);
                    _worldPacket.WriteUInt32(rank.RankOrder);

                    _worldPacket.WriteBits(rank.RankName.GetByteCount(), 7);
                    _worldPacket.WriteString(rank.RankName);
                }

                _worldPacket.WriteString(Info.GuildName);
            }

        }

        public WowGuid128 GuildGUID;
        public WowGuid128 PlayerGuid;
        public GuildInfo Info = new();
        public bool HasGuildInfo;

        public class GuildInfo
        {
            public WowGuid128 GuildGuid;

            public uint VirtualRealmAddress; // a special identifier made from the Index, BattleGroup and Region.

            public uint EmblemStyle;
            public uint EmblemColor;
            public uint BorderStyle;
            public uint BorderColor;
            public uint BackgroundColor;
            public List<RankInfo> Ranks = new();
            public string GuildName = "";

            public struct RankInfo
            {
                public RankInfo(uint id, uint order, string name)
                {
                    RankID = id;
                    RankOrder = order;
                    RankName = name;
                }

                public uint RankID;
                public uint RankOrder;
                public string RankName;
            }
        }
    }

    public class GuildPermissionsQuery : ClientPacket
    {
        public GuildPermissionsQuery(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class GuildBankRemainingWithdrawMoneyQuery : ClientPacket
    {
        public GuildBankRemainingWithdrawMoneyQuery(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class GuildGetRoster : ClientPacket
    {
        public GuildGetRoster(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class GuildRoster : ServerPacket
    {
        public GuildRoster() : base(Opcode.SMSG_GUILD_ROSTER) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(NumAccounts);
            _worldPacket.WritePackedTime(CreateDate);
            _worldPacket.WriteInt32(GuildFlags);
            _worldPacket.WriteInt32(MemberData.Count);
            _worldPacket.WriteBits(WelcomeText.GetByteCount(), 11);
            _worldPacket.WriteBits(InfoText.GetByteCount(), 11);
            _worldPacket.FlushBits();

            MemberData.ForEach(p => p.Write(_worldPacket));

            _worldPacket.WriteString(WelcomeText);
            _worldPacket.WriteString(InfoText);
        }

        public List<GuildRosterMemberData> MemberData = new List<GuildRosterMemberData>();
        public string WelcomeText;
        public string InfoText;
        public uint CreateDate;
        public uint NumAccounts;
        public int GuildFlags = 2;
    }

    public class GuildRosterMemberData
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(Guid);
            data.WriteInt32(RankID);
            data.WriteInt32(AreaID);
            data.WriteInt32(PersonalAchievementPoints);
            data.WriteInt32(GuildReputation);
            data.WriteFloat(LastSave);

            for (byte i = 0; i < 2; i++)
                Profession[i].Write(data);

            data.WriteUInt32(VirtualRealmAddress);
            data.WriteUInt8(Status);
            data.WriteUInt8(Level);
            data.WriteUInt8((byte)ClassID);
            data.WriteUInt8((byte)SexID);

            data.WriteBits(Name.GetByteCount(), 6);
            data.WriteBits(Note.GetByteCount(), 8);
            data.WriteBits(OfficerNote.GetByteCount(), 8);
            data.WriteBit(Authenticated);
            data.WriteBit(SorEligible);

            data.WriteString(Name);
            data.WriteString(Note);
            data.WriteString(OfficerNote);
        }

        public WowGuid128 Guid;
        public long WeeklyXP;
        public long TotalXP;
        public int RankID;
        public int AreaID;
        public int PersonalAchievementPoints = -1;
        public int GuildReputation = -1;
        public int GuildRepToCap;
        public float LastSave;
        public string Name;
        public uint VirtualRealmAddress;
        public string Note;
        public string OfficerNote;
        public byte Status;
        public byte Level;
        public Class ClassID;
        public Gender SexID;
        public bool Authenticated;
        public bool SorEligible;
        public GuildRosterProfessionData[] Profession = new GuildRosterProfessionData[2];
    }

    public struct GuildRosterProfessionData
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(DbID);
            data.WriteInt32(Rank);
            data.WriteInt32(Step);
        }

        public int DbID;
        public int Rank;
        public int Step;
    }

    public class GuildGetRanks : ClientPacket
    {
        public GuildGetRanks(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            GuildGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 GuildGUID;
    }

    public class GuildRanks : ServerPacket
    {
        public GuildRanks() : base(Opcode.SMSG_GUILD_RANKS) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Ranks.Count);

            Ranks.ForEach(p => p.Write(_worldPacket));
        }

        public List<GuildRankData> Ranks = new List<GuildRankData>();
    }

    public class GuildRankData
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(RankID);
            data.WriteUInt32(RankOrder);
            data.WriteUInt32(Flags);
            data.WriteInt32(WithdrawGoldLimit);

            for (byte i = 0; i < GuildConst.MaxBankTabs; i++)
            {
                data.WriteUInt32(TabFlags[i]);
                data.WriteUInt32(TabWithdrawItemLimit[i]);
            }

            data.WriteBits(RankName.GetByteCount(), 7);
            data.WriteString(RankName);
        }

        public byte RankID;
        public uint RankOrder;
        public uint Flags;
        public int WithdrawGoldLimit;
        public string RankName;
        public uint[] TabFlags = new uint[GuildConst.MaxBankTabs];
        public uint[] TabWithdrawItemLimit = new uint[GuildConst.MaxBankTabs];
    }

    public class GuildSendRankChange : ServerPacket
    {
        public GuildSendRankChange() : base(Opcode.SMSG_GUILD_SEND_RANK_CHANGE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Officer);
            _worldPacket.WritePackedGuid128(Other);
            _worldPacket.WriteUInt32(RankID);

            _worldPacket.WriteBit(Promote);
            _worldPacket.FlushBits();
        }

        public WowGuid128 Other;
        public WowGuid128 Officer;
        public bool Promote;
        public uint RankID;
    }

    public class GuildEventMotd : ServerPacket
    {
        public GuildEventMotd() : base(Opcode.SMSG_GUILD_EVENT_MOTD) { }

        public override void Write()
        {
            _worldPacket.WriteBits(MotdText.GetByteCount(), 11);
            _worldPacket.WriteString(MotdText);
        }

        public string MotdText;
    }

    public class GuildEventPlayerJoined : ServerPacket
    {
        public GuildEventPlayerJoined() : base(Opcode.SMSG_GUILD_EVENT_PLAYER_JOINED) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteUInt32(VirtualRealmAddress);

            _worldPacket.WriteBits(Name.GetByteCount(), 6);
            _worldPacket.WriteString(Name);
        }

        public WowGuid128 Guid;
        public uint VirtualRealmAddress;
        public string Name;
    }

    public class GuildEventPlayerLeft : ServerPacket
    {
        public GuildEventPlayerLeft() : base(Opcode.SMSG_GUILD_EVENT_PLAYER_LEFT) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Removed);
            _worldPacket.WriteBits(LeaverName.GetByteCount(), 6);

            if (Removed)
            {
                _worldPacket.WriteBits(RemoverName.GetByteCount(), 6);
                _worldPacket.WritePackedGuid128(RemoverGUID);
                _worldPacket.WriteUInt32(RemoverVirtualRealmAddress);
                _worldPacket.WriteString(RemoverName);
            }

            _worldPacket.WritePackedGuid128(LeaverGUID);
            _worldPacket.WriteUInt32(LeaverVirtualRealmAddress);
            _worldPacket.WriteString(LeaverName);
        }

        public bool Removed;
        public WowGuid128 RemoverGUID;
        public uint RemoverVirtualRealmAddress;
        public string RemoverName;
        public WowGuid128 LeaverGUID;
        public uint LeaverVirtualRealmAddress;
        public string LeaverName;
    }

    public class GuildEventNewLeader : ServerPacket
    {
        public GuildEventNewLeader() : base(Opcode.SMSG_GUILD_EVENT_NEW_LEADER) { }

        public override void Write()
        {
            _worldPacket.WriteBit(SelfPromoted);
            _worldPacket.WriteBits(OldLeaderName.GetByteCount(), 6);
            _worldPacket.WriteBits(NewLeaderName.GetByteCount(), 6);

            _worldPacket.WritePackedGuid128(OldLeaderGUID);
            _worldPacket.WriteUInt32(OldLeaderVirtualRealmAddress);
            _worldPacket.WritePackedGuid128(NewLeaderGUID);
            _worldPacket.WriteUInt32(NewLeaderVirtualRealmAddress);

            _worldPacket.WriteString(OldLeaderName);
            _worldPacket.WriteString(NewLeaderName);
        }

        public bool SelfPromoted;
        public WowGuid128 NewLeaderGUID;
        public uint NewLeaderVirtualRealmAddress;
        public string NewLeaderName;
        public WowGuid128 OldLeaderGUID;
        public uint OldLeaderVirtualRealmAddress;
        public string OldLeaderName;
    }

    public class GuildEventDisbanded : ServerPacket
    {
        public GuildEventDisbanded() : base(Opcode.SMSG_GUILD_EVENT_DISBANDED) { }

        public override void Write() { }
    }

    public class GuildEventRanksUpdated : ServerPacket
    {
        public GuildEventRanksUpdated() : base(Opcode.SMSG_GUILD_EVENT_RANKS_UPDATED) { }

        public override void Write() { }
    }

    public class GuildEventPresenceChange : ServerPacket
    {
        public GuildEventPresenceChange() : base(Opcode.SMSG_GUILD_EVENT_PRESENCE_CHANGE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteUInt32(VirtualRealmAddress);

            _worldPacket.WriteBits(Name.GetByteCount(), 6);
            _worldPacket.WriteBit(LoggedOn);
            _worldPacket.WriteBit(Mobile);

            _worldPacket.WriteString(Name);
        }

        public WowGuid128 Guid;
        public uint VirtualRealmAddress;
        public bool LoggedOn;
        public bool Mobile;
        public string Name;
    }

    public class GuildEventTabAdded : ServerPacket
    {
        public GuildEventTabAdded() : base(Opcode.SMSG_GUILD_EVENT_TAB_ADDED) { }

        public override void Write() { }
    }

    public class GuildEventTabModified : ServerPacket
    {
        public GuildEventTabModified() : base(Opcode.SMSG_GUILD_EVENT_TAB_MODIFIED) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Tab);

            _worldPacket.WriteBits(Name.GetByteCount(), 7);
            _worldPacket.WriteBits(Icon.GetByteCount(), 9);
            _worldPacket.FlushBits();

            _worldPacket.WriteString(Name);
            _worldPacket.WriteString(Icon);
        }

        public int Tab;
        public string Name;
        public string Icon;
    }

    public class GuildEventBankMoneyChanged : ServerPacket
    {
        public GuildEventBankMoneyChanged() : base(Opcode.SMSG_GUILD_EVENT_BANK_MONEY_CHANGED) { }

        public override void Write()
        {
            _worldPacket.WriteUInt64(Money);
        }

        public ulong Money;
    }

    public class GuildEventTabTextChanged : ServerPacket
    {
        public GuildEventTabTextChanged() : base(Opcode.SMSG_GUILD_EVENT_TAB_TEXT_CHANGED) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Tab);
        }

        public int Tab;
    }

    public class GuildUpdateMotdText : ClientPacket
    {
        public GuildUpdateMotdText(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint textLen = _worldPacket.ReadBits<uint>(11);
            MotdText = _worldPacket.ReadString(textLen);
        }

        public string MotdText;
    }

    public class GuildUpdateInfoText : ClientPacket
    {
        public GuildUpdateInfoText(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint textLen = _worldPacket.ReadBits<uint>(11);
            InfoText = _worldPacket.ReadString(textLen);
        }

        public string InfoText;
    }

    public class GuildSetMemberNote : ClientPacket
    {
        public GuildSetMemberNote(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            NoteeGUID = _worldPacket.ReadPackedGuid128();

            uint noteLen = _worldPacket.ReadBits<uint>(8);
            IsPublic = _worldPacket.HasBit();

            Note = _worldPacket.ReadString(noteLen);
        }

        public WowGuid128 NoteeGUID;
        public bool IsPublic;          // 0 == Officer, 1 == Public
        public string Note;
    }

    public class GuildPromoteMember : ClientPacket
    {
        public GuildPromoteMember(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Promotee = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Promotee;
    }

    public class GuildDemoteMember : ClientPacket
    {
        public GuildDemoteMember(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Demotee = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Demotee;
    }

    public class GuildOfficerRemoveMember : ClientPacket
    {
        public GuildOfficerRemoveMember(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Removee = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Removee;
    }

    public class GuildInviteByName : ClientPacket
    {
        public GuildInviteByName(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLen = _worldPacket.ReadBits<uint>(9);
            Name = _worldPacket.ReadString(nameLen);
        }

        public string Name;
    }

    public class GuildInvite : ServerPacket
    {
        public GuildInvite() : base(Opcode.SMSG_GUILD_INVITE) { }

        public override void Write()
        {
            _worldPacket.WriteBits(InviterName.GetByteCount(), 6);
            _worldPacket.WriteBits(GuildName.GetByteCount(), 7);
            _worldPacket.WriteBits(OldGuildName.GetByteCount(), 7);

            _worldPacket.WriteUInt32(InviterVirtualRealmAddress);
            _worldPacket.WriteUInt32(GuildVirtualRealmAddress);
            _worldPacket.WritePackedGuid128(GuildGUID);
            _worldPacket.WriteUInt32(OldGuildVirtualRealmAddress);
            _worldPacket.WritePackedGuid128(OldGuildGUID);
            _worldPacket.WriteUInt32(EmblemStyle);
            _worldPacket.WriteUInt32(EmblemColor);
            _worldPacket.WriteUInt32(BorderStyle);
            _worldPacket.WriteUInt32(BorderColor);
            _worldPacket.WriteUInt32(BackgroundColor);
            _worldPacket.WriteInt32(AchievementPoints);

            _worldPacket.WriteString(InviterName);
            _worldPacket.WriteString(GuildName);
            _worldPacket.WriteString(OldGuildName);
        }

        public WowGuid128 GuildGUID;
        public WowGuid128 OldGuildGUID = WowGuid128.Empty;
        public uint EmblemColor;
        public uint EmblemStyle;
        public uint BorderStyle;
        public uint BorderColor;
        public uint BackgroundColor;
        public int AchievementPoints = -1;
        public uint GuildVirtualRealmAddress;
        public uint OldGuildVirtualRealmAddress;
        public uint InviterVirtualRealmAddress;
        public string InviterName;
        public string GuildName;
        public string OldGuildName = "";
    }

    public class AcceptGuildInvite : ClientPacket
    {
        public AcceptGuildInvite(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class DeclineGuildInvite : ClientPacket
    {
        public DeclineGuildInvite(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class GuildSetRankPermissions : ClientPacket
    {
        public GuildSetRankPermissions(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            RankID = _worldPacket.ReadUInt32();
            RankOrder = _worldPacket.ReadUInt32();
            Flags = _worldPacket.ReadUInt32();
            WithdrawGoldLimit = _worldPacket.ReadInt32();

            for (byte i = 0; i < GuildConst.MaxBankTabs; i++)
            {
                TabFlags[i] = _worldPacket.ReadUInt32();
                TabWithdrawItemLimit[i] = _worldPacket.ReadUInt32();
            }

            OldFlags = _worldPacket.ReadUInt32();

            _worldPacket.ResetBitPos();
            uint rankNameLen = _worldPacket.ReadBits<uint>(7);
            RankName = _worldPacket.ReadString(rankNameLen);
        }

        public uint RankID;
        public uint RankOrder;
        public int WithdrawGoldLimit;
        public uint Flags;
        public uint OldFlags;
        public uint[] TabFlags = new uint[GuildConst.MaxBankTabs];
        public uint[] TabWithdrawItemLimit = new uint[GuildConst.MaxBankTabs];
        public string RankName;
    }

    public class GuildAddRank : ClientPacket
    {
        public GuildAddRank(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLen = _worldPacket.ReadBits<uint>(7);
            _worldPacket.ResetBitPos();

            RankOrder = _worldPacket.ReadInt32();
            Name = _worldPacket.ReadString(nameLen);
        }

        public string Name;
        public int RankOrder;
    }

    public class GuildDeleteRank : ClientPacket
    {
        public GuildDeleteRank(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            RankOrder = _worldPacket.ReadInt32();
        }

        public int RankOrder;
    }

    public class GuildSetGuildMaster : ClientPacket
    {
        public GuildSetGuildMaster(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLen = _worldPacket.ReadBits<uint>(9);
            NewMasterName = _worldPacket.ReadString(nameLen);
        }

        public string NewMasterName;
    }

    public class GuildLeave : ClientPacket
    {
        public GuildLeave(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class GuildDelete : ClientPacket
    {
        public GuildDelete(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class PlayerTabardVendorActivate : ServerPacket
    {
        public PlayerTabardVendorActivate() : base(Opcode.SMSG_PLAYER_TABARD_VENDOR_ACTIVATE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(DesignerGUID);
        }

        public WowGuid128 DesignerGUID;
    }

    public class SaveGuildEmblem : ClientPacket
    {
        public SaveGuildEmblem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            DesignerGUID = _worldPacket.ReadPackedGuid128();
            EmblemStyle = _worldPacket.ReadUInt32();
            EmblemColor = _worldPacket.ReadUInt32();
            BorderStyle = _worldPacket.ReadUInt32();
            BorderColor = _worldPacket.ReadUInt32();
            BackgroundColor = _worldPacket.ReadUInt32();
        }

        public WowGuid128 DesignerGUID;
        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
        public uint BackgroundColor;
    }

    public class PlayerSaveGuildEmblem : ServerPacket
    {
        public PlayerSaveGuildEmblem() : base(Opcode.SMSG_PLAYER_SAVE_GUILD_EMBLEM) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32((uint)Error);
        }

        public GuildEmblemError Error;
    }
}
