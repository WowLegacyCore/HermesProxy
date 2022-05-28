namespace HermesProxy.Auth
{
    // ReSharper disable InconsistentNaming
    public enum AuthResult : byte
    { // See vMangos
        SUCCESS                 = 0x00,
        FAIL_UNKNOWN0           = 0x01, // ? Unable to connect
        FAIL_UNKNOWN1           = 0x02, // ? Unable to connect
        FAIL_BANNED             = 0x03, // This <game> account has been closed and is no longer available for use. Please go to <site>/banned.html for further information.

        FAIL_UNKNOWN_ACCOUNT    = 0x04, // The information you have entered is not valid. Please check the spelling of the account name and password. If you need help in retrieving a lost or stolen password, see <site> for more information
        FAIL_INCORRECT_PASSWORD = 0x05, // The information you have entered is not valid. Please check the spelling of the account name and password. If you need help in retrieving a lost or stolen password, see <site> for more information
                                        // client reject next login attempts after this error, so in code used FAIL_UNKNOWN_ACCOUNT for both cases

        FAIL_ALREADY_ONLINE     = 0x06, // This account is already logged into <game>. Please check the spelling and try again.
        FAIL_NO_TIME            = 0x07, // You have used up your prepaid time for this account. Please purchase more to continue playing
        FAIL_DB_BUSY            = 0x08, // Could not log in to <game> at this time. Please try again later.
        FAIL_VERSION_INVALID    = 0x09, // Unable to validate game version. This may be caused by file corruption or interference of another program. Please visit <site> for more information and possible solutions to this issue.
        FAIL_VERSION_UPDATE     = 0x0A, // Downloading
        FAIL_INVALID_SERVER     = 0x0B, // Unable to connect
        FAIL_SUSPENDED          = 0x0C, // This <game> account has been temporarily suspended. Please go to <site>/banned.html for further information
        FAIL_FAIL_NOACCESS      = 0x0D, // Unable to connect
        SUCCESS_SURVEY          = 0x0E, // Connected.
        FAIL_PARENTCONTROL      = 0x0F, // Access to this account has been blocked by parental controls. Your settings may be changed in your account preferences at <site>
        // TBC+
        FAIL_LOCKED_ENFORCED    = 0x10, // You have applied a lock to your account. You can change your locked status by calling your account lock phone number.
        // WOTLK+
        FAIL_TRIAL_ENDED        = 0x11, // Your trial subscription has expired. Please visit <site> to upgrade your account.
        FAIL_USE_BATTLENET      = 0x12, // This account is now attached to a Battle.net account. Please login with your Battle.net account email address and password.
        FAIL_ANTI_INDULGENCE    = 0x13, // Unable to connect
        FAIL_EXPIRED            = 0x14, // Unable to connect
        FAIL_NO_GAME_ACCOUNT    = 0x15, // Unable to connect
        FAIL_CHARGEBACK         = 0x16, // This World of Warcraft account has been temporary closed due to a chargeback on its subscription. Please refer to this <site> for further information.
        FAIL_IGR_WITHOUT_BNET   = 0x17, // In order to log in to World of Warcraft using IGR time, this World of Warcraft account must first be merged with a Battle.net account. Please visit <site> to merge this account.
        FAIL_GAME_ACCOUNT_LOCKE = 0x18, // Access to your account has been temporarily disabled.
        FAIL_UNLOCKABLE_LOCK    = 0x19, // Your account has been locked but can be unlocked.
        FAIL_CONVERSION_REQUIRE = 0x20, // This account needs to be converted to a Battle.net account. Please [Click Here] or go to: <site> to begin conversion.
        FAIL_DISCONNECTED       = 0xFF,
        
        // HermesProxy internal variables
        FAIL_INTERNAL_ERROR     = 0xFE, // Internal error
        FAIL_WRONG_MODERN_VER   = 0xFD, // Modern client is using unsupported version
    }
    // ReSharper restore InconsistentNaming
}
