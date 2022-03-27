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
        [PacketHandler(Opcode.CMSG_PET_ACTION)]
        void HandlePetAction(PetAction act)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ACTION);
            packet.WriteGuid(act.PetGUID.To64());
            packet.WriteUInt32(act.Action);
            packet.WriteGuid(act.TargetGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PET_STOP_ATTACK)]
        void HandlePetStopAttack(PetStopAttack stop)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_STOP_ATTACK);
            packet.WriteGuid(stop.PetGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PET_SET_ACTION)]
        void HandlePetStopAttack(PetSetAction action)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_SET_ACTION);
            packet.WriteGuid(action.PetGUID.To64());
            packet.WriteUInt32(action.Index);
            packet.WriteUInt32(action.Action);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PET_RENAME)]
        void HandlePetRename(PetRename pet)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_RENAME);
            packet.WriteGuid(pet.RenameData.PetGUID.To64());
            packet.WriteCString(pet.RenameData.NewName);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteBool(pet.RenameData.HasDeclinedNames);
                if (pet.RenameData.HasDeclinedNames)
                {
                    for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                        packet.WriteCString(pet.RenameData.DeclinedNames.name[i]);
                }
            }
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_REQUEST_STABLED_PETS)]
        void HandleRequestStabledPets(RequestStabledPets stable)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_LIST_STABLED_PETS);
            packet.WriteGuid(stable.StableMaster.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_BUY_STABLE_SLOT)]
        void HandleBuyStableSlot(BuyStableSlot stable)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_BUY_STABLE_SLOT);
            packet.WriteGuid(stable.StableMaster.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PET_ABANDON)]
        void HandlePetAbandon(PetAbandon pet)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ABANDON);
            packet.WriteGuid(pet.PetGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_STABLE_PET)]
        void HandleStablePet(StablePet pet)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_STABLE_PET);
            packet.WriteGuid(pet.StableMaster.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_UNSTABLE_PET)]
        void HandleUnstablePet(UnstablePet pet)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_UNSTABLE_PET);
            packet.WriteGuid(pet.StableMaster.To64());
            packet.WriteUInt32(pet.PetNumber);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_STABLE_SWAP_PET)]
        void HandleStableSwapPet(StableSwapPet pet)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_STABLE_SWAP_PET);
            packet.WriteGuid(pet.StableMaster.To64());
            packet.WriteUInt32(pet.PetNumber);
            SendPacketToServer(packet);
        }
    }
}
