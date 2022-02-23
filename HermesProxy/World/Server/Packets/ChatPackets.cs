/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class JoinChannel : ClientPacket
    {
        public JoinChannel(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            ChatChannelId = _worldPacket.ReadInt32();
            uint channelLength = _worldPacket.ReadBits<uint>(7);
            uint passwordLength = _worldPacket.ReadBits<uint>(7);
            _worldPacket.ResetBitPos();
            ChannelName = _worldPacket.ReadString(channelLength);
            Password = _worldPacket.ReadString(passwordLength);
        }

        public string Password;
        public string ChannelName;
        public int ChatChannelId;
    }

    public class ChannelNotifyJoined : ServerPacket
    {
        public ChannelNotifyJoined() : base(Opcode.SMSG_CHANNEL_NOTIFY_JOINED) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Channel.GetByteCount(), 7);
            _worldPacket.WriteBits(ChannelWelcomeMsg.GetByteCount(), 11);
            _worldPacket.WriteUInt32((uint)ChannelFlags);
            _worldPacket.WriteInt32(ChatChannelID);
            _worldPacket.WriteUInt64(InstanceID);
            _worldPacket.WritePackedGuid128(ChannelGUID);
            _worldPacket.WriteString(Channel);
            _worldPacket.WriteString(ChannelWelcomeMsg);
        }

        public string ChannelWelcomeMsg = "";
        public int ChatChannelID;
        public ulong InstanceID;
        public ChannelFlags ChannelFlags;
        public string Channel = "";
        public WowGuid128 ChannelGUID;
    }

    public class LeaveChannel : ClientPacket
    {
        public LeaveChannel(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            ZoneChannelID = _worldPacket.ReadInt32();
            ChannelName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(7));
        }

        public int ZoneChannelID;
        public string ChannelName;
    }

    public class ChannelNotifyLeft : ServerPacket
    {
        public ChannelNotifyLeft() : base(Opcode.SMSG_CHANNEL_NOTIFY_LEFT) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Channel.GetByteCount(), 7);
            _worldPacket.WriteBit(Suspended);
            _worldPacket.WriteInt32(ChatChannelID);
            _worldPacket.WriteString(Channel);
        }

        public string Channel;
        public int ChatChannelID;
        public bool Suspended;
    }

    public class ChatMessageAFK : ClientPacket
    {
        public ChatMessageAFK(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint len = _worldPacket.ReadBits<uint>(9);
            Text = _worldPacket.ReadString(len);
        }

        public string Text;
    }

    public class ChatMessageDND : ClientPacket
    {
        public ChatMessageDND(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            uint len = _worldPacket.ReadBits<uint>(9);
            Text = _worldPacket.ReadString(len);
        }

        public string Text;
    }

    public class ChatMessageChannel : ClientPacket
    {
        public ChatMessageChannel(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Language = _worldPacket.ReadUInt32();
            ChannelGUID = _worldPacket.ReadPackedGuid128();
            uint targetLen = _worldPacket.ReadBits<uint>(9);
            uint textLen = _worldPacket.ReadBits<uint>(9);
            Target = _worldPacket.ReadString(targetLen);
            Text = _worldPacket.ReadString(textLen);
        }

        public uint Language;
        public WowGuid128 ChannelGUID;
        public string Text;
        public string Target;
    }

    public class ChatMessageWhisper : ClientPacket
    {
        public ChatMessageWhisper(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Language = _worldPacket.ReadUInt32();
            uint targetLen = _worldPacket.ReadBits<uint>(9);
            uint textLen = _worldPacket.ReadBits<uint>(9);
            Target = _worldPacket.ReadString(targetLen);
            Text = _worldPacket.ReadString(textLen);
        }

        public uint Language = 0;
        public string Text;
        public string Target;
    }

    public class ChatMessage : ClientPacket
    {
        public ChatMessage(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Language = _worldPacket.ReadUInt32();
            uint len = _worldPacket.ReadBits<uint>(9);
            Text = _worldPacket.ReadString(len);
        }

        public string Text;
        public uint Language;
    }

    public class ChatPkt : ServerPacket
    {
        public ChatPkt(ChatMessageTypeModern chatType, uint language, WowGuid128 sender, string senderName, WowGuid128 receiver, string receiverName, string message, string channelName, ChatFlags chatFlags, uint achievementId = 0) : base(Opcode.SMSG_CHAT)
        {
            SlashCmd = chatType;
            _Language = language;

            SenderGUID = sender != null ? sender : WowGuid128.Empty;
            if (String.IsNullOrEmpty(senderName) && sender != null)
                SenderName = Global.CurrentSessionData.GameState.GetPlayerName(sender);
            else
                SenderName = senderName;

            SenderAccountGUID = sender != null ? Global.CurrentSessionData.GameState.GetGameAccountGuidForPlayer(sender) : WowGuid128.Empty;
            SenderGuildGUID = WowGuid128.Empty;
            PartyGUID = WowGuid128.Empty;

            TargetGUID = receiver != null ? receiver : WowGuid128.Empty;
            if (String.IsNullOrEmpty(receiverName) && receiver != null)
                TargetName = Global.CurrentSessionData.GameState.GetPlayerName(receiver);
            else
                TargetName = receiverName;

            _ChatFlags = chatFlags;
            ChatText = message;
            Channel = channelName;
            AchievementID = achievementId;
            SenderVirtualAddress = Global.CurrentSessionData.RealmId.GetAddress();
            TargetVirtualAddress = Global.CurrentSessionData.RealmId.GetAddress();
        }
        public override void Write()
        {
            _worldPacket.WriteUInt8((byte)SlashCmd);
            _worldPacket.WriteUInt32((uint)_Language);
            _worldPacket.WritePackedGuid128(SenderGUID);
            _worldPacket.WritePackedGuid128(SenderGuildGUID);
            _worldPacket.WritePackedGuid128(SenderAccountGUID);
            _worldPacket.WritePackedGuid128(TargetGUID);
            _worldPacket.WriteUInt32(TargetVirtualAddress);
            _worldPacket.WriteUInt32(SenderVirtualAddress);
            _worldPacket.WritePackedGuid128(PartyGUID);
            _worldPacket.WriteUInt32(AchievementID);
            _worldPacket.WriteFloat(DisplayTime);
            _worldPacket.WriteBits(SenderName.GetByteCount(), 11);
            _worldPacket.WriteBits(TargetName.GetByteCount(), 11);
            _worldPacket.WriteBits(Prefix.GetByteCount(), 5);
            _worldPacket.WriteBits(Channel.GetByteCount(), 7);
            _worldPacket.WriteBits(ChatText.GetByteCount(), 12);
            _worldPacket.WriteBits((byte)_ChatFlags, 14);
            _worldPacket.WriteBit(HideChatLog);
            _worldPacket.WriteBit(FakeSenderName);
            _worldPacket.WriteBit(Unused_801.HasValue);
            _worldPacket.WriteBit(ChannelGUID != null);
            _worldPacket.FlushBits();

            _worldPacket.WriteString(SenderName);
            _worldPacket.WriteString(TargetName);
            _worldPacket.WriteString(Prefix);
            _worldPacket.WriteString(Channel);
            _worldPacket.WriteString(ChatText);

            if (Unused_801.HasValue)
                _worldPacket.WriteUInt32(Unused_801.Value);

            if (ChannelGUID != null)
                _worldPacket.WritePackedGuid128(ChannelGUID);
        }

        public ChatMessageTypeModern SlashCmd = 0;
        public uint _Language = 0;
        public WowGuid128 SenderGUID;
        public WowGuid128 SenderGuildGUID;
        public WowGuid128 SenderAccountGUID;
        public WowGuid128 TargetGUID;
        public WowGuid128 PartyGUID;
        public WowGuid128 ChannelGUID;
        public uint SenderVirtualAddress;
        public uint TargetVirtualAddress;
        public string SenderName = "";
        public string TargetName = "";
        public string Prefix = "";
        public string Channel = "";
        public string ChatText = "";
        public uint AchievementID;
        public ChatFlags _ChatFlags = 0;
        public float DisplayTime = 0.0f;
        public uint? Unused_801;
        public bool HideChatLog = false;
        public bool FakeSenderName = false;
    }

    public class EmoteMessage : ServerPacket
    {
        public EmoteMessage() : base(Opcode.SMSG_EMOTE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(Guid);
            _worldPacket.WriteUInt32(EmoteID);
            _worldPacket.WriteInt32(SpellVisualKitIDs.Count);

            foreach (var id in SpellVisualKitIDs)
                _worldPacket.WriteUInt32(id);
        }

        public WowGuid128 Guid;
        public uint EmoteID;
        public List<uint> SpellVisualKitIDs = new();
    }

    public class CTextEmote : ClientPacket
    {
        public CTextEmote(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Target = _worldPacket.ReadPackedGuid128();
            EmoteID = _worldPacket.ReadInt32();
            SoundIndex = _worldPacket.ReadInt32();

            SpellVisualKitIDs = new uint[_worldPacket.ReadUInt32()];
            for (var i = 0; i < SpellVisualKitIDs.Length; ++i)
                SpellVisualKitIDs[i] = _worldPacket.ReadUInt32();
        }

        public WowGuid128 Target;
        public int EmoteID;
        public int SoundIndex;
        public uint[] SpellVisualKitIDs;
    }

    public class STextEmote : ServerPacket
    {
        public STextEmote() : base(Opcode.SMSG_TEXT_EMOTE, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WritePackedGuid128(SourceGUID);
            _worldPacket.WritePackedGuid128(SourceAccountGUID);
            _worldPacket.WriteInt32(EmoteID);
            _worldPacket.WriteInt32(SoundIndex);
            _worldPacket.WritePackedGuid128(TargetGUID);
        }

        public WowGuid128 SourceGUID;
        public WowGuid128 SourceAccountGUID;
        public WowGuid128 TargetGUID;
        public int SoundIndex = -1;
        public int EmoteID;
    }
}
