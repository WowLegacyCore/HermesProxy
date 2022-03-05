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
    public class SendKnownSpells : ServerPacket
    {
        public SendKnownSpells() : base(Opcode.SMSG_SEND_KNOWN_SPELLS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteBit(InitialLogin);
            _worldPacket.WriteInt32(KnownSpells.Count);
            _worldPacket.WriteInt32(FavoriteSpells.Count);

            foreach (var spellId in KnownSpells)
                _worldPacket.WriteUInt32(spellId);

            foreach (var spellId in FavoriteSpells)
                _worldPacket.WriteUInt32(spellId);
        }

        public bool InitialLogin;
        public List<uint> KnownSpells = new();
        public List<uint> FavoriteSpells = new(); // tradeskill recipes
    }

    public class LearnedSpells : ServerPacket
    {
        public LearnedSpells() : base(Opcode.SMSG_LEARNED_SPELLS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Spells.Count);
            _worldPacket.WriteInt32(FavoriteSpellID.Count);
            _worldPacket.WriteUInt32(SpecializationID);

            foreach (uint spell in Spells)
                _worldPacket.WriteUInt32(spell);

            foreach (int spell in FavoriteSpellID)
                _worldPacket.WriteInt32(spell);

            _worldPacket.WriteBit(SuppressMessaging);
            _worldPacket.FlushBits();
        }

        public List<uint> Spells = new();
        public List<int> FavoriteSpellID = new();
        public uint SpecializationID;
        public bool SuppressMessaging;
    }

    public class SendUnlearnSpells : ServerPacket
    {
        public SendUnlearnSpells() : base(Opcode.SMSG_SEND_UNLEARN_SPELLS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Spells.Count);
            foreach (var spell in Spells)
                _worldPacket.WriteUInt32(spell);
        }
        public List<uint> Spells = new();
    }

    public class UnlearnedSpells : ServerPacket
    {
        public UnlearnedSpells() : base(Opcode.SMSG_UNLEARNED_SPELLS, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Spells.Count);
            foreach (uint spellId in Spells)
                _worldPacket.WriteUInt32(spellId);

            _worldPacket.WriteBit(SuppressMessaging);
            _worldPacket.FlushBits();
        }

        public List<uint> Spells = new();
        public bool SuppressMessaging;
    }

    public class SendSpellHistory : ServerPacket
    {
        public SendSpellHistory() : base(Opcode.SMSG_SEND_SPELL_HISTORY, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Entries.Count);
            Entries.ForEach(p => p.Write(_worldPacket));
        }

        public List<SpellHistoryEntry> Entries = new();
    }

    public class SpellHistoryEntry
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(SpellID);
            data.WriteUInt32(ItemID);
            data.WriteUInt32(Category);
            data.WriteInt32(RecoveryTime);
            data.WriteInt32(CategoryRecoveryTime);
            data.WriteFloat(ModRate);
            data.WriteBit(unused622_1.HasValue);
            data.WriteBit(unused622_2.HasValue);
            data.WriteBit(OnHold);
            data.FlushBits();

            if (unused622_1.HasValue)
                data.WriteUInt32(unused622_1.Value);
            if (unused622_2.HasValue)
                data.WriteUInt32(unused622_2.Value);
        }

        public uint SpellID;
        public uint ItemID;
        public uint Category;
        public int RecoveryTime;
        public int CategoryRecoveryTime;
        public float ModRate = 1.0f;
        public bool OnHold;
        uint? unused622_1;   // This field is not used for anything in the client in 6.2.2.20444
        uint? unused622_2;   // This field is not used for anything in the client in 6.2.2.20444
    }

    public class SendSpellCharges : ServerPacket
    {
        public SendSpellCharges() : base(Opcode.SMSG_SEND_SPELL_CHARGES, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Entries.Count);
            Entries.ForEach(p => p.Write(_worldPacket));
        }

        public List<SpellChargeEntry> Entries = new();
    }

    public class SpellChargeEntry
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Category);
            data.WriteUInt32(NextRecoveryTime);
            data.WriteFloat(ChargeModRate);
            data.WriteUInt8(ConsumedCharges);
        }

        public uint Category;
        public uint NextRecoveryTime;
        public float ChargeModRate = 1.0f;
        public byte ConsumedCharges;
    }

    class CancelAura : ClientPacket
    {
        public CancelAura(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            SpellID = _worldPacket.ReadUInt32();
            CasterGUID = _worldPacket.ReadPackedGuid128();
        }

        public uint SpellID;
        public WowGuid128 CasterGUID;
    }

    public class AuraUpdate : ServerPacket
    {
        public AuraUpdate(WowGuid128 guid, bool all) : base(Opcode.SMSG_AURA_UPDATE, ConnectionType.Instance) 
        {
            UnitGUID = guid;
            UpdateAll = all;
        }

        public override void Write()
        {
            _worldPacket.WriteBit(UpdateAll);
            _worldPacket.WriteBits(Auras.Count, 9);
            foreach (AuraInfo aura in Auras)
                aura.Write(_worldPacket);

            _worldPacket.WritePackedGuid128(UnitGUID);
        }

        public bool UpdateAll;
        public WowGuid128 UnitGUID;
        public List<AuraInfo> Auras = new();
    }

    public struct AuraInfo
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Slot);
            data.WriteBit(AuraData != null);
            data.FlushBits();

            if (AuraData != null)
                AuraData.Write(data);
        }

        public byte Slot;
        public AuraDataInfo AuraData;
    }

    public class AuraDataInfo
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(CastID);
            data.WriteInt32(SpellID);
            data.WriteUInt32(SpellXSpellVisualID);
            data.WriteUInt16((ushort)Flags);
            data.WriteUInt32(ActiveFlags);
            data.WriteUInt16(CastLevel);
            data.WriteUInt8(Applications);
            data.WriteInt32(ContentTuningID);
            data.WriteBit(CastUnit != null);
            data.WriteBit(Duration.HasValue);
            data.WriteBit(Remaining.HasValue);
            data.WriteBit(TimeMod.HasValue);
            data.WriteBits(Points.Count, 6);
            data.WriteBits(EstimatedPoints.Count, 6);
            data.WriteBit(ContentTuning != null);

            if (ContentTuning != null)
                ContentTuning.Write(data);

            if (CastUnit != null)
                data.WritePackedGuid128(CastUnit);

            if (Duration.HasValue)
                data.WriteInt32(Duration.Value);

            if (Remaining.HasValue)
                data.WriteInt32(Remaining.Value);

            if (TimeMod.HasValue)
                data.WriteFloat(TimeMod.Value);

            foreach (var point in Points)
                data.WriteFloat(point);

            foreach (var point in EstimatedPoints)
                data.WriteFloat(point);
        }

        public WowGuid128 CastID;
        public int SpellID;
        public uint SpellXSpellVisualID;
        public AuraFlagsModern Flags;
        public uint ActiveFlags;
        public ushort CastLevel = 1;
        public byte Applications = 1;
        public int ContentTuningID;
        ContentTuningParams ContentTuning;
        public WowGuid128 CastUnit;
        public int? Duration;
        public int? Remaining;
        float? TimeMod;
        public List<float> Points = new();
        public List<float> EstimatedPoints = new();
    }

    class ContentTuningParams
    {
        public void Write(WorldPacket data)
        {
            data.WriteFloat(PlayerItemLevel);
            data.WriteFloat(TargetItemLevel);
            data.WriteInt16(PlayerLevelDelta);
            data.WriteUInt16(ScalingHealthItemLevelCurveID);
            data.WriteUInt8(TargetLevel);
            data.WriteUInt8(Expansion);
            data.WriteUInt8(TargetMinScalingLevel);
            data.WriteUInt8(TargetMaxScalingLevel);
            data.WriteInt8(TargetScalingLevelDelta);
            data.WriteUInt32((uint)Flags);
            data.WriteBits(TuningType, 4);
            data.FlushBits();
        }

        public ContentTuningType TuningType;
        public short PlayerLevelDelta;
        public float PlayerItemLevel;
        public float TargetItemLevel = 0.0f;
        public ushort ScalingHealthItemLevelCurveID;
        public byte TargetLevel;
        public byte Expansion;
        public byte TargetMinScalingLevel;
        public byte TargetMaxScalingLevel;
        public sbyte TargetScalingLevelDelta;
        public ContentTuningFlags Flags = ContentTuningFlags.NoLevelScaling | ContentTuningFlags.NoItemLevelScaling;

        public enum ContentTuningType
        {
            CreatureToPlayerDamage = 1,
            PlayerToCreatureDamage = 2,
            CreatureToCreatureDamage = 4,
            PlayerToSandboxScaling = 7, // NYI
            PlayerToPlayerExpectedStat = 8
        }

        public enum ContentTuningFlags
        {
            NoLevelScaling = 0x1,
            NoItemLevelScaling = 0x2
        }
    }

    public class CastSpell : ClientPacket
    {
        public SpellCastRequest Cast;

        public CastSpell(WorldPacket packet) : base(packet)
        {
            Cast = new SpellCastRequest();
        }

        public override void Read()
        {
            Cast.Read(_worldPacket);
        }
    }

    public class UseItem : ClientPacket
    {
        public byte PackSlot;
        public byte Slot;
        public WowGuid128 CastItem;
        public SpellCastRequest Cast;

        public UseItem(WorldPacket packet) : base(packet)
        {
            Cast = new SpellCastRequest();
        }

        public override void Read()
        {
            PackSlot = _worldPacket.ReadUInt8();
            Slot = _worldPacket.ReadUInt8();
            CastItem = _worldPacket.ReadPackedGuid128();
            Cast.Read(_worldPacket);
        }
    }

    public class SpellCastRequest
    {
        public WowGuid128 CastID;
        public uint SpellID;
        public uint SpellXSpellVisualID;
        public uint SendCastFlags;
        public SpellTargetData Target = new();
        public MissileTrajectoryRequest MissileTrajectory;
        public WowGuid128 MoverGUID;
        public MovementInfo MoveUpdate;
        public List<SpellWeight> Weight = new();
        public Array<SpellOptionalReagent> OptionalReagents = new(3);
        public Array<SpellExtraCurrencyCost> OptionalCurrencies = new(5 /*MAX_ITEM_EXT_COST_CURRENCIES*/);
        public WowGuid128 CraftingNPC;
        public uint[] Misc = new uint[2];

        public void Read(WorldPacket data)
        {
            CastID = data.ReadPackedGuid128();
            Misc[0] = data.ReadUInt32();
            Misc[1] = data.ReadUInt32();
            SpellID = data.ReadUInt32();

            SpellXSpellVisualID = data.ReadUInt32();

            MissileTrajectory.Read(data);
            CraftingNPC = data.ReadPackedGuid128();

            var optionalReagents = data.ReadUInt32();
            var optionalCurrencies = data.ReadUInt32();

            for (var i = 0; i < optionalReagents; ++i)
                OptionalReagents[i].Read(data);

            for (var i = 0; i < optionalCurrencies; ++i)
                OptionalCurrencies[i].Read(data);

            SendCastFlags = data.ReadBits<uint>(5);
            if (data.HasBit())
                MoveUpdate = new();
            var weightCount = data.ReadBits<uint>(2);
            Target.Read(data);

            if (MoveUpdate != null)
            {
                MoverGUID = data.ReadPackedGuid128();
                MoveUpdate.ReadMovementInfoModern(data);
            }

            for (var i = 0; i < weightCount; ++i)
            {
                data.ResetBitPos();
                SpellWeight weight;
                weight.Type = data.ReadBits<uint>(2);
                weight.ID = data.ReadInt32();
                weight.Quantity = data.ReadUInt32();
                Weight.Add(weight);
            }
        }
    }

    public struct MissileTrajectoryRequest
    {
        public float Pitch;
        public float Speed;

        public void Read(WorldPacket data)
        {
            Pitch = data.ReadFloat();
            Speed = data.ReadFloat();
        }
    }

    public struct SpellWeight
    {
        public uint Type;
        public int ID;
        public uint Quantity;
    }

    public struct SpellOptionalReagent
    {
        public int ItemID;
        public int Slot;
        public int Count;

        public void Read(WorldPacket data)
        {
            ItemID = data.ReadInt32();
            Slot = data.ReadInt32();
            Count = data.ReadInt32();
        }
    }

    public struct SpellExtraCurrencyCost
    {
        public int CurrencyID;
        public int Slot;
        public int Count;

        public void Read(WorldPacket data)
        {
            CurrencyID = data.ReadInt32();
            Slot = data.ReadInt32();
            Count = data.ReadInt32();
        }
    }

    public class CancelCast : ClientPacket
    {
        public CancelCast(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            CastID = _worldPacket.ReadPackedGuid128();
            SpellID = _worldPacket.ReadUInt32();
        }

        public uint SpellID;
        public WowGuid128 CastID;
    }

    class SpellPrepare : ServerPacket
    {
        public SpellPrepare() : base(Opcode.SMSG_SPELL_PREPARE) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(ClientCastID);
            _worldPacket.WritePackedGuid128(ServerCastID);
        }

        public WowGuid128 ClientCastID;
        public WowGuid128 ServerCastID;
    }

    class CastFailed : ServerPacket
    {
        public WowGuid128 CastID;
        public int SpellID;
        public uint Reason;
        public int FailedArg1 = -1;
        public int FailedArg2 = -1;
        public uint SpellXSpellVisualID;

        public CastFailed() : base(Opcode.SMSG_CAST_FAILED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(CastID);
            _worldPacket.WriteInt32(SpellID);
            _worldPacket.WriteUInt32(SpellXSpellVisualID);
            _worldPacket.WriteUInt32(Reason);
            _worldPacket.WriteInt32(FailedArg1);
            _worldPacket.WriteInt32(FailedArg2);
        }
    }

    class PetCastFailed : ServerPacket
    {
        public PetCastFailed() : base(Opcode.SMSG_PET_CAST_FAILED, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(CastID);
            _worldPacket.WriteUInt32(SpellID);
            _worldPacket.WriteUInt32(Reason);
            _worldPacket.WriteInt32(FailedArg1);
            _worldPacket.WriteInt32(FailedArg2);
        }

        public WowGuid128 CastID;
        public uint SpellID;
        public uint Reason;
        public int FailedArg1 = -1;
        public int FailedArg2 = -1;
    }

    public class SpellFailure : ServerPacket
    {
        public SpellFailure() : base(Opcode.SMSG_SPELL_FAILURE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(CasterUnit);
            _worldPacket.WritePackedGuid128(CastID);
            _worldPacket.WriteUInt32(SpellID);
            _worldPacket.WriteUInt32(SpellXSpellVisualID);
            _worldPacket.WriteUInt16(Reason);
        }

        public WowGuid128 CasterUnit;
        public WowGuid128 CastID;
        public uint SpellID;
        public uint SpellXSpellVisualID;
        public ushort Reason;
    }

    public class SpellFailedOther : ServerPacket
    {
        public SpellFailedOther() : base(Opcode.SMSG_SPELL_FAILED_OTHER, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(CasterUnit);
            _worldPacket.WritePackedGuid128(CastID);
            _worldPacket.WriteUInt32(SpellID);
            _worldPacket.WriteUInt32(SpellXSpellVisualID);
            _worldPacket.WriteUInt8(Reason);
        }

        public WowGuid128 CasterUnit;
        public WowGuid128 CastID;
        public uint SpellID;
        public uint SpellXSpellVisualID;
        public byte Reason;
    }

    public class SpellStart : ServerPacket
    {
        public SpellCastData Cast;

        public SpellStart() : base(Opcode.SMSG_SPELL_START, ConnectionType.Instance)
        {
            Cast = new SpellCastData();
        }

        public override void Write()
        {
            Cast.Write(_worldPacket);
        }
    }

    class SpellGo : ServerPacket
    {
        public SpellGo() : base(Opcode.SMSG_SPELL_GO, ConnectionType.Instance) { }

        public override void Write()
        {
            Cast.Write(_worldPacket);

            _worldPacket.WriteBit(LogData != null);
            if (LogData != null)
                LogData.Write(_worldPacket);
            _worldPacket.FlushBits();
        }

        public SpellCastData Cast = new();
        public SpellCastLogData LogData;
    }

    public class SpellCastData
    {
        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(CasterGUID);
            data.WritePackedGuid128(CasterUnit);
            data.WritePackedGuid128(CastID);
            data.WritePackedGuid128(OriginalCastID);
            data.WriteInt32(SpellID);
            data.WriteUInt32(SpellXSpellVisualID);
            data.WriteUInt32(CastFlags);
            data.WriteUInt32(CastFlagsEx);
            data.WriteUInt32(CastTime);

            MissileTrajectory.Write(data);

            data.WriteUInt8(DestLocSpellCastIndex);

            Immunities.Write(data);
            Predict.Write(data);

            data.WriteBits(HitTargets.Count, 16);
            data.WriteBits(MissTargets.Count, 16);
            data.WriteBits(MissStatus.Count, 16);
            data.WriteBits(RemainingPower.Count, 9);
            data.WriteBit(RemainingRunes != null);
            data.WriteBits(TargetPoints.Count, 16);
            data.WriteBit(AmmoDisplayId != null);
            data.WriteBit(AmmoInventoryType != null);
            data.FlushBits();

            foreach (SpellMissStatus missStatus in MissStatus)
                missStatus.Write(data);

            Target.Write(data);

            foreach (WowGuid128 hitTarget in HitTargets)
                data.WritePackedGuid128(hitTarget);

            foreach (WowGuid128 missTarget in MissTargets)
                data.WritePackedGuid128(missTarget);

            foreach (SpellPowerData power in RemainingPower)
                power.Write(data);

            if (RemainingRunes != null)
                RemainingRunes.Write(data);

            foreach (TargetLocation targetLoc in TargetPoints)
                targetLoc.Write(data);

            if (AmmoDisplayId != null)
                data.WriteInt32((int)AmmoDisplayId);

            if (AmmoInventoryType != null)
                data.WriteInt32((int)AmmoInventoryType);
        }

        public WowGuid128 CasterGUID;
        public WowGuid128 CasterUnit;
        public WowGuid128 CastID = WowGuid128.Empty;
        public WowGuid128 OriginalCastID = WowGuid128.Empty;
        public int SpellID;
        public uint SpellXSpellVisualID;
        public uint CastFlags;
        public uint CastFlagsEx;
        public uint CastTime;
        public List<WowGuid128> HitTargets = new();
        public List<WowGuid128> MissTargets = new();
        public List<SpellMissStatus> MissStatus = new();
        public SpellTargetData Target = new();
        public List<SpellPowerData> RemainingPower = new();
        public RuneData RemainingRunes;
        public MissileTrajectoryResult MissileTrajectory;
        public int? AmmoDisplayId;
        public int? AmmoInventoryType;
        public byte DestLocSpellCastIndex;
        public List<TargetLocation> TargetPoints = new();
        public CreatureImmunities Immunities;
        public SpellHealPrediction Predict = new();
    }

    public struct SpellMissStatus
    {
        public SpellMissStatus(SpellMissInfo reason, SpellMissInfo reflectStatus)
        {
            Reason = reason;
            ReflectStatus = reflectStatus;
        }

        public void Write(WorldPacket data)
        {
            data.WriteBits((byte)Reason, 4);
            if (Reason == SpellMissInfo.Reflect)
                data.WriteBits(ReflectStatus, 4);

            data.FlushBits();
        }

        public SpellMissInfo Reason;
        public SpellMissInfo ReflectStatus;
    }

    public class TargetLocation
    {
        public WowGuid128 Transport = WowGuid128.Empty;
        public Vector3 Location;

        public void Read(WorldPacket data)
        {
            Transport = data.ReadPackedGuid128();
            Location = data.ReadVector3();
        }

        public void Write(WorldPacket data)
        {
            data.WritePackedGuid128(Transport);
            data.WriteVector3(Location);
        }
    }

    public class SpellTargetData
    {
        public void Read(WorldPacket data)
        {
            Flags = (SpellCastTargetFlags)data.ReadBits<uint>(26);
            if (data.HasBit())
                SrcLocation = new();
            if (data.HasBit())
                DstLocation = new();
            if (data.HasBit())
                Orientation = new();
            if (data.HasBit())
                MapID = new();
            uint nameLength = data.ReadBits<uint>(7);

            Unit = data.ReadPackedGuid128();
            Item = data.ReadPackedGuid128();

            if (SrcLocation != null)
                SrcLocation.Read(data);

            if (DstLocation != null)
                DstLocation.Read(data);

            if (Orientation != null)
                Orientation = data.ReadFloat();

            if (MapID != null)
                MapID = data.ReadInt32();

            Name = data.ReadString(nameLength);
        }

        public void Write(WorldPacket data)
        {
            data.WriteBits((uint)Flags, 26);
            data.WriteBit(SrcLocation != null);
            data.WriteBit(DstLocation != null);
            data.WriteBit(Orientation.HasValue);
            data.WriteBit(MapID.HasValue);
            data.WriteBits(Name.GetByteCount(), 7);
            data.FlushBits();

            data.WritePackedGuid128(Unit);
            data.WritePackedGuid128(Item);

            if (SrcLocation != null)
                SrcLocation.Write(data);

            if (DstLocation != null)
                DstLocation.Write(data);

            if (Orientation.HasValue)
                data.WriteFloat(Orientation.Value);

            if (MapID.HasValue)
                data.WriteInt32(MapID.Value);

            data.WriteString(Name);
        }

        public SpellCastTargetFlags Flags;
        public WowGuid128 Unit;
        public WowGuid128 Item;
        public TargetLocation SrcLocation;
        public TargetLocation DstLocation;
        public float? Orientation;
        public int? MapID;
        public string Name = "";
    }

    public struct SpellPowerData
    {
        public int Cost;
        public PowerType Type;

        public void Write(WorldPacket data)
        {
            data.WriteInt32(Cost);
            data.WriteInt8((sbyte)Type);
        }
    }

    public class RuneData
    {
        public void Write(WorldPacket data)
        {
            data.WriteUInt8(Start);
            data.WriteUInt8(Count);
            data.WriteInt32(Cooldowns.Count);

            foreach (byte cd in Cooldowns)
                data.WriteUInt8(cd);
        }

        public byte Start;
        public byte Count;
        public List<byte> Cooldowns = new();
    }

    public struct MissileTrajectoryResult
    {
        public uint TravelTime;
        public float Pitch;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(TravelTime);
            data.WriteFloat(Pitch);
        }
    }

    public struct CreatureImmunities
    {
        public uint School;
        public uint Value;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(School);
            data.WriteUInt32(Value);
        }
    }

    public class SpellHealPrediction
    {
        public WowGuid128 BeaconGUID = WowGuid128.Empty;
        public uint Points;
        public byte Type;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(Points);
            data.WriteUInt8(Type);
            data.WritePackedGuid128(BeaconGUID);
        }
    }

    class LearnTalent : ClientPacket
    {
        public LearnTalent(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            TalentID = _worldPacket.ReadUInt32();
            Rank = _worldPacket.ReadUInt16();
        }

        public uint TalentID;
        public ushort Rank;
    }
}
