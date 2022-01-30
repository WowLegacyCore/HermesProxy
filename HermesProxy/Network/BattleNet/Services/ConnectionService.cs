using System;

using Bgs.Protocol.Connection.V1;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.Services;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        [BattlenetService(ServiceHash.ConnectionService, 1)]
        public BattlenetRpcErrorCode HandleConnectRequest(ConnectRequest request, ConnectResponse response)
        {
            if (request.ClientId != null)
                response.ClientId.MergeFrom(request.ClientId);

            response.ServerId = new()
            {
                Label = (uint)Environment.ProcessId,
                Epoch = (uint)Time.UnixTime
            };
            response.ServerTime = (ulong)Time.UnixTimeMilliseconds;
            response.UseBindlessRpc = request.UseBindlessRpc;

            return BattlenetRpcErrorCode.Ok;
        }
    }
}
