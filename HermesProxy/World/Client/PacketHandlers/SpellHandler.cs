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
        [PacketHandler(Opcode.SMSG_SEND_KNOWN_SPELLS)]
        void HandleSendKnownSpells(WorldPacket packet)
        {
            SendKnownSpells spells = new SendKnownSpells();
            spells.InitialLogin = packet.ReadBool();
            ushort spellCount = packet.ReadUInt16();
            for (ushort i = 0; i < spellCount; i++)
            {
                uint spellId;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    spellId = packet.ReadUInt32();
                else
                    spellId = packet.ReadUInt16();
                spells.KnownSpells.Add(spellId);
                packet.ReadInt16();
            }
            SendPacketToClient(spells);

            ushort cooldownCount = packet.ReadUInt16();
            if (cooldownCount != 0)
            {
                SendSpellHistory histories = new SendSpellHistory();
                for (ushort i = 0; i < cooldownCount; i++)
                {
                    SpellHistoryEntry history = new SpellHistoryEntry();

                    uint spellId;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                        spellId = packet.ReadUInt32();
                    else
                        spellId = packet.ReadUInt16();
                    history.SpellID = spellId;

                    uint itemId;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                        itemId = packet.ReadUInt32();
                    else
                        itemId = packet.ReadUInt16();
                    history.ItemID = itemId;

                    history.Category = packet.ReadUInt16();
                    history.RecoveryTime = packet.ReadInt32();
                    history.CategoryRecoveryTime = packet.ReadInt32();

                    histories.Entries.Add(history);
                }
                SendPacketToClient(histories, Opcode.SMSG_SEND_UNLEARN_SPELLS);
            }

            // These packets don't exist in Vanilla.
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                SendPacketToClient(new SendUnlearnSpells());
                SendPacketToClient(new SendSpellCharges());
            }
        }

        [PacketHandler(Opcode.SMSG_SEND_UNLEARN_SPELLS)]
        void HandleSendUnlearnSpells(WorldPacket packet)
        {
            SendUnlearnSpells spells = new SendUnlearnSpells();
            uint spellCount = packet.ReadUInt32();
            for (uint i = 0; i < spellCount; i++)
            {
                uint spellId = packet.ReadUInt32();
                spells.Spells.Add(spellId);
            }
            SendPacketToClient(spells);
            SendPacketToClient(new SendUnlearnSpells());
        }
    }
}
