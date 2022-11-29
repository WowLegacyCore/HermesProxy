using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_DUEL_REQUESTED)]
        void HandleDuelRequested(WorldPacket packet)
        {
            DuelRequested duel = new DuelRequested
            {
                ArbiterGUID = packet.ReadGuid().To128(GetSession().GameState),
                RequestedByGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            duel.RequestedByWowAccount = GetSession().GetGameAccountGuidForPlayer(duel.RequestedByGUID);
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_COUNTDOWN)]
        void HandleDuelCountdown(WorldPacket packet)
        {
            DuelCountdown duel = new DuelCountdown
            {
                Countdown = packet.ReadUInt32()
            };
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_COMPLETE)]
        void HandleDuelComplete(WorldPacket packet)
        {
            DuelComplete duel = new DuelComplete
            {
                Started = packet.ReadBool()
            };
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_WINNER)]
        void HandleDuelWinner(WorldPacket packet)
        {
            DuelWinner duel = new DuelWinner
            {
                Fled = packet.ReadBool(),
                BeatenName = packet.ReadCString(),
                WinnerName = packet.ReadCString(),
                BeatenVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                WinnerVirtualRealmAddress = GetSession().RealmId.GetAddress()
            };
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_IN_BOUNDS)]
        void HandleDuelInBounds(WorldPacket packet)
        {
            DuelInBounds duel = new DuelInBounds();
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_OUT_OF_BOUNDS)]
        void HandleDuelOutOfBounds(WorldPacket packet)
        {
            DuelOutOfBounds duel = new DuelOutOfBounds();
            SendPacketToClient(duel);
        }
    }
}
