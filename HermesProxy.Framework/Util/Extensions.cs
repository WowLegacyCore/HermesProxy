using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace HermesProxy.Framework.Util
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the remaining bytes on the stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static uint Remaining(this Stream reader)
        {
            if (reader.Position > reader.Length)
                throw new InvalidOperationException();

            return (uint)(reader.Length - reader.Position);
        }

        public static string ToHexString(this byte[] array)
        {
            var builder = new StringBuilder();

            for (var i = array.Length - 1; i >= 0; --i)
                builder.Append(array[i].ToString("X2"));

            return builder.ToString();
        }

        /// <summary>
        /// places a non-negative value (0) at the MSB, then converts to a BigInteger.
        /// This ensures a non-negative value without changing the binary representation.
        /// </summary>
        public static BigInteger ToBigInteger(this byte[] array)
        {
            var temp = Array.Empty<byte>();
            if ((array[^1] & 0x80) == 0x80)
            {
                temp = new byte[array.Length + 1];
                temp[array.Length] = 0;
            }
            else
                temp = new byte[array.Length];

            Array.Copy(array, temp, array.Length);
            return new BigInteger(temp);
        }

        /// <summary>
        /// Removes the MSB if it is 0, then converts to a byte array.
        /// </summary>
        public static byte[] ToCleanByteArray(this BigInteger b)
        {
            var array = b.ToByteArray();
            if (array[^1] != 0)
                return array;

            var temp = new byte[array.Length - 1];
            Array.Copy(array, temp, temp.Length);
            return temp;
        }

        public static BigInteger ModPow(this BigInteger value, BigInteger pow, BigInteger mod)
        {
            return BigInteger.ModPow(value, pow, mod);
        }

        public static byte[] SubArray(this byte[] array, int start, int count)
        {
            var subArray = new byte[count];
            Array.Copy(array, start, subArray, 0, count);
            return subArray;
        }

        public static byte[] ToCString(this string str)
        {
            var utf8StringBytes = Encoding.UTF8.GetBytes(str);
            var data = new byte[utf8StringBytes.Length + 1];
            Array.Copy(utf8StringBytes, data, utf8StringBytes.Length);
            data[^1] = 0;
            return data;
        }

        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit)
            where T : Attribute
        {
            return (T[])member.GetCustomAttributes(typeof(T), inherit) ?? new T[] { };
        }

        public static bool TryGetAttributes<T>(this MemberInfo member, bool inherit, out IEnumerable<T> attributes)
            where T : Attribute
        {
            var attrs = (T[])member.GetCustomAttributes(typeof(T), inherit) ?? new T[] { };
            attributes = attrs;
            return attrs.Length > 0;
        }

        public static IEnumerable<TSource> TakeRandom<TSource>(this IEnumerable<TSource> source, int count)
        {
            var random = new Random();
            var indexes = new List<int>(source.Count());
            for (var index = 0; index < indexes.Capacity; index++)
                indexes.Add(index);

            var result = new List<TSource>(count);
            for (var index = 0; index < count && indexes.Count > 0; index++)
            {
                var randomIndex = random.Next(indexes.Count);
                result.Add(source.ElementAt(randomIndex));
                indexes.Remove(randomIndex);
            }

            return result;
        }
    }
}
