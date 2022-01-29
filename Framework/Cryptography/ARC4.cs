namespace Framework.Cryptography
{
    public class ARC4
    {
        readonly byte[] _state;
        byte _x;
        byte _y;

        public ARC4(byte[] key)
        {
            _state = new byte[256];
            _x = _y = 0;

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
                _state[counter] = (byte)counter;
            }
            _x = 0;
            _y = 0;
            for (int counter = 0; counter < 256; counter++)
            {
                index2 = (byte)(key[index1] + _state[counter] + index2);
                // swap byte
                byte tmp = _state[counter];
                _state[counter] = _state[index2];
                _state[index2] = tmp;
                index1 = (byte)((index1 + 1) % key.Length);
            }
        }

        private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (var counter = 0; counter < inputCount; counter++)
            {
                _x = (byte)(_x + 1);
                _y = (byte)(_state[_x] + _y);
                // swap byte
                var tmp = _state[_x];
                _state[_x] = _state[_y];
                _state[_y] = tmp;

                var xorIndex = (byte)(_state[_x] + _state[_y]);
                outputBuffer[outputOffset + counter] = (byte)(inputBuffer[inputOffset + counter] ^ _state[xorIndex]);
            }
            return inputCount;
        }
    }
}
