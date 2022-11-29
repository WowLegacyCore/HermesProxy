using System;

namespace HermesProxy.World.Enums
{
    public enum ChatNotify
    {
        Joined                    = 0x00,           //+ "%S Joined Channel.";
        Left                      = 0x01,           //+ "%S Left Channel.";
        YouJoined                 = 0x02,           //+ "Joined Channel: [%S]"; -- You Joined
        YouLeft                   = 0x03,           //+ "Left Channel: [%S]"; -- You Left
        WrongPassword             = 0x04,           //+ "Wrong Password For %S.";
        NotMember                 = 0x05,           //+ "Not On Channel %S.";
        NotModerator              = 0x06,           //+ "Not A Moderator Of %S.";
        PasswordChanged           = 0x07,           //+ "[%S] Password Changed By %S.";
        OwnerChanged              = 0x08,           //+ "[%S] Owner Changed To %S.";
        PlayerNotFound            = 0x09,           //+ "[%S] Player %S Was Not Found.";
        NotOwner                  = 0x0a,           //+ "[%S] You Are Not The Channel Owner.";
        ChannelOwner              = 0x0b,           //+ "[%S] Channel Owner Is %S.";
        ModeChange                = 0x0c,           //?
        AnnouncementsOn           = 0x0d,           //+ "[%S] Channel Announcements Enabled By %S.";
        AnnouncementsOff          = 0x0e,           //+ "[%S] Channel Announcements Disabled By %S.";
        ModerationOn              = 0x0f,           //+ "[%S] Channel Moderation Enabled By %S.";
        ModerationOff             = 0x10,           //+ "[%S] Channel Moderation Disabled By %S.";
        Muted                     = 0x11,           //+ "[%S] You Do Not Have Permission To Speak.";
        PlayerKicked              = 0x12,           //? "[%S] Player %S Kicked By %S.";
        Banned                    = 0x13,           //+ "[%S] You Are Bannedstore From That Channel.";
        PlayerBanned              = 0x14,           //? "[%S] Player %S Bannedstore By %S.";
        PlayerUnbanned            = 0x15,           //? "[%S] Player %S Unbanned By %S.";
        PlayerNotBanned           = 0x16,           //+ "[%S] Player %S Is Not Bannedstore.";
        PlayerAlreadyMember       = 0x17,           //+ "[%S] Player %S Is Already On The Channel.";
        Invite                    = 0x18,           //+ "%2$S Has Invited You To Join The Channel '%1$S'.";
        InviteWrongFaction        = 0x19,           //+ "Target Is In The Wrong Alliance For %S.";
        WrongFaction              = 0x1a,           //+ "Wrong Alliance For %S.";
        InvalidName               = 0x1b,           //+ "Invalid Channel Name";
        NotModerated              = 0x1c,           //+ "%S Is Not Moderated";
        PlayerInvited             = 0x1d,           //+ "[%S] You Invited %S To Join The Channel";
        PlayerInviteBanned        = 0x1e,           //+ "[%S] %S Has Been Bannedstore.";
        Throttled                 = 0x1f,           //+ "[%S] The Number Of Messages That Can Be Sent To This Channel Is Limited, Please Wait To Send Another Message.";
        NotInArea                 = 0x20,           //+ "[%S] You Are Not In The Correct Area For This Channel."; -- The User Is Trying To Send A Chat To A Zone Specific Channel, And They'Re Not Physically In That Zone.
        NotInLfg                  = 0x21,           //+ "[%S] You Must Be Queued In Looking For Group Before Joining This Channel."; -- The User Must Be In The Looking For Group System To Join Lfg Chat Channels.
        VoiceOn                   = 0x22,           //+ "[%S] Channel Voice Enabled By %S.";
        VoiceOff                  = 0x23,            //+ "[%S] Channel Voice Disabled By %S.";
        TrialRestricted           = 0x24,
        NotAllowedInChannel       = 0x25
    }

    public enum ChannelId
    {
        General = 1,
        Trade = 2,
        LocalDefense = 22,
        WorldDefense = 23,
        GuildRecruitment = 25,
        LookingForGroup = 26
    };

    [Flags]
    public enum ChannelFlags : uint
    {
        None             = 0x000000,
        AutoJoin         = 0x000001,              // General, Trade, LocalDefense, LFG
        ZoneBased        = 0x000002,              // General, Trade, LocalDefense, GuildRecruitment
        ReadOnly         = 0x000004,              // WorldDefense
        AllowItemLinks   = 0x000008,              // Trade, LFG
        OnlyInCities     = 0x000010,              // Trade, GuildRecruitment, LFG
        LinkedChannel    = 0x000020,              // Trade, GuildRecruitment, LFG
        ZoneAttackAlerts = 0x010000,              // LocalDefense, WorldDefense
        GuildRecruitment = 0x020000,              // GuildRecruitment
    }
}
