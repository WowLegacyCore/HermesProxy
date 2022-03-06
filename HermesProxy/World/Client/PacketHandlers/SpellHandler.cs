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
            if (Global.CurrentSessionData.GameState.LastClientCastGuid == null)
                return;

            int spellId = packet.ReadInt32();
            var status = packet.ReadUInt8();
            if (status != 2)
            {
                SpellPrepare prepare = new();
                prepare.ClientCastID = Global.CurrentSessionData.GameState.LastClientCastGuid;
                prepare.ServerCastID = WowGuid128.Empty;
                SendPacketToClient(prepare);
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

        [PacketHandler(Opcode.SMSG_SPELL_COOLDOWN)]
        void HandleSpellCooldown(WorldPacket packet)
        {
            SpellCooldownPkt cooldown = new();
            cooldown.Caster = packet.ReadGuid().To128();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                cooldown.Flags = packet.ReadUInt8();
            while (packet.CanRead())
            {
                SpellCooldownStruct cd = new();
                cd.SpellID = packet.ReadUInt32();
                cd.ForcedCooldown = packet.ReadUInt32();
            }
            SendPacketToClient(cooldown);
        }

        [PacketHandler(Opcode.SMSG_COOLDOWN_EVENT)]
        void HandleCooldownEvent(WorldPacket packet)
        {
            CooldownEvent cooldown = new();
            cooldown.SpellID = packet.ReadUInt32();
            WowGuid guid = packet.ReadGuid();
            cooldown.IsPet = guid.GetHighType() == HighGuidType.Pet;
            SendPacketToClient(cooldown);
        }

        [PacketHandler(Opcode.SMSG_CLEAR_COOLDOWN)]
        void HandleClearCooldown(WorldPacket packet)
        {
            ClearCooldown cooldown = new();
            cooldown.SpellID = packet.ReadUInt32();
            WowGuid guid = packet.ReadGuid();
            cooldown.IsPet = guid.GetHighType() == HighGuidType.Pet;
            SendPacketToClient(cooldown);
        }

        [PacketHandler(Opcode.SMSG_COOLDOWN_CHEAT)]
        void HandleCooldownCheat(WorldPacket packet)
        {
            CooldownCheat cooldown = new();
            cooldown.Guid = packet.ReadGuid().To128();
            SendPacketToClient(cooldown);
        }

        [PacketHandler(Opcode.SMSG_SPELL_NON_MELEE_DAMAGE_LOG)]
        void HandleSpellNonMeleeDamageLog(WorldPacket packet)
        {
            SpellNonMeleeDamageLog spell = new();
            spell.TargetGUID = packet.ReadPackedGuid().To128();
            spell.CasterGUID = packet.ReadPackedGuid().To128();
            spell.SpellID = packet.ReadUInt32();
            spell.SpellXSpellVisualID = GameData.GetSpellVisual(spell.SpellID);

            if (Global.CurrentSessionData.GameState.LastClientCastId == spell.SpellID &&
                Global.CurrentSessionData.GameState.LastClientCastGuid != null)
                spell.CastID = Global.CurrentSessionData.GameState.LastClientCastGuid;
            else
                spell.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)Global.CurrentSessionData.GameState.CurrentMapId, spell.SpellID, spell.SpellID + spell.CasterGUID.GetLow());

            spell.Damage = packet.ReadInt32();
            spell.OriginalDamage = spell.Damage;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
                spell.Overkill = packet.ReadInt32();
            else
                spell.Overkill = -1;

            byte school = packet.ReadUInt8();
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                school = (byte)(1u << school);

            spell.SchoolMask = school;
            spell.Absorbed = packet.ReadInt32();
            spell.Resisted = packet.ReadInt32();
            spell.Periodic = packet.ReadBool();
            packet.ReadUInt8(); // unused
            spell.ShieldBlock = packet.ReadInt32();
            spell.Flags = (SpellHitType)packet.ReadUInt32();

            bool debugOutput = packet.ReadBool();
            if (debugOutput)
            {
                if (!spell.Flags.HasAnyFlag(SpellHitType.Split))
                {
                    if (spell.Flags.HasAnyFlag(SpellHitType.CritDebug))
                    {
                        packet.ReadFloat(); // roll
                        packet.ReadFloat(); // needed
                    }

                    if (spell.Flags.HasAnyFlag(SpellHitType.HitDebug))
                    {
                        packet.ReadFloat(); // roll
                        packet.ReadFloat(); // needed
                    }

                    if (spell.Flags.HasAnyFlag(SpellHitType.AttackTableDebug))
                    {
                        packet.ReadFloat(); // miss chance
                        packet.ReadFloat(); // dodge chance
                        packet.ReadFloat(); // parry chance
                        packet.ReadFloat(); // block chance
                        packet.ReadFloat(); // glance chance
                        packet.ReadFloat(); // crush chance
                    }
                }
            }

            SendPacketToClient(spell);
        }

        [PacketHandler(Opcode.SMSG_SPELL_HEAL_LOG)]
        void HandleSpellHealLog(WorldPacket packet)
        {
            SpellHealLog spell = new();
            spell.TargetGUID = packet.ReadPackedGuid().To128();
            spell.CasterGUID = packet.ReadPackedGuid().To128();
            spell.SpellID = packet.ReadUInt32();
            spell.HealAmount = packet.ReadInt32();
            spell.OriginalHealAmount = spell.HealAmount;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
                spell.OverHeal = packet.ReadUInt32();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                spell.Absorbed = packet.ReadUInt32();

            spell.Crit = packet.ReadBool();

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                bool debugOutput = packet.ReadBool();
                if (debugOutput)
                {
                    spell.CritRollMade = packet.ReadFloat();
                    spell.CritRollNeeded = packet.ReadFloat();
                }
            }

            SendPacketToClient(spell);
        }

        [PacketHandler(Opcode.SMSG_SPELL_PERIODIC_AURA_LOG)]
        void HandleSpellPeriodicAuraLog(WorldPacket packet)
        {
            SpellPeriodicAuraLog spell = new();
            spell.TargetGUID = packet.ReadPackedGuid().To128();
            spell.CasterGUID = packet.ReadPackedGuid().To128();
            spell.SpellID = packet.ReadUInt32();

            var count = packet.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var aura = (AuraType)packet.ReadUInt32();
                switch (aura)
                {
                    case AuraType.PeriodicDamage:
                    case AuraType.PeriodicDamagePercent:
                    {
                        SpellPeriodicAuraLog.SpellLogEffect effect = new();
                        effect.Effect = (uint)aura;
                        effect.Amount = packet.ReadInt32();
                        effect.OriginalDamage = effect.Amount;

                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                            effect.OverHealOrKill = packet.ReadUInt32();

                        uint school = packet.ReadUInt32();
                        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                            school = (1u << (byte)school);

                        effect.SchoolMaskOrPower = school;
                        effect.AbsorbedOrAmplitude = packet.ReadUInt32();
                        effect.Resisted = packet.ReadUInt32();

                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
                            effect.Crit = packet.ReadBool();

                        spell.Effects.Add(effect);
                        break;
                    }
                    case AuraType.PeriodicHeal:
                    case AuraType.ObsModHealth:
                    {
                        SpellPeriodicAuraLog.SpellLogEffect effect = new();
                        effect.Effect = (uint)aura;
                        effect.Amount = packet.ReadInt32();
                        effect.OriginalDamage = effect.Amount;

                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                            effect.OverHealOrKill = packet.ReadUInt32();

                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                            // no idea when this was added exactly
                            effect.AbsorbedOrAmplitude = packet.ReadUInt32();

                        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
                            effect.Crit = packet.ReadBool();

                        spell.Effects.Add(effect);
                        break;
                    }
                    case AuraType.ObsModPower:
                    case AuraType.PeriodicEnergize:
                    {
                        packet.ReadInt32(); // Power type
                        packet.ReadUInt32(); // Amount
                        break;
                    }
                    case AuraType.PeriodicManaLeech:
                    {
                        packet.ReadInt32(); // Power type
                        packet.ReadUInt32(); // Amount
                        packet.ReadFloat(); // Gain multiplier
                        break;
                    }
                }
            }
            SendPacketToClient(spell);
        }

        [PacketHandler(Opcode.SMSG_SPELL_DELAYED)]
        void HandleSpellDelayed(WorldPacket packet)
        {
            SpellDelayed delay = new();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                delay.CasterGUID = packet.ReadPackedGuid().To128();
            else
                delay.CasterGUID = packet.ReadGuid().To128();
            delay.Delay = packet.ReadInt32();
            SendPacketToClient(delay);
        }

        [PacketHandler(Opcode.MSG_CHANNEL_START)]
        void HandleSpellChannelStart(WorldPacket packet)
        {
            SpellChannelStart channel = new();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                channel.CasterGUID = packet.ReadPackedGuid().To128();
            else
                channel.CasterGUID = Global.CurrentSessionData.GameState.CurrentPlayerGuid;
            channel.SpellID = packet.ReadUInt32();
            channel.SpellXSpellVisualID = GameData.GetSpellVisual(channel.SpellID);
            channel.Duration = packet.ReadUInt32();
            SendPacketToClient(channel);
        }

        [PacketHandler(Opcode.MSG_CHANNEL_UPDATE)]
        void HandleSpellChannelUpdate(WorldPacket packet)
        {
            SpellChannelUpdate channel = new();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                channel.CasterGUID = packet.ReadPackedGuid().To128();
            else
                channel.CasterGUID = Global.CurrentSessionData.GameState.CurrentPlayerGuid;
            channel.TimeRemaining = packet.ReadInt32();
            SendPacketToClient(channel);
        }
    }
}
