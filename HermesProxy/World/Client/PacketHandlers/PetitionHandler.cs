using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
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
            ServerPetitionShowList petitions = new();
            petitions.Unit = packet.ReadGuid().To128(GetSession().GameState);
            GetSession().GameState.CurrentInteractedWithNPC = petitions.Unit;
            var count = packet.ReadUInt8();
            for (var i = 0; i < count; i++)
            {
                PetitionEntry petition = new PetitionEntry();
                petition.Index = packet.ReadUInt32(); 
                petition.CharterEntry = packet.ReadUInt32();
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
            ServerPetitionShowSignatures petition = new();
            petition.Item = packet.ReadGuid().To128(GetSession().GameState);
            petition.Owner = packet.ReadGuid().To128(GetSession().GameState);
            petition.OwnerAccountID = GetSession().GetGameAccountGuidForPlayer(petition.Owner);
            petition.PetitionID = packet.ReadInt32();
            var counter = packet.ReadUInt8();
            for (var i = 0; i < counter; i++)
            {
                PetitionSignature signature = new PetitionSignature();
                signature.Signer = packet.ReadGuid().To128(GetSession().GameState);
                signature.Choice = packet.ReadInt32();
                petition.Signatures.Add(signature);
            }
            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.SMSG_QUERY_PETITION_RESPONSE)]
        void HandlePetitionQueryResponse(WorldPacket packet)
        {
            QueryPetitionResponse petition = new();
            petition.PetitionID = packet.ReadUInt32();
            petition.Allow = true;
            petition.Info = new PetitionInfo();
            petition.Info.PetitionID = petition.PetitionID;
            petition.Info.Petitioner = packet.ReadGuid().To128(GetSession().GameState);

            petition.Info.Title = packet.ReadCString();
            petition.Info.BodyText = packet.ReadCString();

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
            PetitionRenameGuildResponse petition = new();
            petition.PetitionGuid = packet.ReadGuid().To128(GetSession().GameState);
            petition.NewGuildName = packet.ReadCString();
            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.MSG_PETITION_DECLINE)]
        void HandlePetitionDecline(WorldPacket packet)
        {
            WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
            string name = GetSession().GameState.GetPlayerName(guid);
            if (!String.IsNullOrEmpty(name))
            {
                ChatPkt chat = new ChatPkt(GetSession(), ChatMessageTypeModern.System, 0, null, "", null, "", $"{name} has declined your guild invitation.", "", ChatFlags.None, 0);
                SendPacketToClient(chat);
            }
        }

        [PacketHandler(Opcode.SMSG_PETITION_SIGN_RESULTS)]
        void HandlePetitionSignResults(WorldPacket packet)
        {
            PetitionSignResults petition = new();
            petition.Item = packet.ReadGuid().To128(GetSession().GameState);
            petition.Player = packet.ReadGuid().To128(GetSession().GameState);
            petition.Error = (PetitionSignResult)packet.ReadUInt32();
            SendPacketToClient(petition);
        }

        [PacketHandler(Opcode.SMSG_TURN_IN_PETITION_RESULT)]
        void HandleTurnInPetitionResult(WorldPacket packet)
        {
            TurnInPetitionResult petition = new();
            petition.Result = (PetitionTurnResult)packet.ReadUInt32();
            SendPacketToClient(petition);
        }
    }
}
