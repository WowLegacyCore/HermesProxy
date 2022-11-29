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

    class SetFactionStanding : ServerPacket
    {
        public SetFactionStanding() : base(Opcode.SMSG_SET_FACTION_STANDING, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteFloat(ReferAFriendBonus);
            _worldPacket.WriteFloat(BonusFromAchievementSystem);

            _worldPacket.WriteInt32(Factions.Count);
            foreach (FactionStandingData factionStanding in Factions)
                factionStanding.Write(_worldPacket);

            _worldPacket.WriteBit(ShowVisual);
            _worldPacket.FlushBits();
        }

        public float ReferAFriendBonus;
        public float BonusFromAchievementSystem;
        public List<FactionStandingData> Factions = new();
        public bool ShowVisual;
    }

    struct FactionStandingData
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(Index);
            data.WriteInt32(Standing);
        }

        public int Index;
        public int Standing;
    }

    class SetFactionAtWar : ClientPacket
    {
        public SetFactionAtWar(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            FactionIndex = _worldPacket.ReadUInt8();
        }

        public byte FactionIndex;
    }

    class SetFactionNotAtWar : ClientPacket
    {
        public SetFactionNotAtWar(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            FactionIndex = _worldPacket.ReadUInt8();
        }

        public byte FactionIndex;
    }

    class SetFactionInactive : ClientPacket
    {
        public SetFactionInactive(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            FactionIndex = _worldPacket.ReadUInt32();
            State = _worldPacket.HasBit();
        }

        public uint FactionIndex;
        public bool State;
    }

    class SetWatchedFaction : ClientPacket
    {
        public SetWatchedFaction(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            FactionIndex = _worldPacket.ReadUInt32();
        }

        public uint FactionIndex;
    }

    class SetForcedReactions : ServerPacket
    {
        public SetForcedReactions() : base(Opcode.SMSG_SET_FORCED_REACTIONS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Reactions.Count);
            foreach (ForcedReaction reaction in Reactions)
                reaction.Write(_worldPacket);
        }

        public List<ForcedReaction> Reactions = new();
    }

    struct ForcedReaction
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(Faction);
            data.WriteInt32(Reaction);
        }

        public int Faction;
        public int Reaction;
    }

    class SetFactionVisible : ServerPacket
    {
        public SetFactionVisible(bool visible) : base(visible ? Opcode.SMSG_SET_FACTION_VISIBLE : Opcode.SMSG_SET_FACTION_NOT_VISIBLE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(FactionIndex);
        }

        public uint FactionIndex;
    }
}
