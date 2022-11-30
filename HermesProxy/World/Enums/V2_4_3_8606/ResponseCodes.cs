using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums.V2_4_3_8606
{
    public enum ResponseCodes : byte
    {
        Success = 0,
        Failure = 1,
        Cancelled = 2,
        Disconnected = 3,
        FailedToConnect = 4,
        Connected = 5,
        VersionMismatch = 6,

        CstatusConnecting = 7,
        CstatusNegotiatingSecurity = 8,
        CstatusNegotiationComplete = 9,
        CstatusNegotiationFailed = 10,
        CstatusAuthenticating = 11,

        RealmListInProgress = 0x23,
        RealmListSuccess = 0x24,
        RealmListFailed = 0x25,
        RealmListInvalid = 0x26,
        RealmListRealmNotFound = 0x27,

        AccountCreateInProgress = 0x28,
        AccountCreateSuccess = 0x29,
        AccountCreateFailed = 0x2A,

        CharListRetrieving = 0x2B,
        CharListRetrieved = 0x2C,
        CharListFailed = 0x2D,

        CharCreateInProgress = 0x2E,
        CharCreateSuccess = 0x2F,
        CharCreateError = 0x30,
        CharCreateFailed = 0x31,
        CharCreateNameInUse = 0x32,
        CharCreateDisabled = 0x33,
        CharCreatePvpTeamsViolation = 0x34,
        CharCreateServerLimit = 0x35,
        CharCreateAccountLimit = 0x36,
        CharCreateServerQueue = 0x37,
        CharCreateOnlyExisting = 0x38,
        CharCreateExpansion = 0x39,

        CharDeleteInProgress = 0x3A,
        CharDeleteSuccess = 0x3B,
        CharDeleteFailed = 0x3C,
        CharDeleteFailedLockedForTransfer = 0x3D,
        CharDeleteFailedGuildLeader = 0x3E,
        CharDeleteFailedArenaCaptain = 0x3F,

        CharLoginInProgress = 0x40,
        CharLoginSuccess = 0x41,
        CharLoginNoWorld = 0x42,
        CharLoginDuplicateCharacter = 0x43,
        CharLoginNoInstances = 0x44,
        CharLoginFailed = 0x45,
        CharLoginDisabled = 0x46,
        CharLoginNoCharacter = 0x47,
        CharLoginLockedForTransfer = 0x48,
        CharLoginLockedByBilling = 0x49,

        CharNameSuccess = 0x4A,
        CharNameFailure = 0x4B,
        CharNameNoName = 0x4C,
        CharNameTooShort = 0x4D,
        CharNameTooLong = 0x4E,
        CharNameInvalidCharacter = 0x4F,
        CharNameMixedLanguages = 0x50,
        CharNameProfane = 0x51,
        CharNameReserved = 0x52,
        CharNameInvalidApostrophe = 0x53,
        CharNameMultipleApostrophes = 0x54,
        CharNameThreeConsecutive = 0x55,
        CharNameInvalidSpace = 0x56,
        CharNameConsecutiveSpaces = 0x57,
        CharNameRussianConsecutiveSilentCharacters = 0x58,
        CharNameRussianSilentCharacterAtBeginningOrEnd = 0x59,
        CharNameDeclensionDoesntMatchBaseName = 0x5A,
    }
}
