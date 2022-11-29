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


using System;
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class ContactListRequest : ClientPacket
    {
        public ContactListRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Flags = (SocialFlag)_worldPacket.ReadUInt32();
        }

        public SocialFlag Flags;
    }

    public class ContactList : ServerPacket
    {
        public ContactList() : base(Opcode.SMSG_CONTACT_LIST)
        {
            Contacts = new List<ContactInfo>();
        }

        public override void Write()
        {
            _worldPacket.WriteUInt32((uint)Flags);
            _worldPacket.WriteBits(Contacts.Count, 8);
            _worldPacket.FlushBits();

            foreach (ContactInfo contact in Contacts)
                contact.Write(_worldPacket);
        }

        public List<ContactInfo> Contacts;
        public SocialFlag Flags;
    }

    public class ContactInfo
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(Guid);
            data.WritePackedGuid128(WowAccountGuid);
            data.WriteUInt32(VirtualRealmAddr);
            data.WriteUInt32(NativeRealmAddr);
            data.WriteUInt32((uint)TypeFlags);
            data.WriteUInt8((byte)Status);
            data.WriteUInt32(AreaID);
            data.WriteUInt32(Level);
            data.WriteUInt32((uint)ClassID);
            data.WriteBits(Note.GetByteCount(), 10);
            data.WriteBit(Mobile);
            data.FlushBits();
            data.WriteString(Note);
        }

        public WowGuid128 Guid;
        public WowGuid128 WowAccountGuid;
        public uint VirtualRealmAddr;
        public uint NativeRealmAddr;
        public SocialFlag TypeFlags;
        public FriendStatus Status;
        public uint AreaID;
        public uint Level;
        public Class ClassID;
        public bool Mobile;
        public string Note = "";
    }

    public class FriendStatusPkt : ServerPacket
    {
        public FriendStatusPkt() : base(Opcode.SMSG_FRIEND_STATUS) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)FriendResult);
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WritePackedGuid128(WowAccountGuid);
            _worldPacket.WriteUInt32(VirtualRealmAddress);
            _worldPacket.WriteUInt8((byte)Status);
            _worldPacket.WriteUInt32(AreaID);
            _worldPacket.WriteUInt32(Level);
            _worldPacket.WriteUInt32((uint)ClassID);
            _worldPacket.WriteBits(Notes.GetByteCount(), 10);
            _worldPacket.WriteBit(Mobile);
            _worldPacket.FlushBits();
            _worldPacket.WriteString(Notes);
        }

        public FriendsResult FriendResult;
        public WowGuid128 Guid;
        public WowGuid128 WowAccountGuid;
        public uint VirtualRealmAddress;
        public FriendStatus Status;
        public uint AreaID;
        public uint Level;
        public Class ClassID = Class.None;
        public string Notes;
        public bool Mobile;
    }

    public class AddFriend : ClientPacket
    {
        public AddFriend(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLength = _worldPacket.ReadBits<uint>(9);
            uint noteslength = _worldPacket.ReadBits<uint>(10);
            Name = _worldPacket.ReadString(nameLength);
            Note = _worldPacket.ReadString(noteslength);
        }

        public string Note;
        public string Name;
    }

    public class AddIgnore : ClientPacket
    {
        public AddIgnore(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint nameLength = _worldPacket.ReadBits<uint>(9);
            if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
                AccountGuid = _worldPacket.ReadPackedGuid128();
            Name = _worldPacket.ReadString(nameLength);
        }

        WowGuid128 AccountGuid;
        public string Name;
    }

    public class DelFriend : ClientPacket
    {
        public DelFriend(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VirtualRealmAddress = _worldPacket.ReadUInt32();
            Guid = _worldPacket.ReadPackedGuid128();
        }

        public uint VirtualRealmAddress;
        public WowGuid128 Guid;
    }

    public class SetContactNotes : ClientPacket
    {
        public SetContactNotes(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            VirtualRealmAddress = _worldPacket.ReadUInt32();
            Guid = _worldPacket.ReadPackedGuid128();
            Notes = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(10));
        }

        public uint VirtualRealmAddress;
        public WowGuid128 Guid;
        public string Notes;
    }
}
