using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.Crypto
{
    using CryptoNS = System.Security.Cryptography;
    using HashAlgo = System.Security.Cryptography.HashAlgorithm;

    enum HashAlgorithm
    {
        SHA1,
    }

    static class HashHelper
    {
        private delegate byte[] HashFunction(params byte[][] data);

        static Dictionary<HashAlgorithm, HashFunction> HashFunctions;

        static HashHelper()
        {
            HashFunctions = new Dictionary<HashAlgorithm, HashFunction>();

            HashFunctions[HashAlgorithm.SHA1] = SHA1;
        }

        private static byte[] Combine(byte[][] buffers)
        {
            int length = 0;
            foreach (var buffer in buffers)
                length += buffer.Length;

            byte[] result = new byte[length];

            int position = 0;

            foreach (var buffer in buffers)
            {
                Buffer.BlockCopy(buffer, 0, result, position, buffer.Length);
                position += buffer.Length;
            }

            return result;
        }

        public static byte[] Hash(this HashAlgorithm algorithm, params byte[][] data)
        {
            return HashFunctions[algorithm](data);
        }

        private static byte[] SHA1(params byte[][] data)
        {
            using (System.Security.Cryptography.SHA1 alg = CryptoNS.SHA1.Create())
            {
                return alg.ComputeHash(Combine(data));
            }
        }
    }
}
