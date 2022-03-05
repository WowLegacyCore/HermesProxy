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
        [PacketHandler(Opcode.SMSG_PET_SPELLS_MESSAGE)]
        void HandlePetSpellsMessage(WorldPacket packet)
        {
            WowGuid guid = packet.ReadGuid();

            // Equal to "Clear spells" pre cataclysm
            if (guid.IsEmpty())
            {
                PetClearSpells clear = new();
                SendPacketToClient(clear);
                return;
            }

            PetSpells spells = new();
            spells.PetGUID = guid.To128();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                spells.CreatureFamily = packet.ReadUInt16();

            spells.TimeLimit = packet.ReadUInt32();

            PetModeFlags petModeFlags;
            ReadPetFlags(packet, out spells.ReactState, out spells.CommandState, out petModeFlags);
            spells.Flag = (byte)petModeFlags;

            const int maxCreatureSpells = 10;
            for (int i = 0; i < maxCreatureSpells; i++) // Read pet/vehicle spell ids
                spells.ActionButtons[i] = packet.ReadUInt32();

            byte spellCount = packet.ReadUInt8();
            for (int i = 0; i < spellCount; i++)
                spells.Actions.Add(packet.ReadUInt32());

            byte cdCount = packet.ReadUInt8();
            for (int i = 0; i < cdCount; i++)
            {
                PetSpellCooldown cooldown = new();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    cooldown.SpellID = packet.ReadUInt32();
                else
                    cooldown.SpellID = packet.ReadUInt16();

                cooldown.Category = packet.ReadUInt16();
                cooldown.Duration = packet.ReadUInt32();
                cooldown.CategoryDuration = packet.ReadUInt32();

                spells.Cooldowns.Add(cooldown);
            }
            
            SendPacketToClient(spells);
        }

        void ReadPetFlags(WorldPacket packet, out ReactStates reactState, out CommandStates commandState, out PetModeFlags petModeFlags)
        {
            var raw = packet.ReadUInt32();
            reactState = (ReactStates)((raw >> 8) & 0xFF);
            commandState = (CommandStates)((raw >> 16) & 0xFF);
            petModeFlags = (PetModeFlags)(raw & 0xFFFF0000);
        }
    }
}
