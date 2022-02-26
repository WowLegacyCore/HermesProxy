using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_INITIALIZE_FACTIONS)]
        void HandleInitializeFactions(WorldPacket packet)
        {
            if (!Global.CurrentSessionData.GameState.IsFirstEnterWorld)
                return;

            InitializeFactions factions = new InitializeFactions();
            uint count = packet.ReadUInt32();
            for (uint i = 0; i < count; i ++)
            {
                factions.FactionFlags[i] = (ReputationFlags)packet.ReadUInt8();
                factions.FactionStandings[i] = packet.ReadInt32();
            }
            SendPacketToClient(factions);

            // This packet does not exist in Vanilla, but it must be sent for client to be able to move.
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                SendPacketToClient(new TimeSyncRequest());
        }
    }
}
