using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_ARENA_TEAM_QUERY_RESPONSE)]
        void HandleArenaTeamQueryResponse(WorldPacket packet)
        {
            uint teamId = packet.ReadUInt32();
            if (!GetSession().GameState.ArenaTeams.TryGetValue(teamId, out ArenaTeamData team))
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
            if (!GetSession().GameState.ArenaTeams.TryGetValue(teamId, out ArenaTeamData team))
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
            ArenaTeamRosterResponse arena = new()
            {
                TeamId = packet.ReadUInt32()
            };

            var hiddenRating = false;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                packet.ReadBool();

            var count = packet.ReadUInt32();
            arena.TeamSize = packet.ReadUInt32();

            for (var i = 0; i < count; i++)
            {
                ArenaTeamMember member = new();
                PlayerCache cache = new();
                member.MemberGUID = packet.ReadGuid().To128(GetSession().GameState);
                member.Online = packet.ReadBool();
                member.Name = cache.Name = packet.ReadCString();
                member.Captain = packet.ReadInt32();
                member.Level = cache.Level = packet.ReadUInt8();
                member.ClassId = cache.ClassId = (Class)packet.ReadUInt8();
                GetSession().GameState.UpdatePlayerCache(member.MemberGUID, cache);
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

            if (GetSession().GameState.ArenaTeams.TryGetValue(arena.TeamId, out ArenaTeamData team))
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

        [PacketHandler(Opcode.SMSG_ARENA_TEAM_EVENT)]
        void HandleArenaTeamEvent(WorldPacket packet)
        {
            ArenaTeamEvent arena = new();
            var eventType = (ArenaTeamEventLegacy)packet.ReadUInt8();
            arena.Event = (ArenaTeamEventModern)Enum.Parse(typeof(ArenaTeamEventModern), eventType.ToString());
            byte count = packet.ReadUInt8();
            for (byte i = 0; i < count; i++)
            {
                string str = packet.ReadCString();
                switch (i)
                {
                    case 0:
                        arena.Param1 = str;
                        break;
                    case 1:
                        arena.Param2 = str;
                        break;
                    case 2:
                        arena.Param3 = str;
                        break;
                }
            }
            if (packet.CanRead())
                packet.ReadGuid();
            SendPacketToClient(arena);
        }

        [PacketHandler(Opcode.SMSG_ARENA_TEAM_COMMAND_RESULT)]
        void HandleArenaTeamCommandResult(WorldPacket packet)
        {
            ArenaTeamCommandResult arena = new()
            {
                Action = (ArenaTeamCommandType)packet.ReadUInt32(),
                TeamName = packet.ReadCString(),
                PlayerName = packet.ReadCString()
            };
            var errorType = (ArenaTeamCommandErrorLegacy)packet.ReadUInt32();
            arena.Error = (ArenaTeamCommandErrorModern)Enum.Parse(typeof(ArenaTeamCommandErrorModern), errorType.ToString());
            SendPacketToClient(arena);
        }

        [PacketHandler(Opcode.SMSG_ARENA_TEAM_INVITE)]
        void HandleArenaTeamInvite(WorldPacket packet)
        {
            ArenaTeamInvite arena = new()
            {
                PlayerName = packet.ReadCString(),
                TeamName = packet.ReadCString()
            };
            arena.PlayerGuid = GetSession().GameState.GetPlayerGuidByName(arena.PlayerName);
            if (arena.PlayerGuid == null)
                arena.PlayerGuid = WowGuid128.Empty;
            arena.PlayerVirtualAddress = GetSession().RealmId.GetAddress();
            arena.TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, 1);
            SendPacketToClient(arena);
        }
    }
}
