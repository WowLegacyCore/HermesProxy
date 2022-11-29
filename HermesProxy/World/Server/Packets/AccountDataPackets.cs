using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets
{
    public class AccountCharacterListEntry
    {
        public void Write(WorldPacket packet)
        {
            packet.WritePackedGuid128(AccountId);
            packet.WritePackedGuid128(CharacterGuid);
            packet.WriteUInt32(RealmVirtualAddress);
            packet.WriteByteEnum(Race);
            packet.WriteByteEnum(Class);
            packet.WriteByteEnum(Sex);
            packet.WriteUInt8(Level);

            packet.WriteUInt64(LastLoginUnixSec);

            packet.ResetBitPos();

            packet.WriteBits(Name.GetByteCount(), 6);
            packet.WriteBits(RealmName.GetByteCount(), 9);

            packet.WriteString(Name);
            packet.WriteString(RealmName);
        }

        public WowGuid128 AccountId;

        public uint RealmVirtualAddress;
        public string RealmName;

        public WowGuid128 CharacterGuid;
        public string Name;
        public Race Race;
        public Class Class;
        public Gender Sex;
        public byte Level;
        public ulong LastLoginUnixSec;
    }

    public class GetAccountCharacterListRequest : ClientPacket
    {
        public GetAccountCharacterListRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Token = _worldPacket.ReadUInt32();
        }

        public uint Token = 0;
    }

    public class GetAccountCharacterListResult : ServerPacket
    {
        public GetAccountCharacterListResult() : base(Opcode.SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT)
        {
        }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Token);
            _worldPacket.WriteUInt32((uint) CharacterList.Count);
            
            _worldPacket.ResetBitPos();

            _worldPacket.WriteBit(false); // unknown bit

            foreach (var entry in CharacterList)
                entry.Write(_worldPacket);

        }

        public uint Token = 0;
        public List<AccountCharacterListEntry> CharacterList = new();
    }
}
