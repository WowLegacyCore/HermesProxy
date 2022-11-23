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
        public uint? DisabledClassesMask = new();

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
            public VisualItemInfo[] VisualItems = new VisualItemInfo[Enums.Classic.InventorySlots.BagEnd];
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

    public class AccountCharacterListEntry
    {
        public void Write(WorldPacket packet)
        {
            packet.WritePackedGuid128(AccountId);
            packet.WritePackedGuid128(CharacterGuid);
            packet.WriteUInt32(RealmVirtualAddress);
            packet.WriteUInt8((byte)Race);
            packet.WriteUInt8((byte)Class);
            packet.WriteUInt8((byte)Sex);
            packet.WriteUInt8(Level);

            packet.WriteInt64(LastLoginUnixSec);

            if (ModernVersion.AddedInClassicVersion(1, 14, 1, 2, 5, 3))
                packet.WriteUInt32(Unk);

            packet.ResetBitPos();
            packet.WriteBits(Name.GetByteCount(), 6);
            packet.WriteBits(RealmName.GetByteCount(), 9);

            packet.WriteString(Name);
            packet.WriteString(RealmName);
        }

        public WowGuid128 AccountId;

        public uint RealmVirtualAddress;
        public string RealmName;

        public WowGuid128 CharacterGuid;
        public string Name;
        public Race Race;
        public Class Class;
        public Gender Sex;
        public byte Level;
        public long LastLoginUnixSec;
        public uint Unk;
    }

    public class GetAccountCharacterListRequest : ClientPacket
    {
        public GetAccountCharacterListRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Token = _worldPacket.ReadUInt32();
        }

        public uint Token = 0;
    }

    public class GetAccountCharacterListResult : ServerPacket
    {
        public GetAccountCharacterListResult() : base(Opcode.SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT)
        {
        }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Token);
            _worldPacket.WriteUInt32((uint)CharacterList.Count);

            _worldPacket.ResetBitPos();
            _worldPacket.WriteBit(false); // unknown bit

            foreach (var entry in CharacterList)
                entry.Write(_worldPacket);
        }

        public uint Token = 0;
        public List<AccountCharacterListEntry> CharacterList = new();
    }

    public class GenerateRandomCharacterNameRequest : ClientPacket
    {
        public GenerateRandomCharacterNameRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Race = (Race)_worldPacket.ReadUInt8();
            Sex = (Gender)_worldPacket.ReadUInt8();
        }

        public Race Race;
        public Gender Sex;
    }

    public class GenerateRandomCharacterNameResult : ServerPacket
    {
        public GenerateRandomCharacterNameResult() : base(Opcode.SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteBool(Success);

            _worldPacket.WriteBits(Name.Length, 6);
            _worldPacket.WriteString(Name);
        }

        public bool Success;
        public string Name = "";
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
            if (hasTemplateSet)
                CreateInfo.TemplateSet = _worldPacket.ReadUInt32();

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
        public uint? TemplateSet;
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

        public HermesProxy.World.Enums.Classic.ResponseCodes Code;
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

        public HermesProxy.World.Enums.Classic.ResponseCodes Code;
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

    class LogXPGain : ServerPacket
    {
        public LogXPGain() : base(Opcode.SMSG_LOG_XP_GAIN) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Victim);
            _worldPacket.WriteInt32(Original);
            _worldPacket.WriteUInt8((byte)Reason);
            _worldPacket.WriteInt32(Amount);
            _worldPacket.WriteFloat(GroupBonus);
            _worldPacket.WriteUInt8(RAFBonus);
        }

        public WowGuid128 Victim;
        public int Original;
        public PlayerLogXPReason Reason;
        public int Amount;
        public float GroupBonus = 1;
        public byte RAFBonus; // 1 - 300% of normal XP; 2 - 150% of normal XP
    }

    public class RequestPlayedTime : ClientPacket
    {
        public RequestPlayedTime(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TriggerScriptEvent = _worldPacket.HasBit();
        }

        public bool TriggerScriptEvent;
    }

    public class PlayedTime : ServerPacket
    {
        public PlayedTime() : base(Opcode.SMSG_PLAYED_TIME, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(TotalTime);
            _worldPacket.WriteUInt32(LevelTime);
            _worldPacket.WriteBit(TriggerEvent);
            _worldPacket.FlushBits();
        }

        public uint TotalTime;
        public uint LevelTime;
        public bool TriggerEvent;
    }

    class TogglePvP : ClientPacket
    {
        public TogglePvP(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    class SetPvP : ClientPacket
    {
        public SetPvP(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Enable = _worldPacket.HasBit();
        }

        public bool Enable;
    }

    public class SetActionButton : ClientPacket
    {
        public SetActionButton(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Action = _worldPacket.ReadUInt16();
            Type = _worldPacket.ReadUInt16();
            Index = _worldPacket.ReadUInt8();
        }

        public ushort Action;
        public ushort Type;
        public byte Index;
    }

    public class SetActionBarToggles : ClientPacket
    {
        public SetActionBarToggles(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mask = _worldPacket.ReadUInt8();
        }

        public byte Mask;
    }

    public class LevelUpInfo : ServerPacket
    {
        public LevelUpInfo() : base(Opcode.SMSG_LEVEL_UP_INFO) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Level);
            _worldPacket.WriteInt32(HealthDelta);

            for (int i = 0; i < ModernVersion.GetPowerCountForClientVersion(); i++)
                _worldPacket.WriteInt32(PowerDelta[i]);

            foreach (int stat in StatDelta)
                _worldPacket.WriteInt32(stat);

            _worldPacket.WriteInt32(NumNewTalents);
            _worldPacket.WriteInt32(NumNewPvpTalentSlots);
        }

        public int Level = 0;
        public int HealthDelta = 0;
        public int[] PowerDelta = new int[7];
        public int[] StatDelta = new int[5];
        public int NumNewTalents;
        public int NumNewPvpTalentSlots;
    }

    class UnlearnSkill : ClientPacket
    {
        public UnlearnSkill(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SkillLine = _worldPacket.ReadUInt32();
        }

        public uint SkillLine;
    }

    class PlayerShowingHelmOrCloak : ClientPacket
    {
        public PlayerShowingHelmOrCloak(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            _worldPacket.ResetBitPos();
            Showing = _worldPacket.HasBit();
        }

        public bool Showing;
    }

    public class Inspect : ClientPacket
    {
        public Inspect(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Target = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Target;
    }

    public class InspectResult : ServerPacket
    {
        public InspectResult() : base(Opcode.SMSG_INSPECT_RESULT) { }

        public override void Write()
        {
            DisplayInfo.Write(_worldPacket);
            _worldPacket.WriteInt32(Glyphs.Count);
            _worldPacket.WriteInt32(Talents.Count);
            _worldPacket.WriteInt32(ItemLevel);
            _worldPacket.WriteUInt8(LifetimeMaxRank);
            _worldPacket.WriteUInt16(TodayHK);
            _worldPacket.WriteUInt16(YesterdayHK);
            _worldPacket.WriteUInt32(LifetimeHK);
            _worldPacket.WriteUInt32(HonorLevel);

            for (int i = 0; i < Glyphs.Count; ++i)
                _worldPacket.WriteUInt16(Glyphs[i]);

            for (int i = 0; i < Talents.Count; ++i)
                _worldPacket.WriteUInt8(Talents[i]);

            _worldPacket.WriteBit(GuildData != null);
            _worldPacket.WriteBit(AzeriteLevel.HasValue);
            _worldPacket.FlushBits();

            foreach (PVPBracketData bracket in Bracket)
                bracket.Write(_worldPacket);

            if (GuildData != null)
                GuildData.Write(_worldPacket);

            if (AzeriteLevel.HasValue)
                _worldPacket.WriteUInt32((uint)AzeriteLevel);
        }

        public PlayerModelDisplayInfo DisplayInfo = new();
        public List<ushort> Glyphs = new();
        public List<byte> Talents = new();
        public InspectGuildData GuildData;
        public Array<PVPBracketData> Bracket = new(6, default);
        public uint? AzeriteLevel;
        public int ItemLevel;
        public uint LifetimeHK;
        public uint HonorLevel = 1;
        public ushort TodayHK;
        public ushort YesterdayHK;
        public byte LifetimeMaxRank;
    }

    public class PlayerModelDisplayInfo
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(GUID);
            data.WriteUInt32(SpecializationID);
            data.WriteInt32(Items.Count);
            data.WriteBits(Name.GetByteCount(), 6);
            data.WriteUInt8((byte)SexId);
            data.WriteUInt8((byte)RaceId);
            data.WriteUInt8((byte)ClassId);
            data.WriteInt32(Customizations.Count);
            data.WriteString(Name);

            foreach (var customization in Customizations)
            {
                data.WriteUInt32(customization.ChrCustomizationOptionID);
                data.WriteUInt32(customization.ChrCustomizationChoiceID);
            }

            foreach (InspectItemData item in Items)
                item.Write(data);
        }

        public WowGuid128 GUID;
        public List<InspectItemData> Items = new();
        public string Name;
        public uint SpecializationID;
        public Gender SexId;
        public Race RaceId;
        public Class ClassId;
        public List<ChrCustomizationChoice> Customizations = new();

    }

    public class InspectItemData
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(CreatorGUID);
            data.WriteUInt8(Index);
            data.WriteInt32(AzeritePowers.Count);
            data.WriteInt32(AzeriteEssences.Count);
            foreach (var id in AzeritePowers)
                data.WriteInt32(id);

            Item.Write(data);
            data.WriteBit(Usable);
            data.WriteBits(Enchants.Count, 4);
            data.WriteBits(Gems.Count, 2);
            data.FlushBits();

            foreach (var azeriteEssenceData in AzeriteEssences)
                azeriteEssenceData.Write(data);

            foreach (var enchantData in Enchants)
                enchantData.Write(data);

            foreach (var gem in Gems)
                gem.Write(data);
        }

        public WowGuid128 CreatorGUID = WowGuid128.Empty;
        public ItemInstance Item = new();
        public byte Index;
        public bool Usable;
        public List<InspectEnchantData> Enchants = new();
        public List<ItemGemData> Gems = new();
        public List<int> AzeritePowers = new();
        public List<AzeriteEssenceData> AzeriteEssences = new();
    }

    public struct InspectEnchantData
    {
        public InspectEnchantData(uint id, byte index)
        {
            Id = id;
            Index = index;
        }

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Id);
            data.WriteUInt8(Index);
        }

        public uint Id;
        public byte Index;
    }

    public struct AzeriteEssenceData
    {
        public uint Index;
        public uint AzeriteEssenceID;
        public uint Rank;
        public bool SlotUnlocked;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Index);
            data.WriteUInt32(AzeriteEssenceID);
            data.WriteUInt32(Rank);
            data.WriteBit(SlotUnlocked);
            data.FlushBits();
        }
    }

    public class InspectGuildData
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(GuildGUID);
            data.WriteInt32(NumGuildMembers);
            data.WriteInt32(AchievementPoints);
        }

        public WowGuid128 GuildGUID = WowGuid128.Empty;
        public int NumGuildMembers;
        public int AchievementPoints;
    }

    public struct PVPBracketData
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Bracket);
            data.WriteInt32(Rating);
            data.WriteInt32(Rank);
            data.WriteInt32(WeeklyPlayed);
            data.WriteInt32(WeeklyWon);
            data.WriteInt32(SeasonPlayed);
            data.WriteInt32(SeasonWon);
            data.WriteInt32(WeeklyBestRating);
            data.WriteInt32(SeasonBestRating);
            data.WriteInt32(PvpTierID);
            data.WriteInt32(UnkBCC);
            data.WriteBit(Disqualified);
            data.FlushBits();
        }

        public int Rating;
        public int Rank;
        public int WeeklyPlayed;
        public int WeeklyWon;
        public int SeasonPlayed;
        public int SeasonWon;
        public int WeeklyBestRating;
        public int SeasonBestRating;
        public int PvpTierID;
        public int UnkBCC;
        public byte Bracket;
        public bool Disqualified;
    }

    public class InspectHonorStatsResultClassic : ServerPacket
    {
        public InspectHonorStatsResultClassic() : base(Opcode.SMSG_INSPECT_HONOR_STATS) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PlayerGUID);
            _worldPacket.WriteUInt8(LifetimeHighestRank);
            _worldPacket.WriteUInt16(TodayHonorableKills);
            _worldPacket.WriteUInt16(TodayDishonorableKills);
            _worldPacket.WriteUInt16(YesterdayHonorableKills);
            _worldPacket.WriteUInt16(YesterdayDishonorableKills);
            _worldPacket.WriteUInt16(LastWeekHonorableKills);
            _worldPacket.WriteUInt16(LastWeekDishonorableKills);
            _worldPacket.WriteUInt16(ThisWeekHonorableKills);
            _worldPacket.WriteUInt16(ThisWeekDishonorableKills);
            _worldPacket.WriteUInt32(LifetimeHonorableKills);
            _worldPacket.WriteUInt32(LifetimeDishonorableKills);
            _worldPacket.WriteUInt32(YesterdayHonor);
            _worldPacket.WriteUInt32(LastWeekHonor);
            _worldPacket.WriteUInt32(ThisWeekHonor);
            _worldPacket.WriteUInt32(Standing);
            _worldPacket.WriteUInt8(RankProgress);
        }

        public WowGuid128 PlayerGUID;
        public byte LifetimeHighestRank;
        public ushort TodayHonorableKills;
        public ushort TodayDishonorableKills;
        public ushort YesterdayHonorableKills;
        public ushort YesterdayDishonorableKills;
        public ushort LastWeekHonorableKills;
        public ushort LastWeekDishonorableKills;
        public ushort ThisWeekHonorableKills;
        public ushort ThisWeekDishonorableKills;
        public uint LifetimeHonorableKills;
        public uint LifetimeDishonorableKills;
        public uint YesterdayHonor;
        public uint LastWeekHonor;
        public uint ThisWeekHonor;
        public uint Standing;
        public byte RankProgress;
    }

    public class InspectHonorStatsResultTBC : ServerPacket
    {
        public InspectHonorStatsResultTBC() : base(Opcode.SMSG_INSPECT_HONOR_STATS) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PlayerGUID);
            _worldPacket.WriteUInt8(LifetimeHighestRank);
            _worldPacket.WriteUInt16(Unused1);
            _worldPacket.WriteUInt16(YesterdayHonorableKills);
            _worldPacket.WriteUInt16(Unused3);
            _worldPacket.WriteUInt16(LifetimeHonorableKills);
            _worldPacket.WriteUInt32(Unused4);
            _worldPacket.WriteUInt32(Unused5);
            _worldPacket.WriteUInt32(Unused6);
            _worldPacket.WriteUInt32(Unused7);
            _worldPacket.WriteUInt32(Unused8);
            _worldPacket.WriteUInt8(Unused9);
        }

        public WowGuid128 PlayerGUID;
        public byte LifetimeHighestRank;
        public ushort Unused1;
        public ushort YesterdayHonorableKills;
        public ushort Unused3;
        public ushort LifetimeHonorableKills;
        public uint Unused4;
        public uint Unused5;
        public uint Unused6;
        public uint Unused7;
        public uint Unused8;
        public byte Unused9;
    }

    public class InspectPvP : ServerPacket
    {
        public InspectPvP() : base(Opcode.SMSG_INSPECT_PVP) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PlayerGUID);
            _worldPacket.WriteBits(Brackets.Count, 3);
            _worldPacket.WriteBits(ArenaTeams.Count, 2);
            _worldPacket.FlushBits();

            foreach (var bracket in Brackets)
                bracket.Write(_worldPacket);

            foreach (var team in ArenaTeams)
                team.Write(_worldPacket);
        }

        public WowGuid128 PlayerGUID;
        public List<PvPBracketInspectData> Brackets = new List<PvPBracketInspectData>();
        public List<ArenaTeamInspectData> ArenaTeams = new List<ArenaTeamInspectData>();
    }

    public class PvPBracketInspectData
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Bracket);
            data.WriteInt32(Rating);
            data.WriteInt32(Rank);
            data.WriteInt32(WeeklyPlayed);
            data.WriteInt32(WeeklyWon);
            data.WriteInt32(SeasonPlayed);
            data.WriteInt32(SeasonWon); ;
            data.WriteInt32(WeeklyBestRating);
            data.WriteInt32(SeasonBestRating);
            data.WriteInt32(PvpTierID);
            data.WriteInt32(WeeklyBestWinPvpTierID);
            data.WriteBool(Disqualified);
        }

        public byte Bracket;
        public int Rating;
        public int Rank;
        public int WeeklyPlayed;
        public int WeeklyWon;
        public int SeasonPlayed;
        public int SeasonWon;
        public int WeeklyBestRating;
        public int SeasonBestRating;
        public int PvpTierID;
        public int WeeklyBestWinPvpTierID;
        public bool Disqualified;
    }

    public class ArenaTeamInspectData
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(TeamGuid);
            data.WriteInt32(TeamRating);
            data.WriteInt32(TeamGamesPlayed);
            data.WriteInt32(TeamGamesWon);
            data.WriteInt32(PersonalGamesPlayed);
            data.WriteInt32(PersonalRating);
        }

        public WowGuid128 TeamGuid = WowGuid128.Empty;
        public int TeamRating;
        public int TeamGamesPlayed;
        public int TeamGamesWon;
        public int PersonalGamesPlayed;
        public int PersonalRating;
    }

    public class CharacterRenameRequest : ClientPacket
    {
        public CharacterRenameRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Guid = _worldPacket.ReadPackedGuid128();
            NewName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(6));
        }

        public string NewName;
        public WowGuid128 Guid;
    }

    public class CharacterRenameResult : ServerPacket
    {
        public CharacterRenameResult() : base(Opcode.SMSG_CHARACTER_RENAME_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)Result);
            _worldPacket.WriteBit(Guid != null);
            _worldPacket.WriteBits(Name.GetByteCount(), 6);
            _worldPacket.FlushBits();

            if (Guid != null)
                _worldPacket.WritePackedGuid128(Guid);

            _worldPacket.WriteString(Name);
        }

        public string Name = "";
        public Enums.Classic.ResponseCodes Result = 0;
        public WowGuid128 Guid;
    }
}
