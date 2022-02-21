// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using BNetServer;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using System.Collections.Generic;

public static class Global
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
        public bool IsInWorld;
        public uint? CurrentMapId;
        public uint LastEnteredAreaTrigger;
        public WowGuid128 CurrentPlayerGuid;
        public List<int> ActionButtons = new();
        public Dictionary<WowGuid, PlayerCache> CachedPlayers = new();
        public Dictionary<WowGuid128, UpdateFieldsArray> Objects = new();
        public List<WowGuid128> OwnCharacters = new();
        public Dictionary<string, int> ChannelIds = new();

        public void SetChannelId(string name, int id)
        {
            if (ChannelIds.ContainsKey(name))
                ChannelIds[name] = id;
            else
                ChannelIds.Add(name, id);
        }
        public WowGuid128 GetGameAccountGuidForPlayer(WowGuid128 playerGuid)
        {
            if (OwnCharacters.Contains(playerGuid))
                return WowGuid128.Create(HighGuidType703.WowAccount, Global.CurrentSessionData.GameAccountInfo.Id);
            else
                return WowGuid128.Create(HighGuidType703.WowAccount, playerGuid.GetLow());
        }

        public WowGuid128 GetBnetAccountGuidForPlayer(WowGuid128 playerGuid)
        {
            if (OwnCharacters.Contains(playerGuid))
                return WowGuid128.Create(HighGuidType703.BNetAccount, Global.CurrentSessionData.AccountInfo.Id);
            else
                return WowGuid128.Create(HighGuidType703.BNetAccount, playerGuid.GetLow());
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

        public void UpdatePlayerCache(WowGuid guid, PlayerCache data)
        {
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

        public Class GetUnitClass(WowGuid guid)
        {
            if (CachedPlayers.ContainsKey(guid))
                return CachedPlayers[guid].ClassId;

            // TODO: Add Creature Data

            return Class.Warrior;
        }
    }
    public class LoginSessionData
    {
        public BNetServer.Networking.AccountInfo AccountInfo;
        public BNetServer.Networking.GameAccountInfo GameAccountInfo;
        public string Username;
        public string Password;
        public string LoginTicket;
        public byte[] SessionKey;
        public string Locale;
        public string OS;
        public uint Build;
        public Framework.Realm.RealmId RealmId;
        public GameSessionData GameState = new();
        public WorldSocket RealmSocket;
        public WorldSocket InstanceSocket;
        public WorldClient WorldClient;
        public SniffFile ModernSniff;

        public void OnDisconnect()
        {
            HermesProxy.Auth.AuthClient.Disconnect();

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
    public static LoginSessionData CurrentSessionData = new();

    public static RealmManager RealmMgr { get { return RealmManager.Instance; } }
    public static LoginServiceManager LoginServiceMgr { get { return LoginServiceManager.Instance; } }
}