using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.Enums
{
    enum AuthResult : byte
    {
        SUCCESS = 0,
        FAILURE = 0x01,
        UNKNOWN1 = 0x02,
        ACCOUNT_BANNED = 0x03,
        NO_MATCH = 0x04,
        UNKNOWN2 = 0x05,
        ACCOUNT_IN_USE = 0x06,
        PREPAID_TIME_LIMIT = 0x07,
        SERVER_FULL = 0x08,
        WRONG_BUILD_NUMBER = 0x09,
        UPDATE_CLIENT = 0x0a,
        UNKNOWN3 = 0x0b,
        ACCOUNT_FREEZED = 0x0c,
        UNKNOWN4 = 0x0d,
        UNKNOWN5 = 0x0e,
        PARENTAL_CONTROL = 0x0f
    }
}
