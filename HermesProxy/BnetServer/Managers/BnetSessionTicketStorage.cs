// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using HermesProxy;
using System.Collections.Generic;

namespace BNetServer
{
    public static class BnetSessionTicketStorage
    {
        public static Dictionary<string, GlobalSessionData> SessionsByName = new();
        public static Dictionary<string, GlobalSessionData> SessionsByTicket = new();
        public static Dictionary<ulong, GlobalSessionData> SessionsByKey = new();

        public static void AddNewSessionByName(string name, GlobalSessionData session)
        {
            if (SessionsByName.ContainsKey(name))
            {
                SessionsByName[name].OnDisconnect();
                SessionsByName[name] = session;
            }
            else
                SessionsByName.Add(name, session);
        }

        public static void AddNewSessionByTicket(string loginTicket, GlobalSessionData session)
        {
            if (SessionsByTicket.ContainsKey(loginTicket))
            {
                SessionsByTicket[loginTicket].OnDisconnect();
                SessionsByTicket[loginTicket] = session;
            }
            else
                SessionsByTicket.Add(loginTicket, session);
        }

        public static void AddNewSessionByKey(ulong connectKey, GlobalSessionData session)
        {
            if (SessionsByKey.ContainsKey(connectKey))
            {
                SessionsByKey[connectKey].OnDisconnect();
                SessionsByKey[connectKey] = session;
            }
            else
                SessionsByKey.Add(connectKey, session);
        }
    }
}
