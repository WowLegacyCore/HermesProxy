using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_FEATURE_SYSTEM_STATUS)]
        void HandleFeatureSystemStatus(WorldPacket packet)
        {
            GetSession().RealmSocket.SendFeatureSystemStatus();
        }

        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_MOTD)]
        void HandleMotd(WorldPacket packet)
        {
            MOTD motd = new MOTD();
            uint count = packet.ReadUInt32();
            for (uint i = 0; i < count; i++)
                motd.Text.Add(packet.ReadCString());
            SendPacketToClient(motd);

            // These packets don't exist in old clients (for vanilla servers we send them after account data times along with others).
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                GetSession().RealmSocket.SendSetTimeZoneInformation();
                GetSession().RealmSocket.SendSeasonInfo();
            }
        }
    }
}
