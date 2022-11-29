﻿using System;
using System.IO;

namespace HermesProxy.World
{
    public class SniffFile
    {
        public SniffFile(string fileName, ushort build)
        {
            string dir = "PacketsLog";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string file = fileName + "_" + build + "_" + Time.UnixTime + ".pkt";
            string path = Path.Combine(dir, file);

            _fileWriter = new BinaryWriter(File.Open(path, FileMode.Create));
            _gameVersion = build;
        }

        readonly BinaryWriter _fileWriter;
        readonly ushort _gameVersion;
        readonly System.Threading.Mutex _mutex = new();

        public void WriteHeader()
        {
            _fileWriter.Write('P');
            _fileWriter.Write('K');
            _fileWriter.Write('T');
            ushort sniffVersion = 0x201;
            _fileWriter.Write(sniffVersion);
            _fileWriter.Write(_gameVersion);

            for (int i = 0; i < 40; i++)
            {
                byte zero = 0;
                _fileWriter.Write(zero);
            }
        }

        public void WritePacket(uint opcode, bool isFromClient, byte[] data)
        {
            _mutex.WaitOne();
            byte direction = !isFromClient ? (byte)0xff : (byte)0x0;
            _fileWriter.Write(direction);

            uint unixtime = (uint)Time.UnixTime;
            _fileWriter.Write(unixtime);
            _fileWriter.Write(Environment.TickCount);

            if (isFromClient)
            {
                uint packetSize = (uint)(data.Length - 2 + sizeof(uint));
                _fileWriter.Write(packetSize);
                _fileWriter.Write(opcode);

                for (int i = 2; i < data.Length; i++)
                    _fileWriter.Write(data[i]);
            }
            else
            {
                uint packetSize = (uint)data.Length + sizeof(ushort);
                _fileWriter.Write(packetSize);
                ushort opcode2 = (ushort)opcode;
                _fileWriter.Write(opcode2);
                _fileWriter.Write(data);
            }
            _mutex.ReleaseMutex();
        }

        public void CloseFile()
        {
            _fileWriter.Close();
        }
    }
}
