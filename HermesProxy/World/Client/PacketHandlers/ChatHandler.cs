using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Globalization;
using Framework.Logging;
using static HermesProxy.World.Server.Packets.ChannelListResponse;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_CHANNEL_NOTIFY)]
        void HandleChannelNotify(WorldPacket packet)
        {
            ChatNotify type = (ChatNotify)packet.ReadUInt8();

            if (type == ChatNotify.InvalidName)           // hack, because of some silly reason this type
                packet.ReadBytes(3);                      // has 3 null bytes before the invalid channel name

            string channelName = packet.ReadCString();

            switch (type)
            {
                case ChatNotify.PlayerAlreadyMember:
                case ChatNotify.Invite:
                case ChatNotify.ModerationOn:
                case ChatNotify.ModerationOff:
                case ChatNotify.AnnouncementsOn:
                case ChatNotify.AnnouncementsOff:
                case ChatNotify.PasswordChanged:
                case ChatNotify.OwnerChanged:
                case ChatNotify.Joined:
                case ChatNotify.Left:
                case ChatNotify.VoiceOn:
                case ChatNotify.VoiceOff:
                {
                    packet.ReadGuid();
                    break;
                }
                case ChatNotify.YouJoined:
                {
                    ChannelFlags flags;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        flags = (ChannelFlags)packet.ReadUInt8();
                    else
                        flags = (ChannelFlags)packet.ReadUInt32();
                    int channelId = packet.ReadInt32();
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                        packet.ReadInt32(); // unk

                    if (channelId == 0)
                        channelId = (int)GameData.GetChatChannelIdFromName(channelName);

                    GetSession().GameState.SetChannelId(channelName, channelId);

                    ChannelNotifyJoined joined = new ChannelNotifyJoined();
                    joined.Channel = channelName;
                    joined.ChannelFlags = flags;
                    joined.ChatChannelID = channelId;
                    joined.ChannelGUID = WowGuid128.Create(HighGuidType703.ChatChannel, (uint)GetSession().GameState.CurrentMapId, (uint)GetSession().GameState.CurrentZoneId, (ulong)channelId);
                    SendPacketToClient(joined);

                    break;
                }
                case ChatNotify.YouLeft:
                {
                    ChannelNotifyLeft left = new ChannelNotifyLeft();
                    left.Channel = channelName;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        left.ChatChannelID = packet.ReadInt32();
                        left.Suspended = packet.ReadBool(); // Banned?
                    }
                    else
                    {
                        left.ChatChannelID = GetSession().GameState.ChannelIds[channelName];
                        left.Suspended = false;
                    }

                    // do not send leave notification for default channels when changing zones
                    if (String.Equals(GetSession().GameState.LeftChannelName, channelName) ||
                        GameData.GetChatChannelIdFromName(channelName) == 0)
                        SendPacketToClient(left);
                    break;
                }
                case ChatNotify.PlayerNotFound:
                case ChatNotify.ChannelOwner:
                case ChatNotify.PlayerNotBanned:
                case ChatNotify.PlayerInvited:
                case ChatNotify.PlayerInviteBanned:
                {
                    packet.ReadCString(); // Player Name
                    break;
                }
                case ChatNotify.ModeChange:
                {
                    packet.ReadGuid();
                    packet.ReadUInt8(); // Old ChannelMemberFlag
                    packet.ReadUInt8(); // New ChannelMemberFlag
                    break;
                }
                case ChatNotify.PlayerKicked:
                case ChatNotify.PlayerBanned:
                case ChatNotify.PlayerUnbanned:
                {
                    packet.ReadGuid(); // Bad
                    packet.ReadGuid(); // Good
                    break;
                }
                case ChatNotify.TrialRestricted:
                {
                    packet.ReadGuid();
                    break;
                }
                case ChatNotify.WrongPassword:
                case ChatNotify.NotMember:
                case ChatNotify.NotModerator:
                case ChatNotify.NotOwner:
                case ChatNotify.Muted:
                case ChatNotify.Banned:
                case ChatNotify.InviteWrongFaction:
                case ChatNotify.WrongFaction:
                case ChatNotify.InvalidName:
                case ChatNotify.NotModerated:
                case ChatNotify.Throttled:
                case ChatNotify.NotInArea:
                case ChatNotify.NotInLfg:
                    break;
            }
        }

        [PacketHandler(Opcode.SMSG_CHANNEL_LIST)]
        void HandleChannelList(WorldPacket packet)
        {
            ChannelListResponse list = new ChannelListResponse();
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                list.Display = packet.ReadBool();
            else
                list.Display = GetSession().GameState.ChannelDisplayList;
            list.ChannelName = packet.ReadCString();
            list.ChannelFlags = (ChannelFlags)packet.ReadUInt8();
            int count = packet.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ChannelPlayer member = new ChannelPlayer();
                member.Guid = packet.ReadGuid().To128(GetSession().GameState);
                member.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                member.Flags = packet.ReadUInt8();
                list.Members.Add(member);
            }
            SendPacketToClient(list);
        }

        [PacketHandler(Opcode.SMSG_CHAT, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
        void HandleServerChatMessageVanilla(WorldPacket packet)
        {
            ChatMessageTypeVanilla chatType = (ChatMessageTypeVanilla)packet.ReadUInt8();
            uint language = packet.ReadUInt32();
            string senderName = "";
            WowGuid128 sender = null;
            WowGuid128 receiver = null;
            string channelName = "";

            switch (chatType)
            {
                case ChatMessageTypeVanilla.MonsterWhisper:
                //case CHAT_MSG_RAID_BOSS_WHISPER:
                case ChatMessageTypeVanilla.RaidBossEmote:
                case ChatMessageTypeVanilla.MonsterEmote:
                    packet.ReadUInt32(); // Sender Name Length
                    senderName = packet.ReadCString();
                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    break;
                case ChatMessageTypeVanilla.Say:
                case ChatMessageTypeVanilla.Party:
                case ChatMessageTypeVanilla.Yell:
                    sender = packet.ReadGuid().To128(GetSession().GameState);
                    packet.ReadGuid(); // Sender Guid again
                    break;
                case ChatMessageTypeVanilla.MonsterSay:
                case ChatMessageTypeVanilla.MonsterYell:
                    sender = packet.ReadGuid().To128(GetSession().GameState);
                    packet.ReadUInt32(); // Sender Name Length
                    senderName = packet.ReadCString();
                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    break;

                case ChatMessageTypeVanilla.Channel:
                    channelName = packet.ReadCString();
                    packet.ReadUInt32(); // Player Rank
                    sender = packet.ReadGuid().To128(GetSession().GameState);
                    break;
                default:
                    sender = packet.ReadGuid().To128(GetSession().GameState);
                    break;
            }

            switch (chatType)
            {
                case ChatMessageTypeVanilla.BattlegroundAlliance:
                case ChatMessageTypeVanilla.BattlegroundHorde:
                    Utility.Swap(ref sender, ref receiver);
                    break;
            }

            uint textLength = packet.ReadUInt32();
            string text = packet.ReadString(textLength);
            var chatTag = (ChatTag)packet.ReadUInt8();
            var chatFlags = (ChatFlags)Enum.Parse(typeof(ChatFlags), chatTag.ToString());

            if (Session.GameState.IgnoredPlayers.Contains(sender) && !chatFlags.HasFlag(ChatFlags.GM) && chatType != ChatMessageTypeVanilla.Ignored)
            {
                if (chatType == ChatMessageTypeVanilla.Whisper)
                { // In legacy versions the client handled the ignore itself and also sends a "You are ignored" message back.
                    WorldPacket ignoreResponsePacket = new WorldPacket(Opcode.CMSG_CHAT_REPORT_IGNORED);
                    ignoreResponsePacket.WriteGuid(sender!.To64());
                    SendPacketToServer(ignoreResponsePacket);
                }
                return;
            }
            
            string addonPrefix = "";
            if (!ChatPkt.CheckAddonPrefix(GetSession().GameState.AddonPrefixes, ref language, ref text, ref addonPrefix))
                return;

            ChatMessageTypeModern chatTypeModern = (ChatMessageTypeModern)Enum.Parse(typeof(ChatMessageTypeModern), chatType.ToString());
            ChatPkt chat = new ChatPkt(GetSession(), chatTypeModern, text, language, sender, senderName, receiver, "", channelName, chatFlags, addonPrefix);
            SendPacketToClient(chat);
        }

        [PacketHandler(Opcode.SMSG_CHAT, ClientVersionBuild.V2_0_1_6180)]
        [PacketHandler(Opcode.SMSG_GM_MESSAGECHAT, ClientVersionBuild.V2_0_1_6180)]
        void HandleServerChatMessageWotLK(WorldPacket packet)
        {
            ChatMessageTypeWotLK chatType = (ChatMessageTypeWotLK)packet.ReadUInt8();
            uint language = packet.ReadUInt32();
            WowGuid128 sender = packet.ReadGuid().To128(GetSession().GameState);
            string senderName = "";
            WowGuid128 receiver;
            string receiverName = "";
            string channelName = "";

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_1_0_6692))
                packet.ReadInt32(); // Constant time

            switch (chatType)
            {
                case ChatMessageTypeWotLK.Achievement:
                case ChatMessageTypeWotLK.GuildAchievement:
                {
                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    break;
                }
                case ChatMessageTypeWotLK.WhisperForeign:
                {
                    uint senderNameLength = packet.ReadUInt32();
                    senderName = packet.ReadString(senderNameLength);
                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    break;
                }
                case ChatMessageTypeWotLK.BattlegroundNeutral:
                case ChatMessageTypeWotLK.BattlegroundAlliance:
                case ChatMessageTypeWotLK.BattlegroundHorde:
                {
                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    switch (receiver.GetHighType())
                    {
                        case HighGuidType.Creature:
                        case HighGuidType.Vehicle:
                        case HighGuidType.GameObject:
                        case HighGuidType.Transport:
                        case HighGuidType.Pet:
                            uint senderNameLength = packet.ReadUInt32();
                            senderName = packet.ReadString(senderNameLength);
                            break;
                    }
                    break;
                }
                case ChatMessageTypeWotLK.MonsterSay:
                case ChatMessageTypeWotLK.MonsterYell:
                case ChatMessageTypeWotLK.MonsterParty:
                case ChatMessageTypeWotLK.MonsterEmote:
                case ChatMessageTypeWotLK.MonsterWhisper:
                case ChatMessageTypeWotLK.RaidBossEmote:
                case ChatMessageTypeWotLK.RaidBossWhisper:
                case ChatMessageTypeWotLK.BattleNet:
                {
                    uint senderNameLength = packet.ReadUInt32();
                    senderName = packet.ReadString(senderNameLength);
                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    switch (receiver.GetHighType())
                    {
                        case HighGuidType.Creature:
                        case HighGuidType.Vehicle:
                        case HighGuidType.GameObject:
                        case HighGuidType.Transport:
                            uint receiverNameLength = packet.ReadUInt32();
                            receiverName = packet.ReadString(receiverNameLength);
                            break;
                    }
                    break;
                }
                default:
                {
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) &&
                        packet.GetUniversalOpcode(false) == Opcode.SMSG_GM_MESSAGECHAT)
                    {
                        uint gmNameLength = packet.ReadUInt32();
                        packet.ReadString(gmNameLength);
                    }

                    if (chatType == ChatMessageTypeWotLK.Channel)
                        channelName = packet.ReadCString();

                    receiver = packet.ReadGuid().To128(GetSession().GameState);
                    break;
                }
            }

            switch (chatType)
            {
                case ChatMessageTypeWotLK.BattlegroundAlliance:
                case ChatMessageTypeWotLK.BattlegroundHorde:
                    Utility.Swap(ref sender, ref receiver);
                    break;
            }

            uint textLength = packet.ReadUInt32();
            string text = packet.ReadString(textLength);
            var chatFlags = (ChatFlags)packet.ReadUInt8();

            if (LegacyVersion.InVersion(ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056) &&
                packet.GetUniversalOpcode(false) == Opcode.SMSG_GM_MESSAGECHAT)
            {
                uint gmNameLength = packet.ReadUInt32();
                packet.ReadString(gmNameLength);
            }

            uint achievementId = 0;
            if (chatType == ChatMessageTypeWotLK.Achievement || chatType == ChatMessageTypeWotLK.GuildAchievement)
                achievementId = packet.ReadUInt32();

            if (Session.GameState.IgnoredPlayers.Contains(sender) && !chatFlags.HasFlag(ChatFlags.GM) && chatType != ChatMessageTypeWotLK.Ignored)
            {
                if (chatType == ChatMessageTypeWotLK.Whisper)
                { // In legacy versions the client handled the ignore itself and also sends a "You are ignored" message back.
                    WorldPacket ignoreResponsePacket = new WorldPacket(Opcode.CMSG_CHAT_REPORT_IGNORED);
                    ignoreResponsePacket.WriteGuid(sender!.To64());
                    ignoreResponsePacket.WriteUInt8(0); // unk
                    SendPacketToServer(ignoreResponsePacket);
                }
                return;
            }

            string addonPrefix = "";
            if (!ChatPkt.CheckAddonPrefix(GetSession().GameState.AddonPrefixes, ref language, ref text, ref addonPrefix))
                return;

            ChatMessageTypeModern chatTypeModern = (ChatMessageTypeModern)Enum.Parse(typeof(ChatMessageTypeModern), chatType.ToString());
            ChatPkt chat = new ChatPkt(GetSession(), chatTypeModern, text, language, sender, senderName, receiver, receiverName, channelName, chatFlags, addonPrefix, achievementId);
            SendPacketToClient(chat);
        }

        public void SendMessageChatVanilla(ChatMessageTypeVanilla type, uint lang, string msg, string channel, string to)
        {
            if (HandleHermesInternalChatCommand(msg))
            {
                return; // was handled by us
            }
            
            WorldPacket packet = new WorldPacket(Opcode.CMSG_MESSAGECHAT);
            packet.WriteUInt32((uint)type);
            packet.WriteUInt32(lang);

            switch (type)
            {
                case ChatMessageTypeVanilla.Channel:
                    packet.WriteCString(channel);
                    packet.WriteCString(msg);
                    break;
                case ChatMessageTypeVanilla.Whisper:
                    packet.WriteCString(to);
                    packet.WriteCString(msg);
                    break;
                case ChatMessageTypeVanilla.Say:
                case ChatMessageTypeVanilla.Emote:
                case ChatMessageTypeVanilla.Yell:
                case ChatMessageTypeVanilla.Party:
                case ChatMessageTypeVanilla.Guild:
                case ChatMessageTypeVanilla.Officer:
                case ChatMessageTypeVanilla.Raid:
                case ChatMessageTypeVanilla.RaidLeader:
                case ChatMessageTypeVanilla.RaidWarning:
                case ChatMessageTypeVanilla.Battleground:
                case ChatMessageTypeVanilla.BattlegroundLeader:
                case ChatMessageTypeVanilla.Afk:
                case ChatMessageTypeVanilla.Dnd:
                    packet.WriteCString(msg);
                    break;
            }

            SendPacket(packet);
        }

        // TODO: make all of these available via HTML ingame support menu (as soon as we can influence the page)
        private bool HandleHermesInternalChatCommand(string msg)
        {
            // Marks a quest as completed
            // Useful for /run print(C_QuestLog.IsQuestFlaggedCompleted($questId))
            // !qcomplete <questId>
            if (msg.StartsWith("!qcomplete"))
            {
                var questIdStr = msg.Remove(0, "!qcomplete".Length);
                if (!uint.TryParse(questIdStr, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var questId))
                {
                    Log.Print(LogType.Error, $"Chatcommand Invalid questId '{questIdStr}'");
                    return true;
                }
                GetSession().GameState.CurrentPlayerStorage.CompletedQuests.MarkQuestAsCompleted(questId);
                return true;
            }

            // Marks a quest as uncompleted
            // !quncomplete <questId>
            if (msg.StartsWith("!quncomplete"))
            {
                var questIdStr = msg.Remove(0, "!quncomplete".Length);
                if (!uint.TryParse(questIdStr, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var questId))
                {
                    Log.Print(LogType.Error, $"Chatcommand Invalid questId '{questIdStr}'");
                    return true;
                }
                GetSession().GameState.CurrentPlayerStorage.CompletedQuests.MarkQuestAsNotCompleted(questId);
                return true;
            }

            return false;
        }

        public void SendMessageChatWotLK(ChatMessageTypeWotLK type, uint lang, string msg, string channel, string to)
        {
            if (HandleHermesInternalChatCommand(msg))
            {
                return; // was handled by us
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_MESSAGECHAT);
            packet.WriteUInt32((uint)type);
            packet.WriteUInt32(lang);

            switch (type)
            {
                case ChatMessageTypeWotLK.Channel:
                    packet.WriteCString(channel);
                    packet.WriteCString(msg);
                    break;
                case ChatMessageTypeWotLK.Whisper:
                    packet.WriteCString(to);
                    packet.WriteCString(msg);
                    break;
                case ChatMessageTypeWotLK.Say:
                case ChatMessageTypeWotLK.Emote:
                case ChatMessageTypeWotLK.Yell:
                case ChatMessageTypeWotLK.Party:
                case ChatMessageTypeWotLK.PartyLeader:
                case ChatMessageTypeWotLK.Guild:
                case ChatMessageTypeWotLK.Officer:
                case ChatMessageTypeWotLK.Raid:
                case ChatMessageTypeWotLK.RaidLeader:
                case ChatMessageTypeWotLK.RaidWarning:
                case ChatMessageTypeWotLK.Battleground:
                case ChatMessageTypeWotLK.BattlegroundLeader:
                case ChatMessageTypeWotLK.Afk:
                case ChatMessageTypeWotLK.Dnd:
                    packet.WriteCString(msg);
                    break;
            }

            SendPacket(packet);
        }

        [PacketHandler(Opcode.SMSG_EMOTE)]
        void HandleEmote(WorldPacket packet)
        {
            EmoteMessage emote = new EmoteMessage();
            emote.EmoteID = packet.ReadUInt32();
            emote.Guid = packet.ReadGuid().To128(GetSession().GameState);
            SendPacketToClient(emote);
        }

        [PacketHandler(Opcode.SMSG_TEXT_EMOTE)]
        void HandleTextEmote(WorldPacket packet)
        {
            STextEmote emote = new STextEmote();
            emote.SourceGUID = packet.ReadGuid().To128(GetSession().GameState);
            emote.SourceAccountGUID = GetSession().GetGameAccountGuidForPlayer(emote.SourceGUID);
            emote.EmoteID = packet.ReadInt32();
            emote.SoundIndex = packet.ReadInt32();
            uint nameLength = packet.ReadUInt32();
            string targetName = packet.ReadString(nameLength);
            WowGuid128 targetGuid = GetSession().GameState.GetPlayerGuidByName(targetName);
            emote.TargetGUID = targetGuid != null ? targetGuid : emote.SourceGUID;
            SendPacketToClient(emote);
        }

        [PacketHandler(Opcode.SMSG_PRINT_NOTIFICATION)]
        void HandlePrintNotification(WorldPacket packet)
        {
            PrintNotification notify = new PrintNotification();
            notify.NotifyText = packet.ReadCString();
            SendPacketToClient(notify);
        }

        [PacketHandler(Opcode.SMSG_CHAT_PLAYER_NOTFOUND)]
        void HandleChatPlayerNotFound(WorldPacket packet)
        {
            ChatPlayerNotfound error = new ChatPlayerNotfound();
            error.Name = packet.ReadCString();
            SendPacketToClient(error);
        }

        [PacketHandler(Opcode.SMSG_DEFENSE_MESSAGE)]
        void HandleDefenseMessage(WorldPacket packet)
        {
            DefenseMessage message = new DefenseMessage();
            message.ZoneID = packet.ReadUInt32();
            packet.ReadUInt32(); // message length
            message.MessageText = packet.ReadCString();
            SendPacketToClient(message);
        }

        [PacketHandler(Opcode.SMSG_CHAT_SERVER_MESSAGE)]
        void HandleChatServerMessage(WorldPacket packet)
        {
            ChatServerMessage message = new ChatServerMessage();
            message.MessageID = packet.ReadInt32();
            message.StringParam = packet.ReadCString();
            SendPacketToClient(message);
        }

        public void SendChatJoinChannel(int channelId, string channelName, string password)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAT_JOIN_CHANNEL);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteInt32(channelId);
                packet.WriteUInt8(0); // Has Voice
                packet.WriteUInt8(0); // Joined by zone update
            }
            packet.WriteCString(channelName);
            packet.WriteCString(password);
            SendPacketToServer(packet);
        }

        public void SendChatLeaveChannel(int channelId, string channelName)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_CHAT_LEAVE_CHANNEL);
            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteInt32(channelId);
            packet.WriteCString(channelName);
            SendPacketToServer(packet);
        }
    }
}
