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
            Global.CurrentSessionData.RealmSocket.SendAccountDataTimes();

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

        [PacketHandler(Opcode.SMSG_PLAYER_BOUND)]
        void HandlePlayerBound(WorldPacket packet)
        {
            PlayerBound bound = new PlayerBound();
            bound.BinderGUID = packet.ReadGuid().To128();
            bound.AreaID = packet.ReadUInt32();
            SendPacketToClient(bound);
        }

        [PacketHandler(Opcode.SMSG_CORPSE_RECLAIM_DELAY)]
        void HandleCorpseReclaimDelay(WorldPacket packet)
        {
            CorpseReclaimDelay delay = new CorpseReclaimDelay();
            delay.Remaining = packet.ReadUInt32();
            SendPacketToClient(delay);
        }

        [PacketHandler(Opcode.SMSG_TIME_SYNC_REQUEST)]
        void HandleTimeSyncRequest(WorldPacket packet)
        {
            TimeSyncRequest sync = new TimeSyncRequest();
            sync.SequenceIndex = packet.ReadUInt32();
            SendPacketToClient(sync);
        }

        [PacketHandler(Opcode.SMSG_WEATHER)]
        void HandleWeather(WorldPacket packet)
        {
            WeatherPkt weather = new WeatherPkt();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WeatherType type = (WeatherType)packet.ReadUInt32();
                weather.Intensity = packet.ReadFloat();
                weather.WeatherID = Weather.ConvertWeatherTypeToWeatherState(type, weather.Intensity);
                packet.ReadUInt32(); // sound
                weather.Abrupt = packet.ReadBool();
            }
            else
            {
                weather.WeatherID = (WeatherState)packet.ReadUInt32();
                weather.Intensity = packet.ReadFloat();
                weather.Abrupt = packet.ReadBool();
            }
            SendPacketToClient(weather);
            SendPacketToClient(new StartLightningStorm());
        }

        [PacketHandler(Opcode.SMSG_LOGIN_SET_TIME_SPEED)]
        void HandleLoginSetTimeSpeed(WorldPacket packet)
        {
            if (!Global.CurrentSessionData.GameState.IsFirstEnterWorld)
                return;

            LoginSetTimeSpeed login = new LoginSetTimeSpeed();
            login.ServerTime = packet.ReadUInt32();
            login.GameTime = login.ServerTime;
            login.NewSpeed = packet.ReadFloat();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
            {
                login.ServerTimeHolidayOffset = packet.ReadInt32();
                login.GameTimeHolidayOffset = login.ServerTimeHolidayOffset;
            }
            SendPacketToClient(login);
        }

        [PacketHandler(Opcode.SMSG_AREA_TRIGGER_MESSAGE)]
        void HandleAreaTriggerMessage(WorldPacket packet)
        {
            uint length = packet.ReadUInt32();
            string message = packet.ReadString(length);

            if (Global.CurrentSessionData.GameState.LastEnteredAreaTrigger != 0)
            {
                AreaTriggerMessage denied = new AreaTriggerMessage();
                denied.AreaTriggerID = Global.CurrentSessionData.GameState.LastEnteredAreaTrigger;
                SendPacketToClient(denied);
            }
            else
            {
                ChatPkt chat = new ChatPkt(ChatMessageTypeModern.System, 0, null, "", null, "", message, "", ChatFlags.None, 0);
                SendPacketToClient(chat);
            }
        }

        [PacketHandler(Opcode.MSG_CORPSE_QUERY)]
        void HandleCorpseQuery(WorldPacket packet)
        {
            CorpseLocation corpse = new();
            corpse.Valid = packet.ReadBool();
            if (!corpse.Valid)
                return;

            corpse.MapID = packet.ReadInt32();
            corpse.Position = packet.ReadVector3();
            corpse.ActualMapID = packet.ReadInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
                packet.ReadInt32(); // Corpse Low GUID

            corpse.Player = Global.CurrentSessionData.GameState.CurrentPlayerGuid;
            corpse.Transport = WowGuid128.Empty;
            SendPacketToClient(corpse);
        }

        [PacketHandler(Opcode.SMSG_STAND_STATE_UPDATE)]
        void HandleStandStateUpdate(WorldPacket packet)
        {
            StandStateUpdate state = new();
            state.StandState = packet.ReadUInt8();
            SendPacketToClient(state);
        }

        [PacketHandler(Opcode.SMSG_EXPLORATION_EXPERIENCE)]
        void HandleExplorationExperience(WorldPacket packet)
        {
            ExplorationExperience explore = new();
            explore.AreaID = packet.ReadUInt32();
            explore.Experience = packet.ReadUInt32();
            SendPacketToClient(explore);
        }

        [PacketHandler(Opcode.SMSG_PLAY_MUSIC)]
        void HandlePlayMusic(WorldPacket packet)
        {
            PlayMusic music = new();
            music.SoundEntryID = packet.ReadUInt32();
            SendPacketToClient(music);
        }

        [PacketHandler(Opcode.SMSG_PLAY_SOUND)]
        void HandlePlaySound(WorldPacket packet)
        {
            PlaySound sound = new();
            sound.SoundEntryID = packet.ReadUInt32();
            sound.SourceObjectGuid = Global.CurrentSessionData.GameState.CurrentPlayerGuid;
            SendPacketToClient(sound);
        }

        [PacketHandler(Opcode.SMSG_PLAY_OBJECT_SOUND)]
        void HandlePlayObjectSound(WorldPacket packet)
        {
            PlayObjectSound sound = new();
            sound.SoundEntryID = packet.ReadUInt32();
            sound.SourceObjectGUID = packet.ReadGuid().To128();
            sound.TargetObjectGUID = sound.SourceObjectGUID;
            SendPacketToClient(sound);
        }

        [PacketHandler(Opcode.SMSG_TRIGGER_CINEMATIC)]
        void HandleTriggerCinematic(WorldPacket packet)
        {
            TriggerCinematic cinematic = new();
            cinematic.CinematicID = packet.ReadUInt32();
            SendPacketToClient(cinematic);
        }
    }
}
