using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Objects
{
    public class CorpseData
    {
        public WowGuid128 Owner;
        public WowGuid128 PartyGUID;
        public WowGuid128 GuildGUID;
        public uint? DisplayID;
        public uint?[] Items { get; } = new uint?[19]; // itemDisplayId | (itemInventoryType << 24)
        public byte? RaceId;
        public byte? SexId;
        public byte? ClassId;
        public uint? Flags;
        public uint? DynamicFlags;
        public int? FactionTemplate;
        public ChrCustomizationChoice[] Customizations = new ChrCustomizationChoice[36];
    }
}
