using HermesProxy.World.Enums;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        [PacketHandler(Opcode.SMSG_GM_TICKET_CREATE)]
        void HandleGmTicketCreate(WorldPacket packet)
        {
            var response = (LegacyGmTicketResponse) packet.ReadUInt32();
            switch (response)
            {
                case LegacyGmTicketResponse.CreateSuccess:
                case LegacyGmTicketResponse.UpdateSuccess:
                    Session.SendSystemTextMessage($"GM Ticket Status: |cFF00FF00{response}|r");
                    break;
                default:
                    Session.SendSystemTextMessage($"GM Ticket Status: |cFFFF0000{response}|r");
                    break;
            }
        }
    }
}
