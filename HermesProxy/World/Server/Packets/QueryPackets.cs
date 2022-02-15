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
using HermesProxy.World.Objects;
using Framework.Collections;
using Framework.Constants;
using System.Collections.Generic;
using Framework.IO;

namespace HermesProxy.World.Server.Packets
{
    public class QueryPlayerName : ClientPacket
    {
        public QueryPlayerName(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Player = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Player;
    }

    public class QueryPlayerNameResponse : ServerPacket
    {
        public QueryPlayerNameResponse() : base(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)
        {
            Data = new PlayerGuidLookupData();
        }

        public override void Write()
        {
            _worldPacket.WriteInt8((sbyte)Result);
            _worldPacket.WritePackedGuid128(Player);

            if (Result == HermesProxy.World.Objects.Classic.ResponseCodes.Success)
                Data.Write(_worldPacket);
        }

        public WowGuid128 Player;
        public HermesProxy.World.Objects.Classic.ResponseCodes Result; // 0 - full packet, != 0 - only guid
        public PlayerGuidLookupData Data;
    }

    public class PlayerGuidLookupData
    {
        public void Write(WorldPacket data)
        {
            data.WriteBit(IsDeleted);
            data.WriteBits(Name.GetByteCount(), 6);

            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                data.WriteBits(DeclinedNames.name[i].GetByteCount(), 7);

            data.FlushBits();
            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                data.WriteString(DeclinedNames.name[i]);

            data.WritePackedGuid128(AccountID);
            data.WritePackedGuid128(BnetAccountID);
            data.WritePackedGuid128(GuidActual);
            data.WriteUInt64(GuildClubMemberID);
            data.WriteUInt32(VirtualRealmAddress);
            data.WriteUInt8((byte)RaceID);
            data.WriteUInt8((byte)Sex);
            data.WriteUInt8((byte)ClassID);
            data.WriteUInt8(Level);
            data.WriteUInt8(Unused915);
            data.WriteString(Name);
        }

        public bool IsDeleted;
        public WowGuid128 AccountID;
        public WowGuid128 BnetAccountID;
        public WowGuid128 GuidActual;
        public string Name = "";
        public ulong GuildClubMemberID;   // same as bgs.protocol.club.v1.MemberId.unique_id
        public uint VirtualRealmAddress;
        public Race RaceID = Race.None;
        public Gender Sex = Gender.None;
        public Class ClassID = Class.None;
        public byte Level;
        public byte Unused915;
        public DeclinedName DeclinedNames = new();
    }

    public class DeclinedName
    {
        public StringArray name = new(PlayerConst.MaxDeclinedNameCases);
    }

    public class QueryQuestInfo : ClientPacket
    {
        public QueryQuestInfo(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            QuestID = _worldPacket.ReadUInt32();
            QuestGiver = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 QuestGiver;
        public uint QuestID;
    }

