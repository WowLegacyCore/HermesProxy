using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        [PacketHandler(Opcode.CMSG_GET_ACCOUNT_CHARACTER_LIST)]
        void HandleGetAccountCharacterList(GetAccountCharacterListRequest request)
        {
            GetAccountCharacterListResult response = new();
            response.Token = request.Token;

            foreach (var ownCharacter in GetSession().GameState.OwnCharacters)
            {
                break;
                response.CharacterList.Add(new AccountCharacterListEntry
                {
                    AccountId = ownCharacter.AccountId,
                    CharacterGuid = ownCharacter.CharacterGuid,
                    RealmVirtualAddress = ownCharacter.Realm.Id.GetAddress(),
                    RealmName = "", // If empty the realm name will not be displayed
                    LastLoginUnixSec = ownCharacter.LastLoginUnixSec,

                    Name = ownCharacter.Name,
                    Race = ownCharacter.RaceId,
                    Class = ownCharacter.ClassId,
                    Sex = ownCharacter.SexId,
                    Level = ownCharacter.Level,
                });
            }

            SendPacket(response);
        }
    }
}
