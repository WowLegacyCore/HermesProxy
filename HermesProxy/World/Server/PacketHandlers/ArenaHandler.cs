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
                response.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex(arena.TeamIndex);
                SendPacket(response);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
                packet.WriteUInt32(arena.TeamIndex + 1);
                SendPacketToServer(packet);

                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
                packet2.WriteUInt32(arena.TeamIndex + 1);
                SendPacketToServer(packet2);
            }
        }

        [PacketHandler(Opcode.CMSG_ARENA_TEAM_QUERY)]
        void HandleArenaTeamQuery(ArenaTeamQuery arena)
        {
            ArenaTeamData team;
            if (GetSession().GameState.ArenaTeams.TryGetValue(arena.TeamId, out team))
            {
                ArenaTeamQueryResponse response = new ArenaTeamQueryResponse();
                response.TeamId = arena.TeamId;
                response.Emblem = new ArenaTeamEmblem();
                response.Emblem.TeamId = arena.TeamId;
                response.Emblem.TeamSize = team.TeamSize;
                response.Emblem.BackgroundColor = team.BackgroundColor;
                response.Emblem.EmblemStyle = team.EmblemStyle;
                response.Emblem.EmblemColor = team.EmblemColor;
                response.Emblem.BorderStyle = team.BorderStyle;
                response.Emblem.BorderColor = team.BorderColor;
                response.Emblem.TeamName = team.Name;
                SendPacket(response);
            }
        }

        [PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA)]
        void HandleBattlematerJoinArena(BattlemasterJoinArena join)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
            packet.WriteGuid(join.Guid.To64());
            packet.WriteUInt8(join.TeamSizeIndex);
            packet.WriteBool(true); // As Group
            packet.WriteBool(true); // Is Rated
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN_SKIRMISH)]
        void HandleBattlematerJoinSkirmish(BattlemasterJoinSkirmish join)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
            packet.WriteGuid(join.Guid.To64());
            packet.WriteUInt8(join.TeamSizeIndex);
            packet.WriteBool(join.AsGroup);
            packet.WriteBool(false); // Is Rated
            SendPacketToServer(packet);
        }
    }
}
