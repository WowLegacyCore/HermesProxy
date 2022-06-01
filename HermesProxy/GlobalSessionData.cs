using HermesProxy.Auth;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using System.Collections.Generic;
using ArenaTeamInspectData = HermesProxy.World.Server.Packets.ArenaTeamInspectData;

namespace HermesProxy
{
    public class PlayerCache
    {
        public string Name;
        public Race RaceId;
        public Class ClassId;
        public Gender SexId;
        public byte Level;
    }
    public class GameSessionData
    {
        public bool HasWsgHordeFlagCarrier;
        public bool HasWsgAllyFlagCarrier;
        public bool ChannelDisplayList;
        public bool ShowPlayedTime;
        public bool IsInFarSight;
        public bool IsInTaxiFlight;
        public bool IsWaitingForTaxiStart;
        public bool IsWaitingForNewWorld;
        public bool IsFirstEnterWorld;
        public bool IsConnectedToInstance;
        public bool IsInWorld;
        public uint? CurrentMapId;
        public uint CurrentTaxiNode;
        public List<byte> UsableTaxiNodes = new();
        public uint PendingTransferMapId;
        public uint LastEnteredAreaTrigger;
        public uint LastDispellSpellId;
        public bool IsPassingOnLoot;
        public int GroupUpdateCounter;
        public uint GroupReadyCheckResponses;
        public World.Server.Packets.PartyUpdate[] CurrentGroups = new World.Server.Packets.PartyUpdate[2];
        public WowGuid128 CurrentPlayerGuid;
        public long CurrentPlayerCreateTime;
        public uint CurrentGuildCreateTime;
        public uint CurrentGuildNumAccounts;
        public WowGuid128 CurrentInteractedWithNPC;
        public WowGuid128 CurrentInteractedWithGO;
        public uint LastWhoRequestId;
        public WowGuid128 CurrentPetGuid;
        public uint[] CurrentArenaTeamIds = new uint[3];
        public ClientCastRequest CurrentClientNormalCast;  // regular spell casts
        public ClientCastRequest CurrentClientSpecialCast; // next melee or auto repeat spells
        public ClientCastRequest CurrentClientPetCast;
        public List<ClientCastRequest> PendingClientCasts = new List<ClientCastRequest>();
        public List<ClientCastRequest> PendingClientPetCasts = new List<ClientCastRequest>();
        public WowGuid64 LastLootTargetGuid;
        public List<int> ActionButtons = new();
        public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationUpdateTime = new();
        public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationLeft = new();
        public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationFull = new();
        public Dictionary<WowGuid128, Dictionary<byte, WowGuid128>> UnitAuraCaster = new();
        public Dictionary<WowGuid128, PlayerCache> CachedPlayers = new();
        public Dictionary<WowGuid128, uint> PlayerGuildIds = new();
        public System.Threading.Mutex ObjectCacheMutex = new System.Threading.Mutex();
        public Dictionary<WowGuid128, Dictionary<int, UpdateField>> ObjectCacheLegacy = new();
        public Dictionary<WowGuid128, UpdateFieldsArray> ObjectCacheModern = new();
        public Dictionary<WowGuid128, ObjectType> OriginalObjectTypes = new();
        public Dictionary<WowGuid128, uint[]> ItemGems = new();
        public Dictionary<uint, Class> CreatureClasses = new();
        public List<WowGuid128> OwnCharacters = new();
        public Dictionary<string, int> ChannelIds = new();
        public Dictionary<uint, uint> ItemBuyCount = new();
        public Dictionary<uint, uint> RealSpellToLearnSpell = new();
        public Dictionary<uint, ArenaTeamData> ArenaTeams = new();
        public World.Server.Packets.MailListResult PendingMailListPacket;
        public HashSet<uint> RequestedItemTextIds = new HashSet<uint>();
        public Dictionary<uint, string> ItemTexts = new Dictionary<uint, string>();
        public Dictionary<uint, uint> BattleFieldQueueTypes = new Dictionary<uint, uint>();
        public Dictionary<uint, long> BattleFieldQueueTimes = new Dictionary<uint, long>();
        public Dictionary<uint, uint> DailyQuestsDone = new Dictionary<uint, uint>();
        public HashSet<WowGuid128> FlagCarrierGuids = new HashSet<WowGuid128>();
        public Dictionary<WowGuid64, ushort> ObjectSpawnCount = new Dictionary<WowGuid64, ushort>();
        public HashSet<WowGuid128> HunterPetGuids = new HashSet<WowGuid128>();
        public Dictionary<WowGuid128, Array<ArenaTeamInspectData>> PlayerArenaTeams = new Dictionary<WowGuid128, Array<ArenaTeamInspectData>>();
        public HashSet<string> AddonPrefixes = new HashSet<string>();
        public Dictionary<byte, Dictionary<byte, int>> FlatSpellMods = new Dictionary<byte, Dictionary<byte, int>>();
        public Dictionary<byte, Dictionary<byte, int>> PctSpellMods = new Dictionary<byte, Dictionary<byte, int>>();

