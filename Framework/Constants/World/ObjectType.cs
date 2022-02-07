using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Constants.World
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
        MOTransport,
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
        None                    = -1,
        Item                    = 0x4000,                       // blizz 4000
        Player                  = 0x0000,                       // blizz 0000
        GameObject              = 0xF110,                       // blizz F110
        Transport               = 0xF120,                       // blizz F120 (for GAMEOBJECT_TYPE_TRANSPORT)
        Creature                = 0xF130,                       // blizz F130
        Pet                     = 0xF140,                       // blizz F140
        Vehicle                 = 0xF150,                       // blizz F15/F55
        DynamicObject           = 0xF100,                       // blizz F100
        Corpse                  = 0xF101,                       // blizz F100
        MOTransport             = 0x1FC0,                       // blizz 1FC0 (for GAMEOBJECT_TYPE_MO_TRANSPORT)
        Group                   = 0x1F50,                       // blizz 1F5x
    }

    public enum HighGuidType703
    {
        Null = 0,
        Uniq = 1,
        Player = 2,
        Item = 3,
        WorldTransaction = 4,
        StaticDoor = 5,
        Transport = 6,
        Conversation = 7,
        Creature = 8,
        Vehicle = 9,
        Pet = 10,
        GameObject = 11,
        DynamicObject = 12,
        AreaTrigger = 13,
        Corpse = 14,
        LootObject = 15,
        SceneObject = 16,
        Scenario = 17,
        AIGroup = 18,
        DynamicDoor = 19,
        ClientActor = 20,
        Vignette = 21,
        CallForHelp = 22,
        AIResource = 23,
        AILock = 24,
        AILockTicket = 25,
        ChatChannel = 26,
        Party = 27,
        Guild = 28,
        WowAccount = 29,
        BNetAccount = 30,
        GMTask = 31,
        MobileSession = 32,
        RaidGroup = 33,
        Spell = 34,
        Mail = 35,
        WebObj = 36,
        LFGObject = 37,
        LFGList = 38,
        UserRouter = 39,
        PVPQueueGroup = 40,
        UserClient = 41,
        PetBattle = 42,
        UniqUserClient = 43,
        BattlePet = 44,
        CommerceObj = 45,
        ClientSession = 46,
        Cast = 47,

        Invalid = 63
    }
}
