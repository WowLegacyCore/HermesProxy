using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class QuestTemplate
    {
        public QuestTemplate()
        {
            LogTitle = "";
            LogDescription = "";
            QuestDescription = "";
            AreaDescription = "";
            PortraitGiverText = "";
            PortraitGiverName = "";
            PortraitTurnInText = "";
            PortraitTurnInName = "";
            QuestCompletionLog = "";
        }

        public uint QuestID;
        public int QuestType; // Accepted values: 0, 1 or 2. 0 == IsAutoComplete() (skip objectives/details)
        public int QuestLevel;
        public int QuestScalingFactionGroup;
        public int QuestMaxScalingLevel;
        public uint QuestPackageID;
        public int MinLevel;
        public int QuestSortID; // zone or sort to display in quest log
        public uint QuestInfoID;
        public uint SuggestedGroupNum;
        public uint RewardNextQuest; // client will request this quest from NPC, if not 0
        public uint RewardXPDifficulty; // used for calculating rewarded experience
        public float RewardXPMultiplier = 1.0f;
        public int RewardMoney; // reward money (below max lvl)
        public uint RewardMoneyDifficulty;
        public float RewardMoneyMultiplier = 1.0f;
        public uint RewardBonusMoney;
        public uint[] RewardDisplaySpell = new uint[QuestConst.QuestRewardDisplaySpellCount]; // reward spell, this spell will be displayed (icon)
        public uint RewardSpell;
        public uint RewardHonor;
        public float RewardKillHonor;
        public int RewardArtifactXPDifficulty;
        public float RewardArtifactXPMultiplier;
        public int RewardArtifactCategoryID;
        public uint StartItem;
        public uint Flags;
        public uint FlagsEx;
        public uint FlagsEx2;
        public uint POIContinent;
        public float POIx;
        public float POIy;
        public uint POIPriority;
        public long AllowableRaces = -1;
        public string LogTitle;
        public string LogDescription;
        public string QuestDescription;
        public string AreaDescription;
        public uint RewardTitle; // new 2.4.0, player gets this title (id from CharTitles)
        public int RewardArenaPoints;
        public uint RewardSkillLineID; // reward skill id
        public uint RewardNumSkillUps; // reward skill points
        public uint PortraitGiver; // quest giver entry ?
        public uint PortraitGiverMount;
        public uint PortraitTurnIn; // quest turn in entry ?
        public string PortraitGiverText;
        public string PortraitGiverName;
        public string PortraitTurnInText;
        public string PortraitTurnInName;
        public string QuestCompletionLog;
        public uint RewardFactionFlags; // rep mask (unsure on what it does)
        public uint AcceptedSoundKitID;
        public uint CompleteSoundKitID;
        public uint AreaGroupID;
        public uint TimeAllowed;
        public int TreasurePickerID;
        public int Expansion;
        public List<QuestObjective> Objectives = new();
        public uint[] RewardItems = new uint[QuestConst.QuestRewardItemCount];
        public uint[] RewardAmount = new uint[QuestConst.QuestRewardItemCount];
        public int[] ItemDrop = new int[QuestConst.QuestItemDropCount];
        public int[] ItemDropQuantity = new int[QuestConst.QuestItemDropCount];
        public QuestInfoChoiceItem[] UnfilteredChoiceItems = new QuestInfoChoiceItem[QuestConst.QuestRewardChoicesCount];
        public uint[] RewardFactionID = new uint[QuestConst.QuestRewardReputationsCount];
        public int[] RewardFactionValue = new int[QuestConst.QuestRewardReputationsCount];
        public int[] RewardFactionOverride = new int[QuestConst.QuestRewardReputationsCount];
        public int[] RewardFactionCapIn = new int[QuestConst.QuestRewardReputationsCount];
        public uint[] RewardCurrencyID = new uint[QuestConst.QuestRewardCurrencyCount];
        public uint[] RewardCurrencyQty = new uint[QuestConst.QuestRewardCurrencyCount];
        public bool ReadyForTranslation;
    }

    public struct QuestInfoChoiceItem
    {
        public uint ItemID;
        public uint Quantity;
        public uint DisplayID;
    }

    public class QuestObjective
    {
        public static uint QuestObjectiveCounter = 1;

        public uint Id;
        public uint QuestID;
        public QuestObjectiveType Type;
        public sbyte StorageIndex;
        public int ObjectID;
        public int Amount;
        public QuestObjectiveFlags Flags;
        public uint Flags2;
        public float ProgressBarWeight;
        public string Description;
        public int[] VisualEffects = Array.Empty<int>();

        public bool IsStoringValue()
        {
            switch (Type)
            {
                case QuestObjectiveType.Monster:
                case QuestObjectiveType.Item:
                case QuestObjectiveType.GameObject:
                case QuestObjectiveType.TalkTo:
                case QuestObjectiveType.PlayerKills:
                case QuestObjectiveType.WinPvpPetBattles:
                case QuestObjectiveType.HaveCurrency:
                case QuestObjectiveType.ObtainCurrency:
                case QuestObjectiveType.IncreaseReputation:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public bool IsStoringFlag()
        {
            switch (Type)
            {
                case QuestObjectiveType.AreaTrigger:
                case QuestObjectiveType.WinPetBattleAgainstNpc:
                case QuestObjectiveType.DefeatBattlePet:
                case QuestObjectiveType.CriteriaTree:
                case QuestObjectiveType.AreaTriggerEnter:
                case QuestObjectiveType.AreaTriggerExit:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public static bool CanAlwaysBeProgressedInRaid(QuestObjectiveType type)
        {
            switch (type)
            {
                case QuestObjectiveType.Item:
                case QuestObjectiveType.Currency:
                case QuestObjectiveType.LearnSpell:
                case QuestObjectiveType.MinReputation:
                case QuestObjectiveType.MaxReputation:
                case QuestObjectiveType.Money:
                case QuestObjectiveType.HaveCurrency:
                case QuestObjectiveType.IncreaseReputation:
                    return true;
                default:
                    break;
            }
            return false;
        }
    }
}
