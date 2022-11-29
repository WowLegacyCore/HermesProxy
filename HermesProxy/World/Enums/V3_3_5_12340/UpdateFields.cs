﻿namespace HermesProxy.World.Enums.V3_3_5_12340
{
    // ReSharper disable InconsistentNaming
    // 3.3.5
    public enum ObjectField
    {
        OBJECT_FIELD_GUID = 0x0000,
        OBJECT_FIELD_TYPE = 0x0002,
        OBJECT_FIELD_ENTRY = 0x0003,
        OBJECT_FIELD_SCALE_X = 0x0004,
        OBJECT_FIELD_PADDING = 0x0005,
        OBJECT_END = 0x0006
    }

    public enum ItemField
    {
        ITEM_FIELD_OWNER = ObjectField.OBJECT_END + 0x0000,
        ITEM_FIELD_CONTAINED = ObjectField.OBJECT_END + 0x0002,
        ITEM_FIELD_CREATOR = ObjectField.OBJECT_END + 0x0004,
        ITEM_FIELD_GIFTCREATOR = ObjectField.OBJECT_END + 0x0006,
        ITEM_FIELD_STACK_COUNT = ObjectField.OBJECT_END + 0x0008,
        ITEM_FIELD_DURATION = ObjectField.OBJECT_END + 0x0009,
        ITEM_FIELD_SPELL_CHARGES = ObjectField.OBJECT_END + 0x000A,
        ITEM_FIELD_FLAGS = ObjectField.OBJECT_END + 0x000F,
        ITEM_FIELD_ENCHANTMENT_1_1 = ObjectField.OBJECT_END + 0x0010,
        ITEM_FIELD_ENCHANTMENT_1_3 = ObjectField.OBJECT_END + 0x0012,
        ITEM_FIELD_ENCHANTMENT_2_1 = ObjectField.OBJECT_END + 0x0013,
        ITEM_FIELD_ENCHANTMENT_2_3 = ObjectField.OBJECT_END + 0x0015,
        ITEM_FIELD_ENCHANTMENT_3_1 = ObjectField.OBJECT_END + 0x0016,
        ITEM_FIELD_ENCHANTMENT_3_3 = ObjectField.OBJECT_END + 0x0018,
        ITEM_FIELD_ENCHANTMENT_4_1 = ObjectField.OBJECT_END + 0x0019,
        ITEM_FIELD_ENCHANTMENT_4_3 = ObjectField.OBJECT_END + 0x001B,
        ITEM_FIELD_ENCHANTMENT_5_1 = ObjectField.OBJECT_END + 0x001C,
        ITEM_FIELD_ENCHANTMENT_5_3 = ObjectField.OBJECT_END + 0x001E,
        ITEM_FIELD_ENCHANTMENT_6_1 = ObjectField.OBJECT_END + 0x001F,
        ITEM_FIELD_ENCHANTMENT_6_3 = ObjectField.OBJECT_END + 0x0021,
        ITEM_FIELD_ENCHANTMENT_7_1 = ObjectField.OBJECT_END + 0x0022,
        ITEM_FIELD_ENCHANTMENT_7_3 = ObjectField.OBJECT_END + 0x0024,
        ITEM_FIELD_ENCHANTMENT_8_1 = ObjectField.OBJECT_END + 0x0025,
        ITEM_FIELD_ENCHANTMENT_8_3 = ObjectField.OBJECT_END + 0x0027,
        ITEM_FIELD_ENCHANTMENT_9_1 = ObjectField.OBJECT_END + 0x0028,
        ITEM_FIELD_ENCHANTMENT_9_3 = ObjectField.OBJECT_END + 0x002A,
        ITEM_FIELD_ENCHANTMENT_10_1 = ObjectField.OBJECT_END + 0x002B,
        ITEM_FIELD_ENCHANTMENT_10_3 = ObjectField.OBJECT_END + 0x002D,
        ITEM_FIELD_ENCHANTMENT_11_1 = ObjectField.OBJECT_END + 0x002E,
        ITEM_FIELD_ENCHANTMENT_11_3 = ObjectField.OBJECT_END + 0x0030,
        ITEM_FIELD_ENCHANTMENT_12_1 = ObjectField.OBJECT_END + 0x0031,
        ITEM_FIELD_ENCHANTMENT_12_3 = ObjectField.OBJECT_END + 0x0033,
        ITEM_FIELD_PROPERTY_SEED = ObjectField.OBJECT_END + 0x0034,
        ITEM_FIELD_RANDOM_PROPERTIES_ID = ObjectField.OBJECT_END + 0x0035,
        ITEM_FIELD_DURABILITY = ObjectField.OBJECT_END + 0x0036,
        ITEM_FIELD_MAXDURABILITY = ObjectField.OBJECT_END + 0x0037,
        ITEM_FIELD_CREATE_PLAYED_TIME = ObjectField.OBJECT_END + 0x0038,
        ITEM_FIELD_PAD = ObjectField.OBJECT_END + 0x0039,
        ITEM_END = ObjectField.OBJECT_END + 0x003A
    }

    public enum ContainerField
    {
        CONTAINER_FIELD_NUM_SLOTS = ItemField.ITEM_END + 0x0000,
        CONTAINER_ALIGN_PAD = ItemField.ITEM_END + 0x0001,
        CONTAINER_FIELD_SLOT_1 = ItemField.ITEM_END + 0x0002,
        CONTAINER_END = ItemField.ITEM_END + 0x004A
    }

    public enum UnitField
    {
        UNIT_FIELD_CHARM = ObjectField.OBJECT_END + 0x0000,
        UNIT_FIELD_SUMMON = ObjectField.OBJECT_END + 0x0002,
        UNIT_FIELD_CRITTER = ObjectField.OBJECT_END + 0x0004,
        UNIT_FIELD_CHARMEDBY = ObjectField.OBJECT_END + 0x0006,
        UNIT_FIELD_SUMMONEDBY = ObjectField.OBJECT_END + 0x0008,
        UNIT_FIELD_CREATEDBY = ObjectField.OBJECT_END + 0x000A,
        UNIT_FIELD_TARGET = ObjectField.OBJECT_END + 0x000C,
        UNIT_FIELD_CHANNEL_OBJECT = ObjectField.OBJECT_END + 0x000E,
        UNIT_CHANNEL_SPELL = ObjectField.OBJECT_END + 0x0010,
        UNIT_FIELD_BYTES_0 = ObjectField.OBJECT_END + 0x0011,
        UNIT_FIELD_HEALTH = ObjectField.OBJECT_END + 0x0012,
        UNIT_FIELD_POWER1 = ObjectField.OBJECT_END + 0x0013,
        UNIT_FIELD_POWER2 = ObjectField.OBJECT_END + 0x0014,
        UNIT_FIELD_POWER3 = ObjectField.OBJECT_END + 0x0015,
        UNIT_FIELD_POWER4 = ObjectField.OBJECT_END + 0x0016,
        UNIT_FIELD_POWER5 = ObjectField.OBJECT_END + 0x0017,
        UNIT_FIELD_POWER6 = ObjectField.OBJECT_END + 0x0018,
        UNIT_FIELD_POWER7 = ObjectField.OBJECT_END + 0x0019,
        UNIT_FIELD_MAXHEALTH = ObjectField.OBJECT_END + 0x001A,
        UNIT_FIELD_MAXPOWER1 = ObjectField.OBJECT_END + 0x001B,
        UNIT_FIELD_MAXPOWER2 = ObjectField.OBJECT_END + 0x001C,
        UNIT_FIELD_MAXPOWER3 = ObjectField.OBJECT_END + 0x001D,
        UNIT_FIELD_MAXPOWER4 = ObjectField.OBJECT_END + 0x001E,
        UNIT_FIELD_MAXPOWER5 = ObjectField.OBJECT_END + 0x001F,
        UNIT_FIELD_MAXPOWER6 = ObjectField.OBJECT_END + 0x0020,
        UNIT_FIELD_MAXPOWER7 = ObjectField.OBJECT_END + 0x0021,
        UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER = ObjectField.OBJECT_END + 0x0022,
        UNIT_FIELD_POWER_REGEN_INTERRUPTED_FLAT_MODIFIER = ObjectField.OBJECT_END + 0x0029,
        UNIT_FIELD_LEVEL = ObjectField.OBJECT_END + 0x0030,
        UNIT_FIELD_FACTIONTEMPLATE = ObjectField.OBJECT_END + 0x0031,
        UNIT_VIRTUAL_ITEM_SLOT_ID = ObjectField.OBJECT_END + 0x0032,
        UNIT_FIELD_FLAGS = ObjectField.OBJECT_END + 0x0035,
        UNIT_FIELD_FLAGS_2 = ObjectField.OBJECT_END + 0x0036,
        UNIT_FIELD_AURASTATE = ObjectField.OBJECT_END + 0x0037,
        UNIT_FIELD_BASEATTACKTIME = ObjectField.OBJECT_END + 0x0038,
        UNIT_FIELD_UNK63 = ObjectField.OBJECT_END + 0x0039,
        UNIT_FIELD_RANGEDATTACKTIME = ObjectField.OBJECT_END + 0x003A,
        UNIT_FIELD_BOUNDINGRADIUS = ObjectField.OBJECT_END + 0x003B,
        UNIT_FIELD_COMBATREACH = ObjectField.OBJECT_END + 0x003C,
        UNIT_FIELD_DISPLAYID = ObjectField.OBJECT_END + 0x003D,
        UNIT_FIELD_NATIVEDISPLAYID = ObjectField.OBJECT_END + 0x003E,
        UNIT_FIELD_MOUNTDISPLAYID = ObjectField.OBJECT_END + 0x003F,
        UNIT_FIELD_MINDAMAGE = ObjectField.OBJECT_END + 0x0040,
        UNIT_FIELD_MAXDAMAGE = ObjectField.OBJECT_END + 0x0041,
        UNIT_FIELD_MINOFFHANDDAMAGE = ObjectField.OBJECT_END + 0x0042,
        UNIT_FIELD_MAXOFFHANDDAMAGE = ObjectField.OBJECT_END + 0x0043,
        UNIT_FIELD_BYTES_1 = ObjectField.OBJECT_END + 0x0044,
        UNIT_FIELD_PETNUMBER = ObjectField.OBJECT_END + 0x0045,
        UNIT_FIELD_PET_NAME_TIMESTAMP = ObjectField.OBJECT_END + 0x0046,
        UNIT_FIELD_PETEXPERIENCE = ObjectField.OBJECT_END + 0x0047,
        UNIT_FIELD_PETNEXTLEVELEXP = ObjectField.OBJECT_END + 0x0048,
        UNIT_DYNAMIC_FLAGS = ObjectField.OBJECT_END + 0x0049,
        UNIT_MOD_CAST_SPEED = ObjectField.OBJECT_END + 0x004A,
        UNIT_CREATED_BY_SPELL = ObjectField.OBJECT_END + 0x004B,
        UNIT_NPC_FLAGS = ObjectField.OBJECT_END + 0x004C,
        UNIT_NPC_EMOTESTATE = ObjectField.OBJECT_END + 0x004D,
        UNIT_FIELD_STAT0 = ObjectField.OBJECT_END + 0x004E,
        UNIT_FIELD_STAT1 = ObjectField.OBJECT_END + 0x004F,
        UNIT_FIELD_STAT2 = ObjectField.OBJECT_END + 0x0050,
        UNIT_FIELD_STAT3 = ObjectField.OBJECT_END + 0x0051,
        UNIT_FIELD_STAT4 = ObjectField.OBJECT_END + 0x0052,
        UNIT_FIELD_POSSTAT0 = ObjectField.OBJECT_END + 0x0053,
        UNIT_FIELD_POSSTAT1 = ObjectField.OBJECT_END + 0x0054,
        UNIT_FIELD_POSSTAT2 = ObjectField.OBJECT_END + 0x0055,
        UNIT_FIELD_POSSTAT3 = ObjectField.OBJECT_END + 0x0056,
        UNIT_FIELD_POSSTAT4 = ObjectField.OBJECT_END + 0x0057,
        UNIT_FIELD_NEGSTAT0 = ObjectField.OBJECT_END + 0x0058,
        UNIT_FIELD_NEGSTAT1 = ObjectField.OBJECT_END + 0x0059,
        UNIT_FIELD_NEGSTAT2 = ObjectField.OBJECT_END + 0x005A,
        UNIT_FIELD_NEGSTAT3 = ObjectField.OBJECT_END + 0x005B,
        UNIT_FIELD_NEGSTAT4 = ObjectField.OBJECT_END + 0x005C,
        UNIT_FIELD_RESISTANCES_ARMOR = ObjectField.OBJECT_END + 0x005D,
        UNIT_FIELD_RESISTANCES_HOLY = ObjectField.OBJECT_END + 0x005E,
        UNIT_FIELD_RESISTANCES_FIRE = ObjectField.OBJECT_END + 0x005F,
        UNIT_FIELD_RESISTANCES_NATURE = ObjectField.OBJECT_END + 0x0060,
        UNIT_FIELD_RESISTANCES_FROST = ObjectField.OBJECT_END + 0x0061,
        UNIT_FIELD_RESISTANCES_SHADOW = ObjectField.OBJECT_END + 0x0062,
        UNIT_FIELD_RESISTANCES_ARCANE = ObjectField.OBJECT_END + 0x0063,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_ARMOR = ObjectField.OBJECT_END + 0x0064,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_HOLY = ObjectField.OBJECT_END + 0x0065,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_FIRE = ObjectField.OBJECT_END + 0x0066,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_NATURE = ObjectField.OBJECT_END + 0x0067,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_FROST = ObjectField.OBJECT_END + 0x0068,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_SHADOW = ObjectField.OBJECT_END + 0x0069,
        UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE_ARCANE = ObjectField.OBJECT_END + 0x006A,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_ARMOR = ObjectField.OBJECT_END + 0x006B,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_HOLY = ObjectField.OBJECT_END + 0x006C,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_FIRE = ObjectField.OBJECT_END + 0x006D,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_NATURE = ObjectField.OBJECT_END + 0x006E,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_FROST = ObjectField.OBJECT_END + 0x006F,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_SHADOW = ObjectField.OBJECT_END + 0x0070,
        UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE_ARCANE = ObjectField.OBJECT_END + 0x0071,
        UNIT_FIELD_BASE_MANA = ObjectField.OBJECT_END + 0x0072,
        UNIT_FIELD_BASE_HEALTH = ObjectField.OBJECT_END + 0x0073,
        UNIT_FIELD_BYTES_2 = ObjectField.OBJECT_END + 0x0074,
        UNIT_FIELD_ATTACK_POWER = ObjectField.OBJECT_END + 0x0075,
        UNIT_FIELD_ATTACK_POWER_MODS = ObjectField.OBJECT_END + 0x0076,
        UNIT_FIELD_ATTACK_POWER_MULTIPLIER = ObjectField.OBJECT_END + 0x0077,
        UNIT_FIELD_RANGED_ATTACK_POWER = ObjectField.OBJECT_END + 0x0078,
        UNIT_FIELD_RANGED_ATTACK_POWER_MODS = ObjectField.OBJECT_END + 0x0079,
        UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = ObjectField.OBJECT_END + 0x007A,
        UNIT_FIELD_MINRANGEDDAMAGE = ObjectField.OBJECT_END + 0x007B,
        UNIT_FIELD_MAXRANGEDDAMAGE = ObjectField.OBJECT_END + 0x007C,
        UNIT_FIELD_POWER_COST_MODIFIER = ObjectField.OBJECT_END + 0x007D,
        UNIT_FIELD_POWER_COST_MULTIPLIER1 = ObjectField.OBJECT_END + 0x0084,
        UNIT_FIELD_POWER_COST_MULTIPLIER2 = ObjectField.OBJECT_END + 0x0085,
        UNIT_FIELD_POWER_COST_MULTIPLIER3 = ObjectField.OBJECT_END + 0x0086,
        UNIT_FIELD_POWER_COST_MULTIPLIER4 = ObjectField.OBJECT_END + 0x0087,
        UNIT_FIELD_POWER_COST_MULTIPLIER5 = ObjectField.OBJECT_END + 0x0088,
        UNIT_FIELD_POWER_COST_MULTIPLIER6 = ObjectField.OBJECT_END + 0x0089,
        UNIT_FIELD_POWER_COST_MULTIPLIER7 = ObjectField.OBJECT_END + 0x008A,
        UNIT_FIELD_MAXHEALTHMODIFIER = ObjectField.OBJECT_END + 0x008B,
        UNIT_FIELD_HOVERHEIGHT = ObjectField.OBJECT_END + 0x008C,
        UNIT_FIELD_PADDING = ObjectField.OBJECT_END + 0x008D,
        UNIT_END = ObjectField.OBJECT_END + 0x008E
    }

    public enum PlayerField
    {
        PLAYER_DUEL_ARBITER = UnitField.UNIT_END + 0x0000,
        PLAYER_FLAGS = UnitField.UNIT_END + 0x0002,
        PLAYER_GUILDID = UnitField.UNIT_END + 0x0003,
        PLAYER_GUILDRANK = UnitField.UNIT_END + 0x0004,
        PLAYER_BYTES = UnitField.UNIT_END + 0x0005,
        PLAYER_BYTES_2 = UnitField.UNIT_END + 0x0006,
        PLAYER_BYTES_3 = UnitField.UNIT_END + 0x0007,
        PLAYER_DUEL_TEAM = UnitField.UNIT_END + 0x0008,
        PLAYER_GUILD_TIMESTAMP = UnitField.UNIT_END + 0x0009,
        PLAYER_QUEST_LOG_1_1 = UnitField.UNIT_END + 0x000A,
        PLAYER_QUEST_LOG_1_2 = UnitField.UNIT_END + 0x000B,
        PLAYER_QUEST_LOG_1_3 = UnitField.UNIT_END + 0x000C,
        PLAYER_QUEST_LOG_1_4 = UnitField.UNIT_END + 0x000E,
        PLAYER_QUEST_LOG_2_1 = UnitField.UNIT_END + 0x000F,
        PLAYER_QUEST_LOG_2_2 = UnitField.UNIT_END + 0x0010,
        PLAYER_QUEST_LOG_2_3 = UnitField.UNIT_END + 0x0011,
        PLAYER_QUEST_LOG_2_5 = UnitField.UNIT_END + 0x0013,
        PLAYER_QUEST_LOG_3_1 = UnitField.UNIT_END + 0x0014,
        PLAYER_QUEST_LOG_3_2 = UnitField.UNIT_END + 0x0015,
        PLAYER_QUEST_LOG_3_3 = UnitField.UNIT_END + 0x0016,
        PLAYER_QUEST_LOG_3_5 = UnitField.UNIT_END + 0x0018,
        PLAYER_QUEST_LOG_4_1 = UnitField.UNIT_END + 0x0019,
        PLAYER_QUEST_LOG_4_2 = UnitField.UNIT_END + 0x001A,
        PLAYER_QUEST_LOG_4_3 = UnitField.UNIT_END + 0x001B,
        PLAYER_QUEST_LOG_4_5 = UnitField.UNIT_END + 0x001D,
        PLAYER_QUEST_LOG_5_1 = UnitField.UNIT_END + 0x001E,
        PLAYER_QUEST_LOG_5_2 = UnitField.UNIT_END + 0x001F,
        PLAYER_QUEST_LOG_5_3 = UnitField.UNIT_END + 0x0020,
        PLAYER_QUEST_LOG_5_5 = UnitField.UNIT_END + 0x0022,
        PLAYER_QUEST_LOG_6_1 = UnitField.UNIT_END + 0x0023,
        PLAYER_QUEST_LOG_6_2 = UnitField.UNIT_END + 0x0024,
        PLAYER_QUEST_LOG_6_3 = UnitField.UNIT_END + 0x0025,
        PLAYER_QUEST_LOG_6_5 = UnitField.UNIT_END + 0x0027,
        PLAYER_QUEST_LOG_7_1 = UnitField.UNIT_END + 0x0028,
        PLAYER_QUEST_LOG_7_2 = UnitField.UNIT_END + 0x0029,
        PLAYER_QUEST_LOG_7_3 = UnitField.UNIT_END + 0x002A,
        PLAYER_QUEST_LOG_7_5 = UnitField.UNIT_END + 0x002C,
        PLAYER_QUEST_LOG_8_1 = UnitField.UNIT_END + 0x002D,
        PLAYER_QUEST_LOG_8_2 = UnitField.UNIT_END + 0x002E,
        PLAYER_QUEST_LOG_8_3 = UnitField.UNIT_END + 0x002F,
        PLAYER_QUEST_LOG_8_5 = UnitField.UNIT_END + 0x0031,
        PLAYER_QUEST_LOG_9_1 = UnitField.UNIT_END + 0x0032,
        PLAYER_QUEST_LOG_9_2 = UnitField.UNIT_END + 0x0033,
        PLAYER_QUEST_LOG_9_3 = UnitField.UNIT_END + 0x0034,
        PLAYER_QUEST_LOG_9_5 = UnitField.UNIT_END + 0x0036,
        PLAYER_QUEST_LOG_10_1 = UnitField.UNIT_END + 0x0037,
        PLAYER_QUEST_LOG_10_2 = UnitField.UNIT_END + 0x0038,
        PLAYER_QUEST_LOG_10_3 = UnitField.UNIT_END + 0x0039,
        PLAYER_QUEST_LOG_10_5 = UnitField.UNIT_END + 0x003B,
        PLAYER_QUEST_LOG_11_1 = UnitField.UNIT_END + 0x003C,
        PLAYER_QUEST_LOG_11_2 = UnitField.UNIT_END + 0x003D,
        PLAYER_QUEST_LOG_11_3 = UnitField.UNIT_END + 0x003E,
        PLAYER_QUEST_LOG_11_5 = UnitField.UNIT_END + 0x0040,
        PLAYER_QUEST_LOG_12_1 = UnitField.UNIT_END + 0x0041,
        PLAYER_QUEST_LOG_12_2 = UnitField.UNIT_END + 0x0042,
        PLAYER_QUEST_LOG_12_3 = UnitField.UNIT_END + 0x0043,
        PLAYER_QUEST_LOG_12_5 = UnitField.UNIT_END + 0x0045,
        PLAYER_QUEST_LOG_13_1 = UnitField.UNIT_END + 0x0046,
        PLAYER_QUEST_LOG_13_2 = UnitField.UNIT_END + 0x0047,
        PLAYER_QUEST_LOG_13_3 = UnitField.UNIT_END + 0x0048,
        PLAYER_QUEST_LOG_13_5 = UnitField.UNIT_END + 0x004A,
        PLAYER_QUEST_LOG_14_1 = UnitField.UNIT_END + 0x004B,
        PLAYER_QUEST_LOG_14_2 = UnitField.UNIT_END + 0x004C,
        PLAYER_QUEST_LOG_14_3 = UnitField.UNIT_END + 0x004D,
        PLAYER_QUEST_LOG_14_5 = UnitField.UNIT_END + 0x004F,
        PLAYER_QUEST_LOG_15_1 = UnitField.UNIT_END + 0x0050,
        PLAYER_QUEST_LOG_15_2 = UnitField.UNIT_END + 0x0051,
        PLAYER_QUEST_LOG_15_3 = UnitField.UNIT_END + 0x0052,
        PLAYER_QUEST_LOG_15_5 = UnitField.UNIT_END + 0x0054,
        PLAYER_QUEST_LOG_16_1 = UnitField.UNIT_END + 0x0055,
        PLAYER_QUEST_LOG_16_2 = UnitField.UNIT_END + 0x0056,
        PLAYER_QUEST_LOG_16_3 = UnitField.UNIT_END + 0x0057,
        PLAYER_QUEST_LOG_16_5 = UnitField.UNIT_END + 0x0059,
        PLAYER_QUEST_LOG_17_1 = UnitField.UNIT_END + 0x005A,
        PLAYER_QUEST_LOG_17_2 = UnitField.UNIT_END + 0x005B,
        PLAYER_QUEST_LOG_17_3 = UnitField.UNIT_END + 0x005C,
        PLAYER_QUEST_LOG_17_5 = UnitField.UNIT_END + 0x005E,
        PLAYER_QUEST_LOG_18_1 = UnitField.UNIT_END + 0x005F,
        PLAYER_QUEST_LOG_18_2 = UnitField.UNIT_END + 0x0060,
        PLAYER_QUEST_LOG_18_3 = UnitField.UNIT_END + 0x0061,
        PLAYER_QUEST_LOG_18_5 = UnitField.UNIT_END + 0x0063,
        PLAYER_QUEST_LOG_19_1 = UnitField.UNIT_END + 0x0064,
        PLAYER_QUEST_LOG_19_2 = UnitField.UNIT_END + 0x0065,
        PLAYER_QUEST_LOG_19_3 = UnitField.UNIT_END + 0x0066,
        PLAYER_QUEST_LOG_19_5 = UnitField.UNIT_END + 0x0068,
        PLAYER_QUEST_LOG_20_1 = UnitField.UNIT_END + 0x0069,
        PLAYER_QUEST_LOG_20_2 = UnitField.UNIT_END + 0x006A,
        PLAYER_QUEST_LOG_20_3 = UnitField.UNIT_END + 0x006B,
        PLAYER_QUEST_LOG_20_5 = UnitField.UNIT_END + 0x006D,
        PLAYER_QUEST_LOG_21_1 = UnitField.UNIT_END + 0x006E,
        PLAYER_QUEST_LOG_21_2 = UnitField.UNIT_END + 0x006F,
        PLAYER_QUEST_LOG_21_3 = UnitField.UNIT_END + 0x0070,
        PLAYER_QUEST_LOG_21_5 = UnitField.UNIT_END + 0x0072,
        PLAYER_QUEST_LOG_22_1 = UnitField.UNIT_END + 0x0073,
        PLAYER_QUEST_LOG_22_2 = UnitField.UNIT_END + 0x0074,
        PLAYER_QUEST_LOG_22_3 = UnitField.UNIT_END + 0x0075,
        PLAYER_QUEST_LOG_22_5 = UnitField.UNIT_END + 0x0077,
        PLAYER_QUEST_LOG_23_1 = UnitField.UNIT_END + 0x0078,
        PLAYER_QUEST_LOG_23_2 = UnitField.UNIT_END + 0x0079,
        PLAYER_QUEST_LOG_23_3 = UnitField.UNIT_END + 0x007A,
        PLAYER_QUEST_LOG_23_5 = UnitField.UNIT_END + 0x007C,
        PLAYER_QUEST_LOG_24_1 = UnitField.UNIT_END + 0x007D,
        PLAYER_QUEST_LOG_24_2 = UnitField.UNIT_END + 0x007E,
        PLAYER_QUEST_LOG_24_3 = UnitField.UNIT_END + 0x007F,
        PLAYER_QUEST_LOG_24_5 = UnitField.UNIT_END + 0x0081,
        PLAYER_QUEST_LOG_25_1 = UnitField.UNIT_END + 0x0082,
        PLAYER_QUEST_LOG_25_2 = UnitField.UNIT_END + 0x0083,
        PLAYER_QUEST_LOG_25_3 = UnitField.UNIT_END + 0x0084,
        PLAYER_QUEST_LOG_25_5 = UnitField.UNIT_END + 0x0086,
        PLAYER_VISIBLE_ITEM_1_ENTRYID = UnitField.UNIT_END + 0x0087,
        PLAYER_VISIBLE_ITEM_1_ENCHANTMENT = UnitField.UNIT_END + 0x0088,
        PLAYER_VISIBLE_ITEM_2_ENTRYID = UnitField.UNIT_END + 0x0089,
        PLAYER_VISIBLE_ITEM_2_ENCHANTMENT = UnitField.UNIT_END + 0x008A,
        PLAYER_VISIBLE_ITEM_3_ENTRYID = UnitField.UNIT_END + 0x008B,
        PLAYER_VISIBLE_ITEM_3_ENCHANTMENT = UnitField.UNIT_END + 0x008C,
        PLAYER_VISIBLE_ITEM_4_ENTRYID = UnitField.UNIT_END + 0x008D,
        PLAYER_VISIBLE_ITEM_4_ENCHANTMENT = UnitField.UNIT_END + 0x008E,
        PLAYER_VISIBLE_ITEM_5_ENTRYID = UnitField.UNIT_END + 0x008F,
        PLAYER_VISIBLE_ITEM_5_ENCHANTMENT = UnitField.UNIT_END + 0x0090,
        PLAYER_VISIBLE_ITEM_6_ENTRYID = UnitField.UNIT_END + 0x0091,
        PLAYER_VISIBLE_ITEM_6_ENCHANTMENT = UnitField.UNIT_END + 0x0092,
        PLAYER_VISIBLE_ITEM_7_ENTRYID = UnitField.UNIT_END + 0x0093,
        PLAYER_VISIBLE_ITEM_7_ENCHANTMENT = UnitField.UNIT_END + 0x0094,
        PLAYER_VISIBLE_ITEM_8_ENTRYID = UnitField.UNIT_END + 0x0095,
        PLAYER_VISIBLE_ITEM_8_ENCHANTMENT = UnitField.UNIT_END + 0x0096,
        PLAYER_VISIBLE_ITEM_9_ENTRYID = UnitField.UNIT_END + 0x0097,
        PLAYER_VISIBLE_ITEM_9_ENCHANTMENT = UnitField.UNIT_END + 0x0098,
        PLAYER_VISIBLE_ITEM_10_ENTRYID = UnitField.UNIT_END + 0x0099,
        PLAYER_VISIBLE_ITEM_10_ENCHANTMENT = UnitField.UNIT_END + 0x009A,
        PLAYER_VISIBLE_ITEM_11_ENTRYID = UnitField.UNIT_END + 0x009B,
        PLAYER_VISIBLE_ITEM_11_ENCHANTMENT = UnitField.UNIT_END + 0x009C,
        PLAYER_VISIBLE_ITEM_12_ENTRYID = UnitField.UNIT_END + 0x009D,
        PLAYER_VISIBLE_ITEM_12_ENCHANTMENT = UnitField.UNIT_END + 0x009E,
        PLAYER_VISIBLE_ITEM_13_ENTRYID = UnitField.UNIT_END + 0x009F,
        PLAYER_VISIBLE_ITEM_13_ENCHANTMENT = UnitField.UNIT_END + 0x00A0,
        PLAYER_VISIBLE_ITEM_14_ENTRYID = UnitField.UNIT_END + 0x00A1,
        PLAYER_VISIBLE_ITEM_14_ENCHANTMENT = UnitField.UNIT_END + 0x00A2,
        PLAYER_VISIBLE_ITEM_15_ENTRYID = UnitField.UNIT_END + 0x00A3,
        PLAYER_VISIBLE_ITEM_15_ENCHANTMENT = UnitField.UNIT_END + 0x00A4,
        PLAYER_VISIBLE_ITEM_16_ENTRYID = UnitField.UNIT_END + 0x00A5,
        PLAYER_VISIBLE_ITEM_16_ENCHANTMENT = UnitField.UNIT_END + 0x00A6,
        PLAYER_VISIBLE_ITEM_17_ENTRYID = UnitField.UNIT_END + 0x00A7,
        PLAYER_VISIBLE_ITEM_17_ENCHANTMENT = UnitField.UNIT_END + 0x00A8,
        PLAYER_VISIBLE_ITEM_18_ENTRYID = UnitField.UNIT_END + 0x00A9,
        PLAYER_VISIBLE_ITEM_18_ENCHANTMENT = UnitField.UNIT_END + 0x00AA,
        PLAYER_VISIBLE_ITEM_19_ENTRYID = UnitField.UNIT_END + 0x00AB,
        PLAYER_VISIBLE_ITEM_19_ENCHANTMENT = UnitField.UNIT_END + 0x00AC,
        PLAYER_CHOSEN_TITLE = UnitField.UNIT_END + 0x00AD,
        PLAYER_FAKE_INEBRIATION = UnitField.UNIT_END + 0x00AE,
        PLAYER_FIELD_PAD_0 = UnitField.UNIT_END + 0x00AF,
        PLAYER_FIELD_INV_SLOT_HEAD = UnitField.UNIT_END + 0x00B0,
        PLAYER_FIELD_INV_SLOT_FIXME1 = UnitField.UNIT_END + 0x00B2,
        PLAYER_FIELD_INV_SLOT_FIXME2 = UnitField.UNIT_END + 0x00B4,
        PLAYER_FIELD_INV_SLOT_FIXME3 = UnitField.UNIT_END + 0x00B6,
        PLAYER_FIELD_INV_SLOT_FIXME4 = UnitField.UNIT_END + 0x00B8,
        PLAYER_FIELD_INV_SLOT_FIXME5 = UnitField.UNIT_END + 0x00BA,
        PLAYER_FIELD_INV_SLOT_FIXME6 = UnitField.UNIT_END + 0x00BC,
        PLAYER_FIELD_INV_SLOT_FIXME7 = UnitField.UNIT_END + 0x00BE,
        PLAYER_FIELD_INV_SLOT_FIXME8 = UnitField.UNIT_END + 0x00C0,
        PLAYER_FIELD_INV_SLOT_FIXME9 = UnitField.UNIT_END + 0x00C2,
        PLAYER_FIELD_INV_SLOT_FIXME10 = UnitField.UNIT_END + 0x00C4,
        PLAYER_FIELD_INV_SLOT_FIXME11 = UnitField.UNIT_END + 0x00C6,
        PLAYER_FIELD_INV_SLOT_FIXME12 = UnitField.UNIT_END + 0x00C8,
        PLAYER_FIELD_INV_SLOT_FIXME13 = UnitField.UNIT_END + 0x00CA,
        PLAYER_FIELD_INV_SLOT_FIXME14 = UnitField.UNIT_END + 0x00CC,
        PLAYER_FIELD_INV_SLOT_FIXME15 = UnitField.UNIT_END + 0x00CE,
        PLAYER_FIELD_INV_SLOT_FIXME16 = UnitField.UNIT_END + 0x00D0,
        PLAYER_FIELD_INV_SLOT_FIXME17 = UnitField.UNIT_END + 0x00D2,
        PLAYER_FIELD_INV_SLOT_FIXME18 = UnitField.UNIT_END + 0x00D4,
        PLAYER_FIELD_INV_SLOT_FIXME19 = UnitField.UNIT_END + 0x00D6,
        PLAYER_FIELD_INV_SLOT_FIXME20 = UnitField.UNIT_END + 0x00D8,
        PLAYER_FIELD_INV_SLOT_FIXME21 = UnitField.UNIT_END + 0x00DA,
        PLAYER_FIELD_INV_SLOT_FIXME22 = UnitField.UNIT_END + 0x00DC,
        PLAYER_FIELD_PACK_SLOT_1 = UnitField.UNIT_END + 0x00DE,
        PLAYER_FIELD_BANK_SLOT_1 = UnitField.UNIT_END + 0x00FE,
        PLAYER_FIELD_BANKBAG_SLOT_1 = UnitField.UNIT_END + 0x0136,
        PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = UnitField.UNIT_END + 0x0144,
        PLAYER_FIELD_KEYRING_SLOT_1 = UnitField.UNIT_END + 0x015C,
        PLAYER_FIELD_CURRENCYTOKEN_SLOT_1 = UnitField.UNIT_END + 0x019C,
        PLAYER_FARSIGHT = UnitField.UNIT_END + 0x01DC,
        PLAYER_FIELD_KNOWN_TITLES = UnitField.UNIT_END + 0x01DE,
        PLAYER_FIELD_KNOWN_TITLES1 = UnitField.UNIT_END + 0x01E0,
        PLAYER_FIELD_KNOWN_TITLES2 = UnitField.UNIT_END + 0x01E2,
        PLAYER_FIELD_KNOWN_CURRENCIES = UnitField.UNIT_END + 0x01E4,
        PLAYER_XP = UnitField.UNIT_END + 0x01E6,
        PLAYER_NEXT_LEVEL_XP = UnitField.UNIT_END + 0x01E7,
        PLAYER_SKILL_INFO_1_1 = UnitField.UNIT_END + 0x01E8,
        PLAYER_CHARACTER_POINTS1 = UnitField.UNIT_END + 0x0368,
        PLAYER_CHARACTER_POINTS2 = UnitField.UNIT_END + 0x0369,
        PLAYER_TRACK_CREATURES = UnitField.UNIT_END + 0x036A,
        PLAYER_TRACK_RESOURCES = UnitField.UNIT_END + 0x036B,
        PLAYER_BLOCK_PERCENTAGE = UnitField.UNIT_END + 0x036C,
        PLAYER_DODGE_PERCENTAGE = UnitField.UNIT_END + 0x036D,
        PLAYER_PARRY_PERCENTAGE = UnitField.UNIT_END + 0x036E,
        PLAYER_EXPERTISE = UnitField.UNIT_END + 0x036F,
        PLAYER_OFFHAND_EXPERTISE = UnitField.UNIT_END + 0x0370,
        PLAYER_CRIT_PERCENTAGE = UnitField.UNIT_END + 0x0371,
        PLAYER_RANGED_CRIT_PERCENTAGE = UnitField.UNIT_END + 0x0372,
        PLAYER_OFFHAND_CRIT_PERCENTAGE = UnitField.UNIT_END + 0x0373,
        PLAYER_SPELL_CRIT_PERCENTAGE1 = UnitField.UNIT_END + 0x0374,
        PLAYER_SPELL_CRIT_PERCENTAGE2 = UnitField.UNIT_END + 0x0375,
        PLAYER_SPELL_CRIT_PERCENTAGE3 = UnitField.UNIT_END + 0x0376,
        PLAYER_SPELL_CRIT_PERCENTAGE4 = UnitField.UNIT_END + 0x0377,
        PLAYER_SPELL_CRIT_PERCENTAGE5 = UnitField.UNIT_END + 0x0378,
        PLAYER_SPELL_CRIT_PERCENTAGE6 = UnitField.UNIT_END + 0x0379,
        PLAYER_SPELL_CRIT_PERCENTAGE7 = UnitField.UNIT_END + 0x037A,
        PLAYER_SHIELD_BLOCK = UnitField.UNIT_END + 0x037B,
        PLAYER_SHIELD_BLOCK_CRIT_PERCENTAGE = UnitField.UNIT_END + 0x037C,
        PLAYER_EXPLORED_ZONES_1 = UnitField.UNIT_END + 0x037D,
        PLAYER_REST_STATE_EXPERIENCE = UnitField.UNIT_END + 0x03FD,
        PLAYER_FIELD_COINAGE = UnitField.UNIT_END + 0x03FE,
        PLAYER_FIELD_MOD_DAMAGE_DONE_POS = UnitField.UNIT_END + 0x03FF,
        PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = UnitField.UNIT_END + 0x0406,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT1 = UnitField.UNIT_END + 0x040D,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT2 = UnitField.UNIT_END + 0x040E,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT3 = UnitField.UNIT_END + 0x040F,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT4 = UnitField.UNIT_END + 0x0410,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT5 = UnitField.UNIT_END + 0x0411,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT6 = UnitField.UNIT_END + 0x0412,
        PLAYER_FIELD_MOD_DAMAGE_DONE_PCT7 = UnitField.UNIT_END + 0x0413,
        PLAYER_FIELD_MOD_HEALING_DONE_POS = UnitField.UNIT_END + 0x0414,
        PLAYER_FIELD_MOD_HEALING_PCT = UnitField.UNIT_END + 0x0415,
        PLAYER_FIELD_MOD_HEALING_DONE_PCT = UnitField.UNIT_END + 0x0416,
        PLAYER_FIELD_MOD_TARGET_RESISTANCE = UnitField.UNIT_END + 0x0417,
        PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE = UnitField.UNIT_END + 0x0418,
        PLAYER_FIELD_BYTES = UnitField.UNIT_END + 0x0419,
        PLAYER_AMMO_ID = UnitField.UNIT_END + 0x041A,
        PLAYER_SELF_RES_SPELL = UnitField.UNIT_END + 0x041B,
        PLAYER_FIELD_PVP_MEDALS = UnitField.UNIT_END + 0x041C,
        PLAYER_FIELD_BUYBACK_PRICE_1 = UnitField.UNIT_END + 0x041D,
        PLAYER_FIELD_BUYBACK_PRICE_2 = UnitField.UNIT_END + 0x041E,
        PLAYER_FIELD_BUYBACK_PRICE_3 = UnitField.UNIT_END + 0x041F,
        PLAYER_FIELD_BUYBACK_PRICE_4 = UnitField.UNIT_END + 0x0420,
        PLAYER_FIELD_BUYBACK_PRICE_5 = UnitField.UNIT_END + 0x0421,
        PLAYER_FIELD_BUYBACK_PRICE_6 = UnitField.UNIT_END + 0x0422,
        PLAYER_FIELD_BUYBACK_PRICE_7 = UnitField.UNIT_END + 0x0423,
        PLAYER_FIELD_BUYBACK_PRICE_8 = UnitField.UNIT_END + 0x0424,
        PLAYER_FIELD_BUYBACK_PRICE_9 = UnitField.UNIT_END + 0x0425,
        PLAYER_FIELD_BUYBACK_PRICE_10 = UnitField.UNIT_END + 0x0426,
        PLAYER_FIELD_BUYBACK_PRICE_11 = UnitField.UNIT_END + 0x0427,
        PLAYER_FIELD_BUYBACK_PRICE_12 = UnitField.UNIT_END + 0x0428,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = UnitField.UNIT_END + 0x0429,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_2 = UnitField.UNIT_END + 0x042A,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_3 = UnitField.UNIT_END + 0x042B,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_4 = UnitField.UNIT_END + 0x042C,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_5 = UnitField.UNIT_END + 0x042D,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_6 = UnitField.UNIT_END + 0x042E,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_7 = UnitField.UNIT_END + 0x042F,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_8 = UnitField.UNIT_END + 0x0430,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_9 = UnitField.UNIT_END + 0x0431,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_10 = UnitField.UNIT_END + 0x0432,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_11 = UnitField.UNIT_END + 0x0433,
        PLAYER_FIELD_BUYBACK_TIMESTAMP_12 = UnitField.UNIT_END + 0x0434,
        PLAYER_FIELD_KILLS = UnitField.UNIT_END + 0x0435,
        PLAYER_FIELD_TODAY_CONTRIBUTION = UnitField.UNIT_END + 0x0436,
        PLAYER_FIELD_YESTERDAY_CONTRIBUTION = UnitField.UNIT_END + 0x0437,
        PLAYER_FIELD_LIFETIME_HONORABLE_KILLS = UnitField.UNIT_END + 0x0438,
        PLAYER_FIELD_BYTES2 = UnitField.UNIT_END + 0x0439,
        PLAYER_FIELD_WATCHED_FACTION_INDEX = UnitField.UNIT_END + 0x043A,
        PLAYER_FIELD_COMBAT_RATING_1 = UnitField.UNIT_END + 0x043B,
        PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = UnitField.UNIT_END + 0x0454,
        PLAYER_FIELD_HONOR_CURRENCY = UnitField.UNIT_END + 0x0469,
        PLAYER_FIELD_ARENA_CURRENCY = UnitField.UNIT_END + 0x046A,
        PLAYER_FIELD_MAX_LEVEL = UnitField.UNIT_END + 0x046B,
        PLAYER_FIELD_DAILY_QUESTS_1 = UnitField.UNIT_END + 0x046C,
        PLAYER_RUNE_REGEN_1 = UnitField.UNIT_END + 0x0485,
        PLAYER_RUNE_REGEN_2 = UnitField.UNIT_END + 0x0486,
        PLAYER_RUNE_REGEN_3 = UnitField.UNIT_END + 0x0487,
        PLAYER_RUNE_REGEN_4 = UnitField.UNIT_END + 0x0488,
        PLAYER_NO_REAGENT_COST_1 = UnitField.UNIT_END + 0x0489,
        PLAYER_FIELD_GLYPH_SLOTS_1 = UnitField.UNIT_END + 0x048C,
        PLAYER_FIELD_GLYPH_SLOTS_2 = UnitField.UNIT_END + 0x048D,
        PLAYER_FIELD_GLYPH_SLOTS_3 = UnitField.UNIT_END + 0x048E,
        PLAYER_FIELD_GLYPH_SLOTS_4 = UnitField.UNIT_END + 0x048F,
        PLAYER_FIELD_GLYPH_SLOTS_5 = UnitField.UNIT_END + 0x0490,
        PLAYER_FIELD_GLYPH_SLOTS_6 = UnitField.UNIT_END + 0x0491,
        PLAYER_FIELD_GLYPHS_1 = UnitField.UNIT_END + 0x0492,
        PLAYER_FIELD_GLYPHS_2 = UnitField.UNIT_END + 0x0493,
        PLAYER_FIELD_GLYPHS_3 = UnitField.UNIT_END + 0x0494,
        PLAYER_FIELD_GLYPHS_4 = UnitField.UNIT_END + 0x0495,
        PLAYER_FIELD_GLYPHS_5 = UnitField.UNIT_END + 0x0496,
        PLAYER_FIELD_GLYPHS_6 = UnitField.UNIT_END + 0x0497,
        PLAYER_GLYPHS_ENABLED = UnitField.UNIT_END + 0x0498,
        PLAYER_PET_SPELL_POWER = UnitField.UNIT_END + 0x0499,
        PLAYER_END = UnitField.UNIT_END + 0x049A
    }

    public enum GameObjectField
    {
        GAMEOBJECT_FIELD_CREATED_BY = ObjectField.OBJECT_END + 0x0000,
        GAMEOBJECT_DISPLAYID = ObjectField.OBJECT_END + 0x0002,
        GAMEOBJECT_FLAGS = ObjectField.OBJECT_END + 0x0003,
        GAMEOBJECT_PARENTROTATION = ObjectField.OBJECT_END + 0x0004,
        GAMEOBJECT_DYNAMIC = ObjectField.OBJECT_END + 0x0008,
        GAMEOBJECT_FACTION = ObjectField.OBJECT_END + 0x0009,
        GAMEOBJECT_LEVEL = ObjectField.OBJECT_END + 0x000A,
        GAMEOBJECT_BYTES_1 = ObjectField.OBJECT_END + 0x000B,
        GAMEOBJECT_END = ObjectField.OBJECT_END + 0x000C
    }

    public enum DynamicObjectField
    {
        DYNAMICOBJECT_CASTER = ObjectField.OBJECT_END + 0x0000,
        DYNAMICOBJECT_BYTES = ObjectField.OBJECT_END + 0x0002,
        DYNAMICOBJECT_SPELLID = ObjectField.OBJECT_END + 0x0003,
        DYNAMICOBJECT_RADIUS = ObjectField.OBJECT_END + 0x0004,
        DYNAMICOBJECT_CASTTIME = ObjectField.OBJECT_END + 0x0005,
        DYNAMICOBJECT_END = ObjectField.OBJECT_END + 0x0006
    }

    public enum CorpseField
    {
        CORPSE_FIELD_OWNER = ObjectField.OBJECT_END + 0x0000,
        CORPSE_FIELD_PARTY = ObjectField.OBJECT_END + 0x0002,
        CORPSE_FIELD_DISPLAY_ID = ObjectField.OBJECT_END + 0x0004,
        CORPSE_FIELD_ITEM = ObjectField.OBJECT_END + 0x0005,
        CORPSE_FIELD_BYTES_1 = ObjectField.OBJECT_END + 0x0018,
        CORPSE_FIELD_BYTES_2 = ObjectField.OBJECT_END + 0x0019,
        CORPSE_FIELD_GUILD = ObjectField.OBJECT_END + 0x001A,
        CORPSE_FIELD_FLAGS = ObjectField.OBJECT_END + 0x001B,
        CORPSE_FIELD_DYNAMIC_FLAGS = ObjectField.OBJECT_END + 0x001C,
        CORPSE_FIELD_PAD = ObjectField.OBJECT_END + 0x001D,
        CORPSE_END = ObjectField.OBJECT_END + 0x001E
    }
    // ReSharper restore InconsistentNaming
}
