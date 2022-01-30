using System.Collections.Generic;

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Logging;
using HermesProxy.Network.BattleNet.Services;
using HermesProxy.Network.Realm;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        [BattlenetService(ServiceHash.GameUtilitiesService, 1)]
        public BattlenetRpcErrorCode HandleClientRequest(ClientRequest request, ClientResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            var command = new Attribute();
            var parameters = new Dictionary<string, Variant>();

            for (var i = 0; i < request.Attribute.Count; ++i)
            {
                var attr = request.Attribute[i];
                parameters[attr.Name] = attr.Value;

                if (attr.Name.Contains("Command_"))
                    command = attr;
            }

            if (string.IsNullOrEmpty(command.Name))
            {
                Log.Print(LogType.Debug, $"{GetRemoteEndpoint()} sent malformed ClientRequest");
                return BattlenetRpcErrorCode.RpcMalformedRequest;
            }

            if (!_clientRequestHandlers.TryGetValue(command.Name, out var handler))
            {
                Log.Print(LogType.Debug, $"{GetRemoteEndpoint()} sent ClientRequest with unknown command {command.Name}");
                return BattlenetRpcErrorCode.RpcNotImplemented;
            }

            return handler(parameters, response);
        }

        [BattlenetService(ServiceHash.GameUtilitiesService, 10)]
        public BattlenetRpcErrorCode HandleGetAllValuesForAttributes(GetAllValuesForAttributeRequest request, GetAllValuesForAttributeResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            if (request.AttributeKey == "Command_RealmListRequest_v1_b9")
            {
                RealmManager.WriteSubRegions(response);
                return BattlenetRpcErrorCode.Ok;
            }

            return BattlenetRpcErrorCode.RpcNotImplemented;
        }
    }
}
