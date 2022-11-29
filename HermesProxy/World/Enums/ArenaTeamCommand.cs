namespace HermesProxy.World.Enums
{
    public enum ArenaTeamCommandType : uint
    {
        Create  = 0,
        Invite  = 1,
        Quit    = 3,
        Founder = 14,
    }

    public enum ArenaTeamCommandErrorLegacy : uint
    {
        None                        = 0x00,
        Internal                    = 0x01,
        AlreadyInArenaTeam          = 0x02,
        AlreadyInArenaTeamS         = 0x03,
        InvitedToArenaTeam          = 0x04,
        AlreadyInvitedToArenaTeamS  = 0x05,
        NameInvalid                 = 0x06,
        NameExistsS                 = 0x07,
        LeaderLeaveS                = 0x08,
        Permissions                 = 0x08,
        PlayerNotInTeam             = 0x09,
        PlayerNotInTeamSS           = 0x0A,
        PlayerNotFoundS             = 0x0B,
        NotALlied                   = 0x0C,
        IgnoringYouS                = 0x13,
        TargetTooLowS               = 0x15,
        TooManyMembersS             = 0x16,
    }

    public enum ArenaTeamCommandErrorModern : uint
    {
        None                        = 0x00,
        Internal                    = 0x01,
        AlreadyInArenaTeam          = 0x02,
        AlreadyInArenaTeamS         = 0x03,
        InvitedToArenaTeam          = 0x04,
        AlreadyInvitedToArenaTeamS  = 0x05,
        NameInvalid                 = 0x06,
        NameExistsS                 = 0x07,
        LeaderLeaveS                = 0x08,
        Permissions                 = 0x08,
        PlayerNotInTeam             = 0x09,
        PlayerNotInTeamSS           = 0x0A,
        PlayerNotFoundS             = 0x0B,
        NotALlied                   = 0x0C,
        IgnoringYouS                = 0x13,
        Internal2                   = 0x14,
        TargetTooLowS               = 0x15,
        TargetTooHighS              = 0x16,
        TooManyMembersS             = 0x17,
        NotFound                    = 0x1B,
        Locked                      = 0x1E,
        TooManyCreate               = 0x21,
        Disqualified                = 0x2A,
    }
}
