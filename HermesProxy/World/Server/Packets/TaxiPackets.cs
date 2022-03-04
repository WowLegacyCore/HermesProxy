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
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    class TaxiNodeStatusPkt : ServerPacket
    {
        public TaxiNodeStatusPkt() : base(Opcode.SMSG_TAXI_NODE_STATUS) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(FlightMaster);
            _worldPacket.WriteBits(Status, 2);
            _worldPacket.FlushBits();
        }

        public WowGuid128 FlightMaster;
        public TaxiNodeStatus Status;
    }

    public class ShowTaxiNodes : ServerPacket
    {
        public ShowTaxiNodes() : base(Opcode.SMSG_SHOW_TAXI_NODES) { }

        public override void Write()
        {
            _worldPacket.WriteBit(WindowInfo != null);
            _worldPacket.FlushBits();

            _worldPacket.WriteInt32(CanLandNodes.Count);
            _worldPacket.WriteInt32(CanUseNodes.Count);

            if (WindowInfo != null)
            {
                _worldPacket.WritePackedGuid128(WindowInfo.UnitGUID);
                _worldPacket.WriteUInt32(WindowInfo.CurrentNode);
            }

            foreach (var node in CanLandNodes)
                _worldPacket.WriteUInt8(node);

            foreach (var node in CanUseNodes)
                _worldPacket.WriteUInt8(node);
        }

        public ShowTaxiNodesWindowInfo WindowInfo;
        public List<byte> CanLandNodes = new(); // Nodes known by player
        public List<byte> CanUseNodes = new(); // Nodes available for use - this can temporarily disable a known node
    }

    public class ShowTaxiNodesWindowInfo
    {
        public WowGuid128 UnitGUID;
        public uint CurrentNode;
    }

    class ActivateTaxi : ClientPacket
    {
        public ActivateTaxi(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            FlightMaster = _worldPacket.ReadPackedGuid128();
            Node = _worldPacket.ReadUInt32();
            GroundMountID = _worldPacket.ReadUInt32();
            FlyingMountID = _worldPacket.ReadUInt32();
        }

        public WowGuid128 FlightMaster;
        public uint Node;
        public uint GroundMountID;
        public uint FlyingMountID;
    }

    class NewTaxiPath : ServerPacket
    {
        public NewTaxiPath() : base(Opcode.SMSG_NEW_TAXI_PATH) { }

        public override void Write() { }
    }

    class ActivateTaxiReplyPkt : ServerPacket
    {
        public ActivateTaxiReplyPkt() : base(Opcode.SMSG_ACTIVATE_TAXI_REPLY) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Reply, 4);
            _worldPacket.FlushBits();
        }

        public ActivateTaxiReply Reply;
    }
}
