// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Framework.Constants;
using Framework.Logging;
using Framework.Serialization;
using Framework.Web;
using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace BNetServer.Networking
{
    public partial class Session
    {
        string GetCommandEndingForVersion()
        {
            if (Framework.Settings.GetClientExpansionVersion() == 1)
                return "c1";
            if (Framework.Settings.GetClientExpansionVersion() == 2)
                return "bcc1";
            return "b9";
        }

        [Service(OriginalHash.GameUtilitiesService, 1)]
        BattlenetRpcErrorCode HandleProcessClientRequest(ClientRequest request, ClientResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

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
                return BattlenetRpcErrorCode.RpcMalformedRequest;
            }

            if (command.Name == $"Command_RealmListTicketRequest_v1_{GetCommandEndingForVersion()}")
                return GetRealmListTicket(Params, response);
            if (command.Name == $"Command_LastCharPlayedRequest_v1_{GetCommandEndingForVersion()}")
                return GetLastCharPlayed(Params, response);
            if (command.Name == $"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}")
                return GetRealmList(Params, response);
            if (command.Name == $"Command_RealmJoinRequest_v1_{GetCommandEndingForVersion()}")
                return JoinRealm(Params, response);

            return BattlenetRpcErrorCode.RpcNotImplemented;
        }

        [Service(OriginalHash.GameUtilitiesService, 10)]
        BattlenetRpcErrorCode HandleGetAllValuesForAttribute(GetAllValuesForAttributeRequest request, GetAllValuesForAttributeResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            if (request.AttributeKey == $"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}")
            {
                Global.RealmMgr.WriteSubRegions(response);
                return BattlenetRpcErrorCode.Ok;
            }

            return BattlenetRpcErrorCode.RpcNotImplemented;
        }

        BattlenetRpcErrorCode GetRealmListTicket(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant identity = Params.LookupByKey("Param_Identity");
            if (identity != null)
            {
                var realmListTicketIdentity = Json.CreateObject<RealmListTicketIdentity>(identity.BlobValue.ToStringUtf8(), true);
                var gameAccount = _globalSession.AccountInfo.GameAccounts.LookupByKey(realmListTicketIdentity.GameAccountId);
                if (gameAccount != null)
                    _globalSession.GameAccountInfo = gameAccount;
            }

            if (_globalSession.GameAccountInfo == null)
                return BattlenetRpcErrorCode.UtilServerInvalidIdentityArgs;

            if (_globalSession.GameAccountInfo.IsPermanenetlyBanned)
                return BattlenetRpcErrorCode.GameAccountBanned;
            else if (_globalSession.GameAccountInfo.IsBanned)
                return BattlenetRpcErrorCode.GameAccountSuspended;

            bool clientInfoOk = false;
            Variant clientInfo = Params.LookupByKey("Param_ClientInfo");
            if (clientInfo != null)
            {
                var realmListTicketClientInformation = Json.CreateObject<RealmListTicketClientInformation>(clientInfo.BlobValue.ToStringUtf8(), true);
                clientInfoOk = true;
                int i = 0;
                foreach (byte b in realmListTicketClientInformation.Info.Secret)
                    _clientSecret[i++] = b;
            }

            if (!clientInfoOk)
                return BattlenetRpcErrorCode.WowServicesDeniedRealmListTicket;

            /*
            PreparedStatement stmt = DB.Login.GetPreparedStatement(LoginStatements.UpdBnetLastLoginInfo);
            stmt.AddValue(0, GetRemoteIpEndPoint().ToString());
            stmt.AddValue(1, (byte)locale.ToEnum<Locale>());
            stmt.AddValue(2, os);
            stmt.AddValue(3, accountInfo.Id);

            DB.Login.Execute(stmt);
            */

            var attribute = new Bgs.Protocol.Attribute();
            attribute.Name = "Param_RealmListTicket";
            attribute.Value = new Variant();
            attribute.Value.BlobValue = ByteString.CopyFrom("AuthRealmListTicket", System.Text.Encoding.UTF8);
            response.Attribute.Add(attribute);

            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode GetLastCharPlayed(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant subRegion = Params.LookupByKey($"Command_LastCharPlayedRequest_v1_{GetCommandEndingForVersion()}");
            if (subRegion != null)
            {
                var lastPlayerChar = _globalSession.GameAccountInfo.LastPlayedCharacters.LookupByKey(subRegion.StringValue);
                if (lastPlayerChar != null)
                {
                    var compressed = Global.RealmMgr.GetRealmEntryJSON(lastPlayerChar.RealmId, _globalSession.Build);
                    if (compressed.Length == 0)
                        return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

                    var attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_RealmEntry";
                    attribute.Value = new Variant();
                    attribute.Value.BlobValue = ByteString.CopyFrom(compressed);
                    response.Attribute.Add(attribute);

                    attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_CharacterName";
                    attribute.Value = new Variant();
                    attribute.Value.StringValue = lastPlayerChar.CharacterName;
                    response.Attribute.Add(attribute);

                    attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_CharacterGUID";
                    attribute.Value = new Variant();
                    attribute.Value.BlobValue = ByteString.CopyFrom(BitConverter.GetBytes(lastPlayerChar.CharacterGUID));
                    response.Attribute.Add(attribute);

                    attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_LastPlayedTime";
                    attribute.Value = new Variant();
                    attribute.Value.IntValue = (int)lastPlayerChar.LastPlayedTime;
                    response.Attribute.Add(attribute);
                }

                return BattlenetRpcErrorCode.Ok;
            }

            return BattlenetRpcErrorCode.UtilServerUnknownRealm;
        }

        BattlenetRpcErrorCode GetRealmList(Dictionary<string, Variant> Params, ClientResponse response)
        {
            if (_globalSession.GameAccountInfo == null)
                return BattlenetRpcErrorCode.UserServerBadWowAccount;

            if (!_globalSession.AuthClient.IsConnected())
                return BattlenetRpcErrorCode.UtilServerMissingRealmList;

            _globalSession.AuthClient.RequestRealmListAndWait();

            string subRegionId = "";
            Variant subRegion = Params.LookupByKey($"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}");
            if (subRegion != null)
                subRegionId = subRegion.StringValue;

            var compressed = Global.RealmMgr.GetRealmList(_globalSession.Build, subRegionId);
            if (compressed.Length == 0)
                return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

            var attribute = new Bgs.Protocol.Attribute();
            attribute.Name = "Param_RealmList";
            attribute.Value = new Variant();
            attribute.Value.BlobValue = ByteString.CopyFrom(compressed);
            response.Attribute.Add(attribute);

            var realmCharacterCounts = new RealmCharacterCountList();
            foreach (var characterCount in _globalSession.GameAccountInfo.CharacterCounts)
            {
                var countEntry = new RealmCharacterCountEntry();
                countEntry.WowRealmAddress = (int)characterCount.Key;
                countEntry.Count = characterCount.Value;
                realmCharacterCounts.Counts.Add(countEntry);
            }

            compressed = Json.Deflate("JSONRealmCharacterCountList", realmCharacterCounts);

            attribute = new Bgs.Protocol.Attribute();
            attribute.Name = "Param_CharacterCountList";
            attribute.Value = new Variant();
            attribute.Value.BlobValue = ByteString.CopyFrom(compressed);
            response.Attribute.Add(attribute);
            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode JoinRealm(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant realmAddress = Params.LookupByKey("Param_RealmAddress");
            if (realmAddress != null)
                return Global.RealmMgr.JoinRealm(_globalSession, (uint)realmAddress.UintValue, _globalSession.Build, GetRemoteIpEndPoint().Address, _clientSecret, _globalSession.GameAccountInfo.Name, response);

            return BattlenetRpcErrorCode.WowServicesInvalidJoinTicket;
        }
    }
}
