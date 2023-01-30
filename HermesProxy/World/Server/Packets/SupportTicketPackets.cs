using System;
using System.Collections.Generic;
using Framework.GameMath;
using Framework.Logging;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets
{
    public class SupportTicketSubmitComplaint : ClientPacket
    {
        public SupportTicketSubmitComplaint(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Header.Read(_worldPacket);
            TargetCharacterGuid = _worldPacket.ReadPackedGuid128();
            ChatLog.Read(_worldPacket);

            ComplaintType = (GmTicketComplaintType)_worldPacket.ReadBits<uint>(5);

            var noteLength = _worldPacket.ReadBits<uint>(10); // this is somehow set to 120 when reporting mail spam(?)

            var unk0 = _worldPacket.ReadBit(); // Always false?
            var unk1 = _worldPacket.ReadBit(); // Always false?
            var unk2 = _worldPacket.ReadBit(); // Always false?
            var unk3 = _worldPacket.ReadBit(); // Always false?
            var unk4 = _worldPacket.ReadBit(); // Always false?
            var unk5 = _worldPacket.ReadBit(); // Always false?
            var rightClickedMenu = _worldPacket.ReadBit();
            var hasMailInfo = _worldPacket.ReadBit();

            _worldPacket.ResetBitPos();

            var unkZero1 = _worldPacket.ReadUInt8(); // Always 0?
            var unkZero2 = _worldPacket.ReadUInt32(); // Always 0?

            if (unk0 || unk1 || unk2 || unk3 || unk4 || unk5 || unkZero1 != 0 || unkZero2 != 0)
            {
                Log.Print(LogType.Error, "You reported something that we do not handle (?)");
                Log.Print(LogType.Error, "Please create a new issue on GitHub and tell us what you did");
                return;
            }

            if (rightClickedMenu)
            {
                // No data?
            }

            if (hasMailInfo)
            {
                SelectedMailInfo = new MailInfo();
                SelectedMailInfo.Read(_worldPacket);
            }

            TextNote = _worldPacket.ReadString(noteLength);
        }

        public HeaderInfo Header = new();
        public WowGuid128 TargetCharacterGuid;
        public ChatLogInfo ChatLog = new();
        public MailInfo? SelectedMailInfo = null;
        public GmTicketComplaintType ComplaintType;
        public string TextNote;
        
        public class HeaderInfo
        {
            public void Read(WorldPacket worldPacket)
            {
                SelfPlayerMapId = worldPacket.ReadUInt32();
                SelfPlayerPos = worldPacket.ReadVector3();
                SelfPlayerOrientation = worldPacket.ReadFloat();
            }

            public uint SelfPlayerMapId;
            public Vector3 SelfPlayerPos;
            public float SelfPlayerOrientation;
        }

        public class ChatLogInfo
        {
            public void Read(WorldPacket worldPacket)
            {
                var chatLogLineCount = worldPacket.ReadUInt32();
                var hasReportedLineIndex = worldPacket.ReadBit();
                for (var i = 0; i < chatLogLineCount; i++)
                {
                    var time = worldPacket.ReadTime64(); 
                    var textLength = worldPacket.ReadBits<uint>(12);
                    worldPacket.ResetBitPos();
                    var text = worldPacket.ReadString(textLength);
                    ChatLines.Add(new ChatLine
                    {
                        Time = time,
                        Text = text,
                    });
                }

                if (hasReportedLineIndex)
                    ReportedLineIdx = worldPacket.ReadUInt32();
            }

            public List<ChatLine> ChatLines = new();
            public uint? ReportedLineIdx;

            public class ChatLine
            {
                public DateTime Time;
                public string Text;
            }
        }

        public class MailInfo
        {
            public void Read(WorldPacket worldPacket)
            {
                MailId = worldPacket.ReadUInt32();
                
                var textBodyLength = worldPacket.ReadBits<uint>(13);
                var subjectLength = worldPacket.ReadBits<uint>(9);
                worldPacket.ResetBitPos();

                MailTextBody = worldPacket.ReadString(textBodyLength);
                MailSubject = worldPacket.ReadString(subjectLength);
            }
            
            public uint MailId;
            public string MailTextBody;
            public string MailSubject;
        }
    }
}
