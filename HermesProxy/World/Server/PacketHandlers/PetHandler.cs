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
    }
}
