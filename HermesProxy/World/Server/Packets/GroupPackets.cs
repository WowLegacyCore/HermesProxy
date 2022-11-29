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
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class PartyInviteClient : ClientPacket
    {
        public PartyInviteClient(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();

            uint targetNameLen = _worldPacket.ReadBits<uint>(9);
            uint targetRealmLen = _worldPacket.ReadBits<uint>(9);

            VirtualRealmAddress = _worldPacket.ReadUInt32();
            TargetGUID = _worldPacket.ReadPackedGuid128();

            TargetName = _worldPacket.ReadString(targetNameLen);
            TargetRealm = _worldPacket.ReadString(targetRealmLen);
        }

        public byte PartyIndex;
        public uint VirtualRealmAddress;
        public WowGuid128 TargetGUID;
        public string TargetName;
        public string TargetRealm;
    }

    class PartyCommandResult : ServerPacket
    {
        public PartyCommandResult() : base(Opcode.SMSG_PARTY_COMMAND_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Name.GetByteCount(), 9);
            _worldPacket.WriteBits(Command, 4);
            _worldPacket.WriteBits(Result, 6);

            _worldPacket.WriteUInt32(ResultData);
            _worldPacket.WritePackedGuid128(ResultGUID);
            _worldPacket.WriteString(Name);
        }

        public string Name;
        public byte Command;
        public byte Result;
        public uint ResultData;
        public WowGuid128 ResultGUID = WowGuid128.Empty;
    }

    class GroupDecline : ServerPacket
    {
        public GroupDecline() : base(Opcode.SMSG_GROUP_DECLINE) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Name.GetByteCount(), 9);
            _worldPacket.FlushBits();
            _worldPacket.WriteString(Name);
        }

        public string Name;
    }

    class PartyInvite : ServerPacket
    {
        public PartyInvite() : base(Opcode.SMSG_PARTY_INVITE) { }

        public override void Write()
        {
            _worldPacket.WriteBit(CanAccept);
            _worldPacket.WriteBit(MightCRZYou);
            _worldPacket.WriteBit(IsXRealm);
            _worldPacket.WriteBit(MustBeBNetFriend);
            _worldPacket.WriteBit(AllowMultipleRoles);
            _worldPacket.WriteBit(QuestSessionActive);
            _worldPacket.WriteBits(InviterName.GetByteCount(), 6);

            InviterRealm.Write(_worldPacket);

            _worldPacket.WritePackedGuid128(InviterGUID);
            _worldPacket.WritePackedGuid128(InviterBNetAccountId);
            _worldPacket.WriteUInt16(Unk1);
            _worldPacket.WriteUInt32(ProposedRoles);
            _worldPacket.WriteInt32(LfgSlots.Count);
            _worldPacket.WriteInt32(LfgCompletedMask);

            _worldPacket.WriteString(InviterName);

            foreach (int LfgSlot in LfgSlots)
                _worldPacket.WriteInt32(LfgSlot);
        }

        public bool CanAccept = true;
        public bool MightCRZYou;
        public bool IsXRealm;
        public bool MustBeBNetFriend;
        public bool AllowMultipleRoles;
        public bool QuestSessionActive;
        public ushort Unk1 = 4904;

        // Inviter
        public VirtualRealmInfo InviterRealm;
        public WowGuid128 InviterGUID;
        public WowGuid128 InviterBNetAccountId;
        public string InviterName;

        // Lfg
        public uint ProposedRoles;
        public int LfgCompletedMask;
        public List<int> LfgSlots = new();
    }

    class PartyInviteResponse : ClientPacket
    {
        public PartyInviteResponse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();

            Accept = _worldPacket.HasBit();

            bool hasRolesDesired = _worldPacket.HasBit();
            if (hasRolesDesired)
                RolesDesired = _worldPacket.ReadUInt32();
        }

        public byte PartyIndex;
        public bool Accept;
        public uint? RolesDesired;
    }

    public class PartyUpdate : ServerPacket
    {
        public PartyUpdate() : base(Opcode.SMSG_PARTY_UPDATE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt16((ushort)PartyFlags);
            _worldPacket.WriteUInt8(PartyIndex);
            _worldPacket.WriteUInt8((byte)PartyType);
            _worldPacket.WriteInt32(MyIndex);
            _worldPacket.WritePackedGuid128(PartyGUID);
            _worldPacket.WriteInt32(SequenceNum);
            _worldPacket.WritePackedGuid128(LeaderGUID);
            _worldPacket.WriteInt32(PlayerList.Count);
            _worldPacket.WriteBit(LfgInfos != null);
            _worldPacket.WriteBit(LootSettings != null);
            _worldPacket.WriteBit(DifficultySettings != null);
            _worldPacket.FlushBits();

            foreach (var playerInfo in PlayerList)
                playerInfo.Write(_worldPacket);

            if (LootSettings != null)
                LootSettings.Write(_worldPacket);

            if (DifficultySettings != null)
                DifficultySettings.Write(_worldPacket);

            if (LfgInfos != null)
                LfgInfos.Write(_worldPacket);
        }

        public GroupFlags PartyFlags;
        public byte PartyIndex;
        public GroupType PartyType;

        public WowGuid128 PartyGUID;
        public WowGuid128 LeaderGUID;

        public int MyIndex;
        public int SequenceNum;

        public List<PartyPlayerInfo> PlayerList = new();

        public PartyLFGInfo LfgInfos;
        public PartyLootSettings LootSettings;
        public PartyDifficultySettings DifficultySettings;
    }

    public struct PartyPlayerInfo
    {
        public void Write(WorldPacket data)
        {
            data.WriteBits(Name.GetByteCount(), 6);
            data.WriteBits(VoiceStateID.GetByteCount() + 1, 6);
            data.WriteBit(FromSocialQueue);
            data.WriteBit(VoiceChatSilenced);
            data.WritePackedGuid128(GUID);
            data.WriteUInt8((byte)Status);
            data.WriteUInt8(Subgroup);
            data.WriteUInt8((byte)Flags);
            data.WriteUInt8(RolesAssigned);
            data.WriteUInt8((byte)ClassId);
            data.WriteString(Name);
            if (!VoiceStateID.IsEmpty())
                data.WriteString(VoiceStateID);
        }

        public WowGuid128 GUID;
        public string Name;
        public string VoiceStateID;   // same as bgs.protocol.club.v1.MemberVoiceState.id
        public Class ClassId;
        public GroupMemberOnlineStatus Status;
        public byte Subgroup;
        public GroupMemberFlags Flags;
        public byte RolesAssigned;
        public bool FromSocialQueue;
        public bool VoiceChatSilenced;
    }

    public class PartyLFGInfo
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(MyFlags);
            data.WriteUInt32(Slot);
            data.WriteUInt32(MyRandomSlot);
            data.WriteUInt8(MyPartialClear);
            data.WriteFloat(MyGearDiff);
            data.WriteUInt8(MyStrangerCount);
            data.WriteUInt8(MyKickVoteCount);
            data.WriteUInt8(BootCount);
            data.WriteBit(Aborted);
            data.WriteBit(MyFirstReward);
            data.FlushBits();
        }

        public byte MyFlags;
        public uint Slot;
        public byte BootCount;
        public uint MyRandomSlot;
        public bool Aborted;
        public byte MyPartialClear;
        public float MyGearDiff;
        public byte MyStrangerCount;
        public byte MyKickVoteCount;
        public bool MyFirstReward;
    }

    public class PartyLootSettings
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8((byte)Method);
            data.WritePackedGuid128(LootMaster);
            data.WriteUInt8(Threshold);
        }

        public LootMethod Method;
        public WowGuid128 LootMaster;
        public byte Threshold;
    }

    public class PartyDifficultySettings
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32((uint)DungeonDifficultyID);
            data.WriteUInt32((uint)RaidDifficultyID);
            data.WriteUInt32((uint)LegacyRaidDifficultyID);
        }

        public Difficulty DungeonDifficultyID;
        public Difficulty RaidDifficultyID;
        public Difficulty LegacyRaidDifficultyID;
    }

    class LeaveGroup : ClientPacket
    {
        public LeaveGroup(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
        }

        public sbyte PartyIndex;
    }

    class PartyUninvite : ClientPacket
    {
        public PartyUninvite(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();
            TargetGUID = _worldPacket.ReadPackedGuid128();

            byte reasonLen = _worldPacket.ReadBits<byte>(8);
            Reason = _worldPacket.ReadString(reasonLen);
        }

        public byte PartyIndex;
        public WowGuid128 TargetGUID;
        public string Reason;
    }

    class GroupUninvite : ServerPacket
    {
        public GroupUninvite() : base(Opcode.SMSG_GROUP_UNINVITE) { }

        public override void Write() { }
    }

    class SetAssistantLeader : ClientPacket
    {
        public SetAssistantLeader(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();
            TargetGUID = _worldPacket.ReadPackedGuid128();
            Apply = _worldPacket.HasBit();
        }

        public byte PartyIndex;
        public WowGuid128 TargetGUID;
        public bool Apply;
    }

    class SetEveryoneIsAssistant : ClientPacket
    {
        public SetEveryoneIsAssistant(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();
            Apply = _worldPacket.HasBit();
        }

        public byte PartyIndex;
        public bool Apply;
    }

    class SetPartyLeader : ClientPacket
    {
        public SetPartyLeader(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
            TargetGUID = _worldPacket.ReadPackedGuid128();
        }

        public sbyte PartyIndex;
        public WowGuid128 TargetGUID;
    }

    class GroupNewLeader : ServerPacket
    {
        public GroupNewLeader() : base(Opcode.SMSG_GROUP_NEW_LEADER) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WriteBits(Name.GetByteCount(), 9);
            _worldPacket.WriteString(Name);
        }

        public sbyte PartyIndex;
        public string Name;
    }

    class ConvertRaid : ClientPacket
    {
        public ConvertRaid(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Raid = _worldPacket.HasBit();
        }

        public bool Raid;
    }

    class DoReadyCheck : ClientPacket
    {
        public DoReadyCheck(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
        }

        public sbyte PartyIndex;
    }

    class ReadyCheckStarted : ServerPacket
    {
        public ReadyCheckStarted() : base(Opcode.SMSG_READY_CHECK_STARTED) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WritePackedGuid128(PartyGUID);
            _worldPacket.WritePackedGuid128(InitiatorGUID);
            _worldPacket.WriteUInt64(Duration);
        }

        public sbyte PartyIndex;
        public WowGuid128 PartyGUID;
        public WowGuid128 InitiatorGUID;
        public ulong Duration = 35000;
    }

    class ReadyCheckResponseClient : ClientPacket
    {
        public ReadyCheckResponseClient(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();
            IsReady = _worldPacket.HasBit();
        }

        public byte PartyIndex;
        public bool IsReady;
    }

    class ReadyCheckResponse : ServerPacket
    {
        public ReadyCheckResponse() : base(Opcode.SMSG_READY_CHECK_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PartyGUID);
            _worldPacket.WritePackedGuid128(Player);

            _worldPacket.WriteBit(IsReady);
            _worldPacket.FlushBits();
        }

        public WowGuid128 PartyGUID;
        public WowGuid128 Player;
        public bool IsReady;
    }

    class ReadyCheckCompleted : ServerPacket
    {
        public ReadyCheckCompleted() : base(Opcode.SMSG_READY_CHECK_COMPLETED) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WritePackedGuid128(PartyGUID);
        }

        public sbyte PartyIndex;
        public WowGuid128 PartyGUID;
    }

    class UpdateRaidTarget : ClientPacket
    {
        public UpdateRaidTarget(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
            Target = _worldPacket.ReadPackedGuid128();
            Symbol = _worldPacket.ReadInt8();
        }

        public sbyte PartyIndex;
        public WowGuid128 Target;
        public sbyte Symbol;
    }

    class SendRaidTargetUpdateSingle : ServerPacket
    {
        public SendRaidTargetUpdateSingle() : base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_SINGLE) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WriteInt8(Symbol);
            _worldPacket.WritePackedGuid128(Target);
            _worldPacket.WritePackedGuid128(ChangedBy);
        }

        public sbyte PartyIndex;
        public sbyte Symbol;
        public WowGuid128 Target;
        public WowGuid128 ChangedBy;
    }

    class SendRaidTargetUpdateAll : ServerPacket
    {
        public SendRaidTargetUpdateAll() : base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_ALL) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WriteInt32(TargetIcons.Count);

            foreach (var pair in TargetIcons)
            {
                _worldPacket.WritePackedGuid128(pair.Item2);
                _worldPacket.WriteInt8(pair.Item1);
            }
        }

        public sbyte PartyIndex;
        public List<Tuple<sbyte, WowGuid128>> TargetIcons = new();
    }

    class SummonRequest : ServerPacket
    {
        public SummonRequest() : base(Opcode.SMSG_SUMMON_REQUEST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(SummonerGUID);
            _worldPacket.WriteUInt32(SummonerVirtualRealmAddress);
            _worldPacket.WriteInt32(AreaID);
            _worldPacket.WriteUInt8((byte)Reason);
            _worldPacket.WriteBit(SkipStartingArea);
            _worldPacket.FlushBits();
        }

        public WowGuid128 SummonerGUID;
        public uint SummonerVirtualRealmAddress;
        public int AreaID;
        public SummonReason Reason;
        public bool SkipStartingArea;

        public enum SummonReason
        {
            Spell = 0,
            Scenario = 1
        }
    }

    class SummonResponse : ClientPacket
    {
        public SummonResponse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SummonerGUID = _worldPacket.ReadPackedGuid128();
            Accept = _worldPacket.HasBit();
        }

        public WowGuid128 SummonerGUID;
        public bool Accept;
    }

    class RequestPartyMemberStats : ClientPacket
    {
        public RequestPartyMemberStats(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadUInt8();
            TargetGUID = _worldPacket.ReadPackedGuid128();
        }

        public byte PartyIndex;
        public WowGuid128 TargetGUID;
    }

    class PartyMemberPartialState : ServerPacket
    {
        public PartyMemberPartialState() : base(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE) { }

        public override void Write()
        {
            _worldPacket.WriteBit(ForEnemyChanged);
            _worldPacket.WriteBit(SetPvPInactive); // adds GroupMemberStatusFlag 0x0020 if true, removes 0x0020 if false
            _worldPacket.WriteBit(Unk901_1);

            _worldPacket.WriteBit(PartyType != null);
            _worldPacket.WriteBit(StatusFlags.HasValue);
            _worldPacket.WriteBit(PowerType.HasValue);
            _worldPacket.WriteBit(OverrideDisplayPower.HasValue);
            _worldPacket.WriteBit(CurrentHealth.HasValue);
            _worldPacket.WriteBit(MaxHealth.HasValue);
            _worldPacket.WriteBit(CurrentPower.HasValue);
            _worldPacket.WriteBit(MaxPower.HasValue);
            _worldPacket.WriteBit(Level.HasValue);
            _worldPacket.WriteBit(Spec.HasValue);
            _worldPacket.WriteBit(ZoneID.HasValue);
            _worldPacket.WriteBit(WmoGroupID.HasValue);
            _worldPacket.WriteBit(WmoDoodadPlacementID.HasValue);
            _worldPacket.WriteBit(Position != null);
            _worldPacket.WriteBit(VehicleSeatRecID.HasValue);
            _worldPacket.WriteBit(Auras != null);
            _worldPacket.WriteBit(Pet != null);
            _worldPacket.WriteBit(Phase != null);
            _worldPacket.WriteBit(Unk901_2 != null);
            _worldPacket.FlushBits();

            if (Pet != null)
                Pet.WritePartial(_worldPacket);

            _worldPacket.WritePackedGuid128(AffectedGUID);
            if (PartyType != null)
            {
                _worldPacket.WriteUInt8(PartyType.PartyType1);
                _worldPacket.WriteUInt8(PartyType.PartyType2);
            }

            if (StatusFlags.HasValue)
                _worldPacket.WriteUInt16(StatusFlags.Value);
            if (PowerType.HasValue)
                _worldPacket.WriteUInt8(PowerType.Value);
            if (OverrideDisplayPower.HasValue)
                _worldPacket.WriteUInt16(OverrideDisplayPower.Value);
            if (CurrentHealth.HasValue)
                _worldPacket.WriteUInt32(CurrentHealth.Value);
            if (MaxHealth.HasValue)
                _worldPacket.WriteUInt32(MaxHealth.Value);
            if (CurrentPower.HasValue)
                _worldPacket.WriteUInt16(CurrentPower.Value);
            if (MaxPower.HasValue)
                _worldPacket.WriteUInt16(MaxPower.Value);
            if (Level.HasValue)
                _worldPacket.WriteUInt16(Level.Value);
            if (Spec.HasValue)
                _worldPacket.WriteUInt16(Spec.Value);
            if (ZoneID.HasValue)
                _worldPacket.WriteUInt16(ZoneID.Value);
            if (WmoGroupID.HasValue)
                _worldPacket.WriteUInt16(WmoGroupID.Value);
            if (WmoDoodadPlacementID.HasValue)
                _worldPacket.WriteUInt32(WmoDoodadPlacementID.Value);
            if (Position != null)
            {
                _worldPacket.WriteInt16(Position.X);
                _worldPacket.WriteInt16(Position.Y);
                _worldPacket.WriteInt16(Position.Z);
            }
            if (VehicleSeatRecID.HasValue)
                _worldPacket.WriteUInt32(VehicleSeatRecID.Value);
            if (Auras != null)
            {
                _worldPacket.WriteInt32(Auras.Count);
                foreach (var aura in Auras)
                    aura.Write(_worldPacket);
            }
            if (Phase != null)
                Phase.Write(_worldPacket);

            if (Unk901_2 != null)
                Unk901_2.Write(_worldPacket);
        }

        public WowGuid128 AffectedGUID;
        public bool ForEnemyChanged;
        public bool SetPvPInactive;
        public bool Unk901_1;
        public PartyTypeChange PartyType;
        public ushort? StatusFlags;
        public byte? PowerType;
        public ushort? OverrideDisplayPower;
        public uint? CurrentHealth;
        public uint? MaxHealth;
        public ushort? CurrentPower;
        public ushort? MaxPower;
        public ushort? Level;
        public ushort? Spec;
        public ushort? ZoneID;
        public ushort? WmoGroupID;
        public uint? WmoDoodadPlacementID;
        public Vector3_UInt16 Position;
        public uint? VehicleSeatRecID;
        public List<PartyMemberAuraStates> Auras;
        public PartyMemberPetStats Pet;
        public PartyMemberPhaseStates Phase;
        public UnkStruct901_2 Unk901_2;

        public class PartyTypeChange
        {
            public byte PartyType1;
            public byte PartyType2;
        }
        public class Vector3_UInt16
        {
            public short X;
            public short Y;
            public short Z;
        }
        public class UnkStruct901_2
        {
            public void Write(WorldPacket data)
            {
                data.WriteUInt32(Unk902_3);
                data.WriteUInt32(Unk902_4);
                data.WriteUInt32(Unk902_5);
            }
            public uint Unk902_3;
            public uint Unk902_4;
            public uint Unk902_5;
        }
    }

    class PartyMemberFullState : ServerPacket
    {
        public PartyMemberFullState() : base(Opcode.SMSG_PARTY_MEMBER_FULL_STATE)
        {
            Phases.PhaseShiftFlags = 8;
        }

        public override void Write()
        {
            _worldPacket.WriteBit(ForEnemy);
            _worldPacket.FlushBits();

            for (byte i = 0; i < 2; i++)
                _worldPacket.WriteInt8(PartyType[i]);

            _worldPacket.WriteInt16((short)StatusFlags);
            _worldPacket.WriteUInt8(PowerType);
            _worldPacket.WriteInt16((short)PowerDisplayID);
            _worldPacket.WriteInt32(CurrentHealth);
            _worldPacket.WriteInt32(MaxHealth);
            _worldPacket.WriteUInt16(CurrentPower);
            _worldPacket.WriteUInt16(MaxPower);
            _worldPacket.WriteUInt16(Level);
            _worldPacket.WriteUInt16(SpecID);
            _worldPacket.WriteUInt16(ZoneID);
            _worldPacket.WriteUInt16(WmoGroupID);
            _worldPacket.WriteInt32(WmoDoodadPlacementID);
            _worldPacket.WriteInt16(PositionX);
            _worldPacket.WriteInt16(PositionY);
            _worldPacket.WriteInt16(PositionZ);
            _worldPacket.WriteInt32(VehicleSeat);
            _worldPacket.WriteInt32(Auras.Count);

            Phases.Write(_worldPacket);
            ChromieTime.Write(_worldPacket);

            foreach (PartyMemberAuraStates aura in Auras)
                aura.Write(_worldPacket);

            _worldPacket.WriteBit(Pet != null);
            _worldPacket.FlushBits();

            if (Pet != null)
                Pet.WriteFull(_worldPacket);

            _worldPacket.WritePackedGuid128(MemberGuid);
        }

        public bool ForEnemy;
        public ushort Level;
        public GroupMemberOnlineStatus StatusFlags;

        public int CurrentHealth;
        public int MaxHealth;

        public byte PowerType;
        public ushort CurrentPower;
        public ushort MaxPower;

        public ushort ZoneID;
        public short PositionX;
        public short PositionY;
        public short PositionZ;

        public int VehicleSeat;

        public PartyMemberPhaseStates Phases = new();
        public List<PartyMemberAuraStates> Auras = new();
        public PartyMemberPetStats Pet;

        public ushort PowerDisplayID;
        public ushort SpecID;
        public ushort WmoGroupID;
        public int WmoDoodadPlacementID;
        public sbyte[] PartyType = new sbyte[2];
        public CTROptions ChromieTime;
        public WowGuid128 MemberGuid;
    }

    public struct CTROptions
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(ContentTuningConditionMask);
            data.WriteInt32(Unused901);
            data.WriteUInt32(ExpansionLevelMask);
        }

        public uint ContentTuningConditionMask;
        public int Unused901;
        public uint ExpansionLevelMask;
    }

    public class PartyMemberPhaseStates
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(PhaseShiftFlags);
            data.WriteInt32(Phases.Count);
            data.WritePackedGuid128(PersonalGUID);
            foreach (var phase in Phases)
                phase.Write(data);
        }

        public uint PhaseShiftFlags;
        public List<PartyMemberPhase> Phases = new List<PartyMemberPhase>();
        public WowGuid128 PersonalGUID = WowGuid128.Empty;

        public struct PartyMemberPhase
        {
            public void Write(WorldPacket data)
            {
                data.WriteUInt16(PhaseFlags);
                data.WriteUInt16(Id);
            }
            public ushort PhaseFlags;
            public ushort Id;
        }
    }

    public class PartyMemberAuraStates
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(SpellId);
            data.WriteUInt16(AuraFlags);
            data.WriteUInt32(ActiveFlags);
            data.WriteInt32(Points.Count);
            foreach (var point in Points)
                data.WriteFloat(point);
        }

        public uint SpellId;
        public ushort AuraFlags;
        public uint ActiveFlags;
        public List<float> Points = new();
    }

    public class PartyMemberPetStats
    {
        public void WritePartial(WorldPacket data)
        {
            data.WriteBit(NewPetGuid != null);
            data.WriteBit(NewPetName != null);
            data.WriteBit(DisplayID.HasValue);
            data.WriteBit(MaxHealth.HasValue);
            data.WriteBit(Health.HasValue);
            data.WriteBit(Auras != null);
            data.FlushBits();

            if (NewPetName != null)
            {
                data.WriteBits(NewPetName.GetByteCount(), 8);
                data.WriteString(NewPetName);
            }
            if (NewPetGuid != null)
                data.WritePackedGuid128(NewPetGuid);
            if (DisplayID.HasValue)
                data.WriteUInt32(DisplayID.Value);
            if (MaxHealth.HasValue)
                data.WriteUInt32(MaxHealth.Value);
            if (Health.HasValue)
                data.WriteUInt32(Health.Value);
            if (Auras != null)
            {
                data.WriteInt32(Auras.Count);
                foreach (var aura in Auras)
                    aura.Write(data);
            }
        }

        public void WriteFull(WorldPacket data)
        {
            if (NewPetGuid == null)
                NewPetGuid = WowGuid128.Empty;
            if (NewPetName == null)
                NewPetName = "";
            if (DisplayID == null)
                DisplayID = 0;
            if (MaxHealth == null)
                MaxHealth = 0;
            if (Health == null)
                Health = 0;
            if (Auras == null)
                Auras = new List<PartyMemberAuraStates>();

            data.WritePackedGuid128(NewPetGuid);
            data.WriteUInt32(DisplayID.Value);
            data.WriteUInt32(Health.Value);
            data.WriteUInt32(MaxHealth.Value);
            data.WriteInt32(Auras.Count);
            Auras.ForEach(p => p.Write(data));

            data.WriteBits(NewPetName.GetByteCount(), 8);
            data.FlushBits();
            data.WriteString(NewPetName);
        }

        public WowGuid128 NewPetGuid;
        public string NewPetName;
        public uint? DisplayID;
        public uint? MaxHealth;
        public uint? Health;
        public List<PartyMemberAuraStates> Auras;
    }

    class MinimapPingClient : ClientPacket
    {
        public MinimapPingClient(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Position = _worldPacket.ReadVector2();
            PartyIndex = _worldPacket.ReadInt8();
        }

        public Vector2 Position;
        public sbyte PartyIndex;
    }

    class MinimapPing : ServerPacket
    {
        public MinimapPing() : base(Opcode.SMSG_MINIMAP_PING) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(SenderGUID);
            _worldPacket.WriteVector2(Position);
        }

        public WowGuid128 SenderGUID;
        public Vector2 Position;
    }

    public class RandomRollClient : ClientPacket
    {
        public RandomRollClient(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Min = _worldPacket.ReadInt32();
            Max = _worldPacket.ReadInt32();
            PartyIndex = _worldPacket.ReadUInt8();
        }

        public int Min;
        public int Max;
        public byte PartyIndex;
    }

    public class RandomRoll : ServerPacket
    {
        public RandomRoll() : base(Opcode.SMSG_RANDOM_ROLL) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Roller);
            _worldPacket.WritePackedGuid128(RollerWowAccount);
            _worldPacket.WriteInt32(Min);
            _worldPacket.WriteInt32(Max);
            _worldPacket.WriteInt32(Result);
        }

        public WowGuid128 Roller;
        public WowGuid128 RollerWowAccount;
        public int Min;
        public int Max;
        public int Result;
    }

    class ChangeSubGroup : ClientPacket
    {
        public ChangeSubGroup(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TargetGUID = _worldPacket.ReadPackedGuid128();
            PartyIndex = _worldPacket.ReadInt8();
            NewSubGroup = _worldPacket.ReadUInt8();
        }

        public WowGuid128 TargetGUID;
        public sbyte PartyIndex;
        public byte NewSubGroup;
    }

    class SwapSubGroups : ClientPacket
    {
        public SwapSubGroups(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
            FirstTarget = _worldPacket.ReadPackedGuid128();
            SecondTarget = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 FirstTarget;
        public WowGuid128 SecondTarget;
        public sbyte PartyIndex;
    }
}
