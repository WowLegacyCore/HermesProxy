using HermesProxy.World.Enums;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        [PacketHandler(Opcode.SMSG_GM_TICKET_CREATE)]
        void HandleGmTicketCreate(WorldPacket packet)
        {
            var response = (LegacyGmTicketResponse) packet.ReadUInt32();
            bool isError = !(response is LegacyGmTicketResponse.CreateSuccess or LegacyGmTicketResponse.UpdateSuccess);
            Session.SendHermesTextMessage($"GM Ticket Status: {response}", isError);
        }
    }
}
