using System;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum PetFlags : byte
    {
        None           = 0x00,
        CanBeRenamed   = 0x01,
        CanBeAbandoned = 0x02
    }
}
