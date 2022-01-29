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
            Console.WriteLine("Entering HandleLogon");

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

            locale = logonRequest.Locale;
            os = logonRequest.Platform;
            build = (uint)logonRequest.ApplicationVersion;

            var endpoint = Global.LoginServiceMgr.GetAddressForClient(GetRemoteIpEndPoint().Address);

            ChallengeExternalRequest externalChallenge = new();
            externalChallenge.PayloadType = "web_auth_url";            
            externalChallenge.Payload = ByteString.CopyFromUtf8($"https://{endpoint.Address}:{endpoint.Port}/bnetserver/login/");

            Console.WriteLine("HandleLogon returns Ok");
            SendRequest((uint)OriginalHash.ChallengeListener, 3, externalChallenge);
            return BattlenetRpcErrorCode.Ok;
        }

        [Service(OriginalHash.AuthenticationService, 7)]
        BattlenetRpcErrorCode HandleVerifyWebCredentials(VerifyWebCredentialsRequest verifyWebCredentialsRequest)
        {
            Console.WriteLine("Entering HandleVerifyWebCredentials");

            if (verifyWebCredentialsRequest.WebCredentials.IsEmpty)
            {
                Console.WriteLine("HandleVerifyWebCredentials returns Denied");
                //return BattlenetRpcErrorCode.Denied;
            }

            accountInfo = new AccountInfo();

            if (accountInfo.LoginTicketExpiry < Time.UnixTime)
            {
                Console.WriteLine("HandleVerifyWebCredentials returns TimedOut");
                return BattlenetRpcErrorCode.TimedOut;
            }

            string ip_address = GetRemoteIpEndPoint().ToString();

            // If the IP is 'locked', check that the player comes indeed from the correct IP address
            if (accountInfo.IsLockedToIP)
            {
                Log.Print(LogType.Debug, $"Session.HandleVerifyWebCredentials: Account: {accountInfo.Login} is locked to IP: {accountInfo.LastIP} is logging in from IP: {ip_address}");

                if (accountInfo.LastIP != ip_address)
                    return BattlenetRpcErrorCode.RiskAccountLocked;
            }
            else
            {
                Log.Print(LogType.Debug, $"Session.HandleVerifyWebCredentials: Account: {accountInfo.Login} is not locked to ip");
                if (accountInfo.LockCountry.IsEmpty() || accountInfo.LockCountry == "00")
                    Log.Print(LogType.Debug, $"Session.HandleVerifyWebCredentials: Account: {accountInfo.Login} is not locked to country");
                else if (!accountInfo.LockCountry.IsEmpty() && !ipCountry.IsEmpty())
                {
                    Log.Print(LogType.Debug, $"Session.HandleVerifyWebCredentials: Account: {accountInfo.Login} is locked to Country: {accountInfo.LockCountry} player Country: {ipCountry}");

                    if (ipCountry != accountInfo.LockCountry)
                        return BattlenetRpcErrorCode.RiskAccountLocked;
                }
            }

            // If the account is banned, reject the logon attempt
            if (accountInfo.IsBanned)
            {
                if (accountInfo.IsPermanenetlyBanned)
                {
                    Log.Print(LogType.Debug, $"{GetClientInfo()} Session.HandleVerifyWebCredentials: Banned account {accountInfo.Login} tried to login!");
                    return BattlenetRpcErrorCode.GameAccountBanned;
                }
                else
                {
                    Log.Print(LogType.Debug, $"{GetClientInfo()} Session.HandleVerifyWebCredentials: Temporarily banned account {accountInfo.Login} tried to login!");
                    return BattlenetRpcErrorCode.GameAccountSuspended;
                }
            }

            LogonResult logonResult = new();
            logonResult.ErrorCode = 0;
            logonResult.AccountId = new EntityId();
            logonResult.AccountId.Low = accountInfo.Id;
            logonResult.AccountId.High = 0x100000000000000;
            foreach (var pair in accountInfo.GameAccounts)
            {
                EntityId gameAccountId = new();
                gameAccountId.Low = pair.Value.Id;
                gameAccountId.High = 0x200000200576F57;
                logonResult.GameAccountId.Add(gameAccountId);
            }

            if (!ipCountry.IsEmpty())
                logonResult.GeoipCountry = ipCountry;

            logonResult.SessionKey = ByteString.CopyFrom(new byte[64].GenerateRandomKey(64));

            authed = true;

            Console.WriteLine("HandleVerifyWebCredentials returns Ok");
            SendRequest((uint)OriginalHash.AuthenticationListener, 5, logonResult);
            return BattlenetRpcErrorCode.Ok;
        }
    }
}