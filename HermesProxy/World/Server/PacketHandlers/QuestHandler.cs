using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST)]
        void HandleQuestGiverQueryQuest(QuestGiverQueryQuest quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST);
            packet.WriteGuid(quest.QuestGiverGUID.To64());
            packet.WriteUInt32(quest.QuestID);
            if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V2_0_1_6180))
                packet.WriteBool(quest.RespondToGiver);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST)]
        void HandleQuestGiverAcceptQuest(QuestGiverAcceptQuest quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST);
            packet.WriteGuid(quest.QuestGiverGUID.To64());
            packet.WriteUInt32(quest.QuestID);
            if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V3_1_2_9901))
                packet.WriteInt32(quest.StartCheat ? 1 : 0);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST)]
        void HandleQuestLogRemoveQuest(QuestLogRemoveQuest quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST);
            packet.WriteUInt8(quest.Slot);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY)]
        void HandleQuestGiverStatusQuery(QuestGiverStatusQuery query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
            packet.WriteGuid(query.QuestGiverGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY)]
        void HandleQuestGiverStatusMultipleQuery(QuestGiverStatusMultipleQuery query)
        {
            if (LegacyVersion.AddedInVersion(HermesProxy.Enums.ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY);
                SendPacketToServer(packet);
            }
            else
            {
                int UNIT_NPC_FLAGS = ModernVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
                if (UNIT_NPC_FLAGS < 0)
                    return;

                List<WowGuid128> npcGuids = new List<WowGuid128>();
                GetSession().GameState.ObjectCacheMutex.WaitOne();
                foreach (var obj in GetSession().GameState.ObjectCacheModern)
                {
                    if (obj.Key.GetObjectType() == ObjectType.Unit && 
                        obj.Value.GetUpdateField<uint>(UNIT_NPC_FLAGS).HasAnyFlag(NPCFlags.QuestGiver))
                        npcGuids.Add(obj.Key);
                }
                GetSession().GameState.ObjectCacheMutex.ReleaseMutex();

                foreach (var guid in npcGuids)
                {
                    WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
                    packet.WriteGuid(guid.To64());
                    SendPacketToServer(packet);
                }
            }
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_HELLO)]
        void HandleQuestGiverHello(QuestGiverHello hello)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_HELLO);
            packet.WriteGuid(hello.QuestGiverGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD)]
        void HandleQuestGiverRequestReward(QuestGiverRequestReward quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD);
            packet.WriteGuid(quest.QuestGiverGUID.To64());
            packet.WriteUInt32(quest.QuestID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD)]
        void HandleQuestGiverChooseReward(QuestGiverChooseReward quest)
        {
            int choiceIndex = 0;

            if (quest.Choice.Item.ItemID != 0)
            {
                QuestTemplate questTemplate = GameData.GetQuestTemplate(quest.QuestID);
                if (questTemplate == null)
                {
                    Log.Print(LogType.Error, "Unable to select quest reward because quest template is missing. Try again.");
                    WorldPacket packet2 = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
                    packet2.WriteUInt32(quest.QuestID);
                    SendPacketToServer(packet2);
                    QuestGiverQuestFailed fail = new QuestGiverQuestFailed();
                    fail.QuestID = quest.QuestID;
                    fail.Reason = InventoryResult.ItemNotFound;
                    SendPacket(fail);
                    return;
                }

                for (int i = 0; i < questTemplate.UnfilteredChoiceItems.Length; i++)
                {
                    if (questTemplate.UnfilteredChoiceItems[i].ItemID == quest.Choice.Item.ItemID)
                    {
                        choiceIndex = i;
                        break;
                    }
                }
            }
            
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD);
            packet.WriteGuid(quest.QuestGiverGUID.To64());
            packet.WriteUInt32(quest.QuestID);
            packet.WriteInt32(choiceIndex);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST)]
        void HandleQuestGiverCompleteQuest(QuestGiverCompleteQuest quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST);
            packet.WriteGuid(quest.QuestGiverGUID.To64());
            packet.WriteUInt32(quest.QuestID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_CONFIRM_ACCEPT)]
        void HandleQuestConfirmAcceptResponse(QuestConfirmAcceptResponse quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_CONFIRM_ACCEPT);
            packet.WriteUInt32(quest.QuestID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_PUSH_QUEST_TO_PARTY)]
        void HandlePushQuestToParty(PushQuestToParty quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PUSH_QUEST_TO_PARTY);
            packet.WriteUInt32(quest.QuestID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUEST_PUSH_RESULT)]
        void HandleQuestPushResult(QuestPushResultResponse quest)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_QUEST_PUSH_RESULT);
            packet.WriteGuid(quest.SenderGUID.To64());
            packet.WriteUInt8((byte)quest.Result);
            SendPacketToServer(packet);
        }
    }
}
