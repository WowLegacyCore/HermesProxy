﻿using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_PARTY_COMMAND_RESULT)]
        void HandlePartyCommandResult(WorldPacket packet)
        {
            PartyCommandResult party = new()
            {
                Command = (byte)packet.ReadUInt32(),
                Name = packet.ReadCString(),
                Result = (byte)packet.ReadUInt32()
            };
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                party.ResultData = packet.ReadUInt32();
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_DECLINE)]
        void HandleGroupDecline(WorldPacket packet)
        {
            GroupDecline party = new()
            {
                Name = packet.ReadCString()
            };
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_PARTY_INVITE)]
        void HandleGroupInvite(WorldPacket packet)
        {
            PartyInvite party = new();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                party.CanAccept = packet.ReadBool();

            var realm = GetSession().RealmManager.GetRealm(GetSession().RealmId);
            party.InviterRealm = new VirtualRealmInfo(realm.Id.GetAddress(), true, false, realm.Name, realm.NormalizedName);

            party.InviterName = packet.ReadCString();
            party.InviterGUID = GetSession().GameState.GetPlayerGuidByName(party.InviterName);
            if (party.InviterGUID == null)
            {
                party.InviterGUID = WowGuid128.Empty;
                party.InviterBNetAccountId = WowGuid128.Empty;
            }
            else
                party.InviterBNetAccountId = GetSession().GetBnetAccountGuidForPlayer(party.InviterGUID);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                party.ProposedRoles = packet.ReadUInt32();
                var lfgSlotsCount = packet.ReadUInt8();
                for (var i = 0; i < lfgSlotsCount; ++i)
                    party.LfgSlots.Add(packet.ReadInt32());
                party.LfgCompletedMask = packet.ReadInt32();
            }

            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleGroupListVanilla(WorldPacket packet)
        {
            PartyUpdate party = new()
            {
                SequenceNum = GetSession().GameState.GroupUpdateCounter++
            };
            bool isRaid = packet.ReadBool();
            byte ownSubGroupAndFlags = packet.ReadUInt8();
            party.PartyIndex = (byte)(isRaid && GetSession().GameState.IsInBattleground() ? 1 : 0);
            party.PartyGUID = WowGuid128.Create(HighGuidType703.Party, (ulong)(1000 + party.PartyIndex));
            if (party.PartyIndex != 0)
                party.PartyFlags |= GroupFlags.FakeRaid;

            var uniqueMembers = new HashSet<WowGuid128>();
            uint membersCount = packet.ReadUInt32();
            if (membersCount > 0)
            {
                if (isRaid)
                    party.PartyFlags |= GroupFlags.Raid;

                party.DifficultySettings = new PartyDifficultySettings
                {
                    DungeonDifficultyID = Difficulty.Normal
                };

                if (ModernVersion.ExpansionVersion > 1)
                    party.DifficultySettings.RaidDifficultyID = Difficulty.Raid25N;
                else
                    party.DifficultySettings.RaidDifficultyID = Difficulty.Raid40;

                if (party.PartyIndex != 0)
                    party.PartyType = GroupType.PvP;
                else
                    party.PartyType = GroupType.Normal;

                PartyPlayerInfo player = new()
                {
                    GUID = GetSession().GameState.CurrentPlayerGuid
                };
                player.Name = GetSession().GameState.GetPlayerName(player.GUID);
                player.Subgroup = (byte)(ownSubGroupAndFlags & 0xF);
                player.Flags = (ownSubGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
                player.Status = GroupMemberOnlineStatus.Online;
                party.PlayerList.Add(player);

                bool allAssist = true;
                for (uint i = 0; i < membersCount; i++)
                {
                    PartyPlayerInfo member = new()
                    {
                        Name = packet.ReadCString(),
                        GUID = packet.ReadGuid().To128(GetSession().GameState),
                        Status = (GroupMemberOnlineStatus)packet.ReadUInt8()
                    };
                    byte subGroupAndFlags = packet.ReadUInt8();
                    member.Subgroup = (byte)(subGroupAndFlags & 0xF);
                    member.Flags = (subGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
                    member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                    if (!member.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
                        allAssist = false;

                    if (!uniqueMembers.Contains(member.GUID))
                    {
                        party.PlayerList.Add(member);
                        uniqueMembers.Add(member.GUID);
                    }
                }

                if (allAssist)
                    party.PartyFlags |= GroupFlags.EveryoneAssistant;

                party.LeaderGUID = packet.ReadGuid().To128(GetSession().GameState);

                party.LootSettings = new PartyLootSettings
                {
                    Method = (LootMethod)packet.ReadUInt8(),
                    LootMaster = packet.ReadGuid().To128(GetSession().GameState),
                    Threshold = packet.ReadUInt8()
                };
                GetSession().GameState.CurrentGroups[party.PartyIndex] = party;
            }
            else
            {
                party.PartyFlags |= GroupFlags.Destroyed;
                if (party.PartyIndex == 0)
                    party.PartyGUID = WowGuid128.Empty;
                party.LeaderGUID = WowGuid128.Empty;
                party.MyIndex = -1;
                GetSession().GameState.CurrentGroups[party.PartyIndex] = null;
            }

            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.V2_0_1_6180)]
        void HandleGroupListTBC(WorldPacket packet)
        {
            PartyUpdate party = new()
            {
                SequenceNum = GetSession().GameState.GroupUpdateCounter++
            };
            bool isRaid = packet.ReadBool();
            bool isBattleground = packet.ReadBool();
            byte ownSubGroup = packet.ReadUInt8();
            byte ownGroupFlags = packet.ReadUInt8();
            party.PartyIndex = (byte)(isBattleground ? 1 : 0);
            party.PartyGUID = packet.ReadGuid().To128(GetSession().GameState);
            if (party.PartyIndex != 0)
                party.PartyFlags |= GroupFlags.FakeRaid;

            var uniqueMembers = new HashSet<WowGuid128>();
            uint membersCount = packet.ReadUInt32();
            if (membersCount > 0)
            {
                if (isRaid)
                    party.PartyFlags |= GroupFlags.Raid;

                if (party.PartyIndex != 0)
                    party.PartyType = GroupType.PvP;
                else
                    party.PartyType = GroupType.Normal;

                PartyPlayerInfo player = new()
                {
                    GUID = GetSession().GameState.CurrentPlayerGuid
                };
                player.Name = GetSession().GameState.GetPlayerName(player.GUID);
                player.Subgroup = ownSubGroup;
                player.Flags = (GroupMemberFlags)ownGroupFlags;
                player.Status = GroupMemberOnlineStatus.Online;
                party.PlayerList.Add(player);

                bool allAssist = true;
                for (uint i = 0; i < membersCount; i++)
                {
                    PartyPlayerInfo member = new()
                    {
                        Name = packet.ReadCString(),
                        GUID = packet.ReadGuid().To128(GetSession().GameState),
                        Status = (GroupMemberOnlineStatus)packet.ReadUInt8(),
                        Subgroup = packet.ReadUInt8(),
                        Flags = (GroupMemberFlags)packet.ReadUInt8()
                    };
                    member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                    if (!member.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
                        allAssist = false;

                    if (!uniqueMembers.Contains(member.GUID))
                    {
                        party.PlayerList.Add(member);
                        uniqueMembers.Add(member.GUID);
                    }
                }

                if (allAssist)
                    party.PartyFlags |= GroupFlags.EveryoneAssistant;

                party.LeaderGUID = packet.ReadGuid().To128(GetSession().GameState);

                party.LootSettings = new PartyLootSettings
                {
                    Method = (LootMethod)packet.ReadUInt8(),
                    LootMaster = packet.ReadGuid().To128(GetSession().GameState),
                    Threshold = packet.ReadUInt8()
                };

                party.DifficultySettings = new PartyDifficultySettings
                {
                    DungeonDifficultyID = (Difficulty)packet.ReadUInt8()
                };

                if (ModernVersion.ExpansionVersion > 1)
                    party.DifficultySettings.RaidDifficultyID = Difficulty.Raid25N;
                else
                    party.DifficultySettings.RaidDifficultyID = Difficulty.Raid40;

                GetSession().GameState.CurrentGroups[party.PartyIndex] = party;
            }
            else
            {
                party.PartyFlags |= GroupFlags.Destroyed;
                if (party.PartyIndex  == 0)
                    party.PartyGUID = WowGuid128.Empty;
                party.LeaderGUID = WowGuid128.Empty;
                party.MyIndex = -1;
                GetSession().GameState.CurrentGroups[party.PartyIndex] = null;
            }

            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_UNINVITE)]
        void HandleGroupUninvite(WorldPacket packet)
        {
            GroupUninvite party = new();
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_NEW_LEADER)]
        void HandleGroupNewLeader(WorldPacket packet)
        {
            GroupNewLeader party = new()
            {
                Name = packet.ReadCString(),
                PartyIndex = GetSession().GameState.GetCurrentPartyIndex()
            };
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheckVanilla(WorldPacket packet)
        {
            if (!packet.CanRead())
            {
                ReadyCheckStarted ready = new()
                {
                    InitiatorGUID = GetSession().GameState.GetCurrentGroupLeader(),
                    PartyIndex = GetSession().GameState.GetCurrentPartyIndex(),
                    PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
                };
                SendPacketToClient(ready);
            }
            else
            {
                ReadyCheckResponse ready = new()
                {
                    Player = packet.ReadGuid().To128(GetSession().GameState),
                    IsReady = packet.ReadBool(),
                    PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
                };
                SendPacketToClient(ready);

                GetSession().GameState.GroupReadyCheckResponses++;
                if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.GetCurrentGroupSize())
                {
                    GetSession().GameState.GroupReadyCheckResponses = 0;
                    ReadyCheckCompleted completed = new()
                    {
                        PartyIndex = GetSession().GameState.GetCurrentPartyIndex(),
                        PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
                    };
                    SendPacketToClient(completed);
                }
            }
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheck(WorldPacket packet)
        {
            ReadyCheckStarted ready = new()
            {
                InitiatorGUID = packet.ReadGuid().To128(GetSession().GameState),
                PartyIndex = GetSession().GameState.GetCurrentPartyIndex(),
                PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
            };
            SendPacketToClient(ready);
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK_CONFIRM, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheckConfirm(WorldPacket packet)
        {
            ReadyCheckResponse ready = new()
            {
                Player = packet.ReadGuid().To128(GetSession().GameState),
                IsReady = packet.ReadBool(),
                PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
            };
            SendPacketToClient(ready);

            GetSession().GameState.GroupReadyCheckResponses++;
            if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.GetCurrentGroupSize())
            {
                GetSession().GameState.GroupReadyCheckResponses = 0;
                ReadyCheckCompleted completed = new()
                {
                    PartyIndex = GetSession().GameState.GetCurrentPartyIndex(),
                    PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
                };
                SendPacketToClient(completed);
            }
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK_FINISHED, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheckFinished(WorldPacket packet)
        {
            ReadyCheckCompleted ready = new()
            {
                PartyIndex = GetSession().GameState.GetCurrentPartyIndex(),
                PartyGUID = GetSession().GameState.GetCurrentGroupGuid()
            };
            SendPacketToClient(ready);
        }

        [PacketHandler(Opcode.MSG_RAID_TARGET_UPDATE)]
        void HandleRaidTargetUpdate(WorldPacket packet)
        {
            bool isFullUpdate = packet.ReadBool();
            if (isFullUpdate)
            {
                SendRaidTargetUpdateAll update = new()
                {
                    PartyIndex = GetSession().GameState.GetCurrentPartyIndex()
                };
                while (packet.CanRead())
                {
                    sbyte symbol = packet.ReadInt8();
                    WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
                    update.TargetIcons.Add(new Tuple<sbyte, WowGuid128>(symbol, guid));
                }
                SendPacketToClient(update);
            }
            else
            {
                SendRaidTargetUpdateSingle update = new()
                {
                    PartyIndex = GetSession().GameState.GetCurrentPartyIndex()
                };

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    update.ChangedBy = packet.ReadGuid().To128(GetSession().GameState);
                else
                    update.ChangedBy = GetSession().GameState.CurrentPlayerGuid;

                update.Symbol = packet.ReadInt8();
                update.Target = packet.ReadGuid().To128(GetSession().GameState);
                SendPacketToClient(update);
            }
        }

        [PacketHandler(Opcode.SMSG_SUMMON_REQUEST)]
        void HandleSummonRequest(WorldPacket packet)
        {
            SummonRequest summon = new()
            {
                SummonerGUID = packet.ReadGuid().To128(GetSession().GameState),
                SummonerVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                AreaID = packet.ReadInt32()
            };
            packet.ReadUInt32(); // time to accept
            SendPacketToClient(summon);
        }

        uint _requestBgPlayerPosCounter = 0;

        [PacketHandler(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandlePartyMemberStats(WorldPacket packet)
        {
            if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
               (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
            {
                if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
                {
                    WorldPacket packet2 = new(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberPartialState state = new()
            {
                AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState)
            };
            var updateFlags = (GroupUpdateFlagVanilla)packet.ReadUInt32();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Status))
                state.StatusFlags = packet.ReadUInt8();// GroupMemberOnlineStatus

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentHealth))
                state.CurrentHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxHealth))
                state.MaxHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PowerType))
                state.PowerType = packet.ReadUInt8();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentPower))
                state.CurrentPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxPower))
                state.MaxPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Level))
                state.Level = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Zone))
                state.ZoneID = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Position))
            {
                state.Position = new PartyMemberPartialState.Vector3_UInt16
                {
                    X = packet.ReadInt16(),
                    Y = packet.ReadInt16()
                };
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Auras))
            {
                state.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.AurasNegative))
            {
                state.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                    }
                    state.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetGuid))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetName))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetModelId))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.MaxHealth = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetPowerType))
                packet.ReadUInt8(); // Pet Power Type

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentPower))
                packet.ReadInt16(); // Pet Current Power

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxPower))
                packet.ReadInt16(); // Pet Max Power

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAuras))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Pet Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Pet.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAurasNegative))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Pet Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                    }
                    state.Pet.Auras.Add(aura);
                }
            }

            SendPacketToClient(state);
        }

        [PacketHandler(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, ClientVersionBuild.V2_0_1_6180)]
        void HandlePartyMemberStatsTbc(WorldPacket packet)
        {
            if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
               (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
            {
                if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
                {
                    WorldPacket packet2 = new(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberPartialState state = new()
            {
                AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState)
            };
            var updateFlags = (GroupUpdateFlagTBC)packet.ReadUInt32();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Status))
                state.StatusFlags = packet.ReadUInt16();// GroupMemberOnlineStatus

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentHealth))
                state.CurrentHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxHealth))
                state.MaxHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PowerType))
                state.PowerType = packet.ReadUInt8();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentPower))
                state.CurrentPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxPower))
                state.MaxPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Level))
                state.Level = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Zone))
                state.ZoneID = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Position))
            {
                state.Position = new PartyMemberPartialState.Vector3_UInt16
                {
                    X = packet.ReadInt16(),
                    Y = packet.ReadInt16()
                };
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Auras))
            {
                state.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    packet.ReadUInt8(); // unk
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetGuid))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetName))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetModelId))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.MaxHealth = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetPowerType))
                packet.ReadUInt8(); // Pet Power Type

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentPower))
                packet.ReadInt16(); // Pet Current Power

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxPower))
                packet.ReadInt16(); // Pet Max Power

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetAuras))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    packet.ReadUInt8(); // unk
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Pet.Auras.Add(aura);
                }
            }

            SendPacketToClient(state);
        }

        [PacketHandler(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandlePartyMemberStatsFull(WorldPacket packet)
        {
            if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
               (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
            {
                if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
                {
                    WorldPacket packet2 = new(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberFullState state = new();
            if (GetSession().GameState.IsInBattleground())
            {
                state.PartyType[0] = 0;
                state.PartyType[1] = 2;
            }
            else
            {
                state.PartyType[0] = 1;
                state.PartyType[1] = 0;
            }

            state.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
            var updateFlags = (GroupUpdateFlagVanilla)packet.ReadUInt32();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Status))
                state.StatusFlags = (GroupMemberOnlineStatus)packet.ReadUInt8();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentHealth))
                state.CurrentHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxHealth))
                state.MaxHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PowerType))
                state.PowerType = packet.ReadUInt8();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.CurrentPower))
                state.CurrentPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.MaxPower))
                state.MaxPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Level))
                state.Level = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Zone))
                state.ZoneID = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Position))
            {
                state.PositionX = packet.ReadInt16();
                state.PositionY = packet.ReadInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Auras))
            {
                state.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.AurasNegative))
            {
                state.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                    }
                    state.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetGuid))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetName))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetModelId))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.MaxHealth = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetPowerType))
                packet.ReadUInt8(); // Pet Power Type

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentPower))
                packet.ReadInt16(); // Pet Current Power

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxPower))
                packet.ReadInt16(); // Pet Max Power

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAuras))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Pet Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Pet.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetAurasNegative))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Pet Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Negative;
                    }
                    state.Pet.Auras.Add(aura);
                }
            }

            SendPacketToClient(state);
        }

        [PacketHandler(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, ClientVersionBuild.V2_0_1_6180)]
        void HandlePartyMemberStatsFullTBC(WorldPacket packet)
        {
            if (GetSession().GameState.CurrentMapId == (uint)BattlegroundMapID.WarsongGulch &&
               (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier))
            {
                if (_requestBgPlayerPosCounter++ > 10) // don't spam every time somebody moves
                {
                    WorldPacket packet2 = new(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberFullState state = new();
            if (GetSession().GameState.IsInBattleground())
            {
                state.PartyType[0] = 0;
                state.PartyType[1] = 2;
            }
            else
            {
                state.PartyType[0] = 1;
                state.PartyType[1] = 0;
            }

            state.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
            var updateFlags = (GroupUpdateFlagTBC)packet.ReadUInt32();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Status))
                state.StatusFlags = (GroupMemberOnlineStatus)packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentHealth))
                state.CurrentHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxHealth))
                state.MaxHealth = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PowerType))
                state.PowerType = packet.ReadUInt8();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.CurrentPower))
                state.CurrentPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.MaxPower))
                state.MaxPower = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Level))
                state.Level = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Zone))
                state.ZoneID = packet.ReadUInt16();

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Position))
            {
                state.PositionX = packet.ReadInt16();
                state.PositionY = packet.ReadInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Auras))
            {
                state.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    packet.ReadUInt8(); // unk
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Auras.Add(aura);
                }
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetGuid))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetName))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetModelId))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.MaxHealth = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetPowerType))
                packet.ReadUInt8(); // Pet Power Type

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentPower))
                packet.ReadInt16(); // Pet Current Power

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxPower))
                packet.ReadInt16(); // Pet Max Power

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetAuras))
            {
                state.Pet ??= new PartyMemberPetStats();
                state.Pet.Auras ??= new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new()
                    {
                        SpellId = packet.ReadUInt16()
                    };
                    packet.ReadUInt8(); // unk
                    if (aura.SpellId != 0)
                    {
                        aura.ActiveFlags = 1;
                        aura.AuraFlags = (ushort)AuraFlagsModern.Positive;
                    }
                    state.Pet.Auras.Add(aura);
                }
            }

            SendPacketToClient(state);
        }

        [PacketHandler(Opcode.MSG_MINIMAP_PING)]
        void HandleMinimapPing(WorldPacket packet)
        {
            MinimapPing ping = new()
            {
                SenderGUID = packet.ReadGuid().To128(GetSession().GameState),
                Position = packet.ReadVector2()
            };
            SendPacketToClient(ping);
        }

        [PacketHandler(Opcode.MSG_RANDOM_ROLL)]
        void HandleRandomRoll(WorldPacket packet)
        {
            RandomRoll roll = new()
            {
                Min = packet.ReadInt32(),
                Max = packet.ReadInt32(),
                Result = packet.ReadInt32(),
                Roller = packet.ReadGuid().To128(GetSession().GameState)
            };
            roll.RollerWowAccount = GetSession().GetGameAccountGuidForPlayer(roll.Roller);
            SendPacketToClient(roll);
        }
    }
}
