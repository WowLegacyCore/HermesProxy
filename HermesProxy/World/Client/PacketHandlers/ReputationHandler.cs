using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_INITIALIZE_FACTIONS)]
        void HandleInitializeFactions(WorldPacket packet)
        {
            if (!GetSession().GameState.IsFirstEnterWorld)
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

        [PacketHandler(Opcode.SMSG_SET_FACTION_STANDING)]
        void HandleSetFactionStanding(WorldPacket packet)
        {
            SetFactionStanding standing = new();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                packet.ReadFloat(); // Reputation loss

            bool showVisual = true;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                showVisual = packet.ReadBool();
            standing.ShowVisual = showVisual;

            var count = packet.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                FactionStandingData faction = new()
                {
                    Index = packet.ReadInt32(),
                    Standing = packet.ReadInt32()
                };
                standing.Factions.Add(faction);
            }
            SendPacketToClient(standing);
        }

        [PacketHandler(Opcode.SMSG_SET_FORCED_REACTIONS)]
        void HandleSetForcedReaction(WorldPacket packet)
        {
            SetForcedReactions reactions = new();
            var count = packet.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                ForcedReaction reaction = new()
                {
                    Faction = packet.ReadInt32(),
                    Reaction = packet.ReadInt32()
                };
                reactions.Reactions.Add(reaction);
            }
            SendPacketToClient(reactions);
        }

        [PacketHandler(Opcode.SMSG_SET_FACTION_VISIBLE)]
        void HandleSetFactionVisible(WorldPacket packet)
        {
            SetFactionVisible faction = new(true)
            {
                FactionIndex = packet.ReadUInt32()
            };
            SendPacketToClient(faction);
        }
    }
}
