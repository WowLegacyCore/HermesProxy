using System;
using Framework.Logging;
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
            response.Token = request.Token;

            if (!GetSession().AuthClient.IsConnected() && GetSession().AuthClient.Reconnect() != AuthResult.SUCCESS)
            {
                Log.Print(LogType.Error, "Failed to reconnect to auth server.");
                response.Allow = false;
                SendPacket(response);
                return;
            }
            
        }
    }
}
