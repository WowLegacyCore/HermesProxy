using Bgs.Protocol.Account.V1;

using HermesProxy.Framework.Constants;
using HermesProxy.Network.BattleNet.Services;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        [BattlenetService(ServiceHash.AccountService, 30)]
        public BattlenetRpcErrorCode HandleGetAccountState(GetAccountStateRequest request, GetAccountStateResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            if (request.Options.FieldPrivacyInfo)
            {
                response.State = new AccountState
                {
                    PrivacyInfo = new PrivacyInfo
                    {
                        IsUsingRid = true,
                        IsVisibleForViewFriends = true,
                        IsHiddenFromFriendFinder = false,
                    }
                };
                response.Tags = new AccountFieldTags { PrivacyInfoTag = 0xD7CA834D };
            }

            return BattlenetRpcErrorCode.Ok;
        }

        [BattlenetService(ServiceHash.AccountService, 31)]
        public BattlenetRpcErrorCode HandleGetGameAccountState(GetGameAccountStateRequest request, GetGameAccountStateResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            if (request.Options.FieldGameLevelInfo)
            {
                response.State = new GameAccountState
                {
                    GameLevelInfo = new GameLevelInfo
                    {
                        Name = "Wow1",
                        Program = 5730135
                    }
                };
                response.Tags = new GameAccountFieldTags { GameLevelInfoTag = 0x5C46D483 };
            }

            if (request.Options.FieldGameStatus)
            {
                if (response.State == null)
                    response.State = new GameAccountState();

                response.State.GameStatus = new GameStatus
                {
                    IsSuspended = false,
                    IsBanned = false,
                    SuspensionExpires = 0 * 10000000,
                    Program = 5730135
                };
                response.Tags.GameStatusTag = 0x98B75F99;
            }

            return BattlenetRpcErrorCode.Ok;
        }
    }
}
