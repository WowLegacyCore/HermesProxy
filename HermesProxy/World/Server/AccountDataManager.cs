using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Server
{
    public class AccountData
    {
        public WowGuid128 Guid;
        public long Timestamp;
        public uint Type;
        public uint UncompressedSize;
        public byte[] CompressedData;
    }
    public class AccountDataManager
    {
        public AccountData[] Data;
        string _accountName;
        string _realmName;
        
        public AccountDataManager(string accountName, string realmName)
        {
            _accountName = accountName.Trim();
            _realmName = realmName.Trim();
        }

        public static bool IsGlobalDataType(uint type)
        {
            switch ((AccountDataType)type)
            {
                case AccountDataType.GlobalConfigCache:
                case AccountDataType.GlobalBindingsCache:
                case AccountDataType.GlobalMacrosCache:
                case AccountDataType.GlobalTTSCache:
                case AccountDataType.GlobalFlaggedCache:
                    return true;
            }
            return false;
        }

        public string GetAccountDataDirectory()
        {
            string path = "AccountData";
            path = Path.Combine(path, _accountName);
            path = Path.Combine(path, _realmName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public string GetFullFileName(WowGuid128 guid, uint type)
        {
            string file;
            if (IsGlobalDataType(type))
                file = $"data-{type}.bin";
            else
                file = $"data-{type}-{guid.GetLowValue()}-{guid.GetHighValue()}.bin";

            string path = GetAccountDataDirectory();
            path = Path.Combine(path, file);
            return path;
        }

        public void LoadAllData(WowGuid128 guid)
        {
            Data = new AccountData[ModernVersion.GetAccountDataCount()];

            for (uint i = 0; i < ModernVersion.GetAccountDataCount(); i++)
            {
                Data[i] = LoadData(guid, i);
            }
        }

        public AccountData LoadData(WowGuid128 guid, uint type)
        {
            AccountData data = null;
            string fileName = GetFullFileName(guid, type);

            if (File.Exists(fileName))
            {
                using (FileStream file = File.OpenRead(GetFullFileName(guid, type)))
                {
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(GetFullFileName(guid, type))))
                    {
                        data = new();
                        ulong guidLow = reader.ReadUInt64();
                        ulong guidHigh = reader.ReadUInt64();
                        data.Guid = new WowGuid128(guidHigh, guidLow);

                        if (!IsGlobalDataType(type))
                            System.Diagnostics.Trace.Assert(guid == data.Guid);

                        data.Timestamp = reader.ReadInt64();
                        data.Type = reader.ReadUInt32();
                        System.Diagnostics.Trace.Assert(type == data.Type);
                        data.UncompressedSize = reader.ReadUInt32();

                        int compressedSize = reader.ReadInt32();
                        data.CompressedData = reader.ReadBytes(compressedSize);
                    }
                }
            }
            
            return data;
        }

        public void SaveData(WowGuid128 guid, long timestamp, uint type, uint uncompressedSize, byte[] compressedData)
        {
            if (compressedData == null)
                return;
            if (Data[type] == null)
                Data[type] = new();

            Data[type].Guid = guid;
            Data[type].Timestamp = timestamp;
            Data[type].Type = type;
            Data[type].UncompressedSize = uncompressedSize;
            Data[type].CompressedData = compressedData;

            using (BinaryWriter writer = new BinaryWriter(File.Open(GetFullFileName(guid, type), FileMode.Create)))
            {
                writer.Write(guid.GetLowValue());
                writer.Write(guid.GetHighValue());
                writer.Write(timestamp);
                writer.Write(type);
                writer.Write(uncompressedSize);
                writer.Write(compressedData.Length);
                writer.Write(compressedData);
            }
        }

        public byte[] LoadCUFProfiles()
        {
            string fileName = GetAccountDataDirectory();
            fileName = Path.Combine(fileName, "cuf.bin");

            if (File.Exists(fileName))
            {
                using (FileStream file = File.OpenRead(fileName))
                {
                    using (BinaryReader reader = new BinaryReader(file))
                    {
                        return File.ReadAllBytes(fileName);
                    }
                }
            }

            return new byte[4];
        }

        public void SaveCUFProfiles(byte[] data)
        {
            string fileName = GetAccountDataDirectory();
            fileName = Path.Combine(fileName, "cuf.bin");

            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(data);
            }
        }
    }
}
