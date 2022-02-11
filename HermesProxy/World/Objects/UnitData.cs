using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class UnitChannel
    {
        public int SpellID;
        public int SpellXSpellVisualID;
    }

    public class VisibleItem
    {
        public int ItemID;
        public ushort ItemAppearanceModID;
        public ushort ItemVisual;
    }
    public class UnitData
    {
        public WowGuid128 Charm;
        public WowGuid128 Summon;
        public WowGuid128 Critter;
        public WowGuid128 CharmedBy;
        public WowGuid128 SummonedBy;
        public WowGuid128 CreatedBy;
        public WowGuid128 DemonCreator;
        public WowGuid128 LookAtControllerTarget;
        public WowGuid128 Target;
        public WowGuid128 BattlePetCompanionGUID;
        public ulong? BattlePetDBID;
        public UnitChannel ChannelData;
        public uint? SummonedByHomeRealm;
        public byte? RaceId;
        public byte? ClassId;
        public byte? PlayerClassId;
        public byte? SexId;
        public uint? DisplayPower;
        public uint? OverrideDisplayPowerID;
        public long? Health;
        public int?[] Power = new int?[6];
        public long? MaxHealth;
        public int?[] MaxPower = new int?[6];
        public float?[] ModPowerRegen = new float?[6];
        public int? Level;
        public int? EffectiveLevel;
        public int? ContentTuningID;
        public int? ScalingLevelMin;
        public int? ScalingLevelMax;
        public int? ScalingLevelDelta;
        public int? ScalingFactionGroup;
        public int? ScalingHealthItemLevelCurveID;
        public int? ScalingDamageItemLevelCurveID;
        public int? FactionTemplate;
        public VisibleItem[] VirtualItems = new VisibleItem[3];
        public uint? Flags;
        public uint? Flags2;
        public uint? Flags3;
        public uint? AuraState;
        public uint?[] AttackRoundBaseTime = new uint?[2];
        public uint? RangedAttackRoundBaseTime;
        public float? BoundingRadius;
        public float? CombatReach;
        public int? DisplayID;
        public float? DisplayScale;
        public int? NativeDisplayID;
        public float? NativeXDisplayScale;
        public int? MountDisplayID;
        public float? MinDamage;
        public float? MaxDamage;
        public float? MinOffHandDamage;
        public float? MaxOffHandDamage;
        public byte? StandState;
        public byte? PetLoyaltyIndex;
        public byte? VisFlags;
        public byte? AnimTier;
        public uint? PetNumber;
        public uint? PetNameTimestamp;
        public uint? PetExperience;
        public uint? PetNextLevelExperience;
        public float? ModCastSpeed;
        public float? ModCastHaste;
        public float? ModHaste;
        public float? ModRangedHaste;
        public float? ModHasteRegen;
        public float? ModTimeRate;
        public int? CreatedBySpell;
        public uint?[] NpcFlags = new uint?[2];
        public int? EmoteState;
        public ushort? TrainingPointsUsed;
        public ushort? TrainingPointsTotal;
        public int?[] Stats = new int?[5];
        public int?[] StatPosBuff = new int?[5];
        public int?[] StatNegBuff = new int?[5];
        public int?[] Resistances { get; } = new int?[7];
        public int?[] ResistanceBuffModsPositive { get; } = new int?[7];
        public int?[] ResistanceBuffModsNegative { get; } = new int?[7];
        public int? BaseMana;
        public int? BaseHealth;
        public byte? SheatheState;
        public byte? PvpFlags;
        public byte? PetFlags;
        public byte? ShapeshiftForm;
        public int? AttackPower;
        public int? AttackPowerModPos;
        public int? AttackPowerModNeg;
        public float? AttackPowerMultiplier;
        public int? RangedAttackPower;
        public int? RangedAttackPowerModPos;
        public int? RangedAttackPowerModNeg;
        public float? RangedAttackPowerMultiplier;
        public int? AttackSpeedAura;
        public float? Lifesteal;
        public float? MinRangedDamage;
        public float? MaxRangedDamage;
        public int?[] PowerCostModifier = new int?[7];
        public float?[] PowerCostMultiplier = new float?[7];
        public float? MaxHealthModifier;
        public float? HoverHeight;
        public int? MinItemLevelCutoff;
        public int? MinItemLevel;
        public int? MaxItemLevel;
        public int? WildBattlePetLevel;
        public uint? BattlePetCompanionNameTimestamp;
        public int? InteractSpellID;
        public uint? StateSpellVisualID;
        public uint? StateAnimID;
        public uint? StateAnimKitID;
        public uint? StateWorldEffectsID;
        public int? ScaleDuration;
        public int? LooksLikeMountID;
        public int? LooksLikeCreatureID;
        public int? LookAtControllerID;
        public WowGuid128 GuildGUID;
    }
}
