using Framework.Constants.World;
using HermesProxy.World;
using HermesProxy.World.Objects;
using System;
using World.Packets;

namespace World
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_ENUM_CHARACTERS)]
        void HandleEnumCharacters(EnumCharacters charEnum)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ENUM_CHARACTERS);
            _worldClient.SendPacket(packet);
        }

        [PacketHandler(Opcode.CMSG_CREATE_CHARACTER)]
        void HandleCreateCharacter(CreateCharacter charCreate)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CREATE_CHARACTER);
            packet.WriteCString(charCreate.CreateInfo.Name);
            packet.WriteUInt8((byte)charCreate.CreateInfo.RaceId);
            packet.WriteUInt8((byte)charCreate.CreateInfo.ClassId);
            packet.WriteUInt8((byte)charCreate.CreateInfo.Sex);

            byte skin;
            byte face;
            byte hairStyle;
            byte hairColor;
            byte facialhair;
            CharacterCustomizations.ConvertModernCustomizationsToLegacy(charCreate.CreateInfo.Customizations, out skin, out face, out hairStyle, out hairColor, out facialhair);
            packet.WriteUInt8(skin);
            packet.WriteUInt8(face);
            packet.WriteUInt8(hairStyle);
            packet.WriteUInt8(hairColor);
            packet.WriteUInt8(facialhair);
            packet.WriteUInt8(0); // outfit
            _worldClient.SendPacket(packet);
        }
    }
}
