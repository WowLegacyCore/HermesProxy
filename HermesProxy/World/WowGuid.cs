using Framework;
using Framework.Constants.World;
using HermesProxy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy
{
    public abstract class WowGuid
    {
        public ulong Low { get; protected set; }
        public HighGuid HighGuid { get; protected set; }
        public ulong High { get; protected set; }

        public bool HasEntry()
        {
            switch (GetHighType())
            {
                case HighGuidType.Creature:
                case HighGuidType.GameObject:
                case HighGuidType.Pet:
                case HighGuidType.Vehicle:
                case HighGuidType.AreaTrigger:
                    return true;
                default:
                    return false;
            }
        }

        public abstract ulong GetLow();
        public ulong GetLowValue() => Low;
        public abstract uint GetEntry();

        public HighGuidType GetHighType()
        {
            return HighGuid.GetHighGuidType();
        }
        public ulong GetHighValue() => High;

        public ObjectType GetObjectType()
        {

            switch (GetHighType())
            {
                case HighGuidType.Player:
                    return ObjectType.Player;
                case HighGuidType.DynamicObject:
                    return ObjectType.DynamicObject;
                case HighGuidType.Item:
                    return ObjectType.Item;
                case HighGuidType.GameObject:
                case HighGuidType.Transport:
                    //case HighGuidType.MOTransport: ??
                    return ObjectType.GameObject;
                case HighGuidType.Vehicle:
                case HighGuidType.Creature:
                case HighGuidType.Pet:
                    return ObjectType.Unit;
                case HighGuidType.AreaTrigger:
                    return ObjectType.AreaTrigger;
                default:
                    return ObjectType.Object;
            }
        }

        public static bool operator ==(WowGuid first, WowGuid other)
        {
            if (ReferenceEquals(first, other))
                return true;

            if (((object)first == null) || ((object)other == null))
                return false;

            return first.Equals(other);
        }

        public static bool operator !=(WowGuid first, WowGuid other)
        {
            return !(first == other);
        }

        public override bool Equals(object obj)
        {
            return obj is WowGuid && Equals((WowGuid)obj);
        }

        public bool Equals(WowGuid other)
        {
            return other.Low == Low && other.High == High;
        }

        public override int GetHashCode()
        {
            return new { Low, High }.GetHashCode();
        }

        public bool IsEmpty()
        {
            return High == 0 && Low == 0;
        }

        public abstract WowGuid64 To64();
        public abstract WowGuid128 To128();
    }

    public class WowGuid128 : WowGuid
    {
        public WowGuid128()
        {
            Low = 0;
            High = 0;
            if (Settings.ClientBuild >= ClientVersionBuild.V7_0_3_22248)
                HighGuid = new HighGuid703((byte)((High >> 58) & 0x3F));
            else if (Settings.ClientBuild >= ClientVersionBuild.V6_2_4_21315)
                HighGuid = new HighGuid624((byte)((High >> 58) & 0x3F));
            else
                HighGuid = new HighGuid623((byte)((High >> 58) & 0x3F));
        }
        public WowGuid128(ulong low, ulong high)
        {
            Low = low;
            High = high;
            if (Settings.ClientBuild >= ClientVersionBuild.V7_0_3_22248)
                HighGuid = new HighGuid703((byte)((High >> 58) & 0x3F));
            else if (Settings.ClientBuild >= ClientVersionBuild.V6_2_4_21315)
                HighGuid = new HighGuid624((byte)((High >> 58) & 0x3F));
            else
                HighGuid = new HighGuid623((byte)((High >> 58) & 0x3F));
        }

        public WowGuid128(HighGuidType703 type, ulong counter)
        {
            Low = counter;
            High = (ulong)type << 58;
            HighGuid = new HighGuid703((byte)((High >> 58) & 0x3F));
        }

        public byte GetSubType() // move to base?
        {
            return (byte)(High & 0x3F);
        }

        public ushort GetRealmId() // move to base?
        {
            return (ushort)((High >> 42) & 0x1FFF);
        }

        public uint GetServerId() // move to base?
        {
            return (uint)((Low >> 40) & 0xFFFFFF);
        }

        public ushort GetMapId() // move to base?
        {
            return (ushort)((High >> 29) & 0x1FFF);
        }

        public override uint GetEntry()
        {
            return (uint)((High >> 6) & 0x7FFFFF); // Id
        }

        public override ulong GetLow()
        {
            return Low & 0xFFFFFFFFFF; // CreationBits
        }

        public override string ToString()
        {
            if (Low == 0 && High == 0)
                return "Full: 0x0";

            if (HasEntry())
            {
                // ReSharper disable once UseStringInterpolation
                return $"Full: 0x{High:X16}{Low:X16} {GetHighType()}/{GetSubType()} R{GetRealmId()}/S{GetServerId()} Map: {GetMapId()} Entry: {GetEntry()} Low: {GetLow()}";
            }

            // TODO: Implement extra format for battleground, see WowGuid64.ToString()

            // ReSharper disable once UseStringInterpolation
            return $"Full: 0x{High:X16}{Low:X16} {GetHighType()}/{GetSubType()} R{GetRealmId()}/S{GetServerId()} Map: {GetMapId()} Low: {GetLow()}";
        }

        public override WowGuid64 To64()
        {
            return new WowGuid64(0);
        }
        public override WowGuid128 To128()
        {
            return this;
        }
    }

    public class WowGuid64 : WowGuid
    {
        public static WowGuid Empty = new WowGuid64(0);

        public WowGuid64(ulong id)
        {
            Low = id;
            HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
        }

        public override ulong GetLow()
        {
            switch (GetHighType())
            {
                case HighGuidType.Player:
                case HighGuidType.DynamicObject:
                case HighGuidType.RaidGroup:
                case HighGuidType.Item:
                    return Low & 0x000FFFFFFFFFFFFF;
                case HighGuidType.GameObject:
                case HighGuidType.Transport:
                //case HighGuidType.MOTransport: ??
                case HighGuidType.Vehicle:
                case HighGuidType.Creature:
                case HighGuidType.Pet:
                    if (Settings.ServerBuild >= ClientVersionBuild.V4_0_1_13164)
                        return Low & 0x00000000FFFFFFFFul;
                    return Low & 0x0000000000FFFFFFul;
            }

            // TODO: check if entryless guids dont use now more bytes
            return Low & 0x00000000FFFFFFFFul;
        }

        public override uint GetEntry()
        {
            if (!HasEntry())
                return 0;

            if (Settings.ServerBuild >= ClientVersionBuild.V4_0_1_13164)
                return (uint)((Low & 0x000FFFFF00000000) >> 32);
            return (uint)((Low & 0x000FFFFFFF000000) >> 24);
        }

        public HighGuidTypeLegacy GetHighGuidTypeLegacy()
        {
            if (Low == 0)
                return HighGuidTypeLegacy.None;

            var highGUID = (HighGuidTypeLegacy)((Low & 0xF0F0000000000000) >> 52);
            switch ((int)highGUID & 0xF00)
            {
                case 0x0:
                    return HighGuidTypeLegacy.Player;
                case 0x400:
                    return HighGuidTypeLegacy.Item;
                default:
                    return highGUID;
            }
        }

        public override string ToString()
        {
            if (Low == 0)
                return "0x0";

            // If our guid has an entry and it is an unit or a GO, print its
            // name next to the entry (from a database, if enabled)
            if (HasEntry())
            {
                return "Full: 0x" + Low.ToString("X8") + " Type: " + GetHighType()
                    + " Entry: " + GetEntry() + " Low: " + GetLow();
            }

            switch (GetHighGuidTypeLegacy())
            {
                case HighGuidTypeLegacy.BattleGround1:
                    {
                        var bgType = Low & 0x00000000000000FF;
                        return "Full: 0x" + Low.ToString("X8") + " Type: " + GetHighType()
                            + " BgType: " + bgType;
                    }
                case HighGuidTypeLegacy.BattleGround2:
                    {
                        var bgType = (Low & 0x00FF0000) >> 16;
                        var unkId = (Low & 0x0000FF00) >> 8;
                        var arenaType = (Low & 0x000000FF) >> 0;
                        return "Full: 0x" + Low.ToString("X8") + " Type: " + GetHighType()
                            + " BgType: " + bgType
                            + " Unk: " + unkId + (arenaType > 0 ? (" ArenaType: " + arenaType) : String.Empty);
                    }
            }

            return "Full: 0x" + Low.ToString("X8") + " Type: " + GetHighType()
                + " Low: " + GetLow();
        }

        public override WowGuid64 To64()
        {
            return this;
        }
        public override WowGuid128 To128()
        {
            return new WowGuid128();
        }
    }
}
