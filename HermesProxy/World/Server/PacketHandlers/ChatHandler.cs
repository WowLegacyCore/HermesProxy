using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_CHAT_JOIN_CHANNEL)]
        void HandleChatJoinChannel(JoinChannel join)
        {
            if (GetSession().WorldClient != null)
                GetSession().WorldClient.SendChatJoinChannel(join.ChatChannelId, join.ChannelName, join.Password);
        }

        [PacketHandler(Opcode.CMSG_CHAT_LEAVE_CHANNEL)]
        void HandleChatLeaveChannel(LeaveChannel leave)
        {
            if (GetSession().WorldClient != null)
            {
                GetSession().GameState.LeftChannelName = leave.ChannelName;
                GetSession().WorldClient.SendChatLeaveChannel(leave.ZoneChannelID, leave.ChannelName);
            }
        }

        [PacketHandler(Opcode.CMSG_CHAT_CHANNEL_OWNER)]
        [PacketHandler(Opcode.CMSG_CHAT_CHANNEL_ANNOUNCEMENTS)]

        void HandleChatChannelCommand(ChannelCommand command)
        {
            WorldPacket packet = new(command.GetUniversalOpcode());
            packet.WriteCString(command.ChannelName);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CHAT_CHANNEL_LIST)]
        void HandleChatChannelList(ChannelCommand command)
        {
            WorldPacket packet = new(Opcode.CMSG_CHAT_CHANNEL_LIST);
            packet.WriteCString(command.ChannelName);
            SendPacketToServer(packet);
            GetSession().GameState.ChannelDisplayList = false;
        }

        [PacketHandler(Opcode.CMSG_CHAT_CHANNEL_DISPLAY_LIST)]
        void HandleChatChannelDisplayList(ChannelCommand command)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                WorldPacket packet = new(Opcode.CMSG_CHAT_CHANNEL_LIST);
                packet.WriteCString(command.ChannelName);
                SendPacketToServer(packet);
            }
            else
            {
                WorldPacket packet = new(Opcode.CMSG_CHAT_CHANNEL_DISPLAY_LIST);
                packet.WriteCString(command.ChannelName);
                SendPacketToServer(packet);
            }
            GetSession().GameState.ChannelDisplayList = true;
        }

        [PacketHandler(Opcode.CMSG_CHAT_CHANNEL_DECLINE_INVITE)]
        void HandleChatChannelDeclineInvite(ChannelCommand command)
        {
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                return;

            WorldPacket packet = new(Opcode.CMSG_CHAT_CHANNEL_DECLINE_INVITE);
            packet.WriteCString(command.ChannelName);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_AFK)]
        void HandleChatMessageAFK(ChatMessageAFK afk)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Afk, 0, afk.Text, "", "");
            else
                GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Afk, 0, afk.Text, "", "");
        }

        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_DND)]
        void HandleChatMessageDND(ChatMessageDND dnd)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Dnd, 0, dnd.Text, "", "");
            else
                GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Dnd, 0, dnd.Text, "", "");
        }

        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_CHANNEL)]
        void HandleChatMessageChannel(ChatMessageChannel channel)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Channel, channel.Language, channel.Text, channel.Target, "");
            else
                GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Channel, channel.Language, channel.Text, channel.Target, "");
        }

        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_WHISPER)]
        void HandleChatMessageWhisper(ChatMessageWhisper whisper)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Whisper, whisper.Language, whisper.Text, "", whisper.Target);
            else
                GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Whisper, whisper.Language, whisper.Text, "", whisper.Target);
        }

        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_EMOTE)]
        void HandleChatMessageWhisper(ChatMessageEmote emote)
        {
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Emote, 0, emote.Text, "", "");
            else
                GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Emote, 0, emote.Text, "", "");
        }

        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_GUILD)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_OFFICER)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_PARTY)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_RAID)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_RAID_WARNING)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_SAY)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_YELL)]
        [PacketHandler(Opcode.CMSG_CHAT_MESSAGE_INSTANCE_CHAT)]
        void HandleChatMessage(ChatMessage packet)
        {
            ChatMessageTypeModern type;

            switch (packet.GetUniversalOpcode())
            {
                case Opcode.CMSG_CHAT_MESSAGE_SAY:
                    type = ChatMessageTypeModern.Say;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_YELL:
                    type = ChatMessageTypeModern.Yell;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_GUILD:
                    type = ChatMessageTypeModern.Guild;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_OFFICER:
                    type = ChatMessageTypeModern.Officer;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_PARTY:
                    type = ChatMessageTypeModern.Party;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_RAID:
                    type = ChatMessageTypeModern.Raid;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_RAID_WARNING:
                    type = ChatMessageTypeModern.RaidWarning;
                    break;
                case Opcode.CMSG_CHAT_MESSAGE_INSTANCE_CHAT:
                    if (GetSession().GameState.IsInBattleground())
                        type = ChatMessageTypeModern.Battleground;
                    else
                        type = ChatMessageTypeModern.Party;
                    break;
                default:
                    Log.Print(LogType.Error, $"HandleMessagechatOpcode : Unknown chat opcode ({packet.GetOpcode()})");
                    return;
            }

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                ChatMessageTypeWotLK chatMsg = (ChatMessageTypeWotLK)Enum.Parse(typeof(ChatMessageTypeWotLK), type.ToString());
                GetSession().WorldClient.SendMessageChatWotLK(chatMsg, packet.Language, packet.Text, "", "");
            }
            else
            {
                ChatMessageTypeVanilla chatMsg = (ChatMessageTypeVanilla)Enum.Parse(typeof(ChatMessageTypeVanilla), type.ToString());
                GetSession().WorldClient.SendMessageChatVanilla(chatMsg, packet.Language, packet.Text, "", "");
            }
        }

        [PacketHandler(Opcode.CMSG_CHAT_ADDON_MESSAGE)]
        void HandleAddonMessage(ChatAddonMessage packet)
        {
            uint language = (uint)Language.Addon;
            string text = packet.Params.Prefix + '\t' + packet.Params.Text;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                ChatMessageTypeWotLK chatMsg = (ChatMessageTypeWotLK)Enum.Parse(typeof(ChatMessageTypeWotLK), packet.Params.Type.ToString());
                GetSession().WorldClient.SendMessageChatWotLK(chatMsg, language, text, "", "");
            }
            else
            {
                ChatMessageTypeVanilla chatMsg = (ChatMessageTypeVanilla)Enum.Parse(typeof(ChatMessageTypeVanilla), packet.Params.Type.ToString());
                GetSession().WorldClient.SendMessageChatVanilla(chatMsg, language, text, "", "");
            }
        }

        [PacketHandler(Opcode.CMSG_CHAT_ADDON_MESSAGE_TARGETED)]
        void HandleAddonMessageTargeted(ChatAddonMessageTargeted packet)
        {
            uint language = (uint)Language.Addon;
            string text = packet.Params.Prefix + '\t' + packet.Params.Text;
            string channelName = packet.ChannelGuid.IsEmpty() ? "" :
                GetSession().GameState.GetChannelName((int)packet.ChannelGuid.GetCounter());

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                ChatMessageTypeWotLK chatMsg = (ChatMessageTypeWotLK)Enum.Parse(typeof(ChatMessageTypeWotLK), packet.Params.Type.ToString());
                GetSession().WorldClient.SendMessageChatWotLK(chatMsg, language, text, channelName, packet.Target);
            }
            else
            {
                ChatMessageTypeVanilla chatMsg = (ChatMessageTypeVanilla)Enum.Parse(typeof(ChatMessageTypeVanilla), packet.Params.Type.ToString());
                GetSession().WorldClient.SendMessageChatVanilla(chatMsg, language, text, channelName, packet.Target);
            }
        }

        [PacketHandler(Opcode.CMSG_SEND_TEXT_EMOTE)]
        void HandleSendTextEmote(CTextEmote emote)
        {
            WorldPacket packet = new(Opcode.CMSG_SEND_TEXT_EMOTE);
            packet.WriteInt32(emote.EmoteID);
            packet.WriteInt32(emote.SoundIndex);
            packet.WriteGuid(emote.Target.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_CHAT_REGISTER_ADDON_PREFIXES)]
        void HandleChatRegisterAddonPrefixes(ChatRegisterAddonPrefixes addons)
        {
            foreach (var prefix in addons.Prefixes)
                GetSession().GameState.AddonPrefixes.Add(prefix);
        }

        [PacketHandler(Opcode.CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES)]
        void HandleChatUnregisterAllAddonPrefixes(EmptyClientPacket addons)
        {
            GetSession().GameState.AddonPrefixes.Clear();
        }
    }
}
