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


using Framework.Collections;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class QueryPetition : ClientPacket
    {
        public QueryPetition(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetitionID = _worldPacket.ReadUInt32();
            ItemGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 ItemGUID;
        public uint PetitionID;
    }

    public class QueryPetitionResponse : ServerPacket
    {
        public QueryPetitionResponse() : base(Opcode.SMSG_QUERY_PETITION_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(PetitionID);
            _worldPacket.WriteBit(Allow);
            _worldPacket.FlushBits();

            if (Allow)
                Info.Write(_worldPacket);
        }

        public uint PetitionID = 0;
        public bool Allow = false;
        public PetitionInfo Info;
    }

    public class PetitionInfo
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(PetitionID);
            data.WritePackedGuid128(Petitioner);

            data.WriteUInt32(MinSignatures);
            data.WriteUInt32(MaxSignatures);
            data.WriteInt32(DeadLine);
            data.WriteInt32(IssueDate);
            data.WriteInt32(AllowedGuildID);
            data.WriteInt32(AllowedClasses);
            data.WriteInt32(AllowedRaces);
            data.WriteInt16(AllowedGender);
            data.WriteInt32(AllowedMinLevel);
            data.WriteInt32(AllowedMaxLevel);
            data.WriteInt32(NumChoices);
            data.WriteInt32(StaticType);
            data.WriteUInt32(Muid);

            data.WriteBits(Title.GetByteCount(), 7);
            data.WriteBits(BodyText.GetByteCount(), 12);

            for (byte i = 0; i < Choicetext.Length; i++)
                data.WriteBits(Choicetext[i].GetByteCount(), 6);

            data.FlushBits();

            for (byte i = 0; i < Choicetext.Length; i++)
                data.WriteString(Choicetext[i]);

            data.WriteString(Title);
            data.WriteString(BodyText);
        }

        public uint PetitionID;
        public WowGuid128 Petitioner;
        public string Title;
        public string BodyText;
        public uint MinSignatures;
        public uint MaxSignatures;
        public int DeadLine;
        public int IssueDate;
        public int AllowedGuildID;
        public int AllowedClasses;
        public int AllowedRaces;
        public short AllowedGender;
        public int AllowedMinLevel;
        public int AllowedMaxLevel;
        public int NumChoices;
        public int StaticType;
        public uint Muid = 0;
        public StringArray Choicetext = new(10);
    }

    public class ServerPetitionShowList : ServerPacket
    {
        public ServerPetitionShowList() : base(Opcode.SMSG_PETITION_SHOW_LIST) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Unit);
            _worldPacket.WriteInt32(Petitions.Count);
            foreach (var petition in Petitions)
                petition.Write(_worldPacket);
        }

        public WowGuid128 Unit;
        public List<PetitionEntry> Petitions = new();
    }

    public struct PetitionEntry
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Index);
            data.WriteUInt32(CharterCost);
            data.WriteUInt32(CharterEntry);
            data.WriteUInt32(IsArena);
            data.WriteUInt32(RequiredSignatures);
        }

        public uint Index;
        public uint CharterCost;
        public uint CharterEntry;
        public uint IsArena;
        public uint RequiredSignatures;
    }

    public class PetitionBuy : ClientPacket
    {
        public PetitionBuy(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint titleLen = _worldPacket.ReadBits<uint>(7);
            Unit = _worldPacket.ReadPackedGuid128();
            Index = _worldPacket.ReadUInt32();
            Title = _worldPacket.ReadString(titleLen);
        }

        public WowGuid128 Unit;
        public uint Index;
        public string Title;
    }

    public class PetitionShowSignatures : ClientPacket
    {
        public PetitionShowSignatures(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Item = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Item;
    }

    public class ServerPetitionShowSignatures : ServerPacket
    {
        public ServerPetitionShowSignatures() : base(Opcode.SMSG_PETITION_SHOW_SIGNATURES)
        {
            Signatures = new List<PetitionSignature>();
        }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Item);
            _worldPacket.WritePackedGuid128(Owner);
            _worldPacket.WritePackedGuid128(OwnerAccountID);
            _worldPacket.WriteInt32(PetitionID);

            _worldPacket.WriteInt32(Signatures.Count);
            foreach (PetitionSignature signature in Signatures)
            {
                _worldPacket.WritePackedGuid128(signature.Signer);
                _worldPacket.WriteInt32(signature.Choice);
            }
        }

        public WowGuid128 Item;
        public WowGuid128 Owner;
        public WowGuid128 OwnerAccountID;
        public int PetitionID = 0;
        public List<PetitionSignature> Signatures;

        public struct PetitionSignature
        {
            public WowGuid128 Signer;
            public int Choice;
        }
    }

    public class PetitionRenameGuild : ClientPacket
    {
        public PetitionRenameGuild(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetitionGuid = _worldPacket.ReadPackedGuid128();

            _worldPacket.ResetBitPos();
            uint nameLen = _worldPacket.ReadBits<uint>(7);

            NewGuildName = _worldPacket.ReadString(nameLen);
        }

        public WowGuid128 PetitionGuid;
        public string NewGuildName;
    }

    public class PetitionRenameGuildResponse : ServerPacket
    {
        public PetitionRenameGuildResponse() : base(Opcode.SMSG_PETITION_RENAME_GUILD_RESPONSE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PetitionGuid);

            _worldPacket.WriteBits(NewGuildName.GetByteCount(), 7);
            _worldPacket.FlushBits();

            _worldPacket.WriteString(NewGuildName);
        }

        public WowGuid128 PetitionGuid;
        public string NewGuildName;
    }

    public class OfferPetition : ClientPacket
    {
        public OfferPetition(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            UnkInt = _worldPacket.ReadUInt32();
            ItemGUID = _worldPacket.ReadPackedGuid128();
            TargetPlayer = _worldPacket.ReadPackedGuid128();
        }

        public uint UnkInt;
        public WowGuid128 TargetPlayer;
        public WowGuid128 ItemGUID;
    }

    public class DeclinePetition : ClientPacket
    {
        public DeclinePetition(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetitionGUID = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 PetitionGUID;
    }

    public class SignPetition : ClientPacket
    {
        public SignPetition(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PetitionGUID = _worldPacket.ReadPackedGuid128();
            Choice = _worldPacket.ReadUInt8();
        }

        public WowGuid128 PetitionGUID;
        public byte Choice;
    }

    public class PetitionSignResults : ServerPacket
    {
        public PetitionSignResults() : base(Opcode.SMSG_PETITION_SIGN_RESULTS) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Item);
            _worldPacket.WritePackedGuid128(Player);

            _worldPacket.WriteBits(Error, 4);
            _worldPacket.FlushBits();
        }

        public WowGuid128 Item;
        public WowGuid128 Player;
        public PetitionSignResult Error = 0;
    }

    public class TurnInPetition : ClientPacket
    {
        public TurnInPetition(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Item = _worldPacket.ReadPackedGuid128();

            if (_worldPacket.CanRead())
            {
                BackgroundColor = _worldPacket.ReadUInt32();
                EmblemStyle = _worldPacket.ReadUInt32();
                EmblemColor = _worldPacket.ReadUInt32();
                BorderStyle = _worldPacket.ReadUInt32();
                BorderColor = _worldPacket.ReadUInt32();
            }
        }

        public WowGuid128 Item;
        public uint BackgroundColor;
        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
    }

    public class TurnInPetitionResult : ServerPacket
    {
        public TurnInPetitionResult() : base(Opcode.SMSG_TURN_IN_PETITION_RESULT) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Result, 4);
            _worldPacket.FlushBits();
        }

        public PetitionTurnResult Result = 0;
    }
}
