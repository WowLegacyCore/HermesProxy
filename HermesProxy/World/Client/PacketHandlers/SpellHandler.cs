using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_SEND_KNOWN_SPELLS)]
        void HandleSendKnownSpells(WorldPacket packet)
        {
            SendKnownSpells spells = new SendKnownSpells();
            spells.InitialLogin = packet.ReadBool();
            ushort spellCount = packet.ReadUInt16();
            for (ushort i = 0; i < spellCount; i++)
            {
                uint spellId;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    spellId = packet.ReadUInt32();
                else
                    spellId = packet.ReadUInt16();
                spells.KnownSpells.Add(spellId);
                packet.ReadInt16();
            }
            SendPacketToClient(spells);

            ushort cooldownCount = packet.ReadUInt16();
            if (cooldownCount != 0)
            {
                SendSpellHistory histories = new SendSpellHistory();
                for (ushort i = 0; i < cooldownCount; i++)
                {
                    SpellHistoryEntry history = new SpellHistoryEntry();

                    uint spellId;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                        spellId = packet.ReadUInt32();
                    else
                        spellId = packet.ReadUInt16();
                    history.SpellID = spellId;

                    uint itemId;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                        itemId = packet.ReadUInt32();
                    else
                        itemId = packet.ReadUInt16();
                    history.ItemID = itemId;

                    history.Category = packet.ReadUInt16();
                    history.RecoveryTime = packet.ReadInt32();
                    history.CategoryRecoveryTime = packet.ReadInt32();

                    histories.Entries.Add(history);
                }
                SendPacketToClient(histories, Opcode.SMSG_SEND_UNLEARN_SPELLS);
            }

            // These packets don't exist in Vanilla.
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                SendPacketToClient(new SendUnlearnSpells());
                SendPacketToClient(new SendSpellCharges());
            }
        }

        [PacketHandler(Opcode.SMSG_LEARNED_SPELL)]
        void HandleLearnedSpell(WorldPacket packet)
        {
            LearnedSpells spells = new LearnedSpells();
            uint spellId = packet.ReadUInt32();
            spells.Spells.Add(spellId);
            SendPacketToClient(spells);
        }

        [PacketHandler(Opcode.SMSG_SEND_UNLEARN_SPELLS)]
        void HandleSendUnlearnSpells(WorldPacket packet)
        {
            SendUnlearnSpells spells = new SendUnlearnSpells();
            uint spellCount = packet.ReadUInt32();
            for (uint i = 0; i < spellCount; i++)
            {
                uint spellId = packet.ReadUInt32();
                spells.Spells.Add(spellId);
            }
            SendPacketToClient(spells);
        }

        [PacketHandler(Opcode.SMSG_UNLEARNED_SPELLS)]
        void HandleUnlearnedSpells(WorldPacket packet)
        {
            UnlearnedSpells spells = new UnlearnedSpells();
            uint spellId;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                spellId = packet.ReadUInt32();
            else
                spellId = packet.ReadUInt16();
            spells.Spells.Add(spellId);
            SendPacketToClient(spells);
        }

        [PacketHandler(Opcode.SMSG_PET_CAST_FAILED, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandlePetCastFailed(WorldPacket packet)
        {
            uint spellId = packet.ReadUInt32();
            var status = packet.ReadUInt8();
            if (status != 2)
                return;

            SpellPrepare prepare = new();
            prepare.ClientCastID = WowGuid128.Empty;
            prepare.ServerCastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)Global.CurrentSessionData.GameState.CurrentMapId, spellId, spellId);
            SendPacketToClient(prepare);

            PetCastFailed spell = new PetCastFailed();
            spell.CastID = prepare.ServerCastID;
            spell.SpellID = spellId;
            uint reason = packet.ReadUInt8();
            spell.Reason = LegacyVersion.ConvertSpellCastResult(reason);
            SendPacketToClient(spell);
        }

        [PacketHandler(Opcode.SMSG_CAST_FAILED, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleCastFailed(WorldPacket packet)
        {
            int spellId = packet.ReadInt32();
            var status = packet.ReadUInt8();
            if (status != 2)
            {
                if (Global.CurrentSessionData.GameState.LastClientCastGuid != null)
                {
                    SpellPrepare prepare = new();
                    prepare.ClientCastID = Global.CurrentSessionData.GameState.LastClientCastGuid;
                    prepare.ServerCastID = WowGuid128.Empty;
                    SendPacketToClient(prepare);
                }
                return;
            }

            CastFailed failed = new();
            failed.SpellID = spellId;
            failed.SpellXSpellVisualID = GameData.GetSpellVisual((uint)spellId);
            failed.CastID = Global.CurrentSessionData.GameState.LastClientCastGuid;
            uint reason = packet.ReadUInt8();
            failed.Reason = LegacyVersion.ConvertSpellCastResult(reason);
            switch ((SpellCastResultVanilla)reason)
            {
                case SpellCastResultVanilla.RequiresSpellFocus:
                {
                    failed.FailedArg1 = packet.ReadInt32(); // Required Spell Focus
                    break;
                }
                case SpellCastResultVanilla.RequiresArea:
                {
                    failed.FailedArg1 = packet.ReadInt32(); // Required Area
                    break;
                }
                case SpellCastResultVanilla.EquippedItemClass:
                {
                    failed.FailedArg1 = packet.ReadInt32(); // Equipped Item Class
                    failed.FailedArg2 = packet.ReadInt32(); // Equipped Item Sub Class Mask
                    packet.ReadUInt32(); // Equipped Item Inventory Type Mask
                    break;
                }
            }
            SendPacketToClient(failed);
        }

        [PacketHandler(Opcode.SMSG_SPELL_FAILED_OTHER)]
        void HandleSpellFailedOther(WorldPacket packet)
        {
            WowGuid128 casterUnit;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                casterUnit = packet.ReadPackedGuid().To128();
            else
                casterUnit = packet.ReadGuid().To128();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadUInt8(); // Cast Count
            uint spellId = packet.ReadUInt32();
            uint spellVisual = GameData.GetSpellVisual(spellId);
            byte reason = 61;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                reason = (byte)LegacyVersion.ConvertSpellCastResult(packet.ReadUInt8());
            WowGuid128 castId = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)Global.CurrentSessionData.GameState.CurrentMapId, spellId, spellId + casterUnit.GetLow());

            SpellFailure spell = new SpellFailure();
            spell.CasterUnit = casterUnit;
            spell.CastID = castId;
            spell.SpellID = spellId;
            spell.SpellXSpellVisualID = spellVisual;
            spell.Reason = reason;
            SendPacketToClient(spell);

            SpellFailedOther spell2 = new SpellFailedOther();
            spell2.CasterUnit = casterUnit;
            spell2.CastID = castId;
            spell2.SpellID = spellId;
            spell2.SpellXSpellVisualID = spellVisual;
            spell2.Reason = reason;
            SendPacketToClient(spell2);
        }

        [PacketHandler(Opcode.SMSG_SPELL_START)]
        void HandleSpellStart(WorldPacket packet)
        {
            SpellStart spell = new SpellStart();
            spell.Cast = HandleSpellStart(packet, false);
            SendPacketToClient(spell);
        }

        [PacketHandler(Opcode.SMSG_SPELL_GO)]
        void HandleSpellGo(WorldPacket packet)
        {
            SpellGo spell = new SpellGo();
            spell.Cast = HandleSpellStart(packet, true);
            SendPacketToClient(spell);
        }

        public static SpellCastData HandleSpellStart(WorldPacket packet, bool isSpellGo)
        {
            SpellCastData dbdata = new SpellCastData();
            
            dbdata.CasterGUID = packet.ReadPackedGuid().To128();
            dbdata.CasterUnit = packet.ReadPackedGuid().To128();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadUInt8(); // cast count

            dbdata.SpellID = packet.ReadInt32();
            dbdata.SpellXSpellVisualID = GameData.GetSpellVisual((uint)dbdata.SpellID);
            dbdata.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)Global.CurrentSessionData.GameState.CurrentMapId, (uint)dbdata.SpellID, (ulong)dbdata.SpellID + dbdata.CasterUnit.GetLow());

            if (Global.CurrentSessionData.GameState.CurrentPlayerGuid == dbdata.CasterUnit &&
                Global.CurrentSessionData.GameState.LastClientCastId == dbdata.SpellID &&
                Global.CurrentSessionData.GameState.LastClientCastGuid != null)
            {
                dbdata.OriginalCastID = Global.CurrentSessionData.GameState.LastClientCastGuid;
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) && !isSpellGo)
                packet.ReadUInt8(); // cast count

            uint flags;
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                flags = packet.ReadUInt32();
            else
                flags = packet.ReadUInt16();
            dbdata.CastFlags = flags;

            if (!isSpellGo || LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                dbdata.CastTime = packet.ReadUInt32();

            if (isSpellGo)
            {
                var hitCount = packet.ReadUInt8();
                for (var i = 0; i < hitCount; i++)
                {
                    WowGuid128 hitTarget = packet.ReadGuid().To128();
                    dbdata.HitTargets.Add(hitTarget);
                }

                var missCount = packet.ReadUInt8();
                for (var i = 0; i < missCount; i++)
                {
                    WowGuid128 missTarget = packet.ReadGuid().To128();
                    SpellMissInfo missType = (SpellMissInfo)packet.ReadUInt8();
                    SpellMissInfo reflectType = SpellMissInfo.None;
                    if (missType == SpellMissInfo.Reflect)
                        reflectType = (SpellMissInfo)packet.ReadUInt8();

                    dbdata.MissTargets.Add(missTarget);
                    dbdata.MissStatus.Add(new SpellMissStatus(missType, reflectType));
                }
            }

            var targetFlags = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 
                (SpellCastTargetFlags)packet.ReadUInt32() : (SpellCastTargetFlags)packet.ReadUInt16();
            dbdata.Target.Flags = targetFlags;

            WowGuid128 unitTarget = WowGuid128.Empty;
            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.GameObject |
                SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.UnitMinipet))
                unitTarget = packet.ReadPackedGuid().To128();
            dbdata.Target.Unit = unitTarget;

            WowGuid128 itemTarget = WowGuid128.Empty;
            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem))
                itemTarget = packet.ReadPackedGuid().To128();
            dbdata.Target.Item = itemTarget;

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
            {
                dbdata.Target.SrcLocation = new TargetLocation();
                dbdata.Target.SrcLocation.Transport = WowGuid128.Empty;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                    dbdata.Target.SrcLocation.Transport = packet.ReadPackedGuid().To128();

                dbdata.Target.SrcLocation.Location = packet.ReadVector3();
            }

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
            {
                dbdata.Target.DstLocation = new TargetLocation();
                dbdata.Target.DstLocation.Transport = WowGuid128.Empty;
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                    dbdata.Target.DstLocation.Transport = packet.ReadPackedGuid().To128();

                dbdata.Target.DstLocation.Location = packet.ReadVector3();
            }
            
            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.String))
                dbdata.Target.Name = packet.ReadCString();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                if (flags.HasAnyFlag(CastFlag.PredictedPower))
                {
                        packet.ReadInt32(); // Rune Cooldown
                }

                if (flags.HasAnyFlag(CastFlag.RuneInfo))
                {
                    var spellRuneState = packet.ReadUInt8();
                    var playerRuneState = packet.ReadUInt8();

                    for (var i = 0; i < 6; i++)
                    {
                        var mask = 1 << i;
                        if ((mask & spellRuneState) == 0)
                            continue;

                        if ((mask & playerRuneState) != 0)
                            continue;

                        packet.ReadUInt8(); // Rune Cooldown Passed
                    }
                }

                if (isSpellGo)
                {
                    if (flags.HasAnyFlag(CastFlag.AdjustMissile))
                    {
                        dbdata.MissileTrajectory.Pitch = packet.ReadFloat(); // Elevation
                        dbdata.MissileTrajectory.TravelTime = packet.ReadUInt32(); // Delay time
                    }
                }
            }

            if (flags.HasAnyFlag(CastFlag.Projectile))
            {
                dbdata.AmmoDisplayId = packet.ReadInt32();
                dbdata.AmmoInventoryType = packet.ReadInt32();
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                if (isSpellGo)
                {
                    if (flags.HasAnyFlag(CastFlag.VisualChain))
                    {
                        packet.ReadInt32();
                        packet.ReadInt32();
                    }

                    if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
                        packet.ReadInt8(); // Some count

                    if (targetFlags.HasAnyFlag(SpellCastTargetFlags.ExtraTargets))
                    {
                        var targetCount = packet.ReadInt32();
                        if (targetCount > 0)
                        {
                            TargetLocation location = new();
                            for (var i = 0; i < targetCount; i++)
                            {
                                location.Location = packet.ReadVector3();
                                location.Transport = packet.ReadGuid().To128();
                            }
                            dbdata.TargetPoints.Add(location);
                        }
                    }
                }
                else
                {
                    if (flags.HasAnyFlag(CastFlag.Immunity))
                    {
                        dbdata.Immunities.School = packet.ReadUInt32();
                        dbdata.Immunities.Value = packet.ReadUInt32();
                    }

                    if (flags.HasAnyFlag(CastFlag.HealPrediction))
                    {
                        packet.ReadInt32(); // Predicted Spell ID

                        if (packet.ReadUInt8() == 2)
                            packet.ReadPackedGuid();
                    }
                }
            }

            return dbdata;
        }

        [PacketHandler(Opcode.SMSG_CANCEL_AUTO_REPEAT)]
        void HandleCancelAutoRepeat(WorldPacket packet)
        {
            CancelAutoRepeat cancel = new CancelAutoRepeat();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                cancel.Guid = packet.ReadPackedGuid().To128();
            else
                cancel.Guid = Global.CurrentSessionData.GameState.CurrentPlayerGuid;
            SendPacketToClient(cancel);
        }
    }
}
