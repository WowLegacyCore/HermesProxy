using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_TIME_SYNC_RESPONSE)]
        void HandleTimeSyncResponse(TimeSyncResponse response)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_TIME_SYNC_RESPONSE);
                packet.WriteUInt32(response.SequenceIndex);
                packet.WriteUInt32(response.ClientTime);
                SendPacketToServer(packet);
            }
        }

        [PacketHandler(Opcode.CMSG_AREA_TRIGGER)]
        void HandleAreaTrigger(AreaTriggerPkt at)
        {
            if (at.Entered == false)
                return;

            GetSession().GameState.LastEnteredAreaTrigger = at.AreaTriggerID;
            WorldPacket packet = new WorldPacket(Opcode.CMSG_AREA_TRIGGER);
            packet.WriteUInt32(at.AreaTriggerID);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_SELECTION)]
        void HandleSetSelection(SetSelection selection)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_SELECTION);
            packet.WriteGuid(selection.TargetGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_REPOP_REQUEST)]
        void HandleRepopRequest(RepopRequest repop)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_REPOP_REQUEST);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteBool(repop.CheckInstance);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT)]
        void HandleQueryCorpseLocationFromClient(QueryCorpseLocationFromClient query)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_CORPSE_QUERY);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_RECLAIM_CORPSE)]
        void HandleReclaimCorpse(ReclaimCorpse corpse)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_RECLAIM_CORPSE);
            packet.WriteGuid(corpse.CorpseGUID.To64());
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_STAND_STATE_CHANGE)]
        void HandleStandStateChange(StandStateChange state)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_STAND_STATE_CHANGE);
            packet.WriteUInt32(state.StandState);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_OPENING_CINEMATIC)]
        [PacketHandler(Opcode.CMSG_NEXT_CINEMATIC_CAMERA)]
        [PacketHandler(Opcode.CMSG_COMPLETE_CINEMATIC)]
        void HandleCinematicPacket(ClientCinematicPkt cinematic)
        {
            WorldPacket packet = new WorldPacket(cinematic.GetUniversalOpcode());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_FAR_SIGHT)]
        void HandleFarSight(FarSight sight)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_FAR_SIGHT);
            packet.WriteBool(sight.Enable);
            SendPacketToServer(packet);
            GetSession().GameState.IsInFarSight = sight.Enable;
        }

        [PacketHandler(Opcode.CMSG_MOUNT_SPECIAL_ANIM)]
        void HandleMountSpecialAnim(MountSpecial mount)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MOUNT_SPECIAL_ANIM);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_TUTORIAL_FLAG)]
        void HandleTutorialFlag(TutorialSetFlag tutorial)
        {
            switch (tutorial.Action)
            {
                case TutorialAction.Clear:
                {
                    WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_CLEAR);
                    SendPacketToServer(packet);
                    break;
                }
                case TutorialAction.Reset:
                {
                    WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_RESET);
                    SendPacketToServer(packet);
                    break;
                }
                case TutorialAction.Update:
                {
                    WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_FLAG);
                    packet.WriteUInt32(tutorial.TutorialBit);
                    SendPacketToServer(packet);
                    break;
                }
            }
        }

        [PacketHandler(Opcode.CMSG_REQUEST_LFG_LIST_BLACKLIST)]
        void HandleRequestLFGListBlacklist(EmptyClientPacket request)
        {
            LFGListUpdateBlacklist blacklist = new LFGListUpdateBlacklist();
            if (ModernVersion.ExpansionVersion > 1)
            {
                blacklist.AddBlacklist(796, 3);
                blacklist.AddBlacklist(797, 3);
                blacklist.AddBlacklist(798, 3);
                blacklist.AddBlacklist(799, 3);
                blacklist.AddBlacklist(800, 3);
                blacklist.AddBlacklist(801, 3);
                blacklist.AddBlacklist(802, 3);
                blacklist.AddBlacklist(803, 3);
                blacklist.AddBlacklist(804, 3);
                blacklist.AddBlacklist(805, 3);
                blacklist.AddBlacklist(806, 3);
                blacklist.AddBlacklist(807, 3);
                blacklist.AddBlacklist(808, 3);
                blacklist.AddBlacklist(809, 3);
                blacklist.AddBlacklist(810, 3);
                blacklist.AddBlacklist(811, 3);
                blacklist.AddBlacklist(812, 3);
                blacklist.AddBlacklist(813, 3);
                blacklist.AddBlacklist(814, 3);
                blacklist.AddBlacklist(815, 3);
                blacklist.AddBlacklist(816, 3);
                blacklist.AddBlacklist(817, 3);
                blacklist.AddBlacklist(818, 3);
                blacklist.AddBlacklist(820, 3);
                blacklist.AddBlacklist(827, 3);
                blacklist.AddBlacklist(828, 3);
                blacklist.AddBlacklist(829, 3);
                blacklist.AddBlacklist(835, 1031);
                blacklist.AddBlacklist(837, 3);
                blacklist.AddBlacklist(849, 1031);
                blacklist.AddBlacklist(850, 1031);
                blacklist.AddBlacklist(851, 1031);
                blacklist.AddBlacklist(852, 1031);
                blacklist.AddBlacklist(853, 3);
                blacklist.AddBlacklist(854, 3);
                blacklist.AddBlacklist(855, 3);
                blacklist.AddBlacklist(856, 3);
                blacklist.AddBlacklist(857, 3);
                blacklist.AddBlacklist(858, 3);
                blacklist.AddBlacklist(859, 3);
                blacklist.AddBlacklist(860, 3);
                blacklist.AddBlacklist(861, 3);
                blacklist.AddBlacklist(862, 3);
                blacklist.AddBlacklist(863, 3);
                blacklist.AddBlacklist(864, 3);
                blacklist.AddBlacklist(865, 3);
                blacklist.AddBlacklist(866, 3);
                blacklist.AddBlacklist(867, 3);
                blacklist.AddBlacklist(868, 3);
                blacklist.AddBlacklist(869, 3);
                blacklist.AddBlacklist(870, 3);
                blacklist.AddBlacklist(871, 3);
                blacklist.AddBlacklist(872, 3);
                blacklist.AddBlacklist(873, 3);
                blacklist.AddBlacklist(874, 3);
                blacklist.AddBlacklist(875, 3);
                blacklist.AddBlacklist(876, 3);
                blacklist.AddBlacklist(877, 3);
                blacklist.AddBlacklist(878, 3);
                blacklist.AddBlacklist(879, 3);
                blacklist.AddBlacklist(880, 3);
                blacklist.AddBlacklist(881, 3);
                blacklist.AddBlacklist(882, 3);
                blacklist.AddBlacklist(883, 3);
                blacklist.AddBlacklist(884, 3);
                blacklist.AddBlacklist(885, 3);
                blacklist.AddBlacklist(886, 3);
                blacklist.AddBlacklist(887, 3);
                blacklist.AddBlacklist(888, 3);
                blacklist.AddBlacklist(889, 3);
                blacklist.AddBlacklist(890, 3);
                blacklist.AddBlacklist(891, 3);
                blacklist.AddBlacklist(892, 3);
                blacklist.AddBlacklist(893, 3);
                blacklist.AddBlacklist(898, 3);
                blacklist.AddBlacklist(899, 3);
                blacklist.AddBlacklist(900, 3);
                blacklist.AddBlacklist(901, 3);
                blacklist.AddBlacklist(902, 1031);
                blacklist.AddBlacklist(917, 1031);
                blacklist.AddBlacklist(919, 3);
                blacklist.AddBlacklist(920, 3);
                blacklist.AddBlacklist(921, 3);
                blacklist.AddBlacklist(922, 3);
                blacklist.AddBlacklist(923, 3);
                blacklist.AddBlacklist(924, 3);
                blacklist.AddBlacklist(926, 3);
                blacklist.AddBlacklist(927, 3);
                blacklist.AddBlacklist(928, 3);
                blacklist.AddBlacklist(929, 3);
                blacklist.AddBlacklist(930, 3);
                blacklist.AddBlacklist(932, 3);
                blacklist.AddBlacklist(934, 3);
            }
            SendPacket(blacklist);
        }

        [PacketHandler(Opcode.CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS)]
        void HandleRequestConquestFormulaConstants(EmptyClientPacket request)
        {
            ConquestFormulaConstants response = new ConquestFormulaConstants();
            response.PvpMinCPPerWeek = 1500;
            response.PvpMaxCPPerWeek = 3000;
            response.PvpCPBaseCoefficient = 1511.26f;
            response.PvpCPExpCoefficient = 1639.28f;
            response.PvpCPNumerator = 0.00412f;
            SendPacket(response);
        }

        [PacketHandler(Opcode.CMSG_OBJECT_UPDATE_FAILED)]
        void HandleObjectUpdateFailed(ObjectUpdateFailed fail)
        {
            Log.Print(LogType.Error, $"Object update failed for {fail.ObjectGuid}.");
        }
    }
}
