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
using HermesProxy.World.Objects;
using Framework.Collections;

namespace World.Packets
{
    public class QueryPlayerName : ClientPacket
    {
        public QueryPlayerName(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Player = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Player;
    }

    public class QueryPlayerNameResponse : ServerPacket
    {
        public QueryPlayerNameResponse() : base(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)
        {
            Data = new PlayerGuidLookupData();
        }

        public override void Write()
        {
            _worldPacket.WriteInt8((sbyte)Result);
            _worldPacket.WritePackedGuid128(Player);

            if (Result == HermesProxy.World.Objects.Classic.ResponseCodes.Success)
                Data.Write(_worldPacket);
        }

        public WowGuid128 Player;
        public HermesProxy.World.Objects.Classic.ResponseCodes Result; // 0 - full packet, != 0 - only guid
        public PlayerGuidLookupData Data;
    }

    public class PlayerGuidLookupData
    {
        public void Write(WorldPacket data)
        {
            data.WriteBit(IsDeleted);
            data.WriteBits(Name.GetByteCount(), 6);

            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                data.WriteBits(DeclinedNames.name[i].GetByteCount(), 7);

            data.FlushBits();
            for (byte i = 0; i < PlayerConst.MaxDeclinedNameCases; ++i)
                data.WriteString(DeclinedNames.name[i]);

            data.WritePackedGuid128(AccountID);
            data.WritePackedGuid128(BnetAccountID);
            data.WritePackedGuid128(GuidActual);
            data.WriteUInt64(GuildClubMemberID);
            data.WriteUInt32(VirtualRealmAddress);
            data.WriteUInt8((byte)RaceID);
            data.WriteUInt8((byte)Sex);
            data.WriteUInt8((byte)ClassID);
            data.WriteUInt8(Level);
            data.WriteUInt8(Unused915);
            data.WriteString(Name);
        }

        public bool IsDeleted;
        public WowGuid128 AccountID;
        public WowGuid128 BnetAccountID;
        public WowGuid128 GuidActual;
        public string Name = "";
        public ulong GuildClubMemberID;   // same as bgs.protocol.club.v1.MemberId.unique_id
        public uint VirtualRealmAddress;
        public Race RaceID = Race.None;
        public Gender Sex = Gender.None;
        public Class ClassID = Class.None;
        public byte Level;
        public byte Unused915;
        public DeclinedName DeclinedNames = new();
    }

    public class DeclinedName
    {
        public StringArray name = new(PlayerConst.MaxDeclinedNameCases);
    }
}
