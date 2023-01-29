using System;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        [PacketHandler(Opcode.CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT)]
        void HandleSupportTicketSubmitComplaint(SupportTicketSubmitComplaint complaint)
        {
            var targetPlayerName = Session.GameState.GetPlayerName(complaint.TargetCharacterGuid);

            string ticketText = $"I would like to report player '{targetPlayerName}' (id: {complaint.TargetCharacterGuid.GetCounter()}).";
            if (complaint.SelectedMailInfo != null)
            {
                ticketText += "\r\n" + $"Mail in question (id: {complaint.SelectedMailInfo.MailId}) with subject '{complaint.SelectedMailInfo.MailSubject}'";
            }

            if (!complaint.TextNote.IsEmpty())
            {
                ticketText += "\r\n" + "-------------";
                ticketText += "\r\n" + complaint.TextNote;
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_GM_TICKET_CREATE);

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt8(2); // GMTICKET_BEHAVIOR_HARASSMENT
                packet.WriteUInt32(complaint.Header.SelfPlayerMapId);
                packet.WriteVector3(complaint.Header.SelfPlayerPos);
                packet.WriteCString(ticketText);
                packet.WriteCString(""); // Not used
            }
            else
            {
                packet.WriteUInt32(complaint.Header.SelfPlayerMapId);
                packet.WriteVector3(complaint.Header.SelfPlayerPos);
                packet.WriteCString(ticketText);
                packet.WriteUInt32(0); // we dont need the gm to reach back

                packet.WriteUInt32(0); // chat lines count
                packet.WriteUInt32(0); // chat text inflated size
                packet.WriteBytes(Array.Empty<byte>()); // rest of the message are deflated chat lines
            }

            SendPacketToServer(packet);
        }
    }
}
