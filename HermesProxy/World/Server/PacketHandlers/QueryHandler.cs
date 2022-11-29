﻿using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_QUERY_TIME)]
        void HandleQueryTime(EmptyClientPacket queryTime)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_TIME);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_QUEST_INFO)]
        void HandleQueryQuestInfo(QueryQuestInfo queryQuest)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_QUEST_INFO);
            packet.WriteUInt32(queryQuest.QuestID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_CREATURE)]
        void HandleQueryCreature(QueryCreature queryCreature)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_CREATURE);
            packet.WriteUInt32(queryCreature.CreatureID);
            packet.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Creature, queryCreature.CreatureID, 1));
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_GAME_OBJECT)]
        void HandleQueryGameObject(QueryGameObject queryGo)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_GAME_OBJECT);
            packet.WriteUInt32(queryGo.GameObjectID);
            packet.WriteGuid(queryGo.Guid.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_PAGE_TEXT)]
        void HandleQueryPageText(QueryPageText queryText)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_PAGE_TEXT);
            packet.WriteUInt32(queryText.PageTextID);
            packet.WriteGuid(queryText.ItemGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_NPC_TEXT)]
        void HandleQueryNpcText(QueryNPCText queryText)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_NPC_TEXT);
            packet.WriteUInt32(queryText.TextID);
            packet.WriteGuid(queryText.Guid.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_PET_NAME)]
        void HandleQueryPetName(QueryPetName queryName)
        {
            WorldPacket packet = new(Opcode.CMSG_QUERY_PET_NAME);
            packet.WriteUInt32(queryName.UnitGUID.GetEntry());
            packet.WriteGuid(queryName.UnitGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_WHO)]
        void HandleWhoRequest(WhoRequestPkt who)
        {
            WorldPacket packet = new(Opcode.CMSG_WHO);
            packet.WriteInt32(who.Request.MinLevel);
            packet.WriteInt32(who.Request.MaxLevel);
            packet.WriteCString(who.Request.Name);
            packet.WriteCString(who.Request.Guild);
            packet.WriteInt32((int)who.Request.RaceFilter);
            packet.WriteInt32(who.Request.ClassFilter);

            packet.WriteInt32(who.Areas.Count);
            foreach (int area in who.Areas)
                packet.WriteInt32(area);

            packet.WriteInt32(who.Request.Words.Count);
            foreach (string word in who.Request.Words)
                packet.WriteCString(word);

            SendPacketToServer(packet);
            GetSession().GameState.LastWhoRequestId = who.RequestID;
        }
    }
}
