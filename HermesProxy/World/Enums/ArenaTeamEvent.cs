using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum ArenaTeamEventLegacy
    {
        PlayerJoined = 3,
        PlayerLeft = 4,
        PlayerRemoved = 5,
        LeaderIs = 6,
        LeaderChanged = 7,
        TeamDisbanded = 8
    }

    public enum ArenaTeamEventModern
    {
        PlayerJoined = 4,
        PlayerLeft = 5,
        PlayerRemoved = 6,
        LeaderIs = 7,
        LeaderChanged = 8,
        TeamDisbanded = 9
    }
}
