using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum GroupFlags
    {
        None              = 0x000,
        FakeRaid          = 0x001,
        Raid              = 0x002,
        LfgRestricted     = 0x004, // Script_HasLFGRestrictions()
        Lfg               = 0x008,
        Destroyed         = 0x010,
        OnePersonParty    = 0x020, // Script_IsOnePersonParty()
        EveryoneAssistant = 0x040, // Script_IsEveryoneAssistant()
        GuildGroup        = 0x100,

        MaskBgRaid        = FakeRaid | Raid
    }

    public enum GroupType
    {
        None         = 0,
        Normal       = 1,
        PvP          = 2, // battleground and arena
        WorldPvP     = 4,
    }

    [Flags]
    public enum GroupMemberOnlineStatus
    {
        Offline = 0x000,
        Online  = 0x001, // Lua_UnitIsConnected
        PVP     = 0x002, // Lua_UnitIsPVP
        Dead    = 0x004, // Lua_UnitIsDead
        Ghost   = 0x008, // Lua_UnitIsGhost
        PVPFFA  = 0x010, // Lua_UnitIsPVPFreeForAll
        Unk3    = 0x020, // used in calls from Lua_GetPlayerMapPosition/Lua_GetBattlefieldFlagPosition
        AFK     = 0x040, // Lua_UnitIsAFK
        DND     = 0x080, // Lua_UnitIsDND
        RAF     = 0x100,
        Vehicle = 0x200, // Lua_UnitInVehicle
    }

    [Flags]
    public enum GroupMemberFlags
    {
        None       = 0x00,
        Assistant  = 0x01,
        MainTank   = 0x02,
        MainAssist = 0x04
    }

    [Flags]
    public enum GroupUpdateFlagVanilla: uint
    {
        None             = 0x00000000,
        Status           = 0x00000001,
        CurrentHealth    = 0x00000002,
        MaxHealth        = 0x00000004,
        PowerType        = 0x00000008,
        CurrentPower     = 0x00000010,
        MaxPower         = 0x00000020,
        Level            = 0x00000040,
        Zone             = 0x00000080,
        Position         = 0x00000100,
        Auras            = 0x00000200,
        AurasNegative    = 0x00000400,
        PetGuid          = 0x00000800,
        PetName          = 0x00001000,
        PetModelId       = 0x00002000,
        PetCurrentHealth = 0x00004000,
        PetMaxHealth     = 0x00008000,
        PetPowerType     = 0x00010000,
        PetCurrentPower  = 0x00020000,
        PetMaxPower      = 0x00040000,
        PetAuras         = 0x00080000,
        PetAurasNegative = 0x00100000,
    }
}
