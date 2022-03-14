using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_GUILD_COMMAND_RESULT)]
        void HandleGuildCommandResult(WorldPacket packet)
        {
            GuildCommandResult result = new();
            result.Command = (GuildCommandType)packet.ReadUInt32();
            result.Name = packet.ReadCString();
            result.Result = (GuildCommandError)packet.ReadUInt32();
            SendPacketToClient(result);
        }
    }
}