    public class QueryQuestInfoResponse : ServerPacket
    {
        public QueryQuestInfoResponse() : base(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(QuestID);
            _worldPacket.WriteBit(Allow);
            _worldPacket.FlushBits();

            if (Allow)
            {
                _worldPacket.WriteUInt32(Info.QuestID);
                _worldPacket.WriteInt32(Info.QuestType);
                _worldPacket.WriteInt32(Info.QuestLevel);
                _worldPacket.WriteInt32(Info.QuestScalingFactionGroup);
                _worldPacket.WriteInt32(Info.QuestMaxScalingLevel);
                _worldPacket.WriteUInt32(Info.QuestPackageID);
                _worldPacket.WriteInt32(Info.MinLevel);
                _worldPacket.WriteInt32(Info.QuestSortID);
                _worldPacket.WriteUInt32(Info.QuestInfoID);
                _worldPacket.WriteUInt32(Info.SuggestedGroupNum);
                _worldPacket.WriteUInt32(Info.RewardNextQuest);
                _worldPacket.WriteUInt32(Info.RewardXPDifficulty);

                _worldPacket.WriteFloat(Info.RewardXPMultiplier);

                _worldPacket.WriteInt32(Info.RewardMoney);
                _worldPacket.WriteUInt32(Info.RewardMoneyDifficulty);
                _worldPacket.WriteFloat(Info.RewardMoneyMultiplier);
                _worldPacket.WriteUInt32(Info.RewardBonusMoney);

                for (uint i = 0; i < QuestConst.QuestRewardDisplaySpellCount; ++i)
                    _worldPacket.WriteUInt32(Info.RewardDisplaySpell[i]);

                _worldPacket.WriteUInt32(Info.RewardSpell);
                _worldPacket.WriteUInt32(Info.RewardHonor);

                _worldPacket.WriteFloat(Info.RewardKillHonor);

                _worldPacket.WriteInt32(Info.RewardArtifactXPDifficulty);
                _worldPacket.WriteFloat(Info.RewardArtifactXPMultiplier);
                _worldPacket.WriteInt32(Info.RewardArtifactCategoryID);

                _worldPacket.WriteUInt32(Info.StartItem);
                _worldPacket.WriteUInt32(Info.Flags);
                _worldPacket.WriteUInt32(Info.FlagsEx);
                _worldPacket.WriteUInt32(Info.FlagsEx2);

                for (uint i = 0; i < QuestConst.QuestRewardItemCount; ++i)
                {
                    _worldPacket.WriteUInt32(Info.RewardItems[i]);
                    _worldPacket.WriteUInt32(Info.RewardAmount[i]);
                    _worldPacket.WriteInt32(Info.ItemDrop[i]);
                    _worldPacket.WriteInt32(Info.ItemDropQuantity[i]);
                }

                for (uint i = 0; i < QuestConst.QuestRewardChoicesCount; ++i)
                {
                    _worldPacket.WriteUInt32(Info.UnfilteredChoiceItems[i].ItemID);
                    _worldPacket.WriteUInt32(Info.UnfilteredChoiceItems[i].Quantity);
                    _worldPacket.WriteUInt32(Info.UnfilteredChoiceItems[i].DisplayID);
                }

                _worldPacket.WriteUInt32(Info.POIContinent);
                _worldPacket.WriteFloat(Info.POIx);
                _worldPacket.WriteFloat(Info.POIy);
                _worldPacket.WriteUInt32(Info.POIPriority);

                _worldPacket.WriteUInt32(Info.RewardTitle);
                _worldPacket.WriteInt32(Info.RewardArenaPoints);
                _worldPacket.WriteUInt32(Info.RewardSkillLineID);
                _worldPacket.WriteUInt32(Info.RewardNumSkillUps);

                _worldPacket.WriteUInt32(Info.PortraitGiver);
                _worldPacket.WriteUInt32(Info.PortraitGiverMount);
                _worldPacket.WriteUInt32(Info.PortraitTurnIn);

                _worldPacket.WriteInt32(0); // Unk 2.5.2

                for (uint i = 0; i < QuestConst.QuestRewardReputationsCount; ++i)
                {
                    _worldPacket.WriteUInt32(Info.RewardFactionID[i]);
                    _worldPacket.WriteInt32(Info.RewardFactionValue[i]);
                    _worldPacket.WriteInt32(Info.RewardFactionOverride[i]);
                    _worldPacket.WriteInt32(Info.RewardFactionCapIn[i]);
                }

                _worldPacket.WriteUInt32(Info.RewardFactionFlags);

                for (uint i = 0; i < QuestConst.QuestRewardCurrencyCount; ++i)
                {
                    _worldPacket.WriteUInt32(Info.RewardCurrencyID[i]);
                    _worldPacket.WriteUInt32(Info.RewardCurrencyQty[i]);
                }

                _worldPacket.WriteUInt32(Info.AcceptedSoundKitID);
                _worldPacket.WriteUInt32(Info.CompleteSoundKitID);

                _worldPacket.WriteUInt32(Info.AreaGroupID);
                _worldPacket.WriteUInt32(Info.TimeAllowed);

                _worldPacket.WriteInt32(Info.Objectives.Count);
                _worldPacket.WriteInt64(Info.AllowableRaces);
                _worldPacket.WriteInt32(Info.TreasurePickerID);
                _worldPacket.WriteInt32(Info.Expansion);

                _worldPacket.WriteBits(Info.LogTitle.GetByteCount(), 9);
                _worldPacket.WriteBits(Info.LogDescription.GetByteCount(), 12);
                _worldPacket.WriteBits(Info.QuestDescription.GetByteCount(), 12);
                _worldPacket.WriteBits(Info.AreaDescription.GetByteCount(), 9);
                _worldPacket.WriteBits(Info.PortraitGiverText.GetByteCount(), 10);
                _worldPacket.WriteBits(Info.PortraitGiverName.GetByteCount(), 8);
                _worldPacket.WriteBits(Info.PortraitTurnInText.GetByteCount(), 10);
                _worldPacket.WriteBits(Info.PortraitTurnInName.GetByteCount(), 8);
                _worldPacket.WriteBits(Info.QuestCompletionLog.GetByteCount(), 11);
                _worldPacket.WriteBit(Info.ReadyForTranslation);
                _worldPacket.FlushBits();

                foreach (QuestObjective questObjective in Info.Objectives)
                {
                    _worldPacket.WriteUInt32(questObjective.Id);
                    _worldPacket.WriteUInt8((byte)questObjective.Type);
                    _worldPacket.WriteInt8(questObjective.StorageIndex);
                    _worldPacket.WriteInt32(questObjective.ObjectID);
                    _worldPacket.WriteInt32(questObjective.Amount);
                    _worldPacket.WriteUInt32((uint)questObjective.Flags);
                    _worldPacket.WriteUInt32(questObjective.Flags2);
                    _worldPacket.WriteFloat(questObjective.ProgressBarWeight);

                    _worldPacket.WriteInt32(questObjective.VisualEffects.Length);
                    foreach (var visualEffect in questObjective.VisualEffects)
                        _worldPacket.WriteInt32(visualEffect);

                    _worldPacket.WriteBits(questObjective.Description.GetByteCount(), 8);
                    _worldPacket.FlushBits();

                    _worldPacket.WriteString(questObjective.Description);
                }

                _worldPacket.WriteString(Info.LogTitle);
                _worldPacket.WriteString(Info.LogDescription);
                _worldPacket.WriteString(Info.QuestDescription);
                _worldPacket.WriteString(Info.AreaDescription);
                _worldPacket.WriteString(Info.PortraitGiverText);
                _worldPacket.WriteString(Info.PortraitGiverName);
                _worldPacket.WriteString(Info.PortraitTurnInText);
                _worldPacket.WriteString(Info.PortraitTurnInName);
                _worldPacket.WriteString(Info.QuestCompletionLog);
            }
        }

