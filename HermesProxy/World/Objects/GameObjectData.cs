namespace HermesProxy.World.Objects
{
    public class GameObjectData
    {
        public WowGuid128 CreatedBy;
        public WowGuid128 GuildGUID;
        public int? DisplayID;
        public uint? Flags;
        public float?[] ParentRotation = new float?[4];
        public int? FactionTemplate;
        public int? Level;
        public sbyte? State;
        public sbyte? TypeID;
        public byte? ArtKit;
        public byte? PercentHealth;
        public uint? SpellVisualID;
        public uint? StateSpellVisualID;
        public uint? StateAnimID;
        public uint? StateAnimKitID;
        public uint?[] StateWorldEffectIDs = new uint?[4];
        public uint? CustomParam;
    }
}
