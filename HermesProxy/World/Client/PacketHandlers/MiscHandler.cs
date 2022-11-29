﻿using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_TUTORIAL_FLAGS)]
        void HandleTutorialFlags(WorldPacket packet)
        {
            TutorialFlags tutorials = new();
            for (byte i = 0; i < (byte)Tutorials.Max; ++i)
                tutorials.TutorialData[i] = packet.ReadUInt32();
            SendPacketToClient(tutorials);
        }

        [PacketHandler(Opcode.SMSG_ACCOUNT_DATA_TIMES)]
        void HandleAccountDataTimes(WorldPacket packet)
        {
            GetSession().RealmSocket.SendAccountDataTimes();

            // These packets don't exist in Vanilla and we must send them here.
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                GetSession().RealmSocket.SendFeatureSystemStatus();
                GetSession().RealmSocket.SendMotd();
                GetSession().RealmSocket.SendSetTimeZoneInformation();
                GetSession().RealmSocket.SendSeasonInfo();
            }
        }

        [PacketHandler(Opcode.SMSG_BIND_POINT_UPDATE)]
        void HandleBindPointUpdate(WorldPacket packet)
        {
            BindPointUpdate point = new()
            {
                BindPosition = packet.ReadVector3(),
                BindMapID = packet.ReadUInt32(),
                BindAreaID = packet.ReadUInt32()
            };
            SendPacketToClient(point);
        }

        [PacketHandler(Opcode.SMSG_PLAYER_BOUND)]
        void HandlePlayerBound(WorldPacket packet)
        {
            PlayerBound bound = new()
            {
                BinderGUID = packet.ReadGuid().To128(GetSession().GameState),
                AreaID = packet.ReadUInt32()
            };
            SendPacketToClient(bound);
        }

        [PacketHandler(Opcode.SMSG_DEATH_RELEASE_LOC)]
        void HandleDeathReleaseLoc(WorldPacket packet)
        {
            DeathReleaseLoc death = new()
            {
                MapID = packet.ReadInt32(),
                Location = packet.ReadVector3()
            };
            SendPacketToClient(death);
        }

        [PacketHandler(Opcode.SMSG_CORPSE_RECLAIM_DELAY)]
        void HandleCorpseReclaimDelay(WorldPacket packet)
        {
            CorpseReclaimDelay delay = new()
            {
                Remaining = packet.ReadUInt32()
            };
            SendPacketToClient(delay);
        }

        [PacketHandler(Opcode.SMSG_TIME_SYNC_REQUEST)]
        void HandleTimeSyncRequest(WorldPacket packet)
        {
            TimeSyncRequest sync = new()
            {
                SequenceIndex = packet.ReadUInt32()
            };
            SendPacketToClient(sync);
        }

        [PacketHandler(Opcode.SMSG_WEATHER)]
        void HandleWeather(WorldPacket packet)
        {
            WeatherPkt weather = new();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WeatherType type = (WeatherType)packet.ReadUInt32();
                weather.Intensity = packet.ReadFloat();
                weather.WeatherID = Weather.ConvertWeatherTypeToWeatherState(type, weather.Intensity);
                packet.ReadUInt32(); // sound
                if (packet.CanRead())
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
            if (!GetSession().GameState.IsFirstEnterWorld)
                return;

            LoginSetTimeSpeed login = new()
            {
                ServerTime = packet.ReadUInt32()
            };
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

            if (GetSession().GameState.LastEnteredAreaTrigger != 0)
            {
                AreaTriggerMessage denied = new()
                {
                    AreaTriggerID = GetSession().GameState.LastEnteredAreaTrigger
                };
                SendPacketToClient(denied);
            }
            else
            {
                ChatPkt chat = new(GetSession(), ChatMessageTypeModern.System, message);
                SendPacketToClient(chat);
            }
        }

        [PacketHandler(Opcode.MSG_CORPSE_QUERY)]
        void HandleCorpseQuery(WorldPacket packet)
        {
            CorpseLocation corpse = new()
            {
                Valid = packet.ReadBool()
            };
            if (!corpse.Valid)
            {
                {
                    ChatPkt chatA = new(GetSession(), ChatMessageTypeModern.System, $"----------------------------");
                    SendPacketToClient(chatA);
                    ChatPkt chatB = new(GetSession(), ChatMessageTypeModern.System, $"HermesProxy: Did you log out? If you see this message and you cant find your corpse/rezz please report this on GitHub: \nV!FALSE!:{corpse.Valid}");
                    SendPacketToClient(chatB);
                    ChatPkt chatC = new(GetSession(), ChatMessageTypeModern.System, $"----------------------------");
                    SendPacketToClient(chatC);
                }
                return;
            }

            corpse.MapID = packet.ReadInt32();
            corpse.Position = packet.ReadVector3();
            corpse.ActualMapID = packet.ReadInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
                packet.ReadInt32(); // Corpse Low GUID

            corpse.Player = GetSession().GameState.CurrentPlayerGuid;
            corpse.Transport = WowGuid128.Empty;

            {
                ChatPkt chatA = new(GetSession(), ChatMessageTypeModern.System, $"----------------------------");
                SendPacketToClient(chatA);
                ChatPkt chatB = new(GetSession(), ChatMessageTypeModern.System, $"HermesProxy: Did you log out? If you see this message and you cant find your corpse/rezz please report this on GitHub: \nV:{corpse.Valid}\nI:{corpse.Player == GetSession().GameState.CurrentPlayerGuid}\nC:{GetSession().GameState.CurrentPlayerGuid == GetSession().GameState.CurrentPlayerInfo.CharacterGuid}\nM:{corpse.MapID}/{corpse.ActualMapID}\nCP:{corpse.Position}\nPP:");
                SendPacketToClient(chatB);
                ChatPkt chatC = new(GetSession(), ChatMessageTypeModern.System, $"And just to verify is that your current character name?: {GetSession().GameState.CurrentPlayerInfo.Name}-{GetSession().GameState.CurrentPlayerInfo.Realm.Name}");
                SendPacketToClient(chatC);
                ChatPkt chatD = new(GetSession(), ChatMessageTypeModern.System, $"----------------------------");
                SendPacketToClient(chatD);
            }

            SendPacketToClient(corpse);
        }

        [PacketHandler(Opcode.SMSG_STAND_STATE_UPDATE)]
        void HandleStandStateUpdate(WorldPacket packet)
        {
            StandStateUpdate state = new()
            {
                StandState = packet.ReadUInt8()
            };
            SendPacketToClient(state);
        }

        [PacketHandler(Opcode.SMSG_EXPLORATION_EXPERIENCE)]
        void HandleExplorationExperience(WorldPacket packet)
        {
            ExplorationExperience explore = new()
            {
                AreaID = packet.ReadUInt32(),
                Experience = packet.ReadUInt32()
            };
            SendPacketToClient(explore);
        }

        [PacketHandler(Opcode.SMSG_PLAY_MUSIC)]
        void HandlePlayMusic(WorldPacket packet)
        {
            PlayMusic music = new()
            {
                SoundEntryID = packet.ReadUInt32()
            };
            SendPacketToClient(music);
        }

        [PacketHandler(Opcode.SMSG_PLAY_SOUND)]
        void HandlePlaySound(WorldPacket packet)
        {
            PlaySound sound = new()
            {
                SoundEntryID = packet.ReadUInt32(),
                SourceObjectGuid = GetSession().GameState.CurrentPlayerGuid
            };
            SendPacketToClient(sound);
        }

        [PacketHandler(Opcode.SMSG_PLAY_OBJECT_SOUND)]
        void HandlePlayObjectSound(WorldPacket packet)
        {
            PlayObjectSound sound = new()
            {
                SoundEntryID = packet.ReadUInt32(),
                SourceObjectGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            sound.TargetObjectGUID = sound.SourceObjectGUID;
            SendPacketToClient(sound);
        }

        [PacketHandler(Opcode.SMSG_TRIGGER_CINEMATIC)]
        void HandleTriggerCinematic(WorldPacket packet)
        {
            TriggerCinematic cinematic = new()
            {
                CinematicID = packet.ReadUInt32()
            };
            SendPacketToClient(cinematic);
        }

        [PacketHandler(Opcode.SMSG_SPECIAL_MOUNT_ANIM)]
        void HandleSpecialMountAnim(WorldPacket packet)
        {
            SpecialMountAnim mount = new()
            {
                UnitGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(mount);
        }

        [PacketHandler(Opcode.SMSG_START_MIRROR_TIMER)]
        void HandleStartMirrorTimer(WorldPacket packet)
        {
            StartMirrorTimer timer = new()
            {
                Timer = (MirrorTimerType)packet.ReadUInt32(),
                Value = packet.ReadInt32(),
                MaxValue = packet.ReadInt32(),
                Scale = packet.ReadInt32(),
                Paused = packet.ReadBool(),
                SpellID = packet.ReadInt32()
            };
            SendPacketToClient(timer);
        }

        [PacketHandler(Opcode.SMSG_PAUSE_MIRROR_TIMER)]
        void HandlePauseMirrorTimer(WorldPacket packet)
        {
            PauseMirrorTimer timer = new()
            {
                Timer = (MirrorTimerType)packet.ReadUInt32(),
                Paused = packet.ReadBool()
            };
            SendPacketToClient(timer);
        }

        [PacketHandler(Opcode.SMSG_STOP_MIRROR_TIMER)]
        void HandleStopMirrorTimer(WorldPacket packet)
        {
            StopMirrorTimer timer = new()
            {
                Timer = (MirrorTimerType)packet.ReadUInt32()
            };
            SendPacketToClient(timer);
        }

        [PacketHandler(Opcode.SMSG_INVALIDATE_PLAYER)]
        void HandleInvalidatePlayer(WorldPacket packet)
        {
            InvalidatePlayer invalidate = new()
            {
                Guid = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(invalidate);

            if (GetSession().GameState.CachedPlayers.ContainsKey(invalidate.Guid))
                GetSession().GameState.CachedPlayers.Remove(invalidate.Guid);
        }

        [PacketHandler(Opcode.SMSG_PONG)]
        void HandlePingResponse(WorldPacket packet)
        {
            uint serial = packet.ReadUInt32();
            SendPacketToClient(new Pong(serial));
        }
    }
}
