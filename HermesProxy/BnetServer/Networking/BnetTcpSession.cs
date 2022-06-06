// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using BNetServer.Services;
using HermesProxy.World;
using HermesProxy.World.Enums;

namespace BNetServer.Networking
{
    public class BnetTcpSession : SSLSocket, BnetServices.INetwork
    {
        private readonly BnetServices.ServiceManager _handlerManager;

        public BnetTcpSession(Socket socket) : base(socket)
        {
            _handlerManager = new BnetServices.ServiceManager("BnetTcp", this, initialSession: null);
        }

        public override void Accept()
        {
            string ipAddress = GetRemoteIpEndPoint().ToString();
            Log.Print(LogType.Server, $"Accepting connection from {ipAddress}.");
            AsyncHandshake(BnetServerCertificate.Certificate);
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
                    _handlerManager.Invoke(header.ServiceId, (OriginalHash)header.ServiceHash, header.MethodId, header.Token, stream);
                }
            }

            await AsyncRead();
        }

        public void SendRpcMessage(uint serviceId, OriginalHash service, uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message)
        {
            Header header = new();
            header.Token = token;
            header.Status = (uint)status;
            header.ServiceId = serviceId;
            header.ServiceHash = (uint)service;
            header.MethodId = methodId;
            if (message != null)
                header.Size = (uint)message.CalculateSize();

            ByteBuffer buffer = new();
            buffer.WriteBytes(GetHeaderSize(header), 2);
            buffer.WriteBytes(header.ToByteArray());
            if (message != null)
                buffer.WriteBytes(message.ToByteArray());

            AsyncWrite(buffer.GetData());
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
    }

    public class AccountInfo
    {   
        public uint Id;
        public WowGuid128 BnetAccountGuid => WowGuid128.Create(HighGuidType703.BNetAccount, Id);
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
        public WowGuid128 WoWAccountGuid => WowGuid128.Create(HighGuidType703.WowAccount, Id);
        public string Name;
        public string DisplayName;
        public uint UnbanDate;
        public bool IsBanned;
        public bool IsPermanenetlyBanned;

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
        }
    }
}
