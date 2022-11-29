﻿/*
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

using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class GameRuleValuePair
    {
        public void Write(WorldPacket data)
        {
            data.WriteInt32(Rule);
            data.WriteInt32(Value);
        }
        public int Rule;
        public int Value;
    }

    public class FeatureSystemStatus : ServerPacket
    {
        public FeatureSystemStatus() : base(Opcode.SMSG_FEATURE_SYSTEM_STATUS)
        {
        }

        public override void Write()
        {
            _worldPacket.WriteUInt8(ComplaintStatus);

            _worldPacket.WriteUInt32(ScrollOfResurrectionRequestsRemaining);
            _worldPacket.WriteUInt32(ScrollOfResurrectionMaxRequestsPerDay);

            _worldPacket.WriteUInt32(CfgRealmID);
            _worldPacket.WriteInt32(CfgRealmRecID);

            _worldPacket.WriteUInt32(RAFSystem.MaxRecruits);
            _worldPacket.WriteUInt32(RAFSystem.MaxRecruitMonths);
            _worldPacket.WriteUInt32(RAFSystem.MaxRecruitmentUses);
            _worldPacket.WriteUInt32(RAFSystem.DaysInCycle);

            _worldPacket.WriteUInt32(TwitterPostThrottleLimit);
            _worldPacket.WriteUInt32(TwitterPostThrottleCooldown);

            _worldPacket.WriteUInt32(TokenPollTimeSeconds);
            _worldPacket.WriteUInt32(KioskSessionMinutes);
            _worldPacket.WriteInt64(TokenBalanceAmount);

            _worldPacket.WriteUInt32(BpayStoreProductDeliveryDelay);
            _worldPacket.WriteUInt32(ClubsPresenceUpdateTimer);
            _worldPacket.WriteUInt32(HiddenUIClubsPresenceUpdateTimer);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                _worldPacket.WriteInt32(ActiveSeason);
                _worldPacket.WriteInt32(GameRuleValues.Count);

                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                    _worldPacket.WriteInt16(MaxPlayerNameQueriesPerPacket);

                if (ModernVersion.AddedInVersion(9, 2, 7, 1, 14, 4, 3, 4, 0))
                    _worldPacket.WriteInt16(PlayerNameQueryTelemetryInterval);

                foreach (var rulePair in GameRuleValues)
                    rulePair.Write(_worldPacket);
            }

            _worldPacket.WriteBit(VoiceEnabled);
            _worldPacket.WriteBit(EuropaTicketSystemStatus != null);
            _worldPacket.WriteBit(ScrollOfResurrectionEnabled);
            _worldPacket.WriteBit(BpayStoreEnabled);
            _worldPacket.WriteBit(BpayStoreAvailable);
            _worldPacket.WriteBit(BpayStoreDisabledByParentalControls);
            _worldPacket.WriteBit(ItemRestorationButtonEnabled);
            _worldPacket.WriteBit(BrowserEnabled);
            _worldPacket.WriteBit(SessionAlert != null);
            _worldPacket.WriteBit(RAFSystem.Enabled);
            _worldPacket.WriteBit(RAFSystem.RecruitingEnabled);
            _worldPacket.WriteBit(CharUndeleteEnabled);
            _worldPacket.WriteBit(RestrictedAccount);
            _worldPacket.WriteBit(CommerceSystemEnabled);
            _worldPacket.WriteBit(TutorialsEnabled);
            _worldPacket.WriteBit(TwitterEnabled);
            _worldPacket.WriteBit(Unk67);
            _worldPacket.WriteBit(WillKickFromWorld);
            _worldPacket.WriteBit(KioskModeEnabled);
            _worldPacket.WriteBit(CompetitiveModeEnabled);
            _worldPacket.WriteBit(TokenBalanceEnabled);
            _worldPacket.WriteBit(WarModeFeatureEnabled);
            _worldPacket.WriteBit(ClubsEnabled);
            _worldPacket.WriteBit(ClubsBattleNetClubTypeAllowed);
            _worldPacket.WriteBit(ClubsCharacterClubTypeAllowed);
            _worldPacket.WriteBit(ClubsPresenceUpdateEnabled);
            _worldPacket.WriteBit(VoiceChatDisabledByParentalControl);
            _worldPacket.WriteBit(VoiceChatMutedByParentalControl);
            _worldPacket.WriteBit(QuestSessionEnabled);
            _worldPacket.WriteBit(IsMuted);
            _worldPacket.WriteBit(ClubFinderEnabled);
            _worldPacket.WriteBit(Unknown901CheckoutRelated);

            if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
            {
                _worldPacket.WriteBit(TextToSpeechFeatureEnabled);
                _worldPacket.WriteBit(ChatDisabledByDefault);
                _worldPacket.WriteBit(ChatDisabledByPlayer);
                _worldPacket.WriteBit(LFGListCustomRequiresAuthenticator);
            }

            if (ModernVersion.IsClassicVersionBuild())
            {
                _worldPacket.WriteBit(BattlegroundsEnabled);
                _worldPacket.WriteBit(RaceClassExpansionLevels != null);
            }

            _worldPacket.FlushBits();

            {
                _worldPacket.WriteBit(QuickJoinConfig.ToastsDisabled);
                _worldPacket.WriteFloat(QuickJoinConfig.ToastDuration);
                _worldPacket.WriteFloat(QuickJoinConfig.DelayDuration);
                _worldPacket.WriteFloat(QuickJoinConfig.QueueMultiplier);
                _worldPacket.WriteFloat(QuickJoinConfig.PlayerMultiplier);
                _worldPacket.WriteFloat(QuickJoinConfig.PlayerFriendValue);
                _worldPacket.WriteFloat(QuickJoinConfig.PlayerGuildValue);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleInitialThreshold);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleDecayTime);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottlePrioritySpike);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleMinThreshold);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityNormal);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityLow);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottlePvPHonorThreshold);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityDefault);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityAbove);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityBelow);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingAbove);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingBelow);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleRfPriorityAbove);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleRfIlvlScalingAbove);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleDfMaxItemLevel);
                _worldPacket.WriteFloat(QuickJoinConfig.ThrottleDfBestPriority);
            }

            if (SessionAlert != null)
            {
                _worldPacket.WriteInt32(SessionAlert.Delay);
                _worldPacket.WriteInt32(SessionAlert.Period);
                _worldPacket.WriteInt32(SessionAlert.DisplayTime);
            }

            if (ModernVersion.IsClassicVersionBuild())
            {
                if (RaceClassExpansionLevels != null)
                {
                    _worldPacket.WriteInt32(RaceClassExpansionLevels.Count);
                    for (var i = 0; i < RaceClassExpansionLevels.Count; ++i)
                        _worldPacket.WriteUInt8(RaceClassExpansionLevels[i]);
                }
            }

            _worldPacket.WriteBit(Squelch.IsSquelched);
            _worldPacket.WritePackedGuid128(Squelch.BnetAccountGuid);
            _worldPacket.WritePackedGuid128(Squelch.GuildGuid);

            if (EuropaTicketSystemStatus != null)
                EuropaTicketSystemStatus.Write(_worldPacket);
        }

        public bool VoiceEnabled;
        public bool BrowserEnabled;
        public bool BpayStoreAvailable;
        public bool BpayStoreEnabled;
        public SessionAlertConfig SessionAlert;
        public uint ScrollOfResurrectionMaxRequestsPerDay;
        public bool ScrollOfResurrectionEnabled;
        public EuropaTicketConfig EuropaTicketSystemStatus;
        public uint ScrollOfResurrectionRequestsRemaining;
        public uint CfgRealmID;
        public byte ComplaintStatus;
        public int CfgRealmRecID;
        public uint TwitterPostThrottleLimit;
        public uint TwitterPostThrottleCooldown;
        public uint TokenPollTimeSeconds;
        public long TokenBalanceAmount;
        public uint BpayStoreProductDeliveryDelay;
        public uint ClubsPresenceUpdateTimer;
        public uint HiddenUIClubsPresenceUpdateTimer; // Timer for updating club presence when communities ui frame is hidden
        public int ActiveSeason;
        public List<GameRuleValuePair> GameRuleValues = new();
        public short MaxPlayerNameQueriesPerPacket;
        public short PlayerNameQueryTelemetryInterval;
        public uint KioskSessionMinutes;
        public bool ItemRestorationButtonEnabled;
        public bool CharUndeleteEnabled; // Implemented
        public bool BpayStoreDisabledByParentalControls;
        public bool TwitterEnabled;
        public bool CommerceSystemEnabled;
        public bool Unk67;
        public bool WillKickFromWorld;
        public bool RestrictedAccount;
        public bool TutorialsEnabled;
        public bool KioskModeEnabled;
        public bool CompetitiveModeEnabled;
        public bool TokenBalanceEnabled;
        public bool WarModeFeatureEnabled;
        public bool ClubsEnabled;
        public bool ClubsBattleNetClubTypeAllowed;
        public bool ClubsCharacterClubTypeAllowed;
        public bool ClubsPresenceUpdateEnabled;
        public bool VoiceChatDisabledByParentalControl;
        public bool VoiceChatMutedByParentalControl;
        public bool QuestSessionEnabled;
        public bool IsMuted;
        public bool ClubFinderEnabled;
        public bool Unknown901CheckoutRelated;
        public bool TextToSpeechFeatureEnabled;
        public bool ChatDisabledByDefault;
        public bool ChatDisabledByPlayer;
        public bool LFGListCustomRequiresAuthenticator;
        public bool BattlegroundsEnabled;
        public List<byte> RaceClassExpansionLevels;

        public SocialQueueConfig QuickJoinConfig;
        public SquelchInfo Squelch;
        public RafSystemFeatureInfo RAFSystem;

        public class SessionAlertConfig
        {
            public int Delay;
            public int Period;
            public int DisplayTime;
        }

        public struct SocialQueueConfig
        {
            public bool ToastsDisabled;
            public float ToastDuration;
            public float DelayDuration;
            public float QueueMultiplier;
            public float PlayerMultiplier;
            public float PlayerFriendValue;
            public float PlayerGuildValue;
            public float ThrottleInitialThreshold;
            public float ThrottleDecayTime;
            public float ThrottlePrioritySpike;
            public float ThrottleMinThreshold;
            public float ThrottlePvPPriorityNormal;
            public float ThrottlePvPPriorityLow;
            public float ThrottlePvPHonorThreshold;
            public float ThrottleLfgListPriorityDefault;
            public float ThrottleLfgListPriorityAbove;
            public float ThrottleLfgListPriorityBelow;
            public float ThrottleLfgListIlvlScalingAbove;
            public float ThrottleLfgListIlvlScalingBelow;
            public float ThrottleRfPriorityAbove;
            public float ThrottleRfIlvlScalingAbove;
            public float ThrottleDfMaxItemLevel;
            public float ThrottleDfBestPriority;
        }

        public struct SquelchInfo
        {
            public bool IsSquelched;
            public WowGuid128 BnetAccountGuid;
            public WowGuid128 GuildGuid;
        }

        public struct RafSystemFeatureInfo
        {
            public bool Enabled;
            public bool RecruitingEnabled;
            public uint MaxRecruits;
            public uint MaxRecruitMonths;
            public uint MaxRecruitmentUses;
            public uint DaysInCycle;
        }
    }

    public class FeatureSystemStatusGlueScreen : ServerPacket
    {
        public FeatureSystemStatusGlueScreen() : base(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN) { }

        public override void Write()
        {
            _worldPacket.WriteBit(BpayStoreEnabled);
            _worldPacket.WriteBit(BpayStoreAvailable);
            _worldPacket.WriteBit(BpayStoreDisabledByParentalControls);
            _worldPacket.WriteBit(CharUndeleteEnabled);
            _worldPacket.WriteBit(CommerceSystemEnabled);
            _worldPacket.WriteBit(Unk14);
            _worldPacket.WriteBit(WillKickFromWorld);
            _worldPacket.WriteBit(IsExpansionPreorderInStore);
            _worldPacket.WriteBit(KioskModeEnabled);
            _worldPacket.WriteBit(CompetitiveModeEnabled);
            _worldPacket.WriteBit(TrialBoostEnabled);
            _worldPacket.WriteBit(TokenBalanceEnabled);
            _worldPacket.WriteBit(LiveRegionCharacterListEnabled);
            _worldPacket.WriteBit(LiveRegionCharacterCopyEnabled);
            _worldPacket.WriteBit(LiveRegionAccountCopyEnabled);
            _worldPacket.WriteBit(LiveRegionKeyBindingsCopyEnabled);
            _worldPacket.WriteBit(Unknown901CheckoutRelated);
            _worldPacket.WriteBit(EuropaTicketSystemStatus != null);
            _worldPacket.FlushBits();

            if (EuropaTicketSystemStatus != null)
                EuropaTicketSystemStatus.Write(_worldPacket);

            _worldPacket.WriteUInt32(TokenPollTimeSeconds);
            _worldPacket.WriteUInt32(KioskSessionMinutes);
            _worldPacket.WriteInt64(TokenBalanceAmount);
            _worldPacket.WriteInt32(MaxCharactersPerRealm);
            _worldPacket.WriteInt32(LiveRegionCharacterCopySourceRegions.Count);
            _worldPacket.WriteUInt32(BpayStoreProductDeliveryDelay);
            _worldPacket.WriteInt32(ActiveCharacterUpgradeBoostType);
            _worldPacket.WriteInt32(ActiveClassTrialBoostType);
            _worldPacket.WriteInt32(MinimumExpansionLevel);
            _worldPacket.WriteInt32(MaximumExpansionLevel);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                _worldPacket.WriteInt32(ActiveSeason);
                _worldPacket.WriteInt32(GameRuleValues.Count);

                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                    _worldPacket.WriteInt16(MaxPlayerNameQueriesPerPacket);

                if (ModernVersion.AddedInVersion(9, 2, 7, 1, 14, 4, 3, 4, 0))
                    _worldPacket.WriteInt16(PlayerNameQueryTelemetryInterval);
            }

            foreach (var sourceRegion in LiveRegionCharacterCopySourceRegions)
                _worldPacket.WriteInt32(sourceRegion);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                foreach (var rulePair in GameRuleValues)
                    rulePair.Write(_worldPacket);
            }
        }

        public bool BpayStoreAvailable; // NYI
        public bool BpayStoreDisabledByParentalControls; // NYI
        public bool CharUndeleteEnabled;
        public bool BpayStoreEnabled; // NYI
        public bool CommerceSystemEnabled; // NYI
        public bool Unk14; // NYI
        public bool WillKickFromWorld; // NYI
        public bool IsExpansionPreorderInStore; // NYI
        public bool KioskModeEnabled; // NYI
        public bool CompetitiveModeEnabled; // NYI
        public bool TrialBoostEnabled; // NYI
        public bool TokenBalanceEnabled; // NYI
        public bool LiveRegionCharacterListEnabled; // NYI
        public bool LiveRegionCharacterCopyEnabled; // NYI
        public bool LiveRegionAccountCopyEnabled; // NYI
        public bool LiveRegionKeyBindingsCopyEnabled = false;
        public bool Unknown901CheckoutRelated = false; // NYI
        public EuropaTicketConfig EuropaTicketSystemStatus;
        public List<int> LiveRegionCharacterCopySourceRegions = new();
        public uint TokenPollTimeSeconds;     // NYI
        public long TokenBalanceAmount;     // NYI
        public int MaxCharactersPerRealm;
        public uint BpayStoreProductDeliveryDelay;     // NYI
        public int ActiveCharacterUpgradeBoostType;     // NYI
        public int ActiveClassTrialBoostType;     // NYI
        public int MinimumExpansionLevel;
        public int MaximumExpansionLevel;
        public int ActiveSeason;
        public List<GameRuleValuePair> GameRuleValues = new();
        public short MaxPlayerNameQueriesPerPacket;
        public short PlayerNameQueryTelemetryInterval;
        public uint KioskSessionMinutes;
    }

    public class MOTD : ServerPacket
    {
        public MOTD() : base(Opcode.SMSG_MOTD) { }

        public override void Write()
        {
            _worldPacket.WriteBits(Text.Count, 4);
            _worldPacket.FlushBits();

            foreach (var line in Text)
            {
                _worldPacket.WriteBits(line.GetByteCount(), 7);
                _worldPacket.FlushBits();
                _worldPacket.WriteString(line);
            }
        }

        public List<string> Text = new();
    }

    public class SetTimeZoneInformation : ServerPacket
    {
        public SetTimeZoneInformation() : base(Opcode.SMSG_SET_TIME_ZONE_INFORMATION) { }

        public override void Write()
        {
            _worldPacket.WriteBits(ServerTimeTZ.GetByteCount(), 7);
            _worldPacket.WriteBits(GameTimeTZ.GetByteCount(), 7);
            _worldPacket.WriteString(ServerTimeTZ);
            _worldPacket.WriteString(GameTimeTZ);
        }

        public string ServerTimeTZ;
        public string GameTimeTZ;
    }

    public struct SavedThrottleObjectState
    {
        public uint MaxTries;
        public uint PerMilliseconds;
        public uint TryCount;
        public uint LastResetTimeBeforeNow;

        public void Write(WorldPacket data)
        {
            data.WriteUInt32(MaxTries);
            data.WriteUInt32(PerMilliseconds);
            data.WriteUInt32(TryCount);
            data.WriteUInt32(LastResetTimeBeforeNow);
        }
    }

    public class EuropaTicketConfig
    {
        public bool TicketsEnabled;
        public bool BugsEnabled;
        public bool ComplaintsEnabled;
        public bool SuggestionsEnabled;

        public SavedThrottleObjectState ThrottleState;

        public void Write(WorldPacket data)
        {
            data.WriteBit(TicketsEnabled);
            data.WriteBit(BugsEnabled);
            data.WriteBit(ComplaintsEnabled);
            data.WriteBit(SuggestionsEnabled);

            ThrottleState.Write(data);
        }
    }
}
