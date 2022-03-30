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
        [PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN)]
        void HandleBattlefieldJoin(BattlemasterJoin join)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN);
            packet.WriteGuid(join.BattlemasterGuid.To64());
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteUInt32(GameData.GetMapIdFromBattlegroundId(join.BattlefieldListId));
            else
                packet.WriteUInt32(join.BattlefieldListId);
            packet.WriteInt32(join.BattlefieldInstanceID);
            packet.WriteBool(join.JoinAsGroup);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BATTLEFIELD_PORT)]
        void HandleBattlefieldPort(BattlefieldPort port)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_PORT);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt8(2);
                packet.WriteUInt8(0);
                packet.WriteUInt32(GetSession().GameState.GetBattleFieldQueueType(port.Ticket.Id));
                packet.WriteUInt16(0x1F90);
                packet.WriteBool(port.AcceptedInvite);
            }
            else
            {
                packet.WriteUInt32(GetSession().GameState.GetBattleFieldQueueType(port.Ticket.Id));
                packet.WriteBool(port.AcceptedInvite);
            }
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_REQUEST_BATTLEFIELD_STATUS)]
        void HandleRequestBattlefieldStatus(RequestBattlefieldStatus log)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_STATUS);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PVP_LOG_DATA)]
        void HandlePvPLogData(PVPLogDataRequest log)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_PVP_LOG_DATA);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BATTLEFIELD_LEAVE)]
        void HandleBattlefieldLeave(BattlefieldLeave leave)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_LEAVE);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt8(2);
                packet.WriteUInt8(0);
                packet.WriteUInt32(GetSession().GameState.GetBattleFieldQueueType(1));
                packet.WriteUInt16(0x1F90);
            }
            else
                packet.WriteUInt32((uint)GetSession().GameState.CurrentMapId);
            SendPacketToServer(packet);
        }
    }
}
