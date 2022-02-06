using Framework.Constants.World;
using HermesProxy.World;
using System;
using World.Packets;

namespace World
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_ENUM_CHARACTERS)]
        void HandleCharEnum(EnumCharacters charEnum)
        {
            Console.WriteLine("RECEIVED CHAR ENUM FROM CLIENT");
            Console.WriteLine("!!!!!!!!");
            Console.WriteLine("!!!!!!!!");
        }
    }
}
