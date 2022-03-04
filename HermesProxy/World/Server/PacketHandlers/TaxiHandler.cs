using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_TAXI_NODE_STATUS_QUERY)]
        [PacketHandler(Opcode.CMSG_TAXI_QUERY_AVAILABLE_NODES)]
        void HandleTaxiNodesQuery(InteractWithNPC interact)
        {
            WorldPacket packet = new WorldPacket(interact.GetUniversalOpcode());
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ENABLE_TAXI_NODE)]
        void HandleEnableTaxiNode(InteractWithNPC interact)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_TALK_TO_GOSSIP);
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ACTIVATE_TAXI)]
        void HandleActivateTaxi(ActivateTaxi taxi)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI);
            packet.WriteGuid(taxi.FlightMaster.To64());
            packet.WriteUInt32(Global.CurrentSessionData.GameState.CurrentTaxiNode);
            packet.WriteUInt32(taxi.Node);
            SendPacketToServer(packet);
            Global.CurrentSessionData.GameState.IsWaitingForTaxiStart = true;
        }
    }
}
