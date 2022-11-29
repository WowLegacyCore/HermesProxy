using Framework.Constants;
using Framework.IO;
using Framework.Logging;
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
            ChangeRealmTicketResponse response = new()
            {
                Token = request.Token
            };

            if (GetSession().AuthClient.Reconnect() != AuthResult.SUCCESS)
            {
                response.Allow = false;
                SendPacket(response);
                return;
            }
            GetSession().AuthClient.RequestRealmListUpdate();

            _bnetRpc.SetClientSecret(request.Secret);

            response.Allow = true;
            response.Ticket = new ByteBuffer(new byte[1]);

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
                serviceId: 0,
                (OriginalHash)request.Method.GetServiceHash(),
                request.Method.GetMethodId(),
                request.Method.Token,
                new CodedInputStream(request.Data)
            );
        }
    }
}
