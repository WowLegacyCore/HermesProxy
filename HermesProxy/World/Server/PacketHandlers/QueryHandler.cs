using Framework.Constants;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_QUERY_QUEST_INFO)]
        void HandleQueryQuestInfo(QueryQuestInfo queryQuest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
            packet.WriteUInt32(queryQuest.QuestID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_CREATURE)]
        void HandleQueryCreature(QueryCreature queryCreature)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
            packet.WriteUInt32(queryCreature.CreatureID);
            packet.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Creature, queryCreature.CreatureID, 1));
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_GAME_OBJECT)]
        void HandleQueryGameObject(QueryGameObject queryGo)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GAME_OBJECT);
            packet.WriteUInt32(queryGo.GameObjectID);
            packet.WriteGuid(queryGo.Guid.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_PAGE_TEXT)]
        void HandleQueryPageText(QueryPageText queryText)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PAGE_TEXT);
            packet.WriteUInt32(queryText.PageTextID);
            packet.WriteGuid(queryText.ItemGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_NPC_TEXT)]
        void HandleQueryNpcText(QueryNPCText queryText)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_NPC_TEXT);
            packet.WriteUInt32(queryText.TextID);
            packet.WriteGuid(queryText.Guid.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_PET_NAME)]
        void HandleQueryPetName(QueryPetName queryName)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PET_NAME);
            packet.WriteUInt32(queryName.UnitGUID.GetEntry());
            packet.WriteGuid(queryName.UnitGUID.To64());
            SendPacketToServer(packet);
        }
    }
}
