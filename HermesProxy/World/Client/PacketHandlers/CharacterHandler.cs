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
        [PacketHandler(Opcode.SMSG_ENUM_CHARACTERS_RESULT)]
        void HandleEnumCharactersResult(WorldPacket packet)
        {
            EnumCharactersResult charEnum = new();
            charEnum.Success = true;
            charEnum.IsDeletedCharacters = false;
            charEnum.IsNewPlayerRestrictionSkipped = false;
            charEnum.IsNewPlayerRestricted = false;
            charEnum.IsNewPlayer = true;
            charEnum.IsAlliedRacesCreationAllowed = false;

            byte count = packet.ReadUInt8();
            for (byte i = 0; i < count; i++)
            {
                EnumCharactersResult.CharacterInfo char1 = new EnumCharactersResult.CharacterInfo();
                PlayerCache cache = new PlayerCache();
                char1.Guid = packet.ReadGuid().To128(GetSession().GameState);
                GetSession().GameState.OwnCharacters.Add(char1.Guid);
                char1.Name = cache.Name = packet.ReadCString();
                char1.RaceId = cache.RaceId = (Race)packet.ReadUInt8();
                char1.ClassId = cache.ClassId = (Class)packet.ReadUInt8();
                char1.SexId = cache.SexId = (Gender)packet.ReadUInt8();

                byte skin = packet.ReadUInt8();
                byte face = packet.ReadUInt8();
                byte hairStyle = packet.ReadUInt8();
                byte hairColor = packet.ReadUInt8();
                byte facialHair = packet.ReadUInt8();
                char1.Customizations = CharacterCustomizations.ConvertLegacyCustomizationsToModern((Race)char1.RaceId, (Gender)char1.SexId, skin, face, hairStyle, hairColor, facialHair);

                char1.ExperienceLevel = cache.Level = packet.ReadUInt8();
                if (char1.ExperienceLevel > charEnum.MaxCharacterLevel)
                    charEnum.MaxCharacterLevel = char1.ExperienceLevel;

                GetSession().GameState.UpdatePlayerCache(char1.Guid, cache);

                char1.ZoneId = packet.ReadUInt32();
                char1.MapId = packet.ReadUInt32();
                char1.PreloadPos = packet.ReadVector3();
                uint guildId = packet.ReadUInt32();
                GetSession().GameState.StorePlayerGuildId(char1.Guid, guildId);
                char1.GuildGuid = WowGuid128.Create(HighGuidType703.Guild, guildId);
                char1.Flags = (CharacterFlags)packet.ReadUInt32();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                    char1.Flags2 = packet.ReadUInt32(); // Customization Flags

                char1.FirstLogin = packet.ReadUInt8() != 0;
                char1.PetCreatureDisplayId = packet.ReadUInt32();
                char1.PetExperienceLevel = packet.ReadUInt32();
                char1.PetCreatureFamilyId = packet.ReadUInt32();

                for (int j = EquipmentSlot.Start; j < EquipmentSlot.End; j++)
                {
                    char1.VisualItems[j].DisplayId = packet.ReadUInt32();
                    char1.VisualItems[j].InvType = packet.ReadUInt8();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        char1.VisualItems[j].DisplayEnchantId = packet.ReadUInt32();
                }

                int bagCount = LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685) ? 4 : 1;
                for (int j = 0; j < bagCount; j++)
                {
                    char1.VisualItems[EquipmentSlot.Bag1 + j].DisplayId = packet.ReadUInt32();
                    char1.VisualItems[EquipmentSlot.Bag1 + j].InvType = packet.ReadUInt8();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        char1.VisualItems[EquipmentSlot.Bag1 + j].DisplayEnchantId = packet.ReadUInt32();
                }

                // placeholders
                char1.Flags2 = 402685956;
                char1.Flags3 = 855688192;
                char1.Flags4 = 0;
                char1.ProfessionIds[0] = 0;
                char1.ProfessionIds[1] = 0;
                char1.LastPlayedTime = Time.UnixTime;
                char1.SpecID = 0;
                char1.Unknown703 = 55;
                char1.LastLoginVersion = 11400;
                char1.OverrideSelectScreenFileDataID = 0;
                char1.BoostInProgress = false;
                char1.unkWod61x = 0;
                char1.ExpansionChosen = true;
                charEnum.Characters.Add(char1);
            }

            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(1, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(2, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(3, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(4, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(5, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(6, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(7, true, false, false));
            charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(8, true, false, false));
            if (ModernVersion.GetExpansionVersion() >= 2 &&
                LegacyVersion.GetExpansionVersion() >= 2)
            {
                charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(10, true, false, false));
                charEnum.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(11, true, false, false));
            }
            SendPacketToClient(charEnum);
        }

        [PacketHandler(Opcode.SMSG_CREATE_CHAR)]
        void HandleCreateChar(WorldPacket packet)
        {
            byte result = packet.ReadUInt8();

            CreateChar createChar = new CreateChar();
            createChar.Guid = new WowGuid128();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                Enums.TBC.ResponseCodes legacyCode = (Enums.TBC.ResponseCodes)result;
                createChar.Code = (Enums.Classic.ResponseCodes)Enum.Parse(typeof(Enums.Classic.ResponseCodes), legacyCode.ToString());
            }
            else
            {
                Enums.Vanilla.ResponseCodes legacyCode = (Enums.Vanilla.ResponseCodes)result;
                createChar.Code = (Enums.Classic.ResponseCodes)Enum.Parse(typeof(Enums.Classic.ResponseCodes), legacyCode.ToString());
            }
            SendPacketToClient(createChar);
        }

        [PacketHandler(Opcode.SMSG_DELETE_CHAR)]
        void HandleDeleteChar(WorldPacket packet)
        {
            byte result = packet.ReadUInt8();

            DeleteChar deleteChar = new DeleteChar();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                Enums.TBC.ResponseCodes legacyCode = (Enums.TBC.ResponseCodes)result;
                deleteChar.Code = (Enums.Classic.ResponseCodes)Enum.Parse(typeof(Enums.Classic.ResponseCodes), legacyCode.ToString());
            }
            else
            {
                Enums.Vanilla.ResponseCodes legacyCode = (Enums.Vanilla.ResponseCodes)result;
                deleteChar.Code = (Enums.Classic.ResponseCodes)Enum.Parse(typeof(Enums.Classic.ResponseCodes), legacyCode.ToString());
            }
            SendPacketToClient(deleteChar);
        }

        [PacketHandler(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)]
        void HandleQueryPlayerNameResponse(WorldPacket packet)
        {
            QueryPlayerNameResponse response = new QueryPlayerNameResponse();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                response.Player = response.Data.GuidActual = packet.ReadPackedGuid().To128(GetSession().GameState);
                var fail = packet.ReadBool();
                if (fail)
                {
                    response.Result = Enums.Classic.ResponseCodes.Failure;
                    SendPacketToClient(response);
                    return;
                }
            }
            else
                response.Player = response.Data.GuidActual = packet.ReadGuid().To128(GetSession().GameState);

            PlayerCache cache = new PlayerCache();
            response.Data.Name = cache.Name = packet.ReadCString();
            packet.ReadCString(); // realm name

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                response.Data.RaceID = cache.RaceId = (Race)packet.ReadUInt8();
                response.Data.Sex = cache.SexId = (Gender)packet.ReadUInt8();
                response.Data.ClassID = cache.ClassId =(Class)packet.ReadUInt8();
            }
            else
            {
                response.Data.RaceID = cache.RaceId = (Race)packet.ReadUInt32();
                response.Data.Sex = cache.SexId = (Gender)packet.ReadUInt32();
                response.Data.ClassID = cache.ClassId = (Class)packet.ReadInt32();
            }

            if (GetSession().GameState.CachedPlayers.ContainsKey(response.Player))
                response.Data.Level = GetSession().GameState.CachedPlayers[response.Player].Level;
            if (response.Data.Level == 0)
                response.Data.Level = 1;

            GetSession().GameState.UpdatePlayerCache(response.Player, cache);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                if (packet.ReadBool())
                {
                    for (var i = 0; i < 5; i++)
                        response.Data.DeclinedNames.name[i] = packet.ReadCString();
                }
            }

            response.Data.IsDeleted = false;
            response.Data.AccountID = GetSession().GetGameAccountGuidForPlayer(response.Player);
            response.Data.BnetAccountID = GetSession().GetBnetAccountGuidForPlayer(response.Player);
            response.Data.VirtualRealmAddress = GetSession().RealmId.GetAddress();
            SendPacketToClient(response);
        }

        [PacketHandler(Opcode.SMSG_LOGIN_VERIFY_WORLD)]
        void HandleLoginVerifyWorld(WorldPacket packet)
        {
            LoginVerifyWorld verify = new LoginVerifyWorld();
            verify.MapID = packet.ReadUInt32();
            GetSession().GameState.CurrentMapId = verify.MapID;
            verify.Pos.X = packet.ReadFloat();
            verify.Pos.Y = packet.ReadFloat();
            verify.Pos.Z = packet.ReadFloat();
            verify.Pos.Orientation = packet.ReadFloat();
            SendPacketToClient(verify);

            GetSession().GameState.IsInWorld = true;

            WorldServerInfo info = new();
            if (verify.MapID > 1)
            {
                info.DifficultyID = 1;
                info.InstanceGroupSize = 5;
            }
            SendPacketToClient(info);

            SetAllTaskProgress tasks = new();
            SendPacketToClient(tasks);

            InitialSetup setup = new();
            setup.ServerExpansionLevel = (byte)(LegacyVersion.GetExpansionVersion() - 1);
            SendPacketToClient(setup);

            LoadCUFProfiles cuf = new();
            cuf.Data = GetSession().AccountDataMgr.LoadCUFProfiles();
            SendPacketToClient(cuf);
        }

        [PacketHandler(Opcode.SMSG_CHARACTER_LOGIN_FAILED)]
        void HandleCharacterLoginFailed(WorldPacket packet)
        {
            CharacterLoginFailed failed = new CharacterLoginFailed();
            failed.Code = (Framework.Constants.LoginFailureReason)packet.ReadUInt8();
            SendPacketToClient(failed);

            GetSession().GameState.IsInWorld = false;
        }

        [PacketHandler(Opcode.SMSG_UPDATE_ACTION_BUTTONS)]
        void HandleUpdateActionButtons(WorldPacket packet)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                byte type = packet.ReadUInt8();
                if (type == 2)
                    return;
            }

            List<int> buttons = new List<int>();

            int buttonCount = 120;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                buttonCount = 144;
            else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                buttonCount = 132;

            for (int i = 0; i < buttonCount; i++)
            {
                int packed = packet.ReadInt32();
                buttons.Add(packed);
            }

            while (buttons.Count < 132)
                buttons.Add(0);

            GetSession().GameState.ActionButtons = buttons;
        }

        [PacketHandler(Opcode.SMSG_LOGOUT_RESPONSE)]
        void HandleLogoutResponse(WorldPacket packet)
        {
            LogoutResponse logout = new LogoutResponse();
            logout.LogoutResult = packet.ReadInt32();
            logout.Instant = packet.ReadBool();
            SendPacketToClient(logout);
        }

        [PacketHandler(Opcode.SMSG_LOGOUT_COMPLETE)]
        void HandleLogoutComplete(WorldPacket packet)
        {
            LogoutComplete logout = new LogoutComplete();
            SendPacketToClient(logout);

            GetSession().GameState = new();
            GetSession().InstanceSocket.CloseSocket();
            GetSession().InstanceSocket = null;
        }

        [PacketHandler(Opcode.SMSG_LOGOUT_CANCEL_ACK)]
        void HandleLogoutCancelAck(WorldPacket packet)
        {
            LogoutCancelAck logout = new LogoutCancelAck();
            SendPacketToClient(logout);
        }

        [PacketHandler(Opcode.SMSG_LOG_XP_GAIN)]
        void HandleLogXPGain(WorldPacket packet)
        {
            LogXPGain log = new();
            log.Victim = packet.ReadGuid().To128(GetSession().GameState);
            log.Original = packet.ReadInt32();
            log.Reason = (PlayerLogXPReason)packet.ReadUInt8();
            if (log.Reason == PlayerLogXPReason.Kill)
            {
                log.Amount = packet.ReadInt32();
                log.GroupBonus = packet.ReadFloat();
            }
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) && packet.CanRead())
                log.RAFBonus = packet.ReadUInt8();
            SendPacketToClient(log);
        }

        [PacketHandler(Opcode.SMSG_PLAYED_TIME)]
        void HandlePlayedTime(WorldPacket packet)
        {
            PlayedTime played = new();
            played.TotalTime = packet.ReadUInt32();
            played.LevelTime = packet.ReadUInt32();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                played.TriggerEvent = packet.ReadBool();
            else
                played.TriggerEvent = GetSession().GameState.ShowPlayedTime;
            SendPacketToClient(played);
        }

        [PacketHandler(Opcode.SMSG_LEVEL_UP_INFO)]
        void HandleLevelUpInfo(WorldPacket packet)
        {
            LevelUpInfo info = new LevelUpInfo();
            info.Level = packet.ReadInt32();
            info.HealthDelta = packet.ReadInt32();

            for (var i = 0; i < LegacyVersion.GetPowersCount(); i++)
                info.PowerDelta[i] = packet.ReadInt32();

            for (var i = 0; i < 5; i++)
                info.StatDelta[i] = packet.ReadInt32();

            SendPacketToClient(info);
        }

        [PacketHandler(Opcode.SMSG_UPDATE_COMBO_POINTS)]
        void HandleUpdateComboPoints(WorldPacket packet)
        {
            ObjectUpdate updateData = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
            updateData.ActivePlayerData.ComboTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
            byte comboPoints = packet.ReadUInt8();
            sbyte powerSlot = ClassPowerTypes.GetPowerSlotForClass(GetSession().GameState.GetUnitClass(GetSession().GameState.CurrentPlayerGuid), PowerType.ComboPoints);
            if (powerSlot >= 0)
                updateData.UnitData.Power[powerSlot] = comboPoints;

            UpdateObject updatePacket = new UpdateObject(GetSession().GameState);
            updatePacket.ObjectUpdates.Add(updateData);
            SendPacketToClient(updatePacket);
        }

        [PacketHandler(Opcode.SMSG_INSPECT_RESULT)]
        [PacketHandler(Opcode.SMSG_INSPECT_TALENT)]
        void HandleInspectResult(WorldPacket packet)
        {
            InspectResult inspect = new InspectResult();
            if (packet.GetUniversalOpcode(false) == Opcode.SMSG_INSPECT_RESULT)
                inspect.DisplayInfo.GUID = packet.ReadGuid().To128(GetSession().GameState);
            else
                inspect.DisplayInfo.GUID = packet.ReadPackedGuid().To128(GetSession().GameState);

            PlayerCache cache;
            if (!GetSession().GameState.CachedPlayers.TryGetValue(inspect.DisplayInfo.GUID, out cache))
                return;

            inspect.DisplayInfo.Name = cache.Name;
            inspect.DisplayInfo.ClassId = cache.ClassId;
            inspect.DisplayInfo.RaceId = cache.RaceId;
            inspect.DisplayInfo.SexId = cache.SexId;

            var updates = GetSession().GameState.GetCachedObjectFieldsLegacy(inspect.DisplayInfo.GUID);
            if (updates != null)
            {
                int PLAYER_VISIBLE_ITEM_1_0 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_0);
                if (PLAYER_VISIBLE_ITEM_1_0 >= 0) // vanilla and tbc
                {
                    byte offset = (byte)(LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 16 : 12);
                    for (byte i = 0; i < 19; i++)
                    {
                        if (updates.ContainsKey(PLAYER_VISIBLE_ITEM_1_0 + i * offset))
                        {
                            uint itemId = updates[PLAYER_VISIBLE_ITEM_1_0 + i * offset].UInt32Value;
                            if (itemId != 0)
                            {
                                InspectItemData itemData = new InspectItemData();
                                itemData.Index = i;
                                itemData.Item.ItemID = itemId;
                                inspect.DisplayInfo.Items.Add(itemData);
                            }
                        }
                    }
                }
                int PLAYER_VISIBLE_ITEM_1_ENTRYID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_ENTRYID);
                if (PLAYER_VISIBLE_ITEM_1_ENTRYID >= 0) // wotlk
                {
                    int offset = 2;
                    for (byte i = 0; i < 19; i++)
                    {
                        if (updates.ContainsKey(PLAYER_VISIBLE_ITEM_1_ENTRYID + i * offset))
                        {
                            uint itemId = updates[PLAYER_VISIBLE_ITEM_1_ENTRYID + i * offset].UInt32Value;
                            if (itemId != 0)
                            {
                                InspectItemData itemData = new InspectItemData();
                                itemData.Index = i;
                                itemData.Item.ItemID = itemId;
                                inspect.DisplayInfo.Items.Add(itemData);
                            }
                        }
                    }
                }
                int PLAYER_GUILDID = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
                if (PLAYER_GUILDID >= 0 && updates.ContainsKey(PLAYER_GUILDID))
                {
                    inspect.GuildData = new InspectGuildData();
                    inspect.GuildData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, updates[PLAYER_GUILDID].UInt32Value);
                }
                int PLAYER_FIELD_BYTES = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES);
                if (PLAYER_FIELD_BYTES >= 0 && updates.ContainsKey(PLAYER_FIELD_BYTES))
                {
                    inspect.LifetimeMaxRank = (byte)((updates[PLAYER_FIELD_BYTES].UInt32Value >> 24) & 0xFF);
                }
            }

            // TODO: format seems to be different in new client
            if (packet.GetUniversalOpcode(false) == Opcode.SMSG_INSPECT_TALENT)
            {
                uint talentsCount = packet.ReadUInt32();
                for (uint i = 0; i < talentsCount; i++)
                {
                    byte talent = packet.ReadUInt8();
                    inspect.Talents.Add(talent);
                }
            }

            SendPacketToClient(inspect);
        }

        [PacketHandler(Opcode.MSG_INSPECT_HONOR_STATS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleInspectHonorStatsVanilla(WorldPacket packet)
        {
            WowGuid128 playerGuid = packet.ReadGuid().To128(GetSession().GameState);
            byte lifetimeHighestRank = packet.ReadUInt8();
            ushort todayHonorableKills = packet.ReadUInt16();
            ushort todayDishonorableKills = packet.ReadUInt16();
            ushort yesterdayHonorableKills = packet.ReadUInt16();
            ushort yesterdayDishonorableKills = packet.ReadUInt16();
            ushort lastWeekHonorableKills = packet.ReadUInt16();
            ushort lastWeekDishonorableKills = packet.ReadUInt16();
            ushort thisWeekHonorableKills = packet.ReadUInt16();
            ushort thisWeekDishonorableKills = packet.ReadUInt16();
            uint lifetimeHonorableKills = packet.ReadUInt32();
            uint lifetimeDishonorableKills = packet.ReadUInt32();
            uint yesterdayHonor = packet.ReadUInt32();
            uint lastWeekHonor = packet.ReadUInt32();
            uint thisWeekHonor = packet.ReadUInt32();
            uint standing = packet.ReadUInt32();
            byte rankProgress = packet.ReadUInt8();

            if (ModernVersion.GetExpansionVersion() == 1)
            {
                InspectHonorStatsResultClassic inspect = new InspectHonorStatsResultClassic();
                inspect.PlayerGUID = playerGuid;
                inspect.LifetimeHighestRank = lifetimeHighestRank;
                inspect.TodayHonorableKills = todayHonorableKills;
                inspect.TodayDishonorableKills = todayDishonorableKills;
                inspect.YesterdayHonorableKills = yesterdayHonorableKills;
                inspect.YesterdayDishonorableKills = yesterdayDishonorableKills;
                inspect.LastWeekHonorableKills = lastWeekHonorableKills;
                inspect.LastWeekDishonorableKills = lastWeekDishonorableKills;
                inspect.ThisWeekHonorableKills = thisWeekHonorableKills;
                inspect.ThisWeekDishonorableKills = thisWeekDishonorableKills;
                inspect.LifetimeHonorableKills = lifetimeHonorableKills;
                inspect.LifetimeDishonorableKills = lifetimeDishonorableKills;
                inspect.YesterdayHonor = yesterdayHonor;
                inspect.LastWeekHonor = lastWeekHonor;
                inspect.ThisWeekHonor = thisWeekHonor;
                inspect.Standing = standing;
                inspect.RankProgress = rankProgress;
                SendPacketToClient(inspect);
            }
            else
            {
                InspectHonorStatsResultTBC inspect = new InspectHonorStatsResultTBC();
                inspect.PlayerGUID = playerGuid;
                inspect.LifetimeHighestRank = lifetimeHighestRank;
                inspect.YesterdayHonorableKills = yesterdayHonorableKills;
                inspect.LifetimeHonorableKills = (ushort)lifetimeHonorableKills;
                SendPacketToClient(inspect);
            }
        }

        [PacketHandler(Opcode.MSG_INSPECT_HONOR_STATS, ClientVersionBuild.V2_0_1_6180)]
        void HandleInspectHonorStatsTBC(WorldPacket packet)
        {
            WowGuid128 playerGuid = packet.ReadGuid().To128(GetSession().GameState);
            byte lifetimeHighestRank = packet.ReadUInt8();
            ushort todayHonorableKills = packet.ReadUInt16();
            ushort yesterdayHonorableKills = packet.ReadUInt16();
            uint todayHonor = packet.ReadUInt32();
            uint yesterdayHonor = packet.ReadUInt32();
            uint lifetimeHonorableKills = packet.ReadUInt32();

            if (ModernVersion.GetExpansionVersion() == 1)
            {
                InspectHonorStatsResultClassic inspect = new InspectHonorStatsResultClassic();
                inspect.PlayerGUID = playerGuid;
                inspect.LifetimeHighestRank = lifetimeHighestRank;
                inspect.TodayHonorableKills = todayHonorableKills;
                inspect.YesterdayHonorableKills = yesterdayHonorableKills;
                inspect.LifetimeHonorableKills = lifetimeHonorableKills;
                inspect.YesterdayHonor = yesterdayHonor;
                inspect.LastWeekHonor = todayHonor;
                SendPacketToClient(inspect);
            }
            else
            {
                InspectHonorStatsResultTBC inspect = new InspectHonorStatsResultTBC();
                inspect.PlayerGUID = playerGuid;
                inspect.LifetimeHighestRank = lifetimeHighestRank;
                inspect.YesterdayHonorableKills = yesterdayHonorableKills;
                inspect.LifetimeHonorableKills = (ushort)lifetimeHonorableKills;
                SendPacketToClient(inspect);
            }
        }

        [PacketHandler(Opcode.MSG_INSPECT_ARENA_TEAMS)]
        void HandleInspectArenaTeams(WorldPacket packet)
        {
            InspectPvP inspect = new InspectPvP();
            inspect.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
            ArenaTeamInspectData team = new ArenaTeamInspectData();
            byte slot = packet.ReadUInt8();
            uint teamId = packet.ReadUInt32();
            team.TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, teamId);
            team.TeamRating = packet.ReadInt32();
            team.TeamGamesPlayed = packet.ReadInt32();
            team.TeamGamesWon = packet.ReadInt32();
            team.PersonalGamesPlayed = packet.ReadInt32();
            team.PersonalRating = packet.ReadInt32();
            GetSession().GameState.StoreArenaTeamDataForPlayer(inspect.PlayerGUID, slot, team);
            for (byte i = 0; i < 3; i++)
                inspect.ArenaTeams.Add(GetSession().GameState.GetArenaTeamDataForPlayer(inspect.PlayerGUID, slot));
            SendPacketToClient(inspect);
        }
    }
}
