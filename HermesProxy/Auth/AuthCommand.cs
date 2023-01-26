using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.Auth
{
    // ReSharper disable InconsistentNaming
    public enum AuthCommand : byte
    {
        LOGON_CHALLENGE     = 0x00,
        LOGON_PROOF         = 0x01,
        RECONNECT_CHALLENGE = 0x02,
        RECONNECT_PROOF     = 0x03,
        REALM_LIST          = 0x10,
        TRANSFER_INITIATE   = 0x30,
        TRANSFER_DATA       = 0x31,
        TRANSFER_ACCEPT     = 0x32,
        TRANSFER_RESUME     = 0x33,
        TRANSFER_CANCEL     = 0x34
    }
    // ReSharper restore InconsistentNaming
}
