using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using static HermesProxy.World.Server.Packets.PVPMatchStatisticsMessage;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleBattlefieldListVanilla(WorldPacket packet)
        {
            BattlefieldList bglist = new BattlefieldList
            {
                BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState)
            };
            GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
            bglist.BattlemasterListID = GameData.GetBattlegroundIdFromMapId(packet.ReadUInt32());
            packet.ReadUInt8(); // bracket id
            var instancesCount = packet.ReadUInt32();
            for (var i = 0; i < instancesCount; i++)
            {
                int instanceId = packet.ReadInt32();
                bglist.BattlefieldInstances.Add(instanceId);
            }
            SendPacketToClient(bglist);
        }

        [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056)]
        void HandleBattlefieldListTBC(WorldPacket packet)
        {
            BattlefieldList bglist = new BattlefieldList
            {
                BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState)
            };
            GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
            bglist.BattlemasterListID = packet.ReadUInt32();
            packet.ReadUInt8(); // bracket id
            var instancesCount = packet.ReadUInt32();
            for (var i = 0; i < instancesCount; i++)
            {
                int instanceId = packet.ReadInt32();
                bglist.BattlefieldInstances.Add(instanceId);
            }
            SendPacketToClient(bglist);
        }

        [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.V3_0_2_9056)]
        void HandleBattlefieldListWotLK(WorldPacket packet)
        {
            BattlefieldList bglist = new BattlefieldList
            {
                BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState)
            };
            GetSession().GameState.CurrentInteractedWithNPC = bglist.BattlemasterGuid;
            bglist.PvpAnywhere = packet.ReadBool(); // from UI
            bglist.BattlemasterListID = packet.ReadUInt32();
            bglist.MinLevel = packet.ReadUInt8();
            bglist.MaxLevel = packet.ReadUInt8();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
            {
                packet.ReadBool(); // Has Win
                packet.ReadInt32(); // Winner Honor Reward
                packet.ReadInt32(); // Winner Arena Reward
                packet.ReadInt32(); // Loser Honor Reward

                if (packet.ReadBool()) // Is random
                {
                    bglist.HasRandomWinToday = packet.ReadBool();
                    packet.ReadInt32(); // Random Winner Honor Reward
                    packet.ReadInt32(); // Random Winner Arena Reward
                    packet.ReadInt32(); // Random Loser Honor Reward
                }
            }
            var instancesCount = packet.ReadUInt32();
            for (var i = 0; i < instancesCount; i++)
            {
                int instanceId = packet.ReadInt32();
                bglist.BattlefieldInstances.Add(instanceId);
            }
            SendPacketToClient(bglist);
        }

        [PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleBattlefieldStatusVanilla(WorldPacket packet)
        {
            BattlefieldStatusHeader hdr = new BattlefieldStatusHeader();
            hdr.Ticket.Id = 1 + packet.ReadUInt32(); // Queue Slot
            hdr.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
            hdr.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(hdr.Ticket.Id);
            hdr.Ticket.Type = RideType.Battlegrounds;

            uint mapId = packet.ReadUInt32();
            if (mapId != 0)
            {
                uint battlefieldListId = GameData.GetBattlegroundIdFromMapId(mapId);
                hdr.BattlefieldListIDs.Add(battlefieldListId);
                packet.ReadUInt8(); // bracket id
                hdr.InstanceID = packet.ReadUInt32();
                BattleGroundStatus status = (BattleGroundStatus)packet.ReadUInt32();
                switch (status)
                {
                    case BattleGroundStatus.WaitQueue:
                    {
                        BattlefieldStatusQueued queue = new BattlefieldStatusQueued
                        {
                            Hdr = hdr,
                            AverageWaitTime = packet.ReadUInt32(),
                            WaitTime = packet.ReadUInt32()
                        };
                        SendPacketToClient(queue);
                        break;
                    }
                    case BattleGroundStatus.WaitJoin:
                    {
                        BattlefieldStatusNeedConfirmation confirm = new BattlefieldStatusNeedConfirmation
                        {
                            Hdr = hdr,
                            Mapid = mapId,
                            Timeout = packet.ReadUInt32()
                        };
                        SendPacketToClient(confirm);
                        break;
                    }
                    case BattleGroundStatus.InProgress:
                    {
                        BattlefieldStatusActive active = new BattlefieldStatusActive
                        {
                            Hdr = hdr,
                            Mapid = mapId,
                            ShutdownTimer = packet.ReadUInt32(),
                            StartTimer = packet.ReadUInt32()
                        };
                        if (active.ShutdownTimer == 0)
                        {
                            BattlegroundInit init = new BattlegroundInit
                            {
                                Milliseconds = 1154756799
                            };
                            SendPacketToClient(init);
                        }
                        SendPacketToClient(active);
                        break;
                    }
                    default:
                    {
                        Log.Print(LogType.Error, $"Unexpected BG status {status}.");
                        break;
                    }
                }
            }
            else
            {
                uint queuedMapId = GetSession().GameState.GetBattleFieldQueueType(hdr.Ticket.Id);
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) &&
                    queuedMapId == GetSession().GameState.CurrentMapId)
                {
                    // Clear BG group properly on vanilla servers.
                    var bgGroup = GetSession().GameState.CurrentGroups[1];
                    if (bgGroup != null)
                    {
                        PartyUpdate party = new PartyUpdate
                        {
                            SequenceNum = GetSession().GameState.GroupUpdateCounter++,
                            PartyFlags = GroupFlags.FakeRaid | GroupFlags.Destroyed,
                            PartyIndex = 1,
                            PartyGUID = bgGroup.PartyGUID,
                            LeaderGUID = WowGuid128.Empty,
                            MyIndex = -1
                        };
                        GetSession().GameState.CurrentGroups[1] = null;
                        SendPacketToClient(party);
                    }
                }

                BattlefieldStatusFailed failed = new BattlefieldStatusFailed
                {
                    Ticket = hdr.Ticket,
                    Reason = 30,
                    BattlefieldListId = GameData.GetBattlegroundIdFromMapId(queuedMapId)
                };
                SendPacketToClient(failed);
                GetSession().GameState.BattleFieldQueueTimes.Remove(hdr.Ticket.Id);
            }
            GetSession().GameState.StoreBattleFieldQueueType(hdr.Ticket.Id, mapId);
        }

        [PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS, ClientVersionBuild.V2_0_1_6180)]
        void HandleBattlefieldStatusTBC(WorldPacket packet)
        {
            BattlefieldStatusHeader hdr = new BattlefieldStatusHeader();
            hdr.Ticket.Id = 1 + packet.ReadUInt32(); // Queue Slot
            hdr.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
            hdr.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(hdr.Ticket.Id);
            hdr.Ticket.Type = RideType.Battlegrounds;

            hdr.ArenaTeamSize = packet.ReadUInt8();
            packet.ReadUInt8(); // unk
            uint battlefieldListId = packet.ReadUInt32();
            packet.ReadUInt16(); // 0x1F90

            if (battlefieldListId != 0)
            {
                hdr.BattlefieldListIDs.Add(battlefieldListId);

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
                {
                    hdr.RangeMin = packet.ReadUInt8();
                    hdr.RangeMax = packet.ReadUInt8();
                }

                hdr.InstanceID = packet.ReadUInt32();
                hdr.IsArena = packet.ReadBool();
                BattleGroundStatus status = (BattleGroundStatus)packet.ReadUInt32();
                switch (status)
                {
                    case BattleGroundStatus.WaitQueue:
                    {
                        BattlefieldStatusQueued queue = new BattlefieldStatusQueued
                        {
                            Hdr = hdr,
                            AverageWaitTime = packet.ReadUInt32(),
                            WaitTime = packet.ReadUInt32()
                        };
                        SendPacketToClient(queue);
                        break;
                    }
                    case BattleGroundStatus.WaitJoin:
                    {
                        BattlefieldStatusNeedConfirmation confirm = new BattlefieldStatusNeedConfirmation
                        {
                            Hdr = hdr,
                            Mapid = packet.ReadUInt32()
                        };
                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_5_12213))
                            packet.ReadUInt64(); // unk
                        confirm.Timeout = packet.ReadUInt32();
                        SendPacketToClient(confirm);
                        break;
                    }
                    case BattleGroundStatus.InProgress:
                    {
                        BattlefieldStatusActive active = new BattlefieldStatusActive
                        {
                            Hdr = hdr,
                            Mapid = packet.ReadUInt32()
                        };
                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_5_12213))
                            packet.ReadUInt64(); // unk
                        active.ShutdownTimer = packet.ReadUInt32();
                        active.StartTimer = packet.ReadUInt32();
                        active.ArenaFaction = packet.ReadUInt8();
                        if (active.ShutdownTimer == 0)
                        {
                            BattlegroundInit init = new BattlegroundInit
                            {
                                Milliseconds = 1154756799
                            };
                            SendPacketToClient(init);
                        }
                        SendPacketToClient(active);
                        break;
                    }
                    default:
                    {
                        Log.Print(LogType.Error, $"Unexpected BG status {status}.");
                        break;
                    }
                }
            }
            else
            {
                BattlefieldStatusFailed failed = new BattlefieldStatusFailed
                {
                    Ticket = hdr.Ticket,
                    Reason = 30,
                    BattlefieldListId = GetSession().GameState.GetBattleFieldQueueType(hdr.Ticket.Id)
                };
                SendPacketToClient(failed);
                GetSession().GameState.BattleFieldQueueTimes.Remove(hdr.Ticket.Id);
            }
            GetSession().GameState.StoreBattleFieldQueueType(hdr.Ticket.Id, battlefieldListId);
        }

        [PacketHandler(Opcode.MSG_PVP_LOG_DATA, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandlePvPLogDataVanilla(WorldPacket packet)
        {
            PVPMatchStatisticsMessage pvp = new PVPMatchStatisticsMessage();
            if (packet.ReadBool()) // Has Winner
                pvp.Winner = packet.ReadUInt8();

            int count = packet.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                PVPMatchPlayerStatistics player = new PVPMatchPlayerStatistics
                {
                    PlayerGUID = packet.ReadGuid().To128(GetSession().GameState),
                    Rank = packet.ReadInt32(),
                    Kills = packet.ReadUInt32(),
                    Honor = new()
                    {
                        HonorKills = packet.ReadUInt32(),
                        Deaths = packet.ReadUInt32(),
                        ContributionPoints = packet.ReadUInt32()
                    }
                };

                int statsCount = packet.ReadInt32();
                for (int j = 0; j < statsCount; j++)
                    player.Stats.Add(packet.ReadUInt32());

                if (GetSession().GameState.CachedPlayers.TryGetValue(player.PlayerGUID, out PlayerCache cache))
                {
                    player.Sex = cache.SexId;
                    player.PlayerRace = cache.RaceId;
                    player.PlayerClass = cache.ClassId;
                    player.Faction = GameData.IsAllianceRace(cache.RaceId);
                }
                else
                {
                    player.Sex = Gender.Male;
                    player.PlayerRace = Race.Human;
                    player.PlayerClass = Class.Warrior;
                }
                pvp.Statistics.Add(player);
            }
            SendPacketToClient(pvp);
        }

        [PacketHandler(Opcode.MSG_PVP_LOG_DATA, ClientVersionBuild.V2_0_1_6180)]
        void HandlePvPLogDataTBC(WorldPacket packet)
        {
            PVPMatchStatisticsMessage pvp = new PVPMatchStatisticsMessage();
            if (packet.ReadBool()) // Has Arena Teams
            {
                pvp.ArenaTeams = new ArenaTeamsInfo();
                pvp.ArenaTeams.Guids[0] = WowGuid128.Empty;
                pvp.ArenaTeams.Guids[1] = WowGuid128.Empty;

                for (int i = 0; i < 2; i++)
                {
                    packet.ReadUInt32(); // Rating Lost
                    packet.ReadUInt32(); // Rating gained
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                        packet.ReadUInt32(); // MMR
                }

                for (int i = 0; i < 2; i++)
                {
                    pvp.ArenaTeams.Names[i] = packet.ReadCString();
                }
            }

            if (packet.ReadBool()) // Has Winner
                pvp.Winner = packet.ReadUInt8();

            int count = packet.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                PVPMatchPlayerStatistics player = new PVPMatchPlayerStatistics
                {
                    PlayerGUID = packet.ReadGuid().To128(GetSession().GameState),
                    Kills = packet.ReadUInt32()
                };

                if (pvp.ArenaTeams == null)
                {
                    player.Honor = new()
                    {
                        HonorKills = packet.ReadUInt32(),
                        Deaths = packet.ReadUInt32(),
                        ContributionPoints = packet.ReadUInt32()
                    };
                }
                else
                {
                    player.Faction = packet.ReadBool();
                    pvp.PlayerCount[player.Faction ? 1 : 0]++;
                }

                player.DamageDone = packet.ReadUInt32();
                player.HealingDone = packet.ReadUInt32();

                int statsCount = packet.ReadInt32();
                for (int j = 0; j < statsCount; j++)
                    player.Stats.Add(packet.ReadUInt32());

                if (GetSession().GameState.CachedPlayers.TryGetValue(player.PlayerGUID, out PlayerCache cache))
                {
                    player.Sex = cache.SexId;
                    player.PlayerRace = cache.RaceId;
                    player.PlayerClass = cache.ClassId;

                    if (pvp.ArenaTeams == null)
                        player.Faction = GameData.IsAllianceRace(cache.RaceId);
                }
                else
                {
                    player.Sex = Gender.Male;
                    player.PlayerRace = Race.Human;
                    player.PlayerClass = Class.Warrior;
                }
                pvp.Statistics.Add(player);
            }
            SendPacketToClient(pvp);
        }

        BattlegroundPlayerPosition ReadBattlegroundPlayerPosition(WorldPacket packet)
        {
            BattlegroundPlayerPosition position = new BattlegroundPlayerPosition
            {
                Guid = packet.ReadGuid().To128(GetSession().GameState),
                Pos = packet.ReadVector2()
            };
            return position;
        }

        [PacketHandler(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleBattlegroundPlayerPositionsVanilla(WorldPacket packet)
        {
            GetSession().GameState.FlagCarrierGuids.Clear();
            BattlegroundPlayerPositions bglist = new BattlegroundPlayerPositions();
            uint teamMembersCount = packet.ReadUInt32();
            for (uint i = 0; i < teamMembersCount; i++)
            {
                ReadBattlegroundPlayerPosition(packet);
            }

            bool hasFlagCarrier = packet.ReadBool();
            if (hasFlagCarrier)
            {
                var position = ReadBattlegroundPlayerPosition(packet);

                if (GetSession().GameState.IsAlliancePlayer(position.Guid))
                {
                    position.IconID = 1;
                    position.ArenaSlot = 3;
                }
                else
                {
                    position.IconID = 2;
                    position.ArenaSlot = 2;
                }

                bglist.FlagCarriers.Add(position);
                GetSession().GameState.FlagCarrierGuids.Add(position.Guid);
            }
            SendPacketToClient(bglist);
        }

        [PacketHandler(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, ClientVersionBuild.V2_0_1_6180)]
        void HandleBattlegroundPlayerPositionsTBC(WorldPacket packet)
        {
            BattlegroundPlayerPositions bglist = new BattlegroundPlayerPositions();
            uint teamMembersCount = packet.ReadUInt32();
            uint flagCarriersCount = packet.ReadUInt32();
            for (uint i = 0; i < teamMembersCount; i++)
            {
                ReadBattlegroundPlayerPosition(packet);
            }
            GetSession().GameState.FlagCarrierGuids.Clear();
            for (uint i = 0; i < flagCarriersCount; i++)
            {
                var position = ReadBattlegroundPlayerPosition(packet);

                if (GetSession().GameState.IsAlliancePlayer(position.Guid))
                {
                    position.IconID = 1;
                    position.ArenaSlot = 3;
                }
                else
                {
                    position.IconID = 2;
                    position.ArenaSlot = 2;
                }

                bglist.FlagCarriers.Add(position);
                GetSession().GameState.FlagCarrierGuids.Add(position.Guid);
            }
            SendPacketToClient(bglist);
        }

        [PacketHandler(Opcode.SMSG_BATTLEGROUND_PLAYER_JOINED)]
        [PacketHandler(Opcode.SMSG_BATTLEGROUND_PLAYER_LEFT)]
        void HandleBattlegroundPlayerLeftOrJoined(WorldPacket packet)
        {
            BattlegroundPlayerLeftOrJoined player = new BattlegroundPlayerLeftOrJoined(packet.GetUniversalOpcode(false))
            {
                Guid = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(player);
        }

        [PacketHandler(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME)]
        void HandleAreaSpiritHealerTime(WorldPacket packet)
        {
            AreaSpiritHealerTime healer = new AreaSpiritHealerTime
            {
                HealerGuid = packet.ReadGuid().To128(GetSession().GameState),
                TimeLeft = packet.ReadUInt32()
            };
            SendPacketToClient(healer);
        }

        [PacketHandler(Opcode.SMSG_PVP_CREDIT)]
        void HandlePvPCredit(WorldPacket packet)
        {
            PvPCredit credit = new PvPCredit
            {
                OriginalHonor = packet.ReadInt32(),
                Target = packet.ReadGuid().To128(GetSession().GameState),
                Rank = packet.ReadUInt32()
            };
            SendPacketToClient(credit);
        }

        [PacketHandler(Opcode.SMSG_PLAYER_SKINNED)]
        void HandlePlayerSkinned(WorldPacket packet)
        {
            PlayerSkinned skinned = new PlayerSkinned();
            if (packet.CanRead())
                skinned.FreeRepop = packet.ReadBool();
            SendPacketToClient(skinned);
        }
    }
}
