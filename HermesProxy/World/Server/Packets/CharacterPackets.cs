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

using Framework.Dynamic;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using Framework.GameMath;
using HermesProxy.World.Objects;
using Framework.Constants;

namespace HermesProxy.World.Server.Packets
{
    public class EnumCharacters : ClientPacket
    {
        public EnumCharacters(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class EnumCharactersResult : ServerPacket
    {
        public EnumCharactersResult() : base(Opcode.SMSG_ENUM_CHARACTERS_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Success);
            _worldPacket.WriteBit(IsDeletedCharacters);
            _worldPacket.WriteBit(IsNewPlayerRestrictionSkipped);
            _worldPacket.WriteBit(IsNewPlayerRestricted);
            _worldPacket.WriteBit(IsNewPlayer);
            _worldPacket.WriteBit(DisabledClassesMask.HasValue);
            _worldPacket.WriteBit(IsAlliedRacesCreationAllowed);
            _worldPacket.WriteInt32(Characters.Count);
            _worldPacket.WriteInt32(MaxCharacterLevel);
            _worldPacket.WriteInt32(RaceUnlockData.Count);
            _worldPacket.WriteInt32(UnlockedConditionalAppearances.Count);

            if (DisabledClassesMask.HasValue)
                _worldPacket.WriteUInt32(DisabledClassesMask.Value);

            foreach (UnlockedConditionalAppearance unlockedConditionalAppearance in UnlockedConditionalAppearances)
                unlockedConditionalAppearance.Write(_worldPacket);

            foreach (CharacterInfo charInfo in Characters)
                charInfo.Write(_worldPacket);

            foreach (RaceUnlock raceUnlock in RaceUnlockData)
                raceUnlock.Write(_worldPacket);
        }

        public bool Success;
        public bool IsDeletedCharacters; // used for character undelete list
        public bool IsNewPlayerRestrictionSkipped; // allows client to skip new player restrictions
        public bool IsNewPlayerRestricted; // forbids using level boost and class trials
        public bool IsNewPlayer; // forbids hero classes and allied races
        public bool IsAlliedRacesCreationAllowed;

        public int MaxCharacterLevel = 1;
        public Optional<uint> DisabledClassesMask = new();

        public List<CharacterInfo> Characters = new(); // all characters on the list
        public List<RaceUnlock> RaceUnlockData = new(); //
        public List<UnlockedConditionalAppearance> UnlockedConditionalAppearances = new();

        public class CharacterInfo
        {
            public void Write(WorldPacket data)
            {
                data.WritePackedGuid128(Guid);
                data.WriteUInt64(GuildClubMemberID);
                data.WriteUInt8(ListPosition);
                data.WriteUInt8((byte)RaceId);
                data.WriteUInt8((byte)ClassId);
                data.WriteUInt8((byte)SexId);
                data.WriteInt32(Customizations.Count);

                data.WriteUInt8(ExperienceLevel);
                data.WriteUInt32(ZoneId);
                data.WriteUInt32(MapId);
                data.WriteVector3(PreloadPos);
                data.WritePackedGuid128(GuildGuid);
                data.WriteUInt32((uint)Flags);
                data.WriteUInt32(Flags2);
                data.WriteUInt32(Flags3);
                data.WriteUInt32(PetCreatureDisplayId);
                data.WriteUInt32(PetExperienceLevel);
                data.WriteUInt32(PetCreatureFamilyId);

                data.WriteUInt32(ProfessionIds[0]);
                data.WriteUInt32(ProfessionIds[1]);

                foreach (var visualItem in VisualItems)
                    visualItem.Write(data);

                data.WriteInt64(LastPlayedTime);
                data.WriteUInt16(SpecID);
                data.WriteUInt32(Unknown703);
                data.WriteUInt32(LastLoginVersion);
                data.WriteUInt32(Flags4);
                data.WriteInt32(MailSenders.Count);
                data.WriteInt32(MailSenderTypes.Count);
                data.WriteUInt32(OverrideSelectScreenFileDataID);

                foreach (ChrCustomizationChoice customization in Customizations)
                {
                    data.WriteUInt32(customization.ChrCustomizationOptionID);
                    data.WriteUInt32(customization.ChrCustomizationChoiceID);
                }

                foreach (var mailSenderType in MailSenderTypes)
                    data.WriteUInt32(mailSenderType);

                data.WriteBits(Name.GetByteCount(), 6);
                data.WriteBit(FirstLogin);
                data.WriteBit(BoostInProgress);
                data.WriteBits(unkWod61x, 5);
                data.WriteBit(false);
                data.WriteBit(ExpansionChosen);

                foreach (string str in MailSenders)
                    data.WriteBits(str.GetByteCount() + 1, 6);
                
                data.FlushBits();

                foreach (string str in MailSenders)
                    if (!str.IsEmpty())
                        data.WriteCString(str);

                data.WriteString(Name);
            }

            public WowGuid128 Guid;
            public ulong GuildClubMemberID; // same as bgs.protocol.club.v1.MemberId.unique_id, guessed basing on SMSG_QUERY_PLAYER_NAME_RESPONSE (that one is known)
            public string Name;
            public byte ListPosition; // Order of the characters in list
            public Race RaceId;
            public Class ClassId;
            public Gender SexId;
            public Array<ChrCustomizationChoice> Customizations;
            public byte ExperienceLevel;
            public uint ZoneId;
            public uint MapId;
            public Vector3 PreloadPos;
            public WowGuid128 GuildGuid;
            public CharacterFlags Flags; // Character flag @see enum CharacterFlags
            public uint Flags2;
            public uint Flags3;
            public uint Flags4;
            public bool FirstLogin;
            public byte unkWod61x;
            public bool ExpansionChosen;
            public long LastPlayedTime;
            public ushort SpecID;
            public uint Unknown703;
            public uint LastLoginVersion;
            public uint OverrideSelectScreenFileDataID;
            public uint PetCreatureDisplayId;
            public uint PetExperienceLevel;
            public uint PetCreatureFamilyId;
            public bool BoostInProgress; // @todo
            public uint[] ProfessionIds = new uint[2];      // @todo
            public VisualItemInfo[] VisualItems = new VisualItemInfo[InventorySlots.BagEnd];
            public List<string> MailSenders = new();
            public List<uint> MailSenderTypes = new();

            public struct VisualItemInfo
            {
                public void Write(WorldPacket data)
                {
                    data.WriteUInt32(DisplayId);
                    data.WriteUInt32(DisplayEnchantId);
                    data.WriteUInt32(SecondaryItemModifiedAppearanceID);
                    data.WriteUInt8(InvType);
                    data.WriteUInt8(Subclass);
                }

                public uint DisplayId;
                public uint DisplayEnchantId;
                public uint SecondaryItemModifiedAppearanceID; // also -1 is some special value
                public byte InvType;
                public byte Subclass;
            }

            public struct PetInfo
            {
                public uint CreatureDisplayId; // PetCreatureDisplayID
                public uint Level; // PetExperienceLevel
                public uint CreatureFamily; // PetCreatureFamilyID
            }
        }

        public struct RaceUnlock
        {
            public RaceUnlock(int raceId, bool hasExpansion, bool hasAchievement, bool hasHeritageArmor)
            {
                RaceID = raceId;
                HasExpansion = hasExpansion;
                HasAchievement = hasAchievement;
                HasHeritageArmor = hasHeritageArmor;
            }
            public void Write(WorldPacket data)
            {
                data.WriteInt32(RaceID);
                data.WriteBit(HasExpansion);
                data.WriteBit(HasAchievement);
                data.WriteBit(HasHeritageArmor);
                data.FlushBits();
            }

            public int RaceID;
            public bool HasExpansion;
            public bool HasAchievement;
            public bool HasHeritageArmor;
        }

        public struct UnlockedConditionalAppearance
        {
            public void Write(WorldPacket data)
            {
                data.WriteInt32(AchievementID);
                data.WriteInt32(Unused);
            }

            public int AchievementID;
            public int Unused;
        }
    }

