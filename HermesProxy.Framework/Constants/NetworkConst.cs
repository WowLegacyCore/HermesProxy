using System;

namespace HermesProxy.Framework.Constants
{
    public enum AuthCommand : byte
    {
        LOGON_CHALLENGE     = 0x00,
        LOGON_PROOF         = 0x01,
        REALM_LIST          = 0x10,
        TRANSFER_INITIATE   = 0x30,
        TRANSFER_DATA       = 0x31,
        TRANSFER_ACCEPT     = 0x32,
        TRANSFER_RESUME     = 0x33,
        TRANSFER_CANCEL     = 0x34
    }

    public enum AuthResult : byte
    {
        SUCCESS             = 0,
        FAILURE             = 0x01,
        UNKNOWN1            = 0x02,
        ACCOUNT_BANNED      = 0x03,
        NO_MATCH            = 0x04,
        UNKNOWN2            = 0x05,
        ACCOUNT_IN_USE      = 0x06,
        PREPAID_TIME_LIMIT  = 0x07,
        SERVER_FULL         = 0x08,
        WRONG_BUILD_NUMBER  = 0x09,
        UPDATE_CLIENT       = 0x0a,
        UNKNOWN3            = 0x0b,
        ACCOUNT_FREEZED     = 0x0c,
        UNKNOWN4            = 0x0d,
        UNKNOWN5            = 0x0e,
        PARENTAL_CONTROL    = 0x0f
    }

    [Flags]
    public enum RealmFlags
    {
        None                = 0x00,
        Invalid             = 0x01,
        Offline             = 0x02,
        SpecifyBuild        = 0x04,     // client will show realm version in RealmList screen in form "RealmName (major.minor.revision.build)"
        Unk1                = 0x08,
        Unk2                = 0x10,
        NewPlayers          = 0x20,
        Recommended         = 0x40,
        Full                = 0x80
    }
}
