using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)]
        void HandleQuestGiverQuestDetails(WorldPacket packet)
        {
            QuestGiverQuestDetails quest = new();
            quest.QuestGiverGUID = packet.ReadGuid().To128();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                quest.InformUnit = packet.ReadGuid().To128();
            else
                quest.InformUnit = quest.QuestGiverGUID;

            quest.QuestID = packet.ReadUInt32();
            quest.QuestTitle = packet.ReadCString();
            quest.DescriptionText = packet.ReadCString();
            quest.LogDescription = packet.ReadCString();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                quest.AutoLaunched = packet.ReadBool();
            else
                quest.AutoLaunched = packet.ReadUInt32() != 0;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
                quest.QuestFlags[0] = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                quest.SuggestedPartyMembers = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadUInt8(); // Unknown

            if (LegacyVersion.InVersion(ClientVersionBuild.V3_1_0_9767, ClientVersionBuild.V3_3_3a_11723))
            {
                quest.StartCheat = packet.ReadBool();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_2_11403))
                    quest.DisplayPopup = packet.ReadBool();
            }

            if (quest.QuestFlags[0].HasAnyFlag(QuestFlags.HiddenRewards) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_3_5a_12340))
            {
                packet.ReadUInt32(); // Hidden Chosen Items
                packet.ReadUInt32(); // Hidden Items
                quest.Rewards.Money = packet.ReadUInt32(); // Hidden Money

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
                    quest.Rewards.XP = packet.ReadUInt32(); // Hidden XP
            }

            ReadExtraQuestInfo(packet, quest.Rewards, false);

            var emoteCount = packet.ReadUInt32();
            for (var i = 0; i < emoteCount; i++)
            {
                quest.DescEmotes[i].Type = packet.ReadUInt32();
                quest.DescEmotes[i].Delay = packet.ReadUInt32();
            }
            SendPacketToClient(quest);
        }

        void ReadExtraQuestInfo(WorldPacket packet, QuestRewards rewards, bool readFlags)
        {
            rewards.ChoiceItemCount = packet.ReadUInt32();
            for (var i = 0; i < rewards.ChoiceItemCount; i++)
            {
                rewards.ChoiceItems[i].Item.ItemID = packet.ReadUInt32();
                rewards.ChoiceItems[i].Quantity = packet.ReadUInt32();
                packet.ReadUInt32(); // Choice Item Display Id
            }

            var rewardCount = packet.ReadUInt32();
            for (var i = 0; i < rewardCount; i++)
            {
                rewards.ItemID[i] = packet.ReadUInt32();
                rewards.ItemQty[i] = packet.ReadUInt32();
                packet.ReadUInt32(); // Reward Item Display Id
            }

            rewards.Money = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
               rewards.XP = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
                rewards.Honor = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                packet.ReadFloat(); // Honor Multiplier

            if (readFlags)
                packet.ReadUInt32(); // Quest Flags

            rewards.SpellCompletionID = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.ReadUInt32(); // Spell Cast Id

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                rewards.Title = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                rewards.NumSkillUps = packet.ReadUInt32(); // Bonus Talents

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            {
                packet.ReadUInt32(); // Arena Points
                packet.ReadUInt32(); // Unk
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            {
                for (var i = 0; i < 5; i++)
                    rewards.FactionID[i] = packet.ReadUInt32(); // Reputation Faction

                for (var i = 0; i < 5; i++)
                    rewards.FactionValue[i] = packet.ReadInt32(); // Reputation Value Id

                for (var i = 0; i < 5; i++)
                    packet.ReadInt32(); // Reputation Value
            }
        }

        [PacketHandler(Opcode.SMSG_QUEST_GIVER_STATUS)]
        void HandleQuestGiverStatus(WorldPacket packet)
        {
            QuestGiverStatusPkt response = new QuestGiverStatusPkt();
            response.QuestGiver.Guid = packet.ReadGuid().To128();
            response.QuestGiver.Status = LegacyVersion.ConvertQuestGiverStatus(packet.ReadUInt8());
            SendPacketToClient(response);
        }

        [PacketHandler(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE)]
        void HandleQuestGiverStatusMultple(WorldPacket packet)
        {
            QuestGiverStatusMultiple response = new QuestGiverStatusMultiple();
            int count = packet.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                QuestGiverInfo info = new();
                info.Guid = packet.ReadGuid().To128();
                info.Status = LegacyVersion.ConvertQuestGiverStatus(packet.ReadUInt8());
                response.QuestGivers.Add(info);
            }
            SendPacketToClient(response);
        }

        [PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE)]
        void HandleQuestGiverQuestListMessage(WorldPacket packet)
        {
            QuestGiverQuestListMessage quests = new QuestGiverQuestListMessage();
            quests.QuestGiverGUID = packet.ReadGuid().To128();
            quests.Greeting = packet.ReadCString();
            quests.GreetEmoteDelay = packet.ReadUInt32();
            quests.GreetEmoteType = packet.ReadUInt32();

            byte count = packet.ReadUInt8();
            for (int i = 0; i < count; i++)
            {
                ClientGossipQuest quest = ReadGossipQuestOption(packet);
                quests.QuestOptions.Add(quest);
            }
            SendPacketToClient(quests);
        }

        ClientGossipQuest ReadGossipQuestOption(WorldPacket packet)
        {
            ClientGossipQuest quest = new();
            quest.QuestID = packet.ReadUInt32();
            Int32 dialogStatus = packet.ReadInt32();

            if (dialogStatus == 5)
                quest.QuestType = 2; // available
            else
                quest.QuestType = 4; // complete

            quest.QuestLevel = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                quest.QuestFlags = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                quest.Repeatable = packet.ReadBool();

            quest.QuestTitle = packet.ReadCString();
            return quest;
        }

        [PacketHandler(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS)]
        void HandleQuestGiverRequestItems(WorldPacket packet)
        {
            QuestGiverRequestItems quest = new QuestGiverRequestItems();
            quest.QuestGiverGUID = packet.ReadGuid().To128();
            quest.QuestGiverCreatureID = quest.QuestGiverGUID.GetEntry();
            quest.QuestID = packet.ReadUInt32();
            quest.QuestTitle = packet.ReadCString();
            quest.CompletionText = packet.ReadCString();
            quest.CompEmoteDelay = packet.ReadUInt32();
            quest.CompEmoteType = packet.ReadUInt32();
            quest.AutoLaunched = packet.ReadUInt32() != 0;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
                quest.QuestFlags[0] = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                quest.SuggestPartyMembers = packet.ReadUInt32();

            quest.MoneyToGet = packet.ReadInt32();

            uint itemsCount = packet.ReadUInt32();
            for (int i = 0; i < itemsCount; i++)
            {
                QuestObjectiveCollect item = new();
                item.ObjectID = packet.ReadUInt32();
                item.Amount = packet.ReadUInt32();
                packet.ReadUInt32(); // Item Display Id
                quest.Collect.Add(item);
            }

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.ReadUInt32(); // unknown meaning, mangos sends always 2

            // flags
            quest.StatusFlags = packet.ReadUInt32();
            packet.ReadUInt32(); // Unk flags 2
            packet.ReadUInt32(); // Unk flags 3
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.ReadUInt32(); // Unk flags 4
            SendPacketToClient(quest);
        }
    }
}
