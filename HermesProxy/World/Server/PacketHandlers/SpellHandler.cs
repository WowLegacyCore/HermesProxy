using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_CAST_SPELL)]
        void HandleCastSpell(CastSpell cast)
        {
            Global.CurrentSessionData.GameState.LastClientCastId = cast.Cast.SpellID;
            Global.CurrentSessionData.GameState.LastClientCastGuid = cast.Cast.CastID;
            SpellCastTargetFlags targetFlags = SpellCastTargetFlags.None;
            if (cast.Cast.Target.Unit != null && !cast.Cast.Target.Unit.IsEmpty())
            {
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.Unit))
                    targetFlags |= SpellCastTargetFlags.Unit;
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.CorpseEnemy))
                    targetFlags |= SpellCastTargetFlags.CorpseEnemy;
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.GameObject))
                    targetFlags |= SpellCastTargetFlags.GameObject;
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.CorpseAlly))
                    targetFlags |= SpellCastTargetFlags.CorpseAlly;
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.UnitMinipet))
                    targetFlags |= SpellCastTargetFlags.UnitMinipet;
            }
            if (cast.Cast.Target.Item != null & !cast.Cast.Target.Item.IsEmpty())
            {
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.Item))
                    targetFlags |= SpellCastTargetFlags.Item;
                if (cast.Cast.Target.Flags.HasFlag(SpellCastTargetFlags.TradeItem))
                    targetFlags |= SpellCastTargetFlags.TradeItem;
            }
            if (cast.Cast.Target.SrcLocation != null)
                targetFlags |= SpellCastTargetFlags.SourceLocation;
            if (cast.Cast.Target.DstLocation != null)
                targetFlags |= SpellCastTargetFlags.DestLocation;
            if (!String.IsNullOrEmpty(cast.Cast.Target.Name))
                targetFlags |= SpellCastTargetFlags.String;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_CAST_SPELL);
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt32(cast.Cast.SpellID);
                packet.WriteUInt16((ushort)targetFlags);
            }
            else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                packet.WriteUInt32(cast.Cast.SpellID);
                packet.WriteUInt8(0); // cast count
                packet.WriteUInt32((uint)targetFlags);
            }
            else
            {
                packet.WriteUInt8(0); // cast count
                packet.WriteUInt32(cast.Cast.SpellID);
                packet.WriteUInt8((byte)cast.Cast.SendCastFlags);
                packet.WriteUInt32((uint)targetFlags);

            }

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.GameObject |
                SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.UnitMinipet))
                packet.WritePackedGuid(cast.Cast.Target.Unit.To64());

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem))
                packet.WritePackedGuid(cast.Cast.Target.Item.To64());

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                    packet.WritePackedGuid(cast.Cast.Target.SrcLocation.Transport.To64());
                packet.WriteVector3(cast.Cast.Target.SrcLocation.Location);
            }

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
            {
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                    packet.WritePackedGuid(cast.Cast.Target.DstLocation.Transport.To64());
                packet.WriteVector3(cast.Cast.Target.DstLocation.Location);
            }

            if (targetFlags.HasAnyFlag(SpellCastTargetFlags.String))
                packet.WriteCString(cast.Cast.Target.Name);

            SendPacketToServer(packet);
        }
    }
}
