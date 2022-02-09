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
        [PacketHandler(Opcode.SMSG_SET_PROFICIENCY)]
        void HandleSetProficiency(WorldPacket packet)
        {
            SetProficiency proficiency = new SetProficiency();
            proficiency.ProficiencyClass = packet.ReadUInt8();
            proficiency.ProficiencyMask = packet.ReadUInt32();
            SendPacketToClient(proficiency);
        }
    }
}
