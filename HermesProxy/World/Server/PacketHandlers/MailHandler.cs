using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.SendMail;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_QUERY_NEXT_MAIL_TIME)]
        void HandleMailGetList(EmptyClientPacket mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_NEXT_MAIL_TIME);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_GET_LIST)]
        void HandleMailGetList(MailGetList mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_GET_LIST);
            packet.WriteGuid(mail.Mailbox.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_CREATE_TEXT_ITEM)]
        void HandleMailCreateTextItem(MailCreateTextItem mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_CREATE_TEXT_ITEM);
            packet.WriteGuid(mail.Mailbox.To64());
            packet.WriteUInt32(mail.MailID);
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt32(0); // Mail Template Id
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_DELETE)]
        void HandleMailDelete(MailDelete mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_DELETE);
            packet.WriteGuid(GetSession().GameState.CurrentInteractedWithGO.To64());
            packet.WriteUInt32(mail.MailID);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteUInt32(0); // Mail Template Id
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_MARK_AS_READ)]
        void HandleMailMarkAsRead(MailMarkAsRead mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_MARK_AS_READ);
            packet.WriteGuid(mail.Mailbox.To64());
            packet.WriteUInt32(mail.MailID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_RETURN_TO_SENDER)]
        void HandleMailReturnToSender(MailReturnToSender mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_RETURN_TO_SENDER);
            packet.WriteGuid(GetSession().GameState.CurrentInteractedWithGO.To64());
            packet.WriteUInt32(mail.MailID);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteGuid(mail.SenderGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_TAKE_ITEM)]
        void HandleMailTakeItem(MailTakeItem mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_TAKE_ITEM);
            packet.WriteGuid(mail.Mailbox.To64());
            packet.WriteUInt32(mail.MailID);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteUInt32(mail.AttachID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_MAIL_TAKE_MONEY)]
        void HandleMailTakeMoney(MailTakeMoney mail)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MAIL_TAKE_MONEY);
            packet.WriteGuid(mail.Mailbox.To64());
            packet.WriteUInt32(mail.MailID);
            SendPacketToServer(packet);
        }

        void BuildSendMail(SendMail mail, List<MailAttachment> attachments)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SEND_MAIL);
            packet.WriteGuid(mail.Mailbox.To64());
            packet.WriteCString(mail.Target);
            packet.WriteCString(mail.Subject);
            packet.WriteCString(mail.Body);
            packet.WriteInt32(mail.StationeryID);
            packet.WriteUInt32(0); // unk

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt8((byte)attachments.Count);
                foreach (var item in attachments)
                {
                    packet.WriteUInt8(item.AttachPosition);
                    packet.WriteGuid(item.ItemGUID.To64());
                }
            }
            else
            {
                if (attachments.Count > 0)
                    packet.WriteGuid(attachments[0].ItemGUID.To64());
                else
                    packet.WriteGuid(WowGuid64.Empty);
            }

            packet.WriteUInt32((uint)mail.SendMoney);
            packet.WriteUInt32((uint)mail.Cod);
            packet.WriteUInt64(0); // unk
            packet.WriteUInt8(0); // unk
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SEND_MAIL)]
        void HandleSendMail(SendMail mail)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ||
                mail.Attachments.Count <= 1)
                BuildSendMail(mail, mail.Attachments);
            else
            {
                // only 1 item can be attached in vanilla
                // split them into multiple mails
                mail.SendMoney /= mail.Attachments.Count;
                mail.Cod /= mail.Attachments.Count;
                foreach (var item in mail.Attachments)
                {
                    List<MailAttachment> attachments = new List<MailAttachment>();
                    attachments.Add(item);
                    BuildSendMail(mail, attachments);
                    System.Threading.Thread.Sleep(500); // prevent triggering antiflood on server
                }
            }
        }
    }
}
