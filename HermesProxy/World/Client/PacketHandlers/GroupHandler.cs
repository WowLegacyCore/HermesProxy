using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.MSG_RAID_TARGET_UPDATE)]
        void HandleRaidTargetUpdate(WorldPacket packet)
        {
            bool isFullUpdate = packet.ReadBool();
            if (isFullUpdate)
            {
                SendRaidTargetUpdateAll update = new SendRaidTargetUpdateAll();
                while (packet.CanRead())
                {
                    sbyte symbol = packet.ReadInt8();
                    WowGuid128 guid = packet.ReadGuid().To128();
                    update.TargetIcons.Add(new Tuple<sbyte, WowGuid128>(symbol, guid));
                }
                SendPacketToClient(update);
            }
            else
            {
                SendRaidTargetUpdateSingle update = new SendRaidTargetUpdateSingle();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    update.ChangedBy = packet.ReadGuid().To128();
                else
                    update.ChangedBy = GetSession().GameState.CurrentPlayerGuid;
                
                update.Symbol = packet.ReadInt8();
                update.Target = packet.ReadGuid().To128();
                SendPacketToClient(update);
            }
        }
    }
}
