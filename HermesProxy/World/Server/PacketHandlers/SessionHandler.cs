using System;
using System.Collections.Generic;
using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Serialization;
using Framework.Util;
using Framework.Web;
using Google.Protobuf;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using AuthResult = HermesProxy.Auth.AuthResult;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        [PacketHandler(Opcode.CMSG_CHANGE_REALM_TICKET)]
        void HandleChangeRealmTicket(ChangeRealmTicket request)
        {
            ChangeRealmTicketResponse response = new();
            
            if (GetSession().AuthClient.Reconnect() != AuthResult.SUCCESS)
            {
                response.Allow = false;
                SendPacket(response);
                return;
            }
            GetSession().AuthClient.RequestRealmListUpdate();

            response.Token = request.Token;
            response.Allow = true;
            response.Ticket = new ByteBuffer(new byte[32]);

            SendPacket(response);
        }

        [PacketHandler(Opcode.CMSG_BATTLENET_REQUEST)]
        void HandleBattlenetRequest(BattlenetRequest request)
        {
            if (_bnetRpc == null)
            {
                Log.Print(LogType.Error, $"Client tried {Opcode.CMSG_BATTLENET_REQUEST} without authentication");
                return;
            }

            _bnetRpc.Invoke(
                serviceId: (uint)request.Method.ObjectId,
                (OriginalHash) request.Method.GetServiceHash(),
                request.Method.GetMethodId(),
                request.Method.Token,
                new CodedInputStream(request.Data)
            );
            return;

            BattlenetResponse response = new()
            {
                Method = request.Method
            };
            IMessage responseMessage = null;
            switch ((OriginalHash) request.Method.GetServiceHash(), request.Method.GetMethodId())
            {
                case (OriginalHash.GameUtilitiesService, (uint)GameUtilitiesServiceMethods.GenericClientRequest):
                {
                    ClientRequest x = new();
                    x.MergeFrom(request.Data);
                    (response.Status, responseMessage) = HandleGenericClientRequest(x);
                    break;
                }
                case (OriginalHash.GameUtilitiesService, (uint)GameUtilitiesServiceMethods.GetAllValuesForAttribute):
                {
                    GetAllValuesForAttributeRequest x = new();
                    x.MergeFrom(request.Data);
                    (response.Status, responseMessage) = HandleGetAllValuesForAttribute(x);
                    break;
                }
                default:
                    Log.Print(LogType.Error, $"Client requested unknown Battlenet service/method {(OriginalHash) request.Method.GetServiceHash()}/{request.Method.GetMethodId()}");
                    response.Status = BattlenetRpcErrorCode.NotExists;
                    break;
            }
            
            SendRpcMessage(0, (OriginalHash) request.Method.GetServiceHash(), request.Method.GetMethodId(), request.Method.Token, response.Status, responseMessage);
            return;
            byte[] bytes = responseMessage == null ? Array.Empty<byte>() : responseMessage.ToByteArray();
            response.Data = new ByteBuffer(bytes);

            SendPacket(response);
        }

        private (BattlenetRpcErrorCode, IMessage) HandleGenericClientRequest(ClientRequest request)
        {
            Bgs.Protocol.Attribute command = null;
            Dictionary<string, Variant> Params = new();

            for (int i = 0; i < request.Attribute.Count; ++i)
            {
                Bgs.Protocol.Attribute attr = request.Attribute[i];
                Params[attr.Name] = attr.Value;
                if (attr.Name.Contains("Command_"))
                    command = attr;
            }

            if (command == null)
            {
                Log.Print(LogType.Error, $"{GetClientInfo()} sent ClientRequest with no command.");
                return (BattlenetRpcErrorCode.RpcMalformedRequest, null);
            }
            Log.Print(LogType.Debug, $"Received {command.Name}");

            if (command.Name == $"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}")
                return HandleGetRealList(Params);
            if (command.Name == $"Command_RealmJoinRequest_v1_{GetCommandEndingForVersion()}")
                return HandleJoinRealm(Params);
            
            Log.Print(LogType.Error, $"Unknown s command {command.Name}");

            return (BattlenetRpcErrorCode.Internal, null);
        }

        private (BattlenetRpcErrorCode, IMessage) HandleJoinRealm(Dictionary<string, Variant> Params)
        {
            /*
            Variant realmAddress = Params.LookupByKey("Param_RealmAddress");
            if (realmAddress != null)
                return GetSession().RealmManager.JoinRealm(_globalSession, (uint)realmAddress.UintValue, _globalSession.Build, GetRemoteIpEndPoint().Address, _clientSecret, _globalSession.GameAccountInfo.Name, response);

            return BattlenetRpcErrorCode.WowServicesInvalidJoinTicket;
            */
            
            return (BattlenetRpcErrorCode.Internal, null);
        }

        private (BattlenetRpcErrorCode, IMessage) HandleGetRealList(Dictionary<string, Variant> Params)
        {
            ClientResponse response = new ClientResponse();
            string subRegionId = "";
            Variant subRegion = Params.LookupByKey($"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}");
            if (subRegion != null)
                subRegionId = subRegion.StringValue;

            _globalSession.AuthClient.WaitForRealmlist();

            var compressedRealmList = GetSession().RealmManager.GetRealmList(_globalSession.Build, subRegionId);
            if (compressedRealmList.Length == 0)
                return (BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse, null);

            response.Attribute.AddBlob("Param_RealmList", ByteString.CopyFrom(compressedRealmList));

            var realmCharacterCounts = new RealmCharacterCountList();
            foreach (var realm in _globalSession.RealmManager.GetRealms())
            {
                var countEntry = new RealmCharacterCountEntry();
                countEntry.WowRealmAddress = (int) realm.Id.GetAddress();
                countEntry.Count = realm.CharacterCount;
                realmCharacterCounts.Counts.Add(countEntry);
            }

            var compressedCharCount = Json.Deflate("JSONRealmCharacterCountList", realmCharacterCounts);
            response.Attribute.AddBlob("Param_CharacterCountList", ByteString.CopyFrom(compressedCharCount));

            return (BattlenetRpcErrorCode.Ok, response);
        }
        
        private (BattlenetRpcErrorCode, IMessage) HandleGetAllValuesForAttribute(GetAllValuesForAttributeRequest request)
        {
            GetAllValuesForAttributeResponse response = new();
            if (request.AttributeKey == $"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}")
            {
                GetSession().AuthClient.WaitForRealmlist();

                GetSession().RealmManager.WriteSubRegions(response);

                return (BattlenetRpcErrorCode.Ok, response);
            }

            return (BattlenetRpcErrorCode.RpcNotImplemented, null);
        }
        
        private string GetCommandEndingForVersion()
        {
            if (ModernVersion.GetExpansionVersion() == 1)
                return "c1";
            if (ModernVersion.GetExpansionVersion() == 2)
                return "bcc1";
            return "b9";
        }

        public string GetClientInfo()
        {
            string stream = "[";

            if (_globalSession != null)
            {
                if (_globalSession.AccountInfo != null && !_globalSession.AccountInfo.Login.IsEmpty())
                    stream += ", Account: " + _globalSession.AccountInfo.Login;

                if (_globalSession.GameAccountInfo != null)
                    stream += ", Game account: " + _globalSession.GameAccountInfo.Name;
            }

            stream += ']';

            return stream;
        }
    }
}
