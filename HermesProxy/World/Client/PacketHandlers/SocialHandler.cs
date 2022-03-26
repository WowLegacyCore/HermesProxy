using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_FRIEND_LIST)]
        void HandleFriendList(WorldPacket packet)
        {
            ContactList contacts = new ContactList();
            contacts.Flags = SocialFlag.Friend;
            var count = packet.ReadUInt8();

            for (var i = 0; i < count; i++)
            {
                ContactInfo contact = new ContactInfo();
                contact.TypeFlags = SocialFlag.Friend;
                contact.Guid = packet.ReadGuid().To128();
                contact.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(contact.Guid);
                contact.NativeRealmAddr = GetSession().RealmId.GetAddress();
                contact.VirtualRealmAddr = GetSession().RealmId.GetAddress();
                contact.Status = (FriendStatus)packet.ReadUInt8();
                if (contact.Status != FriendStatus.Offline)
                {
                    contact.AreaID = packet.ReadUInt32();
                    contact.Level = packet.ReadUInt32();
                    contact.ClassID = (Class)packet.ReadUInt32();
                }
                contacts.Contacts.Add(contact);
            }

            SendPacketToClient(contacts);
        }

        [PacketHandler(Opcode.SMSG_IGNORE_LIST)]
        void HandleIgnoreList(WorldPacket packet)
        {
            ContactList contacts = new ContactList();
            contacts.Flags = SocialFlag.Ignored;
            var count = packet.ReadUInt8();

            for (var i = 0; i < count; i++)
            {
                ContactInfo contact = new ContactInfo();
                contact.TypeFlags = SocialFlag.Ignored;
                contact.Guid = packet.ReadGuid().To128();
                contact.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(contact.Guid);
                contact.NativeRealmAddr = GetSession().RealmId.GetAddress();
                contact.VirtualRealmAddr = GetSession().RealmId.GetAddress();
                contacts.Contacts.Add(contact);
            }

            SendPacketToClient(contacts);
        }

        [PacketHandler(Opcode.SMSG_CONTACT_LIST)]
        void HandleContactList(WorldPacket packet)
        {
            ContactList contacts = new ContactList();
            contacts.Flags = (SocialFlag)packet.ReadUInt32();
            var count = packet.ReadUInt32();

            for (var i = 0; i < count; i++)
            {
                ContactInfo contact = new ContactInfo();
                contact.Guid = packet.ReadGuid().To128();
                contact.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(contact.Guid);
                contact.NativeRealmAddr = GetSession().RealmId.GetAddress();
                contact.VirtualRealmAddr = GetSession().RealmId.GetAddress();
                contact.TypeFlags = (SocialFlag)packet.ReadUInt32();
                contact.Note = packet.ReadCString();
                if (contact.TypeFlags.HasAnyFlag(SocialFlag.Friend))
                {
                    contact.Status = (FriendStatus)packet.ReadUInt8();
                    if (contact.Status != FriendStatus.Offline)
                    {
                        contact.AreaID = packet.ReadUInt32();
                        contact.Level = packet.ReadUInt32();
                        contact.ClassID = (Class)packet.ReadUInt32();
                    }
                }
                contacts.Contacts.Add(contact);
            }

            SendPacketToClient(contacts);
        }

        [PacketHandler(Opcode.SMSG_FRIEND_STATUS)]
        void HandleFriendStatus(WorldPacket packet)
        {
            FriendStatusPkt friend = new FriendStatusPkt();
            friend.FriendResult = (FriendsResult)packet.ReadUInt8();
            friend.Guid = packet.ReadGuid().To128();
            friend.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(friend.Guid);
            friend.VirtualRealmAddress = GetSession().RealmId.GetAddress();
            switch (friend.FriendResult)
            {
                case FriendsResult.AddedOffline:
                {
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        friend.Notes = packet.ReadCString();
                    break;
                }
                case FriendsResult.AddedOnline:
                {
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        friend.Notes = packet.ReadCString();
                    friend.Status = (FriendStatus)packet.ReadUInt8();
                    friend.AreaID = packet.ReadUInt32();
                    friend.Level = packet.ReadUInt32();
                    friend.ClassID = (Class)packet.ReadUInt32();
                    break;
                }
                case FriendsResult.Online:
                {
                    friend.Status = (FriendStatus)packet.ReadUInt8();
                    friend.AreaID = packet.ReadUInt32();
                    friend.Level = packet.ReadUInt32();
                    friend.ClassID = (Class)packet.ReadUInt32();
                    break;
                }
            }
            SendPacketToClient(friend);
        }
    }
}
