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
    public class BindPointUpdate : ServerPacket
    {
        public BindPointUpdate() : base(Opcode.SMSG_BIND_POINT_UPDATE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteVector3(BindPosition);
            _worldPacket.WriteUInt32(BindMapID);
            _worldPacket.WriteUInt32(BindAreaID);
        }

        public uint BindMapID = 0xFFFFFFFF;
        public Vector3 BindPosition;
        public uint BindAreaID;
    }

    public class ServerTimeOffset : ServerPacket
    {
        public ServerTimeOffset() : base(Opcode.SMSG_SERVER_TIME_OFFSET) { }

        public override void Write()
        {
            _worldPacket.WriteInt64(Time);
        }

        public long Time;
    }

    public class TutorialFlags : ServerPacket
    {
        public TutorialFlags() : base(Opcode.SMSG_TUTORIAL_FLAGS) { }

        public override void Write()
        {
            for (byte i = 0; i < (int)Tutorials.Max; ++i)
                _worldPacket.WriteUInt32(TutorialData[i]);
        }

        public uint[] TutorialData = new uint[(int)Tutorials.Max];
    }

    public class CorpseReclaimDelay : ServerPacket
    {
        public CorpseReclaimDelay() : base(Opcode.SMSG_CORPSE_RECLAIM_DELAY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(Remaining);
        }

        public uint Remaining;
    }
}
