using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_CAN_DUEL)]
        void HandleCanDuel(CanDuel request)
        {
            CanDuelResult result = new CanDuelResult();
            result.TargetGUID = request.TargetGUID;
            result.Result = true;
            SendPacket(result);
        }

        [PacketHandler(Opcode.CMSG_DUEL_RESPONSE)]
        void HandleDuelResponse(DuelResponse response)
        {
            if (response.Accepted)
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_DUEL_ACCEPTED);
                packet.WriteGuid(response.ArbiterGUID.To64());
                SendPacketToServer(packet);
            }
            else
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_DUEL_CANCELLED);
                packet.WriteGuid(response.ArbiterGUID.To64());
                SendPacketToServer(packet);
            }
        }
    }
}
