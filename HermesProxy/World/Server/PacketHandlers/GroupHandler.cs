using Framework.Constants;
using Framework.Logging;
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
        [PacketHandler(Opcode.CMSG_UPDATE_RAID_TARGET)]
        void HandleUpdateRaidTarget(UpdateRaidTarget update)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_TARGET_UPDATE);
            packet.WriteInt8(update.Symbol);
            packet.WriteGuid(update.Target.To64());
            SendPacketToServer(packet);
        }
    }
}
