using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.QueryPageTextResponse;

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
        [PacketHandler(Opcode.SMSG_QUERY_PAGE_TEXT_RESPONSE)]
        void HandleQueryPageTextResponse(WorldPacket packet)
        {
            QueryPageTextResponse response = new QueryPageTextResponse();
            response.PageTextID = packet.ReadUInt32();
            response.Allow = true;
            PageTextInfo page = new PageTextInfo();
            page.Id = response.PageTextID;
            page.Text = packet.ReadCString();
            page.NextPageID = packet.ReadUInt32();
            response.Pages.Add(page);
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
                string maleText = packet.ReadCString().TrimEnd().Replace("\0", "");
                string femaleText = packet.ReadCString().TrimEnd().Replace("\0", "");
                uint language = packet.ReadUInt32();

                ushort[] emoteDelays = new ushort[3];
                ushort[]  emotes = new ushort[3];
                for (int j = 0; j < 3; j++)
                {
                    emoteDelays[j] = (ushort)packet.ReadUInt32();
                    emotes[j] = (ushort)packet.ReadUInt32();
                }

                const string placeholderGossip = "Greetings $N";

                if (String.IsNullOrEmpty(maleText) && String.IsNullOrEmpty(femaleText) ||
                    maleText == placeholderGossip && femaleText == placeholderGossip && i != 0)
                    response.BroadcastTextID[i] = 0;
                else
                    response.BroadcastTextID[i] = GameData.GetBroadcastTextId(maleText, femaleText, language, emoteDelays, emotes);
            }

            SendPacketToClient(response);
        }
        [PacketHandler(Opcode.SMSG_QUERY_PET_NAME_RESPONSE)]
        void HandleQueryPetNameResponse(WorldPacket packet)
        {
            uint petNumber = packet.ReadUInt32();
            WowGuid128 guid = GetSession().GameState.GetPetGuidByNumber(petNumber);
            if (guid == null)
            {
                Log.Print(LogType.Error, $"Pet name query response for unknown pet {petNumber}!");
                return;
            }

            QueryPetNameResponse response = new QueryPetNameResponse();
            response.UnitGUID = guid;
            response.Name = packet.ReadCString();
            if (response.Name.Length == 0)
            {
                response.Allow = false;
                packet.ReadBytes(7); // 0s
                return;
            }

            response.Allow = true;
            response.Timestamp = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                var declined = packet.ReadBool();

                const int maxDeclinedNameCases = 5;

                if (declined)
                {
                    for (var i = 0; i < maxDeclinedNameCases; i++)
                        response.DeclinedNames.name[i] = packet.ReadCString();
                }
            }
            SendPacketToClient(response);
        }
        [PacketHandler(Opcode.SMSG_WHO)]
        void HandleWhoResponse(WorldPacket packet)
        {
            WhoResponsePkt response = new WhoResponsePkt();
            response.RequestID = GetSession().GameState.LastWhoRequestId;
            var count = packet.ReadUInt32();
            packet.ReadUInt32(); // Online count
            for (var i = 0; i < count; ++i)
            {
                WhoEntry player = new();
                player.PlayerData.Name = packet.ReadCString();
                player.GuildName = packet.ReadCString();
                player.PlayerData.Level = (byte)packet.ReadUInt32();
                player.PlayerData.ClassID = (Class)packet.ReadUInt32();
                player.PlayerData.RaceID = (Race)packet.ReadUInt32();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    player.PlayerData.Sex = (Gender)packet.ReadUInt8();
                player.AreaID = packet.ReadInt32();

                player.PlayerData.GuidActual = GetSession().GameState.GetPlayerGuidByName(player.PlayerData.Name);
                if (player.PlayerData.GuidActual == null)
                    player.PlayerData.GuidActual = WowGuid128.Create(HighGuidType703.Player, 20000 + count);
                player.PlayerData.AccountID = GetSession().GetGameAccountGuidForPlayer(player.PlayerData.GuidActual);
                player.PlayerData.BnetAccountID = GetSession().GetBnetAccountGuidForPlayer(player.PlayerData.GuidActual);
                player.PlayerData.VirtualRealmAddress = GetSession().RealmId.GetAddress();

                if (!String.IsNullOrEmpty(player.GuildName))
                {
                    player.GuildGUID = GetSession().GetGuildGuid(player.GuildName);
                    player.GuildVirtualRealmAddress = player.PlayerData.VirtualRealmAddress;
                }
                response.Players.Add(player);
            }
            SendPacketToClient(response);
        }
    }
}
