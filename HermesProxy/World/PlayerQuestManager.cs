using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World;

public class PlayerQuestTracker
{
    private Dictionary<int, ulong> _cachedQuestCompleted = new();
    public bool NeedToLoadBeSentToClient { get; set; } = true;

    public GlobalSessionData Session { get; }

    public PlayerQuestTracker(GlobalSessionData globalSession)
    {
        Session = globalSession;
    }

    public void MarkQuestAsNotCompleted(uint questQuestId)
    {
        Session.AccountMetaDataMgr.MarkQuestAsNotCompleted(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name, questQuestId);

        var questBit = GameData.GetUniqueQuestBit(questQuestId);
        if (questBit.HasValue)
        {
            SendSingleUpdateToClient(questBit.Value, false);
        }
    }

    public void MarkQuestAsCompleted(uint questQuestId)
    {
        Session.AccountMetaDataMgr.MarkQuestAsCompleted(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name, questQuestId);

        var questBit = GameData.GetUniqueQuestBit(questQuestId);
        if (questBit.HasValue)
        {
            SendSingleUpdateToClient(questBit.Value, true);
        }
    }

    public void Reload()
    {
        var questIds = Session.AccountMetaDataMgr.GetAllCompletedQuests(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name);

        _cachedQuestCompleted = new Dictionary<int, ulong>();
        foreach (uint questId in questIds)
        {
            uint? questBit = GameData.GetUniqueQuestBit(questId);
            if (!questBit.HasValue)
                continue;

            int idx = (int)(((questBit - 1) >> 6));
            int bitIdx = (int)((questBit - 1) & 63);
            _cachedQuestCompleted.TryAdd(idx, 0);
            _cachedQuestCompleted[idx] |= ((ulong)1) << bitIdx;
        }
    }
    
    private void SendSingleUpdateToClient(uint questBit, bool isSet)
    {
        int idx = (int)(((questBit - 1) >> 6));
        int bitIdx = (int)((questBit - 1) & 63);
        _cachedQuestCompleted.TryAdd(idx, 0);
        if (isSet)
            _cachedQuestCompleted[idx] |= ((ulong)1) << bitIdx;
        else
            _cachedQuestCompleted[idx] ^= ((ulong)1) << bitIdx;
        
        ObjectUpdate updateData = new ObjectUpdate(Session.GameState.CurrentPlayerGuid, UpdateTypeModern.Values, Session);
        updateData.ActivePlayerData.QuestCompleted[idx] = _cachedQuestCompleted[idx];

        UpdateObject updatePacket = new UpdateObject(Session.GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        Session.WorldClient.SendPacketToClient(updatePacket);
    }

    public void WriteAllCompletedIntoArray(ulong?[] dest)
    {
        Reload();
        foreach (var kv in _cachedQuestCompleted)
        {
            dest[kv.Key] = kv.Value;
        }
    }
}
