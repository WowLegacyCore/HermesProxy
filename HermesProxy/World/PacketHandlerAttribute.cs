using HermesProxy.Enums;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class PacketHandlerAttribute : Attribute
    {
        public PacketHandlerAttribute(Opcode opcode)
        {
            Opcode = opcode;
        }

        /// <summary>
        /// [addedInVersion, +inf[
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="addedInVersion"></param>
        public PacketHandlerAttribute(Opcode opcode, ClientVersionBuild addedInVersion)
        {
            if (LegacyVersion.AddedInVersion(addedInVersion))
                Opcode = opcode;
        }

        /// <summary>
        /// [addedInVersion, removedInVersion[
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="addedInVersion"></param>
        /// <param name="removedInVersion"></param>
        public PacketHandlerAttribute(Opcode opcode, ClientVersionBuild addedInVersion, ClientVersionBuild removedInVersion)
        {
            if (LegacyVersion.InVersion(addedInVersion, removedInVersion))
                Opcode = opcode;
        }

        public Opcode Opcode { get; private set; }
    }
}
