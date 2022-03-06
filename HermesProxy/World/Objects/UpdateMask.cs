using Framework.IO;
using HermesProxy.World.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
    public class UpdateMask
    {
        public UpdateMask(uint valuesCount = 0)
        {
            _fieldCount = valuesCount;
            _blockCount = (valuesCount + 32 - 1) / 32;

            _mask = new BitArray((int)valuesCount, false);
        }

        public void SetCount(int valuesCount)
        {
            _fieldCount = (uint)valuesCount;
            _blockCount = (uint)(valuesCount + 32 - 1) / 32;

            _mask = new BitArray(valuesCount, false);
        }

        public uint GetCount() { return _fieldCount; }

        public virtual void AppendToPacket(ByteBuffer data)
        {
            data.WriteUInt8((byte)_blockCount);
            var maskArray = new byte[_blockCount << 2];

            _mask.CopyTo(maskArray, 0);
            data.WriteBytes(maskArray);
        }

        public bool GetBit(int index)
        {
            return _mask.Get(index);
        }

        public void SetBit(int index)
        {
            _mask.Set(index, true);
        }

        void UnsetBit(int index)
        {
            _mask.Set(index, false);
        }

        public void Clear()
        {
            _mask.SetAll(false);
        }

        uint _fieldCount;
        protected uint _blockCount;
        protected BitArray _mask;
    }

    public class DynamicUpdateMask : UpdateMask
    {
        public DynamicUpdateMask(uint valuesCount) : base(valuesCount) { }

        public void EncodeDynamicFieldChangeType(DynamicFieldChangeType changeType, UpdateTypeModern updateType)
        {
            DynamicFieldChangeType = (uint)(_blockCount | ((uint)(changeType & HermesProxy.World.Objects.DynamicFieldChangeType.ValueAndSizeChanged) * ((3 - (int)updateType /*this part evaluates to 0 if update type is not VALUES*/) / 3)));
        }

        public override void AppendToPacket(ByteBuffer data)
        {
            data.WriteUInt16((ushort)DynamicFieldChangeType);
            if (ValueCount != null)
                data.WriteInt32((int)ValueCount);

            var maskArray = new byte[_blockCount << 2];

            _mask.CopyTo(maskArray, 0);
            data.WriteBytes(maskArray);
        }

        public uint DynamicFieldChangeType;
        public int? ValueCount;
    }

    public enum DynamicFieldChangeType
    {
        Unchanged = 0,
        ValueChanged = 0x7FFF,
        ValueAndSizeChanged = 0x8000
    }
}
