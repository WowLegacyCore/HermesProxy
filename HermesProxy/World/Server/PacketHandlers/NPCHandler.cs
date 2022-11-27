using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_BANKER_ACTIVATE)]
        [PacketHandler(Opcode.CMSG_BINDER_ACTIVATE)]
        [PacketHandler(Opcode.CMSG_LIST_INVENTORY)]
        [PacketHandler(Opcode.CMSG_SPIRIT_HEALER_ACTIVATE)]
        [PacketHandler(Opcode.CMSG_TALK_TO_GOSSIP)]
        [PacketHandler(Opcode.CMSG_TRAINER_LIST)]
        [PacketHandler(Opcode.CMSG_BATTLEMASTER_HELLO)]
        [PacketHandler(Opcode.CMSG_AREA_SPIRIT_HEALER_QUERY)]
        [PacketHandler(Opcode.CMSG_AREA_SPIRIT_HEALER_QUEUE)]
        void HandleInteractWithNPC(InteractWithNPC interact)
        {
            WorldPacket packet = new WorldPacket(interact.GetUniversalOpcode());
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GOSSIP_SELECT_OPTION)]
        void HandleGossipSelectOption(GossipSelectOption gossip)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GOSSIP_SELECT_OPTION);
            packet.WriteGuid(gossip.GossipUnit.To64());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteUInt32(gossip.GossipID);
            packet.WriteUInt32(gossip.GossipIndex);
            if (!String.IsNullOrEmpty(gossip.PromotionCode))
                packet.WriteCString(gossip.PromotionCode);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BUY_BANK_SLOT)]
        void HandleBuyBankSlot(BuyBankSlot bank)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_BANK_SLOT);
            packet.WriteGuid(bank.Guid.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_TRAINER_BUY_SPELL)]
        void HandleTrainerBuySpell(TrainerBuySpell buy)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_TRAINER_BUY_SPELL);
            packet.WriteGuid(buy.TrainerGUID.To64());
            if (ModernVersion.ExpansionVersion > 1 &&
                LegacyVersion.ExpansionVersion <= 1)
            {
                // in vanilla the server sends learn spell with effect 36
                // in expansions the server sends the actual spell
                buy.SpellID = GetSession().GameState.GetLearnSpellFromRealSpell(buy.SpellID);
            }
            packet.WriteUInt32(buy.SpellID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CONFIRM_RESPEC_WIPE)]
        void HandleConfirmRespecWipe(ConfirmRespecWipe respec)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_TALENT_WIPE_CONFIRM);
            packet.WriteGuid(respec.TrainerGUID.To64());
            SendPacketToServer(packet);
        }
    }
}
