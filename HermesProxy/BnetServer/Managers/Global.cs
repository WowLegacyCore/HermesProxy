// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using BNetServer;
using HermesProxy;
using HermesProxy.World.Client;
using World;

public static class Global
{
    public struct GameSessionData
    {
        public bool IsInWorld;
        public uint? CurrentMapId;
        public WowGuid128 CurrentPlayerGuid;
    }
    public struct LoginSessionData
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
        public GameSessionData GameData;
        public WorldSocket RealmSocket;
        public WorldSocket InstanceSocket;
        public WorldClient WorldClient;
    }
    public static LoginSessionData CurrentSessionData;

    public static RealmManager RealmMgr { get { return RealmManager.Instance; } }
    public static LoginServiceManager LoginServiceMgr { get { return LoginServiceManager.Instance; } }
}