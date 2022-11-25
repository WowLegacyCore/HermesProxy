using System;
using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_QUERY_GUILD_INFO)]
        void HandleQueryGuildInfo(QueryGuildInfo query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GUILD_INFO);
            packet.WriteUInt32((uint)query.GuildGuid.GetCounter());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_PERMISSIONS_QUERY)]
        void HandleGuildPermissionsQuery(GuildPermissionsQuery query)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_PERMISSIONS);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY)]
        void HandleGuildBankRemainingWithdrawnMoneyQuery(GuildBankRemainingWithdrawMoneyQuery query)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_GET_ROSTER)]
        void HandleGuildGetRoster(GuildGetRoster query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INFO);
            SendPacketToServer(packet);

            WorldPacket packet2 = new WorldPacket(Opcode.CMSG_GUILD_GET_ROSTER);
            SendPacketToServer(packet2);
        }

        [PacketHandler(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT)]
        void HandleGuildUpdateMotdText(GuildUpdateMotdText text)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT);
            packet.WriteCString(text.MotdText);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT)]
        void HandleGuildUpdateInfoText(GuildUpdateInfoText text)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT);
            packet.WriteCString(text.InfoText);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_SET_MEMBER_NOTE)]
        void HandleGuildSetMemberNote(GuildSetMemberNote note)
        {
            WorldPacket packet = new WorldPacket(note.IsPublic ? Opcode.CMSG_GUILD_SET_PUBLIC_NOTE : Opcode.CMSG_GUILD_SET_OFFICER_NOTE);
            packet.WriteCString(GetSession().GameState.GetPlayerName(note.NoteeGUID));
            packet.WriteCString(note.Note);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_PROMOTE_MEMBER)]
        void HandleGuildPromoteMember(GuildPromoteMember promote)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_PROMOTE_MEMBER);
            packet.WriteCString(GetSession().GameState.GetPlayerName(promote.Promotee));
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_DEMOTE_MEMBER)]
        void HandleGuildDemoteMember(GuildDemoteMember demote)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DEMOTE_MEMBER);
            packet.WriteCString(GetSession().GameState.GetPlayerName(demote.Demotee));
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER)]
        void HandleGuildOfficerRemoveMember(GuildOfficerRemoveMember remove)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER);
            packet.WriteCString(GetSession().GameState.GetPlayerName(remove.Removee));
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_INVITE_BY_NAME)]
        void HandleGuildInviteByName(GuildInviteByName invite)
        {
            if (invite.ArenaTeamId == 0)
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INVITE_BY_NAME);
                packet.WriteCString(invite.Name);
                SendPacketToServer(packet);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_ARENA_TEAM_INVITE);
                packet.WriteUInt32(invite.ArenaTeamId);
                packet.WriteCString(invite.Name);
                SendPacketToServer(packet);
            }
        }

        [PacketHandler(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS)]
        void HandleGuildSetRankPermissions(GuildSetRankPermissions rank)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS);
            packet.WriteUInt32(rank.RankID);
            packet.WriteUInt32(rank.Flags);
            packet.WriteCString(rank.RankName);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteInt32(rank.WithdrawGoldLimit);
                for (var i = 0; i < 6; i++)
                {
                    packet.WriteUInt32(rank.TabFlags[i]);
                    packet.WriteUInt32(rank.TabWithdrawItemLimit[i]);
                }
            }
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_ADD_RANK)]
        void HandleGuildAddRank(GuildAddRank rank)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_ADD_RANK);
            packet.WriteCString(rank.Name);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_DELETE_RANK)]
        void HandleGuildDeleteRank(GuildDeleteRank rank)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE_RANK);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_SET_GUILD_MASTER)]
        void HandleGuildSetGuildMaster(GuildSetGuildMaster master)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_SET_GUILD_MASTER);
            packet.WriteCString(master.NewMasterName);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_LEAVE)]
        void HandleGuildLeave(GuildLeave leave)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_LEAVE);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ACCEPT_GUILD_INVITE)]
        void HandleGuildAccept(AcceptGuildInvite accept)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ACCEPT_GUILD_INVITE);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_DECLINE_INVITATION)]
        void HandleGuildDecline(DeclineGuildInvite decline)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_DELETE)]
        void HandleGuildDelete(GuildDelete delete)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SAVE_GUILD_EMBLEM)]
        void HandleSaveGuildEmblem(SaveGuildEmblem emblem)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_SAVE_GUILD_EMBLEM);
            packet.WriteGuid(emblem.DesignerGUID.To64());
            packet.WriteUInt32(emblem.EmblemStyle);
            packet.WriteUInt32(emblem.EmblemColor);
            packet.WriteUInt32(emblem.BorderStyle);
            packet.WriteUInt32(emblem.BorderColor);
            packet.WriteUInt32(emblem.BackgroundColor);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_DECLINE_GUILD_INVITES)]
        void HandleDeclineGuildInvites(SetAutoDeclineGuildInvites packet)
        {
            GetSession().GameState.CurrentPlayerStorage.Settings.SetAutoBlockGuildInvites(packet.GuildInvitesShouldGetBlocked);

            // Send update to client
            ObjectUpdate updateData = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
            PlayerFlags flags = GetSession().GameState.CurrentPlayerStorage.Settings.CreateNewFlags();
            updateData.PlayerData.PlayerFlags = (uint) flags;
            UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
            updatePacket.ObjectUpdates.Add(updateData);
            GetSession().WorldClient.SendPacketToClient(updatePacket);
        }

        [PacketHandler(Opcode.CMSG_GUILD_AUTO_DECLINE_INVITATION)]
        void HandleGuildAutoDeclineInvitation(AutoDeclineGuildInvite autoDecline)
        { // This is called when the client still receives a guild invite after enabling AutoDecline
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
            SendPacketToServer(packet);
        }
    }
}
