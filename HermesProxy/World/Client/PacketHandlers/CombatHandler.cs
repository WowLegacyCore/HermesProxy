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
        [PacketHandler(Opcode.SMSG_ATTACK_START)]
        void HandleAttackStart(WorldPacket packet)
        {
            SAttackStart attack = new();
            attack.Attacker = packet.ReadGuid().To128();
            attack.Victim = packet.ReadGuid().To128();
            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_ATTACK_STOP)]
        void HandleAttackStop(WorldPacket packet)
        {
            SAttackStop attack = new();
            attack.Attacker = packet.ReadPackedGuid().To128();
            attack.Victim = packet.ReadPackedGuid().To128();
            attack.NowDead = packet.ReadUInt32() != 0;
            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_ATTACKER_STATE_UPDATE)]
        void HandleAttackerStateUpdate(WorldPacket packet)
        {
            AttackerStateUpdate attack = new();
            uint hitInfo = packet.ReadUInt32();
            attack.HitInfo = LegacyVersion.ConvertHitInfoFlags(hitInfo);
            attack.AttackerGUID = packet.ReadPackedGuid().To128();
            attack.VictimGUID = packet.ReadPackedGuid().To128();
            attack.Damage = packet.ReadInt32();
            attack.OriginalDamage = attack.Damage;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
                attack.OverDamage = packet.ReadInt32();
            else
                attack.OverDamage = -1;

            byte subDamageCount = packet.ReadUInt8();
            for (int i = 0; i < subDamageCount; i++)
            {
                SubDamage subDmg = new();

                uint school = packet.ReadUInt32();
                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                    school = (1u << (byte)school);

                subDmg.SchoolMask = school;
                subDmg.FloatDamage = packet.ReadFloat();
                subDmg.IntDamage = packet.ReadInt32();

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) ||
                    hitInfo.HasAnyFlag(HitInfo.PartialAbsorb | HitInfo.FullAbsorb))
                    subDmg.Absorbed = packet.ReadInt32();

                if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) ||
                    hitInfo.HasAnyFlag(HitInfo.PartialResist | HitInfo.FullResist))
                    subDmg.Resisted = packet.ReadInt32();

                attack.SubDmg.Add(subDmg);
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
                attack.VictimState = packet.ReadUInt8();
            else
                attack.VictimState = (byte)packet.ReadUInt32();

            attack.AttackerState = packet.ReadInt32();
            attack.MeleeSpellID = packet.ReadUInt32();

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) ||
                hitInfo.HasAnyFlag(HitInfo.Block))
                attack.BlockAmount = packet.ReadInt32();

            if (hitInfo.HasAnyFlag(HitInfo.RageGain))
                attack.RageGained = packet.ReadInt32();

            if (hitInfo.HasAnyFlag(HitInfo.Unk0))
            {
                attack.UnkState = new();
                attack.UnkState.State1 = packet.ReadUInt32();
                attack.UnkState.State2 = packet.ReadFloat();
                attack.UnkState.State3 = packet.ReadFloat();
                attack.UnkState.State4 = packet.ReadFloat();
                attack.UnkState.State5 = packet.ReadFloat();
                attack.UnkState.State6 = packet.ReadFloat();
                attack.UnkState.State7 = packet.ReadFloat();
                attack.UnkState.State8 = packet.ReadFloat();
                attack.UnkState.State9 = packet.ReadFloat();
                attack.UnkState.State10 = packet.ReadFloat();
                attack.UnkState.State11 = packet.ReadFloat();
                attack.UnkState.State12 = packet.ReadUInt32();
                packet.ReadUInt32();
                packet.ReadUInt32();
            }

            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_ATTACKSWING_NOTINRANGE)]
        void HandleAttackSwingNotInRange(WorldPacket packet)
        {
            AttackSwingError attack = new();
            attack.Reason = AttackSwingErr.NotInRange;
            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_ATTACKSWING_BADFACING)]
        void HandleAttackSwingBadFacing(WorldPacket packet)
        {
            AttackSwingError attack = new();
            attack.Reason = AttackSwingErr.BadFacing;
            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_ATTACKSWING_DEADTARGET)]
        void HandleAttackSwingDeadTarget(WorldPacket packet)
        {
            AttackSwingError attack = new();
            attack.Reason = AttackSwingErr.DeadTarget;
            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_ATTACKSWING_CANT_ATTACK)]
        void HandleAttackSwingCantAttack(WorldPacket packet)
        {
            AttackSwingError attack = new();
            attack.Reason = AttackSwingErr.CantAttack;
            SendPacketToClient(attack);
        }
        [PacketHandler(Opcode.SMSG_CANCEL_COMBAT)]
        void HandleCancelCombat(WorldPacket packet)
        {
            CancelCombat combat = new();
            SendPacketToClient(combat);
        }
        [PacketHandler(Opcode.SMSG_AI_REACTION)]
        void HandleAIReaction(WorldPacket packet)
        {
            AIReaction reaction = new();
            reaction.UnitGUID = packet.ReadGuid().To128();
            reaction.Reaction = packet.ReadUInt32();
            SendPacketToClient(reaction);
        }
        [PacketHandler(Opcode.SMSG_PARTY_KILL_LOG)]
        void HandlePartyKillLog(WorldPacket packet)
        {
            PartyKillLog log = new();
            log.Player = packet.ReadGuid().To128();
            log.Victim = packet.ReadGuid().To128();
            SendPacketToClient(log);
        }
    }
}
