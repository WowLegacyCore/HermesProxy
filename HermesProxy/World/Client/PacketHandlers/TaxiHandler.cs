using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_TAXI_NODE_STATUS)]
        void HandleTaxiNodeStatus(WorldPacket packet)
        {
            TaxiNodeStatusPkt taxi = new()
            {
                FlightMaster = packet.ReadGuid().To128(GetSession().GameState)
            };
            bool learned = packet.ReadBool();
            taxi.Status = learned ? TaxiNodeStatus.Learned : TaxiNodeStatus.Unlearned;
            SendPacketToClient(taxi);
        }
        [PacketHandler(Opcode.SMSG_SHOW_TAXI_NODES)]
        void HandleShowTaxiNodes(WorldPacket packet)
        {
            ShowTaxiNodes taxi = new();
            bool hasWindowInfo = packet.ReadUInt32() != 0;
            if (hasWindowInfo)
            {
                taxi.WindowInfo = new()
                {
                    UnitGUID = packet.ReadGuid().To128(GetSession().GameState),
                    CurrentNode = GetSession().GameState.CurrentTaxiNode = packet.ReadUInt32()
                };
            }
            while (packet.CanRead())
            {
                byte nodesMask = packet.ReadUInt8();
                taxi.CanLandNodes.Add(nodesMask);
                taxi.CanUseNodes.Add(nodesMask);
            }
            GetSession().GameState.UsableTaxiNodes = taxi.CanUseNodes; // save for CMSG_ACTIVATE_TAXI_EXPRESS
            SendPacketToClient(taxi);
        }
        [PacketHandler(Opcode.SMSG_NEW_TAXI_PATH)]
        void HandleNewTaxiPath(WorldPacket packet)
        {
            NewTaxiPath taxi = new();
            SendPacketToClient(taxi);
        }
        [PacketHandler(Opcode.SMSG_ACTIVATE_TAXI_REPLY)]
        void HandleActivateTaxiReply(WorldPacket packet)
        {
            ActivateTaxiReply reply = (ActivateTaxiReply)packet.ReadUInt32();
            // Ok status needs to be sent after the monster move packet.
            if (reply != ActivateTaxiReply.Ok)
            {
                ActivateTaxiReplyPkt taxi = new()
                {
                    Reply = reply
                };
                SendPacketToClient(taxi);
                GetSession().GameState.IsWaitingForTaxiStart = false;
            }
        }
    }
}
