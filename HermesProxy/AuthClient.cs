using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using HermesProxy.Enums;
using System.Numerics;
using HermesProxy.Crypto;

namespace HermesProxy
{
    public static class AuthClient
    {
        private static Socket clientSocket;
        private static bool? isSuccessful = null;
        private static byte[] PasswordHash;
        private static BigInteger Key { get; set; }
        private static byte[] m2;
        public static List<RealmInfo> RealmList = null;

        public static bool ConnectToAuthServer()
        {
            isSuccessful = null;
            string authstring = $"{Settings.ServerUsername.ToUpper()}:{Settings.ServerPassword}";
            PasswordHash = HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(authstring.ToUpper()));

            try
            {
                Console.WriteLine("[AuthClient] Connecting to auth server...");
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the specified host.
                var endPoint = new IPEndPoint(IPAddress.Parse(Settings.ServerAddress), 3724);
                clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AuthClient] Socket Error:");
                Console.WriteLine(ex.Message);
                isSuccessful = false;
            }

            while (isSuccessful == null)
            { }

            return (bool)isSuccessful;
        }

        private static void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndConnect(AR);
                clientSocket.ReceiveBufferSize = 65535;
                byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
                SendLogonChallenge();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AuthClient] Connect Error:");
                Console.WriteLine(ex.Message);
                isSuccessful = false;
            }
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    if (isSuccessful == null)
                        isSuccessful = false;
                    Console.WriteLine("[AuthClient] Socket Closed By Server");
                    return;
                }

                byte[] oldBuffer = (byte[])AR.AsyncState;

                HandlePacket(oldBuffer, received);

                byte[] newBuffer = new byte[clientSocket.ReceiveBufferSize];

                // Start receiving data again.
                clientSocket.BeginReceive(newBuffer, 0, newBuffer.Length, SocketFlags.None, ReceiveCallback, newBuffer);


            }
            catch (Exception ex)
            {
                Console.WriteLine("[AuthClient] Packet Read Error:");
                Console.WriteLine(ex.Message);
                isSuccessful = false;
            }
        }

        private static void SendCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndSend(AR);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AuthClient] Packet Send Error:");
                Console.WriteLine(ex.Message);
                isSuccessful = false;
            }
        }

        private static void SendPacket(ByteBuffer packet)
        {
            try
            {
                clientSocket.BeginSend(packet.GetData(), 0, (int)packet.GetSize(), SocketFlags.None, SendCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AuthClient] Packet Write Error:");
                Console.WriteLine(ex.Message);
                isSuccessful = false;
            }
        }

        private static void HandlePacket(byte[] buffer, int size)
        {
            ByteBuffer packet = new ByteBuffer(buffer);
            AuthCommand opcode = (AuthCommand)packet.ReadUInt8();
            Console.WriteLine("[AuthClient] Received opcode " + opcode + " size " + size);

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
                    Console.WriteLine("[AuthClient] Unsupported opcode!");
                    isSuccessful = false;
                    break;
            }
        }

        private static void SendLogonChallenge()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt8((byte)AuthCommand.LOGON_CHALLENGE);
            buffer.WriteUInt8(6);
            buffer.WriteUInt16((UInt16)(Settings.ServerUsername.Length + 30));
            buffer.WriteBytes(Encoding.ASCII.GetBytes("WoW"));
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(Settings.GetServerExpansionVersion());
            buffer.WriteUInt8(Settings.GetServerMajorPatchVersion());
            buffer.WriteUInt8(Settings.GetServerMinorPatchVersion());
            buffer.WriteUInt16((ushort)Settings.ServerBuild);
            buffer.WriteBytes(Encoding.ASCII.GetBytes("68x"));
            buffer.WriteUInt8(0);
            buffer.WriteBytes(Encoding.ASCII.GetBytes("niW"));
            buffer.WriteUInt8(0);
            buffer.WriteBytes(Encoding.ASCII.GetBytes("SUne"));
            buffer.WriteUInt32((uint)0x3c);
            buffer.WriteUInt32(0); // IP
            buffer.WriteUInt8((byte)Settings.ServerUsername.Length);
            buffer.WriteBytes(Encoding.ASCII.GetBytes(Settings.ServerUsername.ToUpper()));
            SendPacket(buffer);
        }

        private static void HandleLogonChallenge(ByteBuffer packet)
        {
            byte unk2 = packet.ReadUInt8();
            AuthResult error = (AuthResult)packet.ReadUInt8();
            if (error != AuthResult.SUCCESS)
            {
                Console.WriteLine("[AuthSocket] Login failed. Reason: " + ((uint)error).ToString());
                isSuccessful = false;
                return;
            }

            byte[] challenge_B = packet.ReadBytes(32);
            byte challenge_gLen = packet.ReadUInt8();
            byte[] challenge_g = packet.ReadBytes(1);
            byte challenge_nLen = packet.ReadUInt8();
            byte[] challenge_N = packet.ReadBytes(32);
            byte[] challenge_salt = packet.ReadBytes(32);
            byte[] challenge_unk3 = packet.ReadBytes(16);
            byte challenge_securityFlags = packet.ReadUInt8();

            //Console.WriteLine("Received logon challenge");

            BigInteger N, A, B, a, u, x, S, salt, unk1, g, k;
            k = new BigInteger(3);

            #region Receive and initialize

            B = challenge_B.ToBigInteger();            // server public key
            g = challenge_g.ToBigInteger();
            N = challenge_N.ToBigInteger();            // modulus
            salt = challenge_salt.ToBigInteger();
            unk1 = challenge_unk3.ToBigInteger();

            //Console.WriteLine("---====== Received from server: ======---");
            //Console.WriteLine($"B={B.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"N={N.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"salt={challenge_salt.ToHexString()}");

            #endregion

            #region Hash password

            x = HashAlgorithm.SHA1.Hash(challenge_salt, PasswordHash).ToBigInteger();

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

            Key = keyData.ToBigInteger();

            //Console.WriteLine("---====== Compute session key ======---");
            //Console.WriteLine($"u={u.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"S={S.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"K={Key.ToCleanByteArray().ToHexString()}");

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
            byte[] userHash = HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(Settings.ServerUsername.ToUpper()));

            // our proof
            byte[] m1Hash = HashAlgorithm.SHA1.Hash
            (
                gNHash,
                userHash,
                challenge_salt,
                A.ToCleanByteArray(),
                B.ToCleanByteArray(),
                Key.ToCleanByteArray()
            );

            //Console.WriteLine("---====== Client proof: ======---");
            //Console.WriteLine($"gNHash={gNHash.ToHexString()}");
            //Console.WriteLine($"userHash={userHash.ToHexString()}");
            //Console.WriteLine($"salt={challenge_salt.ToHexString()}");
            //Console.WriteLine($"A={A.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"B={B.ToCleanByteArray().ToHexString()}");
            //Console.WriteLine($"key={Key.ToCleanByteArray().ToHexString()}");

            //Console.WriteLine("---====== Send proof to server: ======---");
            //Console.WriteLine($"M={m1Hash.ToHexString()}");

            // expected proof for server
            m2 = HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), m1Hash, keyData);

            #endregion

            SendLogonProof(A.ToCleanByteArray(), m1Hash, new byte[20]);
        }

        private static void SendLogonProof(byte[] A, byte[] M1, byte[] crc)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt8((byte)AuthCommand.LOGON_PROOF);
            buffer.WriteBytes(A);
            buffer.WriteBytes(M1);
            buffer.WriteBytes(crc);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            SendPacket(buffer);
        }

        private static void HandleLogonProof(ByteBuffer packet)
        {
            AuthResult error = (AuthResult)packet.ReadUInt8();
            if (error != AuthResult.SUCCESS)
            {
                Console.WriteLine("[AuthSocket] Login failed. Reason: " + ((uint)error).ToString());
                isSuccessful = false;
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

            bool equal = m2 != null && m2.Length == 20;
            for (int i = 0; equal && i < m2.Length; ++i)
                if (!(equal = m2[i] == M2[i]))
                    break;

            if (!equal)
            {
                Console.WriteLine("[AuthClient] Server auth failed!");
                isSuccessful = false;
                return;
            }
            else
            {
                Console.WriteLine("[AuthClient] Authentication succeeded!");
                SendRealmListRequest();
            }
        }

        private static void SendRealmListRequest()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt8((byte)AuthCommand.REALM_LIST);
            for (int i = 0; i < 4; i++)
                buffer.WriteUInt8(0);
            SendPacket(buffer);
        }

        private static void HandleRealmList(ByteBuffer packet)
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

            Console.WriteLine("[AuthClient] Realms Count: " + realmsCount);
            RealmList = new List<RealmInfo>();

            for (ushort i = 0; i < realmsCount; i++)
            {
                RealmInfo realmInfo = new RealmInfo();
                realmInfo.ID = i;

                if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
                {
                    realmInfo.Type = (byte)packet.ReadUInt32();
                }
                else
                {
                    realmInfo.Type = packet.ReadUInt8();
                    realmInfo.IsLocked = packet.ReadUInt8();
                }

                realmInfo.Flags = (RealmFlags)packet.ReadUInt8();
                realmInfo.Name = packet.ReadCString();
                string addressAndPort = packet.ReadCString();
                string[] strArr = addressAndPort.Split(':');
                realmInfo.Address = strArr[0];
                realmInfo.Port = Int32.Parse(strArr[1]);
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
                RealmList.Add(realmInfo);
            }

            //PrintRealmList();
            isSuccessful = true;
        }

        public static void PrintRealmList()
        {
            if (RealmList == null || RealmList.Count == 0)
                return;

            Console.WriteLine();
            Console.WriteLine($"{"Type",-5} {"Type",-5} {"Locked",-8} {"Flags",-10} {"Name",-15} {"Address",-15} {"Port",-10} {"Build",-10}");
            foreach (var realm in RealmList)
            {
                Console.WriteLine(realm.ToString());
            }
            Console.WriteLine();
        }
    }
}
