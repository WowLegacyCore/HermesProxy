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
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) ||
                GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex] == 0)
            {
                ArenaTeamRosterResponse response = new ArenaTeamRosterResponse();
                response.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex(arena.TeamIndex);
                SendPacket(response);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
                packet.WriteUInt32(GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex]);
                SendPacketToServer(packet);

                WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
                packet2.WriteUInt32(GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex]);
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
            packet.WriteUInt8(join.TeamIndex);
            packet.WriteBool(true); // As Group
            packet.WriteBool(true); // Is Rated
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN_SKIRMISH)]
        void HandleBattlematerJoinSkirmish(BattlemasterJoinSkirmish join)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
            packet.WriteGuid(join.Guid.To64());
            packet.WriteUInt8(join.TeamSize);
            packet.WriteBool(join.AsGroup);
            packet.WriteBool(false); // Is Rated
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ARENA_TEAM_REMOVE)]
        [PacketHandler(Opcode.CMSG_ARENA_TEAM_LEADER)]
        void HandleArenaUnimplemented(ArenaTeamRemove arena)
        {
            WorldPacket packet = new WorldPacket(arena.GetUniversalOpcode());
            packet.WriteUInt32(arena.TeamId);
            packet.WriteCString(GetSession().GameState.GetPlayerName(arena.PlayerGuid));
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ARENA_TEAM_DISBAND)]
        [PacketHandler(Opcode.CMSG_ARENA_TEAM_LEAVE)]
        void HandleArenaTeamLeave(ArenaTeamLeave arena)
        {
            WorldPacket packet = new WorldPacket(arena.GetUniversalOpcode());
            packet.WriteUInt32(arena.TeamId);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ARENA_TEAM_ACCEPT)]
        [PacketHandler(Opcode.CMSG_ARENA_TEAM_DECLINE)]
        void HandleArenaTeamInviteResponse(ArenaTeamAccept arena)
        {
            WorldPacket packet = new WorldPacket(arena.GetUniversalOpcode());
            SendPacketToServer(packet);
        }
    }
}
