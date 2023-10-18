// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Bgs.Protocol;
using Bgs.Protocol.Authentication.V1;
using Bgs.Protocol.Challenge.V1;
using Framework.Constants;
using Framework.Logging;
using Framework.Realm;
using Google.Protobuf;
using System;
using BNetServer.Networking;

namespace BNetServer.Services
{
    public partial class BnetServices
    {
        [Service(ServiceRequirement.Unauthorized, OriginalHash.AuthenticationService, 1)]
        BattlenetRpcErrorCode HandleLogon(LogonRequest logonRequest, NoData response)
        {
            if (logonRequest.Program != "WoW")
            {
                ServiceLog(LogType.Error, $"Battlenet.LogonRequest: Attempted to log in with game other than WoW (using {logonRequest.Program})!");
                return BattlenetRpcErrorCode.BadProgram;
            }

            if (logonRequest.ApplicationVersion != HermesProxy.ModernVersion.BuildInt)
            {
                ServiceLog(LogType.Error, $"Battlenet.LogonRequest: Attempted to log in with wrong game version (using {logonRequest.ApplicationVersion})!");
                return BattlenetRpcErrorCode.BadVersion;
            }

            if (logonRequest.Platform != "Win" && logonRequest.Platform != "Wn64" && logonRequest.Platform != "Mc64" && logonRequest.Platform != "MacA")
            {
                ServiceLog(LogType.Error, $"Battlenet.LogonRequest: Attempted to log in from an unsupported platform (using {logonRequest.Platform})!");
                return BattlenetRpcErrorCode.BadPlatform;
            }

            if (!LocaleChecker.IsValidLocale(logonRequest.Locale.ToEnum<Locale>()))
            {
                ServiceLog(LogType.Error, $"Battlenet.LogonRequest: Attempted to log in with unsupported locale (using {logonRequest.Locale})!");
                return BattlenetRpcErrorCode.BadLocale;
            }

            var endpoint = LoginServiceManager.Instance.GetAddressForClient(GetRemoteIpEndPoint().Address);

            ChallengeExternalRequest externalChallenge = new();
            externalChallenge.PayloadType = "web_auth_url";            
            externalChallenge.Payload = ByteString.CopyFromUtf8($"https://{endpoint.Address}:{endpoint.Port}/bnetserver/login/{logonRequest.Platform}/{logonRequest.ApplicationVersion}/{logonRequest.Locale}/");

            SendRequest(OriginalHash.ChallengeListener, 3, externalChallenge);
            return BattlenetRpcErrorCode.Ok;
        }

        [Service(ServiceRequirement.Unauthorized, OriginalHash.AuthenticationService, 7)]
        BattlenetRpcErrorCode HandleVerifyWebCredentials(VerifyWebCredentialsRequest verifyWebCredentialsRequest)
        {
            if (!BnetSessionTicketStorage.SessionsByTicket.TryGetValue(verifyWebCredentialsRequest.WebCredentials.ToStringUtf8(), out var tmpSession))
                return BattlenetRpcErrorCode.Denied;

            tmpSession.AccountInfo = new AccountInfo(tmpSession.Username);

            if (tmpSession.AccountInfo.LoginTicketExpiry < Time.UnixTime)
            {
                return BattlenetRpcErrorCode.TimedOut;
            }

            // If the account is banned, reject the logon attempt
            if (tmpSession.AccountInfo.IsBanned)
            {
                if (tmpSession.AccountInfo.IsPermanenetlyBanned)
                {
                    ServiceLog(LogType.Debug, $"Session.HandleVerifyWebCredentials: Banned account {tmpSession.AccountInfo.Login} tried to login!");
                    return BattlenetRpcErrorCode.GameAccountBanned;
                }
                else
                {
                    ServiceLog(LogType.Debug, $"Session.HandleVerifyWebCredentials: Temporarily banned account {tmpSession.AccountInfo.Login} tried to login!");
                    return BattlenetRpcErrorCode.GameAccountSuspended;
                }
            }

            LogonResult logonResult = new();
            logonResult.ErrorCode = 0;
            logonResult.AccountId = new EntityId();
            logonResult.AccountId.Low = tmpSession.AccountInfo.Id;
            logonResult.AccountId.High = 0x100000000000000; // Some magic high guid?
            foreach (var gameAccount in tmpSession.AccountInfo.GameAccounts.Values)
            {
                EntityId gameAccountId = new();
                gameAccountId.Low = gameAccount.Id;
                gameAccountId.High = 0x200000200576F51; // Some magic high guid? When using HighGuid of 703 client disconnects
                logonResult.GameAccountId.Add(gameAccountId);
            }

            tmpSession.SessionKey = new byte[64].GenerateRandomKey(64);
            logonResult.SessionKey = ByteString.CopyFrom(tmpSession.SessionKey);

            _globalSession = tmpSession;

            SendRequest(OriginalHash.AuthenticationListener, 5, logonResult);
            return BattlenetRpcErrorCode.Ok;
        }
    }
}
