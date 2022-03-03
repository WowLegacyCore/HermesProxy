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

        [PacketHandler(Opcode.CMSG_AREA_TRIGGER)]
        void HandleAreaTrigger(AreaTriggerPkt at)
        {
            if (at.Entered == false)
                return;

            Global.CurrentSessionData.GameState.LastEnteredAreaTrigger = at.AreaTriggerID;
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AREA_TRIGGER);
            packet.WriteUInt32(at.AreaTriggerID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_SELECTION)]
        void HandleSetSelection(SetSelection selection)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_SELECTION);
            packet.WriteGuid(selection.TargetGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_REPOP_REQUEST)]
        void HandleRepopRequest(RepopRequest repop)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_REPOP_REQUEST);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteBool(repop.CheckInstance);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT)]
        void HandleQueryCorpseLocationFromClient(QueryCorpseLocationFromClient query)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_CORPSE_QUERY);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_RECLAIM_CORPSE)]
        void HandleReclaimCorpse(ReclaimCorpse corpse)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_RECLAIM_CORPSE);
            packet.WriteGuid(corpse.CorpseGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_STAND_STATE_CHANGE)]
        void HandleStandStateChange(StandStateChange state)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_STAND_STATE_CHANGE);
            packet.WriteUInt32(state.StandState);
            SendPacketToServer(packet);
        }
    }
}
