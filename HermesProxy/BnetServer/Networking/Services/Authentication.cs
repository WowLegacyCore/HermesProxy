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

namespace BNetServer.Networking
{
    public partial class Session
    {
        [Service(OriginalHash.AuthenticationService, 1)]
        BattlenetRpcErrorCode HandleLogon(LogonRequest logonRequest, NoData response)
        {
            if (logonRequest.Program != "WoW")
            {
                Log.Print(LogType.Error, $"Battlenet.LogonRequest: {GetClientInfo()} attempted to log in with game other than WoW (using {logonRequest.Program})!");
                return BattlenetRpcErrorCode.BadProgram;
            }

            if (logonRequest.Platform != "Win" && logonRequest.Platform != "Wn64" && logonRequest.Platform != "Mc64")
            {
                Log.Print(LogType.Error, $"Battlenet.LogonRequest: {GetClientInfo()} attempted to log in from an unsupported platform (using {logonRequest.Platform})!");
                return BattlenetRpcErrorCode.BadPlatform;
            }

            if (!LocaleChecker.IsValidLocale(logonRequest.Locale.ToEnum<Locale>()))
            {
                Log.Print(LogType.Error, $"Battlenet.LogonRequest: {GetClientInfo()} attempted to log in with unsupported locale (using {logonRequest.Locale})!");
                return BattlenetRpcErrorCode.BadLocale;
            }

            var endpoint = Global.LoginServiceMgr.GetAddressForClient(GetRemoteIpEndPoint().Address);

            ChallengeExternalRequest externalChallenge = new();
            externalChallenge.PayloadType = "web_auth_url";            
            externalChallenge.Payload = ByteString.CopyFromUtf8($"https://{endpoint.Address}:{endpoint.Port}/bnetserver/login/{logonRequest.Platform}/{logonRequest.ApplicationVersion}/{logonRequest.Locale}/");

            SendRequest((uint)OriginalHash.ChallengeListener, 3, externalChallenge);
            return BattlenetRpcErrorCode.Ok;
        }

        [Service(OriginalHash.AuthenticationService, 7)]
        BattlenetRpcErrorCode HandleVerifyWebCredentials(VerifyWebCredentialsRequest verifyWebCredentialsRequest)
        {
            globalSession = Global.SessionsByTicket[verifyWebCredentialsRequest.WebCredentials.ToStringUtf8()];
            globalSession.AccountInfo = new AccountInfo(globalSession.Username);

            if (globalSession.AccountInfo.LoginTicketExpiry < Time.UnixTime)
            {
                return BattlenetRpcErrorCode.TimedOut;
            }

            string ip_address = GetRemoteIpEndPoint().ToString();

            // If the account is banned, reject the logon attempt
            if (globalSession.AccountInfo.IsBanned)
            {
                if (globalSession.AccountInfo.IsPermanenetlyBanned)
                {
                    Log.Print(LogType.Debug, $"{GetClientInfo()} Session.HandleVerifyWebCredentials: Banned account {globalSession.AccountInfo.Login} tried to login!");
                    return BattlenetRpcErrorCode.GameAccountBanned;
                }
                else
                {
                    Log.Print(LogType.Debug, $"{GetClientInfo()} Session.HandleVerifyWebCredentials: Temporarily banned account {globalSession.AccountInfo.Login} tried to login!");
                    return BattlenetRpcErrorCode.GameAccountSuspended;
                }
            }

            LogonResult logonResult = new();
            logonResult.ErrorCode = 0;
            logonResult.AccountId = new EntityId();
            logonResult.AccountId.Low = globalSession.AccountInfo.Id;
            logonResult.AccountId.High = 0x100000000000000;
            foreach (var pair in globalSession.AccountInfo.GameAccounts)
            {
                EntityId gameAccountId = new();
                gameAccountId.Low = pair.Value.Id;
                gameAccountId.High = 0x200000200576F57;
                logonResult.GameAccountId.Add(gameAccountId);
            }

            globalSession.SessionKey = new byte[64].GenerateRandomKey(64);
            logonResult.SessionKey = ByteString.CopyFrom(globalSession.SessionKey);

            authed = true;

            SendRequest((uint)OriginalHash.AuthenticationListener, 5, logonResult);
            return BattlenetRpcErrorCode.Ok;
        }
    }
}