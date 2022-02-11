using HermesProxy.World.Enums.V2_5_2_39570;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects.Version.V2_5_2_39570
{
    public class ObjectUpdateBuilder
    {
        public ObjectUpdateBuilder(ObjectUpdate updateData)
        {
            m_updateData = updateData;

            Enums.ObjectType objectType = updateData.Guid.GetObjectType();
            if (updateData.CreateData != null)
            {
                objectType = updateData.CreateData.ObjectType;
                if (updateData.CreateData.ThisIsYou)
                    objectType = Enums.ObjectType.ActivePlayer;
            }
            m_objectType = ObjectTypeConverter.ConvertToBCC(objectType);

            int size;
            switch (m_objectType)
            {
                case Enums.ObjectTypeBCC.Item:
                    size = (int)ItemField.ITEM_END;
                    break;
                case Enums.ObjectTypeBCC.Container:
                    size = (int)ContainerField.CONTAINER_END;
                    break;
                case Enums.ObjectTypeBCC.Unit:
                    size = (int)UnitField.UNIT_END;
                    break;
                case Enums.ObjectTypeBCC.Player:
                    size = (int)PlayerField.PLAYER_END;
                    break;
                case Enums.ObjectTypeBCC.ActivePlayer:
                    size = (int)ActivePlayerField.ACTIVE_PLAYER_END;
                    break;
                case Enums.ObjectTypeBCC.GameObject:
                    size = (int)GameObjectField.GAMEOBJECT_END;
                    break;
                case Enums.ObjectTypeBCC.DynamicObject:
                    size = (int)DynamicObjectField.DYNAMICOBJECT_END;
                    break;
                case Enums.ObjectTypeBCC.Corpse:
                    size = (int)CorpseField.CORPSE_END;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported object type!");
            }

            m_fields = new UpdateFieldsArray(size);
        }

        protected ObjectUpdate m_updateData;
        protected UpdateFieldsArray m_fields;
        protected Enums.ObjectTypeBCC m_objectType;

        public void WriteValuesToArray()
        {
            ObjectData objectData = m_updateData.ObjectData;
            if (objectData.Guid != null)
                m_fields.SetUpdateField<WowGuid128>(ObjectField.OBJECT_FIELD_GUID, objectData.Guid);
            if (objectData.EntryID != null)
                m_fields.SetUpdateField<int>(ObjectField.OBJECT_FIELD_ENTRY, (int)objectData.EntryID);
            if (objectData.DynamicFlags != null)
                m_fields.SetUpdateField<uint>(ObjectField.OBJECT_DYNAMIC_FLAGS, (uint)objectData.DynamicFlags);
            if (objectData.Scale != null)
                m_fields.SetUpdateField<float>(ObjectField.OBJECT_DYNAMIC_FLAGS, (float)objectData.Scale);

            UnitData unitData = m_updateData.UnitData;
            if (unitData.Charm != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_CHARM, unitData.Charm);
            if (unitData.Summon != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_SUMMON, unitData.Summon);
            if (unitData.Critter != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_CRITTER, unitData.Critter);
            if (unitData.CharmedBy != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_CHARMEDBY, unitData.CharmedBy);
            if (unitData.SummonedBy != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_SUMMONEDBY, unitData.SummonedBy);
            if (unitData.CreatedBy != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_CREATEDBY, unitData.CreatedBy);
            if (unitData.DemonCreator != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_DEMON_CREATOR, unitData.DemonCreator);
            if (unitData.LookAtControllerTarget != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_LOOK_AT_CONTROLLER_TARGET, unitData.LookAtControllerTarget);
            if (unitData.Target != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_TARGET, unitData.Target);
            if (unitData.BattlePetCompanionGUID != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_BATTLE_PET_COMPANION_GUID, unitData.BattlePetCompanionGUID);
            if (unitData.BattlePetDBID != null)
                m_fields.SetUpdateField<ulong>(UnitField.UNIT_FIELD_BATTLE_PET_DB_ID, (ulong)unitData.BattlePetDBID);
            if (unitData.ChannelData != null)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_CHANNEL_DATA;
                m_fields.SetUpdateField<int>(startIndex, (int)unitData.ChannelData.SpellID);
                m_fields.SetUpdateField<int>(startIndex + 1, (int)unitData.ChannelData.SpellXSpellVisualID);
            }
            if (unitData.SummonedByHomeRealm != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_SUMMONED_BY_HOME_REALM, (uint)unitData.SummonedByHomeRealm);
            if (unitData.RaceId != null || unitData.ClassId != null || unitData.PlayerClassId != null || unitData.SexId != null)
            {
                if (unitData.RaceId != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_0, (byte)unitData.RaceId, 0);
                if (unitData.ClassId != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_0, (byte)unitData.ClassId, 1);
                if (unitData.PlayerClassId != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_0, (byte)unitData.PlayerClassId, 2);
                if (unitData.SexId != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_0, (byte)unitData.SexId, 3);
            }
            if (unitData.DisplayPower != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_DISPLAY_POWER, (uint)unitData.DisplayPower);
            if (unitData.OverrideDisplayPowerID != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_OVERRIDE_DISPLAY_POWER_ID, (uint)unitData.OverrideDisplayPowerID);
            if (unitData.Health != null)
                m_fields.SetUpdateField<long>(UnitField.UNIT_FIELD_HEALTH, (long)unitData.Health);
            for (int i = 0; i < 6; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_POWER;
                if (unitData.Power[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.Power[i]);
            }
            if (unitData.MaxHealth != null)
                m_fields.SetUpdateField<long>(UnitField.UNIT_FIELD_MAXHEALTH, (long)unitData.MaxHealth);
            for (int i = 0; i < 6; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_MAXPOWER;
                if (unitData.MaxPower[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.MaxPower[i]);
            }
            for (int i = 0; i < 6; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_MOD_POWER_REGEN;
                if (unitData.ModPowerRegen[i] != null)
                    m_fields.SetUpdateField<float>(startIndex + i, (float)unitData.ModPowerRegen[i]);
            }
            if (unitData.Level != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_LEVEL, (int)unitData.Level);
            if (unitData.EffectiveLevel != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_EFFECTIVE_LEVEL, (int)unitData.EffectiveLevel);
            if (unitData.ContentTuningID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_CONTENT_TUNING_ID, (int)unitData.ContentTuningID);
            if (unitData.ScalingLevelMin != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALING_LEVEL_MIN, (int)unitData.ScalingLevelMin);
            if (unitData.ScalingLevelMax != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALING_LEVEL_MAX, (int)unitData.ScalingLevelMax);
            if (unitData.ScalingLevelDelta != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALING_LEVEL_DELTA, (int)unitData.ScalingLevelDelta);
            if (unitData.ScalingFactionGroup != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALING_FACTION_GROUP, (int)unitData.ScalingFactionGroup);
            if (unitData.ScalingHealthItemLevelCurveID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALING_HEALTH_ITEM_LEVEL_CURVE_ID, (int)unitData.ScalingHealthItemLevelCurveID);
            if (unitData.ScalingDamageItemLevelCurveID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALING_DAMAGE_ITEM_LEVEL_CURVE_ID, (int)unitData.ScalingDamageItemLevelCurveID);
            if (unitData.FactionTemplate != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_FACTIONTEMPLATE, (int)unitData.FactionTemplate);
            for (int i = 0; i < 3; i++)
            {
                int startIndex = (int)UnitField.UNIT_VIRTUAL_ITEM_SLOT_ID;
                int sizePerEntry = 2;
                if (unitData.VirtualItems[i] != null)
                {
                    m_fields.SetUpdateField<int>(startIndex + i * sizePerEntry, (int)unitData.VirtualItems[i].ItemID);
                    m_fields.SetUpdateField<ushort>(startIndex + i * sizePerEntry + 1, (ushort)unitData.VirtualItems[i].ItemAppearanceModID, 0);
                    m_fields.SetUpdateField<ushort>(startIndex + i * sizePerEntry + 1, (ushort)unitData.VirtualItems[i].ItemVisual, 1);
                }
            }
            if (unitData.Flags != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_FLAGS, (uint)unitData.Flags);
            if (unitData.Flags2 != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_FLAGS_2, (uint)unitData.Flags2);
            if (unitData.Flags3 != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_FLAGS_3, (uint)unitData.Flags3);
            if (unitData.AuraState != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_AURASTATE, (uint)unitData.AuraState);
            for (int i = 0; i < 2; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_BASEATTACKTIME;
                if (unitData.AttackRoundBaseTime[i] != null)
                    m_fields.SetUpdateField<uint>(startIndex + i, (uint)unitData.AttackRoundBaseTime[i]);
            }
            if (unitData.RangedAttackRoundBaseTime != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_RANGEDATTACKTIME, (uint)unitData.RangedAttackRoundBaseTime);
            if (unitData.BoundingRadius != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_BOUNDINGRADIUS, (float)unitData.BoundingRadius);
            if (unitData.CombatReach != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_COMBATREACH, (float)unitData.CombatReach);
            if (unitData.DisplayID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_DISPLAYID, (int)unitData.DisplayID);
            if (unitData.DisplayScale != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_DISPLAYSCALE, (float)unitData.DisplayScale);
            if (unitData.NativeDisplayID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_NATIVEDISPLAYID, (int)unitData.NativeDisplayID);
            if (unitData.NativeXDisplayScale != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_NATIVEXDISPLAYSCALE, (float)unitData.NativeXDisplayScale);
            if (unitData.MountDisplayID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_MOUNTDISPLAYID, (int)unitData.MountDisplayID);
            if (unitData.MinDamage != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MINDAMAGE, (float)unitData.MinDamage);
            if (unitData.MaxDamage != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MAXDAMAGE, (float)unitData.MaxDamage);
            if (unitData.MinOffHandDamage != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MINOFFHANDDAMAGE, (float)unitData.MinOffHandDamage);
            if (unitData.MaxOffHandDamage != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MAXOFFHANDDAMAGE, (float)unitData.MaxOffHandDamage);
            if (unitData.StandState != null || unitData.PetLoyaltyIndex != null || unitData.VisFlags != null || unitData.AnimTier != null)
            {
                if (unitData.StandState != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_1, (byte)unitData.StandState, 0);
                if (unitData.PetLoyaltyIndex != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_1, (byte)unitData.PetLoyaltyIndex, 1);
                if (unitData.VisFlags != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_1, (byte)unitData.VisFlags, 2);
                if (unitData.AnimTier != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_1, (byte)unitData.AnimTier, 3);
            }
            if (unitData.PetNumber != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_PETNUMBER, (uint)unitData.PetNumber);
            if (unitData.PetNameTimestamp != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_PET_NAME_TIMESTAMP, (uint)unitData.PetNameTimestamp);
            if (unitData.PetExperience != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_PETEXPERIENCE, (uint)unitData.PetExperience);
            if (unitData.PetNextLevelExperience != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_PETNEXTLEVELEXPERIENCE, (uint)unitData.PetNextLevelExperience);
            if (unitData.ModCastSpeed != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_MOD_CAST_SPEED, (float)unitData.ModCastSpeed);
            if (unitData.ModCastHaste != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_MOD_CAST_HASTE, (float)unitData.ModCastHaste);
            if (unitData.ModHaste != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MOD_HASTE, (float)unitData.ModHaste);
            if (unitData.ModRangedHaste != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MOD_RANGED_HASTE, (float)unitData.ModRangedHaste);
            if (unitData.ModHasteRegen != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MOD_HASTE_REGEN, (float)unitData.ModHasteRegen);
            if (unitData.ModTimeRate != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MOD_TIME_RATE, (float)unitData.ModTimeRate);
            if (unitData.CreatedBySpell != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_CREATED_BY_SPELL, (int)unitData.CreatedBySpell);
            for (int i = 0; i < 2; i++)
            {
                int startIndex = (int)UnitField.UNIT_NPC_FLAGS;
                if (unitData.NpcFlags[i] != null)
                    m_fields.SetUpdateField<uint>(startIndex + i, (uint)unitData.NpcFlags[i]);
            }
            if (unitData.EmoteState != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_NPC_EMOTESTATE, (int)unitData.EmoteState);
            if (unitData.TrainingPointsUsed != null && unitData.TrainingPointsTotal != null)
            {
                m_fields.SetUpdateField<ushort>(UnitField.UNIT_FIELD_TRAINING_POINTS_TOTAL, (ushort)unitData.TrainingPointsUsed, 0);
                m_fields.SetUpdateField<ushort>(UnitField.UNIT_FIELD_TRAINING_POINTS_TOTAL, (ushort)unitData.TrainingPointsTotal, 1);
            }
            for (int i = 0; i < 5; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_STAT;
                if (unitData.Stats[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.Stats[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_POSSTAT;
                if (unitData.StatPosBuff[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.StatPosBuff[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_NEGSTAT;
                if (unitData.StatNegBuff[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.StatNegBuff[i]);
            }
            for (int i = 0; i < 7; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_RESISTANCES;
                if (unitData.Resistances[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.Resistances[i]);
            }
            for (int i = 0; i < 7; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE;
                if (unitData.ResistanceBuffModsPositive[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.ResistanceBuffModsPositive[i]);
            }
            for (int i = 0; i < 7; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE;
                if (unitData.ResistanceBuffModsNegative[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.ResistanceBuffModsNegative[i]);
            }
            if (unitData.BaseMana != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_BASE_MANA, (int)unitData.BaseMana);
            if (unitData.BaseHealth != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_BASE_HEALTH, (int)unitData.BaseHealth);
            if (unitData.SheatheState != null || unitData.PvpFlags != null || unitData.PetFlags != null || unitData.ShapeshiftForm != null)
            {
                if (unitData.SheatheState != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_2, (byte)unitData.SheatheState, 0);
                if (unitData.PvpFlags != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_2, (byte)unitData.PvpFlags, 1);
                if (unitData.PetFlags != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_2, (byte)unitData.PetFlags, 2);
                if (unitData.ShapeshiftForm != null)
                    m_fields.SetUpdateField<byte>(UnitField.UNIT_FIELD_BYTES_2, (byte)unitData.ShapeshiftForm, 3);
            }
            if (unitData.AttackPower != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_ATTACK_POWER, (int)unitData.AttackPower);
            if (unitData.AttackPowerModPos != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_ATTACK_POWER_MOD_POS, (int)unitData.AttackPowerModPos);
            if (unitData.AttackPowerModNeg != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_ATTACK_POWER_MOD_NEG, (int)unitData.AttackPowerModNeg);
            if (unitData.AttackPowerMultiplier != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_ATTACK_POWER_MULTIPLIER, (float)unitData.AttackPowerMultiplier);
            if (unitData.RangedAttackPower != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_RANGED_ATTACK_POWER, (int)unitData.RangedAttackPower);
            if (unitData.RangedAttackPowerModPos != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_RANGED_ATTACK_POWER_MOD_POS, (int)unitData.RangedAttackPowerModPos);
            if (unitData.RangedAttackPowerModNeg != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_RANGED_ATTACK_POWER_MOD_NEG, (int)unitData.RangedAttackPowerModNeg);
            if (unitData.RangedAttackPowerMultiplier != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER, (float)unitData.RangedAttackPowerMultiplier);
            if (unitData.AttackSpeedAura != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_ATTACK_SPEED_AURA, (int)unitData.AttackSpeedAura);
            if (unitData.Lifesteal != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_LIFESTEAL, (float)unitData.Lifesteal);
            if (unitData.MinRangedDamage != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MINRANGEDDAMAGE, (float)unitData.MinRangedDamage);
            if (unitData.MaxRangedDamage != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MAXRANGEDDAMAGE, (float)unitData.MaxRangedDamage);
            for (int i = 0; i < 7; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_POWER_COST_MODIFIER;
                if (unitData.PowerCostModifier[i] != null)
                    m_fields.SetUpdateField<int>(startIndex + i, (int)unitData.PowerCostModifier[i]);
            }
            for (int i = 0; i < 7; i++)
            {
                int startIndex = (int)UnitField.UNIT_FIELD_POWER_COST_MULTIPLIER;
                if (unitData.PowerCostMultiplier[i] != null)
                    m_fields.SetUpdateField<float>(startIndex + i, (float)unitData.PowerCostMultiplier[i]);
            }
            if (unitData.MaxHealthModifier != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_MAXHEALTHMODIFIER, (float)unitData.MaxHealthModifier);
            if (unitData.HoverHeight != null)
                m_fields.SetUpdateField<float>(UnitField.UNIT_FIELD_HOVERHEIGHT, (float)unitData.HoverHeight);
            if (unitData.MinItemLevelCutoff != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_MIN_ITEM_LEVEL_CUTOFF, (int)unitData.MinItemLevelCutoff);
            if (unitData.MinItemLevel != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_MIN_ITEM_LEVEL, (int)unitData.MinItemLevel);
            if (unitData.MaxItemLevel != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_MAXITEMLEVEL, (int)unitData.MaxItemLevel);
            if (unitData.WildBattlePetLevel != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_WILD_BATTLE_PET_LEVEL, (int)unitData.WildBattlePetLevel);
            if (unitData.BattlePetCompanionNameTimestamp != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_BATTLEPET_COMPANION_NAME_TIMESTAMP, (uint)unitData.BattlePetCompanionNameTimestamp);
            if (unitData.InteractSpellID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_INTERACT_SPELL_ID, (int)unitData.InteractSpellID);
            if (unitData.StateSpellVisualID != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_STATE_SPELL_VISUAL_ID, (uint)unitData.StateSpellVisualID);
            if (unitData.StateAnimID != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_STATE_ANIM_ID, (uint)unitData.StateAnimID);
            if (unitData.StateAnimKitID != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_STATE_ANIM_KIT_ID, (uint)unitData.StateAnimKitID);
            if (unitData.StateWorldEffectsID != null)
                m_fields.SetUpdateField<uint>(UnitField.UNIT_FIELD_STATE_WORLD_EFFECT_ID, (uint)unitData.StateWorldEffectsID);
            if (unitData.ScaleDuration != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_SCALE_DURATION, (int)unitData.ScaleDuration);
            if (unitData.LooksLikeMountID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_LOOKS_LIKE_MOUNT_ID, (int)unitData.LooksLikeMountID);
            if (unitData.LooksLikeCreatureID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_LOOKS_LIKE_CREATURE_ID, (int)unitData.LooksLikeCreatureID);
            if (unitData.LookAtControllerID != null)
                m_fields.SetUpdateField<int>(UnitField.UNIT_FIELD_LOOK_AT_CONTROLLER_ID, (int)unitData.LookAtControllerID);
            if (unitData.GuildGUID != null)
                m_fields.SetUpdateField<WowGuid128>(UnitField.UNIT_FIELD_GUILD_GUID, unitData.GuildGUID);

            PlayerData playerData = m_updateData.PlayerData;
            if (playerData.DuelArbiter != null)
                m_fields.SetUpdateField<WowGuid128>(PlayerField.PLAYER_DUEL_ARBITER, playerData.DuelArbiter);
            if (playerData.WowAccount != null)
                m_fields.SetUpdateField<WowGuid128>(PlayerField.PLAYER_WOW_ACCOUNT, playerData.WowAccount);
            if (playerData.LootTargetGUID != null)
                m_fields.SetUpdateField<WowGuid128>(PlayerField.PLAYER_LOOT_TARGET_GUID, playerData.LootTargetGUID);
            if (playerData.PlayerFlags != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_FLAGS, (uint)playerData.PlayerFlags);
            if (playerData.PlayerFlagsEx != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_FLAGS_EX, (uint)playerData.PlayerFlagsEx);
            if (playerData.GuildRankID != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_GUILDRANK, (uint)playerData.GuildRankID);
            if (playerData.GuildDeleteDate != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_GUILDDELETE_DATE, (uint)playerData.GuildDeleteDate);
            if (playerData.GuildLevel != null)
                m_fields.SetUpdateField<int>(PlayerField.PLAYER_GUILDLEVEL, (int)playerData.GuildLevel);
            if (playerData.PartyType != null || playerData.NumBankSlots != null || playerData.NativeSex != null || playerData.Inebriation != null)
            {
                if (playerData.PartyType != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES, (byte)playerData.PartyType, 0);
                if (playerData.NumBankSlots != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES, (byte)playerData.NumBankSlots, 1);
                if (playerData.NativeSex != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES, (byte)playerData.NativeSex, 2);
                if (playerData.Inebriation != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES, (byte)playerData.Inebriation, 3);
            }
            if (playerData.PvpTitle != null || playerData.ArenaFaction != null || playerData.PvPRank != null)
            {
                if (playerData.PvpTitle != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES_2, (byte)playerData.PvpTitle, 0);
                if (playerData.ArenaFaction != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES_2, (byte)playerData.ArenaFaction, 1);
                if (playerData.PvPRank != null)
                    m_fields.SetUpdateField<byte>(PlayerField.PLAYER_BYTES_2, (byte)playerData.PvPRank, 2);
            }
            if (playerData.DuelTeam != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_DUEL_TEAM, (uint)playerData.DuelTeam);
            if (playerData.GuildTimeStamp != null)
                m_fields.SetUpdateField<int>(PlayerField.PLAYER_GUILD_TIMESTAMP, (int)playerData.GuildTimeStamp);
            for (int i = 0; i < 25; i++)
            {
                int startIndex = (int)PlayerField.PLAYER_QUEST_LOG;
                int sizePerEntry = 16;
                if (playerData.QuestLog[i] != null)
                {
                    if (playerData.QuestLog[i].QuestID != null)
                        m_fields.SetUpdateField<int>(startIndex + i * sizePerEntry, (int)playerData.QuestLog[i].QuestID);
                    if (playerData.QuestLog[i].StateFlags != null)
                        m_fields.SetUpdateField<uint>(startIndex + i * sizePerEntry + 1, (uint)playerData.QuestLog[i].StateFlags);
                    if (playerData.QuestLog[i].EndTime != null)
                        m_fields.SetUpdateField<uint>(startIndex + i * sizePerEntry + 2, (uint)playerData.QuestLog[i].EndTime);
                    if (playerData.QuestLog[i].AcceptTime != null)
                        m_fields.SetUpdateField<uint>(startIndex + i * sizePerEntry + 3, (uint)playerData.QuestLog[i].AcceptTime);
                    for (int j = 0; j < 24; j++)
                    {
                        if (playerData.QuestLog[i].ObjectiveProgress[j] != null)
                            m_fields.SetUpdateField<ushort>(startIndex + i * sizePerEntry + 4 + j / 2, (ushort)playerData.QuestLog[i].ObjectiveProgress[j], (byte)(j & 1));
                    }
                }
            }
            for (int i = 0; i < 19; i++)
            {
                int startIndex = (int)PlayerField.PLAYER_VISIBLE_ITEM;
                int sizePerEntry = 2;
                if (playerData.VisibleItems[i] != null)
                {
                    m_fields.SetUpdateField<int>(startIndex + i * sizePerEntry, (int)playerData.VisibleItems[i].ItemID);
                    m_fields.SetUpdateField<ushort>(startIndex + i * sizePerEntry + 1, (ushort)playerData.VisibleItems[i].ItemAppearanceModID, 0);
                    m_fields.SetUpdateField<ushort>(startIndex + i * sizePerEntry + 1, (ushort)playerData.VisibleItems[i].ItemVisual, 1);
                }
            }
            if (playerData.ChosenTitle != null)
                m_fields.SetUpdateField<int>(PlayerField.PLAYER_CHOSEN_TITLE, (int)playerData.ChosenTitle);
            if (playerData.FakeInebriation != null)
                m_fields.SetUpdateField<int>(PlayerField.PLAYER_FAKE_INEBRIATION, (int)playerData.FakeInebriation);
            if (playerData.VirtualPlayerRealm != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_FIELD_VIRTUAL_PLAYER_REALM, (uint)playerData.VirtualPlayerRealm);
            if (playerData.CurrentSpecID != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_FIELD_CURRENT_SPEC_ID, (uint)playerData.CurrentSpecID);
            if (playerData.TaxiMountAnimKitID != null)
                m_fields.SetUpdateField<int>(PlayerField.PLAYER_FIELD_TAXI_MOUNT_ANIM_KIT_ID, (int)playerData.TaxiMountAnimKitID);
            for (int i = 0; i < 6; i++)
            {
                int startIndex = (int)PlayerField.PLAYER_FIELD_AVG_ITEM_LEVEL;
                if (playerData.AvgItemLevel[i] != null)
                    m_fields.SetUpdateField<float>(startIndex + i, (float)playerData.AvgItemLevel[i]);
            }
            if (playerData.CurrentBattlePetBreedQuality != null)
                m_fields.SetUpdateField<uint>(PlayerField.PLAYER_FIELD_CURRENT_BATTLE_PET_BREED_QUALITY, (uint)playerData.CurrentBattlePetBreedQuality);
            if (playerData.HonorLevel != null)
                m_fields.SetUpdateField<int>(PlayerField.PLAYER_FIELD_HONOR_LEVEL, (int)playerData.HonorLevel);
            for (int i = 0; i < 36; i++)
            {
                int startIndex = (int)PlayerField.PLAYER_FIELD_CUSTOMIZATION_CHOICES;
                int sizePerEntry = 2;
                if (playerData.Customizations[i] != null)
                {
                    m_fields.SetUpdateField<uint>(startIndex + i * sizePerEntry, (uint)playerData.Customizations[i].ChrCustomizationOptionID);
                    m_fields.SetUpdateField<uint>(startIndex + i * sizePerEntry + 1, (uint)playerData.Customizations[i].ChrCustomizationChoiceID);
                }
            }
        }
    }
}
