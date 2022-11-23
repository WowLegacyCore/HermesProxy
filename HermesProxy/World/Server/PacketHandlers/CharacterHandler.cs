using System;
using System.Linq;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Auth;
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
        [PacketHandler(Opcode.CMSG_ENUM_CHARACTERS)]
        void HandleEnumCharacters(EnumCharacters charEnum)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ENUM_CHARACTERS);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CREATE_CHARACTER)]
        void HandleCreateCharacter(CreateCharacter charCreate)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CREATE_CHARACTER);
            packet.WriteCString(charCreate.CreateInfo.Name);
            packet.WriteUInt8((byte)charCreate.CreateInfo.RaceId);
            packet.WriteUInt8((byte)charCreate.CreateInfo.ClassId);
            packet.WriteUInt8((byte)charCreate.CreateInfo.Sex);

            CharacterCustomizations.ConvertModernCustomizationsToLegacy(charCreate.CreateInfo.Customizations, out byte skin, out byte face, out byte hairStyle, out byte hairColor, out byte facialhair);
            packet.WriteUInt8(skin);
            packet.WriteUInt8(face);
            packet.WriteUInt8(hairStyle);
            packet.WriteUInt8(hairColor);
            packet.WriteUInt8(facialhair);
            packet.WriteUInt8(0); // outfit
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CHAR_DELETE)]
        void HandleCharDelete(CharDelete charDelete)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAR_DELETE);
            packet.WriteGuid(charDelete.Guid.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_LOADING_SCREEN_NOTIFY)]
        void HandleLoadScreen(LoadingScreenNotify loadingScreenNotify)
        {
            if (loadingScreenNotify.MapID >= 0)
                GetSession().GameState.CurrentMapId = loadingScreenNotify.MapID;
        }

        [PacketHandler(Opcode.CMSG_QUERY_PLAYER_NAME)]
        void HandleNameQueryRequest(QueryPlayerName queryPlayerName)
        {
            if (GetSession().GameState.CurrentPlayerGuid == null)
            {
                // The first queried player is always us
                GetSession().GameState.CurrentPlayerGuid = queryPlayerName.Player;
                GetSession().GameState.CurrentPlayerInfo = GetSession().GameState.OwnCharacters.Single(x => x.CharacterGuid == queryPlayerName.Player);
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_NAME_QUERY);
            packet.WriteGuid(queryPlayerName.Player.To64());
            SendPacketToServer(packet, GetSession().GameState.IsInWorld ? Opcode.MSG_NULL_ACTION : Opcode.SMSG_LOGIN_VERIFY_WORLD);
        }

        [PacketHandler(Opcode.CMSG_PLAYER_LOGIN)]
        void HandlePlayerLogin(PlayerLogin playerLogin)
        {
            if (!GetSession().GameState.CachedPlayers.TryGetValue(playerLogin.Guid, out var selectedChar))
            {
                Log.Print(LogType.Error, $"Player tried to log in with unknown char id: {playerLogin.Guid}");
                return;
            }

            var realm = GetSession().RealmManager.GetRealm(GetSession().RealmId);
            if (realm == null)
            {
                Log.Print(LogType.Error, $"Player tried to log in to unknown realm id: {GetSession().RealmId}");
                return;
            }

            GetSession().AccountMetaDataMgr.SaveLastSelectedCharacter(realm.Name, selectedChar.Name, playerLogin.Guid.Low, Time.UnixTime);

            GetSession().GameState.IsFirstEnterWorld = true;
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PLAYER_LOGIN);
            packet.WriteGuid(playerLogin.Guid.To64());
            SendPacketToServer(packet);
            SendConnectToInstance(ConnectToSerial.WorldAttempt1);
            GetSession().GameState.IsConnectedToInstance = true;
        }

        [PacketHandler(Opcode.CMSG_LOGOUT_REQUEST)]
        void HandleLogoutRequest(LogoutRequest logoutRequest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_REQUEST);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_LOGOUT_CANCEL)]
        void HandleLogoutCancel(LogoutCancel logoutCancel)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_CANCEL);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_REQUEST_PLAYED_TIME)]
        void HandleRequestPlayedTime(RequestPlayedTime played)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_PLAYED_TIME);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteBool(played.TriggerScriptEvent);
            SendPacketToServer(packet);
            GetSession().GameState.ShowPlayedTime = played.TriggerScriptEvent;
        }

        [PacketHandler(Opcode.CMSG_TOGGLE_PVP)]
        void HandleTogglePvP(TogglePvP pvp)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_TOGGLE_PVP);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_PVP)]
        void HandleTogglePvP(SetPvP pvp)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_TOGGLE_PVP);
            packet.WriteBool(pvp.Enable);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_ACTION_BUTTON)]
        void HandleSetActionButton(SetActionButton button)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTION_BUTTON);
            packet.WriteUInt8(button.Index);
            packet.WriteUInt16(button.Action);
            packet.WriteUInt16(button.Type);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_SET_ACTION_BAR_TOGGLES)]
        void HandleSetActionBarToggles(SetActionBarToggles bars)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_ACTION_BAR_TOGGLES);
            packet.WriteUInt8(bars.Mask);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_UNLEARN_SKILL)]
        void HandleUnlearnSkill(UnlearnSkill skill)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_UNLEARN_SKILL);
            packet.WriteUInt32(skill.SkillLine);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_PLAYER_SHOWING_CLOAK)]
        [PacketHandler(Opcode.CMSG_PLAYER_SHOWING_HELM)]
        void HandleShowHelmOrCloak(PlayerShowingHelmOrCloak show)
        {
            WorldPacket packet = new WorldPacket(show.GetUniversalOpcode());
            packet.WriteBool(show.Showing);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_INSPECT)]
        void HandleInspect(Inspect inspect)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_INSPECT);
            packet.WriteGuid(inspect.Target.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_INSPECT_HONOR_STATS)]
        void HandleInspectHonorStats(Inspect inspect)
        {
            WorldPacket packet = new WorldPacket(Opcode.MSG_INSPECT_HONOR_STATS);
            packet.WriteGuid(inspect.Target.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_INSPECT_PVP)]
        void HandleInspectArenaTeams(Inspect inspect)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.MSG_INSPECT_ARENA_TEAMS);
                packet.WriteGuid(inspect.Target.To64());
                SendPacketToServer(packet);
            }
            else
            {
                InspectPvP pvp = new InspectPvP();
                pvp.PlayerGUID = inspect.Target;
                pvp.ArenaTeams.Add(new ArenaTeamInspectData());
                pvp.ArenaTeams.Add(new ArenaTeamInspectData());
                pvp.ArenaTeams.Add(new ArenaTeamInspectData());
                SendPacket(pvp);
            }
        }

        [PacketHandler(Opcode.CMSG_CHARACTER_RENAME_REQUEST)]
        void HandleCharacterRenameRequest(CharacterRenameRequest rename)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CHARACTER_RENAME_REQUEST);
            packet.WriteGuid(rename.Guid.To64());
            packet.WriteCString(rename.NewName);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GENERATE_RANDOM_CHARACTER_NAME)]
        void HandleGenerateRandomCharacterNameRequest(GenerateRandomCharacterNameRequest randomCharacterName)
        {
            GenerateRandomCharacterNameResult result = new();

            // The client can generate the name itself
            result.Success = false;

            SendPacket(result);
        }
    }
}
