// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using BNetServer;
using HermesProxy.World;
using HermesProxy.World.Client;
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
        public int? CurrentMapId;
        public WowGuid128 CurrentPlayerGuid;
        public List<int> ActionButtons = new();
        public Dictionary<WowGuid, PlayerCache> CachedPlayers = new();
        public Dictionary<WowGuid128, UpdateFieldsArray> Objects = new();

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