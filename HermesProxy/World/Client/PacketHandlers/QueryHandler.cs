using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE)]
        void HandleQueryQuestInfoResponse(WorldPacket packet)
        {
            QueryQuestInfoResponse response = new QueryQuestInfoResponse();
            var id = packet.ReadEntry();
            response.QuestID = (uint)id.Key;
            if (id.Value) // entry is masked
            {
                response.Allow = false;
                SendPacketToClient(response);
                return;
            }

            response.Allow = true;
            response.Info = new QuestInfo();
            QuestInfo quest = response.Info;

            quest.QuestID = response.QuestID;
            quest.QuestType = packet.ReadInt32();
            quest.QuestLevel = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                quest.MinLevel = packet.ReadInt32();
            else
                quest.MinLevel = 1;

            quest.QuestSortID = packet.ReadInt32();
            quest.QuestInfoID = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                quest.SuggestedGroupNum = packet.ReadUInt32();

            sbyte objectiveCounter = 0;
            for (int i = 0; i < 2; i++)
            {
                int factionId = packet.ReadInt32(); // RequiredFactionID
                int factionValue = packet.ReadInt32(); // RequiredFactionValue
                if (factionId != 0 && factionValue != 0)
                {
                    QuestObjective objective = new QuestObjective();
                    objective.QuestID = response.QuestID;
                    objective.Id = QuestObjective.QuestObjectiveCounter++;
                    objective.StorageIndex = objectiveCounter++;
                    objective.Type = QuestObjectiveType.MinReputation;
                    objective.ObjectID = factionId;
                    objective.Amount = factionValue;
                    quest.Objectives.Add(objective);
                }
            }

            quest.RewardNextQuest = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                quest.RewardXPDifficulty = packet.ReadUInt32();

            int rewOrReqMoney = packet.ReadInt32();
            if (rewOrReqMoney >= 0)
                quest.RewardMoney = rewOrReqMoney;
            else
            {
                QuestObjective objective = new QuestObjective();
                objective.QuestID = response.QuestID;
                objective.Id = QuestObjective.QuestObjectiveCounter++;
                objective.StorageIndex = objectiveCounter++;
                objective.Type = QuestObjectiveType.Money;
                objective.ObjectID = 0;
                objective.Amount = -rewOrReqMoney;
                quest.Objectives.Add(objective);
            }
            quest.RewardBonusMoney = packet.ReadUInt32();
            quest.RewardDisplaySpell[0] = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                quest.RewardSpell = packet.ReadUInt32();
                quest.RewardHonor = packet.ReadUInt32();
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                quest.RewardKillHonor = packet.ReadFloat();

            quest.StartItem = packet.ReadUInt32();
            quest.Flags = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                quest.RewardTitle = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                int requiredPlayerKills = packet.ReadInt32();
                if (requiredPlayerKills != 0)
                {
                    QuestObjective objective = new QuestObjective();
                    objective.QuestID = response.QuestID;
                    objective.Id = QuestObjective.QuestObjectiveCounter++;
                    objective.StorageIndex = objectiveCounter++;
                    objective.Type = QuestObjectiveType.PlayerKills;
                    objective.ObjectID = 0;
                    objective.Amount = requiredPlayerKills;
                    quest.Objectives.Add(objective);
                }
                packet.ReadUInt32(); // RewardTalents
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                quest.RewardArenaPoints = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                packet.ReadInt32(); // Unk

            for (int i = 0; i < 4; i++)
            {
                quest.RewardItems[i] = packet.ReadUInt32();
                quest.RewardAmount[i] = packet.ReadUInt32();
            }

            for (int i = 0; i < 6; i++)
            {
                QuestInfoChoiceItem choiceItem = new QuestInfoChoiceItem();
                choiceItem.ItemID = packet.ReadUInt32();
                choiceItem.Quantity = packet.ReadUInt32();

                ItemTemplate item = GameData.GetItemTemplate(choiceItem.ItemID);
                if (item != null)
                    choiceItem.DisplayID = item.DisplayId;

                quest.UnfilteredChoiceItems[i] = choiceItem;
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
            {
                for (int i = 0; i < 5; i++)
                    quest.RewardFactionID[i] = packet.ReadUInt32();

                for (int i = 0; i < 5; i++)
                    quest.RewardFactionValue[i] = packet.ReadInt32();

                for (int i = 0; i < 5; i++)
                    quest.RewardFactionOverride[i] = (int)packet.ReadUInt32();
            }

            quest.POIContinent = packet.ReadUInt32();
            quest.POIx = packet.ReadFloat();
            quest.POIy = packet.ReadFloat();
            quest.POIPriority = packet.ReadUInt32();
            quest.LogTitle = packet.ReadCString();
            quest.LogDescription = packet.ReadCString();
            quest.QuestDescription = packet.ReadCString();
            quest.AreaDescription = packet.ReadCString();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                quest.QuestCompletionLog = packet.ReadCString();

            var reqId = new KeyValuePair<int, bool>[4];
            var reqItemFieldCount = 4;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                reqItemFieldCount = 5;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                reqItemFieldCount = 6;
            int[] requiredItemID = new int[reqItemFieldCount];
            int[] requiredItemCount = new int[reqItemFieldCount];

            for (int i = 0; i < 4; i++)
            {
                reqId[i] = packet.ReadEntry();
                bool isGo = reqId[i].Value;

                int creatureOrGoId = reqId[i].Key * (isGo ? -1 : 1); ;
                int creatureOrGoAmount = packet.ReadInt32();

                if (creatureOrGoId != 0 && creatureOrGoAmount != 0)
                {
                    QuestObjective objective = new QuestObjective();
                    objective.QuestID = response.QuestID;
                    objective.Id = QuestObjective.QuestObjectiveCounter++;
                    objective.StorageIndex = objectiveCounter++;
                    objective.Type = isGo ? QuestObjectiveType.GameObject : QuestObjectiveType.Monster;
                    objective.ObjectID = creatureOrGoId;
                    objective.Amount = creatureOrGoAmount;
                    quest.Objectives.Add(objective);
                }

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    requiredItemID[i] = packet.ReadInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                    requiredItemCount[i] = packet.ReadInt32();

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_8_9464))
                {
                    requiredItemID[i] = packet.ReadInt32();
                    requiredItemCount[i] = packet.ReadInt32();
                }
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
            {
                for (int i = 0; i < reqItemFieldCount; i++)
                {
                    requiredItemID[i] = packet.ReadInt32();
                    requiredItemCount[i] = packet.ReadInt32();
                }
            }

            for (int i = 0; i < reqItemFieldCount; i++)
            {
                if (requiredItemID[i] != 0 && requiredItemCount[i] != 0)
                {
                    QuestObjective objective = new QuestObjective();
                    objective.QuestID = response.QuestID;
                    objective.Id = QuestObjective.QuestObjectiveCounter++;
                    objective.StorageIndex = objectiveCounter++;
                    objective.Type = QuestObjectiveType.Item;
                    objective.ObjectID = requiredItemID[i];
                    objective.Amount = requiredItemCount[i];
                    quest.Objectives.Add(objective);
                }
            }

            // Placeholders
            quest.QuestMaxScalingLevel = 255;
            quest.RewardXPMultiplier = 1;
            quest.RewardMoneyMultiplier = 1;
            quest.RewardArtifactXPMultiplier = 1;
            for (int i = 0; i < QuestConst.QuestRewardReputationsCount; i++)
                quest.RewardFactionCapIn[i] = 7;
            quest.AllowableRaces = 511;
            quest.AcceptedSoundKitID = 890;
            quest.CompleteSoundKitID = 878;

            SendPacketToClient(response);
        }

        [PacketHandler(Opcode.SMSG_QUERY_CREATURE_RESPONSE)]
        void HandleQueryCreatureResponse(WorldPacket packet)
        {
            QueryCreatureResponse response = new QueryCreatureResponse();
            var id = packet.ReadEntry();
            response.CreatureID = (uint)id.Key;
            if (id.Value) // entry is masked
            {
                response.Allow = false;
                SendPacketToClient(response);
                return;
            }

            response.Allow = true;
            response.Stats = new CreatureStats();
            CreatureStats creature = response.Stats;

            for (int i = 0; i < 4; i++)
                creature.Name[i] = packet.ReadCString();

            creature.Title = packet.ReadCString();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                creature.CursorName = packet.ReadCString();

            creature.Flags[0] = packet.ReadUInt32(); // Type Flags
            creature.Type = packet.ReadInt32();
            creature.Family = packet.ReadInt32();
            creature.Classification = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                for (int i = 0; i < 2; ++i)
                    creature.ProxyCreatureID[i] = packet.ReadUInt32();
            }
            else
            {
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    packet.ReadInt32(); // unk
                creature.PetSpellDataId = packet.ReadUInt32();
            }

            int displayIdCount = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 4 : 1;
            for (int i = 0; i < displayIdCount; i++)
            {
                uint displayId = packet.ReadUInt32();
                if (displayId != 0)
                    creature.Display.CreatureDisplay.Add(new CreatureXDisplay(displayId, 1, 0));
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                creature.HpMulti = packet.ReadFloat();
                creature.EnergyMulti = packet.ReadFloat();
            }
            else
            {
                creature.HpMulti = 1;
                creature.EnergyMulti = 1;
            }

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                creature.Civilian = packet.ReadBool();
            creature.Leader = packet.ReadBool();

            int questItems = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192) ? 6 : 4;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                for (uint i = 0; i < questItems; ++i)
                {
                    uint itemId = packet.ReadUInt32();
                    if (itemId != 0)
                        creature.QuestItems.Add(itemId);
                }

                packet.ReadUInt32(); // Movement ID
            }

            // Placeholders
            creature.Flags[0] |= 134217728;
            creature.MovementInfoID = 1693;
            creature.Class = 1;

            SendPacketToClient(response);
        }
        [PacketHandler(Opcode.SMSG_QUERY_GAME_OBJECT_RESPONSE)]
        void HandleQueryGameObjectResposne(WorldPacket packet)
        {
            QueryGameObjectResponse response = new QueryGameObjectResponse();
            var id = packet.ReadEntry();
            response.GameObjectID = (uint)id.Key;
            response.Guid = WowGuid128.Empty;
            if (id.Value) // entry is masked
            {
                response.Allow = false;
                SendPacketToClient(response);
                return;
            }

            response.Allow = true;
            response.Stats = new GameObjectStats();
            GameObjectStats gameObject = response.Stats;

            gameObject.Type = packet.ReadUInt32();
            gameObject.DisplayID = packet.ReadUInt32();

            for (int i = 0; i < 4; i++)
                gameObject.Name[i] = packet.ReadCString();

            gameObject.IconName = packet.ReadCString();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                gameObject.CastBarCaption = packet.ReadCString();
                gameObject.UnkString = packet.ReadCString();
            }

            for (int i = 0; i < 24; i++)
                gameObject.Data[i] = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                gameObject.Size = packet.ReadFloat();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                uint count = (uint)(LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192) ? 6 : 4);
                for (uint i = 0; i < count; i++)
                {
                    uint itemId = packet.ReadUInt32();
                    if (itemId != 0)
                        gameObject.QuestItems.Add(itemId);
                }
            }

            SendPacketToClient(response);
        }
        [PacketHandler(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE)]
        void HandleQueryNpcTextResponse(WorldPacket packet)
        {
            QueryNPCTextResponse response = new QueryNPCTextResponse();
            var id = packet.ReadEntry();
            response.TextID = (uint)id.Key;
            if (id.Value) // entry is masked
            {
                response.Allow = false;
                SendPacketToClient(response);
                return;
            }

            response.Allow = true;

            for (int i = 0; i < 8; i++)
            {
                response.Probabilities[i] = packet.ReadFloat();
                string maleText = packet.ReadCString();
                string femaleText = packet.ReadCString();
                uint language = packet.ReadUInt32();

                ushort[] emoteDelays = new ushort[3];
                ushort[]  emotes = new ushort[3];
                for (int j = 0; j < 3; j++)
                {
                    emoteDelays[j] = (ushort)packet.ReadUInt32();
                    emotes[j] = (ushort)packet.ReadUInt32();
                }

                if (String.IsNullOrEmpty(maleText) && String.IsNullOrEmpty(femaleText))
                    response.BroadcastTextID[i] = 0;
                else
                    response.BroadcastTextID[i] = GameData.GetBroadcastTextId(maleText, femaleText, language, emoteDelays, emotes);
            }

            SendPacketToClient(response);
        }
    }
}
