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
        [PacketHandler(Opcode.SMSG_TUTORIAL_FLAGS)]
        void HandleTutorialFlags(WorldPacket packet)
        {
            TutorialFlags tutorials = new TutorialFlags();
            for (byte i = 0; i < (byte)Tutorials.Max; ++i)
                tutorials.TutorialData[i] = packet.ReadUInt32();
            SendPacketToClient(tutorials);
        }

        [PacketHandler(Opcode.SMSG_ACCOUNT_DATA_TIMES)]
        void HandleAccountDataTimes(WorldPacket packet)
        {
            AccountDataTimes accountData = new AccountDataTimes();
            accountData.PlayerGuid = Global.CurrentSessionData.GameData.CurrentPlayerGuid;
            accountData.ServerTime = Time.UnixTime;

            int count = (Settings.GetClientExpansionVersion() == 1) ? 10 : 8;
            accountData.AccountTimes = new long[count];
            for (int i = 0; i < count; i++)
                accountData.AccountTimes[i] = 0;

            SendPacketToClient(accountData);

            // These packets don't exist in Vanilla and we must send them here.
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                Global.CurrentSessionData.RealmSocket.SendFeatureSystemStatus();
                Global.CurrentSessionData.RealmSocket.SendMotd();
                Global.CurrentSessionData.RealmSocket.SendSetTimeZoneInformation();
                Global.CurrentSessionData.RealmSocket.SendSeasonInfo();
            }
        }

        [PacketHandler(Opcode.SMSG_BIND_POINT_UPDATE)]
        void HandleBindPointUpdate(WorldPacket packet)
        {
            BindPointUpdate point = new BindPointUpdate();
            point.BindPosition = packet.ReadVector3();
            point.BindMapID = packet.ReadUInt32();
            point.BindAreaID = packet.ReadUInt32();
            SendPacketToClient(point);
        }

        [PacketHandler(Opcode.SMSG_CORPSE_RECLAIM_DELAY)]
        void HandleCorpseReclaimDelay(WorldPacket packet)
        {
            CorpseReclaimDelay delay = new CorpseReclaimDelay();
            delay.Remaining = packet.ReadUInt32();
            SendPacketToClient(delay);
        }
    }
}
