using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using HermesProxy.Enums;
using System.Numerics;
using System.Threading.Tasks;
using Framework.Constants;
using Framework.Cryptography;
using Framework;
using Framework.IO;
using Framework.Logging;

namespace HermesProxy.Auth
{
    public partial class AuthClient
    {
        // For ez debugging: Call this function wherever you want
        private static readonly Action<ByteBuffer> _debugTraceBreakpointHandler = (b) =>
        {
#if DEBUG
            // Debugger.Log(0, "TraceMe", $"{b}");
#endif
        };
        readonly GlobalSessionData _globalSession;
        Socket _clientSocket;
        TaskCompletionSource<AuthResult> _response;
        TaskCompletionSource _hasRealmlist;
        byte[] _passwordHash;
        BigInteger _key;
        byte[] _m2;
        string _username;
        string _locale;

        public AuthClient(GlobalSessionData globalSession)
        {
            _globalSession = globalSession;
        }

        public GlobalSessionData GetSession()
        {
            return _globalSession;
        }

        public AuthResult ConnectToAuthServer(string username, string password, string locale)
        {
            _username = username;
            _locale = locale;

            _response = new ();
            _hasRealmlist = new();

            string authstring = $"{_username}:{password}";
            _passwordHash = HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(authstring.ToUpper()));

            try
            {
                Log.PrintNet(LogType.Network, LogNetDir.P2S, $"Connecting to auth server... (realmlist addr: {Settings.ServerAddress}:{Settings.ServerPort})");
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the specified host.
                var endPoint = new IPEndPoint(IPAddress.Parse(Settings.ServerAddress), Settings.ServerPort);
                _clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Socket Error: {ex.Message}");
                _response.SetResult(AuthResult.FAIL_INTERNAL_ERROR);
            }

            _response.Task.Wait();

            return _response.Task.Result;
        }

        public AuthResult Reconnect()
        {
            _response = new ();
            _hasRealmlist = new();

            try
            {
                Log.PrintNet(LogType.Network, LogNetDir.P2S, $"Re-Connecting to auth server... (realmlist addr: {Settings.ServerAddress}:{Settings.ServerPort})");
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the specified host.
                var endPoint = new IPEndPoint(IPAddress.Parse(Settings.ServerAddress), Settings.ServerPort);
                _clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Socket Error: {ex.Message}");
                _response.SetResult(AuthResult.FAIL_INTERNAL_ERROR);
            }

            _response.Task.Wait();

            return _response.Task.Result;
        }

        private void SetAuthResponse(AuthResult response)
        {
            _response.TrySetResult(response);
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

        public byte[] GetSessionKey()
        {
            return _key.ToCleanByteArray();
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndConnect(AR);
                _clientSocket.ReceiveBufferSize = 65535;
                byte[] buffer = new byte[_clientSocket.ReceiveBufferSize];
                _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
                SendLogonChallenge();
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Connect Error: {ex.Message}");
                SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = _clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);

                    Log.PrintNet(LogType.Error, LogNetDir.S2P, "Socket Closed By Server");
                    return;
                }

                byte[] oldBuffer = (byte[])AR.AsyncState;

                HandlePacket(oldBuffer, received);

                byte[] newBuffer = new byte[_clientSocket.ReceiveBufferSize];

