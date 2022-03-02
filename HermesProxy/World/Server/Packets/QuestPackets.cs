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
    public class QuestGiverQueryQuest : ClientPacket
    {
        public QuestGiverQueryQuest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            QuestGiverGUID = _worldPacket.ReadPackedGuid128();
            QuestID = _worldPacket.ReadUInt32();
            RespondToGiver = _worldPacket.HasBit();
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestID;
        public bool RespondToGiver;
    }

    public class QuestGiverQuestDetails : ServerPacket
    {
        public QuestGiverQuestDetails() : base(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)
        {
            for (int i = 0; i < QuestConst.QuestRewardReputationsCount; i++)
                Rewards.FactionCapIn[i] = 7;
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(QuestGiverGUID);
            _worldPacket.WritePackedGuid128(InformUnit);
            _worldPacket.WriteUInt32(QuestID);
            _worldPacket.WriteInt32(QuestPackageID);
            _worldPacket.WriteUInt32(PortraitGiver);
            _worldPacket.WriteUInt32(PortraitGiverMount);
            _worldPacket.WriteUInt32(PortraitGiverModelSceneID);
            _worldPacket.WriteUInt32(PortraitTurnIn);
            _worldPacket.WriteUInt32(QuestFlags[0]); // Flags
            _worldPacket.WriteUInt32(QuestFlags[1]); // FlagsEx
            _worldPacket.WriteUInt32(SuggestedPartyMembers);
            _worldPacket.WriteInt32(LearnSpells.Count);
            _worldPacket.WriteInt32(DescEmotes.Length);
            _worldPacket.WriteInt32(Objectives.Count);
            _worldPacket.WriteInt32(QuestStartItemID);
            _worldPacket.WriteInt32(QuestSessionBonus);

            foreach (uint spell in LearnSpells)
                _worldPacket.WriteUInt32(spell);

            foreach (QuestDescEmote emote in DescEmotes)
            {
                _worldPacket.WriteUInt32(emote.Type);
                _worldPacket.WriteUInt32(emote.Delay);
            }

            foreach (QuestObjectiveSimple obj in Objectives)
            {
                _worldPacket.WriteUInt32(obj.Id);
                _worldPacket.WriteInt32(obj.ObjectID);
                _worldPacket.WriteInt32(obj.Amount);
                _worldPacket.WriteUInt8(obj.Type);
            }

            _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
            _worldPacket.WriteBits(DescriptionText.GetByteCount(), 12);
            _worldPacket.WriteBits(LogDescription.GetByteCount(), 12);
            _worldPacket.WriteBits(PortraitGiverText.GetByteCount(), 10);
            _worldPacket.WriteBits(PortraitGiverName.GetByteCount(), 8);
            _worldPacket.WriteBits(PortraitTurnInText.GetByteCount(), 10);
            _worldPacket.WriteBits(PortraitTurnInName.GetByteCount(), 8);
            _worldPacket.WriteBit(AutoLaunched);
            _worldPacket.WriteBit(false);   // unused in client
            _worldPacket.WriteBit(StartCheat);
            _worldPacket.WriteBit(DisplayPopup);
            _worldPacket.FlushBits();

            Rewards.Write(_worldPacket);

            _worldPacket.WriteString(QuestTitle);
            _worldPacket.WriteString(DescriptionText);
            _worldPacket.WriteString(LogDescription);
            _worldPacket.WriteString(PortraitGiverText);
            _worldPacket.WriteString(PortraitGiverName);
            _worldPacket.WriteString(PortraitTurnInText);
            _worldPacket.WriteString(PortraitTurnInName);
        }

        public WowGuid128 QuestGiverGUID;
        public WowGuid128 InformUnit;
        public uint QuestID;
        public int QuestPackageID;
        public uint[] QuestFlags = new uint[2];
        public uint SuggestedPartyMembers;
        public QuestRewards Rewards = new();
        public List<QuestObjectiveSimple> Objectives = new();
        public QuestDescEmote[] DescEmotes = new QuestDescEmote[QuestConst.QuestEmoteCount];
        public List<uint> LearnSpells = new();
        public uint PortraitTurnIn;
        public uint PortraitGiver;
        public uint PortraitGiverMount;
        public uint PortraitGiverModelSceneID;
        public int QuestStartItemID;
        public int QuestSessionBonus;
        public string PortraitGiverText = "";
        public string PortraitGiverName = "";
        public string PortraitTurnInText = "";
        public string PortraitTurnInName = "";
        public string QuestTitle = "";
        public string DescriptionText = "";
        public string LogDescription = "";
        public bool DisplayPopup;
        public bool StartCheat;
        public bool AutoLaunched;
    }

    public class QuestRewards
    {
        public QuestRewards()
        {
            for (int i = 0; i < QuestConst.QuestRewardChoicesCount; i++)
                ChoiceItems[i] = new();
        }
        public uint ChoiceItemCount;
        public uint ItemCount;
        public uint Money;
        public uint XP;
        public uint ArtifactXP;
        public uint ArtifactCategoryID;
        public uint Honor;
        public uint Title;
        public uint FactionFlags;
        public int[] SpellCompletionDisplayID = new int[QuestConst.QuestRewardDisplaySpellCount];
        public uint SpellCompletionID;
        public uint SkillLineID;
        public uint NumSkillUps;
        public uint TreasurePickerID;
        public QuestChoiceItem[] ChoiceItems = new QuestChoiceItem[QuestConst.QuestRewardChoicesCount];
        public uint[] ItemID = new uint[QuestConst.QuestRewardItemCount];
        public uint[] ItemQty = new uint[QuestConst.QuestRewardItemCount];
        public uint[] FactionID = new uint[QuestConst.QuestRewardReputationsCount];
        public int[] FactionValue = new int[QuestConst.QuestRewardReputationsCount];
        public int[] FactionOverride = new int[QuestConst.QuestRewardReputationsCount];
        public int[] FactionCapIn = new int[QuestConst.QuestRewardReputationsCount];
        public uint[] CurrencyID = new uint[QuestConst.QuestRewardCurrencyCount];
        public uint[] CurrencyQty = new uint[QuestConst.QuestRewardCurrencyCount];
        public bool IsBoostSpell;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(ChoiceItemCount);
            data.WriteUInt32(ItemCount);

            for (int i = 0; i < QuestConst.QuestRewardItemCount; ++i)
            {
                data.WriteUInt32(ItemID[i]);
                data.WriteUInt32(ItemQty[i]);
            }

            data.WriteUInt32(Money);
            data.WriteUInt32(XP);
            data.WriteUInt64(ArtifactXP);
            data.WriteUInt32(ArtifactCategoryID);
            data.WriteUInt32(Honor);
            data.WriteUInt32(Title);
            data.WriteUInt32(FactionFlags);

            for (int i = 0; i < QuestConst.QuestRewardReputationsCount; ++i)
            {
                data.WriteUInt32(FactionID[i]);
                data.WriteInt32(FactionValue[i]);
                data.WriteInt32(FactionOverride[i]);
                data.WriteInt32(FactionCapIn[i]);
            }

            foreach (var id in SpellCompletionDisplayID)
                data.WriteInt32(id);

            data.WriteUInt32(SpellCompletionID);

            for (int i = 0; i < QuestConst.QuestRewardCurrencyCount; ++i)
            {
                data.WriteUInt32(CurrencyID[i]);
                data.WriteUInt32(CurrencyQty[i]);
            }

            data.WriteUInt32(SkillLineID);
            data.WriteUInt32(NumSkillUps);
            data.WriteUInt32(TreasurePickerID);

            foreach (var choice in ChoiceItems)
                choice.Write(data);

            data.WriteBit(IsBoostSpell);
            data.FlushBits();
        }
    }

    public class QuestChoiceItem
    {
        public byte LootItemType;
        public ItemInstance Item = new();
        public uint Quantity;

        public void Read(WorldPacket data)
        {
            data.ResetBitPos();
            LootItemType = data.ReadBits<byte>(2);
            Item.Read(data);
            Quantity = data.ReadUInt32();
        }

        public void Write(WorldPacket data)
        {
            data.WriteBits(LootItemType, 2);
            Item.Write(data);
            data.WriteUInt32(Quantity);
        }
    }

    public struct QuestObjectiveSimple
    {
        public uint Id;
        public int ObjectID;
        public int Amount;
        public byte Type;
    }

    public struct QuestDescEmote
    {
        public uint Type;
        public uint Delay;
    }

    public class QuestGiverAcceptQuest : ClientPacket
    {
        public QuestGiverAcceptQuest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            QuestGiverGUID = _worldPacket.ReadPackedGuid128();
            QuestID = _worldPacket.ReadUInt32();
            StartCheat = _worldPacket.HasBit();
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestID;
        public bool StartCheat;

    }

    public class QuestLogRemoveQuest : ClientPacket
    {
        public QuestLogRemoveQuest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Slot = _worldPacket.ReadUInt8();
        }

        public byte Slot;
    }

    public class QuestGiverStatusQuery : ClientPacket
    {
        public QuestGiverStatusQuery(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 QuestGiverGUID;
    }

    public class QuestGiverStatusMultipleQuery : ClientPacket
    {
        public QuestGiverStatusMultipleQuery(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class QuestGiverStatusPkt : ServerPacket
    {
        public QuestGiverStatusPkt() : base(Opcode.SMSG_QUEST_GIVER_STATUS, ConnectionType.Instance)
        {
            QuestGiver = new QuestGiverInfo();
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(QuestGiver.Guid);
            _worldPacket.WriteUInt32((uint)QuestGiver.Status);
        }

        public QuestGiverInfo QuestGiver;
    }

    public class QuestGiverStatusMultiple : ServerPacket
    {
        public QuestGiverStatusMultiple() : base(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(QuestGivers.Count);
            foreach (QuestGiverInfo questGiver in QuestGivers)
            {
                _worldPacket.WritePackedGuid128(questGiver.Guid);
                _worldPacket.WriteUInt32((uint)questGiver.Status);
            }
        }

        public List<QuestGiverInfo> QuestGivers = new();
    }

    public class QuestGiverInfo
    {
        public QuestGiverInfo() { }
        public QuestGiverInfo(WowGuid128 guid, QuestGiverStatusModern status)
        {
            Guid = guid;
            Status = status;
        }

        public WowGuid128 Guid;
        public QuestGiverStatusModern Status = QuestGiverStatusModern.None;
    }

    public class QuestGiverHello : ClientPacket
    {
        public QuestGiverHello(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            QuestGiverGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 QuestGiverGUID;
    }

    public class QuestGiverQuestListMessage : ServerPacket
    {
        public QuestGiverQuestListMessage() : base(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(QuestGiverGUID);
            _worldPacket.WriteUInt32(GreetEmoteDelay);
            _worldPacket.WriteUInt32(GreetEmoteType);
            _worldPacket.WriteInt32(QuestOptions.Count);
            _worldPacket.WriteBits(Greeting.GetByteCount(), 11);
            _worldPacket.FlushBits();

            foreach (ClientGossipQuest quest in QuestOptions)
                quest.Write(_worldPacket);

            _worldPacket.WriteString(Greeting);
        }

        public WowGuid128 QuestGiverGUID;
        public uint GreetEmoteDelay;
        public uint GreetEmoteType;
        public List<ClientGossipQuest> QuestOptions = new();
        public string Greeting = "";
    }

    public class QuestGiverRequestItems : ServerPacket
    {
        public QuestGiverRequestItems() : base(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(QuestGiverGUID);
            _worldPacket.WriteUInt32(QuestGiverCreatureID);
            _worldPacket.WriteUInt32(QuestID);
            _worldPacket.WriteUInt32(CompEmoteDelay);
            _worldPacket.WriteUInt32(CompEmoteType);
            _worldPacket.WriteUInt32(QuestFlags[0]);
            _worldPacket.WriteUInt32(QuestFlags[1]);
            _worldPacket.WriteUInt32(SuggestPartyMembers);
            _worldPacket.WriteInt32(MoneyToGet);
            _worldPacket.WriteInt32(Collect.Count);
            _worldPacket.WriteInt32(Currency.Count);
            _worldPacket.WriteUInt32(StatusFlags);

            foreach (QuestObjectiveCollect obj in Collect)
            {
                _worldPacket.WriteUInt32(obj.ObjectID);
                _worldPacket.WriteUInt32(obj.Amount);
                _worldPacket.WriteUInt32(obj.Flags);
            }
            foreach (QuestCurrency cur in Currency)
            {
                _worldPacket.WriteUInt32(cur.CurrencyID);
                _worldPacket.WriteInt32(cur.Amount);
            }

            _worldPacket.WriteBit(AutoLaunched);
            _worldPacket.FlushBits();

            _worldPacket.WriteBits(QuestTitle.GetByteCount(), 9);
            _worldPacket.WriteBits(CompletionText.GetByteCount(), 12);

            _worldPacket.WriteString(QuestTitle);
            _worldPacket.WriteString(CompletionText);
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestGiverCreatureID;
        public uint QuestID;
        public uint CompEmoteDelay;
        public uint CompEmoteType;
        public bool AutoLaunched;
        public uint SuggestPartyMembers;
        public int MoneyToGet;
        public List<QuestObjectiveCollect> Collect = new();
        public List<QuestCurrency> Currency = new();
        public uint StatusFlags;
        public uint[] QuestFlags = new uint[2];
        public string QuestTitle = "";
        public string CompletionText = "";
    }

    public struct QuestObjectiveCollect
    {
        public uint ObjectID;
        public uint Amount;
        public uint Flags;
    }

    public struct QuestCurrency
    {
        public uint CurrencyID;
        public int Amount;
    }
}
