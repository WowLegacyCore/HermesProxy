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
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class InitWorldStates : ServerPacket
    {
        public InitWorldStates() : base(Opcode.SMSG_INIT_WORLD_STATES, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteUInt32(ZoneID);
            _worldPacket.WriteUInt32(AreaID);

            _worldPacket.WriteInt32(Worldstates.Count);
            foreach (WorldStateInfo wsi in Worldstates)
            {
                _worldPacket.WriteUInt32(wsi.VariableID);
                _worldPacket.WriteInt32(wsi.Value);
            }
        }

        public void AddState(uint variableID, int value)
        {
            Worldstates.Add(new WorldStateInfo(variableID, value));
        }

        public void AddState(uint variableID, bool value)
        {
            Worldstates.Add(new WorldStateInfo(variableID, value ? 1 : 0));
        }

        public void AddMissingState(uint variableID, int value)
        {
            foreach (var state in Worldstates)
            {
                if (state.VariableID == variableID)
                    return;
            }
            Worldstates.Add(new WorldStateInfo(variableID, value));
        }

        public void AddClassicStates()
        {
            if (ModernVersion.ExpansionVersion == 1)
            {
                AddMissingState(17101, 1);
                AddMissingState(17222, 1);
                AddMissingState(17223, 1);
                AddMissingState(17224, 1);
                AddMissingState(17225, 1);
                AddMissingState(17226, 1);
                AddMissingState(17227, 1);
                AddMissingState(17228, 1);
                AddMissingState(17229, 1);
                AddMissingState(17230, 1);
                AddMissingState(17231, 1);
                AddMissingState(17232, 1);
                AddMissingState(17233, 1);
                AddMissingState(17234, 1);
                AddMissingState(17424, 1);
                AddMissingState(17430, 1);
                AddMissingState(17478, 1);
                AddMissingState(17560, 1);
                AddMissingState(17640, 1);
                AddMissingState(17641, 1);
                AddMissingState(17642, 1);
                AddMissingState(17643, 1);
                AddMissingState(17647, 1);
                AddMissingState(17648, 1);
                AddMissingState(17687, 1);
                AddMissingState(17697, 1);
                AddMissingState(17698, 1);
                AddMissingState(17704, 1);
                AddMissingState(17705, 1);
                AddMissingState(17706, 1);
                AddMissingState(17707, 1);
                AddMissingState(18261, 1);
                AddMissingState(19361, 1);
                AddMissingState(20281, 1);
                AddMissingState(20470, 1);
                AddMissingState(21260, 1);
            }
            else
            {
                AddMissingState(17223, 1);
                AddMissingState(17647, 1);
                AddMissingState(17648, 1);
                AddMissingState(20445, 0);
                AddMissingState(20446, 0);
                AddMissingState(20447, 1);
                AddMissingState(20487, 1);
                AddMissingState(20488, 1);
                AddMissingState(20489, 1);
                AddMissingState(20491, 1);
                AddMissingState(20492, 1);
                AddMissingState(20493, 1);
                AddMissingState(20494, 0);
                AddMissingState(20495, 0);
                AddMissingState(20496, 0);
                AddMissingState(20497, 0);
                AddMissingState(20518, 0);
                AddMissingState(20560, 0);
                AddMissingState(20562, 1);
                AddMissingState(20563, 1);
                AddMissingState(20567, 0);
                AddMissingState(20738, 0);
                AddMissingState(20882, 0);
                AddMissingState(21125, 1);
                AddMissingState(21126, 1);
                AddMissingState(21195, 2725);
                AddMissingState(21196, 2542);
                AddMissingState(21197, 2203);
                AddMissingState(21198, 1898);
                AddMissingState(21199, 1453);
                AddMissingState(21200, 2548);
                AddMissingState(21201, 2391);
                AddMissingState(21202, 2086);
                AddMissingState(21203, 1777);
                AddMissingState(21204, 1431);
                AddMissingState(21205, 2354);
                AddMissingState(21206, 2181);
                AddMissingState(21207, 1922);
                AddMissingState(21208, 1686);
                AddMissingState(21209, 1408);
                AddMissingState(21238, 2);
            }
        }

        public uint ZoneID;
        public uint AreaID;
        public uint MapID;

        List<WorldStateInfo> Worldstates = new();

        struct WorldStateInfo
        {
            public WorldStateInfo(uint variableID, int value)
            {
                VariableID = variableID;
                Value = value;
            }

            public uint VariableID;
            public int Value;
        }
    }

    public class UpdateWorldState : ServerPacket
    {
        public UpdateWorldState() : base(Opcode.SMSG_UPDATE_WORLD_STATE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(VariableID);
            _worldPacket.WriteInt32(Value);
            _worldPacket.WriteBit(Hidden);
            _worldPacket.FlushBits();
        }

        public uint VariableID;
        public int Value;
        public bool Hidden;
    }
}
