using Framework.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Client
{
    public interface LegacyWorldCrypt
    {
        public void Initialize(byte[] sessionKey);
        public void Decrypt(byte[] data, int len);
        public void Encrypt(byte[] data, int len);

    }
    public class VanillaWorldCrypt : LegacyWorldCrypt
    {
        public const uint CRYPTED_SEND_LEN = 6;
        public const uint CRYPTED_RECV_LEN = 4;

        public void Initialize(byte[] sessionKey)
        {
            SetKey(sessionKey);
            m_send_i = m_send_j = m_recv_i = m_recv_j = 0;
            m_isInitialized = true;
        }

        public void Decrypt(byte[] data, int len)
        {
            if (len < CRYPTED_RECV_LEN)
                return;

            for (byte t = 0; t < CRYPTED_RECV_LEN; t++)
            {
                m_recv_i %= (byte)m_key.Count();
                byte x = (byte)((data[t] - m_recv_j) ^ m_key[m_recv_i]);
                ++m_recv_i;
                m_recv_j = data[t];
                data[t] = x;
            }
        }

        public void Encrypt(byte[] data, int len)
        {
            if (!m_isInitialized)
                return;

            if (len < CRYPTED_SEND_LEN)
                return;

            for (byte t = 0; t < CRYPTED_SEND_LEN; t++)
            {
                m_send_i %= (byte)m_key.Count();
                byte x = (byte)((data[t] ^ m_key[m_send_i]) + m_send_j);
                ++m_send_i;
                data[t] = m_send_j = x;
            }
        }

        public void SetKey(byte[] key)
        {
            System.Diagnostics.Trace.Assert(key.Length != 0);

            m_key = key.ToArray();
        }

        byte[] m_key;
        byte m_send_i, m_send_j, m_recv_i, m_recv_j;
        bool m_isInitialized;
    }

    public class TbcWorldCrypt : LegacyWorldCrypt
    {
        public const uint CRYPTED_SEND_LEN = 6;
        public const uint CRYPTED_RECV_LEN = 4;

        public void Initialize(byte[] sessionKey)
        {
            byte[] recvSeed = new byte[16] { 0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x4, 0xE2, 0xAA };
            HmacHash recvHash = new HmacHash(recvSeed);
            recvHash.Finish(sessionKey, sessionKey.Count());
            m_key = recvHash.Digest.ToArray();

            m_send_i = m_send_j = m_recv_i = m_recv_j = 0;
            m_isInitialized = true;
        }

        public void Decrypt(byte[] data, int len)
        {
            if (len < CRYPTED_RECV_LEN)
                return;

            for (byte t = 0; t < CRYPTED_RECV_LEN; t++)
            {
                m_recv_i %= (byte)m_key.Count();
                byte x = (byte)((data[t] - m_recv_j) ^ m_key[m_recv_i]);
                ++m_recv_i;
                m_recv_j = data[t];
                data[t] = x;
            }
        }

        public void Encrypt(byte[] data, int len)
        {
            if (!m_isInitialized)
                return;

            if (len < CRYPTED_SEND_LEN)
                return;

            for (byte t = 0; t < CRYPTED_SEND_LEN; t++)
            {
                m_send_i %= (byte)m_key.Count();
                byte x = (byte)((data[t] ^ m_key[m_send_i]) + m_send_j);
                ++m_send_i;
                data[t] = m_send_j = x;
            }
        }

        byte[] m_key;
        byte m_send_i, m_send_j, m_recv_i, m_recv_j;
        bool m_isInitialized;
    }
}