    public class ChrCustomizationChoice : IComparable<ChrCustomizationChoice>
    {
        public uint ChrCustomizationOptionID;
        public uint ChrCustomizationChoiceID;

        public ChrCustomizationChoice(uint optionId, uint chocieId)
        {
            ChrCustomizationOptionID = optionId;
            ChrCustomizationChoiceID = chocieId;
        }

        public void WriteCreate(WorldPacket data)
        {
            data.WriteUInt32(ChrCustomizationOptionID);
            data.WriteUInt32(ChrCustomizationChoiceID);
        }

        public void WriteUpdate(WorldPacket data)
        {
            data.WriteUInt32(ChrCustomizationOptionID);
            data.WriteUInt32(ChrCustomizationChoiceID);
        }

        public int CompareTo(ChrCustomizationChoice other)
        {
            return ChrCustomizationOptionID.CompareTo(other.ChrCustomizationOptionID);
        }
    }

    public class CreateCharacter : ClientPacket
    {
        public CreateCharacter(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            CreateInfo = new CharacterCreateInfo();
            uint nameLength = _worldPacket.ReadBits<uint>(6);
            bool hasTemplateSet = _worldPacket.HasBit();
            CreateInfo.IsTrialBoost = _worldPacket.HasBit();
            CreateInfo.UseNPE = _worldPacket.HasBit();

            CreateInfo.RaceId = (Race)_worldPacket.ReadUInt8();
            CreateInfo.ClassId = (Class)_worldPacket.ReadUInt8();
            CreateInfo.Sex = (Gender)_worldPacket.ReadUInt8();
            var customizationCount = _worldPacket.ReadUInt32();

            CreateInfo.Name = _worldPacket.ReadString(nameLength);
            if (CreateInfo.TemplateSet.HasValue)
                CreateInfo.TemplateSet.Set(_worldPacket.ReadUInt32());

            for (var i = 0; i < customizationCount; ++i)
            {
                CreateInfo.Customizations[i] = new ChrCustomizationChoice(_worldPacket.ReadUInt32(), _worldPacket.ReadUInt32());
            }

            CreateInfo.Customizations.Sort();
        }

