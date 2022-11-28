using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums.V1_12_1_5875
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

        RealmListInProgress = 34,
        RealmListSuccess = 35,
        RealmListFailed = 36,
        RealmListInvalid = 37,
        RealmListRealmNotFound = 38,

        AccountCreateInProgress = 39,
        AccountCreateSuccess = 40,
        AccountCreateFailed = 41,

        CharListRetrieving = 42,
        CharListRetrieved = 43,
        CharListFailed = 44,

        CharCreateInProgress = 45,
        CharCreateSuccess = 46,
        CharCreateError = 47,
        CharCreateFailed = 48,
        CharCreateNameInUse = 49,
        CharCreateDisabled = 50,
        CharCreatePvpTeamsViolation = 51,
        CharCreateServerLimit = 52,
        CharCreateAccountLimit = 53,
        CharCreateServerQueue = 54,
        CharCreateOnlyExisting = 55,

        CharDeleteInProgress = 56,
        CharDeleteSuccess = 57,
        CharDeleteFailed = 58,
        CharDeleteFailedLockedForTransfer = 59,

        CharLoginInProgress = 60,
        CharLoginSuccess = 61,
        CharLoginNoWorld = 62,
        CharLoginDuplicateCharacter = 63,
        CharLoginNoInstances = 64,
        CharLoginFailed = 65,
        CharLoginDisabled = 66,
        CharLoginNoCharacter = 67,
        CharLoginLockedForTransfer = 68,

        CharNameNoName = 69,
        CharNameTooShort = 70,
        CharNameTooLong = 71,
        CharNameInvalidCharacter = 72,
        CharNameMixedLanguages = 73,
        CharNameProfane = 74,
        CharNameReserved = 75,
        CharNameInvalidApostrophe = 76,
        CharNameMultipleApostrophes = 77,
        CharNameThreeConsecutive = 78,
        CharNameInvalidSpace = 79,
        CharNameConsecutiveSpaces = 80,
        CharNameFailure = 81,
        CharNameSuccess = 82
    }
}
