using Framework.Collections;
using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using System;
using System.Runtime.InteropServices;

namespace HermesProxy.World.Objects
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UpdateValues
    {
        [FieldOffset(0)]
        public uint UnsignedValue;

        [FieldOffset(0)]
        public int SignedValue;

        [FieldOffset(0)]
        public float FloatValue;
    }

    public class UpdateFieldsArray
    {
        public UpdateFieldsArray(uint size)
        {
            ValuesCount = size;
            m_updateValues = new UpdateValues[size];
            m_updateMask = new UpdateMask(size);
        }
        public uint ValuesCount;
        public UpdateValues[] m_updateValues;
        public UpdateMask m_updateMask;

        public void WriteToPacket(ByteBuffer buffer)
        {
            var fieldBuffer = new ByteBuffer();
            for (var index = 0; index < ValuesCount; ++index)
            {
                if (m_updateMask.GetBit(index))
                {
                    fieldBuffer.WriteUInt32(m_updateValues[index].UnsignedValue);
                }
            }
            m_updateMask.AppendToPacket(buffer);
            buffer.WriteBytes(fieldBuffer);
        }

        public void SetUpdateField<T>(object index, T value, byte offset = 0) where T : new()
        {
            if (value is byte byteValue)
            {
                if (offset > 3)
                {
                    Log.Print(LogType.Error, $"SetUpdateField<UInt8>: Wrong offset: {offset}");
                    return;
                }

                if ((byte)(m_updateValues[(int)index].UnsignedValue >> (offset * 8)) != byteValue)
                {
                    m_updateValues[(int)index].UnsignedValue &= ~(uint)(0xFF << (offset * 8));
                    m_updateValues[(int)index].UnsignedValue |= (uint)byteValue << (offset * 8);
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is ushort ushortValue)
            {
                if (offset > 1)
                {
                    Log.Print(LogType.Error, $"SetUpdateField<UInt16>: Wrong offset: {offset}");
                    return;
                }

                if ((ushort)(GetUpdateField<uint>(index) >> (offset * 16)) != ushortValue)
                {
                    m_updateValues[(int)index].UnsignedValue &= ~((uint)0xFFFF << (offset * 16));
                    m_updateValues[(int)index].UnsignedValue |= (uint)ushortValue << (offset * 16);
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is int intValue)
            {
                if (m_updateValues[(int)index].SignedValue != intValue)
                {
                    m_updateValues[(int)index].SignedValue = intValue;
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is uint uintValue)
            {
                if (m_updateValues[(int)index].UnsignedValue != uintValue)
                {
                    m_updateValues[(int)index].UnsignedValue = uintValue;
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is float floatValue)
            {
                if (m_updateValues[(int)index].FloatValue != floatValue)
                {
                    m_updateValues[(int)index].FloatValue = floatValue;
                    m_updateMask.SetBit((int)index);
                }
            }
            else if (value is ulong ulongValue)
            {
                if (GetUpdateField<ulong>(index) != ulongValue)
                {
                    m_updateValues[(int)index].UnsignedValue = MathFunctions.Pair64_LoPart(ulongValue);
                    m_updateValues[(int)index + 1].UnsignedValue = MathFunctions.Pair64_HiPart(ulongValue);
                    m_updateMask.SetBit((int)index);
                    m_updateMask.SetBit((int)index + 1);
                }
            }
            else if (value is WowGuid128 guid)
            {
                //if (GetUpdateField<WowGuid128>(index) != guid)
                //{
                SetUpdateField<ulong>(index, guid.GetLowValue());
                SetUpdateField<ulong>((int)index + 2, guid.GetHighValue());
                //}
            }
            else
                throw new Exception($"Unhandled type {typeof(T).ToString()} in SetUpdateField!");
        }

        public T GetUpdateField<T>(object index, byte offset = 0) =>
            default(T) switch
            {
                byte => (T)Convert.ChangeType((byte)(m_updateValues[(int)index].UnsignedValue >> (offset * 8)) & 0xFF, typeof(T)),
                ushort => (T)Convert.ChangeType((ushort)(m_updateValues[(int)index].UnsignedValue >> (offset * 16)) & 0xFFFF, typeof(T)),
                int => (T)Convert.ChangeType(m_updateValues[(int)index].SignedValue, typeof(T)),
                uint => (T)Convert.ChangeType(m_updateValues[(int)index].UnsignedValue, typeof(T)),
                float => (T)Convert.ChangeType(m_updateValues[(int)index].FloatValue, typeof(T)),
                ulong => (T)Convert.ChangeType((ulong)m_updateValues[(int)index + 1].UnsignedValue << 32 | m_updateValues[(int)index].UnsignedValue, typeof(T)),
                WowGuid128 => (T)Convert.ChangeType(new WowGuid128(GetUpdateField<ulong>((int)index + 2), GetUpdateField<ulong>(index)), typeof(T)),
                _ => throw new Exception($"{typeof(T)} is not implemented in GetUpdateField<T>"),
            };


        public void _LoadIntoDataField(string data, uint startOffset, uint count)
        {
            if (string.IsNullOrEmpty(data))
                return;

            var lines = new StringArray(data, ' ');
            if (lines.Length != count)
                return;

            for (var index = 0; index < count; ++index)
            {
                if (uint.TryParse(lines[index], out uint value))
                {
                    m_updateValues[(int)startOffset + index].UnsignedValue = value;
                    m_updateMask.SetBit((int)(startOffset + index));
                }
            }
        }
        public bool HasFlag(object index, object flag)
        {
            if ((int)index >= ValuesCount)
                return false;

            return (GetUpdateField<uint>(index) & (uint)flag) != 0;
        }

        public void AddFlag(object index, object newFlag)
        {
            var oldValue = m_updateValues[(int)index].UnsignedValue;
            var newValue = oldValue | Convert.ToUInt32(newFlag);

            if (oldValue != newValue)
                SetUpdateField<uint>(index, newValue);
        }

        public void RemoveFlag(object index, object newFlag)
        {
            var oldValue = m_updateValues[(int)index].UnsignedValue;
            var newValue = oldValue & ~Convert.ToUInt32(newFlag);

            if (oldValue != newValue)
            {
                SetUpdateField<uint>(index, newValue);
            }
        }

        public void ApplyFlag<T>(object index, T flag, bool apply)
        {
            if (apply)
                AddFlag(index, flag);
            else
                RemoveFlag(index, flag);
        }

        public void AddFlag64(object index, object newFlag)
        {
            var oldValue = GetUpdateField<ulong>(index);
            var newValue = oldValue | Convert.ToUInt64(newFlag);

            if (oldValue != newValue)
                SetUpdateField<ulong>(index, newValue);
        }

        public void RemoveFlag64(object index, object newFlag)
        {
            var oldValue = GetUpdateField<ulong>(index);
            var newValue = oldValue & ~Convert.ToUInt64(newFlag);

            if (oldValue != newValue)
                SetUpdateField<ulong>(index, newValue);
        }

        public void ApplyFlag64<T>(object index, T flag, bool apply)
        {
            if (apply)
                AddFlag(index, flag);
            else
                RemoveFlag(index, flag);
        }

        public void AddByteFlag(object index, byte offset, object newFlag)
        {
            if (offset > 4)
            {
                Log.Print(LogType.Error,  $"Object.SetByteFlag: Wrong offset {offset}");
                return;
            }

            if (((byte)m_updateValues[(int)index].UnsignedValue >> (offset * 8) & (int)newFlag) == 0)
            {
                m_updateValues[(int)index].UnsignedValue |= (uint)newFlag << (offset * 8);
                m_updateMask.SetBit((int)index);
            }
        }

        public void RemoveByteFlag(object index, byte offset, object oldFlag)
        {
            if (offset > 4)
            {
                Log.Print(LogType.Error,  $"Object.RemoveByteFlag: Wrong offset {offset}");
                return;
            }

            if (((byte)m_updateValues[(int)index].UnsignedValue >> (offset * 8) & (int)oldFlag) != 0)
            {
                m_updateValues[(int)index].UnsignedValue &= ~((uint)oldFlag << (offset * 8));
                m_updateMask.SetBit((int)index);
            }
        }
    }

    public class DynamicUpdateFieldsArray
    {
        public DynamicUpdateFieldsArray(uint size, UpdateTypeModern updateType)
        {
            ValuesCount = size;
            m_updateType = updateType;
            m_updateMask = new UpdateMask(size);
            m_fieldBuffer = new();
        }

        readonly uint ValuesCount;
        readonly UpdateTypeModern m_updateType;
        readonly UpdateMask m_updateMask;
        readonly ByteBuffer m_fieldBuffer;

        public void WriteToPacket(ByteBuffer buffer)
        {
            m_updateMask.AppendToPacket(buffer);
            buffer.WriteBytes(m_fieldBuffer);
        }

        public void SetUpdateField(int index, uint[] values, DynamicFieldChangeType changeType)
        {
            var valueBuffer = new ByteBuffer();
            m_updateMask.SetBit(index);

            var arrayMask = new DynamicUpdateMask((uint)values.Length);
            arrayMask.EncodeDynamicFieldChangeType(changeType, m_updateType);
            if (m_updateType == UpdateTypeModern.Values && changeType == DynamicFieldChangeType.ValueAndSizeChanged)
            {
                arrayMask.ValueCount = values.Length;
                arrayMask.SetCount(values.Length);
            }

            for (var v = 0; v < values.Length; ++v)
            {
                arrayMask.SetBit(v);
                valueBuffer.WriteUInt32(values[v]);
            }

            arrayMask.AppendToPacket(m_fieldBuffer);
            m_fieldBuffer.WriteBytes(valueBuffer);
        }

        public void SetUpdateField<T>(object index, T value, DynamicFieldChangeType changeType) where T : new()
        {
            if (value is int intValue)
            {
                uint[] values = new uint[1];
                UpdateValues union = new()
                {
                    SignedValue = intValue
                };
                values[0] = union.UnsignedValue;
                SetUpdateField((int)index, values, changeType);
            }
            else if (value is uint uintValue)
            {
                uint[] values = new uint[1];
                values[0] = uintValue;
                SetUpdateField((int)index, values, changeType);
            }
            else if (value is float floatValue)
            {
                uint[] values = new uint[1];
                UpdateValues union = new()
                {
                    FloatValue = floatValue
                };
                values[0] = union.UnsignedValue;
                SetUpdateField((int)index, values, changeType);
            }
            else if (value is ulong ulongValue)
            {
                uint[] values = new uint[2];
                values[0] = MathFunctions.Pair64_LoPart(ulongValue);
                values[1] = MathFunctions.Pair64_HiPart(ulongValue);
                SetUpdateField((int)index, values, changeType);
            }
            else if (value is WowGuid128 guid)
            {
                uint[] values = new uint[4];
                values[0] = MathFunctions.Pair64_LoPart(guid.GetLowValue());
                values[1] = MathFunctions.Pair64_HiPart(guid.GetLowValue());
                values[2] = MathFunctions.Pair64_LoPart(guid.GetHighValue());
                values[3] = MathFunctions.Pair64_HiPart(guid.GetHighValue());
                SetUpdateField((int)index, values, changeType);
            }
            else
                throw new Exception($"Unhandled type {typeof(T).ToString()} in SetUpdateField!");
        }
    }
}