        public bool Allow;
        public QuestInfo Info = new();
        public uint QuestID;
    }

    public class QuestInfo
    {
        public QuestInfo()
        {
            LogTitle = "";
            LogDescription = "";
            QuestDescription = "";
            AreaDescription = "";
            PortraitGiverText = "";
            PortraitGiverName = "";
            PortraitTurnInText = "";
            PortraitTurnInName = "";
            QuestCompletionLog = "";
        }

        public uint QuestID;
        public int QuestType; // Accepted values: 0, 1 or 2. 0 == IsAutoComplete() (skip objectives/details)
        public int QuestLevel;
        public int QuestScalingFactionGroup;
        public int QuestMaxScalingLevel;
        public uint QuestPackageID;
        public int MinLevel;
        public int QuestSortID; // zone or sort to display in quest log
        public uint QuestInfoID;
        public uint SuggestedGroupNum;
        public uint RewardNextQuest; // client will request this quest from NPC, if not 0
        public uint RewardXPDifficulty; // used for calculating rewarded experience
        public float RewardXPMultiplier = 1.0f;
        public int RewardMoney; // reward money (below max lvl)
        public uint RewardMoneyDifficulty;
        public float RewardMoneyMultiplier = 1.0f;
        public uint RewardBonusMoney;
        public uint[] RewardDisplaySpell = new uint[QuestConst.QuestRewardDisplaySpellCount]; // reward spell, this spell will be displayed (icon)
        public uint RewardSpell;
        public uint RewardHonor;
        public float RewardKillHonor;
        public int RewardArtifactXPDifficulty;
        public float RewardArtifactXPMultiplier;
        public int RewardArtifactCategoryID;
        public uint StartItem;
        public uint Flags;
        public uint FlagsEx;
        public uint FlagsEx2;
        public uint POIContinent;
        public float POIx;
        public float POIy;
        public uint POIPriority;
        public long AllowableRaces = -1;
        public string LogTitle;
        public string LogDescription;
        public string QuestDescription;
        public string AreaDescription;
        public uint RewardTitle; // new 2.4.0, player gets this title (id from CharTitles)
        public int RewardArenaPoints;
        public uint RewardSkillLineID; // reward skill id
        public uint RewardNumSkillUps; // reward skill points
        public uint PortraitGiver; // quest giver entry ?
        public uint PortraitGiverMount;
        public uint PortraitTurnIn; // quest turn in entry ?
        public string PortraitGiverText;
        public string PortraitGiverName;
        public string PortraitTurnInText;
        public string PortraitTurnInName;
        public string QuestCompletionLog;
        public uint RewardFactionFlags; // rep mask (unsure on what it does)
        public uint AcceptedSoundKitID;
        public uint CompleteSoundKitID;
        public uint AreaGroupID;
        public uint TimeAllowed;
        public int TreasurePickerID;
        public int Expansion;
        public List<QuestObjective> Objectives = new();
        public uint[] RewardItems = new uint[QuestConst.QuestRewardItemCount];
        public uint[] RewardAmount = new uint[QuestConst.QuestRewardItemCount];
        public int[] ItemDrop = new int[QuestConst.QuestItemDropCount];
        public int[] ItemDropQuantity = new int[QuestConst.QuestItemDropCount];
        public QuestInfoChoiceItem[] UnfilteredChoiceItems = new QuestInfoChoiceItem[QuestConst.QuestRewardChoicesCount];
        public uint[] RewardFactionID = new uint[QuestConst.QuestRewardReputationsCount];
        public int[] RewardFactionValue = new int[QuestConst.QuestRewardReputationsCount];
        public int[] RewardFactionOverride = new int[QuestConst.QuestRewardReputationsCount];
        public int[] RewardFactionCapIn = new int[QuestConst.QuestRewardReputationsCount];
        public uint[] RewardCurrencyID = new uint[QuestConst.QuestRewardCurrencyCount];
        public uint[] RewardCurrencyQty = new uint[QuestConst.QuestRewardCurrencyCount];
        public bool ReadyForTranslation;
    }

