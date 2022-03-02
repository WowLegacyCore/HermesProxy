using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class QuestConst
    {
        public const int MaxQuestLogSize = 25;
        public const int MaxQuestCounts = 24;

        public const int QuestItemDropCount = 4;
        public const int QuestRewardChoicesCount = 6;
        public const int QuestRewardItemCount = 4;
        public const int QuestDeplinkCount = 10;
        public const int QuestRewardReputationsCount = 5;
        public const int QuestEmoteCount = 4;
        public const int QuestRewardCurrencyCount = 4;
        public const int QuestRewardDisplaySpellCount = 3;
    }

    public enum QuestGiverStatusVanilla : uint
    {
        None                 = 0,
        Unavailable          = 1,
        LowLevelAvailable    = 2,
        Incomplete           = 3,
        RewardRep            = 4,
        Available            = 5,
        Reward               = 6,
        Reward2              = 7,
    }

    public enum QuestGiverStatusTBC : uint
    {
        None                 = 0,
        Unavailable          = 1,
        LowLevelAvailable    = 2,
        Incomplete           = 3,
        RewardRep            = 4,
        AvailableRep         = 5,
        Available            = 6,
        Reward2              = 7,
        Reward               = 8,
    }

    public enum QuestGiverStatusWotLK : uint
    {
        None                 = 0,
        Unavailable          = 1,
        LowLevelAvailable    = 2,
        LowLevelRewardRep    = 3,
        LowLevelAvailableRep = 4,
        Incomplete           = 5,
        RewardRep            = 6,
        AvailableRep         = 7,
        Available            = 8,
        Reward2              = 9,
        Reward               = 10
    }

    [Flags]
    public enum QuestGiverStatusModern : uint
    {
        None                      = 0x000000,
        Unavailable               = 0x000002,
        LowLevelAvailable         = 0x000004,
        LowLevelRewardRep         = 0x000008,
        LowLevelAvailableRep      = 0x000010,
        Incomplete                = 0x000020,
        IncompleteJourney         = 0x000040,
        IncompleteCovenantCalling = 0x000080,
        RewardRep                 = 0x000100,
        AvailableRep              = 0x000200,
        Available                 = 0x000400,
        Reward2                   = 0x000800,
        Reward                    = 0x001000,
        AvailableLegendaryQuest   = 0x002000,
        Reward2Legendary          = 0x004000,
        RewardLegendary           = 0x008000,
        AvailableJourney          = 0x010000,
        Reward2Journey            = 0x020000,
        RewardJourney             = 0x040000,
        AvailableCovenantCalling  = 0x080000,
        Reward2CovenantCalling    = 0x100000,
        RewardCovenantCalling     = 0x200000,
    }

    public enum QuestObjectiveType
    {
        Monster = 0,
        Item = 1,
        GameObject = 2,
        TalkTo = 3,
        Currency = 4,
        LearnSpell = 5,
        MinReputation = 6,
        MaxReputation = 7,
        Money = 8,
        PlayerKills = 9,
        AreaTrigger = 10,
        WinPetBattleAgainstNpc = 11,
        DefeatBattlePet = 12,
        WinPvpPetBattles = 13,
        CriteriaTree = 14,
        ProgressBar = 15,
        HaveCurrency = 16,      // requires the player to have X currency when turning in but does not consume it
        ObtainCurrency = 17,    // requires the player to gain X currency after starting the quest but not required to keep it until the end (does not consume)
        IncreaseReputation = 18,// requires the player to gain X reputation with a faction
        AreaTriggerEnter = 19,
        AreaTriggerExit = 20,
        Max
    }

    public enum QuestObjectiveFlags
    {
        TrackedOnMinimap = 0x01, // Client Displays Large Yellow Blob On Minimap For Creature/Gameobject
        Sequenced = 0x02, // Client Will Not See The Objective Displayed Until All Previous Objectives Are Completed
        Optional = 0x04, // Not Required To Complete The Quest
        Hidden = 0x08, // Never Displayed In Quest Log
        HideCreditMsg = 0x10, // Skip Showing Item Objective Progress
        PreserveQuestItems = 0x20,
        PartOfProgressBar = 0x40, // Hidden Objective Used To Calculate Progress Bar Percent (Quests Are Limited To A Single Progress Bar Objective)
        KillPlayersSameFaction = 0x80
    }
}
