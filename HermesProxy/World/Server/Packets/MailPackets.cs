/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class NotifyReceivedMail : ServerPacket
    {
        public NotifyReceivedMail() : base(Opcode.SMSG_NOTIFY_RECEIVED_MAIL) { }

        public override void Write()
        {
            _worldPacket.WriteFloat(Delay);
        }

        public float Delay;
    }

    public class MailQueryNextTimeResult : ServerPacket
    {
        public MailQueryNextTimeResult() : base(Opcode.SMSG_MAIL_QUERY_NEXT_TIME_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteFloat(NextMailTime);
            _worldPacket.WriteInt32(Mails.Count);

            foreach (var entry in Mails)
            {
                _worldPacket.WritePackedGuid128(entry.SenderGuid);
                _worldPacket.WriteFloat(entry.TimeLeft);
                _worldPacket.WriteInt32(entry.AltSenderID);
                _worldPacket.WriteInt8(entry.AltSenderType);
                _worldPacket.WriteInt32(entry.StationeryID);
            }
        }

        public float NextMailTime;
        public List<MailNextTimeEntry> Mails = new List<MailNextTimeEntry>();

        public class MailNextTimeEntry
        {
            public WowGuid128 SenderGuid;
            public float TimeLeft;
            public int AltSenderID;
            public sbyte AltSenderType;
            public int StationeryID;
        }
    }

    public class MailGetList : ClientPacket
    {
        public MailGetList(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mailbox = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Mailbox;
    }

    public class MailListResult : ServerPacket
    {
        public MailListResult() : base(Opcode.SMSG_MAIL_LIST_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Mails.Count);
            _worldPacket.WriteInt32(TotalNumRecords);

            Mails.ForEach(p => p.Write(_worldPacket));
        }

        public int TotalNumRecords;
        public List<MailListEntry> Mails = new();
    }

    public class MailListEntry
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(MailID);
            data.WriteUInt8((byte)SenderType);
            data.WriteUInt64(Cod);
            data.WriteInt32(StationeryID);
            data.WriteUInt64(SentMoney);
            data.WriteUInt32(Flags);
            data.WriteFloat(DaysLeft);
            data.WriteInt32(MailTemplateID);
            data.WriteInt32(Attachments.Count);

            data.WriteBit(SenderCharacter != null);
            data.WriteBit(AltSenderID.HasValue);
            data.WriteBits(Subject.GetByteCount(), 8);
            data.WriteBits(Body.GetByteCount(), 13);
            data.FlushBits();

            Attachments.ForEach(p => p.Write(data));

            if (SenderCharacter != null)
                data.WritePackedGuid128(SenderCharacter);

            if (AltSenderID.HasValue)
                data.WriteUInt32(AltSenderID.Value);

            data.WriteString(Subject);
            data.WriteString(Body);
        }

        public int MailID;
        public MailType SenderType;
        public WowGuid128 SenderCharacter;
        public uint? AltSenderID;
        public ulong Cod;
        public int StationeryID;
        public ulong SentMoney;
        public uint Flags;
        public float DaysLeft;
        public int MailTemplateID;
        public string Subject = "";
        public string Body = "";
        public uint ItemTextId; // not sent for new clients, save it here so we can fetch text prior to 3.3
        public List<MailAttachedItem> Attachments = new();
    }

    public class MailAttachedItem
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Position);
            data.WriteInt32(AttachID);
            data.WriteUInt32(Count);
            data.WriteInt32(Charges);
            data.WriteUInt32(MaxDurability);
            data.WriteUInt32(Durability);
            Item.Write(data);
            data.WriteBits(Enchants.Count, 4);
            data.WriteBits(Gems.Count, 2);
            data.WriteBit(Unlocked);
            data.FlushBits();

            foreach (ItemGemData gem in Gems)
                gem.Write(data);

            foreach (ItemEnchantData en in Enchants)
                en.Write(data);
        }

        public byte Position;
        public int AttachID;
        public ItemInstance Item = new();
        public uint Count;
        public int Charges;
        public uint MaxDurability;
        public uint Durability;
        public bool Unlocked;
        public List<ItemEnchantData> Enchants = new();
        public List<ItemGemData> Gems = new();
    }

    public class MailCreateTextItem : ClientPacket
    {
        public MailCreateTextItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mailbox = _worldPacket.ReadPackedGuid128();
            MailID = _worldPacket.ReadUInt32();
        }

        public WowGuid128 Mailbox;
        public uint MailID;
    }

    public class MailDelete : ClientPacket
    {
        public MailDelete(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            MailID = _worldPacket.ReadUInt32();
            DeleteReason = _worldPacket.ReadInt32();
        }

        public uint MailID;
        public int DeleteReason;
    }

    public class MailMarkAsRead : ClientPacket
    {
        public MailMarkAsRead(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mailbox = _worldPacket.ReadPackedGuid128();
            MailID = _worldPacket.ReadUInt32();
        }

        public WowGuid128 Mailbox;
        public uint MailID;
    }

    public class MailReturnToSender : ClientPacket
    {
        public MailReturnToSender(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            MailID = _worldPacket.ReadUInt32();
            SenderGUID = _worldPacket.ReadPackedGuid128();
        }

        public uint MailID;
        public WowGuid128 SenderGUID;
    }

    public class MailTakeItem : ClientPacket
    {
        public MailTakeItem(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mailbox = _worldPacket.ReadPackedGuid128();
            MailID = _worldPacket.ReadUInt32();
            AttachID = _worldPacket.ReadUInt32();
        }

        public WowGuid128 Mailbox;
        public uint MailID;
        public uint AttachID;
    }

    public class MailTakeMoney : ClientPacket
    {
        public MailTakeMoney(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mailbox = _worldPacket.ReadPackedGuid128();
            MailID = _worldPacket.ReadUInt32();
            Money = _worldPacket.ReadInt64();
        }

        public WowGuid128 Mailbox;
        public uint MailID;
        public long Money;
    }

    public class SendMail : ClientPacket
    {
        public SendMail(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Mailbox = _worldPacket.ReadPackedGuid128();
            StationeryID = _worldPacket.ReadInt32();
            SendMoney = _worldPacket.ReadInt64();
            Cod = _worldPacket.ReadInt64();

            uint targetLength = _worldPacket.ReadBits<uint>(9);
            uint subjectLength = _worldPacket.ReadBits<uint>(9);
            uint bodyLength = _worldPacket.ReadBits<uint>(11);

            uint count = _worldPacket.ReadBits<uint>(5);

            Target = _worldPacket.ReadString(targetLength);
            Subject = _worldPacket.ReadString(subjectLength);
            Body = _worldPacket.ReadString(bodyLength);

            for (var i = 0; i < count; ++i)
            {
                var att = new MailAttachment()
                {
                    AttachPosition = _worldPacket.ReadUInt8(),
                    ItemGUID = _worldPacket.ReadPackedGuid128()
                };

                Attachments.Add(att);
            }
        }

        public WowGuid128 Mailbox;
        public int StationeryID;
        public long SendMoney;
        public long Cod;
        public string Target;
        public string Subject;
        public string Body;
        public List<MailAttachment> Attachments = new();

        public struct MailAttachment
        {
            public byte AttachPosition;
            public WowGuid128 ItemGUID;
        }
    }

    public class MailCommandResult : ServerPacket
    {
        public MailCommandResult() : base(Opcode.SMSG_MAIL_COMMAND_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MailID);
            _worldPacket.WriteUInt32((uint)Command);
            _worldPacket.WriteUInt32((uint)ErrorCode);
            _worldPacket.WriteUInt32((uint)BagResult);
            _worldPacket.WriteUInt32(AttachID);
            _worldPacket.WriteUInt32(QtyInInventory);
        }

        public uint MailID;
        public MailActionType Command;
        public MailErrorType ErrorCode;
        public InventoryResult BagResult;
        public uint AttachID;
        public uint QtyInInventory;
    }
}
