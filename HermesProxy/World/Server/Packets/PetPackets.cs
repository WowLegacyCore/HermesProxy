/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class PetSpells : ServerPacket
    {
        public PetSpells() : base(Opcode.SMSG_PET_SPELLS_MESSAGE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PetGUID);
            _worldPacket.WriteUInt16(CreatureFamily);
            _worldPacket.WriteInt16(Specialization);
            _worldPacket.WriteUInt32(TimeLimit);
            _worldPacket.WriteUInt16((ushort)((byte)CommandState | (Flag << 16)));
            _worldPacket.WriteUInt8((byte)ReactState);

            foreach (uint actionButton in ActionButtons)
                _worldPacket.WriteUInt32(actionButton);

            _worldPacket.WriteInt32(Actions.Count);
            _worldPacket.WriteInt32(Cooldowns.Count);
            _worldPacket.WriteInt32(SpellHistory.Count);

            foreach (uint action in Actions)
                _worldPacket.WriteUInt32(action);

            foreach (PetSpellCooldown cooldown in Cooldowns)
            {
                _worldPacket.WriteUInt32(cooldown.SpellID);
                _worldPacket.WriteUInt32(cooldown.Duration);
                _worldPacket.WriteUInt32(cooldown.CategoryDuration);
                _worldPacket.WriteFloat(cooldown.ModRate);
                _worldPacket.WriteUInt16(cooldown.Category);
            }

            foreach (PetSpellHistory history in SpellHistory)
            {
                _worldPacket.WriteUInt32(history.CategoryID);
                _worldPacket.WriteUInt32(history.RecoveryTime);
                _worldPacket.WriteFloat(history.ChargeModRate);
                _worldPacket.WriteInt8(history.ConsumedCharges);
            }
        }

        public WowGuid128 PetGUID;
        public ushort CreatureFamily;
        public short Specialization = -1;
        public uint TimeLimit;
        public ReactStates ReactState;
        public CommandStates CommandState;
        public byte Flag;

        public uint[] ActionButtons = new uint[10];

        public List<uint> Actions = new();
        public List<PetSpellCooldown> Cooldowns = new();
        public List<PetSpellHistory> SpellHistory = new();
    }

    public class PetSpellCooldown
    {
        public uint SpellID;
        public uint Duration;
        public uint CategoryDuration;
        public float ModRate = 1.0f;
        public ushort Category;
    }

    public class PetSpellHistory
    {
        public uint CategoryID;
        public uint RecoveryTime;
        public float ChargeModRate = 1.0f;
        public sbyte ConsumedCharges;
    }

    public class PetClearSpells : ServerPacket
    {
        public PetClearSpells() : base(Opcode.SMSG_PET_CLEAR_SPELLS, ConnectionType.Instance) { }

        public override void Write()
        {
        }
    }

    class PetAction : ClientPacket
    {
        public PetAction(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetGUID = _worldPacket.ReadPackedGuid128();

            Action = _worldPacket.ReadUInt32();
            TargetGUID = _worldPacket.ReadPackedGuid128();

            ActionPosition = _worldPacket.ReadVector3();
        }

        public WowGuid128 PetGUID;
        public uint Action;
        public WowGuid128 TargetGUID;
        public Vector3 ActionPosition;
    }

    class PetRename : ClientPacket
    {
        public PetRename(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            RenameData.PetGUID = _worldPacket.ReadPackedGuid128();
            RenameData.PetNumber = _worldPacket.ReadInt32();

            uint nameLen = _worldPacket.ReadBits<uint>(8);

            RenameData.HasDeclinedNames = _worldPacket.HasBit();
            if (RenameData.HasDeclinedNames)
            {
                RenameData.DeclinedNames = new DeclinedName();
                uint[] count = new uint[PlayerConst.MaxDeclinedNameCases];
                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    count[i] = _worldPacket.ReadBits<uint>(7);

                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    RenameData.DeclinedNames.name[i] = _worldPacket.ReadString(count[i]);
            }

            RenameData.NewName = _worldPacket.ReadString(nameLen);
        }

        public PetRenameData RenameData;
    }

    struct PetRenameData
    {
        public WowGuid128 PetGUID;
        public int PetNumber;
        public string NewName;
        public bool HasDeclinedNames;
        public DeclinedName DeclinedNames;
    }

    class PetAbandon : ClientPacket
    {
        public PetAbandon(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 PetGUID;
    }

    class RequestStabledPets : ClientPacket
    {
        public RequestStabledPets(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            StableMaster = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 StableMaster;
    }

    class PetStableList : ServerPacket
    {
        public PetStableList() : base(Opcode.SMSG_PET_STABLE_LIST, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(StableMaster);
            _worldPacket.WriteInt32(Pets.Count);
            _worldPacket.WriteUInt8(NumStableSlots);
            foreach (PetStableInfo pet in Pets)
            {
                _worldPacket.WriteUInt32(pet.PetNumber);
                _worldPacket.WriteUInt32(pet.CreatureID);
                _worldPacket.WriteUInt32(pet.DisplayID);
                _worldPacket.WriteUInt32(pet.ExperienceLevel);
                _worldPacket.WriteUInt8(pet.LoyaltyLevel);
                _worldPacket.WriteUInt8(pet.PetFlags);
                _worldPacket.WriteBits(pet.PetName.GetByteCount(), 8);
                _worldPacket.WriteString(pet.PetName);
            }
        }

        public WowGuid128 StableMaster;
        public byte NumStableSlots;
        public List<PetStableInfo> Pets = new();
    }

    class PetStableInfo
    {
        public uint PetNumber;
        public uint CreatureID;
        public uint DisplayID;
        public uint ExperienceLevel;
        public byte LoyaltyLevel = 1;
        public byte PetFlags;
        public string PetName;
    }

    class BuyStableSlot : ClientPacket
    {
        public BuyStableSlot(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            StableMaster = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 StableMaster;
    }

    public class PetGuids : ServerPacket
    {
        public PetGuids() : base(Opcode.SMSG_PET_GUIDS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Guids.Count);
            foreach (var guid in Guids)
                _worldPacket.WritePackedGuid128(guid);
        }

        public List<WowGuid128> Guids = new List<WowGuid128>();
    }

    class PetStableResult : ServerPacket
    {
        public PetStableResult() : base(Opcode.SMSG_PET_STABLE_RESULT, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8(Result);
        }

        public byte Result;
    }

    class StablePet : ClientPacket
    {
        public StablePet(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            StableMaster = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 StableMaster;
    }

    class UnstablePet : ClientPacket
    {
        public UnstablePet(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetNumber = _worldPacket.ReadUInt32();
            StableMaster = _worldPacket.ReadPackedGuid128();
        }

        public uint PetNumber;
        public WowGuid128 StableMaster;
    }

    class StableSwapPet : ClientPacket
    {
        public StableSwapPet(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetNumber = _worldPacket.ReadUInt32();
            StableMaster = _worldPacket.ReadPackedGuid128();
        }

        public uint PetNumber;
        public WowGuid128 StableMaster;
    }
}
