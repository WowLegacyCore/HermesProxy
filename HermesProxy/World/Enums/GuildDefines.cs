using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum GuildCommandType
    {
        CreateGuild = 0,
        InvitePlayer = 1,
        LeaveGuild = 3,
        GetRoster = 5,
        PromotePlayer = 6,
        DemotePlayer = 7,
        RemovePlayer = 8,
        ChangeLeader = 10,
        EditMOTD = 11,
        GuildChat = 13,
        Founder = 14,
        ChangeRank = 16,
        EditPublicNote = 19,
        ViewTab = 21,
        MoveItem = 22,
        Repair = 25
    }

    public enum GuildCommandError
    {
        Success = 0,
        GuildInternal = 1,
        AlreadyInGuild = 2,
        AlreadyInGuild_S = 3,
        InvitedToGuild = 4,
        AlreadyInvitedToGuild_S = 5,
        NameInvalid = 6,
        NameExists_S = 7,
        LeaderLeave = 8,
        Permissions = 8,
        PlayerNotInGuild = 9,
        PlayerNotInGuild_S = 10,
        PlayerNotFound_S = 11,
        NotAllied = 12,
        RankTooHigh_S = 13,
        RankTooLow_S = 14,
        RanksLocked = 17,
        RankInUse = 18,
        IgnoringYou_S = 19,
        Unk1 = 20,
        WithdrawLimit = 25,
        NotEnoughMoney = 26,
        BankFull = 28,
        ItemNotFound = 29,
        TooMuchMoney = 31,
        WrongTab = 32,
        RequiresAuthenticator = 34,
        BankVoucherFailed = 35,
        TrialAccount = 36,
        UndeletableDueToLevel = 37,
        MoveStarting = 38,
        RepTooLow = 39
    }
}
