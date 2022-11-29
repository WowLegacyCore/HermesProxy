using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_GAME_OBJ_USE)]
        void HandleGameObjUse(GameObjUse use)
        {
            WorldPacket packet = new(Opcode.CMSG_GAME_OBJ_USE);
            packet.WriteGuid(use.Guid.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GAME_OBJ_REPORT_USE)]
        void HandleGameObjUse(GameObjReportUse use)
        {
            GetSession().GameState.CurrentInteractedWithGO = use.Guid;
        }
    }
}
