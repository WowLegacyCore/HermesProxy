using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_ARENA_TEAM_QUERY_RESPONSE)]
        void HandleArenaTeamQueryResponse(WorldPacket packet)
        {
            uint teamId = packet.ReadUInt32();
            ArenaTeamData team;
            if (!GetSession().GameState.ArenaTeams.TryGetValue(teamId, out team))
            {
                team = new ArenaTeamData();
                GetSession().GameState.ArenaTeams.Add(teamId, team);
            }

            team.Name = packet.ReadCString();
            team.TeamSize = packet.ReadUInt32();
            team.BackgroundColor = packet.ReadUInt32();
            team.EmblemStyle = packet.ReadUInt32();
            team.EmblemColor = packet.ReadUInt32();
            team.BorderStyle = packet.ReadUInt32();
            team.BorderColor = packet.ReadUInt32();
        }

        [PacketHandler(Opcode.SMSG_ARENA_TEAM_STATS)]
        void HandleArenaTeamStats(WorldPacket packet)
        {
            uint teamId = packet.ReadUInt32();
            ArenaTeamData team;
            if (!GetSession().GameState.ArenaTeams.TryGetValue(teamId, out team))
            {
                team = new ArenaTeamData();
                GetSession().GameState.ArenaTeams.Add(teamId, team);
            }

            team.Rating = packet.ReadUInt32();
            team.WeekPlayed = packet.ReadUInt32();
            team.WeekWins = packet.ReadUInt32();
            team.SeasonPlayed = packet.ReadUInt32();
            team.SeasonWins = packet.ReadUInt32();
            team.Rank = packet.ReadUInt32();
        }

        [PacketHandler(Opcode.SMSG_ARENA_TEAM_ROSTER)]
        void HandleArenaTeamRoster(WorldPacket packet)
        {
            ArenaTeamRosterResponse arena = new ArenaTeamRosterResponse();
            arena.TeamId = packet.ReadUInt32();

            var hiddenRating = false;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                packet.ReadBool();

            var count = packet.ReadUInt32();
            arena.TeamSize = packet.ReadUInt32();

            for (var i = 0; i < count; i++)
            {
                ArenaTeamMember member = new ArenaTeamMember();
                member.MemberGUID = packet.ReadGuid().To128();
                member.Online = packet.ReadBool();
                member.Name = packet.ReadCString();
                member.Captain = packet.ReadInt32();
                member.Level = packet.ReadUInt8();
                member.ClassId = (Class)packet.ReadUInt8();
                member.WeekGamesPlayed = packet.ReadUInt32();
                member.WeekGamesWon = packet.ReadUInt32();
                member.SeasonGamesPlayed = packet.ReadUInt32();
                member.SeasonGamesWon = packet.ReadUInt32();
                member.PersonalRating = packet.ReadUInt32();
                if (hiddenRating)
                {
                    // Hidden rating, see LUA GetArenaTeamGdfInfo - gdf = Gaussian Density Filter
                    // Introduced in Patch 3.0.8
                    member.dword60 = packet.ReadFloat();
                    member.dword68 = packet.ReadFloat();
                }
                arena.Members.Add(member);
            }

            ArenaTeamData team;
            if (GetSession().GameState.ArenaTeams.TryGetValue(arena.TeamId, out team))
            {
                arena.TeamPlayed = team.WeekPlayed;
                arena.TeamWins = team.WeekWins;
                arena.SeasonPlayed = team.SeasonPlayed;
                arena.SeasonWins = team.SeasonWins;
                arena.TeamRating = team.Rating;
                arena.PlayerRating = team.Rank;
            }

            SendPacketToClient(arena);
        }
    }
}
