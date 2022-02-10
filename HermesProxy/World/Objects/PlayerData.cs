using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class QuestLog
    {
        public int QuestID { get; set; }
        public uint StateFlags { get; set; }
        public uint EndTime { get; set; }
        public uint AcceptTime { get; set; }
        public short[] ObjectiveProgress { get; } = new short[24];
    }
    public class PlayerData
    {
        public WowGuid DuelArbiter { get; set; }
        public WowGuid WowAccount { get; set; }
        public WowGuid LootTargetGUID { get; set; }
        public uint PlayerFlags { get; set; }
        public uint PlayerFlagsEx { get; set; }
        public uint GuildRankID { get; set; }
        public uint GuildDeleteDate { get; set; }
        public int GuildLevel { get; set; }
        public byte PartyType { get; set; }
        public byte NumBankSlots { get; set; }
        public byte NativeSex { get; set; }
        public byte Inebriation { get; set; }
        public byte PvpTitle { get; set; }
        public byte ArenaFaction { get; set; }
        public byte PvPRank { get; set; }
        public uint DuelTeam { get; set; }
        public int GuildTimeStamp { get; set; }
        public QuestLog[] QuestLog { get; } = new QuestLog[25];
        public VisibleItem[] VisibleItems { get; set; } = new VisibleItem[19];
        public int PlayerTitle { get; set; }
        public int FakeInebriation { get; set; }
        public uint VirtualPlayerRealm { get; set; }
        public uint CurrentSpecID { get; set; }
        public int TaxiMountAnimKitID { get; set; }
        public float[] AvgItemLevel { get; } = new float[6];
        public byte CurrentBattlePetBreedQuality { get; set; }
        public int HonorLevel { get; set; }
        public ChrCustomizationChoice[] Customizations = new ChrCustomizationChoice[36];
    }
}
