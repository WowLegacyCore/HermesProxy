using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public struct PlayerConst
    {
        public const int MaxTalentTiers = 7;
        public const int MaxTalentColumns = 3;
        public const int MaxTalentRank = 5;
        public const int MaxPvpTalentSlots = 4;
        public const int MinSpecializationLevel = 10;
        public const int MaxSpecializations = 5;
        public const int InitialSpecializationIndex = 4;
        public const int MaxMasterySpells = 2;
        public const int MaxDeclinedNameCases = 5;

        public const int ReqPrimaryTreeTalents = 31;
        public const int ExploredZonesSize = 192;
        public const ulong MaxMoneyAmount = 99999999999UL;
        public const int MaxActionButtons = 132;
        public const int MaxActionButtonActionValue = 0x00FFFFFF + 1;

        public const int MaxDailyQuests = 25;
        public const int QuestsCompletedBitsSize = 1750;

        public static TimeSpan InfinityCooldownDelay = TimeSpan.FromSeconds(Time.Month);  // used for set "infinity cooldowns" for spells and check
        public const uint infinityCooldownDelayCheck = Time.Month / 2;
        public const int MaxPlayerSummonDelay = 2 * Time.Minute;

        public const int TaxiMaskSize = 339;

        // corpse reclaim times
        public const int DeathExpireStep = (5 * Time.Minute);
        public const int MaxDeathCount = 3;

        public const int MaxCUFProfiles = 5;

        public static uint[] copseReclaimDelay = { 30, 60, 120 };

        public const int MaxRunes = 7;
        public const int MaxRechargingRunes = 3;

        public const int CustomDisplaySize = 3;

        public const int ArtifactsAllWeaponsGeneralWeaponEquippedPassive = 197886;

        public const int MaxArtifactTier = 1;

        public const int MaxHonorLevel = 500;
        public const byte LevelMinHonor = 20;
        public const uint SpellPvpRulesEnabled = 134735;

        //Azerite
        public const uint ItemIdHeartOfAzeroth = 158075;
        public const uint MaxAzeriteItemLevel = 129;
        public const uint MaxAzeriteItemKnowledgeLevel = 30;
        public const uint PlayerConditionIdUnlockedAzeriteEssences = 69048;
        public const uint SpellIdHeartEssenceActionBarOverride = 298554;
    }

    public struct MoneyConstants
    {
        public const int Copper = 1;
        public const int Silver = Copper * 100;
        public const int Gold = Silver * 100;
    }

    public enum TradeSlots
    {
        Invalid = -1,
        NonTraded = 6,
        TradedCount = 6,
        Count = 7
    }

    public enum Tutorials
    {
        Talent = 0,
        Spec = 1,
        Glyph = 2,
        SpellBook = 3,
        Professions = 4,
        CoreAbilitites = 5,
        PetJournal = 6,
        WhatHasChanged = 7,
        Max = 8
    }

    public enum TradeStatus
    {
        PlayerBusy = 0,
        Proposed = 1,
        Initiated = 2,
        Cancelled = 3,
        Accepted = 4,
        AlreadyTrading = 5,
        NoTarget = 6,
        Unaccepted = 7,
        Complete = 8,
        StateChanged = 9,
        TooFarAway = 10,
        WrongFaction = 11,
        Failed = 12,
        Petition = 13,
        PlayerIgnored = 14,
        Stunned = 15,
        TargetStunned = 16,
        Dead = 17,
        TargetDead = 18,
        LoggingOut = 19,
        TargetLoggingOut = 20,
        RestrictedAccount = 21,
        WrongRealm = 22,
        NotOnTaplist = 23,
        CurrencyNotTradable = 24,
        NotEnoughCurrency = 25,
    }

    public enum RestType : byte
    {
        XP = 0,
        Honor = 1,
        Max
    };

    public enum RestFlag
    {
        Tavern = 0x01,
        City = 0x02,
        FactionArea = 0x04
    }

    public enum RestState
    {
        Rested = 0x01,
        Normal = 0x02,
    };

    public enum ChatTag // vanilla only
    {
        None = 0x00,
        AFK = 0x01,
        DND = 0x02,
        GM = 0x03,
    }

    [Flags]
    public enum ChatFlags // tbc+
    {
        None = 0x00,
        AFK = 0x01,
        DND = 0x02,
        GM = 0x04,
        Com = 0x08, // Commentator
        Dev = 0x10,
        BossSound = 0x20, // Plays "RaidBossEmoteWarning" sound on raid boss emote/whisper
        Mobile = 0x40
    }

    public enum DrunkenState
    {
        Sober = 0,
        Tipsy = 1,
        Drunk = 2,
        Smashed = 3
    }

    public enum TalentSpecialization // talent tabs
    {
        MageArcane = 62,
        MageFire = 63,
        MageFrost = 64,
        PaladinHoly = 65,
        PaladinProtection = 66,
        PaladinRetribution = 70,
        WarriorArms = 71,
        WarriorFury = 72,
        WarriorProtection = 73,
        DruidBalance = 102,
        DruidFeralCombat = 103,
        DruidRestoration = 104,
        DeathKnightBlood = 250,
        DeathKnightFrost = 251,
        DeathKnightUnholy = 252,
        HunterBeastMastery = 253,
        HunterMarksman = 254,
        HunterSurvival = 255,
        PriestDiscipline = 256,
        PriestHoly = 257,
        PriestShadow = 258,
        RogueAssassination = 259,
        RogueCombat = 260,
        RogueSubtlety = 261,
        ShamanElemental = 262,
        ShamanEnhancement = 263,
        ShamanRestoration = 264,
        WarlockAffliction = 265,
        WarlockDemonology = 266,
        WarlockDestruction = 267,
        MonkBrewmaster = 268,
        MonkBattledancer = 269,
        MonkMistweaver = 270,
        DemonHunterHavoc = 577,
        DemonHunterVengeance = 581
    }

    public enum SpecResetType
    {
        Talents = 0,
        Specialization = 1,
        Glyphs = 2,
        PetTalents = 3
    }

    public enum MirrorTimerType
    {
        Disabled = -1,
        Fatigue = 0,
        Breath = 1,
        Fire = 2, // feign death
        Max = 3
    }

    enum TransferAbortReasonLegacy // vanilla and tbc
    {
        None                 = 0,
        MaxPlayers           = 1,     // Transfer Aborted: instance is full
        NotFound             = 2,     // Transfer Aborted: instance not found
        TooManyInstances     = 3,     // You have entered too many instances recently.
        Error                = 4,     // no message shown; the same effect give values above 5
        ZoneInCombat         = 5,     // Unable to zone in while an encounter is in progress.
        // TBC
        InsufExpanLvl        = 6,     // You must have TBC expansion installed to access this area.
        Difficulty           = 7,     // <Normal,Heroic,Epic> difficulty mode is not available for %s.
    };

    public enum TransferAbortReasonModern
    {
        None                       = 0,
        Error                      = 1,
        MaxPlayers                 = 2,   // Transfer ed: Instance Is Full
        NotFound                   = 3,   // Transfer ed: Instance Not Found
        TooManyInstances           = 4,   // You Have Entered Too Many Instances Recently.
        ZoneInCombat               = 6,   // Unable To Zone In While An Encounter Is In Progress.
        InsufExpanLvl              = 7,   // You Must Have <Tbc, Wotlk> Expansion Installed To Access This Area.
        Difficulty                 = 8,   // <Normal, Heroic, Epic> Difficulty Mode Is Not Available For %S.
        UniqueMessage              = 9,   // Until You'Ve Escaped Tlk'S Grasp, You Cannot Leave This Place!
        TooManyRealmInstances      = 10,  // Additional Instances Cannot Be Launched, Please Try Again Later.
        NeedGroup                  = 11,  // Transfer ed: You Must Be In A Raid Group To Enter This Instance
        NotFound2                  = 12,  // Transfer ed: Instance Not Found
        NotFound3                  = 13,  // Transfer ed: Instance Not Found
        NotFound4                  = 14,  // Transfer ed: Instance Not Found
        RealmOnly                  = 15,  // All Players In The Party Must Be From The Same Realm To Enter %S.
        MapNotAllowed              = 16,  // Map Cannot Be Entered At This Time.
        LockedToDifferentInstance  = 18,  // You Are Already Locked To %S
        AlreadyCompletedEncounter  = 19,  // You Are Ineligible To Participate In At Least One Encounter In This Instance Because You Are Already Locked To An Instance In Which It Has Been Defeated.
        DifficultyNotFound         = 22,  // Client Writes To Console "Unable To Resolve Requested Difficultyid %U To Actual Difficulty For Map %D"
        XrealmZoneDown             = 24,  // Transfer ed: Cross-Realm Zone Is Down
        SoloPlayerSwitchDifficulty = 26,  // This Instance Is Already In Progress. You May Only Switch Difficulties From Inside The Instance.
    }

    public enum ActivateTaxiReply
    {
        Ok = 0,
        UnspecifiedServerError = 1,
        NoSuchPath = 2,
        NotEnoughMoney = 3,
        TooFarAway = 4,
        NoVendorNearby = 5,
        NotVisited = 6,
        PlayerBusy = 7,
        PlayerAlreadyMounted = 8,
        PlayerShapeshifted = 9,
        PlayerMoving = 10,
        SameNode = 11,
        NotStanding = 12,
    }

    public enum TaxiNodeStatus
    {
        None = 0,
        Learned = 1,
        Unlearned = 2,
        NotEligible = 3
    }

    public enum PlayerDelayedOperations
    {
        SavePlayer = 0x01,
        ResurrectPlayer = 0x02,
        SpellCastDeserter = 0x04,
        BGMountRestore = 0x08,                     // Flag to restore mount state after teleport from BG
        BGTaxiRestore = 0x10,                     // Flag to restore taxi state after teleport from BG
        BGGroupRestore = 0x20,                     // Flag to restore group state after teleport from BG
        End
    }

    public enum CorpseType
    {
        Bones = 0,
        ResurrectablePVE = 1,
        ResurrectablePVP = 2,
        Max = 3
    }

    public enum CorpseFlags
    {
        None = 0x00,
        Bones = 0x01,
        Unk1 = 0x02,
        PvP = 0x04,
        HideHelm = 0x08,
        HideCloak = 0x10,
        Skinnable = 0x20,
        FFAPvP = 0x40
    }

    public enum ActionButtonUpdateState
    {
        UnChanged = 0,
        Changed = 1,
        New = 2,
        Deleted = 3
    }

    public enum ActionButtonType
    {
        Spell = 0x00,
        C = 0x01,                         // click?
        Eqset = 0x20,
        Dropdown = 0x30,
        Macro = 0x40,
        CMacro = C | Macro,
        Mount = 0x60,
        Item = 0x80
    }

    public enum TeleportToOptions
    {
        GMMode = 0x01,
        NotLeaveTransport = 0x02,
        NotLeaveCombat = 0x04,
        NotUnSummonPet = 0x08,
        Spell = 0x10,
        Seamless = 0x20
    }

    /// Type of environmental damages
    public enum EnvironmentalDamage
    {
        Exhausted = 0,
        Drowning  = 1,
        Fall      = 2,
        Lava      = 3,
        Slime     = 4,
        Fire      = 5,
    }

    public enum PlayerUnderwaterState
    {
        None = 0x00,
        InWater = 0x01,             // terrain type is water and player is afflicted by it
        InLava = 0x02,             // terrain type is lava and player is afflicted by it
        InSlime = 0x04,             // terrain type is lava and player is afflicted by it
        InDarkWater = 0x08,             // terrain type is dark water and player is afflicted by it

        ExistTimers = 0x10
    }

    public struct RuneCooldowns
    {
        public const int Base = 10000;
        public const int Miss = 1500;     // cooldown applied on runes when the spell misses
    }

    [Flags]
    enum PlayerFlagsLegacy
    {
        None                = 0x00000000,
        GroupLeader         = 0x00000001,
        AFK                 = 0x00000002,
        DND                 = 0x00000004,
        GM                  = 0x00000008,
        Ghost               = 0x00000010,
        Resting             = 0x00000020,
        Unk7                = 0x00000040,       // admin?
        FreeForAllPvP       = 0x00000080,
        ContestedPvP        = 0x00000100,       // Player has been involved in a PvP combat and will be attacked by contested guards
        PvPDesired          = 0x00000200,       // Stores player's permanent PvP flag preference
        HideHelm            = 0x00000400,
        HideCloak           = 0x00000800,
        PlayedLongTime      = 0x00001000,       // played long time
        PlayedTooLong       = 0x00002000,       // played too long time
        Unk15               = 0x00004000,
        Unk16               = 0x00008000,       // strange visual effect (2.0.1), looks like PLAYER_FLAGS_GHOST flag
        Sanctuary           = 0x00010000,       // player entered sanctuary
        TaxiBenchmark       = 0x00020000,       // taxi benchmark mode (on/off) (2.0.1)
        PVPTimer            = 0x00040000,       // 3.0.2, pvp timer active (after you disable pvp manually)
        Commentator         = 0x00080000,       // first appeared in TBC
        Unk21               = 0x00100000,
        Unk22               = 0x00200000,
        CommentatorCamera   = 0x00400000,       // something like COMMENTATOR_CAN_USE_INSTANCE_COMMAND
    };

    [Flags]
    public enum PlayerFlags : uint
    {
        None                = 0x00000000,
        GroupLeader         = 0x00000001,
        AFK                 = 0x00000002,
        DND                 = 0x00000004,
        GM                  = 0x00000008,
        Ghost               = 0x00000010,
        Resting             = 0x00000020,
        VoiceChat           = 0x00000040,
        Unk7                = 0x00000080,
        ContestedPvP        = 0x00000100,
        PvPDesired          = 0x00000200,
        WarModeActive       = 0x00000400,
        WarModeDesired      = 0x00000800,
        PlayedLongTime      = 0x00001000,
        PlayedTooLong       = 0x00002000,
        IsOutOfBounds       = 0x00004000,
        Developer           = 0x00008000,
        LowLevelRaidEnabled = 0x00010000,
        TaxiBenchmark       = 0x00020000,
        PVPTimer            = 0x00040000,
        Uber                = 0x00080000,
        Unk20               = 0x00100000,
        Unk21               = 0x00200000,
        Commentator         = 0x00400000,
        HideAccountAchievements = 0x00800000,
        PetBattlesUnlocked  = 0x01000000,
        NoXPGain            = 0x02000000,
        Unk26               = 0x04000000,
        AutoDeclineGuild    = 0x08000000,
        GuildLevelEnabled   = 0x10000000,
        VoidUnlocked        = 0x20000000,
        Timewalking         = 0x40000000,
        CommentatorCamera   = 0x80000000
    }

    [Flags]
    public enum PlayerFlagsEx
    {
        ReagentBankUnlocked = 0x01,
        MercenaryMode = 0x02,
        ArtifactForgeCheat = 0x04,
        InPvpCombat = 0x0040,       // Forbids /Follow
        HideHelm = 0x0080,
        HideCloak = 0x0100,
        UnlockedAoeLoot = 0x0200
    }

    public enum CharacterFlags : uint
    {
        None = 0x00000000,
        Unk1 = 0x00000001,
        Unk2 = 0x00000002,
        CharacterLockedForTransfer = 0x00000004,
        Unk4 = 0x00000008,
        Unk5 = 0x00000010,
        Unk6 = 0x00000020,
        Unk7 = 0x00000040,
        Unk8 = 0x00000080,
        Unk9 = 0x00000100,
        Unk10 = 0x00000200,
        HideHelm = 0x00000400,
        HideCloak = 0x00000800,
        Unk13 = 0x00001000,
        Ghost = 0x00002000,
        Rename = 0x00004000,
        Unk16 = 0x00008000,
        Unk17 = 0x00010000,
        Unk18 = 0x00020000,
        Unk19 = 0x00040000,
        Unk20 = 0x00080000,
        Unk21 = 0x00100000,
        Unk22 = 0x00200000,
        Unk23 = 0x00400000,
        Unk24 = 0x00800000,
        LockedByBilling = 0x01000000,
        Declined = 0x02000000,
        Unk27 = 0x04000000,
        Unk28 = 0x08000000,
        Unk29 = 0x10000000,
        Unk30 = 0x20000000,
        Unk31 = 0x40000000,
        Unk32 = 0x80000000
    }

    public enum PlayerLocalFlags
    {
        ControllingPet = 0x01, // Displays "You have an active summon already" when trying to tame new pet
        TrackStealthed = 0x02,
        ReleaseTimer = 0x08,       // Display time till auto release spirit
        NoReleaseWindow = 0x10,        // Display no "release spirit" window at all
        NoPetBar = 0x20,   // CGPetInfo::IsPetBarUsed
        OverrideCameraMinHeight = 0x40,
        NewlyBosstedCharacter = 0x80,
        UsingPartGarrison = 0x100,
        CanUseObjectsMounted = 0x200,
        CanVisitPartyGarrison = 0x400,
        WarMode = 0x800,
        AccountSecured = 0x1000, // Script_IsAccountSecured
        OverrideTransportServerTime = 0x8000,
        MentorRestricted = 0x20000,
        WeeklyRewardAvailable = 0x40000
    }

    public enum PlayerFieldByte2Flags
    {
        None = 0x00,
        Stealth = 0x20,
        InvisibilityGlow = 0x40
    }

    public enum ArenaTeams
    {
        None = 0,
        Ally = 2,
        Horde = 4,
        Any = 6
    }

    public enum RestTypes : byte
    {
        XP = 0,
        Honor = 1,
        Max
    }

    public enum PlayerRestState
    {
        Rested = 0x01,
        NotRAFLinked = 0x02,
        RAFLinked = 0x06
    }

    public enum CharacterCustomizeFlags
    {
        None = 0x00,
        Customize = 0x01,       // Name, Gender, Etc...
        Faction = 0x10000,       // Name, Gender, Faction, Etc...
        Race = 0x100000        // Name, Gender, Race, Etc...
    }

    public enum CharacterFlags3 : uint
    {
        LockedByRevokedVasTransaction = 0x100000,
        LockedByRevokedCharacterUpgrade = 0x80000000,
    }

    public enum CharacterFlags4
    {
        TrialBoost = 0x80,
        TrialBoostLocked = 0x40000,
    }

    public enum TextureSection // TODO: Find a better name. Used in CharSections.dbc
    {
        BaseSkin = 0,
        Face = 1,
        FacialHair = 2,
        Hair = 3,
        Underwear = 4,
    }

    public enum AtLoginFlags
    {
        None = 0x00,
        Rename = 0x01,
        ResetSpells = 0x02,
        ResetTalents = 0x04,
        Customize = 0x08,
        ResetPetTalents = 0x10,
        FirstLogin = 0x20,
        ChangeFaction = 0x40,
        ChangeRace = 0x80,
        Resurrect = 0x100
    }

    public enum PlayerSlots
    {
        // first slot for item stored (in any way in player items data)
        Start = 0,
        // last+1 slot for item stored (in any way in player items data)
        End = 199,
        Count = (End - Start)
    }

    public enum PlayerTitle : ulong
    {
        Disabled = 0x0000000000000000,
        None = 0x0000000000000001,
        Private = 0x0000000000000002, // 1
        Corporal = 0x0000000000000004, // 2
        SergeantA = 0x0000000000000008, // 3
        MasterSergeant = 0x0000000000000010, // 4
        SergeantMajor = 0x0000000000000020, // 5
        Knight = 0x0000000000000040, // 6
        KnightLieutenant = 0x0000000000000080, // 7
        KnightCaptain = 0x0000000000000100, // 8
        KnightChampion = 0x0000000000000200, // 9
        LieutenantCommander = 0x0000000000000400, // 10
        Commander = 0x0000000000000800, // 11
        Marshal = 0x0000000000001000, // 12
        FieldMarshal = 0x0000000000002000, // 13
        GrandMarshal = 0x0000000000004000, // 14
        Scout = 0x0000000000008000, // 15
        Grunt = 0x0000000000010000, // 16
        SergeantH = 0x0000000000020000, // 17
        SeniorSergeant = 0x0000000000040000, // 18
        FirstSergeant = 0x0000000000080000, // 19
        StoneGuard = 0x0000000000100000, // 20
        BloodGuard = 0x0000000000200000, // 21
        Legionnaire = 0x0000000000400000, // 22
        Centurion = 0x0000000000800000, // 23
        Champion = 0x0000000001000000, // 24
        LieutenantGeneral = 0x0000000002000000, // 25
        General = 0x0000000004000000, // 26
        Warlord = 0x0000000008000000, // 27
        HighWarlord = 0x0000000010000000, // 28
        Gladiator = 0x0000000020000000, // 29
        Duelist = 0x0000000040000000, // 30
        Rival = 0x0000000080000000, // 31
        Challenger = 0x0000000100000000, // 32
        ScarabLord = 0x0000000200000000, // 33
        Conqueror = 0x0000000400000000, // 34
        Justicar = 0x0000000800000000, // 35
        ChampionOfTheNaaru = 0x0000001000000000, // 36
        MercilessGladiator = 0x0000002000000000, // 37
        OfTheShatteredSun = 0x0000004000000000, // 38
        HandOfAdal = 0x0000008000000000, // 39
        VengefulGladiator = 0x0000010000000000, // 40
    }

    [Flags]
    public enum PlayerExtraFlags
    {
        // gm abilities
        GMOn = 0x01,
        AcceptWhispers = 0x04,
        TaxiCheat = 0x08,
        GMInvisible = 0x10,
        GMChat = 0x20,               // Show GM badge in chat messages

        // other states
        PVPDeath = 0x100,            // store PvP death status until corpse creating.

        // Character services markers
        HasRaceChanged = 0x0200,
        GrantedLevelsFromRaf = 0x0400,
        LevelBoosted = 0x0800
    }

    public enum EquipmentSetUpdateState
    {
        Unchanged = 0,
        Changed = 1,
        New = 2,
        Deleted = 3
    }

    public enum CUFBoolOptions
    {
        KeepGroupsTogether,
        DisplayPets,
        DisplayMainTankAndAssist,
        DisplayHealPrediction,
        DisplayAggroHighlight,
        DisplayOnlyDispellableDebuffs,
        DisplayPowerBar,
        DisplayBorder,
        UseClassColors,
        DisplayHorizontalGroups,
        DisplayNonBossDebuffs,
        DynamicPosition,
        Locked,
        Shown,
        AutoActivate2Players,
        AutoActivate3Players,
        AutoActivate5Players,
        AutoActivate10Players,
        AutoActivate15Players,
        AutoActivate25Players,
        AutoActivate40Players,
        AutoActivateSpec1,
        AutoActivateSpec2,
        AutoActivateSpec3,
        AutoActivateSpec4,
        AutoActivatePvp,
        AutoActivatePve,

        BoolOptionsCount,
    }

    public enum DuelCompleteType
    {
        Interrupted = 0,
        Won = 1,
        Fled = 2
    }

    public enum ReferAFriendError
    {
        None = 0,
        NotReferredBy = 1,
        TargetTooHigh = 2,
        InsufficientGrantableLevels = 3,
        TooFar = 4,
        DifferentFaction = 5,
        NotNow = 6,
        GrantLevelMaxI = 7,
        NoTarget = 8,
        NotInGroup = 9,
        SummonLevelMaxI = 10,
        SummonCooldown = 11,
        InsufExpanLvl = 12,
        SummonOfflineS = 13,
        NoXrealm = 14,
        MapIncomingTransferNotAllowed = 15
    }

    public enum PlayerCommandStates
    {
        None = 0x00,
        God = 0x01,
        Casttime = 0x02,
        Cooldown = 0x04,
        Power = 0x08,
        Waterwalk = 0x10
    }

    public enum AttackSwingErr
    {
        NotInRange = 0,
        BadFacing = 1,
        CantAttack = 2,
        DeadTarget = 3
    }

    public enum PlayerLogXPReason
    {
        Kill = 0,
        NoKill = 1
    }

    public enum HeirloomPlayerFlags
    {
        None = 0x00,
        BonusLevel90 = 0x01,
        BonusLevel100 = 0x02,
        BonusLevel110 = 0x04,
        BonusLevel120 = 0x08
    }

    public enum HeirloomItemFlags
    {
        None = 0x00,
        ShowOnlyIfKnown = 0x01,
        Pvp = 0x02
    }

    public enum DeclinedNameResult
    {
        Success = 0,
        Error = 1
    }

    public enum BindExtensionState
    {
        Expired = 0,
        Normal = 1,
        Extended = 2,
        Keep = 255   // special state: keep current save type
    }

    public enum TalentLearnResult
    {
        LearnOk = 0,
        FailedUnknown = 1,
        FailedNotEnoughTalentsInPrimaryTree = 2,
        FailedNoPrimaryTreeSelected = 3,
        FailedCantDoThatRightNow = 4,
        FailedAffectingCombat = 5,
        FailedCantRemoveTalent = 6,
        FailedCantDoThatChallengeModeActive = 7,
        FailedRestArea = 8
    }

    public enum TutorialsFlag
    {
        None = 0x00,
        Changed = 0x01,
        LoadedFromDB = 0x02
    }

    [Flags]
    public enum ItemSearchLocation
    {
        Equipment = 0x01,
        Inventory = 0x02,
        Bank = 0x04,
        ReagentBank = 0x08,

        Default = Equipment | Inventory,
        Everywhere = Equipment | Inventory | Bank | ReagentBank
    }

    public enum ZonePVPTypeOverride
    {
        None = 0,
        Friendly = 1,
        Hostile = 2,
        Contested = 3,
        Combat = 4
    }

    public enum PlayerCreateMode
    {
        Normal = 0,
        NPE = 1
    }

    namespace Vanilla
    {
        public struct InventorySlots
        {
            public const byte BagStart = 19;
            public const byte BagEnd = 23;
            public const byte ItemStart = 23;
            public const byte ItemEnd = 39;

            public const byte BankItemStart = 39;
            public const byte BankItemEnd = 63;
            public const byte BankBagStart = 63;
            public const byte BankBagEnd = 69;

            public const byte BuyBackStart = 69;
            public const byte BuyBackEnd = 81;
            public const byte KeyringStart = 81;
            public const byte KeyringEnd = 97;

            public const byte Bag0 = 255;
            public const byte DefaultSize = 16;
        }
    }
    namespace TBC
    {
        public struct InventorySlots
        {
            public const byte BagStart = 19;
            public const byte BagEnd = 23;
            public const byte ItemStart = 23;
            public const byte ItemEnd = 39;

            public const byte BankItemStart = 39;
            public const byte BankItemEnd = 67;
            public const byte BankBagStart = 67;
            public const byte BankBagEnd = 74;

            public const byte BuyBackStart = 74;
            public const byte BuyBackEnd = 86;
            public const byte KeyringStart = 86;
            public const byte KeyringEnd = 118;

            public const byte Bag0 = 255;
            public const byte DefaultSize = 16;
        }
    }
    namespace WotLK
    {
        public struct InventorySlots
        {
            public const byte BagStart = 19;
            public const byte BagEnd = 23;
            public const byte ItemStart = 23;
            public const byte ItemEnd = 39;

            public const byte BankItemStart = 39;
            public const byte BankItemEnd = 67;
            public const byte BankBagStart = 67;
            public const byte BankBagEnd = 74;

            public const byte BuyBackStart = 74;
            public const byte BuyBackEnd = 86;
            public const byte KeyringStart = 86;
            public const byte KeyringEnd = 118;
            public const byte CurrencyStart = 118;
            public const byte CurrencyEnd = 150;

            public const byte Bag0 = 255;
            public const byte DefaultSize = 16;
        }
    }
    namespace Classic
    {
        public struct InventorySlots
        {
            public const byte BagStart = 19;
            public const byte BagEnd = 23;
            public const byte ItemStart = 23;
            public const byte ItemEnd = 47;

            public const byte BankItemStart = 47;
            public const byte BankItemEnd = 75;
            public const byte BankBagStart = 75;
            public const byte BankBagEnd = 82;

            public const byte BuyBackStart = 82;
            public const byte BuyBackEnd = 94;
            public const byte KeyringStart = 94;
            public const byte KeyringEnd = 126;

            public const byte Bag0 = 255;
            public const byte DefaultSize = 16;
        }
    }
    

    public struct EquipmentSlot
    {
        public const byte Start = 0;
        public const byte Head = 0;
        public const byte Neck = 1;
        public const byte Shoulders = 2;
        public const byte Shirt = 3;
        public const byte Chest = 4;
        public const byte Waist = 5;
        public const byte Legs = 6;
        public const byte Feet = 7;
        public const byte Wrist = 8;
        public const byte Hands = 9;
        public const byte Finger1 = 10;
        public const byte Finger2 = 11;
        public const byte Trinket1 = 12;
        public const byte Trinket2 = 13;
        public const byte Cloak = 14;
        public const byte MainHand = 15;
        public const byte OffHand = 16;
        public const byte Ranged = 17;
        public const byte Tabard = 18;
        public const byte End = 19; // for update fields

        // only in char enum
        public const byte Bag1 = 19;
        public const byte Bag2 = 20;
        public const byte Bag3 = 21;
        public const byte Bag4 = 22;
    }

    public enum CharacterUndeleteResult
    {
        Ok = 0,
        Cooldown = 1,
        CharCreate = 2,
        Disabled = 3,
        NameTakenByThisAccount = 4,
        Unknown = 5
    }

    [Flags]
    public enum ReputationFlags : ushort
    {
        None                     = 0x0000,
        Visible                  = 0x0001,                   // makes visible in client (set or can be set at interaction with target of this faction)
        AtWar                    = 0x0002,                   // enable AtWar-button in client. player controlled (except opposition team always war state), Flag only set on initial creation
        Hidden                   = 0x0004,                   // hidden faction from reputation pane in client (player can gain reputation, but this update not sent to client)
        Header                   = 0x0008,                   // Display as header in UI
        Peaceful                 = 0x0010,
        Inactive                 = 0x0020,                   // player controlled (CMSG_SET_FACTION_INACTIVE)
        ShowPropagated           = 0x0040,
        HeaderShowsBar           = 0x0080,                   // Header has its own reputation bar
        CapitalCityForRaceChange = 0x0100,
        Guild                    = 0x0200,
        GarrisonInvasion         = 0x0400
    }

    namespace Vanilla
    {
        public enum ResponseCodes
        {
            Success = 0,
            Failure = 1,
            Cancelled = 2,
            Disconnected = 3,
            FailedToConnect = 4,
            Connected = 5,
            VersionMismatch = 6,

            CstatusConnecting = 7,
            CstatusNegotiatingSecurity = 8,
            CstatusNegotiationComplete = 9,
            CstatusNegotiationFailed = 10,
            CstatusAuthenticating = 11,

            RealmListInProgress = 34,
            RealmListSuccess = 35,
            RealmListFailed = 36,
            RealmListInvalid = 37,
            RealmListRealmNotFound = 38,

            AccountCreateInProgress = 39,
            AccountCreateSuccess = 40,
            AccountCreateFailed = 41,

            CharListRetrieving = 42,
            CharListRetrieved = 43,
            CharListFailed = 44,

            CharCreateInProgress = 45,
            CharCreateSuccess = 46,
            CharCreateError = 47,
            CharCreateFailed = 48,
            CharCreateNameInUse = 49,
            CharCreateDisabled = 50,
            CharCreatePvpTeamsViolation = 51,
            CharCreateServerLimit = 52,
            CharCreateAccountLimit = 53,
            CharCreateServerQueue = 54,
            CharCreateOnlyExisting = 55,

            CharDeleteInProgress = 56,
            CharDeleteSuccess = 57,
            CharDeleteFailed = 58,
            CharDeleteFailedLockedForTransfer = 59,

            CharLoginInProgress = 60,
            CharLoginSuccess = 61,
            CharLoginNoWorld = 62,
            CharLoginDuplicateCharacter = 63,
            CharLoginNoInstances = 64,
            CharLoginFailed = 65,
            CharLoginDisabled = 66,
            CharLoginNoCharacter = 67,
            CharLoginLockedForTransfer = 68,

            CharNameNoName = 69,
            CharNameTooShort = 70,
            CharNameTooLong = 71,
            CharNameInvalidCharacter = 72,
            CharNameMixedLanguages = 73,
            CharNameProfane = 74,
            CharNameReserved = 75,
            CharNameInvalidApostrophe = 76,
            CharNameMultipleApostrophes = 77,
            CharNameThreeConsecutive = 78,
            CharNameInvalidSpace = 79,
            CharNameConsecutiveSpaces = 80,
            CharNameFailure = 81,
            CharNameSuccess = 82
        }
    }

    namespace TBC
    {
        public enum ResponseCodes
        {
            Success = 0,
            Failure = 1,
            Cancelled = 2,
            Disconnected = 3,
            FailedToConnect = 4,
            Connected = 5,
            VersionMismatch = 6,

            CstatusConnecting = 7,
            CstatusNegotiatingSecurity = 8,
            CstatusNegotiationComplete = 9,
            CstatusNegotiationFailed = 10,
            CstatusAuthenticating = 11,

            RealmListInProgress = 0x23,
            RealmListSuccess = 0x24,
            RealmListFailed = 0x25,
            RealmListInvalid = 0x26,
            RealmListRealmNotFound = 0x27,

            AccountCreateInProgress = 0x28,
            AccountCreateSuccess = 0x29,
            AccountCreateFailed = 0x2A,

            CharListRetrieving = 0x2B,
            CharListRetrieved = 0x2C,
            CharListFailed = 0x2D,

            CharCreateInProgress = 0x2E,
            CharCreateSuccess = 0x2F,
            CharCreateError = 0x30,
            CharCreateFailed = 0x31,
            CharCreateNameInUse = 0x32,
            CharCreateDisabled = 0x33,
            CharCreatePvpTeamsViolation = 0x34,
            CharCreateServerLimit = 0x35,
            CharCreateAccountLimit = 0x36,
            CharCreateServerQueue = 0x37,
            CharCreateOnlyExisting = 0x38,
            CharCreateExpansion = 0x39,

            CharDeleteInProgress = 0x3A,
            CharDeleteSuccess = 0x3B,
            CharDeleteFailed = 0x3C,
            CharDeleteFailedLockedForTransfer = 0x3D,
            CharDeleteFailedGuildLeader = 0x3E,
            CharDeleteFailedArenaCaptain = 0x3F,

            CharLoginInProgress = 0x40,
            CharLoginSuccess = 0x41,
            CharLoginNoWorld = 0x42,
            CharLoginDuplicateCharacter = 0x43,
            CharLoginNoInstances = 0x44,
            CharLoginFailed = 0x45,
            CharLoginDisabled = 0x46,
            CharLoginNoCharacter = 0x47,
            CharLoginLockedForTransfer = 0x48,
            CharLoginLockedByBilling = 0x49,

            CharNameSuccess = 0x4A,
            CharNameFailure = 0x4B,
            CharNameNoName = 0x4C,
            CharNameTooShort = 0x4D,
            CharNameTooLong = 0x4E,
            CharNameInvalidCharacter = 0x4F,
            CharNameMixedLanguages = 0x50,
            CharNameProfane = 0x51,
            CharNameReserved = 0x52,
            CharNameInvalidApostrophe = 0x53,
            CharNameMultipleApostrophes = 0x54,
            CharNameThreeConsecutive = 0x55,
            CharNameInvalidSpace = 0x56,
            CharNameConsecutiveSpaces = 0x57,
            CharNameRussianConsecutiveSilentCharacters = 0x58,
            CharNameRussianSilentCharacterAtBeginningOrEnd = 0x59,
            CharNameDeclensionDoesntMatchBaseName = 0x5A,
        }
    }

    namespace Classic
    {
        public enum ResponseCodes
        {
            Success = 0,
            Failure = 1,
            Cancelled = 2,
            Disconnected = 3,
            FailedToConnect = 4,
            Connected = 5,
            VersionMismatch = 6,

            CstatusConnecting = 7,
            CstatusNegotiatingSecurity = 8,
            CstatusNegotiationComplete = 9,
            CstatusNegotiationFailed = 10,
            CstatusAuthenticating = 11,

            RealmListInProgress = 12,
            RealmListSuccess = 13,
            RealmListFailed = 14,
            RealmListInvalid = 15,
            RealmListRealmNotFound = 16,

            AccountCreateInProgress = 17,
            AccountCreateSuccess = 18,
            AccountCreateFailed = 19,

            CharListRetrieving = 20,
            CharListRetrieved = 21,
            CharListFailed = 22,

            CharCreateInProgress = 23,
            CharCreateSuccess = 24,
            CharCreateError = 25,
            CharCreateFailed = 26,
            CharCreateNameInUse = 27,
            CharCreateDisabled = 28,
            CharCreatePvpTeamsViolation = 29,
            CharCreateServerLimit = 30,
            CharCreateAccountLimit = 31,
            CharCreateServerQueue = 32,
            CharCreateOnlyExisting = 33,
            CharCreateExpansion = 34,
            CharCreateExpansionClass = 35,
            CharCreateCharacterInGuild = 36,
            CharCreateRestrictedRaceclass = 37,
            CharCreateCharacterChooseRace = 38,
            CharCreateCharacterArenaLeader = 39,
            CharCreateCharacterDeleteMail = 40,
            CharCreateCharacterSwapFaction = 41,
            CharCreateCharacterRaceOnly = 42,
            CharCreateCharacterGoldLimit = 43,
            CharCreateForceLogin = 44,
            CharCreateTrial = 45,
            CharCreateTimeout = 46,
            CharCreateThrottle = 47,
            CharCreateAlliedRaceAchievement = 48,
            CharCreateCharacterInCommunity = 49,
            CharCreateNewPlayer = 50,

            CharDeleteInProgress = 51,
            CharDeleteSuccess = 52,
            CharDeleteFailed = 53,
            CharDeleteFailedLockedForTransfer = 54,
            CharDeleteFailedGuildLeader = 55,
            CharDeleteFailedArenaCaptain = 56,
            CharDeleteFailedHasHeirloomOrMail = 57,
            CharDeleteFailedUpgradeInProgress = 58,
            CharDeleteFailedHasWowToken = 59,
            CharDeleteFailedVasTransactionInProgress = 60,
            CharDeleteFailedCommunityOwner = 61,

            CharLoginInProgress = 62,
            CharLoginSuccess = 63,
            CharLoginNoWorld = 64,
            CharLoginDuplicateCharacter = 65,
            CharLoginNoInstances = 66,
            CharLoginFailed = 67,
            CharLoginDisabled = 68,
            CharLoginNoCharacter = 69,
            CharLoginLockedForTransfer = 70,
            CharLoginLockedByBilling = 71,
            CharLoginLockedByMobileAh = 72,
            CharLoginTemporaryGmLock = 73,
            CharLoginLockedByCharacterUpgrade = 74,
            CharLoginLockedByRevokedCharacterUpgrade = 75,
            CharLoginLockedByRevokedVasTransaction = 76,
            CharLoginLockedByRestriction = 77,

            CharLoginLockedForRealmPlaytype = 78,

            CharNameSuccess = 79,
            CharNameFailure = 80,
            CharNameNoName = 81,
            CharNameTooShort = 82,
            CharNameTooLong = 83,
            CharNameInvalidCharacter = 84,
            CharNameMixedLanguages = 85,
            CharNameProfane = 86,
            CharNameReserved = 87,
            CharNameInvalidApostrophe = 88,
            CharNameMultipleApostrophes = 89,
            CharNameThreeConsecutive = 90,
            CharNameInvalidSpace = 91,
            CharNameConsecutiveSpaces = 92,
            CharNameRussianConsecutiveSilentCharacters = 93,
            CharNameRussianSilentCharacterAtBeginningOrEnd = 94,
            CharNameDeclensionDoesntMatchBaseName = 95,
            CharNameSpacesDisallowed = 96,
        }
    }
}
