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

        [PacketHandler(Opcode.CMSG_GUILD_BANK_ACTIVATE)]
        void HandleGuildBankActivate(GuildBankAtivate activate)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_ACTIVATE);
            packet.WriteGuid(activate.BankGuid.To64());
            packet.WriteBool(activate.FullUpdate);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_QUERY_TAB)]
        void HandleGuildBankQueryTab(GuildBankQueryTab query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_QUERY_TAB);
            packet.WriteGuid(query.BankGuid.To64());
            packet.WriteUInt8(query.Tab);
            packet.WriteBool(query.FullUpdate);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY)]
        void HandleGuildBankDepositMoney(GuildBankDepositMoney deposit)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY);
            packet.WriteGuid(deposit.BankGuid.To64());
            packet.WriteUInt32((uint)deposit.Money);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_TEXT_QUERY)]
        void HandleGuildBankTextQuery(GuildBankTextQuery query)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_GUILD_BANK_TEXT);
            packet.WriteUInt8((byte)query.Tab);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_UPDATE_TAB)]
        void HandleGuildBankUpdateTab(GuildBankUpdateTab update)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_UPDATE_TAB);
            packet.WriteGuid(update.BankGuid.To64());
            packet.WriteUInt8(update.BankTab);
            packet.WriteCString(update.Name);
            packet.WriteCString(update.Icon);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_LOG_QUERY)]
        void HandleGuildBankLogQuery(GuildBankLogQuery query)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_LOG_QUERY);
            packet.WriteUInt8((byte)query.Tab);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT)]
        void HandleGuildBankSetTabText(GuildBankSetTabText query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT);
            packet.WriteUInt8((byte)query.Tab);
            packet.WriteCString(query.TabText);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_BUY_TAB)]
        void HandleGuildBankBuyTab(GuildBankBuyTab buy)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_BUY_TAB);
            packet.WriteGuid(buy.BankGuid.To64());
            packet.WriteUInt8(buy.BankTab);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY)]
        void HandleGuildBankBuyTab(GuildBankWithdrawMoney withdraw)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY);
            packet.WriteGuid(withdraw.BankGuid.To64());
            packet.WriteUInt32((uint)withdraw.Money);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUTO_GUILD_BANK_ITEM)]
        void HandleGuildBankItem(AutoGuildBankItem item)
        {
            // moves an item from the player to the bank
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(false); // bank to bank
            packet.WriteUInt8(item.BankTab);
            packet.WriteUInt8(item.BankSlot);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(false); // auto store
            if (item.ContainerSlot != null)
            {
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot((byte)item.ContainerSlot));
                packet.WriteUInt8(item.ContainerItemSlot);
            }
            else
            {
                packet.WriteUInt8(Enums.Classic.InventorySlots.Bag0);
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
            }
            packet.WriteBool(false); // to char
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0); // splitted amount
            else
                packet.WriteUInt8(0); // splitted amount
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SPLIT_ITEM_TO_GUILD_BANK)]
        [PacketHandler(Opcode.CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM)]
        void HandleSplitItemToGuildBank(SplitItemToGuildBank item)
        {
            // moves a specific amount of stacks from the player to the bank
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(false); // bank to bank
            packet.WriteUInt8(item.BankTab);
            packet.WriteUInt8(item.BankSlot);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(false); // auto store
            if (item.ContainerSlot != null)
            {
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot((byte)item.ContainerSlot));
                packet.WriteUInt8(item.ContainerItemSlot);
            }
            else
            {
                packet.WriteUInt8(Enums.Classic.InventorySlots.Bag0);
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
            }
            packet.WriteBool(false); // to char
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(item.StackCount);
            else
                packet.WriteUInt8((byte)item.StackCount);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_AUTO_STORE_GUILD_BANK_ITEM)]
        void HandleAutoStoreGuildBankItem(AutoStoreGuildBankItem item)
        {
            // moves an item from the bank to the player
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(false); // bank to bank
            packet.WriteUInt8(item.BankTab);
            packet.WriteUInt8(item.BankSlot);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(true); // auto store
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0); // auto store count
            else
                packet.WriteUInt8(0); // auto store count
            packet.WriteBool(true); // to char
            packet.WriteUInt8(0); // unknown
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_STORE_GUILD_BANK_ITEM)]
        void HandleStoreGuildBankItem(AutoGuildBankItem item)
        {
            // moves an item from the bank to a specific slot in the player inventory
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(false); // bank to bank
            packet.WriteUInt8(item.BankTab);
            packet.WriteUInt8(item.BankSlot);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(false); // auto store
            if (item.ContainerSlot != null)
            {
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot((byte)item.ContainerSlot));
                packet.WriteUInt8(item.ContainerItemSlot);
            }
            else
            {
                packet.WriteUInt8(Enums.Classic.InventorySlots.Bag0);
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
            }
            packet.WriteBool(true); // to char
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0); // splitted amount
            else
                packet.WriteUInt8(0); // splitted amount
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM)]
        [PacketHandler(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY)]
        void HandleMergeGuildBankItemWithItem(SplitItemToGuildBank item)
        {
            // moves a specific amount of stacks from the bank to the player
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(false); // bank to bank
            packet.WriteUInt8(item.BankTab);
            packet.WriteUInt8(item.BankSlot);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(false); // auto store
            if (item.ContainerSlot != null)
            {
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot((byte)item.ContainerSlot));
                packet.WriteUInt8(item.ContainerItemSlot);
            }
            else
            {
                packet.WriteUInt8(Enums.Classic.InventorySlots.Bag0);
                packet.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
            }
            packet.WriteBool(true); // to char
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(item.StackCount);
            else
                packet.WriteUInt8((byte)item.StackCount);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MOVE_GUILD_BANK_ITEM)]
        void HandleMoveGuildBankItem(MoveGuildBankItem item)
        {
            // moves an item from the bank to the bank
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(true); // bank to bank
            packet.WriteUInt8(item.BankTab2);
            packet.WriteUInt8(item.BankSlot2);
            packet.WriteUInt32(0); // item id
            packet.WriteUInt8(item.BankTab1);
            packet.WriteUInt8(item.BankSlot1);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(false); // auto store
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0); // splitted amount
            else
                packet.WriteUInt8(0); // splitted amount
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM)]
        [PacketHandler(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM)]
        void HandleMoveGuildBankItem(SplitGuildBankItem item)
        {
            // moves a specific amount of stacks from the bank to the bank
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
            packet.WriteGuid(item.BankGuid.To64());
            packet.WriteBool(true); // bank to bank
            packet.WriteUInt8(item.BankTab2);
            packet.WriteUInt8(item.BankSlot2);
            packet.WriteUInt32(0); // item id
            packet.WriteUInt8(item.BankTab1);
            packet.WriteUInt8(item.BankSlot1);
            packet.WriteUInt32(0); // item id
            packet.WriteBool(false); // auto store
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(item.StackCount);
            else
                packet.WriteUInt8((byte)item.StackCount);
            SendPacketToServer(packet);
        }
    }
}
