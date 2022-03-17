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
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class ArenaTeamRosterRequest : ClientPacket
    {
        public ArenaTeamRosterRequest(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TeamId = _worldPacket.ReadInt32();
        }

        public int TeamId;
    }

    class ArenaTeamRosterResponse : ServerPacket
    {
        public ArenaTeamRosterResponse() : base(Opcode.SMSG_ARENA_TEAM_ROSTER) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(dword20);
            _worldPacket.WriteUInt32(TeamSize);
            _worldPacket.WriteUInt32(TeamPlayed);
            _worldPacket.WriteUInt32(TeamWins);
            _worldPacket.WriteUInt32(SeasonPlayed);
            _worldPacket.WriteUInt32(SeasonWins);
            _worldPacket.WriteUInt32(TeamRating);
            _worldPacket.WriteUInt32(PlayerRating);
            _worldPacket.WriteInt32(Members.Count);
            foreach (var member in Members)
                member.Write(_worldPacket);
        }

        public int dword20;
        public uint TeamSize;
        public uint TeamPlayed;
        public uint TeamWins;
        public uint SeasonPlayed;
        public uint SeasonWins;
        public uint TeamRating;
        public uint PlayerRating;
        public List<ArenaTeamMember> Members = new List<ArenaTeamMember>();
    }

    struct ArenaTeamMember
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(MemberGUID);
            data.WriteBool(Online); // ???????
            data.WriteInt32(Captain);
            data.WriteUInt8(Level);
            data.WriteUInt8((byte)ClassId);
            data.WriteUInt32(WeekGamesPlayed);
            data.WriteUInt32(WeekGamesWon);
            data.WriteUInt32(SeasonGamesPlayed);
            data.WriteUInt32(SeasonGamesWon);
            data.WriteUInt32(PersonalRating);

            data.WriteBits(Name.GetByteCount(), 6);
            data.WriteBit(dword60 != null);
            data.WriteBit(dword68 != null);
            data.FlushBits();

            data.WriteString(Name);
            if (dword60 != null)
                data.WriteFloat((float)dword60);
            if (dword68 != null)
                data.WriteFloat((float)dword68);
        }

        public WowGuid128 MemberGUID;
        public bool Online;
        public int Captain;
        public byte Level;
        public Class ClassId;
        public uint WeekGamesPlayed;
        public uint WeekGamesWon;
        public uint SeasonGamesPlayed;
        public uint SeasonGamesWon;
        public uint PersonalRating;
        public string Name;
        public float? dword60;
        public float? dword68;
    }
}
