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
            _accountDataMgr.SaveData(data.PlayerGuid, data.Time, data.DataType, data.Size, data.CompressedData);
        }
        [PacketHandler(Opcode.CMSG_REQUEST_ACCOUNT_DATA)]
        void HandleRequestAccountData(RequestAccountData data)
        {
            if (_accountDataMgr.Data[data.DataType] == null)
            {
                Log.Print(LogType.Error, $"Client requested missing account data {data.DataType}.");
                _accountDataMgr.Data[data.DataType] = new();
                _accountDataMgr.Data[data.DataType].Type = data.DataType;
                _accountDataMgr.Data[data.DataType].Timestamp = Time.UnixTime;
                _accountDataMgr.Data[data.DataType].UncompressedSize = 0;
                _accountDataMgr.Data[data.DataType].CompressedData = new byte[0];
            }

            _accountDataMgr.Data[data.DataType].Guid = data.PlayerGuid;
            UpdateAccountData update = new(_accountDataMgr.Data[data.DataType]);
            SendPacket(update);
        }
    }
}
