using Framework.Constants;
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
                GetSession().GameState.CurrentPlayerGuid = queryPlayerName.Player;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_NAME_QUERY);
            packet.WriteGuid(queryPlayerName.Player.To64());
            SendPacketToServer(packet, GetSession().GameState.IsInWorld ? Opcode.MSG_NULL_ACTION : Opcode.SMSG_LOGIN_VERIFY_WORLD);
        }

        [PacketHandler(Opcode.CMSG_PLAYER_LOGIN)]
        void HandlePlayerLogin(PlayerLogin playerLogin)
        {
            GetSession().GameState.IsFirstEnterWorld = true;
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PLAYER_LOGIN);
            packet.WriteGuid(playerLogin.Guid.To64());
            SendPacketToServer(packet);
            SendConnectToInstance(ConnectToSerial.WorldAttempt1);
        }

        [PacketHandler(Opcode.CMSG_LOGOUT_REQUEST)]
        void HandleLogoutRequest(LogoutRequest logoutRequest)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_REQUEST);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_LOGOUT_CANCEL)]
        void HandleLogoutCancel(LogoutRequest logoutRequest)
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
    }
}
