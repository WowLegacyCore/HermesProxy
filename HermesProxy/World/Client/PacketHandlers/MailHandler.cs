using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.MailQueryNextTimeResult;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_NOTIFY_RECEIVED_MAIL)]
        void HandleNotifyReceivedMail(WorldPacket packet)
        {
            NotifyReceivedMail mail = new NotifyReceivedMail();
            mail.Delay = packet.ReadFloat();
            SendPacketToClient(mail);
        }

        [PacketHandler(Opcode.MSG_QUERY_NEXT_MAIL_TIME)]
        void HandleQueryNextMailTime(WorldPacket packet)
        {
            MailQueryNextTimeResult result = new MailQueryNextTimeResult();
            result.NextMailTime = packet.ReadFloat();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_3_0_7561))
            {
                if (result.NextMailTime == 0)
                {
                    MailNextTimeEntry mail = new MailNextTimeEntry();
                    mail.SenderGuid = GetSession().GameState.CurrentPlayerGuid;
                    mail.AltSenderID = 0;
                    mail.AltSenderType = 0;
                    mail.StationeryID = 41;
                    mail.TimeLeft = 3600;
                    result.Mails.Add(mail);
                }
            }
            else
            {
                var count = packet.ReadUInt32();
                for (var i = 0; i < count; ++i)
                {
                    MailNextTimeEntry mail = new MailNextTimeEntry();
                    mail.SenderGuid = packet.ReadGuid().To128();
                    mail.AltSenderID = packet.ReadInt32();
                    mail.AltSenderType = (sbyte)packet.ReadInt32();
                    mail.StationeryID = packet.ReadInt32();
                    mail.TimeLeft = packet.ReadFloat();
                    result.Mails.Add(mail);
                }
            }
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.SMSG_MAIL_LIST_RESULT)]
        void HandleMailListResult(WorldPacket packet)
        {
            MailListResult result = new MailListResult();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                result.TotalNumRecords = packet.ReadInt32();

            var count = packet.ReadUInt8();

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_2_0_10192))
                result.TotalNumRecords = count;

            for (var i = 0; i < count; ++i)
            {
                MailListEntry mail = new MailListEntry();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    packet.ReadUInt16(); // Message Size

                mail.MailID = packet.ReadInt32();
                mail.SenderType = (MailType)packet.ReadUInt8();
                switch (mail.SenderType) // Read GUID if MailType.Normal, int32 (entry) if not
                {
                    case MailType.Normal:
                        mail.SenderCharacter = packet.ReadGuid().To128();
                        break;
                    case MailType.Item:
                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                            mail.AltSenderID = packet.ReadUInt32();
                        break;
                    default:
                        mail.AltSenderID = packet.ReadUInt32();
                        break;
                }

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    mail.Cod = packet.ReadUInt32();
                else
                    mail.Subject = packet.ReadCString();

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_3_0_10958))
                {
                    mail.ItemTextId = packet.ReadUInt32();
                    if (mail.ItemTextId != 0 && !GetSession().GameState.ItemTexts.ContainsKey(mail.ItemTextId))
                    {
                        GetSession().GameState.RequestedItemTextIds.Add(mail.ItemTextId);
                        WorldPacket query = new WorldPacket(Opcode.CMSG_ITEM_TEXT_QUERY);
                        query.WriteUInt32(mail.ItemTextId);
                        query.WriteInt32(mail.MailID);
                        query.WriteUInt32(0); // unk
                        SendPacket(query);
                    }
                }

                packet.ReadUInt32(); // Package.dbc ID
                mail.StationeryID = packet.ReadInt32(); // Stationary.dbc ID

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    MailAttachedItem mailItem = ReadMailItem(packet);
                    if (mailItem.Item.ItemID != 0)
                    {
                        mailItem.AttachID = 1;
                        mail.Attachments.Add(mailItem);
                    }   
                }

                mail.SentMoney = packet.ReadUInt32();
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    mail.Cod = packet.ReadUInt32();

                mail.Flags = packet.ReadUInt32();
                mail.DaysLeft = packet.ReadFloat();
                mail.MailTemplateID = packet.ReadInt32(); // MailTemplate.dbc ID

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    mail.Subject = packet.ReadCString();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                    mail.Body = packet.ReadCString();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    byte itemsCount = packet.ReadUInt8();
                    for (var j = 0; j < itemsCount; ++j)
                    {
                        MailAttachedItem mailItem = ReadMailItem(packet);
                        mail.Attachments.Add(mailItem);
                    }
                }

                result.Mails.Add(mail);
            }

            if (GetSession().GameState.RequestedItemTextIds.Count == 0)
            {
                foreach (var mail in result.Mails)
                {
                    if (mail.ItemTextId != 0)
                        mail.Body = GetSession().GameState.ItemTexts[mail.ItemTextId];
                }
                SendPacketToClient(result);
            }
            else
                GetSession().GameState.PendingMailListPacket = result;
        }

        [PacketHandler(Opcode.SMSG_QUERY_ITEM_TEXT_RESPONSE)]
        void HandleQueryItemTextResponse(WorldPacket packet)
        {
            uint itemTextId = packet.ReadUInt32();
            string text = packet.ReadCString();

            if (GetSession().GameState.ItemTexts.ContainsKey(itemTextId))
                GetSession().GameState.ItemTexts[itemTextId] = text;
            else
                GetSession().GameState.ItemTexts.Add(itemTextId, text);

            if (GetSession().GameState.RequestedItemTextIds.Contains(itemTextId))
                GetSession().GameState.RequestedItemTextIds.Remove(itemTextId);

            if (GetSession().GameState.PendingMailListPacket != null &&
                GetSession().GameState.RequestedItemTextIds.Count == 0)
            {
                MailListResult result = GetSession().GameState.PendingMailListPacket;
                foreach (var mail in result.Mails)
                {
                    if (mail.ItemTextId != 0)
                        mail.Body = GetSession().GameState.ItemTexts[mail.ItemTextId];
                }
                SendPacketToClient(result);
            }
        }

        MailAttachedItem ReadMailItem(WorldPacket packet)
        {
            MailAttachedItem mailItem = new MailAttachedItem();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                mailItem.Position = packet.ReadUInt8();
                mailItem.AttachID = packet.ReadInt32();
            }

            mailItem.Item.ItemID = packet.ReadUInt32();

            byte enchantmentCount;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                enchantmentCount = 7;
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                enchantmentCount = 6;
            else
                enchantmentCount = 1;

            for (byte k = 0; k < enchantmentCount; ++k)
            {
                ItemEnchantData enchant = new ItemEnchantData();
                enchant.Slot = k;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                {
                    enchant.Charges = packet.ReadInt32();
                    enchant.Expiration = packet.ReadUInt32();
                }
                enchant.ID = packet.ReadUInt32();
                if (enchant.ID != 0)
                    mailItem.Enchants.Add(enchant);
            }

            mailItem.Item.RandomPropertiesID = packet.ReadUInt32();
            mailItem.Item.RandomPropertiesSeed = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                mailItem.Count = (byte)packet.ReadUInt32();
            else
                mailItem.Count = (byte)packet.ReadUInt8();

            mailItem.Charges = packet.ReadInt32();
            mailItem.MaxDurability = packet.ReadUInt32();
            mailItem.Durability = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                mailItem.Unlocked = packet.ReadBool();

            return mailItem;
        }

        [PacketHandler(Opcode.SMSG_MAIL_COMMAND_RESULT)]
        void HandleMailCommandResult(WorldPacket packet)
        {
            MailCommandResult mail = new MailCommandResult();
            mail.MailID = packet.ReadUInt32();
            mail.Command = (MailActionType)packet.ReadUInt32();
            mail.ErrorCode = (MailErrorType)packet.ReadUInt32();
            if (mail.ErrorCode == MailErrorType.Equip)
                mail.BagResult = (InventoryResult)packet.ReadUInt32();
            else if (mail.Command == MailActionType.AttachmentExpired)
            {
                mail.AttachID = packet.ReadUInt32();
                mail.QtyInInventory = packet.ReadUInt32();

                // not sent in mail list in 1.12 so have to use placeholder
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    mail.AttachID = 1;
            }
            SendPacketToClient(mail);
        }
    }
}
