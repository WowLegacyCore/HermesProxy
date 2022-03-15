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
        [PacketHandler(Opcode.CMSG_QUERY_GUILD_INFO)]
        void HandleQueryGuildInfo(QueryGuildInfo query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_GUILD_INFO);
            packet.WriteUInt32((uint)query.GuildGuid.GetLow());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_PERMISSIONS_QUERY)]
        void HandleGuildPermissionsQuery(GuildPermissionsQuery query)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_PERMISSIONS);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY)]
        void HandleGuildBankRemainingWithdrawnMoneyQuery(GuildBankRemainingWithdrawMoneyQuery query)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GUILD_GET_ROSTER)]
        void HandleGuildGetRoster(GuildGetRoster query)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_GET_ROSTER);
            SendPacketToServer(packet);
        }
    }
}
