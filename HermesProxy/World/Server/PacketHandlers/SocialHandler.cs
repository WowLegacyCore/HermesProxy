using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_CONTACT_LIST)]
        void HandleContactList(ContactListRequest contacts)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_FRIEND_LIST);
                SendPacketToServer(packet);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_CONTACT_LIST);
                packet.WriteUInt32((uint)contacts.Flags);
                SendPacketToServer(packet);
            }
        }

        [PacketHandler(Opcode.CMSG_ADD_FRIEND)]
        void HandleAddFriend(AddFriend friend)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ADD_FRIEND);
            packet.WriteCString(friend.Name);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteCString(friend.Note);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ADD_IGNORE)]
        void HandleAddIgnore(AddIgnore ignore)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ADD_IGNORE);
            packet.WriteCString(ignore.Name);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_DEL_FRIEND)]
        [PacketHandler(Opcode.CMSG_DEL_IGNORE)]
        void HandleDelFriend(DelFriend friend)
        {
            WorldPacket packet = new WorldPacket(friend.GetUniversalOpcode());
            packet.WriteGuid(friend.Guid.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_CONTACT_NOTES)]
        void HandleSetContactNotes(SetContactNotes friend)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_CONTACT_NOTES);
            packet.WriteGuid(friend.Guid.To64());
            packet.WriteCString(friend.Notes);
            SendPacketToServer(packet);
        }
    }
}
