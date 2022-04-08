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
        [PacketHandler(Opcode.SMSG_PARTY_COMMAND_RESULT)]
        void HandlePartyCommandResult(WorldPacket packet)
        {
            PartyCommandResult party = new PartyCommandResult();
            party.Command = (byte)packet.ReadUInt32();
            party.Name = packet.ReadCString();
            party.Result = (byte)packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                party.ResultData = packet.ReadUInt32();
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_DECLINE)]
        void HandleGroupDecline(WorldPacket packet)
        {
            GroupDecline party = new GroupDecline();
            party.Name = packet.ReadCString();
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_PARTY_INVITE)]
        void HandleGroupInvite(WorldPacket packet)
        {
            PartyInvite party = new PartyInvite();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                party.CanAccept = packet.ReadBool();

            var realm = RealmManager.Instance.GetRealm(GetSession().RealmId);
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
            PartyUpdate party = new PartyUpdate();
            party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
            bool isRaid = packet.ReadBool();
            byte ownSubGroupAndFlags = packet.ReadUInt8();

            GetSession().GameState.CurrentGroupMembers = new List<WowGuid128>();
            uint membersCount = packet.ReadUInt32();
            if (membersCount > 0)
            {
                if (isRaid)
                    party.PartyFlags |= GroupFlags.Raid;

                party.DifficultySettings = new PartyDifficultySettings();
                party.DifficultySettings.DungeonDifficultyID = 1;

                if (ModernVersion.GetExpansionVersion() > 1)
                    party.DifficultySettings.RaidDifficultyID = 4;
                else
                    party.DifficultySettings.RaidDifficultyID = 9;

                if (GetSession().GameState.IsInBattleground())
                {
                    party.PartyFlags |= GroupFlags.FakeRaid;
                    party.PartyIndex = 1;
                    party.PartyType = GroupType.PvP;
                }
                else
                    party.PartyType = GroupType.Normal;

                party.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);

                PartyPlayerInfo player = new PartyPlayerInfo();
                player.GUID = GetSession().GameState.CurrentPlayerGuid;
                player.Name = GetSession().GameState.GetPlayerName(player.GUID);
                player.Subgroup = (byte)(ownSubGroupAndFlags & 0xF);
                player.Flags = (ownSubGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
                player.Status = GroupMemberOnlineStatus.Online;
                party.PlayerList.Add(player);

                bool allAssist = true;
                for (uint i = 0; i < membersCount; i++)
                {
                    PartyPlayerInfo member = new PartyPlayerInfo();
                    member.Name = packet.ReadCString();
                    member.GUID = packet.ReadGuid().To128(GetSession().GameState);
                    member.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
                    byte subGroupAndFlags = packet.ReadUInt8();
                    member.Subgroup = (byte)(subGroupAndFlags & 0xF);
                    member.Flags = (subGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
                    member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                    if (!member.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
                        allAssist = false;
                    party.PlayerList.Add(member);
                    GetSession().GameState.CurrentGroupMembers.Add(member.GUID);
                }

                if (allAssist)
                    party.PartyFlags |= GroupFlags.EveryoneAssistant;

                party.LeaderGUID = GetSession().GameState.CurrentGroupLeader = packet.ReadGuid().To128(GetSession().GameState);

                party.LootSettings = new PartyLootSettings();
                party.LootSettings.Method = GetSession().GameState.CurrentGroupLootMethod = (LootMethod)packet.ReadUInt8();
                party.LootSettings.LootMaster = packet.ReadGuid().To128(GetSession().GameState);
                party.LootSettings.Threshold = packet.ReadUInt8();
            }
            else
            {
                GetSession().GameState.CurrentGroupLeader = null;
                GetSession().GameState.CurrentGroupLootMethod = LootMethod.FreeForAll;
                party.PartyFlags = GroupFlags.Destroyed;
                party.PartyGUID = WowGuid128.Empty;
                party.LeaderGUID = WowGuid128.Empty;
                party.MyIndex = -1;
            }

            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.V2_0_1_6180)]
        void HandleGroupListTBC(WorldPacket packet)
        {
            PartyUpdate party = new PartyUpdate();
            party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
            bool isRaid = packet.ReadBool();
            bool isBattleground = packet.ReadBool();
            byte ownSubGroup = packet.ReadUInt8();
            byte ownGroupFlags = packet.ReadUInt8();
            party.PartyGUID = packet.ReadGuid().To128(GetSession().GameState);

            GetSession().GameState.CurrentGroupMembers = new List<WowGuid128>();
            uint membersCount = packet.ReadUInt32();
            if (membersCount > 0)
            {
                if (isRaid)
                    party.PartyFlags |= GroupFlags.Raid;

                if (isBattleground)
                {
                    party.PartyFlags |= GroupFlags.FakeRaid;
                    party.PartyIndex = 1;
                    party.PartyType = GroupType.PvP;
                }
                else
                    party.PartyType = GroupType.Normal;

                PartyPlayerInfo player = new PartyPlayerInfo();
                player.GUID = GetSession().GameState.CurrentPlayerGuid;
                player.Name = GetSession().GameState.GetPlayerName(player.GUID);
                player.Subgroup = ownSubGroup;
                player.Flags = (GroupMemberFlags)ownGroupFlags;
                player.Status = GroupMemberOnlineStatus.Online;
                party.PlayerList.Add(player);

                bool allAssist = true;
                for (uint i = 0; i < membersCount; i++)
                {
                    PartyPlayerInfo member = new PartyPlayerInfo();
                    member.Name = packet.ReadCString();
                    member.GUID = packet.ReadGuid().To128(GetSession().GameState);
                    member.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
                    member.Subgroup = packet.ReadUInt8();
                    member.Flags = (GroupMemberFlags)packet.ReadUInt8();
                    member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                    if (!member.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
                        allAssist = false;
                    party.PlayerList.Add(member);
                    GetSession().GameState.CurrentGroupMembers.Add(member.GUID);
                }

                if (allAssist)
                    party.PartyFlags |= GroupFlags.EveryoneAssistant;

                party.LeaderGUID = GetSession().GameState.CurrentGroupLeader = packet.ReadGuid().To128(GetSession().GameState);

                party.LootSettings = new PartyLootSettings();
                party.LootSettings.Method = GetSession().GameState.CurrentGroupLootMethod = (LootMethod)packet.ReadUInt8();
                party.LootSettings.LootMaster = packet.ReadGuid().To128(GetSession().GameState);
                party.LootSettings.Threshold = packet.ReadUInt8();

                party.DifficultySettings = new PartyDifficultySettings();
                party.DifficultySettings.DungeonDifficultyID = packet.ReadUInt8();

                if (ModernVersion.GetExpansionVersion() > 1)
                    party.DifficultySettings.RaidDifficultyID = 4;
                else
                    party.DifficultySettings.RaidDifficultyID = 9;
            }
            else
            {
                GetSession().GameState.CurrentGroupLeader = null;
                GetSession().GameState.CurrentGroupLootMethod = LootMethod.FreeForAll;
                party.PartyFlags = GroupFlags.Destroyed;
                party.PartyGUID = WowGuid128.Empty;
                party.LeaderGUID = WowGuid128.Empty;
                party.MyIndex = -1;
            }

            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_UNINVITE)]
        void HandleGroupUninvite(WorldPacket packet)
        {
            GroupUninvite party = new GroupUninvite();
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.SMSG_GROUP_NEW_LEADER)]
        void HandleGroupNewLeader(WorldPacket packet)
        {
            GroupNewLeader party = new GroupNewLeader();
            party.Name = packet.ReadCString();
            party.PartyIndex = (sbyte)(GetSession().GameState.IsInBattleground() ? 1 : 0);
            SendPacketToClient(party);
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheckVanilla(WorldPacket packet)
        {
            if (!packet.CanRead())
            {
                ReadyCheckStarted ready = new ReadyCheckStarted();
                ready.InitiatorGUID = GetSession().GameState.CurrentGroupLeader;
                ready.PartyIndex = (sbyte)(GetSession().GameState.IsInBattleground() ? 1 : 0);
                ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
                SendPacketToClient(ready);
            }
            else
            {
                ReadyCheckResponse ready = new ReadyCheckResponse();
                ready.Player = packet.ReadGuid().To128(GetSession().GameState);
                ready.IsReady = packet.ReadBool();
                ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
                SendPacketToClient(ready);

                GetSession().GameState.GroupReadyCheckResponses++;
                if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.CurrentGroupMembers.Count)
                {
                    GetSession().GameState.GroupReadyCheckResponses = 0;
                    ReadyCheckCompleted completed = new ReadyCheckCompleted();
                    completed.PartyIndex = (sbyte)(GetSession().GameState.IsInBattleground() ? 1 : 0);
                    completed.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
                    SendPacketToClient(completed);
                }
            }
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheck(WorldPacket packet)
        {
            ReadyCheckStarted ready = new ReadyCheckStarted();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                ready.InitiatorGUID = packet.ReadGuid().To128(GetSession().GameState);
            else
                ready.InitiatorGUID = WowGuid128.Empty;
            ready.PartyIndex = (sbyte)(GetSession().GameState.IsInBattleground() ? 1 : 0);
            ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
            SendPacketToClient(ready);
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK_CONFIRM, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheckConfirm(WorldPacket packet)
        {
            ReadyCheckResponse ready = new ReadyCheckResponse();
            ready.Player = packet.ReadGuid().To128(GetSession().GameState);
            ready.IsReady = packet.ReadBool();
            ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
            SendPacketToClient(ready);
        }

        [PacketHandler(Opcode.MSG_RAID_READY_CHECK_FINISHED, ClientVersionBuild.V2_0_1_6180)]
        void HandleRaidReadyCheckFinished(WorldPacket packet)
        {
            ReadyCheckCompleted ready = new ReadyCheckCompleted();
            ready.PartyIndex = (sbyte)(GetSession().GameState.IsInBattleground() ? 1 : 0);
            ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
            SendPacketToClient(ready);
        }

        [PacketHandler(Opcode.MSG_RAID_TARGET_UPDATE)]
        void HandleRaidTargetUpdate(WorldPacket packet)
        {
            bool isFullUpdate = packet.ReadBool();
            if (isFullUpdate)
            {
                SendRaidTargetUpdateAll update = new SendRaidTargetUpdateAll();
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
                SendRaidTargetUpdateSingle update = new SendRaidTargetUpdateSingle();

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
            SummonRequest summon = new SummonRequest();
            summon.SummonerGUID = packet.ReadGuid().To128(GetSession().GameState);
            summon.SummonerVirtualRealmAddress = GetSession().RealmId.GetAddress();
            summon.AreaID = packet.ReadInt32();
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
                    WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberPartialState state = new PartyMemberPartialState();
            state.AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
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
                state.Position = new PartyMemberPartialState.Vector3_UInt16();
                state.Position.X = packet.ReadInt16();
                state.Position.Y = packet.ReadInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.Auras))
            {
                if (state.Auras == null)
                    state.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Auras == null)
                    state.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }
                

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetName))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetModelId))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                if (state.Pet.Auras == null)
                    state.Pet.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Pet Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                if (state.Pet.Auras == null)
                    state.Pet.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Pet Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                    WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberPartialState state = new PartyMemberPartialState();
            state.AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
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
                state.Position = new PartyMemberPartialState.Vector3_UInt16();
                state.Position.X = packet.ReadInt16();
                state.Position.Y = packet.ReadInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.Auras))
            {
                if (state.Auras == null)
                    state.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetName))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetModelId))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                if (state.Pet.Auras == null)
                    state.Pet.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                    WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberFullState state = new PartyMemberFullState();
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
                if (state.Auras == null)
                    state.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Auras == null)
                    state.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetName))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetModelId))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                if (state.Pet.Auras == null)
                    state.Pet.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt32(); // Pet Positive Aura Mask

                byte maxAura = 32;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                if (state.Pet.Auras == null)
                    state.Pet.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt16(); // Pet Negative Aura Mask

                byte maxAura = 48;

                for (byte i = 0; i < maxAura; ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                    WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
                    SendPacket(packet2);
                    _requestBgPlayerPosCounter = 0;
                }
            }

            PartyMemberFullState state = new PartyMemberFullState();
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
                if (state.Auras == null)
                    state.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
            }


            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetName))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.NewPetName = packet.ReadCString();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetModelId))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.DisplayID = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                state.Pet.Health = packet.ReadUInt16();
            }

            if (updateFlags.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
            {
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
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
                if (state.Pet == null)
                    state.Pet = new PartyMemberPetStats();
                if (state.Pet.Auras == null)
                    state.Pet.Auras = new List<PartyMemberAuraStates>();

                var auraMask = packet.ReadUInt64();

                for (byte i = 0; i < LegacyVersion.GetAuraSlotsCount(); ++i)
                {
                    if ((auraMask & (1ul << i)) == 0)
                        continue;

                    PartyMemberAuraStates aura = new PartyMemberAuraStates();
                    aura.SpellId = packet.ReadUInt16();
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
            MinimapPing ping = new MinimapPing();
            ping.SenderGUID = packet.ReadGuid().To128(GetSession().GameState);
            ping.Position = packet.ReadVector2();
            SendPacketToClient(ping);
        }

        [PacketHandler(Opcode.MSG_RANDOM_ROLL)]
        void HandleRandomRoll(WorldPacket packet)
        {
            RandomRoll roll = new RandomRoll();
            roll.Min = packet.ReadInt32();
            roll.Max = packet.ReadInt32();
            roll.Result = packet.ReadInt32();
            roll.Roller = packet.ReadGuid().To128(GetSession().GameState);
            roll.RollerWowAccount = GetSession().GetGameAccountGuidForPlayer(roll.Roller);
            SendPacketToClient(roll);
        }
    }
}
