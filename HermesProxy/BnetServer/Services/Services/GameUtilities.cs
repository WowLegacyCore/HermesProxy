// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Framework.Constants;
using Framework.Logging;
using Framework.Serialization;
using Framework.Web;
using Google.Protobuf;
using HermesProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.Util;
using HermesProxy.Auth;

namespace BNetServer.Services
{
    public partial class BnetServices
    {
        string GetCommandEndingForVersion()
        {
            if (ModernVersion.GetExpansionVersion() == 1)
                return "c1";
            if (ModernVersion.GetExpansionVersion() == 2)
                return "bcc1";
            return "b9";
        }

        [Service(ServiceRequirement.LoggedIn, OriginalHash.GameUtilitiesService, (uint) GameUtilitiesServiceMethods.GenericClientRequest)]
        BattlenetRpcErrorCode HandleProcessClientRequest(ClientRequest request, ClientResponse response)
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
                ServiceLog(LogType.Error, $"Sent ClientRequest with no command.");
                return BattlenetRpcErrorCode.RpcMalformedRequest;
            }
            ServiceLog(LogType.Debug, $"Received {command.Name}");

            if (command.Name == $"Command_RealmListTicketRequest_v1_{GetCommandEndingForVersion()}")
                return GetRealmListTicket(Params, response);
            if (command.Name == $"Command_LastCharPlayedRequest_v1_{GetCommandEndingForVersion()}")
                return GetLastCharPlayed(Params, response);
            if (command.Name == $"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}")
                return GetRealmList(Params, response);
            if (command.Name == $"Command_RealmJoinRequest_v1_{GetCommandEndingForVersion()}")
                return JoinRealm(Params, response);

            ServiceLog(LogType.Warn, $"Sent unhandled command '{command.Name}'.");
            return BattlenetRpcErrorCode.RpcNotImplemented;
        }

        [Service(ServiceRequirement.LoggedIn, OriginalHash.GameUtilitiesService, (uint) GameUtilitiesServiceMethods.GetAllValuesForAttribute)]
        BattlenetRpcErrorCode HandleGetAllValuesForAttribute(GetAllValuesForAttributeRequest request, GetAllValuesForAttributeResponse response)
        {
            if (request.AttributeKey == $"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}")
            {
                GetSession().AuthClient.WaitForRealmlist();

                GetSession().RealmManager.WriteSubRegions(response);
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
                var gameAccount = GetSession().AccountInfo.GameAccounts.LookupByKey(realmListTicketIdentity.GameAccountId);
                if (gameAccount != null)
                    GetSession().GameAccountInfo = gameAccount;
            }

            if (GetSession().GameAccountInfo == null)
                return BattlenetRpcErrorCode.UtilServerInvalidIdentityArgs;
            if (GetSession().GameAccountInfo.IsPermanenetlyBanned)
                return BattlenetRpcErrorCode.GameAccountBanned;
            if (GetSession().GameAccountInfo.IsBanned)
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

            response.Attribute.AddBlob("Param_RealmListTicket", ByteString.CopyFrom("AuthRealmListTicket", Encoding.UTF8));

            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode GetLastCharPlayed(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant subRegion = Params.LookupByKey($"Command_LastCharPlayedRequest_v1_{GetCommandEndingForVersion()}");
            if (subRegion == null)
                return BattlenetRpcErrorCode.UtilServerUnknownRealm;

            var rawLastPlayedChar = GetSession().AccountMetaDataMgr.GetLastSelectedCharacter();
            if (!rawLastPlayedChar.HasValue)
                return BattlenetRpcErrorCode.Ok;
            var lastPlayedChar = rawLastPlayedChar.Value;

            GetSession().AuthClient.WaitForRealmlist();

            var realm = GetSession().RealmManager.GetRealms().FirstOrDefault(r => r.Name == lastPlayedChar.realmName);
            if (realm == null)
                return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

            byte[] compressedRealmEntry = GetSession().RealmManager.GetCompressdRealmEntryJSON(realm, GetSession().Build);
            if (compressedRealmEntry.Length == 0)
                return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

            response.Attribute.AddBlob("Param_RealmEntry", ByteString.CopyFrom(compressedRealmEntry));
            response.Attribute.AddString("Param_CharacterName", lastPlayedChar.charName);
            response.Attribute.AddBlob("Param_CharacterGUID", ByteString.CopyFrom(BitConverter.GetBytes(lastPlayedChar.charLowerGuid)));
            response.Attribute.AddInt("Param_LastPlayedTime", lastPlayedChar.lastLoginUnixSec);

            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode GetRealmList(Dictionary<string, Variant> Params, ClientResponse response)
        {
            if (GetSession().GameAccountInfo == null)
                return BattlenetRpcErrorCode.UserServerBadWowAccount;

            if (!GetSession().AuthClient.IsConnected())
                return BattlenetRpcErrorCode.UtilServerMissingRealmList;

            string subRegionId = "";
            Variant subRegion = Params.LookupByKey($"Command_RealmListRequest_v1_{GetCommandEndingForVersion()}");
            if (subRegion != null)
                subRegionId = subRegion.StringValue;

            GetSession().AuthClient.WaitForRealmlist();

            var compressedRealmList = GetSession().RealmManager.GetRealmList(GetSession().Build, subRegionId);
            if (compressedRealmList.Length == 0)
                return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

            response.Attribute.AddBlob("Param_RealmList", ByteString.CopyFrom(compressedRealmList));

            var realmCharacterCounts = new RealmCharacterCountList();
            foreach (var realm in GetSession().RealmManager.GetRealms())
            {
                var countEntry = new RealmCharacterCountEntry();
                countEntry.WowRealmAddress = (int) realm.Id.GetAddress();
                countEntry.Count = realm.CharacterCount;
                realmCharacterCounts.Counts.Add(countEntry);
            }

            var compressedCharCount = Json.Deflate("JSONRealmCharacterCountList", realmCharacterCounts);
            response.Attribute.AddBlob("Param_CharacterCountList", ByteString.CopyFrom(compressedCharCount));

            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode JoinRealm(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant realmAddress = Params.LookupByKey("Param_RealmAddress");
            if (realmAddress == null)
                return BattlenetRpcErrorCode.WowServicesInvalidJoinTicket;

            return GetSession().RealmManager.JoinRealm(GetSession(), (uint)realmAddress.UintValue, GetSession().Build, GetRemoteIpEndPoint().Address, _clientSecret, GetSession().GameAccountInfo.Name, response);
        }
    }
}
