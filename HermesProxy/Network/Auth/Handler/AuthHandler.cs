using System;
using System.Numerics;
using System.Text;
using SystemCrypto = System.Security.Cryptography;

using HermesProxy.Crypto;
using HermesProxy.Framework.Constants;
using HermesProxy.Framework.IO.Packet;
using HermesProxy.Enums;
using HermesProxy.Network.Realm;
using HermesProxy.Framework.Util;
using HermesProxy.Framework.Logging;

namespace HermesProxy.Network.Auth.Handler
{
    public static class AuthHandler
    {
        /// <summary>
        /// Handles the <see cref="AuthCommand.LOGON_CHALLENGE"/> packet.
        /// </summary>
        public static void HandleLogonChallenge(AuthSession session, PacketReader reader)
        {
            _ = reader.ReadUInt8();                     //< Unk

            var result = (AuthResult)reader.ReadUInt8();
            if (result != AuthResult.SUCCESS)
            {
                Log.Print(LogType.Error, $"Login Failed. Reason: {result}");
                session.RequestDisconnect = true;
                return;
            }

            var challengeServerPublicKey = reader.ReadBytes(32);

            var challengeGLen = reader.ReadUInt8();
            var challengeG = reader.ReadBytes(challengeGLen);

            var challengeNLen = reader.ReadUInt8();
            var challengeModulus = reader.ReadBytes(challengeNLen);

            var challengeSalt = reader.ReadBytes(32);

            _ = reader.ReadBytes(16);                   //< VersionChallenge
            _ = reader.ReadUInt8();                     //< SecurityFlags

            BigInteger modulus, A, serverPublicKey, a, u, x, S, g, k;
            k = new BigInteger(3);

            #region Receive and initialize

            serverPublicKey = challengeServerPublicKey.ToBigInteger();  // server public key
            g               = challengeG.ToBigInteger();
            modulus         = challengeModulus.ToBigInteger();          // modulus

            #endregion

            #region Hash password

            x = HashAlgorithm.SHA1.Hash(challengeSalt, session.PasswordHash).ToBigInteger();

            #endregion

            #region Create random key pair

            var rand = SystemCrypto.RandomNumberGenerator.Create();

            do
            {
                var randBytes = new byte[19];
                rand.GetBytes(randBytes);
                a = randBytes.ToBigInteger();

                A = g.ModPow(a, modulus);
            }
            while (A.ModPow(1, modulus) == 0);

            #endregion

            #region Compute session key

            u = HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), serverPublicKey.ToCleanByteArray()).ToBigInteger();

            // compute session key
            S = ((serverPublicKey + k * (modulus - g.ModPow(x, modulus))) % modulus).ModPow(a + (u * x), modulus);

            var sData = S.ToCleanByteArray();
            if (sData.Length < 32)
            {
                var tmpBuffer = new byte[32];
                Buffer.BlockCopy(sData, 0, tmpBuffer, 32 - sData.Length, sData.Length);
                sData = tmpBuffer;
            }

            var keyData = new byte[40];
            var temp = new byte[16];

            // take every even indices byte, hash, store in even indices
            for (int i = 0; i < 16; ++i)
                temp[i] = sData[i * 2];
            var keyHash = HashAlgorithm.SHA1.Hash(temp);
            for (int i = 0; i < 20; ++i)
                keyData[i * 2] = keyHash[i];

            // do the same for odd indices
            for (int i = 0; i < 16; ++i)
                temp[i] = sData[i * 2 + 1];
            keyHash = HashAlgorithm.SHA1.Hash(temp);
            for (int i = 0; i < 20; ++i)
                keyData[i * 2 + 1] = keyHash[i];

            session.Key = keyData.ToBigInteger();

            #endregion

            #region Generate crypto proof

            // XOR the hashes of N and g together
            var gNHash = new byte[20];

            var nHash = HashAlgorithm.SHA1.Hash(modulus.ToCleanByteArray());
            for (int i = 0; i < 20; ++i)
                gNHash[i] = nHash[i];

            var gHash = HashAlgorithm.SHA1.Hash(g.ToCleanByteArray());
            for (int i = 0; i < 20; ++i)
                gNHash[i] ^= gHash[i];

