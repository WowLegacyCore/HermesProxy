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
                Global.PlayerCache cache = new Global.PlayerCache();
                char1.Guid = packet.ReadGuid().To128();
                Global.CurrentSessionData.GameState.OwnCharacters.Add(char1.Guid);
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

                Global.CurrentSessionData.GameState.UpdatePlayerCache(char1.Guid, cache);

                char1.ZoneId = packet.ReadUInt32();
                char1.MapId = packet.ReadUInt32();
                char1.PreloadPos = packet.ReadVector3();
                uint guildId = packet.ReadUInt32();
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
            if (Framework.Settings.GetClientExpansionVersion() >= 2 &&
                Framework.Settings.GetServerExpansionVersion() >= 2)
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
                Objects.TBC.ResponseCodes legacyCode = (Objects.TBC.ResponseCodes)result;
                createChar.Code = (Objects.Classic.ResponseCodes)Enum.Parse(typeof(Objects.Classic.ResponseCodes), legacyCode.ToString());
            }
            else
            {
                Objects.Vanilla.ResponseCodes legacyCode = (Objects.Vanilla.ResponseCodes)result;
                createChar.Code = (Objects.Classic.ResponseCodes)Enum.Parse(typeof(Objects.Classic.ResponseCodes), legacyCode.ToString());
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
                Objects.TBC.ResponseCodes legacyCode = (Objects.TBC.ResponseCodes)result;
                deleteChar.Code = (Objects.Classic.ResponseCodes)Enum.Parse(typeof(Objects.Classic.ResponseCodes), legacyCode.ToString());
            }
            else
            {
                Objects.Vanilla.ResponseCodes legacyCode = (Objects.Vanilla.ResponseCodes)result;
                deleteChar.Code = (Objects.Classic.ResponseCodes)Enum.Parse(typeof(Objects.Classic.ResponseCodes), legacyCode.ToString());
            }
            SendPacketToClient(deleteChar);
        }

        [PacketHandler(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)]
        void HandleQueryPlayerNameResponse(WorldPacket packet)
        {
            QueryPlayerNameResponse response = new QueryPlayerNameResponse();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
            {
                response.Player = response.Data.GuidActual = packet.ReadPackedGuid().To128();
                var fail = packet.ReadBool();
                if (fail)
                {
                    response.Result = Objects.Classic.ResponseCodes.Failure;
                    SendPacketToClient(response);
                    return;
                }
            }
            else
                response.Player = response.Data.GuidActual = packet.ReadGuid().To128();

            Global.PlayerCache cache = new Global.PlayerCache();
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

            if (Global.CurrentSessionData.GameState.CachedPlayers.ContainsKey(response.Player))
                response.Data.Level = Global.CurrentSessionData.GameState.CachedPlayers[response.Player].Level;
            if (response.Data.Level == 0)
                response.Data.Level = 1;

            Global.CurrentSessionData.GameState.UpdatePlayerCache(response.Player, cache);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                if (packet.ReadBool())
                {
                    for (var i = 0; i < 5; i++)
                        response.Data.DeclinedNames.name[i] = packet.ReadCString();
                }
            }

            response.Data.IsDeleted = false;
            response.Data.AccountID = Global.CurrentSessionData.GameState.GetGameAccountGuidForPlayer(response.Player);
            response.Data.BnetAccountID = Global.CurrentSessionData.GameState.GetBnetAccountGuidForPlayer(response.Player);
            response.Data.GuidActual = WowGuid128.Empty;
            response.Data.VirtualRealmAddress = Global.CurrentSessionData.RealmId.GetAddress();
            SendPacketToClient(response);
        }

        [PacketHandler(Opcode.SMSG_LOGIN_VERIFY_WORLD)]
        void HandleLoginVerifyWorld(WorldPacket packet)
        {
            LoginVerifyWorld verify = new LoginVerifyWorld();
            verify.MapID = packet.ReadUInt32();
            Global.CurrentSessionData.GameState.CurrentMapId = verify.MapID;
            verify.Pos.X = packet.ReadFloat();
            verify.Pos.Y = packet.ReadFloat();
            verify.Pos.Z = packet.ReadFloat();
            verify.Pos.Orientation = packet.ReadFloat();
            SendPacketToClient(verify);

            Global.CurrentSessionData.GameState.IsInWorld = true;

            WorldServerInfo info = new();
            if (verify.MapID > 1)
            {
                info.DifficultyID = 1;
                info.InstanceGroupSize = 5;
            }
            SendPacketToClient(info);
        }

        [PacketHandler(Opcode.SMSG_CHARACTER_LOGIN_FAILED)]
        void HandleCharacterLoginFailed(WorldPacket packet)
        {
            CharacterLoginFailed failed = new CharacterLoginFailed();
            failed.Code = (Framework.Constants.LoginFailureReason)packet.ReadUInt8();
            SendPacketToClient(failed);

            Global.CurrentSessionData.GameState.IsInWorld = false;
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

            Global.CurrentSessionData.GameState.ActionButtons = buttons;
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

            Global.CurrentSessionData.GameState = new();
            Global.CurrentSessionData.InstanceSocket.CloseSocket();
            Global.CurrentSessionData.InstanceSocket = null;
        }

        [PacketHandler(Opcode.SMSG_LOGOUT_CANCEL_ACK)]
        void HandleLogoutCancelAck(WorldPacket packet)
        {
            LogoutCancelAck logout = new LogoutCancelAck();
            SendPacketToClient(logout);
        }
    }
}
