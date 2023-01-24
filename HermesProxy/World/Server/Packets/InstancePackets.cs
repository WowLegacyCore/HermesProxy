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
    class UpdateInstanceOwnership : ServerPacket
    {
        public UpdateInstanceOwnership() : base(Opcode.SMSG_UPDATE_INSTANCE_OWNERSHIP) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(IOwnInstance);
        }

        public uint IOwnInstance;
    }

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

    class RaidInstanceInfo : ServerPacket
    {
        public RaidInstanceInfo() : base(Opcode.SMSG_RAID_INSTANCE_INFO) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(LockList.Count);

            foreach (InstanceLock lockInfos in LockList)
                lockInfos.Write(_worldPacket);
        }

        public List<InstanceLock> LockList = new();
    }

    public class InstanceLock
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(MapID);
            data.WriteUInt32((uint)DifficultyID);
            data.WriteUInt64(InstanceID);
            data.WriteInt32(TimeRemaining);
            data.WriteUInt32(CompletedMask);

            data.WriteBit(Locked);
            data.WriteBit(Extended);
            data.FlushBits();
        }

        public uint MapID;
        public DifficultyModern DifficultyID;
        public ulong InstanceID;
        public int TimeRemaining;
        public uint CompletedMask = 1;

        public bool Locked = true;
        public bool Extended;
    }

    class InstanceSaveCreated : ServerPacket
    {
        public InstanceSaveCreated() : base(Opcode.SMSG_INSTANCE_SAVE_CREATED) { }

        public override void Write()
        {
            _worldPacket.WriteBit(Gm);
            _worldPacket.FlushBits();
        }

        public bool Gm;
    }

    class RaidGroupOnly : ServerPacket
    {
        public RaidGroupOnly() : base(Opcode.SMSG_RAID_GROUP_ONLY) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Delay);
            _worldPacket.WriteUInt32((uint)Reason);
        }

        public int Delay;
        public RaidGroupReason Reason;
    }

    class RaidInstanceMessage : ServerPacket
    {
        public RaidInstanceMessage() : base(Opcode.SMSG_RAID_INSTANCE_MESSAGE) { }

        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)Type);
            _worldPacket.WriteUInt32(MapID);
            _worldPacket.WriteUInt32((uint)DifficultyID);
            _worldPacket.WriteBit(Locked);
            _worldPacket.WriteBit(Extended);
            _worldPacket.FlushBits();
        }

        public InstanceResetWarningType Type;
        public uint MapID;
        public DifficultyModern DifficultyID;
        public bool Locked;
        public bool Extended;
    }
}
