using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace HermesProxy.Crypto
{
    public enum HashAlgorithm
    {
        SHA1,
    }

    public static class HashHelper
    {
        private delegate byte[] HashFunction(params byte[][] data);

        static Dictionary<HashAlgorithm, HashFunction> _hashFunctions;

        static HashHelper()
        {
            _hashFunctions = new Dictionary<HashAlgorithm, HashFunction>
            {
                [HashAlgorithm.SHA1] = SHA1Func
            };
        }

        /// <summary>
        /// Hash based on <see cref="HashAlgorithm"/> and provided <see cref="byte[][]"/> data.
        /// </summary>
        public static byte[] Hash(this HashAlgorithm algorithm, params byte[][] data) 
            => _hashFunctions[algorithm](data);

        static byte[] SHA1Func(params byte[][] data)
        {
            using (SHA1 alg = SHA1.Create())
            {
                return alg.ComputeHash(Combine(data));
            }
        }

        static byte[] Combine(byte[][] buffers)
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
    }
}
