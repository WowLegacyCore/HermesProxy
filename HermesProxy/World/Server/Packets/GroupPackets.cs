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
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class UpdateRaidTarget : ClientPacket
    {
        public UpdateRaidTarget(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PartyIndex = _worldPacket.ReadInt8();
            Target = _worldPacket.ReadPackedGuid128();
            Symbol = _worldPacket.ReadInt8();
        }

        public sbyte PartyIndex;
        public WowGuid128 Target;
        public sbyte Symbol;
    }

    class SendRaidTargetUpdateSingle : ServerPacket
    {
        public SendRaidTargetUpdateSingle() : base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_SINGLE) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WriteInt8(Symbol);
            _worldPacket.WritePackedGuid128(Target);
            _worldPacket.WritePackedGuid128(ChangedBy);
        }

        public sbyte PartyIndex;
        public sbyte Symbol;
        public WowGuid128 Target;
        public WowGuid128 ChangedBy;
    }

    class SendRaidTargetUpdateAll : ServerPacket
    {
        public SendRaidTargetUpdateAll() : base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_ALL) { }

        public override void Write()
        {
            _worldPacket.WriteInt8(PartyIndex);
            _worldPacket.WriteInt32(TargetIcons.Count);

            foreach (var pair in TargetIcons)
            {
                _worldPacket.WritePackedGuid128(pair.Item2);
                _worldPacket.WriteInt8(pair.Item1);
            }
        }

        public sbyte PartyIndex;
        public List<Tuple<sbyte, WowGuid128>> TargetIcons = new();
    }

    class SummonRequest : ServerPacket
    {
        public SummonRequest() : base(Opcode.SMSG_SUMMON_REQUEST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(SummonerGUID);
            _worldPacket.WriteUInt32(SummonerVirtualRealmAddress);
            _worldPacket.WriteInt32(AreaID);
            _worldPacket.WriteUInt8((byte)Reason);
            _worldPacket.WriteBit(SkipStartingArea);
            _worldPacket.FlushBits();
        }

        public WowGuid128 SummonerGUID;
        public uint SummonerVirtualRealmAddress;
        public int AreaID;
        public SummonReason Reason;
        public bool SkipStartingArea;

        public enum SummonReason
        {
            Spell = 0,
            Scenario = 1
        }
    }

    class SummonResponse : ClientPacket
    {
        public SummonResponse(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SummonerGUID = _worldPacket.ReadPackedGuid128();
            Accept = _worldPacket.HasBit();
        }

        public WowGuid128 SummonerGUID;
        public bool Accept;
    }
}
