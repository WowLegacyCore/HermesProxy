using Framework.Constants;
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
                Global.CurrentSessionData.GameData.CurrentMapId = loadingScreenNotify.MapID;
        }

        [PacketHandler(Opcode.CMSG_QUERY_PLAYER_NAME)]
        void HandleNameQueryRequest(QueryPlayerName queryPlayerName)
        {
            if (Global.CurrentSessionData.GameData.CurrentPlayerGuid == null)
                Global.CurrentSessionData.GameData.CurrentPlayerGuid = queryPlayerName.Player;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_NAME_QUERY);
            packet.WriteGuid(queryPlayerName.Player.To64());
            SendPacketToServer(packet, Global.CurrentSessionData.GameData.IsInWorld ? Opcode.MSG_NULL_ACTION : Opcode.SMSG_LOGIN_VERIFY_WORLD);
        }

        [PacketHandler(Opcode.CMSG_PLAYER_LOGIN)]
        void HandlePlayerLogin(PlayerLogin playerLogin)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_PLAYER_LOGIN);
            packet.WriteGuid(playerLogin.Guid.To64());
            SendPacketToServer(packet);
            SendConnectToInstance(ConnectToSerial.WorldAttempt1);
        }
    }
}
