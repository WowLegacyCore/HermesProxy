// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Bgs.Protocol.Connection.V1;
using Framework.Constants;
using System;

namespace BNetServer.Services
{
    public partial class BnetServices
    {
        [Service(ServiceRequirement.Unauthorized, OriginalHash.ConnectionService, 1)]
        BattlenetRpcErrorCode HandleConnect(ConnectRequest request, ConnectResponse response)
        {
            if (request.ClientId != null)
                response.ClientId.MergeFrom(request.ClientId);

            response.ServerId = new ProcessId
            {
                Label = (uint)Environment.ProcessId,
                Epoch = (uint)Time.UnixTime
            };
            response.ServerTime = (ulong)Time.UnixTimeMilliseconds;

            response.UseBindlessRpc = request.UseBindlessRpc;

            return BattlenetRpcErrorCode.Ok;
        }

        [Service(ServiceRequirement.Always, OriginalHash.ConnectionService, 5)]
        BattlenetRpcErrorCode HandleKeepAlive(NoData request)
        {
            return BattlenetRpcErrorCode.Ok;
        }

        [Service(ServiceRequirement.Always, OriginalHash.ConnectionService, 7)]
        BattlenetRpcErrorCode HandleRequestDisconnect(DisconnectRequest request)
        {
            if (GetSession() != null && GetSession().AuthClient != null)
                GetSession().AuthClient.Disconnect();

            var disconnectNotification = new DisconnectNotification
            {
                ErrorCode = request.ErrorCode
            };
            SendRequest(OriginalHash.ConnectionService, 4, disconnectNotification);

            CloseSocket();

            return BattlenetRpcErrorCode.Ok;
        }
    }
}
