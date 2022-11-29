using System;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum ObjectTypeMask
    {
        Object        = 0x01,
        Item          = 0x02,
        Container     = 0x04,
        Unit          = 0x8,
        Player        = 0x10,
        ActivePlayer  = 0x20,
        GameObject    = 0x40,
        DynamicObject = 0x80,
        Corpse        = 0x100,
        AreaTrigger   = 0x200,
        Sceneobject   = 0x400,
        Conversation  = 0x800,
        Seer = Player | Unit | DynamicObject
    }
}