    public struct QuestInfoChoiceItem
    {
        public uint ItemID;
        public uint Quantity;
        public uint DisplayID;
    }

    public class QuestObjective
    {
        public static uint QuestObjectiveCounter = 1;

        public uint Id;
        public uint QuestID;
        public QuestObjectiveType Type;
        public sbyte StorageIndex;
        public int ObjectID;
        public int Amount;
        public QuestObjectiveFlags Flags;
        public uint Flags2;
        public float ProgressBarWeight;
        public string Description;
        public int[] VisualEffects = Array.Empty<int>();

        public bool IsStoringValue()
        {
            switch (Type)
            {
                case QuestObjectiveType.Monster:
                case QuestObjectiveType.Item:
                case QuestObjectiveType.GameObject:
                case QuestObjectiveType.TalkTo:
                case QuestObjectiveType.PlayerKills:
                case QuestObjectiveType.WinPvpPetBattles:
                case QuestObjectiveType.HaveCurrency:
                case QuestObjectiveType.ObtainCurrency:
                case QuestObjectiveType.IncreaseReputation:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public bool IsStoringFlag()
        {
            switch (Type)
            {
                case QuestObjectiveType.AreaTrigger:
                case QuestObjectiveType.WinPetBattleAgainstNpc:
                case QuestObjectiveType.DefeatBattlePet:
                case QuestObjectiveType.CriteriaTree:
                case QuestObjectiveType.AreaTriggerEnter:
                case QuestObjectiveType.AreaTriggerExit:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public static bool CanAlwaysBeProgressedInRaid(QuestObjectiveType type)
        {
            switch (type)
            {
                case QuestObjectiveType.Item:
                case QuestObjectiveType.Currency:
                case QuestObjectiveType.LearnSpell:
                case QuestObjectiveType.MinReputation:
                case QuestObjectiveType.MaxReputation:
                case QuestObjectiveType.Money:
                case QuestObjectiveType.HaveCurrency:
                case QuestObjectiveType.IncreaseReputation:
                    return true;
                default:
                    break;
            }
            return false;
        }
    }

    public class QueryCreature : ClientPacket
    {
        public QueryCreature(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            CreatureID = _worldPacket.ReadUInt32();
        }

        public uint CreatureID;
    }

    public class QueryCreatureResponse : ServerPacket
    {
        public QueryCreatureResponse() : base(Opcode.SMSG_QUERY_CREATURE_RESPONSE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(CreatureID);
            _worldPacket.WriteBit(Allow);
            _worldPacket.FlushBits();

            if (Allow)
            {
                _worldPacket.WriteBits(Stats.Title.IsEmpty() ? 0 : Stats.Title.GetByteCount() + 1, 11);
                _worldPacket.WriteBits(Stats.TitleAlt.IsEmpty() ? 0 : Stats.TitleAlt.GetByteCount() + 1, 11);
                _worldPacket.WriteBits(Stats.CursorName.IsEmpty() ? 0 : Stats.CursorName.GetByteCount() + 1, 6);
                _worldPacket.WriteBit(Stats.Civilian);
                _worldPacket.WriteBit(Stats.Leader);

                for (var i = 0; i < CreatureConst.MaxCreatureNames; ++i)
                {
                    _worldPacket.WriteBits(Stats.Name[i].GetByteCount() + 1, 11);
                    _worldPacket.WriteBits(Stats.NameAlt[i].GetByteCount() + 1, 11);
                }

                for (var i = 0; i < CreatureConst.MaxCreatureNames; ++i)
                {
                    if (!string.IsNullOrEmpty(Stats.Name[i]))
                        _worldPacket.WriteCString(Stats.Name[i]);
                    if (!string.IsNullOrEmpty(Stats.NameAlt[i]))
                        _worldPacket.WriteCString(Stats.NameAlt[i]);
                }

                for (var i = 0; i < 2; ++i)
                    _worldPacket.WriteUInt32(Stats.Flags[i]);

                _worldPacket.WriteInt32(Stats.Type);
                _worldPacket.WriteInt32(Stats.Family);
                _worldPacket.WriteInt32(Stats.Classification);
                _worldPacket.WriteUInt32(Stats.PetSpellDataId);

                for (var i = 0; i < CreatureConst.MaxCreatureKillCredit; ++i)
                    _worldPacket.WriteUInt32(Stats.ProxyCreatureID[i]);

                _worldPacket.WriteInt32(Stats.Display.CreatureDisplay.Count);
                _worldPacket.WriteFloat(Stats.Display.TotalProbability);

                foreach (CreatureXDisplay display in Stats.Display.CreatureDisplay)
                {
                    _worldPacket.WriteUInt32(display.CreatureDisplayID);
                    _worldPacket.WriteFloat(display.Scale);
                    _worldPacket.WriteFloat(display.Probability);
                }

                _worldPacket.WriteFloat(Stats.HpMulti);
                _worldPacket.WriteFloat(Stats.EnergyMulti);

                _worldPacket.WriteInt32(Stats.QuestItems.Count);
                _worldPacket.WriteUInt32(Stats.MovementInfoID);
                _worldPacket.WriteInt32(Stats.HealthScalingExpansion);
                _worldPacket.WriteUInt32(Stats.RequiredExpansion);
                _worldPacket.WriteUInt32(Stats.VignetteID);
                _worldPacket.WriteInt32(Stats.Class);
                _worldPacket.WriteInt32(Stats.DifficultyID);
                _worldPacket.WriteInt32(Stats.WidgetSetID);
                _worldPacket.WriteInt32(Stats.WidgetSetUnitConditionID);

                if (!Stats.Title.IsEmpty())
                    _worldPacket.WriteCString(Stats.Title);

                if (!Stats.TitleAlt.IsEmpty())
                    _worldPacket.WriteCString(Stats.TitleAlt);

                if (!Stats.CursorName.IsEmpty())
                    _worldPacket.WriteCString(Stats.CursorName);

                foreach (var questItem in Stats.QuestItems)
                    _worldPacket.WriteUInt32(questItem);
            }
        }

        public bool Allow;
        public CreatureStats Stats = new();
        public uint CreatureID;
    }

    public class CreatureStats
    {
        public string Title;
        public string TitleAlt;
        public string CursorName;
        public int Type;
        public int Family;
        public int Classification;
        public uint PetSpellDataId;
        public CreatureDisplayStats Display = new();
        public float HpMulti;
        public float EnergyMulti;
        public bool Civilian;
        public bool Leader;
        public List<uint> QuestItems = new();
        public uint MovementInfoID;
        public int HealthScalingExpansion;
        public uint RequiredExpansion;
        public uint VignetteID;
        public int Class;
        public int DifficultyID;
        public int WidgetSetID;
        public int WidgetSetUnitConditionID;
        public uint[] Flags = new uint[2];
        public uint[] ProxyCreatureID = new uint[CreatureConst.MaxCreatureKillCredit];
        public StringArray Name = new(CreatureConst.MaxCreatureNames);
        public StringArray NameAlt = new(CreatureConst.MaxCreatureNames);
    }

    public class CreatureXDisplay
    {
        public CreatureXDisplay(uint creatureDisplayID, float displayScale, float probability)
        {
            CreatureDisplayID = creatureDisplayID;
            Scale = displayScale;
            Probability = probability;
        }

        public uint CreatureDisplayID;
        public float Scale = 1.0f;
        public float Probability = 1.0f;
    }

    public class CreatureDisplayStats
    {
        public float TotalProbability;
        public List<CreatureXDisplay> CreatureDisplay = new();
    }

    public class QueryGameObject : ClientPacket
    {
        public QueryGameObject(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            GameObjectID = _worldPacket.ReadUInt32();
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public uint GameObjectID;
        public WowGuid128 Guid;
    }

    public class QueryGameObjectResponse : ServerPacket
    {
        public QueryGameObjectResponse() : base(Opcode.SMSG_QUERY_GAME_OBJECT_RESPONSE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(GameObjectID);
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteBit(Allow);
            _worldPacket.FlushBits();

            ByteBuffer statsData = new();
            if (Allow)
            {
                statsData.WriteUInt32(Stats.Type);
                statsData.WriteUInt32(Stats.DisplayID);
                for (int i = 0; i < 4; i++)
                    statsData.WriteCString(Stats.Name[i]);

                statsData.WriteCString(Stats.IconName);
                statsData.WriteCString(Stats.CastBarCaption);
                statsData.WriteCString(Stats.UnkString);

                for (uint i = 0; i < 34; i++)
                    statsData.WriteInt32(Stats.Data[i]);

                statsData.WriteFloat(Stats.Size);
                statsData.WriteUInt8((byte)Stats.QuestItems.Count);
                foreach (uint questItem in Stats.QuestItems)
                    statsData.WriteUInt32(questItem);

                statsData.WriteUInt32(Stats.ContentTuningId);
            }

            _worldPacket.WriteUInt32(statsData.GetSize());
            if (statsData.GetSize() != 0)
                _worldPacket.WriteBytes(statsData);
        }

        public uint GameObjectID;
        public WowGuid128 Guid;
        public bool Allow;
        public GameObjectStats Stats;
    }

    public class GameObjectStats
    {
        public string[] Name = new string[4];
        public string IconName = "";
        public string CastBarCaption = "";
        public string UnkString = "";
        public uint Type;
        public uint DisplayID;
        public int[] Data = new int[34];
        public float Size = 1;
        public List<uint> QuestItems = new();
        public uint ContentTuningId;
    }
}
