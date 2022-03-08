using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

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

                    GetSession().GameState.SetChannelId(channelName, channelId);

                    ChannelNotifyJoined joined = new ChannelNotifyJoined();
                    joined.Channel = channelName;
                    joined.ChannelFlags = flags;
                    joined.ChatChannelID = channelId;
                    joined.ChannelGUID = WowGuid128.Create(HighGuidType703.ChatChannel, (uint)GetSession().GameState.CurrentMapId, 1, (ulong)channelId);
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
                    receiver = packet.ReadGuid().To128();
                    break;
                case ChatMessageTypeVanilla.Say:
                case ChatMessageTypeVanilla.Party:
                case ChatMessageTypeVanilla.Yell:
                    sender = packet.ReadGuid().To128();
                    packet.ReadGuid(); // Sender Guid again
                    break;
                case ChatMessageTypeVanilla.MonsterSay:
                case ChatMessageTypeVanilla.MonsterYell:
                    sender = packet.ReadGuid().To128();
                    packet.ReadUInt32(); // Sender Name Length
                    senderName = packet.ReadCString();
                    receiver = packet.ReadGuid().To128();
                    break;

                case ChatMessageTypeVanilla.Channel:
                    channelName = packet.ReadCString();
                    packet.ReadUInt32(); // Player Rank
                    sender = packet.ReadGuid().To128();
                    break;
                default:
                    sender = packet.ReadGuid().To128();
                    break;
            }

            packet.ReadInt32(); // Text Length
            string text = packet.ReadCString();
            ChatFlags chatFlags = (ChatFlags)packet.ReadUInt8();

            ChatMessageTypeModern chatTypeModern = (ChatMessageTypeModern)Enum.Parse(typeof(ChatMessageTypeModern), chatType.ToString());
            ChatPkt chat = new ChatPkt(GetSession(), chatTypeModern, language, sender, senderName, receiver, "", text, channelName, chatFlags);
            SendPacketToClient(chat);
        }

        [PacketHandler(Opcode.SMSG_CHAT, ClientVersionBuild.V2_0_1_6180)]
        [PacketHandler(Opcode.SMSG_GM_MESSAGECHAT, ClientVersionBuild.V2_0_1_6180)]
        void HandleServerChatMessageWotLK(WorldPacket packet)
        {
            ChatMessageTypeWotLK chatType = (ChatMessageTypeWotLK)packet.ReadUInt8();
            uint language = packet.ReadUInt32();
            WowGuid128 sender = packet.ReadGuid().To128();
            string senderName = "";
            WowGuid128 receiver = null;
            string receiverName = "";
            string channelName = "";

            packet.ReadInt32(); // Constant time

            switch (chatType)
            {
                case ChatMessageTypeWotLK.Achievement:
                case ChatMessageTypeWotLK.GuildAchievement:
                {
                    packet.ReadGuid(); // Sender GUID again
                    break;
                }
                case ChatMessageTypeWotLK.WhisperForeign:
                {
                    packet.ReadInt32(); // Name Length
                    senderName = packet.ReadCString();
                    receiver = packet.ReadGuid().To128();
                    break;
                }
                case ChatMessageTypeWotLK.BattlegroundNeutral:
                case ChatMessageTypeWotLK.BattlegroundAlliance:
                case ChatMessageTypeWotLK.BattlegroundHorde:
                {
                    var target = packet.ReadGuid(); // Sender GUID
                    switch (target.GetHighType())
                    {
                        case HighGuidType.Creature:
                        case HighGuidType.Vehicle:
                        case HighGuidType.GameObject:
                        case HighGuidType.Transport:
                        case HighGuidType.Pet:
                            packet.ReadInt32(); // Sender Name Length
                            senderName = packet.ReadCString();
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
                    packet.ReadInt32(); // Name Length
                    senderName = packet.ReadCString();
                    receiver = packet.ReadGuid().To128();
                    switch (receiver.GetHighType())
                    {
                        case HighGuidType.Creature:
                        case HighGuidType.Vehicle:
                        case HighGuidType.GameObject:
                        case HighGuidType.Transport:
                            packet.ReadInt32(); // Receiver Name Length
                            receiverName = packet.ReadCString();
                            break;
                    }
                    break;
                }
                default:
                {
                    if (packet.GetUniversalOpcode(false) == Opcode.SMSG_GM_MESSAGECHAT)
                    {
                        packet.ReadInt32(); // GMNameLength
                        packet.ReadCString(); // GMSenderName
                    }

                    if (chatType == ChatMessageTypeWotLK.Channel)
                        channelName = packet.ReadCString();

                    packet.ReadGuid(); // Sender GUID
                    break;
                }
            }

            packet.ReadInt32(); // Text Length
            string text = packet.ReadCString();
            ChatFlags chatFlags = (ChatFlags)packet.ReadUInt8();

            uint achievementId = 0;
            if (chatType == ChatMessageTypeWotLK.Achievement || chatType == ChatMessageTypeWotLK.GuildAchievement)
                achievementId = packet.ReadUInt32();

            ChatMessageTypeModern chatTypeModern = (ChatMessageTypeModern)Enum.Parse(typeof(ChatMessageTypeModern), chatType.ToString());
            ChatPkt chat = new ChatPkt(GetSession(), chatTypeModern, language, sender, senderName, receiver, receiverName, text, channelName, chatFlags, achievementId);
            SendPacketToClient(chat);
        }

        public void SendMessageChatVanilla(ChatMessageTypeVanilla type, uint lang, string msg, string channel, string to)
        {
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

        public void SendMessageChatWotLK(ChatMessageTypeWotLK type, uint lang, string msg, string channel, string to)
        {
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
            emote.Guid = packet.ReadGuid().To128();
            SendPacketToClient(emote);
        }

        [PacketHandler(Opcode.SMSG_TEXT_EMOTE)]
        void HandleTextEmote(WorldPacket packet)
        {
            STextEmote emote = new STextEmote();
            emote.SourceGUID = packet.ReadGuid().To128();
            emote.SourceAccountGUID = GetSession().GetGameAccountGuidForPlayer(emote.SourceGUID);
            emote.EmoteID = packet.ReadInt32();
            emote.SoundIndex = packet.ReadInt32();
            uint nameLength = packet.ReadUInt32();
            string targetName = packet.ReadString(nameLength);
            WowGuid128 targetGuid = GetSession().GameState.GetPlayerGuidByName(targetName);
            emote.TargetGUID = targetGuid != null ? targetGuid : emote.SourceGUID;
            SendPacketToClient(emote);
        }
    }
}
