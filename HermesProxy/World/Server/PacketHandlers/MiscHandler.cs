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
        [PacketHandler(Opcode.CMSG_TIME_SYNC_RESPONSE)]
        void HandleTimeSyncResponse(TimeSyncResponse response)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_TIME_SYNC_RESPONSE);
                packet.WriteUInt32(response.SequenceIndex);
                packet.WriteUInt32(response.ClientTime);
                SendPacketToServer(packet);
            }
        }
    }
}
