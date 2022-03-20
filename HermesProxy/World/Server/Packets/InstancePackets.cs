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
    class UpdateLastInstance : ServerPacket
    {
        public UpdateLastInstance() : base(Opcode.SMSG_UPDATE_LAST_INSTANCE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
        }

        public uint MapID;
    }

    class InstanceReset : ServerPacket
    {
        public InstanceReset() : base(Opcode.SMSG_INSTANCE_RESET) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
        }

        public uint MapID;
    }

    class InstanceResetFailed : ServerPacket
    {
        public InstanceResetFailed() : base(Opcode.SMSG_INSTANCE_RESET_FAILED) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteBits(ResetFailedReason, 2);
            _worldPacket.FlushBits();
        }

        public uint MapID;
        public ResetFailedReason ResetFailedReason;
    }

    class ResetFailedNotify : ServerPacket
    {
        public ResetFailedNotify() : base(Opcode.SMSG_RESET_FAILED_NOTIFY) { }

        public override void Write() { }
    }
}
