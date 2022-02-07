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
using World;
using Framework.Constants.World;
using System.Reflection;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        Socket _clientSocket;
        bool? _isSuccessful;
        string _username;
        Realm _realm;
        LegacyWorldCrypt _worldCrypt;
        Dictionary<Opcode, Action<WorldPacket, List<ServerPacket>>> _packetHandlers;
        WorldSocket _modernSocket;

        public WorldClient()
        {
            InitializePacketHandlers();
        }

        public bool ConnectToWorldServer(Realm realm, WorldSocket modernSocket)
        {
            _worldCrypt = null;
            _realm = realm;
            _modernSocket = modernSocket;
            _username = Global.CurrentSessionData.Username;
            _isSuccessful = null;

            try
            {
                Log.Print(LogType.Server, "Connecting to world server...");
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
        }

        public bool IsConnected()
        {
            return _clientSocket != null && _clientSocket.Connected;
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                Log.Print(LogType.Debug, "Connection established!");

                _clientSocket.EndConnect(AR);
                _clientSocket.ReceiveBufferSize = 65535;
                byte[] buffer = new byte[LegacyServerPacketHeader.StructSize];
                _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, $"Connect Error: {ex.Message}");
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
                    Log.Print(LogType.Error, "Socket Closed By Server");
                    _isSuccessful = false;
                    return;
                }

                if (received != LegacyServerPacketHeader.StructSize)
                {
                    Log.Print(LogType.Error, $"Received {received} bytes when reading header!");
                    _isSuccessful = false;
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
                        Log.Print(LogType.Error, $"Packet size is greater than max buffer size!");
                        return;
                    }

                    byte[] buffer = new byte[packetSize];

                    // copy the opcode into the new buffer
                    buffer[0] = headerBuffer[2];
                    buffer[1] = headerBuffer[3];

                    received = sizeof(ushort); // size includes opcode and we have it already
                    while (received != packetSize)
                    {
                        Log.Print(LogType.Debug, $"Waiting to receive remaining {packetSize - received} bytes.");

                        int receivedNow = _clientSocket.Receive(buffer, received, packetSize - received, SocketFlags.None);
                        if (receivedNow == 0)
                            return;

                        received += receivedNow;
                    }

                    HandlePacket(buffer, header.Opcode, header.Size);
                }
                else
                {
                    Log.Print(LogType.Error, $"Received an empty packet!");
                }

                headerBuffer = new byte[LegacyServerPacketHeader.StructSize];

                // Start receiving data again.
                _clientSocket.BeginReceive(headerBuffer, 0, headerBuffer.Length, SocketFlags.None, ReceiveCallback, headerBuffer);

            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, $"Packet Read Error: {ex.Message}");
                _isSuccessful = false;
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
                Log.Print(LogType.Error, $"Packet Send Error: {ex.Message}");
                _isSuccessful = false;
            }
        }

        public void SendPacket(WorldPacket packet)
        {
            try
            {
                ByteBuffer buffer = new ByteBuffer();
                LegacyClientPacketHeader header = new LegacyClientPacketHeader();

                // Endian Reverse
                header.Size = (ushort)(packet.GetSize() + sizeof(uint)); // size includes the opcode
                header.Opcode = packet.GetOpcode();
                header.Write(buffer);

                Log.Print(LogType.Debug, $"Sending opcode {Opcodes.GetOpcodeNameForVersion(header.Opcode, Settings.ServerBuild)} ({header.Opcode}) with size {header.Size}.");

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
                Log.Print(LogType.Error, $"Packet Write Error: {ex.Message}");
                _isSuccessful = false;
            }
        }

        private void HandlePacket(byte[] buffer, ushort opcode, ushort size)
        {
            WorldPacket packet = new WorldPacket(buffer);
            System.Diagnostics.Trace.Assert(opcode == packet.GetOpcode());

            Opcode universalOpcode = packet.GetUniversalOpcode(false);
            Log.Print(LogType.Debug, $"Received opcode {universalOpcode.ToString()} ({opcode}).");

            switch (universalOpcode)
            {
                case Opcode.SMSG_AUTH_CHALLENGE:
                    HandleAuthChallenge(packet);
                    break;
                case Opcode.SMSG_AUTH_RESPONSE:
                    HandleAuthResponse(packet);
                    break;
                case Opcode.SMSG_PONG:
                    break; // don't need to handle
                default:
                    if (_packetHandlers.ContainsKey(universalOpcode))
                    {
                        List<ServerPacket> packetsForModernGame = new List<ServerPacket>();
                        _packetHandlers[universalOpcode](packet, packetsForModernGame);
                        foreach (ServerPacket pkt in packetsForModernGame)
                            _modernSocket.SendPacket(pkt);
                    }
                    else
                    {
                        Log.Print(LogType.Error, "Unsupported opcode!");
                        if (_isSuccessful == null)
                            _isSuccessful = false;
                    }
                    break;
            }
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
                Auth.AuthClient.GetSessionKey()
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
            packet.WriteUInt32(zero); // length of addon data

            SendPacket(packet);

            InitializeEncryption(Auth.AuthClient.GetSessionKey());
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
                Log.Print(LogType.Server, "Authentication succeeded!");
                _isSuccessful = true;
            }
            else
            {
                Log.Print(LogType.Server, "Authentication failed!");
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
                        Log.Print(LogType.Error, $"Tried to override OpcodeHandler of {_packetHandlers[msgAttr.Opcode].ToString()} with {methodInfo.Name} (Opcode {msgAttr.Opcode})");
                        continue;
                    }

                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length == 0)
                    {
                        Log.Print(LogType.Error, $"Method: {methodInfo.Name} Has no paramters");
                        continue;
                    }

                    if (parameters[0].ParameterType != typeof(WorldPacket))
                    {
                        Log.Print(LogType.Error, $"Method: {methodInfo.Name} has wrong BaseType");
                        continue;
                    }

                    var del = (Action<WorldPacket, List<ServerPacket>>)Delegate.CreateDelegate(typeof(Action<WorldPacket, List<ServerPacket>>), this, methodInfo);

                    _packetHandlers[msgAttr.Opcode] = del;
                }
            }
        }
    }
}
