﻿using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using static HermesProxy.World.Server.Packets.ServerPetitionShowSignatures;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_PETITION_SHOW_LIST)]
        void HandlePetitionShowList(WorldPacket packet)
        {
            ServerPetitionShowList petitions = new()
            {
                Unit = packet.ReadGuid().To128(GetSession().GameState)
            };
            GetSession().GameState.CurrentInteractedWithNPC = petitions.Unit;
            var count = packet.ReadUInt8();
            for (var i = 0; i < count; i++)
            {
                PetitionEntry petition = new()
                {
                    Index = packet.ReadUInt32(),
                    CharterEntry = packet.ReadUInt32()
                };
                packet.ReadUInt32(); // Charter Display
                petition.CharterCost = packet.ReadUInt32();

                if (packet.ReadUInt32() != 0)
                    petition.IsArena = 1;

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    petition.RequiredSignatures = packet.ReadUInt32(); // Required signs
                else
                    petition.RequiredSignatures = 9;

                petitions.Petitions.Add(petition);

            }
            SendPacketToClient(petitions);
        }

        [PacketHandler(Opcode.SMSG_PETITION_SHOW_SIGNATURES)]
        void HandlePetitionShowSignatures(WorldPacket packet)
        {
            ServerPetitionShowSignatures petition = new()
            {
                Item = packet.ReadGuid().To128(GetSession().GameState),
                Owner = packet.ReadGuid().To128(GetSession().GameState)
            };
            petition.OwnerAccountID = GetSession().GetGameAccountGuidForPlayer(petition.Owner);
            petition.PetitionID = packet.ReadInt32();
            var counter = packet.ReadUInt8();
            for (var i = 0; i < counter; i++)
            {
                PetitionSignature signature = new()
                {
                    Signer = packet.ReadGuid().To128(GetSession().GameState),
                    Choice = packet.ReadInt32()
                };
                petition.Signatures.Add(signature);
            }
            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.SMSG_QUERY_PETITION_RESPONSE)]
        void HandlePetitionQueryResponse(WorldPacket packet)
        {
            QueryPetitionResponse petition = new()
            {
                PetitionID = packet.ReadUInt32(),
                Allow = true
            };
            petition.Info = new PetitionInfo
            {
                PetitionID = petition.PetitionID,
                Petitioner = packet.ReadGuid().To128(GetSession().GameState),

                Title = packet.ReadCString(),
                BodyText = packet.ReadCString()
            };

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.ReadUInt32(); // flags

            petition.Info.MinSignatures = packet.ReadUInt32();
            petition.Info.MaxSignatures = packet.ReadUInt32();
            petition.Info.DeadLine = packet.ReadInt32();
            petition.Info.IssueDate = packet.ReadInt32();
            petition.Info.AllowedGuildID = packet.ReadInt32();
            petition.Info.AllowedClasses = packet.ReadInt32();
            petition.Info.AllowedRaces = packet.ReadInt32();
            petition.Info.AllowedGender = packet.ReadInt16();
            petition.Info.AllowedMinLevel = packet.ReadInt32();
            petition.Info.AllowedMaxLevel = packet.ReadInt32();
            petition.Info.NumChoices = packet.ReadInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                for (var i = 0; i < 10; i++)
                    petition.Info.Choicetext[i] = packet.ReadCString();

            petition.Info.Muid = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                petition.Info.StaticType = packet.ReadInt32();

            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.MSG_PETITION_RENAME)]
        void HandlePetitionRename(WorldPacket packet)
        {
            PetitionRenameGuildResponse petition = new()
            {
                PetitionGuid = packet.ReadGuid().To128(GetSession().GameState),
                NewGuildName = packet.ReadCString()
            };
            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.MSG_PETITION_DECLINE)]
        void HandlePetitionDecline(WorldPacket packet)
        {
            WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
            string name = GetSession().GameState.GetPlayerName(guid);
            if (!string.IsNullOrEmpty(name))
            {
                ChatPkt chat = new(GetSession(), ChatMessageTypeModern.System, $"{name} has declined your guild invitation.");
                SendPacketToClient(chat);
            }
        }

        [PacketHandler(Opcode.SMSG_PETITION_SIGN_RESULTS)]
        void HandlePetitionSignResults(WorldPacket packet)
        {
            PetitionSignResults petition = new()
            {
                Item = packet.ReadGuid().To128(GetSession().GameState),
                Player = packet.ReadGuid().To128(GetSession().GameState),
                Error = (PetitionSignResult)packet.ReadUInt32()
            };
            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.SMSG_TURN_IN_PETITION_RESULT)]
        void HandleTurnInPetitionResult(WorldPacket packet)
        {
            TurnInPetitionResult petition = new()
            {
                Result = (PetitionTurnResult)packet.ReadUInt32()
            };
            SendPacketToClient(petition);
        }
    }
}