        public uint GetCurrentGroupSize()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return 0;

            // Don't count self.
            return (uint)(group.PlayerList.Count > 1 ? group.PlayerList.Count - 1 : 0);
        }
        public WowGuid128 GetCurrentGroupLeader()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return WowGuid128.Empty;

            return group.LeaderGUID;
        }
        public LootMethod GetCurrentLootMethod()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return LootMethod.FreeForAll;

            return group.LootSettings.Method;
        }
        public WowGuid128 GetCurrentGroupGuid()
        {
            var group = GetCurrentGroup();
            if (group == null)
                return WowGuid128.Empty;

            return group.PartyGUID;
        }
        public World.Server.Packets.PartyUpdate GetCurrentGroup()
        {
            return CurrentGroups[GetCurrentPartyIndex()];
        }
        public sbyte GetCurrentPartyIndex()
        {
            return (sbyte)(IsInBattleground() ? 1 : 0);
        }
        public byte GetItemSpellSlot(WowGuid128 guid, uint spellId)
        {
            int OBJECT_FIELD_ENTRY = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
            if (OBJECT_FIELD_ENTRY < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            uint itemId = updates[OBJECT_FIELD_ENTRY].UInt32Value;
            return GameData.GetItemEffectSlot(itemId, spellId);
        }
        public uint GetItemId(WowGuid128 guid)
        {
            int OBJECT_FIELD_ENTRY = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
            if (OBJECT_FIELD_ENTRY < 0)
                return 0;

            var updates = GetCachedObjectFieldsLegacy(guid);
            if (updates == null)
                return 0;

            uint itemId = updates[OBJECT_FIELD_ENTRY].UInt32Value;
            return itemId;
        }
        public void SetFlatSpellMod(byte spellMod, byte spellMask, int amount)
        {
            if (FlatSpellMods.ContainsKey(spellMod))
            {
                if (FlatSpellMods[spellMod].ContainsKey(spellMask))
                {
                    FlatSpellMods[spellMod][spellMask] = amount;

                }
                else
                {
                    FlatSpellMods[spellMod].Add(spellMask, amount);
                }
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(spellMask, amount);
                FlatSpellMods.Add(spellMod, dict);
            }
        }
        public void SetPctSpellMod(byte spellMod, byte spellMask, int amount)
        {
            if (PctSpellMods.ContainsKey(spellMod))
            {
                if (PctSpellMods[spellMod].ContainsKey(spellMask))
                {
                    PctSpellMods[spellMod][spellMask] = amount;

                }
                else
                {
                    PctSpellMods[spellMod].Add(spellMask, amount);
                }
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(spellMask, amount);
                PctSpellMods.Add(spellMod, dict);
            }
        }
        public ArenaTeamInspectData GetArenaTeamDataForPlayer(WowGuid128 guid, byte slot)
        {
            if (PlayerArenaTeams.ContainsKey(guid))
                return PlayerArenaTeams[guid][slot];

            return new ArenaTeamInspectData();
        }
        public void StoreArenaTeamDataForPlayer(WowGuid128 guid, byte slot, ArenaTeamInspectData team)
        {
            if (!PlayerArenaTeams.ContainsKey(guid))
                PlayerArenaTeams.Add(guid, new Array<ArenaTeamInspectData>(3, new ArenaTeamInspectData()));

            PlayerArenaTeams[guid][slot] = team;
        }
        public WowGuid64 GetInventorySlotItem(int slot)
        {
            int PLAYER_FIELD_INV_SLOT_HEAD = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_INV_SLOT_HEAD);
            if (PLAYER_FIELD_INV_SLOT_HEAD >= 0)
            {
                var updates = GetCachedObjectFieldsLegacy(CurrentPlayerGuid);
                if (updates != null)
                    return updates.GetGuidValue(PLAYER_FIELD_INV_SLOT_HEAD + slot * 2).To64();
            }
            return WowGuid64.Empty;
        }
        public ushort GetObjectSpawnCounter(WowGuid64 guid)
        {
            ushort count;
            if (ObjectSpawnCount.TryGetValue(guid, out count))
                return count;
            return 0;
        }
        public void IncrementObjectSpawnCounter(WowGuid64 guid)
        {
            if (ObjectSpawnCount.ContainsKey(guid))
                ObjectSpawnCount[guid]++;
            else
                ObjectSpawnCount.Add(guid, 0);
        }
        public void SetDailyQuestSlot(uint slot, uint questId)
        {
            if (DailyQuestsDone.ContainsKey(slot))
            {
                if (questId != 0)
                    DailyQuestsDone[slot] = questId;
                else
                    DailyQuestsDone.Remove(slot);
            }
            else if (questId != 0)
                DailyQuestsDone.Add(slot, questId);
        }
        public bool IsAlliancePlayer(WowGuid128 guid)
        {
            PlayerCache cache;
            if (CachedPlayers.TryGetValue(guid, out cache))
                return GameData.IsAllianceRace(cache.RaceId);
            return false;
        }
        public bool IsInBattleground()
        {
            if (CurrentMapId == null)
                return false;

            uint bgId = GameData.GetBattlegroundIdFromMapId((uint)CurrentMapId);
            if (bgId != 0)
            {
                // Only if we are properly queued for the BG.
                foreach (var queue in BattleFieldQueueTypes)
                {
                    if (LegacyVersion.RemovedInVersion(Enums.ClientVersionBuild.V2_0_1_6180))
                    {
                        if (queue.Value == CurrentMapId)
                            return true;
                    }
                    else
                    {
                        if (queue.Value == bgId)
                            return true;
                    }
                }
            }

            return false;
        }
        public long GetBattleFieldQueueTime(uint queueSlot)
        {
            if (BattleFieldQueueTimes.ContainsKey(queueSlot))
                return BattleFieldQueueTimes[queueSlot];
            else
            {
                long time = Time.UnixTime;
                BattleFieldQueueTimes.Add(queueSlot, time);
                return time;
            }
        }
        public void StoreBattleFieldQueueType(uint queueSlot, uint mapOrBgId)
        {
            if (BattleFieldQueueTypes.ContainsKey(queueSlot))
                BattleFieldQueueTypes[queueSlot] = mapOrBgId;
            else
                BattleFieldQueueTypes.Add(queueSlot, mapOrBgId);
        }
        public uint GetBattleFieldQueueType(uint queueSlot)
        {
            if (BattleFieldQueueTypes.ContainsKey(queueSlot))
                return BattleFieldQueueTypes[queueSlot];
            return 0;
        }
        public void StoreAuraDurationLeft(WowGuid128 guid, byte slot, int duration, int currentTime)
        {
            if (UnitAuraDurationLeft.ContainsKey(guid))
            {
                if (UnitAuraDurationLeft[guid].ContainsKey(slot))
                    UnitAuraDurationLeft[guid][slot] = duration;
                else
                    UnitAuraDurationLeft[guid].Add(slot, duration);
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(slot, duration);
                UnitAuraDurationLeft.Add(guid, dict);
            }

            if (UnitAuraDurationUpdateTime.ContainsKey(guid))
            {
                if (UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
                    UnitAuraDurationUpdateTime[guid][slot] = currentTime;
                else
                    UnitAuraDurationUpdateTime[guid].Add(slot, currentTime);
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(slot, currentTime);
                UnitAuraDurationUpdateTime.Add(guid, dict);
            }
        }
        public void StoreAuraDurationFull(WowGuid128 guid, byte slot, int duration)
        {
            if (UnitAuraDurationFull.ContainsKey(guid))
            {
                if (UnitAuraDurationFull[guid].ContainsKey(slot))
                    UnitAuraDurationFull[guid][slot] = duration;
                else
                    UnitAuraDurationFull[guid].Add(slot, duration);
            }
            else
            {
                Dictionary<byte, int> dict = new Dictionary<byte, int>();
                dict.Add(slot, duration);
                UnitAuraDurationFull.Add(guid, dict);
            }
        }
        public void ClearAuraDuration(WowGuid128 guid, byte slot)
        {
            if (UnitAuraDurationUpdateTime.ContainsKey(guid) &&
                UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
                UnitAuraDurationUpdateTime[guid].Remove(slot);

            if (UnitAuraDurationLeft.ContainsKey(guid) &&
                UnitAuraDurationLeft[guid].ContainsKey(slot))
                UnitAuraDurationLeft[guid].Remove(slot);

            if (UnitAuraDurationFull.ContainsKey(guid) &&
                UnitAuraDurationFull[guid].ContainsKey(slot))
                UnitAuraDurationFull[guid].Remove(slot);
        }
        public void GetAuraDuration(WowGuid128 guid, byte slot, out int left, out int full)
        {
            if (UnitAuraDurationLeft.ContainsKey(guid) &&
                UnitAuraDurationLeft[guid].ContainsKey(slot))
                left = UnitAuraDurationLeft[guid][slot];
            else
                left = -1;

            if (UnitAuraDurationFull.ContainsKey(guid) &&
                UnitAuraDurationFull[guid].ContainsKey(slot))
                full = UnitAuraDurationFull[guid][slot];
            else
                full = left;

            if (left > 0 &&
                UnitAuraDurationUpdateTime.ContainsKey(guid) &&
                UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
                left = left - (System.Environment.TickCount - UnitAuraDurationUpdateTime[guid][slot]);
        }
        public void StoreAuraCaster(WowGuid128 target, byte slot, WowGuid128 caster)
        {
            if (UnitAuraCaster.ContainsKey(target))
            {
                if (UnitAuraCaster[target].ContainsKey(slot))
                    UnitAuraCaster[target][slot] = caster;
                else
                    UnitAuraCaster[target].Add(slot, caster);
            }
            else
            {
                Dictionary<byte, WowGuid128> dict = new Dictionary<byte, WowGuid128>();
                dict.Add(slot, caster);
                UnitAuraCaster.Add(target, dict);
            }
        }
        public void ClearAuraCaster(WowGuid128 guid, byte slot)
        {
            if (UnitAuraCaster.ContainsKey(guid) &&
                UnitAuraCaster[guid].ContainsKey(slot))
                UnitAuraCaster[guid].Remove(slot);
        }
        public WowGuid128 GetAuraCaster(WowGuid128 target, byte slot)
        {
            if (UnitAuraCaster.ContainsKey(target) &&
                UnitAuraCaster[target].ContainsKey(slot))
                return UnitAuraCaster[target][slot];
            return null;
        }
        public void StorePlayerGuildId(WowGuid128 guid, uint guildId)
        {
            if (PlayerGuildIds.ContainsKey(guid))
                PlayerGuildIds[guid] = guildId;
            else
                PlayerGuildIds.Add(guid, guildId);
        }
        public uint GetPlayerGuildId(WowGuid128 guid)
        {
            if (PlayerGuildIds.ContainsKey(guid))
                return PlayerGuildIds[guid];
            return 0;
        }
        public uint[] GetGemsForItem(WowGuid128 guid)
        {
            if (ItemGems.ContainsKey(guid))
                return ItemGems[guid];
            return null;
        }
        public void SaveGemsForItem(WowGuid128 guid, uint?[] gems)
        {
            uint[] existing;
            if (ItemGems.ContainsKey(guid))
                existing = ItemGems[guid];
            else
            {
                existing = new uint[ItemConst.MaxGemSockets];
                ItemGems.Add(guid, existing);
            }

            for (int i = 0; i < ItemConst.MaxGemSockets; i++)
            {
                if (gems[i] != null)
                    existing[i] = (uint)gems[i];
            }
        }
        public WowGuid128 GetPetGuidByNumber(uint petNumber)
        {
            ObjectCacheMutex.WaitOne();
            foreach (var itr in ObjectCacheModern)
            {
                if (itr.Key.GetHighType() == HighGuidType.Pet &&
                    itr.Key.GetEntry() == petNumber)
                {
                    ObjectCacheMutex.ReleaseMutex();
                    return itr.Key;
                }  
            }
            ObjectCacheMutex.ReleaseMutex();
            return null;
        }
        public void StoreOriginalObjectType(WowGuid128 guid, ObjectType type)
        {
            if (OriginalObjectTypes.ContainsKey(guid))
                OriginalObjectTypes[guid] = type;
            else
                OriginalObjectTypes.Add(guid, type);
        }
        public ObjectType GetOriginalObjectType(WowGuid128 guid)
        {
            if (OriginalObjectTypes.ContainsKey(guid))
                return OriginalObjectTypes[guid];

            return guid.GetObjectType();
        }
        public void StoreRealSpell(uint realSpellId, uint learnSpellId)
        {
            if (RealSpellToLearnSpell.ContainsKey(realSpellId))
                RealSpellToLearnSpell[realSpellId] = learnSpellId;
            else
                RealSpellToLearnSpell.Add(realSpellId, learnSpellId);
        }
        public uint GetLearnSpellFromRealSpell(uint spellId)
        {
            if (RealSpellToLearnSpell.ContainsKey(spellId))
                return RealSpellToLearnSpell[spellId];

            return spellId;
        }
        public void StoreCreatureClass(uint entry, Class classId)
        {
            if (CreatureClasses.ContainsKey(entry))
                CreatureClasses[entry] = classId;
            else
                CreatureClasses.Add(entry, classId);
        }
        public void SetItemBuyCount(uint itemId, uint buyCount)
        {
            if (ItemBuyCount.ContainsKey(itemId))
                ItemBuyCount[itemId] = buyCount;
            else
                ItemBuyCount.Add(itemId, buyCount);
        }
        public uint GetItemBuyCount(uint itemId)
        {
            if (ItemBuyCount.ContainsKey(itemId))
                return ItemBuyCount[itemId];

            return 1;
        }
        public void SetChannelId(string name, int id)
        {
            if (ChannelIds.ContainsKey(name))
                ChannelIds[name] = id;
            else
                ChannelIds.Add(name, id);
        }
        public string GetChannelName(int id)
        {
            foreach (var itr in ChannelIds)
            {
                if (itr.Value == id)
                    return itr.Key;
            }
            return "";
        }

        public string GetPlayerName(WowGuid128 guid)
        {
            if (CachedPlayers.ContainsKey(guid))
            {
                if (CachedPlayers[guid].Name != null)
                    return CachedPlayers[guid].Name;
            }
            return "";
        }

        public WowGuid128 GetPlayerGuidByName(string name)
        {
            name = name.Trim().Replace("\0", "");
            foreach (var player in CachedPlayers)
            {
                if (player.Value.Name == name)
                    return player.Key;
            }
            return null;
        }

        public void UpdatePlayerCache(WowGuid128 guid, PlayerCache data)
        {
            if (data.Name != null)
                data.Name = data.Name.Trim().Replace("\0", "");
            if (CachedPlayers.ContainsKey(guid))
            {
                if (!string.IsNullOrEmpty(data.Name))
                    CachedPlayers[guid].Name = data.Name;
                if (data.RaceId != Race.None)
                    CachedPlayers[guid].RaceId = data.RaceId;
                if (data.ClassId != Class.None)
                    CachedPlayers[guid].ClassId = data.ClassId;
                if (data.SexId != Gender.None)
                    CachedPlayers[guid].SexId = data.SexId;
                if (data.Level != 0)
                    CachedPlayers[guid].Level = data.Level;
            }
            else
                CachedPlayers.Add(guid, data);
        }

        public Class GetUnitClass(WowGuid128 guid)
        {
            if (CachedPlayers.ContainsKey(guid))
                return CachedPlayers[guid].ClassId;

            if (CreatureClasses.ContainsKey(guid.GetEntry()))
                return CreatureClasses[guid.GetEntry()];

            return Class.Warrior;
        }

        public Dictionary<int, UpdateField> GetCachedObjectFieldsLegacy(WowGuid128 guid)
        {
            Dictionary<int, UpdateField> dict;
            ObjectCacheMutex.WaitOne();
            if (ObjectCacheLegacy.TryGetValue(guid, out dict))
            {
                ObjectCacheMutex.ReleaseMutex();
                return dict;
            }
            ObjectCacheMutex.ReleaseMutex();
            return null;
        }

        public UpdateFieldsArray GetCachedObjectFieldsModern(WowGuid128 guid)
        {
            UpdateFieldsArray array;
            ObjectCacheMutex.WaitOne();
            if (ObjectCacheModern.TryGetValue(guid, out array))
            {
                ObjectCacheMutex.ReleaseMutex();
                return array;
            }
            ObjectCacheMutex.ReleaseMutex();
            return null;
        }
    }

    public class ClientCastRequest
    {
        public bool HasStarted;
        public uint SpellId;
        public uint SpellXSpellVisualId;
        public long Timestamp;
        public WowGuid128 ClientGUID;
        public WowGuid128 ServerGUID;
        public WowGuid128 ItemGUID;
    }
    public class ArenaTeamData
    {
        public string Name;
        public uint TeamSize;
        public uint WeekPlayed;
        public uint WeekWins;
        public uint SeasonPlayed;
        public uint SeasonWins;
        public uint Rating;
        public uint Rank;
        public uint BackgroundColor;
        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
    }
    public class GlobalSessionData
    {
        public BNetServer.Networking.AccountInfo AccountInfo;
        public BNetServer.Networking.GameAccountInfo GameAccountInfo;
        public RealmManager RealmManager = new();
        public string Username;
        public string LoginTicket;
        public byte[] SessionKey;
        public string Locale;
        public string OS;
        public uint Build;
        public Framework.Realm.RealmId RealmId;
        public GameSessionData GameState = new();
        public AccountMetaDataManager AccountMetaDataMgr;
        public AccountDataManager AccountDataMgr;
        public WorldSocket RealmSocket;
        public WorldSocket InstanceSocket;
        public AuthClient AuthClient;
        public WorldClient WorldClient;
        public SniffFile ModernSniff;
        public Dictionary<string, WowGuid128> GuildsByName = new();
        public Dictionary<uint, List<string>> GuildRanks = new();

        public void StoreGuildRankNames(uint guildId, List<string> ranks)
        {
            if (GuildRanks.ContainsKey(guildId))
                GuildRanks[guildId] = ranks;
            else
                GuildRanks.Add(guildId, ranks);
        }
        public uint GetGuildRankIdByName(uint guildId, string name)
        {
            if (GuildRanks.ContainsKey(guildId))
            {
                for (int i = 0; i < GuildRanks[guildId].Count; i++)
                {
                    if (GuildRanks[guildId][i] == name)
                        return (uint)i;
                }
            }
            return 0;
        }
        public string GetGuildRankNameById(uint guildId, byte rankId)
        {
            if (GuildRanks.ContainsKey(guildId))
                return GuildRanks[guildId][rankId];

            return $"Rank {rankId}";
        }
        public void StoreGuildGuidAndName(WowGuid128 guid, string name)
        {
            if (GuildsByName.ContainsKey(name))
                GuildsByName[name] = guid;
            else
                GuildsByName.Add(name, guid);
        }
        public WowGuid128 GetGuildGuid(string name)
        {
            if (GuildsByName.ContainsKey(name))
                return GuildsByName[name];

            WowGuid128 guid = WowGuid128.Create(HighGuidType703.Guild, (ulong)(GuildsByName.Count + 1));
            GuildsByName.Add(name, guid);
            return guid;
        }

        public WowGuid128 GetGameAccountGuidForPlayer(WowGuid128 playerGuid)
        {
            if (GameState.OwnCharacters.Contains(playerGuid))
                return WowGuid128.Create(HighGuidType703.WowAccount, GameAccountInfo.Id);
            else
                return WowGuid128.Create(HighGuidType703.WowAccount, playerGuid.GetCounter());
        }

        public WowGuid128 GetBnetAccountGuidForPlayer(WowGuid128 playerGuid)
        {
            if (GameState.OwnCharacters.Contains(playerGuid))
                return WowGuid128.Create(HighGuidType703.BNetAccount, AccountInfo.Id);
            else
                return WowGuid128.Create(HighGuidType703.BNetAccount, playerGuid.GetCounter());
        }

        public void OnDisconnect()
        {
            if (ModernSniff != null)
            {
                ModernSniff.CloseFile();
                ModernSniff = null;
            }
            if (AuthClient != null)
            {
                AuthClient.Disconnect();
                AuthClient = null;
            }
            if (WorldClient != null)
            {
                WorldClient.Disconnect();
                WorldClient = null;
            }
            if (RealmSocket != null)
            {
                RealmSocket.CloseSocket();
                RealmSocket = null;
            }
            if (InstanceSocket != null)
            {
                InstanceSocket.CloseSocket();
                InstanceSocket = null;
            }

            GameState = new();
        }
    }
}
