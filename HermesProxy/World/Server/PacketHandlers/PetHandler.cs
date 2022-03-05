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
        [PacketHandler(Opcode.CMSG_PET_ABANDON)]
        void HandlePetAbandon(PetAbandon pet)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_ABANDON);
            packet.WriteGuid(pet.PetGUID.To64());
            SendPacketToServer(packet);
        }
    }
}
