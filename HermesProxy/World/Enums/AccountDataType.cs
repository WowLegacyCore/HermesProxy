using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum AccountDataType
    {
        GlobalConfigCache              = 0,
        PerCharacterConfigCache        = 1,
        GlobalBindingsCache            = 2,
        PerCharacterBindingsCache      = 3,
        GlobalMacrosCache              = 4,
        PerCharacterMacrosCache        = 5,
        PerCharacterLayoutCache        = 6,
        PerCharacterChatCache          = 7,
        GlobalTTSCache                 = 8,
        PerCharacterTTSCache           = 9,
        GlobalFlaggedCache             = 10,
        PerCharacterFlaggedCache       = 11,
        PerCharacterClickBindingsCache = 12,
    }
}
