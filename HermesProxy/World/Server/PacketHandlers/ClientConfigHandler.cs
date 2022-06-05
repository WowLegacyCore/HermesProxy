using BNetServer.Services;
using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_UPDATE_ACCOUNT_DATA)]
        void HandleUpdateAccountData(UserClientUpdateAccountData data)
        {
            GetSession().AccountDataMgr.SaveData(data.PlayerGuid, data.Time, data.DataType, data.Size, data.CompressedData);
        }

        [PacketHandler(Opcode.CMSG_REQUEST_ACCOUNT_DATA)]
        void HandleRequestAccountData(RequestAccountData data)
        {
            if (GetSession().AccountDataMgr.Data[data.DataType] == null)
            {
                Log.Print(LogType.Error, $"Client requested missing account data {data.DataType}.");
                GetSession().AccountDataMgr.Data[data.DataType] = new();
                GetSession().AccountDataMgr.Data[data.DataType].Type = data.DataType;
                GetSession().AccountDataMgr.Data[data.DataType].Timestamp = Time.UnixTime;
                GetSession().AccountDataMgr.Data[data.DataType].UncompressedSize = 0;
                GetSession().AccountDataMgr.Data[data.DataType].CompressedData = new byte[0];
            }

            GetSession().AccountDataMgr.Data[data.DataType].Guid = data.PlayerGuid;
            UpdateAccountData update = new(GetSession().AccountDataMgr.Data[data.DataType]);
            SendPacket(update);
        }

        [PacketHandler(Opcode.CMSG_SAVE_CUF_PROFILES)]
        void HandleUpdateAccountData(SaveCUFProfiles cuf)
        {
            GetSession().AccountDataMgr.SaveCUFProfiles(cuf.Data);
        }
    }
}
