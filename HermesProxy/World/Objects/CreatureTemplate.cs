using Framework.Collections;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects
{
    public class CreatureTemplate
    {
        public string Title;
        public string TitleAlt;
        public string CursorName;
        public int Type;
        public int Family;
        public int Classification;
        public uint PetSpellDataId;
        public CreatureDisplayStats Display = new();
        public float HpMulti;
        public float EnergyMulti;
        public bool Civilian;
        public bool Leader;
        public List<uint> QuestItems = new();
        public uint MovementInfoID;
        public int HealthScalingExpansion;
        public uint RequiredExpansion;
        public uint VignetteID;
        public int Class;
        public int DifficultyID;
        public int WidgetSetID;
        public int WidgetSetUnitConditionID;
        public uint[] Flags = new uint[2];
        public uint[] ProxyCreatureID = new uint[CreatureConst.MaxCreatureKillCredit];
        public StringArray Name = new(CreatureConst.MaxCreatureNames);
        public StringArray NameAlt = new(CreatureConst.MaxCreatureNames);
    }

    public class CreatureXDisplay
    {
        public CreatureXDisplay(uint creatureDisplayID, float displayScale, float probability)
        {
            CreatureDisplayID = creatureDisplayID;
            Scale = displayScale;
            Probability = probability;
        }

        public uint CreatureDisplayID;
        public float Scale = 1.0f;
        public float Probability = 1.0f;
    }

    public class CreatureDisplayStats
    {
        public float TotalProbability;
        public List<CreatureXDisplay> CreatureDisplay = new();
    }
}
