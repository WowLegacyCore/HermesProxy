using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class SkillInfo
    {
        public ushort?[] SkillLineID = new ushort?[256];
        public ushort?[] SkillStep = new ushort?[256];
        public ushort?[] SkillRank = new ushort?[256];
        public ushort?[] SkillStartingRank = new ushort?[256];
        public ushort?[] SkillMaxRank = new ushort?[256];
        public short?[] SkillTempBonus = new short?[256];
        public ushort?[] SkillPermBonus = new ushort?[256];
    }
    public class RestInfo
    {
        public uint? StateID;
        public uint? Threshold;
    }
    public class PVPInfo
    {
        public uint WeeklyPlayed;
        public uint WeeklyWon;
        public uint SeasonPlayed;
        public uint SeasonWon;
        public uint Rating;
        public uint WeeklyBestRating;
        public uint SeasonBestRating;
        public uint PvpTierID;
        public uint WeeklyBestWinPvpTierID;
        public uint Field_28;
        public uint Field_2C;
        public bool Disqualified;
    }
    public class ActivePlayerData
    {
        public WowGuid128[] InvSlots = new WowGuid128[23]; // equipped gear and bags
        public WowGuid128[] PackSlots = new WowGuid128[24]; // main bag 16 default + 8 vulpera racial
        public WowGuid128[] BankSlots = new WowGuid128[28]; // only 24 in vanilla
        public WowGuid128[] BankBagSlots = new WowGuid128[7]; // only 6 in vanilla
        public WowGuid128[] BuyBackSlots = new WowGuid128[12];
        public WowGuid128[] KeyringSlots = new WowGuid128[32];
        public WowGuid128 FarsightObject;
        public WowGuid128 ComboTarget;
        public WowGuid128 SummonedBattlePetGUID;
        public uint?[] KnownTitles = new uint?[12];
        public ulong? Coinage;
        public int? XP;
        public int? NextLevelXP;
        public int? TrialXP;
        public SkillInfo Skill = new SkillInfo();
        public int? CharacterPoints;
        public int? MaxTalentTiers;
        public uint? TrackCreatureMask;
        public uint?[] TrackResourceMask = new uint?[2];
        public float? MainhandExpertise;
        public float? OffhandExpertise;
        public float? RangedExpertise;
        public float? CombatRatingExpertise;
        public float? BlockPercentage;
        public float? DodgePercentage;
        public float? DodgePercentageFromAttribute;
        public float? ParryPercentage;
        public float? ParryPercentageFromAttribute;
        public float? CritPercentage;
        public float? RangedCritPercentage;
        public float? OffhandCritPercentage;
        public float?[] SpellCritPercentage = new float?[7];
        public int? ShieldBlock;
        public float? Mastery;
        public float? Speed;
        public float? Avoidance;
        public float? Sturdiness;
        public int? Versatility;
        public float? VersatilityBonus;
        public float? PvpPowerDamage;
        public float? PvpPowerHealing;
        public ulong?[] ExploredZones = new ulong?[240];
        public RestInfo[] RestInfo = new RestInfo[2];
        public int?[] ModDamageDonePos = new int?[7];
        public int?[] ModDamageDoneNeg= new int?[7];
        public float?[] ModDamageDonePercent = new float?[7];
        public int? ModHealingDonePos;
        public float? ModHealingPercent;
        public float? ModHealingDonePercent;
        public float? ModPeriodicHealingDonePercent;
        public float?[] WeaponDmgMultipliers = new float?[3];
        public float?[] WeaponAtkSpeedMultipliers = new float?[3];
        public float? ModSpellPowerPercent;
        public float? ModResiliencePercent;
        public float? OverrideSpellPowerByAPPercent;
        public float? OverrideAPBySpellPowerPercent;
        public int? ModTargetResistance;
        public int? ModTargetPhysicalResistance;
        public uint? LocalFlags;
        public byte? GrantableLevels;
        public byte? MultiActionBars;
        public byte? LifetimeMaxRank;
        public byte? NumRespecs;
        public uint? AmmoID;
        public uint? PvpMedals;
        public uint?[] BuybackPrice = new uint?[12];
        public uint?[] BuybackTimestamp = new uint?[12];
        public ushort? TodayHonorableKills;
        public ushort? TodayDishonorableKills;
        public ushort? YesterdayHonorableKills;
        public ushort? YesterdayDishonorableKills;
        public ushort? LastWeekHonorableKills;
        public ushort? LastWeekDishonorableKills;
        public ushort? ThisWeekHonorableKills;
        public ushort? ThisWeekDishonorableKills;
        public uint? ThisWeekContribution;
        public uint? LifetimeHonorableKills;
        public uint? LifetimeDishonorableKills;
        public uint? YesterdayContribution;
        public uint? LastWeekContribution;
        public uint? LastWeekRank;
        public int? WatchedFactionIndex;
        public int?[] CombatRatings { get; } = new int?[32];
        public PVPInfo[] PvpInfo { get; } = new PVPInfo[6];
        public int? MaxLevel;
        public int? ScalingPlayerLevelDelta;
        public int? MaxCreatureScalingLevel;
        public uint?[] NoReagentCostMask { get; } = new uint?[4];
        public int? PetSpellPower;
        public int?[] ProfessionSkillLine { get; } = new int?[2];
        public float? UiHitModifier;
        public float? UiSpellHitModifier;
        public int? HomeRealmTimeOffset;
        public float? ModPetHaste;
        public byte? LocalRegenFlags;
        public byte? AuraVision;
        public byte? NumBackpackSlots;
        public int? OverrideSpellsID;
        public int? LfgBonusFactionID;
        public uint? LootSpecID;
        public uint? OverrideZonePVPType;
        public uint?[] BagSlotFlags { get; } = new uint?[4];
        public uint?[] BankBagSlotFlags { get; } = new uint?[7];
        public ulong?[] QuestCompleted { get; } = new ulong?[875];
        public int? Honor;
        public int? HonorNextLevel;
        public uint? PvPTierMaxFromWins;
        public uint? PvPLastWeeksTierMaxFromWins;
        public bool? InsertItemsLeftToRight;
        public byte? PvPRankProgress;

        // Dynamic Fields
        public List<uint> SelfResSpells;
        public bool HasDailyQuestsUpdate;
    }
}
