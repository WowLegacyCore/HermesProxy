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
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class AttackSwing : ClientPacket
    {
        public AttackSwing(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Victim = _worldPacket.ReadPackedGuid128();
        }

        public WowGuid128 Victim;
    }

    public class AttackSwingError : ServerPacket
    {
        public AttackSwingError() : base(Opcode.SMSG_ATTACK_SWING_ERROR) { }

        public override void Write()
        {
            _worldPacket.WriteBits((uint)Reason, 3);
            _worldPacket.FlushBits();
        }

        public AttackSwingErr Reason;
    }

    public class AttackStop : ClientPacket
    {
        public AttackStop(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class SAttackStart : ServerPacket
    {
        public SAttackStart() : base(Opcode.SMSG_ATTACK_START, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Attacker);
            _worldPacket.WritePackedGuid128(Victim);
        }

        public WowGuid128 Attacker;
        public WowGuid128 Victim;
    }

    public class SAttackStop : ServerPacket
    {
        public SAttackStop() : base(Opcode.SMSG_ATTACK_STOP, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Attacker);
            _worldPacket.WritePackedGuid128(Victim);
            _worldPacket.WriteBit(NowDead);
            _worldPacket.FlushBits();
        }

        public WowGuid128 Attacker;
        public WowGuid128 Victim;
        public bool NowDead;
    }

    class AttackerStateUpdate : ServerPacket
    {
        public AttackerStateUpdate() : base(Opcode.SMSG_ATTACKER_STATE_UPDATE, ConnectionType.Instance) { }
        public override void Write()
        {
            WorldPacket attackRoundInfo = new();
            attackRoundInfo.WriteUInt32((uint)HitInfo);
            attackRoundInfo.WritePackedGuid128(AttackerGUID);
            attackRoundInfo.WritePackedGuid128(VictimGUID);
            attackRoundInfo.WriteInt32(Damage);
            attackRoundInfo.WriteInt32(OriginalDamage);
            attackRoundInfo.WriteInt32(OverDamage);
            attackRoundInfo.WriteUInt8((byte)SubDmg.Count);

            foreach (var subDmg in SubDmg)
            {
                attackRoundInfo.WriteUInt32(subDmg.SchoolMask);
                attackRoundInfo.WriteFloat(subDmg.FloatDamage);
                attackRoundInfo.WriteInt32(subDmg.IntDamage);
                if (HitInfo.HasAnyFlag(HitInfo.FullAbsorb | HitInfo.PartialAbsorb))
                    attackRoundInfo.WriteInt32(subDmg.Absorbed);
                if (HitInfo.HasAnyFlag(HitInfo.FullResist | HitInfo.PartialResist))
                    attackRoundInfo.WriteInt32(subDmg.Resisted);
            }

            attackRoundInfo.WriteUInt8(VictimState);
            attackRoundInfo.WriteInt32(AttackerState);
            attackRoundInfo.WriteUInt32(MeleeSpellID);

            if (HitInfo.HasAnyFlag(HitInfo.Block))
                attackRoundInfo.WriteInt32(BlockAmount);

            if (HitInfo.HasAnyFlag(HitInfo.RageGain))
                attackRoundInfo.WriteInt32(RageGained);

            if (HitInfo.HasAnyFlag(HitInfo.Unk0))
            {
                attackRoundInfo.WriteUInt32(UnkState.State1);
                attackRoundInfo.WriteFloat(UnkState.State2);
                attackRoundInfo.WriteFloat(UnkState.State3);
                attackRoundInfo.WriteFloat(UnkState.State4);
                attackRoundInfo.WriteFloat(UnkState.State5);
                attackRoundInfo.WriteFloat(UnkState.State6);
                attackRoundInfo.WriteFloat(UnkState.State7);
                attackRoundInfo.WriteFloat(UnkState.State8);
                attackRoundInfo.WriteFloat(UnkState.State9);
                attackRoundInfo.WriteFloat(UnkState.State10);
                attackRoundInfo.WriteFloat(UnkState.State11);
                attackRoundInfo.WriteUInt32(UnkState.State12);
            }

            if (HitInfo.HasAnyFlag(HitInfo.Block | HitInfo.Unk12))
                attackRoundInfo.WriteFloat(Unk);

            attackRoundInfo.WriteUInt8((byte)ContentTuning.TuningType);
            attackRoundInfo.WriteUInt8(ContentTuning.TargetLevel);
            attackRoundInfo.WriteUInt8(ContentTuning.Expansion);
            attackRoundInfo.WriteInt16(ContentTuning.PlayerLevelDelta);
            attackRoundInfo.WriteFloat(ContentTuning.PlayerItemLevel);
            attackRoundInfo.WriteFloat(ContentTuning.TargetItemLevel);

            _worldPacket.WriteBit(LogData != null);
            if (LogData != null)
                LogData.Write(_worldPacket);
            _worldPacket.FlushBits();

            _worldPacket.WriteUInt32(attackRoundInfo.GetSize());
            _worldPacket.WriteBytes(attackRoundInfo);
        }

        public HitInfo HitInfo; // Flags
        public WowGuid128 AttackerGUID;
        public WowGuid128 VictimGUID;
        public int Damage;
        public int OriginalDamage;
        public int OverDamage = -1; // (damage - health) or -1 if unit is still alive
        public List<SubDamage> SubDmg = new();
        public byte VictimState;
        public int AttackerState = 0;
        public uint MeleeSpellID = 0;
        public int BlockAmount;
        public int RageGained = 0;
        public UnkAttackerState UnkState;
        public float Unk = 0.0f;
        public ContentTuningParams ContentTuning = new();
        public SpellCastLogData LogData;
    }

    public class SubDamage
    {
        public uint SchoolMask;
        public float FloatDamage; // Float damage (Most of the time equals to Damage)
        public int IntDamage;
        public int Absorbed;
        public int Resisted;
    }

    public struct UnkAttackerState
    {
        public uint State1;
        public float State2;
        public float State3;
        public float State4;
        public float State5;
        public float State6;
        public float State7;
        public float State8;
        public float State9;
        public float State10;
        public float State11;
        public uint State12;
    }

    public class SpellCastLogData
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt64(Health);
            data.WriteInt32(AttackPower);
            data.WriteInt32(SpellPower);
            data.WriteUInt32(Armor);
            data.WriteBits(PowerData.Count, 9);
            data.FlushBits();

            foreach (SpellLogPowerData powerData in PowerData)
            {
                data.WriteInt32(powerData.PowerType);
                data.WriteInt32(powerData.Amount);
                data.WriteInt32(powerData.Cost);
            }
        }

        long Health;
        int AttackPower;
        int SpellPower;
        uint Armor;
        List<SpellLogPowerData> PowerData = new();
    }

    public struct SpellLogPowerData
    {
        public int PowerType;
        public int Amount;
        public int Cost;
    }

    public class CancelCombat : ServerPacket
    {
        public CancelCombat() : base(Opcode.SMSG_CANCEL_COMBAT) { }

        public override void Write() { }
    }

    public class SetSheathed : ClientPacket
    {
        public SetSheathed(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SheathState = _worldPacket.ReadInt32();
            Animate = _worldPacket.HasBit();
        }

        public int SheathState;
        public bool Animate = true;
    }

    public class AIReaction : ServerPacket
    {
        public AIReaction() : base(Opcode.SMSG_AI_REACTION, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(UnitGUID);
            _worldPacket.WriteUInt32(Reaction);
        }

        public WowGuid128 UnitGUID;
        public uint Reaction;
    }

    class PartyKillLog : ServerPacket
    {
        public PartyKillLog() : base(Opcode.SMSG_PARTY_KILL_LOG) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Player);
            _worldPacket.WritePackedGuid128(Victim);
        }

        public WowGuid128 Player;
        public WowGuid128 Victim;
    }
}
