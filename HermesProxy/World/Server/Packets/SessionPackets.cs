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

using Framework.Constants;
using Framework.IO;
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class BattlenetNotification : ServerPacket
    {
        public BattlenetNotification() : base(Opcode.SMSG_BATTLENET_NOTIFICATION) { }

        public override void Write()
        {
            Method.Write(_worldPacket);

            _worldPacket.WriteUInt32(Data.GetSize());
            _worldPacket.WriteBytes(Data);
        }

        public MethodCall Method;
        public ByteBuffer Data = new();
    }

    class BattlenetResponse : ServerPacket
    {
        public BattlenetResponse() : base(Opcode.SMSG_BATTLENET_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WriteUint32Enum(Status);

            Method.Write(_worldPacket);

            _worldPacket.WriteUInt32(Data.GetSize());
            _worldPacket.WriteBytes(Data);
        }

        public BattlenetRpcErrorCode Status = BattlenetRpcErrorCode.Ok;
        public MethodCall Method;
        public ByteBuffer Data = new();
    }

    class ConnectionStatus : ServerPacket
    {
        public ConnectionStatus() : base(Opcode.SMSG_BATTLE_NET_CONNECTION_STATUS) { }

        public override void Write()
        {
            _worldPacket.WriteBits(State, 2);
            _worldPacket.WriteBit(SuppressNotification);
            _worldPacket.FlushBits();
        }

        public byte State;
        public bool SuppressNotification;
    }

    class ChangeRealmTicketResponse : ServerPacket
    {
        public ChangeRealmTicketResponse() : base(Opcode.SMSG_CHANGE_REALM_TICKET_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Token);
            _worldPacket.WriteBit(Allow);
            _worldPacket.WriteUInt32(Ticket.GetSize());
            _worldPacket.WriteBytes(Ticket);
        }

        public uint Token;
        public bool Allow = true;
        public ByteBuffer Ticket;
    }

    class BattlenetRequest : ClientPacket
    {
        public BattlenetRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Method.Read(_worldPacket);

            uint protoSize = _worldPacket.ReadUInt32();
            Data = _worldPacket.ReadBytes(protoSize);
        }

        public MethodCall Method;
        public byte[] Data;
    }

    class ChangeRealmTicket : ClientPacket
    {
        public ChangeRealmTicket(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Token = _worldPacket.ReadUInt32();
            for (var i = 0; i < Secret.GetLimit(); ++i)
                Secret[i] = _worldPacket.ReadUInt8();
        }

        public uint Token;
        public Array<byte> Secret = new(32);
    }

    public struct MethodCall
    {
        public uint GetServiceHash() { return (uint)(Type >> 32); }
        public uint GetMethodId() { return (uint)(Type & 0xFFFFFFFF); }

        public void SetServiceHash(uint serviceHash)
        {
            Type = (Type & 0x00000000_FFFFFFFF) | (((ulong) serviceHash) << 32);
        }

        public void SetMethodId(uint methodId)
        {
            Type = (Type & 0xFFFFFFFF_00000000) | methodId;
        }

        public void Read(ByteBuffer data)
        {
            Type = data.ReadUInt64();
            ObjectId = data.ReadUInt64();
            Token = data.ReadUInt32();
        }

        public void Write(ByteBuffer data)
        {
            data.WriteUInt64(Type);
            data.WriteUInt64(ObjectId);
            data.WriteUInt32(Token);
        }

        public ulong Type;
        public ulong ObjectId;
        public uint Token;
    }
}
