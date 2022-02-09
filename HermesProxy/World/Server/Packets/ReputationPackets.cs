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
    public class InitializeFactions : ServerPacket
    {
        const ushort FactionCount = 400;

        public InitializeFactions() : base(Opcode.SMSG_INITIALIZE_FACTIONS, ConnectionType.Instance) { }

        public override void Write()
        {
            for (ushort i = 0; i < FactionCount; ++i)
            {
                _worldPacket.WriteUInt8((byte)((ushort)FactionFlags[i] & 0xFF));
                _worldPacket.WriteInt32(FactionStandings[i]);
            }

            for (ushort i = 0; i < FactionCount; ++i)
                _worldPacket.WriteBit(FactionHasBonus[i]);

            _worldPacket.FlushBits();
        }

        public int[] FactionStandings = new int[FactionCount];
        public bool[] FactionHasBonus = new bool[FactionCount]; //@todo: implement faction bonus
        public ReputationFlags[] FactionFlags = new ReputationFlags[FactionCount];
    }
}
