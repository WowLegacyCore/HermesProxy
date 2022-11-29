﻿using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_RESET_INSTANCES)]
        void HandleResetInstances(EmptyClientPacket reset)
        {
            WorldPacket packet = new(Opcode.CMSG_RESET_INSTANCES);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_REQUEST_RAID_INFO)]
        void HandleRequestRaidInfo(EmptyClientPacket reset)
        {
            WorldPacket packet = new(Opcode.CMSG_REQUEST_RAID_INFO);
            SendPacketToServer(packet);
        }
    }
}
