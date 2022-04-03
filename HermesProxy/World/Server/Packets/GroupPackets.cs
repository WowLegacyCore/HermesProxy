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

    class PartyUpdate : ServerPacket
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

    struct PartyPlayerInfo
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

    class PartyLFGInfo
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

    class PartyLootSettings
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Method);
            data.WritePackedGuid128(LootMaster);
            data.WriteUInt8(Threshold);
        }

        public byte Method;
        public WowGuid128 LootMaster;
        public byte Threshold;
    }

    class PartyDifficultySettings
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(DungeonDifficultyID);
            data.WriteUInt32(RaidDifficultyID);
            data.WriteUInt32(LegacyRaidDifficultyID);
        }

        public uint DungeonDifficultyID;
        public uint RaidDifficultyID;
        public uint LegacyRaidDifficultyID;
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
            _worldPacket.WriteBit(Power.HasValue);
            _worldPacket.WriteBit(MaxPower.HasValue);
            _worldPacket.WriteBit(Level.HasValue);
            _worldPacket.WriteBit(Spec.HasValue);
            _worldPacket.WriteBit(AreaID.HasValue);
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
                Pet.Write(_worldPacket);

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
            if (Power.HasValue)
                _worldPacket.WriteUInt16(Power.Value);
            if (MaxPower.HasValue)
                _worldPacket.WriteUInt16(MaxPower.Value);
            if (Level.HasValue)
                _worldPacket.WriteUInt16(Level.Value);
            if (Spec.HasValue)
                _worldPacket.WriteUInt16(Spec.Value);
            if (AreaID.HasValue)
                _worldPacket.WriteUInt16(AreaID.Value);
            if (WmoGroupID.HasValue)
                _worldPacket.WriteUInt16(WmoGroupID.Value);
            if (WmoDoodadPlacementID.HasValue)
                _worldPacket.WriteUInt32(WmoDoodadPlacementID.Value);
            if (Position != null)
            {
                _worldPacket.WriteUInt16(Position.X);
                _worldPacket.WriteUInt16(Position.Y);
                _worldPacket.WriteUInt16(Position.Z);
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
        public ushort? Power;
        public ushort? MaxPower;
        public ushort? Level;
        public ushort? Spec;
        public ushort? AreaID;
        public ushort? WmoGroupID;
        public uint? WmoDoodadPlacementID;
        public Vector3_UInt16 Position;
        public uint? VehicleSeatRecID;
        public List<AuraInfo> Auras;
        public PetState Pet;
        public PhaseInfo Phase;
        public UnkStruct901_2 Unk901_2;

        public class PartyTypeChange
        {
            public byte PartyType1;
            public byte PartyType2;
        }
        public class Vector3_UInt16
        {
            public ushort X;
            public ushort Y;
            public ushort Z;
        }
        public class AuraInfo
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
        public class PetState
        {
            public void Write(WorldPacket data)
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

            public WowGuid128 NewPetGuid;
            public string NewPetName;
            public uint? DisplayID;
            public uint? MaxHealth;
            public uint? Health;
            public List<AuraInfo> Auras;
        }
        public class PhaseInfo
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
            public List<PhaseData> Phases;
            public WowGuid128 PersonalGUID;

            public struct PhaseData
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
}
