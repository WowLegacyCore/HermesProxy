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
            GetSession().GameState.CurrentPetGuid = guid.To128();
            GetSession().GameState.LastClientPetCastGuid = null;
            GetSession().GameState.LastClientPetCastSpellId = 0;
            GetSession().GameState.LastClientPetCastHasStarted = false;

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

        [PacketHandler(Opcode.SMSG_PET_ACTION_SOUND)]
        void HandlePetActionSound(WorldPacket packet)
        {
            PetActionSound sound = new PetActionSound();
            sound.UnitGUID = packet.ReadGuid().To128();
            sound.Action = packet.ReadUInt32();
            SendPacketToClient(sound);
        }

        [PacketHandler(Opcode.MSG_LIST_STABLED_PETS)]
        void HandleListStabledPets(WorldPacket packet)
        {
            PetGuids pets = new PetGuids();
            var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
            int UNIT_FIELD_SUMMON = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMON);
            if (UNIT_FIELD_SUMMON >= 0 && updateFields.ContainsKey(UNIT_FIELD_SUMMON))
            {
                WowGuid128 guid = GetGuidValue(updateFields, UnitField.UNIT_FIELD_SUMMON).To128();
                if (!guid.IsEmpty())
                    pets.Guids.Add(guid);
            }
            SendPacketToClient(pets);

            PetStableList stable = new PetStableList();
            stable.StableMaster = packet.ReadGuid().To128();
            byte count = packet.ReadUInt8();
            stable.NumStableSlots = packet.ReadUInt8();
            for (byte i = 0; i < count; i++)
            {
                PetStableInfo pet = new PetStableInfo();
                pet.PetNumber = packet.ReadUInt32();
                pet.CreatureID = packet.ReadUInt32();
                pet.ExperienceLevel = packet.ReadUInt32();
                pet.PetName = packet.ReadCString();
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                    pet.LoyaltyLevel = (byte)packet.ReadUInt32();
                pet.PetFlags = packet.ReadUInt8();

                if (pet.PetFlags != 1)
                    pet.PetFlags = 3;

                CreatureTemplate template = GameData.GetCreatureTemplate(pet.CreatureID);
                if (template != null)
                    pet.DisplayID = template.Display.CreatureDisplay[0].CreatureDisplayID;
                else
                {
                    WorldPacket query = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
                    query.WriteUInt32(pet.CreatureID);
                    query.WriteGuid(WowGuid64.Empty);
                    SendPacket(query);
                }

                stable.Pets.Add(pet);
            }
            SendPacketToClient(stable);
        }

        [PacketHandler(Opcode.SMSG_PET_STABLE_RESULT)]
        void HandlePetStableResult(WorldPacket packet)
        {
            PetStableResult stable = new PetStableResult();
            stable.Result = packet.ReadUInt8();
            SendPacketToClient(stable);
        }
    }
}
