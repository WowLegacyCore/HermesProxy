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
        [PacketHandler(Opcode.SMSG_INIT_WORLD_STATES)]
        void HandleInitWorldStates(WorldPacket packet)
        {
            InitWorldStates states = new InitWorldStates();
            states.MapID = packet.ReadUInt32();
            Global.CurrentSessionData.GameState.CurrentMapId = states.MapID;
            states.AreaID = packet.ReadUInt32();
            states.SubareaID = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_1_0_6692) ? packet.ReadUInt32() : states.AreaID;

            ushort count = packet.ReadUInt16();
            for (ushort i = 0; i < count; i++)
            {
                uint variable = packet.ReadUInt32();
                int value = packet.ReadInt32();
                if (variable != 0 || value != 0)
                    states.AddState(variable, value);
            }
            states.AddClassicStates();
            SendPacketToClient(states);

            // These packets don't exist in old versions.
            SendPacketToClient(new SetupCurrency());
            SendPacketToClient(new AllAccountCriteria());
        }
    }
}
