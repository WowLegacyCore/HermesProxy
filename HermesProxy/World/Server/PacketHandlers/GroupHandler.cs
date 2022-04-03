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
        [PacketHandler(Opcode.CMSG_PARTY_INVITE)]
        void HandleUpdateRaidTarget(PartyInviteClient invite)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PARTY_INVITE);
            packet.WriteCString(invite.TargetName);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PARTY_INVITE_RESPONSE)]
        void HandlePartyInviteResponse(PartyInviteResponse invite)
        {
            if (invite.Accept)
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_ACCEPT);
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    packet.WriteUInt32(0);
                SendPacketToServer(packet);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_DECLINE);
                SendPacketToServer(packet);
            }
        }

        [PacketHandler(Opcode.CMSG_LEAVE_GROUP)]
        void HandleLeaveGroup(LeaveGroup leave)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_DISBAND);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PARTY_UNINVITE)]
        void HandlePartyUninvite(PartyUninvite kick)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_UNINVITE_GUID);
            packet.WriteGuid(kick.TargetGUID.To64());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteCString(kick.Reason);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_PARTY_LEADER)]
        void HandleSetPartyLeader(SetPartyLeader leader)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_PARTY_LEADER);
            packet.WriteGuid(leader.TargetGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CONVERT_RAID)]
        void HandleConvertRaid(ConvertRaid raid)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CONVERT_RAID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_DO_READY_CHECK)]
        void HandlReadyCheck(DoReadyCheck raid)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_READY_CHECK);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_READY_CHECK_RESPONSE)]
        void HandlReadyCheckResponse(ReadyCheckResponseClient raid)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_READY_CHECK);
            packet.WriteBool(raid.IsReady);
            SendPacketToServer(packet);

            ReadyCheckResponse ready = new ReadyCheckResponse();
            ready.Player = GetSession().GameState.CurrentPlayerGuid;
            ready.IsReady = raid.IsReady;
            ready.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000);
            SendPacket(ready);
        }

        [PacketHandler(Opcode.CMSG_UPDATE_RAID_TARGET)]
        void HandleUpdateRaidTarget(UpdateRaidTarget update)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_TARGET_UPDATE);
            packet.WriteInt8(update.Symbol);
            packet.WriteGuid(update.Target.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SUMMON_RESPONSE)]
        void HandleSummonResponse(SummonResponse update)
        {
            if (update.Accept || LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_SUMMON_RESPONSE);
                packet.WriteGuid(update.SummonerGUID.To64());
                packet.WriteBool(update.Accept);
                SendPacketToServer(packet);
            }
        }
    }
}