            // hash username
            var userHash = HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(session.Username.ToUpper()));

            // our proof
            var m1Hash = HashAlgorithm.SHA1.Hash
            (
                gNHash,
                userHash,
                challengeSalt,
                A.ToCleanByteArray(),
                serverPublicKey.ToCleanByteArray(),
                session.Key.ToCleanByteArray()
            );

            // expected proof for server
            session.Modulus2 = HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), m1Hash, keyData);

            #endregion

            SendLogonProof(session, A.ToCleanByteArray(), m1Hash, new byte[20]);
        }

        private static void SendLogonProof(AuthSession session, byte[] a, byte[] m1Hash, byte[] crc)
        {
            using (var writer = new PacketWriter())
            {
                writer.WriteUInt8((byte)AuthCommand.LOGON_PROOF);
                writer.WriteBytes(a);
                writer.WriteBytes(m1Hash);
                writer.WriteBytes(crc);
                writer.WriteUInt8(0);
                writer.WriteUInt8(0);

                session.SendPacket(writer.GetData());
            }
        }

        /// <summary>
        /// Handles the <see cref="AuthCommand.LOGON_PROOF"/> packet.
        /// </summary>
        public static void HandleLogonProof(AuthSession session, PacketReader reader)
        {
            var result = (AuthResult)reader.ReadUInt8();
            if (result != AuthResult.SUCCESS)
            {
                Log.Print(LogType.Error, $"Login Failed. Reason: {result}");
                session.RequestDisconnect = true;
                return;
            }

            var modulus2 = reader.ReadBytes(20);

            if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
                _ = reader.ReadUInt32();        //< SurveyID
            else if (Settings.ServerBuild < ClientVersionBuild.V2_4_0_8089)
            {
                _ = reader.ReadUInt32();        //< SurveyID
                _ = reader.ReadUInt16();        //< LoginFlags
            }
            else
            {
                _ = reader.ReadUInt32();        //< AccountFlags
                _ = reader.ReadUInt32();        //< SurveyID
                _ = reader.ReadUInt16();        //< LoginFlags
            }

            var equal = session.Modulus2 != null && session.Modulus2.Length == 20;
            for (var i = 0; equal && i < session.Modulus2.Length; ++i)
                if (!(equal = session.Modulus2[i] == modulus2[i]))
                    break;

            if (!equal)
            {
                Log.Print(LogType.Error, "Server Auth Failed!");
                session.RequestDisconnect = true;
                session.HasSucceededLogin = false;

                return;
            }

            Log.Print(LogType.Debug, "Server Auth Succeeded!");
            session.HasSucceededLogin = true;

            using (var writer = new PacketWriter())
            {
                writer.WriteUInt8((byte)AuthCommand.REALM_LIST);
                for (var i = 0; i < 4; ++i)
                    writer.WriteUInt8(0);
                session.SendPacket(writer.GetData());
            }
        }

        /// <summary>
        /// Handles the <see cref="AuthCommand.REALM_LIST"/> packet.
        /// </summary>
        public static void HandleRealmlist(AuthSession session, PacketReader reader)
        {
            _ = reader.ReadUInt16();        //< Packet Size
            _ = reader.ReadUInt32();        //< Unused

            var realmsCount = 0;
            if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
                realmsCount = reader.ReadUInt8();
            else
                realmsCount = reader.ReadUInt16();

            for (var i = 0; i < realmsCount; ++i)
            {
                var realm = new RealmInfo
                {
                    ID = i + 1
                };

                if (Settings.ServerBuild < ClientVersionBuild.V2_0_3_6299)
                    realm.Type          = (byte)reader.ReadUInt32();
                else
                {
                    realm.Type          = reader.ReadUInt8();
                    realm.IsLocked      = reader.ReadUInt8();
                }

                realm.Flags             = (RealmFlags)reader.ReadUInt8();
                realm.Name              = reader.ReadCString();

                var addressAndPort      = reader.ReadCString();
                var strArr              = addressAndPort.Split(':');
                realm.Address           = strArr[0];

                realm.Port              = int.Parse(strArr[1]);
                realm.Population        = reader.ReadFloat();
                realm.CharacterCount    = reader.ReadUInt8();
                realm.Timezone          = reader.ReadUInt8();

                _ = reader.ReadUInt8(); // unk

                if (realm.Flags.HasFlag(RealmFlags.SpecifyBuild))
                {
                    realm.VersionMajor  = reader.ReadUInt8();
                    realm.VersionMinor  = reader.ReadUInt8();
                    realm.VersonBugfix  = reader.ReadUInt8();
                    realm.Build         = reader.ReadUInt16();
                }

                RealmManager.AddRealm(realm);
            }

            // RealmManager.PrintRealmList();
        }
    }
}