                // Start receiving data again.
                _clientSocket.BeginReceive(newBuffer, 0, newBuffer.Length, SocketFlags.None, ReceiveCallback, newBuffer);


            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, $"Packet Read Error: {ex.Message}");
                SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
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
                SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
            }
        }

        // C P>R: Sends data to realm server
        private void SendPacket(ByteBuffer packet)
        {
            try
            {
                _clientSocket.BeginSend(packet.GetData(), 0, (int)packet.GetSize(), SocketFlags.None, SendCallback, null);
            }
            catch (Exception ex)
            {
                Log.PrintNet(LogType.Error, LogNetDir.P2S, $"Packet Write Error: {ex.Message}");
                SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
            }
        }

        private void HandlePacket(byte[] buffer, int size)
        {
            ByteBuffer packet = new ByteBuffer(buffer);
            AuthCommand opcode = (AuthCommand)packet.ReadUInt8();
            Log.PrintNet(LogType.Debug, LogNetDir.S2P, $"Received opcode {opcode} size {size}.");

            switch (opcode)
            {
                case AuthCommand.LOGON_CHALLENGE:
                    HandleLogonChallenge(packet);
                    break;
                case AuthCommand.LOGON_PROOF:
                    HandleLogonProof(packet);
                    break;
                case AuthCommand.REALM_LIST:
                    HandleRealmList(packet);
                    break;
                default:
                    Log.PrintNet(LogType.Error, LogNetDir.S2P, $"No handler for opcode {opcode}!");
                    SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
                    break;
            }
        }

        private void SendLogonChallenge()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt8((byte)AuthCommand.LOGON_CHALLENGE);
            buffer.WriteUInt8((byte)(LegacyVersion.ExpansionVersion > 1 ? 8 : 3));
            buffer.WriteUInt16((ushort)(_username.Length + 30));
            buffer.WriteBytes(Encoding.ASCII.GetBytes("WoW"));
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(LegacyVersion.ExpansionVersion);
            buffer.WriteUInt8(LegacyVersion.MajorVersion);
            buffer.WriteUInt8(LegacyVersion.MinorVersion);
            buffer.WriteUInt16((ushort)Settings.ServerBuild);
            buffer.WriteBytes(Encoding.ASCII.GetBytes(Settings.ReportedPlatform.Reverse()));
            buffer.WriteUInt8(0);
            buffer.WriteBytes(Encoding.ASCII.GetBytes(Settings.ReportedOS.Reverse()));
            buffer.WriteUInt8(0);
            buffer.WriteBytes(Encoding.ASCII.GetBytes(_locale.Reverse()));
            buffer.WriteUInt32((uint)0x3c);
            buffer.WriteUInt32(16777343); // IP (127.0.0.1)
            buffer.WriteUInt8((byte)_username.Length);
            buffer.WriteBytes(Encoding.ASCII.GetBytes(_username.ToUpper()));
            SendPacket(buffer);
        }

        private void HandleLogonChallenge(ByteBuffer packet)
        {
            byte unk2 = packet.ReadUInt8();
            AuthResult error = (AuthResult)packet.ReadUInt8();
            if (error != AuthResult.SUCCESS)
            {
                Log.Print(LogType.Error, $"Login failed. Reason: {error}");
                SetAuthResponse(error);
                return;
            }

            byte[] challenge_B = packet.ReadBytes(32);
            byte challenge_gLen = packet.ReadUInt8();
            byte[] challenge_g = packet.ReadBytes(1);
            byte challenge_nLen = packet.ReadUInt8();
            byte[] challenge_N = packet.ReadBytes(32);
            byte[] challenge_salt = packet.ReadBytes(32);
            byte[] challenge_version = packet.ReadBytes(16);
            byte challenge_securityFlags = packet.ReadUInt8();

            //Console.WriteLine("Received logon challenge");

            BigInteger N, A, B, a, u, x, S, salt, versionChallenge, g, k;
            k = new BigInteger(3);

            #region Receive and initialize

            B = challenge_B.ToBigInteger();            // server public key
            g = challenge_g.ToBigInteger();
            N = challenge_N.ToBigInteger();            // modulus
            salt = challenge_salt.ToBigInteger();
            versionChallenge = challenge_version.ToBigInteger();

            //Console.WriteLine("---====== Received from server: ======---");
            //Console.WriteLine($"B={B.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"N={N.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"salt={challenge_salt.ToHexString()}");
            //Console.WriteLine($"versionChallenge={challenge_version.ToHexString()}");
            #endregion

            #region Hash password

            x = HashAlgorithm.SHA1.Hash(challenge_salt, _passwordHash).ToBigInteger();

            //Console.WriteLine("---====== shared password hash ======---");
            //Console.WriteLine($"g={g.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"x={x.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"N={N.ToCleanByteArray().ToHexString()}");

            #endregion

            #region Create random key pair

            var rand = System.Security.Cryptography.RandomNumberGenerator.Create();

            do
            {
                byte[] randBytes = new byte[19];
                rand.GetBytes(randBytes);
                a = randBytes.ToBigInteger();

                A = g.ModPow(a, N);
            } while (A.ModPow(1, N) == 0);

            //Console.WriteLine("---====== Send data to server: ======---");
            //Console.WriteLine($"A={A.ToCleanByteArray().ToHexString()}");

            #endregion

            #region Compute session key

            u = HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), B.ToCleanByteArray()).ToBigInteger();

            // compute session key
            S = ((B + k * (N - g.ModPow(x, N))) % N).ModPow(a + (u * x), N);
            byte[] keyHash;
            byte[] sData = S.ToCleanByteArray();
            if (sData.Length < 32)
            {
                var tmpBuffer = new byte[32];
                Buffer.BlockCopy(sData, 0, tmpBuffer, 32 - sData.Length, sData.Length);
                sData = tmpBuffer;
            }
            byte[] keyData = new byte[40];
            byte[] temp = new byte[16];

            // take every even indices byte, hash, store in even indices
            for (int i = 0; i < 16; ++i)
                temp[i] = sData[i * 2];
            keyHash = HashAlgorithm.SHA1.Hash(temp);
            for (int i = 0; i < 20; ++i)
                keyData[i * 2] = keyHash[i];

            // do the same for odd indices
            for (int i = 0; i < 16; ++i)
                temp[i] = sData[i * 2 + 1];
            keyHash = HashAlgorithm.SHA1.Hash(temp);
            for (int i = 0; i < 20; ++i)
                keyData[i * 2 + 1] = keyHash[i];

            _key = keyData.ToBigInteger();

            //Console.WriteLine("---====== Compute session key ======---");
            //Console.WriteLine($"u={u.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"S={S.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"K={_key.ToCleanByteArray().ToHexString()}");

            #endregion

            #region Generate crypto proof

            // XOR the hashes of N and g together
            byte[] gNHash = new byte[20];

            byte[] nHash = HashAlgorithm.SHA1.Hash(N.ToCleanByteArray());
            for (int i = 0; i < 20; ++i)
                gNHash[i] = nHash[i];
            //Console.WriteLine($"nHash={nHash.ToHexString()}");

            byte[] gHash = HashAlgorithm.SHA1.Hash(g.ToCleanByteArray());
            for (int i = 0; i < 20; ++i)
                gNHash[i] ^= gHash[i];
            //Console.WriteLine($"gHash={gHash.ToHexString()}");

            // hash username
            byte[] userHash = HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(_username.ToUpper()));

            // our proof
            byte[] m1Hash = HashAlgorithm.SHA1.Hash
            (
                gNHash,
                userHash,
                challenge_salt,
                A.ToCleanByteArray(),
                B.ToCleanByteArray(),
                _key.ToCleanByteArray()
            );

            //Console.WriteLine("---====== Client proof: ======---");
            //Console.WriteLine($"gNHash={gNHash.ToHexString()}");
            //Console.WriteLine($"userHash={userHash.ToHexString()}");
            //Console.WriteLine($"salt={challenge_salt.ToHexString()}");
            //Console.WriteLine($"A={A.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"B={B.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"key={_key.ToCleanByteArray().ToHexString()}");

            //Console.WriteLine("---====== Send proof to server: ======---");
            //Console.WriteLine($"M={m1Hash.ToHexString()}");

            // expected proof for server
            _m2 = HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), m1Hash, keyData);

            #endregion

            SendLogonProof(A.ToCleanByteArray(), m1Hash, new byte[20]);
        }

        private void SendLogonProof(byte[] A, byte[] M1, byte[] crc)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt8((byte)AuthCommand.LOGON_PROOF);
            buffer.WriteBytes(A);
            buffer.WriteBytes(M1);
            buffer.WriteBytes(crc);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);

            _debugTraceBreakpointHandler(buffer);

            SendPacket(buffer);
        }

        private void HandleLogonProof(ByteBuffer packet)
        {
            AuthResult error = (AuthResult)packet.ReadUInt8();
            if (error != AuthResult.SUCCESS)
            {
                Log.Print(LogType.Error, $"Login failed. Reason: {error}");
                SetAuthResponse(error);
                return;
            }

            byte[] M2 = packet.ReadBytes(20);
            uint accountFlags = 0;
            uint surveyId = 0;
            ushort loginFlags = 0;

            if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
            {
                surveyId = packet.ReadUInt32();
            }
            else if (Settings.ServerBuild < ClientVersionBuild.V2_4_0_8089)
            {
                surveyId = packet.ReadUInt32();
                loginFlags = packet.ReadUInt16();
            }
            else
            {
                accountFlags = packet.ReadUInt32();
                surveyId = packet.ReadUInt32();
                loginFlags = packet.ReadUInt16();
            }

            bool equal = _m2 != null && _m2.Length == 20;
            for (int i = 0; equal && i < _m2.Length; ++i)
                if (!(equal = _m2[i] == M2[i]))
                    break;

            if (!equal)
            {
                Log.Print(LogType.Error, "Authentication failed!");
                SetAuthResponse(AuthResult.FAIL_INTERNAL_ERROR);
            }
            else
            {
                Log.Print(LogType.Network, "Authentication succeeded!");
                SetAuthResponse(AuthResult.SUCCESS);
            }
        }

        public void RequestRealmListUpdate()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt8((byte)AuthCommand.REALM_LIST);
            for (int i = 0; i < 4; i++)
                buffer.WriteUInt8(0);
            SendPacket(buffer);
        }

        private void HandleRealmList(ByteBuffer packet)
        {
            packet.ReadUInt16(); // packet size
            packet.ReadUInt32(); // unused
            ushort realmsCount = 0;

            if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
            {
                realmsCount = packet.ReadUInt8();
            }
            else
            {
                realmsCount = packet.ReadUInt16();
            }

            Log.Print(LogType.Network, $"Received {realmsCount} realms.");
            List<RealmInfo> realmList = new List<RealmInfo>();

            for (ushort i = 0; i < realmsCount; i++)
            {
                RealmInfo realmInfo = new RealmInfo();
                realmInfo.ID = i;

                if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
                {
                    realmInfo.Type = (RealmType)packet.ReadUInt32();
                }
                else
                {
                    realmInfo.Type = (RealmType)packet.ReadUInt8();
                    realmInfo.IsLocked = packet.ReadUInt8();
                }

                realmInfo.Flags = (RealmFlags)packet.ReadUInt8();
                realmInfo.Name = packet.ReadCString();
                string addressAndPort = packet.ReadCString();
                string[] strArr = addressAndPort.Split(':');
                realmInfo.Address = Dns.GetHostAddresses(strArr[0]).First().ToString();
                realmInfo.Port = ushort.Parse(strArr[1]);
                realmInfo.Population = packet.ReadFloat();
                realmInfo.CharacterCount = packet.ReadUInt8();
                realmInfo.Timezone = packet.ReadUInt8();
                packet.ReadUInt8(); // unk

                if ((realmInfo.Flags & RealmFlags.SpecifyBuild) != 0)
                {
                    realmInfo.VersionMajor = packet.ReadUInt8();
                    realmInfo.VersionMinor = packet.ReadUInt8();
                    realmInfo.VersonBugfix = packet.ReadUInt8();
                    realmInfo.Build = packet.ReadUInt16();
                }
                realmList.Add(realmInfo);
            }

            GetSession().RealmManager.UpdateRealms(realmList);
            _hasRealmlist.SetResult();
        }

        public void WaitForRealmlist()
        {
            _hasRealmlist.Task.Wait();
        }
    }
}
