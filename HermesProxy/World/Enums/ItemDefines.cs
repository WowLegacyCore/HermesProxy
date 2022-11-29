namespace HermesProxy.World.Enums
{
    public struct ItemConst
    {
        public const int MaxDamages = 2;                           // changed in 3.1.0
        public const int MaxGemSockets = 3;
        public const int MaxSpells = 5;
        public const int MaxStats = 10;
        public const int MaxBagSize = 36;
        public const byte NullBag = 0;
        public const byte NullSlot = 255;
        public const int MaxOutfitItems = 24;
        public const int MaxItemExtCostItems = 5;
        public const int MaxItemExtCostCurrencies = 5;
        public const int MaxItemEnchantmentEffects = 3;
        public const int MaxProtoSpells = 5;
        public const int MaxEquipmentSetIndex = 20;

        public const int MaxItemSubclassTotal = 21;

        public const int MaxItemSetItems = 17;
        public const int MaxItemSetSpells = 8;

        public static uint[] ItemQualityColors =
        {
            0xff9d9d9d, // GREY
            0xffffffff, // WHITE
            0xff1eff00, // GREEN
            0xff0070dd, // BLUE
            0xffa335ee, // PURPLE
            0xffff8000, // ORANGE
            0xffe6cc80, // LIGHT YELLOW
            0xffe6cc80  // LIGHT YELLOW
        };
    }

    public enum ItemModifier
    {
        TransmogAppearanceAllSpecs = 0,
        TransmogAppearanceSpec1 = 1,
        UpgradeId = 2,
        BattlePetSpeciesId = 3,
        BattlePetBreedData = 4, // (Breedid) | (Breedquality << 24)
        BattlePetLevel = 5,
        BattlePetDisplayId = 6,
        EnchantIllusionAllSpecs = 7,
        ArtifactAppearanceId = 8,
        TimewalkerLevel = 9,
        EnchantIllusionSpec1 = 10,
        TransmogAppearanceSpec2 = 11,
        EnchantIllusionSpec2 = 12,
        TransmogAppearanceSpec3 = 13,
        EnchantIllusionSpec3 = 14,
        TransmogAppearanceSpec4 = 15,
        EnchantIllusionSpec4 = 16,
        ChallengeMapChallengeModeId = 17,
        ChallengeKeystoneLevel = 18,
        ChallengeKeystoneAffixId1 = 19,
        ChallengeKeystoneAffixId2 = 20,
        ChallengeKeystoneAffixId3 = 21,
        ChallengeKeystoneAffixId4 = 22,
        ArtifactKnowledgeLevel = 23,
        ArtifactTier = 24,
        TransmogAppearanceSpec5 = 25,
        PvpRating = 26,
        EnchantIllusionSpec5 = 27,
        ContentTuningId = 28,
        ChangeModifiedCraftingStat1 = 29,
        ChangeModifiedCraftingStat2 = 30,
        TransmogSecondaryAppearanceAllSpecs = 31,
        TransmogSecondaryAppearanceSpec1 = 32,
        TransmogSecondaryAppearanceSpec2 = 33,
        TransmogSecondaryAppearanceSpec3 = 34,
        TransmogSecondaryAppearanceSpec4 = 35,
        TransmogSecondaryAppearanceSpec5 = 36,
        SoulbindConduitRank = 37,

        Max
    }

    public enum ItemContext : byte
    {
        None = 0,
        DungeonNormal = 1,
        DungeonHeroic = 2,
        RaidNormal = 3,
        RaidRaidFinder = 4,
        RaidHeroic = 5,
        RaidMythic = 6,
        PvpUnranked1 = 7,
        PvpRanked1Unrated = 8,
        ScenarioNormal = 9,
        ScenarioHeroic = 10,
        QuestReward = 11,
        InGameStore = 12,
        TradeSkill = 13,
        Vendor = 14,
        BlackMarket = 15,
        MythicplusEndOfRun = 16,
        DungeonLvlUp1 = 17,
        DungeonLvlUp2 = 18,
        DungeonLvlUp3 = 19,
        DungeonLvlUp4 = 20,
        ForceToNone = 21,
        Timewalking = 22,
        DungeonMythic = 23,
        PvpHonorReward = 24,
        WorldQuest1 = 25,
        WorldQuest2 = 26,
        WorldQuest3 = 27,
        WorldQuest4 = 28,
        WorldQuest5 = 29,
        WorldQuest6 = 30,
        MissionReward1 = 31,
        MissionReward2 = 32,
        MythicplusEndOfRunTimeChest = 33,
        ZzchallengeMode3 = 34,
        MythicplusJackpot = 35,
        WorldQuest7 = 36,
        WorldQuest8 = 37,
        PvpRanked2Combatant = 38,
        PvpRanked3Challenger = 39,
        PvpRanked4Rival = 40,
        PvpUnranked2 = 41,
        WorldQuest9 = 42,
        WorldQuest10 = 43,
        PvpRanked5Duelist = 44,
        PvpRanked6Elite = 45,
        PvpRanked7 = 46,
        PvpUnranked3 = 47,
        PvpUnranked4 = 48,
        PvpUnranked5 = 49,
        PvpUnranked6 = 50,
        PvpUnranked7 = 51,
        PvpRanked8 = 52,
        WorldQuest11 = 53,
        WorldQuest12 = 54,
        WorldQuest13 = 55,
        PvpRankedJackpot = 56,
        TournamentRealm = 57,
        Relinquished = 58,
        LegendaryForge = 59,
        QuestBonusLoot = 60,
        CharacterBoostBfa = 61,
        CharacterBoostShadowlands = 62,
        LegendaryCrafting1 = 63,
        LegendaryCrafting2 = 64,
        LegendaryCrafting3 = 65,
        LegendaryCrafting4 = 66,
        LegendaryCrafting5 = 67,
        LegendaryCrafting6 = 68,
        LegendaryCrafting7 = 69,
        LegendaryCrafting8 = 70,
        LegendaryCrafting9 = 71,
        WeeklyRewardsAdditional = 72,
        WeeklyRewardsConcession = 73,
        WorldQuestJackpot = 74,
        NewCharacter = 75,
        WarMode = 76,
        PvpBrawl1 = 77,
        PvpBrawl2 = 78,
        Torghast = 79,
        CorpseRecovery = 80,
        WorldBoss = 81,
        RaidNormalExtended = 82,
        RaidRaidFinderExtended = 83,
        RaidHeroicExtended = 84,
        RaidMythicExtended = 85,
        CharacterTemplate91 = 86,

        Max
    }

    public enum ItemVendorType
    {
        None     = 0,
        Item     = 1,
        Currency = 2,
        Spell    = 3,
        MawPower = 4
    }

    public enum BuyResult
    {
        CantFindItem      = 0,
        ItemAlreadySold   = 1,
        NotEnoughtMoney   = 2,
        SellerDontLikeYou = 4,
        DistanceTooFar    = 5,
        ItemSoldOut       = 7,
        CantCarryMore     = 8,
        RankRequire       = 11,
        ReputationRequire = 12
    }

    public enum InventoryResultVanilla
    {
        Ok = 0,
        CantEquipLevel,
        CantEquipSkill,
        WrongSlot,
        BagFull,
        BagInBag,
        TradeEquippedBag,
        AmmoOnly,
        ProficiencyNeeded,
        NoSlotAvailable,
        CantEquipEver,
        CantEquipEver2,
        NoSlotAvailable2,
        Equipped2handed,
        TwoHandSkillNotFound,
        WrongBagType,
        WrongBagType2,
        ItemMaxCount,
        NoSlotAvailable3,
        CantStack,
        NotEquippable,
        CantSwap,
        SlotEmpty,
        ItemNotFound,
        DropBoundItem,
        OutOfRange,
        TooFewToSplit,
        SplitFailed,
        SpellFailedReagentsGeneric,
        NotEnoughMoney,
        NotABag,
        DestroyNonemptyBag,
        NotOwner,
        OnlyOneQuiver,
        NoBankSlot,
        NoBankHere,
        ItemLocked,
        GenericStunned,
        PlayerDead,
        ClientLockedOut,
        InternalBagError,
        OnlyOneBolt,
        OnlyOneAmmo,
        CantWrapStackable,
        CantWrapEquipped,
        CantWrapWrapped,
        CantWrapBound,
        CantWrapUnique,
        CantWrapBags,
        LootGone,
        InvFull,
        BankFull,
        VendorSoldOut,
        BagFull2,
        ItemNotFound2,
        CantStack2,
        BagFull3,
        VendorSoldOut2,
        ObjectIsBusy,
        CantBeDisenchanted,
        NotInCombat,
        NotWhileDisarmed,
        BagFull4,
        CantEquipRank,
        CantEquipReputation,
        TooManySpecialBags,
        LootCantLootThatNow,
    };

    public enum InventoryResultTBC
    {
        Ok = 0,
        CantEquipLevel,
        CantEquipSkill,
        WrongSlot,
        BagFull,
        BagInBag,
        TradeEquippedBag,
        AmmoOnly,
        ProficiencyNeeded,
        NoSlotAvailable,
        CantEquipEver,
        CantEquipEver2,
        NoSlotAvailable2,
        Equipped2handed,
        TwoHandSkillNotFound,
        WrongBagType,
        WrongBagType2,
        ItemMaxCount,
        NoSlotAvailable3,
        CantStack,
        NotEquippable,
        CantSwap,
        SlotEmpty,
        ItemNotFound,
        DropBoundItem,
        OutOfRange,
        TooFewToSplit,
        SplitFailed,
        SpellFailedReagentsGeneric,
        NotEnoughMoney,
        NotABag,
        DestroyNonemptyBag,
        NotOwner,
        OnlyOneQuiver,
        NoBankSlot,
        NoBankHere,
        ItemLocked,
        GenericStunned,
        PlayerDead,
        ClientLockedOut,
        InternalBagError,
        OnlyOneBolt,
        OnlyOneAmmo,
        CantWrapStackable,
        CantWrapEquipped,
        CantWrapWrapped,
        CantWrapBound,
        CantWrapUnique,
        CantWrapBags,
        LootGone,
        InvFull,
        BankFull,
        VendorSoldOut,
        BagFull2,
        ItemNotFound2,
        CantStack2,
        BagFull3,
        VendorSoldOut2,
        ObjectIsBusy,
        CantBeDisenchanted,
        NotInCombat,
        NotWhileDisarmed,
        BagFull4,
        CantEquipRank,
        CantEquipReputation,
        TooManySpecialBags,
        LootCantLootThatNow,
        ItemUniqueEquippable,
        VendorMissingTurnins,
        NotEnoughHonorPoints,
        NotEnoughArenaPoints,
        ItemMaxCountSocketed,
        MailBoundItem,
        InternalBagError2,
        BagFull5,
        ItemMaxCountEquippedSocketed,
        ItemUniqueEquippableSocketed,
        TooMuchGold,
        NotDuringArenaMatch,
        TradeBoundItem,
        CantEquipRating,
    };

    public enum InventoryResult
    {
        Ok = 0,
        CantEquipLevel = 1,  // You Must Reach Level %D To Use That Item.
        CantEquipSkill = 2,  // You Aren'T Skilled Enough To Use That Item.
        WrongSlot = 3,  // That Item Does Not Go In That Slot.
        BagFull = 4,  // That Bag Is Full.
        BagInBag = 5,  // Can'T Put Non-Empty Bags In Other Bags.
        TradeEquippedBag = 6,  // You Can'T Trade Equipped Bags.
        AmmoOnly = 7,  // Only Ammo Can Go There.
        ProficiencyNeeded = 8,  // You Do Not Have The Required Proficiency For That Item.
        NoSlotAvailable = 9,  // No Equipment Slot Is Available For That Item.
        CantEquipEver = 10, // You Can Never Use That Item.
        CantEquipEver2 = 11, // You Can Never Use That Item.
        NoSlotAvailable2 = 12, // No Equipment Slot Is Available For That Item.
        Equipped2handed = 13, // Cannot Equip That With A Two-Handed Weapon.
        TwoHandSkillNotFound = 14, // You Cannot Dual-Wield
        WrongBagType = 15, // That Item Doesn'T Go In That Container.
        WrongBagType2 = 16, // That Item Doesn'T Go In That Container.
        ItemMaxCount = 17, // You Can'T Carry Any More Of Those Items.
        NoSlotAvailable3 = 18, // No Equipment Slot Is Available For That Item.
        CantStack = 19, // This Item Cannot Stack.
        NotEquippable = 20, // This Item Cannot Be Equipped.
        CantSwap = 21, // These Items Can'T Be Swapped.
        SlotEmpty = 22, // That Slot Is Empty.
        ItemNotFound = 23, // The Item Was Not Found.
        DropBoundItem = 24, // You Can'T Drop A Soulbound Item.
        OutOfRange = 25, // Out Of Range.
        TooFewToSplit = 26, // Tried To Split More Than Number In Stack.
        SplitFailed = 27, // Couldn'T Split Those Items.
        SpellFailedReagentsGeneric = 28, // Missing Reagent
        CantTradeGold = 29, // Gold May Only Be Offered By One Trader.
        NotEnoughMoney = 30, // You Don'T Have Enough Money.
        NotABag = 31, // Not A Bag.
        DestroyNonemptyBag = 32, // You Can Only Do That With Empty Bags.
        NotOwner = 33, // You Don'T Own That Item.
        OnlyOneQuiver = 34, // You Can Only Equip One Quiver.
        NoBankSlot = 35, // You Must Purchase That Bag Slot First
        NoBankHere = 36, // You Are Too Far Away From A Bank.
        ItemLocked = 37, // Item Is Locked.
        GenericStunned = 38, // You Are Stunned
        PlayerDead = 39, // You Can'T Do That When You'Re Dead.
        ClientLockedOut = 40, // You Can'T Do That Right Now.
        InternalBagError = 41, // Internal Bag Error
        OnlyOneBolt = 42, // You Can Only Equip One Quiver.
        OnlyOneAmmo = 43, // You Can Only Equip One Ammo Pouch.
        CantWrapStackable = 44, // Stackable Items Can'T Be Wrapped.
        CantWrapEquipped = 45, // Equipped Items Can'T Be Wrapped.
        CantWrapWrapped = 46, // Wrapped Items Can'T Be Wrapped.
        CantWrapBound = 47, // Bound Items Can'T Be Wrapped.
        CantWrapUnique = 48, // Unique Items Can'T Be Wrapped.
        CantWrapBags = 49, // Bags Can'T Be Wrapped.
        LootGone = 50, // Already Looted
        InvFull = 51, // Inventory Is Full.
        BankFull = 52, // Your Bank Is Full
        VendorSoldOut = 53, // That Item Is Currently Sold Out.
        BagFull2 = 54, // That Bag Is Full.
        ItemNotFound2 = 55, // The Item Was Not Found.
        CantStack2 = 56, // This Item Cannot Stack.
        BagFull3 = 57, // That Bag Is Full.
        VendorSoldOut2 = 58, // That Item Is Currently Sold Out.
        ObjectIsBusy = 59, // That Object Is Busy.
        CantBeDisenchanted = 60, // Item Cannot Be Disenchanted
        NotInCombat = 61, // You Can'T Do That While In Combat
        NotWhileDisarmed = 62, // You Can'T Do That While Disarmed
        BagFull4 = 63, // That Bag Is Full.
        CantEquipRank = 64, // You Don'T Have The Required Rank For That Item
        CantEquipReputation = 65, // You Don'T Have The Required Reputation For That Item
        TooManySpecialBags = 66, // You Cannot Equip Another Bag Of That Type
        LootCantLootThatNow = 67, // You Can'T Loot That Item Now.
        ItemUniqueEquippable = 68, // You Cannot Equip More Than One Of Those.
        VendorMissingTurnins = 69, // You Do Not Have The Required Items For That Purchase
        NotEnoughHonorPoints = 70, // You Don'T Have Enough Honor Points
        NotEnoughArenaPoints = 71, // You Don'T Have Enough Arena Points
        ItemMaxCountSocketed = 72, // You Have The Maximum Number Of Those Gems In Your Inventory Or Socketed Into Items.
        MailBoundItem = 73, // You Can'T Mail Soulbound Items.
        InternalBagError2 = 74, // Internal Bag Error
        BagFull5 = 75, // That Bag Is Full.
        ItemMaxCountEquippedSocketed = 76, // You Have The Maximum Number Of Those Gems Socketed Into Equipped Items.
        ItemUniqueEquippableSocketed = 77, // You Cannot Socket More Than One Of Those Gems Into A Single Item.
        TooMuchGold = 78, // At Gold Limit
        NotDuringArenaMatch = 79, // You Can'T Do That While In An Arena Match
        TradeBoundItem = 80, // You Can'T Trade A Soulbound Item.
        CantEquipRating = 81, // You Don'T Have The Personal, Team, Or Battleground Rating Required To Buy That Item
        EventAutoEquipBindConfirm = 82,
        NotSameAccount = 83, // Account-Bound Items Can Only Be Given To Your Own Characters.
        EquipNone3 = 84,
        ItemMaxLimitCategoryCountExceeded = 85, // You Can Only Carry %D %S
        ItemMaxLimitCategorySocketedExceeded = 86, // You Can Only Equip %D |4item:Items In The %S Category
        ScalingStatItemLevelExceeded = 87, // Your Level Is Too High To Use That Item
        PurchaseLevelTooLow = 88, // You Must Reach Level %D To Purchase That Item.
        CantEquipNeedTalent = 89, // You Do Not Have The Required Talent To Equip That.
        ItemMaxLimitCategoryEquippedExceeded = 90, // You Can Only Equip %D |4item:Items In The %S Category
        ShapeshiftFormCannotEquip = 91, // Cannot Equip Item In This Form
        ItemInventoryFullSatchel = 92, // Your Inventory Is Full. Your Satchel Has Been Delivered To Your Mailbox.
        ScalingStatItemLevelTooLow = 93, // Your Level Is Too Low To Use That Item
        CantBuyQuantity = 94, // You Can'T Buy The Specified Quantity Of That Item.
        ItemIsBattlePayLocked = 95, // Your Purchased Item Is Still Waiting To Be Unlocked
        WrongBagType3 = 96, // That Item Doesn'T Go In That Container.
        CantUseItem = 97, // You Can'T Use That Item.
        CantBeObliterated = 98,// You Can'T Obliterate That Item
        GuildBankConjuredItem = 99,// You Cannot Store Conjured Items In The Guild Bank
        CantDoThatRightNow = 100,// You Can'T Do That Right Now.
        BagFull6 = 101,// That Bag Is Full.
    }
}
