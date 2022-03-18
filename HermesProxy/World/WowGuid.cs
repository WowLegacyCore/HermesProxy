using Framework;
using HermesProxy.World.Enums;
using Framework.Logging;

namespace HermesProxy.World
{
    public abstract class WowGuid
    {
        public ulong Low { get; protected set; }
        public HighGuid HighGuid { get; protected set; }
        public ulong High { get; protected set; }

        public abstract bool HasEntry();

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
                case HighGuidType.Corpse:
                    return ObjectType.Corpse;
                case HighGuidType.Item:
                    return ObjectType.Item;
                case HighGuidType.GameObject:
                case HighGuidType.Transport:
                case HighGuidType.MOTransport:
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

        public bool IsTransport()
        {
            switch (GetHighType())
            {
                case HighGuidType.Transport:
                case HighGuidType.MOTransport:
                    return true;
            }
            return false;
        }

        public bool IsPlayer()
        {
            switch (GetObjectType())
            {
                case ObjectType.Player:
                case ObjectType.ActivePlayer:
                    return true;
            }
            return false;
        }

        public bool IsCreature()
        {
            return GetObjectType() == ObjectType.Unit;
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
        public static WowGuid128 Empty = new WowGuid128();

        public WowGuid128()
        {
            Low = 0;
            High = 0;
            HighGuid = new HighGuid703((byte)((High >> 58) & 0x3F));
        }
        public WowGuid128(ulong high, ulong low)
        {
            Low = low;
            High = high;
            HighGuid = new HighGuid703((byte)((High >> 58) & 0x3F));
        }

        public static WowGuid128 Create(WowGuid64 guid)
        {
            switch (guid.GetHighType())
            {
                case HighGuidType.Player:
                    return Create(HighGuidType703.Player, guid.GetLow());
                case HighGuidType.Item:
                    return Create(HighGuidType703.Item, guid.GetLow());
                case HighGuidType.Transport:
                case HighGuidType.MOTransport:
                    return Create(HighGuidType703.Transport, guid.GetLow());
                case HighGuidType.RaidGroup:
                    return Create(HighGuidType703.RaidGroup, guid.GetLow());
                case HighGuidType.GameObject:
                    return Create(HighGuidType703.GameObject, 0, guid.GetEntry(), guid.GetLow());
                case HighGuidType.Creature:
                    return Create(HighGuidType703.Creature, 0, guid.GetEntry(), guid.GetLow());
                case HighGuidType.Pet:
                    return Create(HighGuidType703.Pet, 0, guid.GetEntry(), guid.GetLow());
                case HighGuidType.Vehicle:
                    return Create(HighGuidType703.Vehicle, 0, guid.GetEntry(), guid.GetLow());
                case HighGuidType.DynamicObject:
                    return Create(HighGuidType703.DynamicObject, 0, guid.GetEntry(), guid.GetLow());
                case HighGuidType.Corpse:
                    return Create(HighGuidType703.Corpse, 0, guid.GetEntry(), guid.GetLow());
            }
            return WowGuid128.Empty;
        }
        public static WowGuid128 Create(HighGuidType703 type, ulong counter)
        {
            switch (type)
            {
                case HighGuidType703.Uniq:
                case HighGuidType703.Party:
                case HighGuidType703.WowAccount:
                case HighGuidType703.BNetAccount:
                case HighGuidType703.GMTask:
                case HighGuidType703.RaidGroup:
                case HighGuidType703.Spell:
                case HighGuidType703.Mail:
                case HighGuidType703.UserRouter:
                case HighGuidType703.PVPQueueGroup:
                case HighGuidType703.UserClient:
                case HighGuidType703.UniqUserClient:
                case HighGuidType703.BattlePet:
                case HighGuidType703.CommerceObj:
                case HighGuidType703.ClientSession:
                    return GlobalCreate(type, counter);
                case HighGuidType703.Player:
                case HighGuidType703.Item:   // This is not exactly correct, there are 2 more unknown parts in highguid: (high >> 10 & 0xFF), (high >> 18 & 0xFFFFFF)
                case HighGuidType703.Guild:
                case HighGuidType703.Transport:
                    return RealmSpecificCreate(type, counter);
                default:
                    Log.Print(LogType.Error, $"This guid type cannot be constructed using Create(HighGuid: {type} ulong counter).");
                    break;
            }
            return WowGuid128.Empty;
        }
        public static WowGuid128 Create(HighGuidType703 type, uint mapId, uint entry, ulong counter)
        {
            return MapSpecificCreate(type, 0, (ushort)mapId, 0, entry, counter);
        }
        public static WowGuid128 Create(HighGuidType703 type, World.Objects.SpellCastSource subType, uint mapId, uint entry, ulong counter)
        {
            return MapSpecificCreate(type, (byte)subType, (ushort)mapId, 0, entry, counter);
        }

        static WowGuid128 GlobalCreate(HighGuidType703 type, ulong counter)
        {
            return new WowGuid128((ulong)type << 58, counter);
        }
        static WowGuid128 RealmSpecificCreate(HighGuidType703 type, ulong counter)
        {
            if (type == HighGuidType703.Transport)
                return new WowGuid128((ulong)type << 58 | (counter << 38), 0);
            else
                return new WowGuid128((ulong)type << 58 | (ulong)1 /*realmId*/ << 42, counter);
        }
        static WowGuid128 MapSpecificCreate(HighGuidType703 type, byte subType, ushort mapId, uint serverId, uint entry, ulong counter)
        {
            return new WowGuid128((((ulong)type << 58) | ((ulong)(1 /*realmId*/ & 0x1FFF) << 42) | ((ulong)(mapId & 0x1FFF) << 29) | ((ulong)(entry & 0x7FFFFF) << 6) | ((ulong)subType & 0x3F)),
                (((ulong)(serverId & 0xFFFFFF) << 40) | (counter & 0xFFFFFFFFFF)));
        }

        public override bool HasEntry()
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
            if (GetHighType() == HighGuidType.Transport)
                return (High >> 38) & 0xFFFFF;
            else
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
            return WowGuid64.Create(this);
        }
        public override WowGuid128 To128()
        {
            return this;
        }
    }

