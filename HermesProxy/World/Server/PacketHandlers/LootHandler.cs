using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_LOOT_RELEASE)]
        void HandleLootRelease(LootRelease loot)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_RELEASE);
            packet.WriteGuid(loot.Owner.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_LOOT_ITEM)]
        void HandleLootItem(LootItemPkt loot)
        {
            foreach (var item in loot.Loot)
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTOSTORE_LOOT_ITEM);
                packet.WriteUInt8(item.LootListID);
                SendPacketToServer(packet);
            }
        }
    }
}
