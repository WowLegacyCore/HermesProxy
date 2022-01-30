using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Google.Protobuf;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.IO.Packet;
using HermesProxy.Framework.Logging;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.REST;
using HermesProxy.Network.BattleNet.Services;
using HermesProxy.Network.Realm;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        public delegate BattlenetRpcErrorCode ClientRequestHandler(Dictionary<string, Variant> parameters, ClientResponse response);

        readonly Dictionary<string, ClientRequestHandler> _clientRequestHandlers;
        readonly Socket _socket;
        readonly SslStream _sslStream;
        readonly byte[] _buffer = new byte[4096];

        bool _authed;
        uint _requestToken = 0;

        public BattlenetSession(Socket socket, X509Certificate2 cert)
        {
            if (_sslStream != null)
                throw new InvalidOperationException("There is already a BattlenetSession initialized!");

            _clientRequestHandlers = new Dictionary<string, ClientRequestHandler>
            {
                { "Command_RealmListTicketRequest_v1_b9",   GetRealmListTicket },
                { "Command_LastCharPlayedRequest_v1_b9",    GetLastPlayedCharacter },
                { "Command_RealmListRequest_v1_b9",         GetRealmList },
                // { "Command_RealmJoinRequest_v1_b9",         JoinRealm }
            };

            _socket = socket;

            _sslStream = new SslStream(new NetworkStream(socket), false);
            _sslStream.AuthenticateAsServer(cert, false, SslProtocols.Tls, false);
        }

        /// <summary>
        /// Handles any incoming <see cref="BattlenetHandler"/>
        /// </summary>
        public async Task HandleIncomingConnection()
        {
            while (true)
            {
                if (_sslStream == null)
                    return;

                var receivedLen = await _sslStream.ReadAsync(_buffer);
                if (receivedLen > 0)
                {
                    var data = new byte[receivedLen];
                    Buffer.BlockCopy(_buffer, 0, data, 0, receivedLen);

                    var inputStream = new CodedInputStream(data, 0, data.Length);
                    while (!inputStream.IsAtEnd)
                    {
                        try
                        {
                            var header = new Header();
                            inputStream.ReadMessage(header);

                            if (header.ServiceId != 0xFE && header.ServiceHash != 0)
                            {
                                var handler = ServiceHandler.GetHandler((ServiceHash)header.ServiceHash, header.MethodId);
                                if (handler != null)
                                    await handler.Invoke(this, header.Token, inputStream);
                                else
                                {
                                    Log.Print(LogType.Error, $"Session ({GetRemoteEndpoint()}) tried to call not implemented MethodID: {header.MethodId} for ServiceHash: {(ServiceHash)header.ServiceHash} (0x{header.ServiceHash:X})");
                                    await SendResponse(header.Token, BattlenetRpcErrorCode.RpcNotImplemented);
                                }
                            }
                        }
                        catch /*(Exception ex)*/
                        {
                            // Log.Print(LogType.Error, ex);

                            await CloseSocket();
                            return;
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Sends a <see cref="IMessage"/> to the client.
        /// </summary>
        public async Task SendResponse(uint token, IMessage message)
        {
            var header = new Header
            {
                Token = token,
                ServiceId = 0xFE,
                Size = (uint)message.CalculateSize()
            };

            var headerSize = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSize);

            var writer = new PacketWriter();
            writer.WriteBytes(headerSize, 2);
            writer.WriteBytes(header.ToByteArray());
            writer.WriteBytes(message.ToByteArray());

            // Send the data through the sslStream
            await SendData(writer.GetData());
        }

        /// <summary>
        /// Sends a <see cref="BattlenetRpcErrorCode"/> to the client.
        /// </summary>
        public async Task SendResponse(uint token, BattlenetRpcErrorCode errorCode)
        {
            var header = new Header
            {
                Token = token,
                Status = (uint)errorCode,
                ServiceId = 0xFE
            };

            var headerSize = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSize);

            var writer = new PacketWriter();
            writer.WriteBytes(headerSize, 2);
            writer.WriteBytes(header.ToByteArray());

            // Send the data through the sslStream
            await SendData(writer.GetData());
        }

        /// <summary>
        /// Sends a <see cref="IMessage"/> request to the client from the sslstream
        /// </summary>
        public async Task SendRequest(ServiceHash hash, uint methodId, IMessage message)
        {
            var header = new Header
            {
                ServiceId = 0,
                ServiceHash = (uint)hash,
                MethodId = methodId,
                Size = (uint)message.CalculateSize(),
                Token = _requestToken++
            };

            var headerSize = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSize);

            var writer = new PacketWriter();
            writer.WriteBytes(headerSize);
            writer.WriteBytes(header.ToByteArray());
            writer.WriteBytes(message.ToByteArray());

            // Send the data through the sslStream
            await SendData(writer.GetData());
        }

        private BattlenetRpcErrorCode GetRealmListTicket(Dictionary<string, Variant> parameters, ClientResponse response)
        {
            var clientInfo = GetParam(parameters, "Param_ClientInfo");
            if (clientInfo != null)
            {
                var realmListInformation = JSON.CreateObject<RealmListTicketClientInformation>(clientInfo.BlobValue.ToStringUtf8(), true);
                if (realmListInformation == null)
                    return BattlenetRpcErrorCode.WowServicesDeniedRealmListTicket;

                // clientsercret thing
            }

            response.Attribute.Add(new Bgs.Protocol.Attribute()
            {
                Name = "Param_RealmListTicket",
                Value = new Variant
                {
                    BlobValue = ByteString.CopyFrom("AuthRealmListTicket", Encoding.UTF8)
                }
            });

            return BattlenetRpcErrorCode.Ok;
        }

        private BattlenetRpcErrorCode GetLastPlayedCharacter(Dictionary<string, Variant> parameters, ClientResponse response)
            => _authed ? BattlenetRpcErrorCode.Ok : BattlenetRpcErrorCode.Denied;

        private BattlenetRpcErrorCode GetRealmList(Dictionary<string, Variant> parameters, ClientResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            var compressed = RealmManager.GetRealmList();
            if (compressed.Length == 0)
                return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

            // Initialize the first attribute with the
            // compressed realmlist data.
            response.Attribute.Add(new Bgs.Protocol.Attribute
            {
                Name = "Param_RealmList",
                Value = new()
                {
                    BlobValue = ByteString.CopyFrom(compressed)
                }
            });

            var realmCharCount = new RealmCharacterCountList();
            foreach (var realm in RealmManager.Realms)
            {
                realmCharCount.Counts.Add(new()
                {
                    WowRealmAddress = realm.ID,
                    Count = 0,
                });
            }

            // Character realmlist count
            compressed = JSON.Deflate("JSONRealmCharacterCointList", realmCharCount);
            response.Attribute.Add(new Bgs.Protocol.Attribute
            {
                Name = "Param_CharacterCountList",
                Value = new()
                {
                    BlobValue = ByteString.CopyFrom(compressed)
                }
            });

            return BattlenetRpcErrorCode.Ok;
        }

        private Variant GetParam(Dictionary<string, Variant> parameters, string paramName) => parameters[paramName];

        private async Task SendData(byte[] data) => await _sslStream.WriteAsync(data);

        /// <summary>
        /// Returns the <see cref="Socket"/> instance <see cref="EndPoint"/>.
        /// </summary>
        public string GetRemoteEndpoint() => $"{_socket.RemoteEndPoint}";

        /// <summary>
        /// Closes the <see cref="SslStream"/> instance.
        /// </summary>
        public async Task CloseSocket()
        {
            await _sslStream.ShutdownAsync();
            _sslStream?.Close();
        }
    }
}