    public class WowGuid64 : WowGuid
    {
        public static WowGuid64 Empty = new WowGuid64(0);

        public WowGuid64(ulong id)
        {
            Low = id;
            HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
        }

        public WowGuid64(HighGuidTypeLegacy hi, uint counter)
        {
            Low = counter != 0 ? (ulong)counter | ((ulong)hi << 48) : 0;
            HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
        }

        public WowGuid64(HighGuidTypeLegacy hi, uint entry, uint counter)
        {
            Low = counter != 0 ? (ulong)counter | ((ulong)entry << 24) | ((ulong)hi << 48) : 0;
            HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
        }

        public static WowGuid64 Create(WowGuid128 guid)
        {
            switch (guid.GetHighType())
            {
                case HighGuidType.Player:
                    return new WowGuid64(HighGuidTypeLegacy.Player, (uint)guid.GetLow());
                case HighGuidType.Item:
                    return new WowGuid64(HighGuidTypeLegacy.Item, (uint)guid.GetLow());
                case HighGuidType.Transport:
                case HighGuidType.MOTransport:
                    return new WowGuid64(HighGuidTypeLegacy.Transport, (uint)guid.GetLow());
                case HighGuidType.RaidGroup:
                    return new WowGuid64(HighGuidTypeLegacy.Group, (uint)guid.GetLow());
                case HighGuidType.GameObject:
                    return new WowGuid64(HighGuidTypeLegacy.GameObject, guid.GetEntry(), (uint)guid.GetLow());
                case HighGuidType.Creature:
                    return new WowGuid64(HighGuidTypeLegacy.Creature, guid.GetEntry(), (uint)guid.GetLow());
                case HighGuidType.Pet:
                    return new WowGuid64(HighGuidTypeLegacy.Pet, guid.GetEntry(), (uint)guid.GetLow());
                case HighGuidType.Vehicle:
                    return new WowGuid64(HighGuidTypeLegacy.Vehicle, guid.GetEntry(), (uint)guid.GetLow());
                case HighGuidType.DynamicObject:
                    return new WowGuid64(HighGuidTypeLegacy.DynamicObject, guid.GetEntry(), (uint)guid.GetLow());
                case HighGuidType.Corpse:
                    return new WowGuid64(HighGuidTypeLegacy.Corpse, guid.GetEntry(), (uint)guid.GetLow());
            }
            return WowGuid64.Empty;
        }

        public override bool HasEntry()
        {
            switch (GetHighType())
            {
                case HighGuidType.Item:
                case HighGuidType.Player:
                case HighGuidType.DynamicObject:
                case HighGuidType.Corpse:
                case HighGuidType.MOTransport:
                case HighGuidType.RaidGroup:
                    return false;
                case HighGuidType.GameObject:
                case HighGuidType.Transport:
                case HighGuidType.Creature:
                case HighGuidType.Pet:
                default:
                    return true;
            }
        }

        public override ulong GetLow()
        {
            return HasEntry()
                ? (uint)(Low & 0x0000000000FFFFFFul)
                : (uint)(Low & 0x00000000FFFFFFFFul);
        }

        public override uint GetEntry()
        {
            if (!HasEntry())
                return 0;

            return (uint)((Low >> 24) & 0x0000000000FFFFFFul);
        }

        public HighGuidTypeLegacy GetHighGuidTypeLegacy()
        {
            if (Low == 0)
                return HighGuidTypeLegacy.None;

            return (HighGuidTypeLegacy)((Low >> 48) & 0x0000FFFF);
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

            return "Full: 0x" + Low.ToString("X8") + " Type: " + GetHighType()
                + " Low: " + GetLow();
        }

        public override WowGuid64 To64()
        {
            return this;
        }
        public override WowGuid128 To128()
        {
            return WowGuid128.Create(this);
        }
    }
}
