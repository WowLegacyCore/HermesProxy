using System.Security.Cryptography;
using System.Threading.Tasks;

using Bgs.Protocol;
using Bgs.Protocol.Authentication.V1;
using Bgs.Protocol.Challenge.V1;
using Google.Protobuf;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Logging;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.Services;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        [BattlenetService(ServiceHash.AuthenticationService, 1)]
        public async Task<BattlenetRpcErrorCode> HandleLogon(LogonRequest request)
        {
            if (request.Program != "WoW")
            {
                Log.Print(LogType.Error, $"{GetRemoteEndpoint()} attempted to log in with a different game (using {request.Program})");
                return BattlenetRpcErrorCode.BadProgram;
            }

            if (request.Platform != "Wn64")
            {
                Log.Print(LogType.Error, $"{GetRemoteEndpoint()} attempted to log in with a different platform (using {request.Platform})");
                return BattlenetRpcErrorCode.BadPlatform;
            }

            var externalChallenge = new ChallengeExternalRequest
            {
                PayloadType = "web_auth_url",
                Payload = ByteString.CopyFromUtf8($"https://127.0.0.1:8081/bnetserver/login")
            };

            await SendRequest(ServiceHash.ChallengeListener, 3, externalChallenge);

            return BattlenetRpcErrorCode.Ok;
        }

        [BattlenetService(ServiceHash.AuthenticationService, 7)]
        public async Task<BattlenetRpcErrorCode> HandleVerifyWebCredentials(VerifyWebCredentialsRequest request)
        {
            if (request.WebCredentials.IsEmpty)
                return BattlenetRpcErrorCode.Denied;

            var logonResult = new LogonResult
            {
                ErrorCode = 0,
                AccountId = new EntityId
                {
                    Low = 1,
                    High = 0x100000000000000
                },
            };

            var gameAccountId = new EntityId
            {
                Low = 1,
                High = 0x200000200576F57
            };

            logonResult.GameAccountId.Add(gameAccountId);
            logonResult.SessionKey = ByteString.CopyFrom(RandomNumberGenerator.GetBytes(64));

            _authed = true;

            await SendRequest(ServiceHash.AuthenticationListener, 5, logonResult);
            return BattlenetRpcErrorCode.Ok;
        }
    }
}
