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
        [PacketHandler(Opcode.CMSG_SET_FACTION_AT_WAR)]
        void HandleSetFactionAtWar(SetFactionAtWar faction)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_FACTION_AT_WAR);
            packet.WriteUInt32(faction.FactionIndex);
            packet.WriteBool(true);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_SET_FACTION_NOT_AT_WAR)]
        void HandleSetFactionNotAtWar(SetFactionNotAtWar faction)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_FACTION_AT_WAR);
            packet.WriteUInt32(faction.FactionIndex);
            packet.WriteBool(false);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_SET_FACTION_INACTIVE)]
        void HandleSetFactionInactive(SetFactionInactive faction)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_FACTION_INACTIVE);
            packet.WriteUInt32(faction.FactionIndex);
            packet.WriteBool(faction.State);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_SET_WATCHED_FACTION)]
        void HandleSetFactionInactive(SetWatchedFaction faction)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_WATCHED_FACTION);
            packet.WriteUInt32(faction.FactionIndex);
            SendPacketToServer(packet);
        }
    }
}
