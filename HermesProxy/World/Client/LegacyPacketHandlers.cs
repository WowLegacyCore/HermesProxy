using Framework.Constants.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using World;
using static HermesProxy.World.Client.WorldClient;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_ENUM_CHARACTERS_RESULT)]
        void HandleEnumCharactersResult(WorldPacket packet, List<ServerPacket> responses)
        {
            Console.WriteLine("Hello!!!!!! HandleEnumCharactersResult ");
            Console.WriteLine("IT WORKS");
            Console.WriteLine("IT WORKS");
            Console.WriteLine("IT WORKS");
            Console.WriteLine("IT WORKS");
            Console.WriteLine("IT WORKS");
        }
    }
}
