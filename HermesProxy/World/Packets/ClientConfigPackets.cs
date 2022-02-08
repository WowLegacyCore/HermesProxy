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
using Framework.Cryptography;
using Framework.Dynamic;
using Framework.IO;
using World;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using HermesProxy;

namespace World.Packets
{
    public class AccountDataTimes : ServerPacket
    {
        public AccountDataTimes() : base(Opcode.SMSG_ACCOUNT_DATA_TIMES) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(PlayerGuid);
            _worldPacket.WriteInt64(ServerTime);
            foreach (var accounttime in AccountTimes)
                _worldPacket.WriteInt64(accounttime);
        }

        public WowGuid128 PlayerGuid;
        public long ServerTime;
        public Array<long> AccountTimes = new((int)AccountDataTypes.Max);
    }

    public class ClientCacheVersion : ServerPacket
    {
        public ClientCacheVersion() : base(Opcode.SMSG_CACHE_VERSION) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(CacheVersion);
        }

        public uint CacheVersion = 0;
    }

    public class RequestAccountData : ClientPacket
    {
        public RequestAccountData(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PlayerGuid = _worldPacket.ReadPackedGuid128();
            DataType = (AccountDataTypes)_worldPacket.ReadBits<uint>(4);
        }

        public WowGuid128 PlayerGuid;
        public AccountDataTypes DataType = 0;
    }

    public class UpdateAccountData : ServerPacket
    {
        public UpdateAccountData() : base(Opcode.SMSG_UPDATE_ACCOUNT_DATA) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Player);
            _worldPacket.WriteInt64(Time);
            _worldPacket.WriteUInt32(Size);
            _worldPacket.WriteBits(DataType, 4);

            if (CompressedData == null)
                _worldPacket.WriteUInt32(0);
            else
            {
                var bytes = CompressedData.GetData();
                _worldPacket.WriteInt32(bytes.Length);
                _worldPacket.WriteBytes(bytes);
            }
        }

        public WowGuid128 Player;
        public long Time; // UnixTime
        public uint Size; // decompressed size
        public AccountDataTypes DataType = 0;
        public ByteBuffer CompressedData;
    }

    public class UserClientUpdateAccountData : ClientPacket
    {
        public UserClientUpdateAccountData(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            PlayerGuid = _worldPacket.ReadPackedGuid128();
            Time = _worldPacket.ReadInt64();
            Size = _worldPacket.ReadUInt32();
            DataType = (AccountDataTypes)_worldPacket.ReadBits<uint>(4);

            uint compressedSize = _worldPacket.ReadUInt32();
            if (compressedSize != 0)
            {
                CompressedData = new ByteBuffer(_worldPacket.ReadBytes(compressedSize));
            }
        }

        public WowGuid128 PlayerGuid;
        public long Time; // UnixTime
        public uint Size; // decompressed size
        public AccountDataTypes DataType = 0;
        public ByteBuffer CompressedData;
    }

    class SetAdvancedCombatLogging : ClientPacket
    {
        public SetAdvancedCombatLogging(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Enable = _worldPacket.HasBit();
        }

        public bool Enable;
    }
}
