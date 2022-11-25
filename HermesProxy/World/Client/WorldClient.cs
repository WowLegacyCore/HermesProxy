using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using HermesProxy.Enums;
using System.Numerics;
using Framework.Constants;
using Framework.Cryptography;
using Framework;
using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using System.Reflection;
using HermesProxy.World.Server;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        Socket _clientSocket;
        bool? _isSuccessful;
        string _username;
        Realm _realm;
        LegacyWorldCrypt _worldCrypt;
        Dictionary<Opcode, Action<WorldPacket>> _packetHandlers;
        GlobalSessionData _globalSession;
        System.Threading.Mutex _sendMutex = new System.Threading.Mutex();

        // packet order is not always the same as new client, sometimes we need to delay packet until another one
        Dictionary<Opcode, List<WorldPacket>> _delayedPacketsToServer;
        Dictionary<Opcode, List<ServerPacket>> _delayedPacketsToClient;

        public WorldClient()
        {
            InitializePacketHandlers();
        }

        public GlobalSessionData GetSession()
        {
            return _globalSession;
        }

        public bool ConnectToWorldServer(Realm realm, GlobalSessionData globalSession)
        {
            _worldCrypt = null;
            _realm = realm;
            _globalSession = globalSession;
            _username = globalSession.Username;
            _isSuccessful = null;
            _delayedPacketsToServer = new Dictionary<Opcode, List<WorldPacket>>();
            _delayedPacketsToClient = new Dictionary<Opcode, List<ServerPacket>>();

            try
            {
                Log.Print(LogType.Network, "Connecting to world server...");
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the specified host.
                var endPoint = new IPEndPoint(realm.ExternalAddress, realm.Port);
                _clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, $"Socket Error: {ex.Message}");
                _isSuccessful = false;
            }

            while (_isSuccessful == null)
            { }

            return (bool)_isSuccessful;
        }

        private void InitializeEncryption(byte[] sessionKey)
        {
            switch (Settings.ServerBuild)
            {
                case ClientVersionBuild.V1_12_1_5875:
                    _worldCrypt = new VanillaWorldCrypt();
                    break;
                case ClientVersionBuild.V2_4_3_8606:
                    _worldCrypt = new TbcWorldCrypt();
                    break;
            }

            if (_worldCrypt != null)
                _worldCrypt.Initialize(sessionKey);
        }

        public void Disconnect()
        {
            if (!IsConnected())
                return;

            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Disconnect(false);

            if (GetSession().WorldClient == this)
                GetSession().WorldClient = null;
        }

        public bool IsConnected()
        {
            return _clientSocket != null && _clientSocket.Connected;
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                Log.Print(LogType.Network, "Connection established!");

                _clientSocket.EndConnect(AR);
                _clientSocket.ReceiveBufferSize = 65535;
                byte[] buffer = new byte[LegacyServerPacketHeader.StructSize];
                _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, $"Connect Error: {ex.Message}");
                if (_isSuccessful == null)
                    _isSuccessful = false;
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = _clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    Log.PrintNet(LogType.Error, LogNetDir.S2P, "Socket Closed By Server");
                    if (_isSuccessful == null)
                        _isSuccessful = false;
                    else if (GetSession().WorldClient == this)
                        GetSession().OnDisconnect();
                    return;
                }

                if (received != LegacyServerPacketHeader.StructSize)
                {
                    Log.PrintNet(LogType.Error, LogNetDir.S2P, $"Received {received} bytes when reading header!");
                    if (_isSuccessful == null)
                        _isSuccessful = false;
                    else
                        GetSession().OnDisconnect();
                    return;
                }

                byte[] headerBuffer = (byte[])AR.AsyncState;

                if (_worldCrypt != null)
                    _worldCrypt.Decrypt(headerBuffer, LegacyServerPacketHeader.StructSize);

                LegacyServerPacketHeader header = new LegacyServerPacketHeader();
                header.Read(headerBuffer);
                ushort packetSize = header.Size;

                if (packetSize != 0)
                {
                    if (packetSize > _clientSocket.ReceiveBufferSize)
                    {
                        Log.PrintNet(LogType.Error, LogNetDir.S2P, "Packet size is greater than max buffer size!");
                        return;
                    }

                    byte[] buffer = new byte[packetSize];

                    // copy the opcode into the new buffer
                    buffer[0] = headerBuffer[2];
                    buffer[1] = headerBuffer[3];

                    received = sizeof(ushort); // size includes opcode and we have it already
                    while (received != packetSize)
                    {
                        //Log.Print(LogType.Debug, $"Waiting to receive remaining {packetSize - received} bytes.");

                        int receivedNow = _clientSocket.Receive(buffer, received, packetSize - received, SocketFlags.None);
                        if (receivedNow == 0)
                            return;

                        received += receivedNow;
                    }

                    WorldPacket packet = new WorldPacket(buffer);
                    packet.SetReceiveTime(Environment.TickCount);
                    HandlePacket(packet);
                }
                else
                {
                    Log.PrintNet(LogType.Error, LogNetDir.S2P, "Received an empty packet!");
                }

                if (!IsConnected())
                    return;

                headerBuffer = new byte[LegacyServerPacketHeader.StructSize];

                // Start receiving data again.
                _clientSocket.BeginReceive(headerBuffer, 0, headerBuffer.Length, SocketFlags.None, ReceiveCallback, headerBuffer);

            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.S2P, $"Packet Read Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (_isSuccessful == null)
                    _isSuccessful = false;
                else
                {
                    Disconnect();
                    GetSession().OnDisconnect();
                }
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndSend(AR);
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Packet Send Error: {ex.Message}");
                if (_isSuccessful == null)
                    _isSuccessful = false;
            }
        }

        // C P>S: Sends data to world server
        private void SendPacket(WorldPacket packet)
        {
            _sendMutex.WaitOne();
            try
            {
                ByteBuffer buffer = new ByteBuffer();
                LegacyClientPacketHeader header = new LegacyClientPacketHeader();

                header.Size = (ushort)(packet.GetSize() + sizeof(uint)); // size includes the opcode
                header.Opcode = packet.GetOpcode();
                header.Write(buffer);

                Log.PrintNet(LogType.Debug, LogNetDir.P2S, $"Sending opcode {LegacyVersion.GetUniversalOpcode(header.Opcode)} ({header.Opcode}) with size {header.Size}.");

                byte[] headerArray = buffer.GetData();
                if (_worldCrypt != null)
                    _worldCrypt.Encrypt(headerArray, LegacyClientPacketHeader.StructSize);
                buffer.Clear();
                buffer.WriteBytes(headerArray);

                buffer.WriteBytes(packet.GetData(), packet.GetSize());

                _clientSocket.BeginSend(buffer.GetData(), 0, (int)buffer.GetSize(), SocketFlags.None, SendCallback, null);
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Packet Write Error: {ex.Message}");
                if (_isSuccessful == null)
                    _isSuccessful = false;
            }
            _sendMutex.ReleaseMutex();
        }

        public void SendPacketToClient(ServerPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
        {
            Opcode opcode = packet.GetUniversalOpcode();
            if (delayUntilOpcode != Opcode.MSG_NULL_ACTION)
            {
                if (_delayedPacketsToClient.ContainsKey(delayUntilOpcode))
                    _delayedPacketsToClient[delayUntilOpcode].Add(packet);
                else
                {
                    List<ServerPacket> packets = new List<ServerPacket>();
                    packets.Add(packet);
                    _delayedPacketsToClient.Add(delayUntilOpcode, packets);
                }
                return;
            }

            SendPacketToClientDirect(packet);
            SendDelayedPacketsToClientOnOpcode(opcode);
        }

        private void SendPacketToClientDirect(ServerPacket packet)
        {
            if (packet.GetConnection() == ConnectionType.Realm)
            {
                GetSession().RealmSocket.SendPacket(packet);
            }
            else
            {
                if (GetSession().InstanceSocket == null &&
                   !GetSession().GameState.IsConnectedToInstance)
                {
                    Log.PrintNet(LogType.Error, LogNetDir.P2C, $"Can't send opcode {packet.GetUniversalOpcode()} ({packet.GetOpcode()}) before entering world!");
                    return;
                }

                // block these packets until connected to instance
                while (GetSession().InstanceSocket == null)
                {
                    Log.PrintNet(LogType.Network, LogNetDir.P2C, $"Waiting to send {packet.GetUniversalOpcode()} ({packet.GetOpcode()}).");
                    System.Threading.Thread.Sleep(200);
                };
                GetSession().InstanceSocket.SendPacket(packet);
            }
        }

        public void SendPacketToServer(WorldPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
        {
            Opcode opcode = packet.GetUniversalOpcode(false);
            if (delayUntilOpcode != Opcode.MSG_NULL_ACTION)
            {
                if (_delayedPacketsToServer.ContainsKey(delayUntilOpcode))
                    _delayedPacketsToServer[delayUntilOpcode].Add(packet);
                else
                {
                    List<WorldPacket> packets = new List<WorldPacket>();
                    packets.Add(packet);
                    _delayedPacketsToServer.Add(delayUntilOpcode, packets);
                }
                return;
            }

            SendPacket(packet);
            SendDelayedPacketsToServerOnOpcode(opcode);
        }

        private void SendDelayedPacketsToServerOnOpcode(Opcode opcode)
        {
            if (_delayedPacketsToServer.ContainsKey(opcode))
            {
                List<WorldPacket> packets = _delayedPacketsToServer[opcode];
                for (int i = packets.Count - 1; i >= 0; i--)
                {
                    SendPacket(packets[i]);
                    packets.RemoveAt(i);
                }
            }
        }

        private void SendDelayedPacketsToClientOnOpcode(Opcode opcode)
        {
            if (_delayedPacketsToClient.ContainsKey(opcode))
            {
                List<ServerPacket> packets = _delayedPacketsToClient[opcode];
                for (int i = packets.Count - 1; i >= 0; i--)
                {
                    SendPacketToClientDirect(packets[i]);
                    packets.RemoveAt(i);
                }
            }
        }

        private void HandlePacket(WorldPacket packet)
        {
            Opcode universalOpcode = packet.GetUniversalOpcode(false);
            Log.PrintNet(LogType.Debug, LogNetDir.S2P, $"Received opcode {universalOpcode} ({packet.GetOpcode()}).");

            switch (universalOpcode)
            {
                case Opcode.SMSG_AUTH_CHALLENGE:
                    HandleAuthChallenge(packet);
                    break;
                case Opcode.SMSG_AUTH_RESPONSE:
                    HandleAuthResponse(packet);
                    break;
                case Opcode.SMSG_PONG:
                case Opcode.SMSG_ADDON_INFO:
                    break; // don't need to handle
                default:
                    if (_packetHandlers.ContainsKey(universalOpcode))
                    {
                        _packetHandlers[universalOpcode](packet);
                    }
                    else
                    {
                        Log.Print(LogType.Warn, $"No handler for opcode {universalOpcode} ({packet.GetOpcode()})");
                        if (_isSuccessful == null)
                            _isSuccessful = false;
                    }
                    break;
            }

            SendDelayedPacketsToServerOnOpcode(universalOpcode);
        }

        private void HandleAuthChallenge(WorldPacket packet)
        {
            if (Settings.ServerBuild >= ClientVersionBuild.V3_3_5a_12340)
            {
                uint one = packet.ReadUInt32();
            }

            uint seed = packet.ReadUInt32();

            if (Settings.ServerBuild >= ClientVersionBuild.V3_3_5a_12340)
            {
                BigInteger seed1 = packet.ReadBytes(16).ToBigInteger();
                BigInteger seed2 = packet.ReadBytes(16).ToBigInteger();
            }

            var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rand.GetBytes(bytes);
            BigInteger ourSeed = bytes.ToBigInteger();

            SendAuthResponse((uint)ourSeed, seed);
        }

        public void SendAuthResponse(uint clientSeed, uint serverSeed)
        {
            uint zero = 0;

            byte[] authResponse = HashAlgorithm.SHA1.Hash
            (
                Encoding.ASCII.GetBytes(_username.ToUpper()),
                BitConverter.GetBytes(zero),
                BitConverter.GetBytes(clientSeed),
                BitConverter.GetBytes(serverSeed),
                GetSession().AuthClient.GetSessionKey()
            );

            WorldPacket packet = new WorldPacket(Opcode.CMSG_AUTH_SESSION);
            packet.WriteUInt32((uint)Settings.ServerBuild);
            packet.WriteUInt32(_realm.Id.Index);
            packet.WriteBytes(_username.ToUpper().ToCString());

            if (Settings.ServerBuild >= ClientVersionBuild.V3_0_2_9056)
                packet.WriteUInt32(zero); // LoginServerType

            packet.WriteUInt32(clientSeed);

            if (Settings.ServerBuild >= ClientVersionBuild.V3_3_5a_12340)
            {
                packet.WriteUInt32(_realm.Id.Region);
                packet.WriteUInt32(_realm.Id.Site);
                packet.WriteUInt32(_realm.Id.Index);
            }

            if (Settings.ServerBuild >= ClientVersionBuild.V3_2_0_10192)
                packet.WriteUInt64(zero); // DosResponse

            packet.WriteBytes(authResponse);

            // packet.WriteUInt32(zero); // length of addon data
            byte[] addonBytes = new byte[] { 208, 1, 0, 0, 120, 156, 117, 207, 61, 14, 194, 48, 12, 5, 224, 114, 14, 184, 12, 97, 64, 149, 154, 133, 150, 25, 153, 196, 173, 172, 38, 78, 21, 82, 126, 58, 113, 66, 206, 68, 81, 133, 24, 98, 188, 126, 126, 79, 182, 114, 52, 77, 16, 237, 105, 59, 154, 68, 129, 143, 101, 177, 242, 183, 77, 85, 204, 163, 190, 166, 32, 37, 135, 45, 161, 179, 154, 152, 60, 12, 210, 18, 177, 37, 238, 230, 130, 87, 102, 187, 224, 207, 144, 170, 208, 9, 185, 197, 26, 188, 39, 9, 35, 180, 73, 188, 105, 175, 235, 49, 94, 241, 33, 227, 72, 206, 42, 224, 94, 212, 146, 47, 3, 154, 79, 237, 58, 183, 132, 190, 14, 166, 199, 180, 252, 146, 167, 53, 152, 24, 102, 121, 102, 114, 0, 178, 51, 196, 12, 26, 112, 200, 242, 27, 77, 4, 139, 117, 79, 206, 253, 99, 98, 140, 178, 145, 71, 13, 12, 29, 198, 159, 190, 1, 43, 0, 141, 195 };
            packet.WriteBytes(addonBytes);

            SendPacket(packet);

            InitializeEncryption(GetSession().AuthClient.GetSessionKey());
        }

        private void HandleAuthResponse(WorldPacket packet)
        {
            AuthResult result = (AuthResult)packet.ReadUInt8();

            uint billingTimeRemaining = packet.ReadUInt32();
            byte billingFlags = packet.ReadUInt8();
            uint billingTimeRested = packet.ReadUInt32();

            if (Settings.ServerBuild >= ClientVersionBuild.V2_0_1_6180)
            {
                byte expansion = packet.ReadUInt8();
            }

            // uncomment to test encryption
            //WorldPacket charEnum = new WorldPacket(Opcode.CMSG_ENUM_CHARACTERS);
            //SendPacket(charEnum);

            if (result == AuthResult.AUTH_OK)
            {
                Log.Print(LogType.Network, "Authentication succeeded!");
                _isSuccessful = true;
            }
            else
            {
                Log.Print(LogType.Network, "Authentication failed!");
                _isSuccessful = false;
            }
        }

        public void SendPing(uint ping, uint latency)
        {
            if (!IsConnected() || _isSuccessful == false)
                return;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_PING);
            packet.WriteUInt32(ping);
            packet.WriteUInt32(latency);
            SendPacket(packet);
        }

        public void InitializePacketHandlers()
        {
            _packetHandlers = new();

            foreach (var methodInfo in typeof(WorldClient).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                foreach (var msgAttr in methodInfo.GetCustomAttributes<PacketHandlerAttribute>())
                {
                    if (msgAttr == null)
                        continue;

                    if (msgAttr.Opcode == Opcode.MSG_NULL_ACTION)
                        continue;

                    if (_packetHandlers.ContainsKey(msgAttr.Opcode))
                    {
                        Log.Print(LogType.Error, $"Tried to override OpcodeHandler of {_packetHandlers[msgAttr.Opcode]} with {methodInfo.Name} (Opcode {msgAttr.Opcode})");
                        continue;
                    }

                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length == 0)
                    {
                        Log.Print(LogType.Error, $"Method: {methodInfo.Name} Has no parameters");
                        continue;
                    }

                    if (parameters[0].ParameterType != typeof(WorldPacket))
                    {
                        Log.Print(LogType.Error, $"Method: {methodInfo.Name} has wrong BaseType");
                        continue;
                    }

                    var del = (Action<WorldPacket>)Delegate.CreateDelegate(typeof(Action<WorldPacket>), this, methodInfo);

                    _packetHandlers[msgAttr.Opcode] = del;
                }
            }
        }
    }
}