        public CharacterCreateInfo CreateInfo;
    }

    public class CharacterCreateInfo
    {
        // User specified variables
        public Race RaceId = Race.None;
        public Class ClassId = Class.None;
        public Gender Sex = Gender.None;
        public Array<ChrCustomizationChoice> Customizations = new(50);
        public Optional<uint> TemplateSet = new();
        public bool IsTrialBoost;
        public bool UseNPE;
        public string Name;

        // Server side data
        public byte CharCount = 0;
    }

    public class CreateChar : ServerPacket
    {
        public CreateChar() : base(Opcode.SMSG_CREATE_CHAR) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)Code);
            _worldPacket.WritePackedGuid128(Guid);
        }

        public HermesProxy.World.Objects.Classic.ResponseCodes Code;
        public WowGuid128 Guid;
    }

    public class CharDelete : ClientPacket
    {
        public CharDelete(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Guid; // Guid of the character to delete
    }

    public class DeleteChar : ServerPacket
    {
        public DeleteChar() : base(Opcode.SMSG_DELETE_CHAR) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)Code);
        }

        public HermesProxy.World.Objects.Classic.ResponseCodes Code;
    }

    public class LoadingScreenNotify : ClientPacket
    {
        public LoadingScreenNotify(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            MapID = _worldPacket.ReadUInt32();
            Showing = _worldPacket.HasBit();
        }

        public uint MapID;
        public bool Showing;
    }

    public class PlayerLogin : ClientPacket
    {
        public PlayerLogin(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128();
            FarClip = _worldPacket.ReadFloat();
            UnkBit = _worldPacket.HasBit();
        }

        public WowGuid128 Guid;      // Guid of the player that is logging in
        public float FarClip;        // Visibility distance (for terrain)
        public bool UnkBit;
    }

    public class LoginVerifyWorld : ServerPacket
    {
        public LoginVerifyWorld() : base(Opcode.SMSG_LOGIN_VERIFY_WORLD, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteFloat(Pos.X);
            _worldPacket.WriteFloat(Pos.Y);
            _worldPacket.WriteFloat(Pos.Z);
            _worldPacket.WriteFloat(Pos.Orientation);
            _worldPacket.WriteUInt32(Reason);
        }

        public uint MapID;
        public Position Pos;
        public uint Reason;
    }

    public class CharacterLoginFailed : ServerPacket
    {
        public CharacterLoginFailed() : base(Opcode.SMSG_CHARACTER_LOGIN_FAILED) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)Code);
        }

        public LoginFailureReason Code;
    }

    public class LogoutRequest : ClientPacket
    {
        public LogoutRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            IdleLogout = _worldPacket.HasBit();
        }

        public bool IdleLogout;
    }

    public class LogoutResponse : ServerPacket
    {
        public LogoutResponse() : base(Opcode.SMSG_LOGOUT_RESPONSE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(LogoutResult);
            _worldPacket.WriteBit(Instant);
            _worldPacket.FlushBits();
        }

        public int LogoutResult;
        public bool Instant = false;
    }

    public class LogoutComplete : ServerPacket
    {
        public LogoutComplete() : base(Opcode.SMSG_LOGOUT_COMPLETE) { }

        public override void Write() { }
    }

    public class LogoutCancel : ClientPacket
    {
        public LogoutCancel(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class LogoutCancelAck : ServerPacket
    {
        public LogoutCancelAck() : base(Opcode.SMSG_LOGOUT_CANCEL_ACK, ConnectionType.Instance) { }

        public override void Write() { }
    }
}
