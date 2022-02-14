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
            System.Console.WriteLine($"Query CREATURE {queryCreature.CreatureID}");
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
            packet.WriteUInt32(queryCreature.CreatureID);
            packet.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Creature, queryCreature.CreatureID, 1));
            SendPacketToServer(packet);
        }
    }
}
