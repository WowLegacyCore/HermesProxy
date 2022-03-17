using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_ARENA_TEAM_ROSTER)]
        void HandleArenaTeamRoster(ArenaTeamRosterRequest arena)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                ArenaTeamRosterResponse response = new ArenaTeamRosterResponse();
                response.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex(arena.TeamId);
                SendPacket(response);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
                packet.WriteInt32(arena.TeamId + 1);
                SendPacketToServer(packet);

                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
                packet2.WriteInt32(arena.TeamId + 1);
                SendPacketToServer(packet2);
            }
        }
    }
}
