/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets
{
    public class SeasonInfo : ServerPacket
    {
        public SeasonInfo() : base(Opcode.SMSG_SEASON_INFO) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(MythicPlusSeasonID);
            _worldPacket.WriteInt32(CurrentSeason);
            _worldPacket.WriteInt32(PreviousSeason);
            _worldPacket.WriteInt32(ConquestWeeklyProgressCurrencyID);
            _worldPacket.WriteInt32(PvpSeasonID);
            _worldPacket.WriteBit(WeeklyRewardChestsEnabled);
            _worldPacket.FlushBits();
        }

        public int MythicPlusSeasonID;
        public int PreviousSeason;
        public int CurrentSeason;
        public int PvpSeasonID;
        public int ConquestWeeklyProgressCurrencyID;
        public bool WeeklyRewardChestsEnabled;
    }
}
