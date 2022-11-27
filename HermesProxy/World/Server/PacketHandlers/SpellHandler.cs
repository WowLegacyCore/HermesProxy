using Framework;
using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Threading;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        SpellCastTargetFlags ConvertSpellTargetFlags(SpellTargetData target)
        {
            SpellCastTargetFlags targetFlags = SpellCastTargetFlags.None;
            if (target.Unit != null && !target.Unit.IsEmpty())
            {
                if (target.Flags.HasFlag(SpellCastTargetFlags.Unit))
                    targetFlags |= SpellCastTargetFlags.Unit;
                if (target.Flags.HasFlag(SpellCastTargetFlags.CorpseEnemy))
                    targetFlags |= SpellCastTargetFlags.CorpseEnemy;
                if (target.Flags.HasFlag(SpellCastTargetFlags.GameObject))
                    targetFlags |= SpellCastTargetFlags.GameObject;
                if (target.Flags.HasFlag(SpellCastTargetFlags.CorpseAlly))
                    targetFlags |= SpellCastTargetFlags.CorpseAlly;
                if (target.Flags.HasFlag(SpellCastTargetFlags.UnitMinipet))
                    targetFlags |= SpellCastTargetFlags.UnitMinipet;
            }
            if (target.Item != null & !target.Item.IsEmpty())
            {
                if (target.Flags.HasFlag(SpellCastTargetFlags.Item))
                    targetFlags |= SpellCastTargetFlags.Item;
                if (target.Flags.HasFlag(SpellCastTargetFlags.TradeItem))
                    targetFlags |= SpellCastTargetFlags.TradeItem;
            }
            if (target.SrcLocation != null)
                targetFlags |= SpellCastTargetFlags.SourceLocation;
            if (target.DstLocation != null)
                targetFlags |= SpellCastTargetFlags.DestLocation;
            if (!String.IsNullOrEmpty(target.Name))
                targetFlags |= SpellCastTargetFlags.String;
            return targetFlags;
        }
        void WriteSpellTargets(SpellTargetData target, SpellCastTargetFlags targetFlags, WorldPacket packet)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteUInt16((ushort)targetFlags);
            else
                packet.WriteUInt32((uint)targetFlags);

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.GameObject |
                SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.UnitMinipet))
                packet.WritePackedGuid(target.Unit.To64());

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem))
                packet.WritePackedGuid(target.Item.To64());

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                    packet.WritePackedGuid(target.SrcLocation.Transport.To64());
                packet.WriteVector3(target.SrcLocation.Location);
            }

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                    packet.WritePackedGuid(target.DstLocation.Transport.To64());
                packet.WriteVector3(target.DstLocation.Location);
            }

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.String))
                packet.WriteCString(target.Name);
        }
        public void SendCastRequestFailed(ClientCastRequest castRequest, bool isPet)
        {
            if (!castRequest.HasStarted)
            {
                SpellPrepare prepare2 = new SpellPrepare();
                prepare2.ClientCastID = castRequest.ClientGUID;
                prepare2.ServerCastID = castRequest.ServerGUID;
                SendPacket(prepare2);
            }

            if (isPet)
            {
                PetCastFailed failed = new();
                failed.SpellID = castRequest.SpellId;
                failed.Reason = (uint)SpellCastResultClassic.SpellInProgress;
                failed.CastID = castRequest.ServerGUID;
                SendPacket(failed);
            }
            else
            {
                CastFailed failed = new();
                failed.SpellID = castRequest.SpellId;
                failed.SpellXSpellVisualID = castRequest.SpellXSpellVisualId;
                failed.Reason = (uint)SpellCastResultClassic.SpellInProgress;
                failed.CastID = castRequest.ServerGUID;
                SendPacket(failed);
            }    
        }
        [PacketHandler(Opcode.CMSG_CAST_SPELL)]
        void HandleCastSpell(CastSpell cast)
        {
            // Artificial lag is needed for spell packets,
            // or spells will bug out and glow if spammed.
            if (Settings.ServerSpellDelay > 0)
            {
                Thread.Sleep(Settings.ServerSpellDelay);
            }

            if (GameData.NextMeleeSpells.Contains(cast.Cast.SpellID) ||
                GameData.AutoRepeatSpells.Contains(cast.Cast.SpellID))
            {
                ClientCastRequest castRequest = new ClientCastRequest();
                castRequest.Timestamp = Environment.TickCount;
                castRequest.SpellId = cast.Cast.SpellID;
                castRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
                castRequest.ClientGUID = cast.Cast.CastID;
                
                if (GetSession().GameState.CurrentClientSpecialCast != null)
                {
                    castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());
                    SendCastRequestFailed(castRequest, false);
                    return;
                }
                else
                {
                    castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId, cast.Cast.SpellID, cast.Cast.SpellID + GetSession().GameState.CurrentPlayerGuid.GetCounter());

                    SpellPrepare prepare = new SpellPrepare();
                    prepare.ClientCastID = cast.Cast.CastID;
                    prepare.ServerCastID = castRequest.ServerGUID;
                    SendPacket(prepare);

                    GetSession().GameState.CurrentClientSpecialCast = castRequest;
                } 
            }
            else
            {
                ClientCastRequest castRequest = new ClientCastRequest();
                castRequest.Timestamp = Environment.TickCount;
                castRequest.SpellId = cast.Cast.SpellID;
                castRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
                castRequest.ClientGUID = cast.Cast.CastID;
                castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());

                if (GetSession().GameState.CurrentClientNormalCast != null)
                {
                    if (GetSession().GameState.CurrentClientNormalCast.HasStarted)
                    {
                        SendCastRequestFailed(castRequest, false);
                    }
                    else
                    {
                        if (GetSession().GameState.CurrentClientNormalCast.Timestamp + 10000 < castRequest.Timestamp)
                        {
                            SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, false);
                            GetSession().GameState.CurrentClientNormalCast = null;
                            foreach (var pending in GetSession().GameState.PendingClientCasts)
                                SendCastRequestFailed(pending, false);
                            GetSession().GameState.PendingClientCasts.Clear();
                            SendCastRequestFailed(castRequest, false);
                        }
                        else
                            GetSession().GameState.PendingClientCasts.Add(castRequest);
                    }
                    return;
                }

                GetSession().GameState.CurrentClientNormalCast = castRequest;
            }

            SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Cast.Target);

            WorldPacket packet = new WorldPacket(Opcode.CMSG_CAST_SPELL);
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt32(cast.Cast.SpellID);
            }
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                packet.WriteUInt32(cast.Cast.SpellID);
                packet.WriteUInt8(0); // cast count
            }
            else
            {
                packet.WriteUInt8(0); // cast count
                packet.WriteUInt32(cast.Cast.SpellID);
                packet.WriteUInt8((byte)cast.Cast.SendCastFlags);
            }
            WriteSpellTargets(cast.Cast.Target, targetFlags, packet);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_PET_CAST_SPELL)]
        void HandlePetCastSpell(PetCastSpell cast)
        {
            // Artificial lag is needed for spell packets,
            // or spells will bug out and glow if spammed.
            if (Settings.ServerSpellDelay > 0)
            {
                Thread.Sleep(Settings.ServerSpellDelay);
            }

            ClientCastRequest castRequest = new ClientCastRequest();
            castRequest.Timestamp = Environment.TickCount;
            castRequest.SpellId = cast.Cast.SpellID;
            castRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
            castRequest.ClientGUID = cast.Cast.CastID;
            castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());

            if (GetSession().GameState.CurrentClientPetCast != null)
            {
                if (GetSession().GameState.CurrentClientPetCast.HasStarted)
                {
                    SendCastRequestFailed(castRequest, true);
                }
                else
                {
                    if (GetSession().GameState.CurrentClientPetCast.Timestamp + 10000 < castRequest.Timestamp)
                    {
                        SendCastRequestFailed(GetSession().GameState.CurrentClientPetCast, true);
                        GetSession().GameState.CurrentClientPetCast = null;
                        foreach (var pending in GetSession().GameState.PendingClientPetCasts)
                            SendCastRequestFailed(pending, true);
                        GetSession().GameState.PendingClientPetCasts.Clear();
                        SendCastRequestFailed(castRequest, true);
                    }
                    else
                        GetSession().GameState.PendingClientPetCasts.Add(castRequest);
                }
                return;
            }

            GetSession().GameState.CurrentClientPetCast = castRequest;

            SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Cast.Target);

            WorldPacket packet = new WorldPacket(Opcode.CMSG_PET_CAST_SPELL);
            packet.WriteGuid(cast.PetGUID.To64());
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt8(0); // cast count
            packet.WriteUInt32(cast.Cast.SpellID);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt8((byte)cast.Cast.SendCastFlags);
            WriteSpellTargets(cast.Cast.Target, targetFlags, packet);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_USE_ITEM)]
        void HandleUseItem(UseItem use)
        {
            // Artificial lag is needed for spell packets,
            // or spells will bug out and glow if spammed.
            if (Settings.ServerSpellDelay > 0)
            {
                Thread.Sleep(Settings.ServerSpellDelay);
            }

            ClientCastRequest castRequest = new ClientCastRequest();
            castRequest.Timestamp = Environment.TickCount;
            castRequest.SpellId = use.Cast.SpellID;
            castRequest.SpellXSpellVisualId = use.Cast.SpellXSpellVisualID;
            castRequest.ClientGUID = use.Cast.CastID;
            castRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, (uint)GetSession().GameState.CurrentMapId, use.Cast.SpellID, 10000 + use.Cast.CastID.GetCounter());
            castRequest.ItemGUID = use.CastItem;

            if (GetSession().GameState.CurrentClientNormalCast != null)
            {
                if (GetSession().GameState.CurrentClientNormalCast.HasStarted)
                {
                    SendCastRequestFailed(castRequest, false);
                }
                else
                {
                    if (GetSession().GameState.CurrentClientNormalCast.Timestamp + 10000 < castRequest.Timestamp)
                    {
                        SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, false);
                        GetSession().GameState.CurrentClientNormalCast = null;
                        foreach (var pending in GetSession().GameState.PendingClientCasts)
                            SendCastRequestFailed(pending, false);
                        GetSession().GameState.PendingClientCasts.Clear();
                        SendCastRequestFailed(castRequest, false);
                    }
                    else
                        GetSession().GameState.PendingClientCasts.Add(castRequest);
                }
                return;
            }

            GetSession().GameState.CurrentClientNormalCast = castRequest;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_USE_ITEM);
            byte containerSlot = use.PackSlot != Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(use.PackSlot) : use.PackSlot;
            byte slot = use.PackSlot == Enums.Classic.InventorySlots.Bag0 ? ModernVersion.AdjustInventorySlot(use.Slot) : use.Slot;
            packet.WriteUInt8(containerSlot);
            packet.WriteUInt8(slot);
            packet.WriteUInt8(GetSession().GameState.GetItemSpellSlot(use.CastItem, use.Cast.SpellID));
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt8(0); // cast count;
                packet.WriteGuid(use.CastItem.To64());
            }
            SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(use.Cast.Target);
            WriteSpellTargets(use.Cast.Target, targetFlags, packet);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_CANCEL_CAST)]
        void HandleCancelCast(CancelCast cast)
        {
            // Artificial lag is needed for spell packets,
            // or spells will bug out and glow if spammed.
            if (Settings.ServerSpellDelay > 0)
            {
                Thread.Sleep(Settings.ServerSpellDelay);
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_CAST);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.WriteUInt8(0);
            packet.WriteUInt32(cast.SpellID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_CANCEL_CHANNELLING)]
        void HandleCancelChannelling(CancelChannelling cast)
        {
            // Artificial lag is needed for spell packets,
            // or spells will bug out and glow if spammed.
            if (Settings.ServerSpellDelay > 0)
            {
                Thread.Sleep(Settings.ServerSpellDelay);
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_CHANNELLING);
            packet.WriteInt32(cast.SpellID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_CANCEL_AURA)]
        void HandleCancelAura(CancelAura aura)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
            packet.WriteUInt32(aura.SpellID);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_CANCEL_MOUNT_AURA)]
        void HandleCancelMountAura(EmptyClientPacket cancel)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_MOUNT_AURA);
                SendPacketToServer(packet);
            }
            else
            {
                WowGuid128 guid = GetSession().GameState.CurrentPlayerGuid;
                var updateFields = GetSession().GameState.GetCachedObjectFieldsLegacy(guid);
                if (updateFields == null)
                    return;

                for (byte i = 0; i < 32; i++)
                {
                    var aura = GetSession().WorldClient.ReadAuraSlot(i, guid, updateFields);
                    if (aura == null)
                        continue;

                    if (GameData.MountAuras.Contains(aura.SpellID))
                    {
                        WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
                        packet.WriteUInt32(aura.SpellID);
                        SendPacketToServer(packet);
                    }
                }
            }
        }
        [PacketHandler(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL)]
        void HandleCancelAutoRepeatSpell(CancelAutoRepeatSpell aura)
        {
            // Artificial lag is needed for spell packets,
            // or spells will bug out and glow if spammed.
            if (Settings.ServerSpellDelay > 0)
            {
                Thread.Sleep(Settings.ServerSpellDelay);
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_LEARN_TALENT)]
        void HandleLearnTalent(LearnTalent talent)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_LEARN_TALENT);
            packet.WriteUInt32(talent.TalentID);
            packet.WriteUInt32(talent.Rank);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_RESURRECT_RESPONSE)]
        void HandleResurrectResponse(ResurrectResponse revive)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_RESURRECT_RESPONSE);
            packet.WriteGuid(revive.CasterGUID.To64());
            packet.WriteUInt8((byte)(revive.Response != 0 ? 0 : 1));
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_SELF_RES)]
        void HandleSelfRes(SelfRes revive)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SELF_RES);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_TOTEM_DESTROYED)]
        void HandleTotemDestroyed(TotemDestroyed totem)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_TOTEM_DESTROYED);
            packet.WriteUInt8(totem.Slot);
            SendPacketToServer(packet);
        }
    }
}
