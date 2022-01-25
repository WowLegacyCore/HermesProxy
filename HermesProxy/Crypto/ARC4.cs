using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.Crypto
{
    public class ARC4
    {
        private readonly byte[] state;
        private byte x, y;

        public ARC4(byte[] key)
        {
            state = new byte[256];
            x = y = 0;
            KeySetup(key);
        }

        public int Process(byte[] buffer, int start, int count)
        {
            return InternalTransformBlock(buffer, start, count, buffer, start);
        }

        private void KeySetup(byte[] key)
        {
            byte index1 = 0;
            byte index2 = 0;

            for (int counter = 0; counter < 256; counter++)
            {
                state[counter] = (byte)counter;
            }
            x = 0;
            y = 0;
            for (int counter = 0; counter < 256; counter++)
            {
                index2 = (byte)(key[index1] + state[counter] + index2);
                // swap byte
                byte tmp = state[counter];
                state[counter] = state[index2];
                state[index2] = tmp;
                index1 = (byte)((index1 + 1) % key.Length);
            }
        }

        private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (var counter = 0; counter < inputCount; counter++)
            {
                x = (byte)(x + 1);
                y = (byte)(state[x] + y);
                // swap byte
                var tmp = state[x];
                state[x] = state[y];
                state[y] = tmp;

                var xorIndex = (byte)(state[x] + state[y]);
                outputBuffer[outputOffset + counter] = (byte)(inputBuffer[inputOffset + counter] ^ state[xorIndex]);
            }
            return inputCount;
        }
    }
}
