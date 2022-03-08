using Framework.IO;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects
{
     public class UpdateData
    {
        uint MapId;
        uint BlockCount;
        List<WowGuid128> destroyGUIDs = new();
        List<WowGuid128> outOfRangeGUIDs = new();
        ByteBuffer data = new();
        GameSessionData gameState;

        public UpdateData(uint mapId)
        {
            MapId = mapId;
        }

        public void AddDestroyObject(WowGuid128 guid)
        {
            destroyGUIDs.Add(guid);
        }

        public void AddOutOfRangeGUID(List<WowGuid128> guids)
        {
            outOfRangeGUIDs.AddRange(guids);
        }

        public void AddOutOfRangeGUID(WowGuid128 guid)
        {
            outOfRangeGUIDs.Add(guid);
        }

        public void AddUpdateBlock(ByteBuffer block)
        {
            data.WriteBytes(block.GetData());
            ++BlockCount;
        }

        public bool BuildPacket(out UpdateObject packet)
        {
            packet = new UpdateObject(gameState);

            packet.NumObjUpdates = BlockCount;
            packet.MapID = (ushort)MapId;

            WorldPacket buffer = new();
            if (buffer.WriteBit(!outOfRangeGUIDs.Empty() || !destroyGUIDs.Empty()))
            {
                buffer.WriteUInt16((ushort)destroyGUIDs.Count);
                buffer.WriteInt32(destroyGUIDs.Count + outOfRangeGUIDs.Count);

                foreach (var destroyGuid in destroyGUIDs)
                    buffer.WritePackedGuid128(destroyGuid);

                foreach (var outOfRangeGuid in outOfRangeGUIDs)
                    buffer.WritePackedGuid128(outOfRangeGuid);
            }

            var bytes = data.GetData();
            buffer.WriteInt32(bytes.Length);
            buffer.WriteBytes(bytes);

            packet.Data = buffer.GetData();
            return true;
        }

        public void Clear()
        {
            data.Clear();
            destroyGUIDs.Clear();
            outOfRangeGUIDs.Clear();
            BlockCount = 0;
            MapId = 0;
        }

        public bool HasData() { return BlockCount > 0 || outOfRangeGUIDs.Count != 0; }

        public List<WowGuid128> GetOutOfRangeGUIDs() { return outOfRangeGUIDs; }

        public void SetMapId(ushort mapId) { MapId = mapId; }
    }
}
