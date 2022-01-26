namespace HermesProxy.Framework.Constants
{
    public enum ObjectType
    {
        Object                  = 0,
        Item                    = 1,
        Container               = 2,
        AzeriteEmpoweredItem    = 3,
        AzeriteItem             = 4,
        Unit                    = 5,
        Player                  = 6,
        ActivePlayer            = 7,
        GameObject              = 8,
        DynamicObject           = 9,
        Corpse                  = 10,
        AreaTrigger             = 11,
        SceneObject             = 12,
        Conversation            = 13,
        Map                     = 14
    }

    public enum ObjectTypeLegacy
    {
        Object                  = 0,
        Item                    = 1,
        Container               = 2,
        Unit                    = 3,
        Player                  = 4,
        GameObject              = 5,
        DynamicObject           = 6,
        Corpse                  = 7,
        AreaTrigger             = 8,
        SceneObject             = 9,
        Conversation            = 10
    }

    public enum ObjectType801
    {
        Object                  = 0,
        Item                    = 1,
        Container               = 2,
        AzeriteEmpoweredItem    = 3,
        AzeriteItem             = 4,
        Unit                    = 5,
        Player                  = 6,
        ActivePlayer            = 7,
        GameObject              = 8,
        DynamicObject           = 9,
        Corpse                  = 10,
        AreaTrigger             = 11,
        SceneObject             = 12,
        Conversation            = 13
    }

    public enum ObjectTypeBCC
    {
        Object                  = 0,
        Item                    = 1,
        Container               = 2,
        Unit                    = 3,
        Player                  = 4,
        ActivePlayer            = 5,
        GameObject              = 6,
        DynamicObject           = 7,
        Corpse                  = 8,
        AreaTrigger             = 9,
        SceneObject             = 10,
        Conversation            = 11,
    }

    public enum HighGuidType
    {
        Null = 0,
        Uniq,
        Player,
        Item,
        WorldTransaction,
        StaticDoor,
        Transport,
        Conversation,
        Creature,
        Vehicle,
        Pet,
        GameObject,
        DynamicObject,
        AreaTrigger,
        Corpse,
        LootObject,
        SceneObject,
        Scenario,
        AIGroup,
        DynamicDoor,
        ClientActor,
        Vignette,
        CallForHelp,
        AIResource,
        AILock,
        AILockTicket,
        ChatChannel,
        Party,
        Guild,
        WowAccount,
        BNetAccount,
        GMTask,
        MobileSession,
        RaidGroup,
        Spell,
        Mail,
        WebObj,
        LFGObject,
        LFGList,
        UserRouter,
        PVPQueueGroup,
        UserClient,
        PetBattle,
        UniqUserClient,
        BattlePet,
        CommerceObj,
        ClientSession,
        Cast,
        Invalid
    };

    public enum HighGuidTypeLegacy
    {
        None                = -1,
        Player              = 0x000, // Seen 0x280 for players too
        BattleGround1       = 0x101,
        InstanceSave        = 0x104,
        Group               = 0x105,
        BattleGround2       = 0x109,
        MOTransport         = 0x10C,
        Unknown270          = 0x10E, // pets and mounts?
        Guild               = 0x10F,
        Item                = 0x400, // Container
        DynObject           = 0xF00, // Corpses
        GameObject          = 0xF01,
        Transport           = 0xF02,
        Unit                = 0xF03,
        Pet                 = 0xF04,
        Vehicle             = 0xF05
    }
}
