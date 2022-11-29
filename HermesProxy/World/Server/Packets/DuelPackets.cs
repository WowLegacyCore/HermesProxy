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
using HermesProxy.World.Enums;
using System;

namespace HermesProxy.World.Server.Packets
{
    public class CanDuel : ClientPacket
    {
        public CanDuel(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TargetGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 TargetGUID;
    }

    public class CanDuelResult : ServerPacket
    {
        public CanDuelResult() : base(Opcode.SMSG_CAN_DUEL_RESULT) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(TargetGUID);
            _worldPacket.WriteBit(Result);
            _worldPacket.FlushBits();
        }

        public WowGuid128 TargetGUID;
        public bool Result;
    }

    public class DuelRequested : ServerPacket
    {
        public DuelRequested() : base(Opcode.SMSG_DUEL_REQUESTED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ArbiterGUID);
            _worldPacket.WritePackedGuid128(RequestedByGUID);
            _worldPacket.WritePackedGuid128(RequestedByWowAccount);
        }

        public WowGuid128 ArbiterGUID;
        public WowGuid128 RequestedByGUID;
        public WowGuid128 RequestedByWowAccount;
    }

    public class DuelResponse : ClientPacket
    {
        public DuelResponse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            ArbiterGUID = _worldPacket.ReadPackedGuid128();
            Accepted = _worldPacket.HasBit();
            Forfeited = _worldPacket.HasBit();
        }

        public WowGuid128 ArbiterGUID;
        public bool Accepted;
        public bool Forfeited;
    }

    public class DuelCountdown : ServerPacket
    {
        public DuelCountdown() : base(Opcode.SMSG_DUEL_COUNTDOWN) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Countdown);
        }

        public uint Countdown;
    }

    public class DuelComplete : ServerPacket
    {
        public DuelComplete() : base(Opcode.SMSG_DUEL_COMPLETE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Started);
            _worldPacket.FlushBits();
        }

        public bool Started;
    }

    public class DuelWinner : ServerPacket
    {
        public DuelWinner() : base(Opcode.SMSG_DUEL_WINNER, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBits(BeatenName.GetByteCount(), 6);
            _worldPacket.WriteBits(WinnerName.GetByteCount(), 6);
            _worldPacket.WriteBit(Fled);
            _worldPacket.WriteUInt32(BeatenVirtualRealmAddress);
            _worldPacket.WriteUInt32(WinnerVirtualRealmAddress);
            _worldPacket.WriteString(BeatenName);
            _worldPacket.WriteString(WinnerName);
        }

        public string BeatenName;
        public string WinnerName;
        public uint BeatenVirtualRealmAddress;
        public uint WinnerVirtualRealmAddress;
        public bool Fled;
    }

    public class DuelInBounds : ServerPacket
    {
        public DuelInBounds() : base(Opcode.SMSG_DUEL_IN_BOUNDS, ConnectionType.Instance) { }

        public override void Write() { }
    }

    public class DuelOutOfBounds : ServerPacket
    {
        public DuelOutOfBounds() : base(Opcode.SMSG_DUEL_OUT_OF_BOUNDS, ConnectionType.Instance) { }

        public override void Write() { }
    }
}
