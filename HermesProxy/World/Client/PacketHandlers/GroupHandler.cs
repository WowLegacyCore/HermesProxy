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
        void HandleGroupList(WorldPacket packet)
        {
            PartyUpdate party = new PartyUpdate();
            party.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
            bool isRaid = packet.ReadBool();
            byte ownSubGroupAndFlags = packet.ReadUInt8();
            
            uint membersCount = GetSession().GameState.CurrentGroupSize = packet.ReadUInt32();
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

                for (uint i = 0; i < membersCount; i++)
                {
                    PartyPlayerInfo member = new PartyPlayerInfo();
                    member.Name = packet.ReadCString();
                    member.GUID = packet.ReadGuid().To128();
                    member.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
                    byte subGroupAndFlags = packet.ReadUInt8();
                    member.Subgroup = (byte)(subGroupAndFlags & 0xF);
                    member.Flags = (subGroupAndFlags & 0x80) != 0 ? GroupMemberFlags.Assistant : GroupMemberFlags.None;
                    member.ClassId = GetSession().GameState.GetUnitClass(member.GUID);
                    party.PlayerList.Add(member);
                }

                party.LeaderGUID = GetSession().GameState.CurrentGroupLeader = packet.ReadGuid().To128();

                party.LootSettings = new PartyLootSettings();
                party.LootSettings.Method = packet.ReadUInt8();
                party.LootSettings.LootMaster = packet.ReadGuid().To128();
                party.LootSettings.Threshold = packet.ReadUInt8();
            }
            else
            {
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
                ready.Player = packet.ReadGuid().To128();
                ready.IsReady = packet.ReadBool();
                ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
                SendPacketToClient(ready);

                GetSession().GameState.GroupReadyCheckResponses++;
                if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.CurrentGroupSize)
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
                ready.InitiatorGUID = packet.ReadGuid().To128();
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
            ready.Player = packet.ReadGuid().To128();
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
                    WowGuid128 guid = packet.ReadGuid().To128();
                    update.TargetIcons.Add(new Tuple<sbyte, WowGuid128>(symbol, guid));
                }
                SendPacketToClient(update);
            }
            else
            {
                SendRaidTargetUpdateSingle update = new SendRaidTargetUpdateSingle();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    update.ChangedBy = packet.ReadGuid().To128();
                else
                    update.ChangedBy = GetSession().GameState.CurrentPlayerGuid;
                
                update.Symbol = packet.ReadInt8();
                update.Target = packet.ReadGuid().To128();
                SendPacketToClient(update);
            }
        }

        [PacketHandler(Opcode.SMSG_SUMMON_REQUEST)]
        void HandleSummonRequest(WorldPacket packet)
        {
            SummonRequest summon = new SummonRequest();
            summon.SummonerGUID = packet.ReadGuid().To128();
            summon.SummonerVirtualRealmAddress = GetSession().RealmId.GetAddress();
            summon.AreaID = packet.ReadInt32();
            packet.ReadUInt32(); // time to accept
            SendPacketToClient(summon);
        }

        uint _requestBgPlayerPosCounter = 0;

        [PacketHandler(Opcode.SMSG_PARTY_MEMBER_STATS)]
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
        }
    }
}
