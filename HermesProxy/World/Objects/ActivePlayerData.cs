using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class SkillInfo
    {
        public ushort[] SkillLineID { get; } = new ushort[256];
        public ushort[] SkillStep { get; } = new ushort[256];
        public ushort[] SkillRank { get; } = new ushort[256];
        public ushort[] SkillStartingRank { get; } = new ushort[256];
        public ushort[] SkillMaxRank { get; } = new ushort[256];
        public short[] SkillTempBonus { get; } = new short[256];
        public ushort[] SkillPermBonus { get; } = new ushort[256];
    }
    public class RestInfo
    {
        public uint Threshold { get; set; }
        public byte StateID { get; set; }
    }
    public class PVPInfo
    {
        public uint WeeklyPlayed { get; set; }
        public uint WeeklyWon { get; set; }
        public uint SeasonPlayed { get; set; }
        public uint SeasonWon { get; set; }
        public uint Rating { get; set; }
        public uint WeeklyBestRating { get; set; }
        public uint SeasonBestRating { get; set; }
        public uint PvpTierID { get; set; }
        public uint WeeklyBestWinPvpTierID { get; set; }
        public bool Disqualified { get; set; }
    }
    public class ActivePlayerData
    {
        public WowGuid[] InvSlots { get; } = new WowGuid[129];
        public WowGuid FarsightObject { get; set; }
        public WowGuid ComboTarget { get; set; }
        public WowGuid SummonedBattlePetGUID { get; set; }
        public uint?[] KnownTitles = new uint?[12];
        public ulong Coinage { get; set; }
        public int XP { get; set; }
        public int NextLevelXP { get; set; }
        public int TrialXP { get; set; }
        public SkillInfo Skill { get; set; }
        public int CharacterPoints { get; set; }
        public int MaxTalentTiers { get; set; }
        public uint TrackCreatureMask { get; set; }
        public uint[] TrackResourceMask { get; } = new uint[2];
        public float MainhandExpertise { get; set; }
        public float OffhandExpertise { get; set; }
        public float RangedExpertise { get; set; }
        public float CombatRatingExpertise { get; set; }
        public float BlockPercentage { get; set; }
        public float DodgePercentage { get; set; }
        public float DodgePercentageFromAttribute { get; set; }
        public float ParryPercentage { get; set; }
        public float ParryPercentageFromAttribute { get; set; }
        public float CritPercentage { get; set; }
        public float RangedCritPercentage { get; set; }
        public float OffhandCritPercentage { get; set; }
        public float?[] SpellCritPercentage = new float?[7];
        public int ShieldBlock { get; set; }
        public float Mastery { get; set; }
        public float Speed { get; set; }
        public float Avoidance { get; set; }
        public float Sturdiness { get; set; }
        public int Versatility { get; set; }
        public float VersatilityBonus { get; set; }
        public float PvpPowerDamage { get; set; }
        public float PvpPowerHealing { get; set; }
        public ulong[] ExploredZones { get; } = new ulong[240];
        public RestInfo[] RestInfo { get; } = new RestInfo[2];
        public int[] ModDamageDonePos { get; } = new int[7];
        public int[] ModDamageDoneNeg { get; } = new int[7];
        public float[] ModDamageDonePercent { get; } = new float[7];
        public int ModHealingDonePos { get; set; }
        public float ModHealingPercent { get; set; }
        public float ModHealingDonePercent { get; set; }
        public float ModPeriodicHealingDonePercent { get; set; }
        public float[] WeaponDmgMultipliers { get; } = new float[3];
        public float[] WeaponAtkSpeedMultipliers { get; } = new float[3];
        public float ModSpellPowerPercent { get; set; }
        public float ModResiliencePercent { get; set; }
        public float OverrideSpellPowerByAPPercent { get; set; }
        public float OverrideAPBySpellPowerPercent { get; set; }
        public int ModTargetResistance { get; set; }
        public int ModTargetPhysicalResistance { get; set; }
        public uint LocalFlags { get; set; }
        public byte GrantableLevels { get; set; }
        public byte MultiActionBars { get; set; }
        public byte LifetimeMaxRank { get; set; }
        public byte NumRespecs { get; set; }
        public uint AmmoID { get; set; }
        public uint PvpMedals { get; set; }
        public uint[] BuybackPrice { get; } = new uint[12];
        public uint[] BuybackTimestamp { get; } = new uint[12];
        public ushort TodayHonorableKills { get; set; }
        public ushort YesterdayHonorableKills { get; set; }
        public ushort LastWeekHonorableKills { get; set; }
        public ushort ThisWeekHonorableKills { get; set; }
        public uint ThisWeekContribution { get; set; }
        public uint LifetimeHonorableKills { get; set; }
        public uint YesterdayContribution { get; set; }
        public uint LastWeekContribution { get; set; }
        public uint LastWeekRank { get; set; }
        public int WatchedFactionIndex { get; set; }
        public int[] CombatRatings { get; } = new int[32];
        public PVPInfo[] PvpInfo { get; } = new PVPInfo[6];
        public int MaxLevel { get; set; }
        public int ScalingPlayerLevelDelta { get; set; }
        public int MaxCreatureScalingLevel { get; set; }
        public uint[] NoReagentCostMask { get; } = new uint[4];
        public int PetSpellPower { get; set; }
        public int[] ProfessionSkillLine { get; } = new int[2];
        public float UiHitModifier { get; set; }
        public float UiSpellHitModifier { get; set; }
        public int HomeRealmTimeOffset { get; set; }
        public float ModPetHaste { get; set; }
        public byte LocalRegenFlags { get; set; }
        public byte AuraVision { get; set; }
        public byte NumBackpackSlots { get; set; }
        public int OverrideSpellsID { get; set; }
        public int LfgBonusFactionID { get; set; }
        public uint LootSpecID { get; set; }
        public uint OverrideZonePVPType { get; set; }
        public uint[] BagSlotFlags { get; } = new uint[4];
        public uint[] BankBagSlotFlags { get; } = new uint[7];
        public ulong[] QuestCompleted { get; } = new ulong[875];
        public int Honor { get; set; }
        public int HonorNextLevel { get; set; }
        public uint PvPTierMaxFromWins { get; set; }
        public uint PvPLastWeeksTierMaxFromWins { get; set; }
        public bool InsertItemsLeftToRight { get; set; }
        public byte PvPRankProgress { get; set; }
    }
}
