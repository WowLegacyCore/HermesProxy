// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Framework.Realm;
using Google.Protobuf;
using HermesProxy;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace BNetServer.Networking
{
    partial class Session : SSLSocket
    {
        GlobalSessionData _globalSession;

        byte[] _clientSecret;
        bool _authed;
        uint _requestToken;

        Dictionary<uint, Action<CodedInputStream>> _responseCallbacks;

        public Session(Socket socket) : base(socket)
        {
            _clientSecret = new byte[32];
            _responseCallbacks = new Dictionary<uint, Action<CodedInputStream>>();
        }

        public override void Accept()
        {
            string ipAddress = GetRemoteIpEndPoint().ToString();
            Log.Print(LogType.Server, $"Accepting connection from {ipAddress}.");
            AsyncHandshake(Global.LoginServiceMgr.GetCertificate());
        }

        public override bool Update()
        {
            if (!base.Update())
                return false;

            return true;
        }

        public async override void ReadHandler(byte[] data, int receivedLength)
        {
            if (!IsOpen())
                return;

            var stream = new CodedInputStream(data, 0, receivedLength);
            while (!stream.IsAtEnd)
            {
                var header = new Header();
                stream.ReadMessage(header);

                if (header.ServiceId != 0xFE && header.ServiceHash != 0)
                {
                    var handler = Global.LoginServiceMgr.GetHandler(header.ServiceHash, header.MethodId);
                    if (handler != null)
                    {
                        Log.Print(LogType.Debug, $"Service {header.ServiceHash} Method {header.MethodId} Token {header.Token}");
                        handler.Invoke(this, header.Token, stream);
                    }
                    else
                    {
                        Log.Print(LogType.Error, $"{GetClientInfo()} tried to call not implemented methodId: {header.MethodId} for servicehash: {header.ServiceHash}");
                        SendResponse(header.Token, BattlenetRpcErrorCode.RpcNotImplemented);
                    }
                }
                else
                {
                    var handler = _responseCallbacks.LookupByKey(header.Token);
                    if (handler != null)
                    {
                        handler(stream);
                        _responseCallbacks.Remove(header.Token);
                    }
                }
            }

            await AsyncRead();
        }

        public async void SendResponse(uint token, IMessage response)
        {
            Header header = new();
            header.Token = token;
            header.ServiceId = 0xFE;
            header.Size = (uint)response.CalculateSize();

            ByteBuffer buffer = new();
            buffer.WriteBytes(GetHeaderSize(header), 2);
            buffer.WriteBytes(header.ToByteArray());
            buffer.WriteBytes(response.ToByteArray());

            await AsyncWrite(buffer.GetData());
        }

        public async void SendResponse(uint token, BattlenetRpcErrorCode status)
        {
            Header header = new();
            header.Token = token;
            header.Status = (uint)status;
            header.ServiceId = 0xFE;

            ByteBuffer buffer = new();
            buffer.WriteBytes(GetHeaderSize(header), 2);
            buffer.WriteBytes(header.ToByteArray());

            await AsyncWrite(buffer.GetData());
        }

        public async void SendRequest(uint serviceHash, uint methodId, IMessage request)
        {
            Header header = new();
            header.ServiceId = 0;
            header.ServiceHash = serviceHash;
            header.MethodId = methodId;
            header.Size = (uint)request.CalculateSize();
            header.Token = _requestToken++;

            ByteBuffer buffer = new();
            buffer.WriteBytes(GetHeaderSize(header), 2);
            buffer.WriteBytes(header.ToByteArray());
            buffer.WriteBytes(request.ToByteArray());

            await AsyncWrite(buffer.GetData());
        }

        public byte[] GetHeaderSize(Header header)
        {
            var size = (ushort)header.CalculateSize();
            byte[] bytes = new byte[2];
            bytes[0] = (byte)((size >> 8) & 0xff);
            bytes[1] = (byte)(size & 0xff);

            var headerSizeBytes = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSizeBytes);

            return bytes;
        }

        public string GetClientInfo()
        {
            string stream = '[' + GetRemoteIpEndPoint().ToString();
            if (_globalSession.AccountInfo != null && !_globalSession.AccountInfo.Login.IsEmpty())
                stream += ", Account: " + _globalSession.AccountInfo.Login;

            if (_globalSession.GameAccountInfo != null)
                stream += ", Game account: " + _globalSession.GameAccountInfo.Name;

            stream += ']';

            return stream;
        }
    }

    public class AccountInfo
    {   
        public uint Id;
        public string Login;
        public uint LoginTicketExpiry;
        public bool IsBanned;
        public bool IsPermanenetlyBanned;

        public Dictionary<uint, GameAccountInfo> GameAccounts;

        public AccountInfo(string name)
        {
            Id = 1;
            Login = name;
            LoginTicketExpiry = (uint)(Time.UnixTime + 10000);
            IsBanned = false;
            IsPermanenetlyBanned = false;

            GameAccounts = new Dictionary<uint, GameAccountInfo>();
            var account = new GameAccountInfo(name);
            GameAccounts[1] = account;
        }
    }

    public class GameAccountInfo
    {
        public uint Id;
        public string Name;
        public string DisplayName;
        public uint UnbanDate;
        public bool IsBanned;
        public bool IsPermanenetlyBanned;

        public Dictionary<uint, byte> CharacterCounts;
        public Dictionary<string, LastPlayedCharacterInfo> LastPlayedCharacters;

        public GameAccountInfo(string name)
        {
            Id = 1;
            Name = name;
            UnbanDate = 0;
            IsPermanenetlyBanned = false;
            IsBanned = IsPermanenetlyBanned || UnbanDate > Time.UnixTime;

            int hashPos = Name.IndexOf('#');
            if (hashPos != -1)
                DisplayName = "WoW" + Name[(hashPos + 1)..];
            else
                DisplayName = Name;

            CharacterCounts = new Dictionary<uint, byte>();
            LastPlayedCharacters = new Dictionary<string, LastPlayedCharacterInfo>();
        }
    }

    public class LastPlayedCharacterInfo
    {
        public RealmId RealmId;
        public string CharacterName;
        public ulong CharacterGUID;
        public uint LastPlayedTime;
    }
}
