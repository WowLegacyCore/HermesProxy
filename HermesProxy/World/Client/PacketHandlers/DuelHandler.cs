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
        [PacketHandler(Opcode.SMSG_DUEL_REQUESTED)]
        void HandleDuelRequested(WorldPacket packet)
        {
            DuelRequested duel = new DuelRequested();
            duel.ArbiterGUID = packet.ReadGuid().To128();
            duel.RequestedByGUID = packet.ReadGuid().To128();
            duel.RequestedByWowAccount = GetSession().GetGameAccountGuidForPlayer(duel.RequestedByGUID);
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_COUNTDOWN)]
        void HandleDuelCountdown(WorldPacket packet)
        {
            DuelCountdown duel = new DuelCountdown();
            duel.Countdown = packet.ReadUInt32();
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_COMPLETE)]
        void HandleDuelComplete(WorldPacket packet)
        {
            DuelComplete duel = new DuelComplete();
            duel.Started = packet.ReadBool();
            SendPacketToClient(duel);
        }

        [PacketHandler(Opcode.SMSG_DUEL_WINNER)]
        void HandleDuelWinner(WorldPacket packet)
        {
            DuelWinner duel = new DuelWinner();
            duel.Fled = packet.ReadBool();
            duel.BeatenName = packet.ReadCString();
            duel.WinnerName = packet.ReadCString();
            duel.BeatenVirtualRealmAddress = GetSession().RealmId.GetAddress();
            duel.WinnerVirtualRealmAddress = GetSession().RealmId.GetAddress();
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
