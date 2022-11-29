using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.QueryGuildInfoResponse.GuildInfo;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_GUILD_COMMAND_RESULT)]
        void HandleGuildCommandResult(WorldPacket packet)
        {
            GuildCommandResult result = new()
            {
                Command = (GuildCommandType)packet.ReadUInt32(),
                Name = packet.ReadCString(),
                Result = (GuildCommandError)packet.ReadUInt32()
            };
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.SMSG_GUILD_EVENT)]
        void HandleGuildEvent(WorldPacket packet)
        {
            GuildEventType eventType = (GuildEventType)packet.ReadUInt8();

            var size = packet.ReadUInt8();
            string[] strings = new string[size];
            for (var i = 0; i < size; i++)
                strings[i] = packet.ReadCString();

            WowGuid128 guid = WowGuid128.Empty;
            if (packet.CanRead())
                guid = packet.ReadGuid().To128(GetSession().GameState);

            switch (eventType)
            {
                case GuildEventType.Promotion:
                case GuildEventType.Demotion:
                {
                    WowGuid128 officer = GetSession().GameState.GetPlayerGuidByName(strings[0]);
                    WowGuid128 player = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                    uint rankId = GetSession().GetGuildRankIdByName(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), strings[2]);
                    if (officer != null && player != null)
                    {
                        GuildSendRankChange promote = new GuildSendRankChange
                        {
                            Officer = officer,
                            Other = player,
                            Promote = eventType == GuildEventType.Promotion,
                            RankID = rankId
                        };
                        SendPacketToClient(promote);
                    }
                    break;
                }
                case GuildEventType.MOTD:
                {
                    GuildEventMotd motd = new GuildEventMotd
                    {
                        MotdText = strings[0]
                    };
                    SendPacketToClient(motd);
                    break;
                }
                case GuildEventType.PlayerJoined:
                {
                    GuildEventPlayerJoined joined = new GuildEventPlayerJoined
                    {
                        Guid = guid,
                        VirtualRealmAddress = GetSession().RealmId.GetAddress(),
                        Name = strings[0]
                    };
                    SendPacketToClient(joined);
                    break;
                }
                case GuildEventType.PlayerLeft:
                {
                    GuildEventPlayerLeft left = new GuildEventPlayerLeft
                    {
                        Removed = false,
                        LeaverGUID = guid,
                        LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                        LeaverName = strings[0]
                    };
                    SendPacketToClient(left);
                    break;
                }
                case GuildEventType.PlayerRemoved:
                {
                    GuildEventPlayerLeft removed = new GuildEventPlayerLeft
                    {
                        Removed = true,
                        LeaverGUID = guid,
                        LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                        LeaverName = strings[0],
                        RemoverGUID = GetSession().GameState.GetPlayerGuidByName(strings[1]),
                        RemoverVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                        RemoverName = strings[1]
                    };
                    SendPacketToClient(removed);
                    break;
                }
                case GuildEventType.LeaderIs:
                {
                    break;
                }
                case GuildEventType.LeaderChanged:
                {
                    WowGuid128 oldLeader = GetSession().GameState.GetPlayerGuidByName(strings[0]);
                    WowGuid128 newLeader = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                    if (oldLeader != null && newLeader != null)
                    {
                        GuildEventNewLeader leader = new GuildEventNewLeader
                        {
                            OldLeaderGUID = oldLeader,
                            OldLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                            OldLeaderName = strings[0],
                            NewLeaderGUID = newLeader,
                            NewLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                            NewLeaderName = strings[1]
                        };
                        SendPacketToClient(leader);
                    }
                    break;
                }
                case GuildEventType.Disbanded:
                {
                    GuildEventDisbanded disband = new GuildEventDisbanded();
                    SendPacketToClient(disband);
                    break;
                }
                case GuildEventType.TabardChange:
                {
                    break;
                }
                case GuildEventType.RankUpdated:
                {
                    GuildEventRanksUpdated ranks = new GuildEventRanksUpdated();
                    SendPacketToClient(ranks);
                    break;
                }
                case GuildEventType.Unk11:
                {
                    break;
                }
                case GuildEventType.PlayerSignedOn:
                case GuildEventType.PlayerSignedOff:
                {
                    GuildEventPresenceChange presence = new GuildEventPresenceChange
                    {
                        Guid = guid,
                        VirtualRealmAddress = GetSession().RealmId.GetAddress(),
                        LoggedOn = eventType == GuildEventType.PlayerSignedOn,
                        Name = strings[0]
                    };
                    SendPacketToClient(presence);
                    break;
                }
                case GuildEventType.BankBagSlotsChanged:
                {
                    break;
                }
                case GuildEventType.BankTabPurchased:
                {
                    GuildEventTabAdded tab = new GuildEventTabAdded();
                    SendPacketToClient(tab);
                    break;
                }
                case GuildEventType.BankTabUpdated:
                {
                    GuildEventTabModified tab = new GuildEventTabModified
                    {
                        Name = strings[0],
                        Icon = strings[1]
                    };
                    SendPacketToClient(tab);
                    break;
                }
                case GuildEventType.BankMoneyUpdate:
                {
                    GuildEventBankMoneyChanged money = new GuildEventBankMoneyChanged
                    {
                        Money = (ulong)int.Parse(strings[0], System.Globalization.NumberStyles.HexNumber)
                    };
                    SendPacketToClient(money);
                    break;
                }
                case GuildEventType.BankMoneyWithdraw:
                {
                    break;
                }
                case GuildEventType.BankTextChanged:
                {
                    GuildEventTabTextChanged tab = new GuildEventTabTextChanged();
                    SendPacketToClient(tab);
                    break;
                }
            }
        }

        [PacketHandler(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE)]
        void HandleQueryGuildInfoResponse(WorldPacket packet)
        {
            QueryGuildInfoResponse guild = new();
            uint guildId = packet.ReadUInt32();
            guild.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, guildId);
            guild.PlayerGuid = GetSession().GameState.CurrentPlayerGuid;
            guild.HasGuildInfo = true;
            guild.Info = new QueryGuildInfoResponse.GuildInfo
            {
                GuildGuid = guild.GuildGUID,
                VirtualRealmAddress = GetSession().RealmId.GetAddress(),

                GuildName = packet.ReadCString()
            };
            GetSession().StoreGuildGuidAndName(guild.GuildGUID, guild.Info.GuildName);

            List<string> ranks = new List<string>();
            for (uint i = 0; i < 10; i++)
            {
                string rankName = packet.ReadCString();
                if (!string.IsNullOrEmpty(rankName))
                {
                    RankInfo rank = new RankInfo
                    {
                        RankID = i,
                        RankOrder = i,
                        RankName = rankName
                    };
                    ranks.Add(rankName);
                    guild.Info.Ranks.Add(rank);
                }
            }
            GetSession().StoreGuildRankNames(guildId, ranks);

            guild.Info.EmblemStyle = packet.ReadUInt32();
            guild.Info.EmblemColor = packet.ReadUInt32();
            guild.Info.BorderStyle = packet.ReadUInt32();
            guild.Info.BorderColor = packet.ReadUInt32();
            guild.Info.BackgroundColor = packet.ReadUInt32();

            SendPacketToClient(guild);
        }

        [PacketHandler(Opcode.SMSG_GUILD_INFO)]
        void HandleGuildInfo(WorldPacket packet)
        {
            packet.ReadCString(); // Guild Name

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                GetSession().GameState.CurrentGuildCreateTime = packet.ReadPackedTime();
            else
            {
                int day = packet.ReadInt32();
                int month = packet.ReadInt32();
                int year = packet.ReadInt32();

                DateTime date;
                try
                {
                    date = new DateTime(year, month, day);
                    GetSession().GameState.CurrentGuildCreateTime = (uint)Time.DateTimeToUnixTime(date);
                }
                catch
                {
                    Log.Print(LogType.Error, $"Invalid guild create date: {day}-{month}-{year}");
                }
            }

            packet.ReadUInt32(); // Players Count

            GetSession().GameState.CurrentGuildNumAccounts = packet.ReadUInt32();
        }

        [PacketHandler(Opcode.SMSG_GUILD_ROSTER)]
        void HandleGuildRoster(WorldPacket packet)
        {
            GuildRoster guild = new();
            var membersCount = packet.ReadUInt32();

            if (GetSession().GameState.CurrentGuildNumAccounts != 0)
                guild.NumAccounts = GetSession().GameState.CurrentGuildNumAccounts;
            else
                guild.NumAccounts = membersCount;

            guild.WelcomeText = packet.ReadCString();
            guild.InfoText = packet.ReadCString();

            if (GetSession().GameState.CurrentGuildCreateTime != 0)
                guild.CreateDate = GetSession().GameState.CurrentGuildCreateTime;
            else
                guild.CreateDate = (uint)Time.UnixTime;

            var ranksCount = packet.ReadInt32();
            if (ranksCount > 0)
            {
                GuildRanks ranks = new GuildRanks();
                for (byte i = 0; i < ranksCount; i++)
                {
                    GuildRankData rank = new GuildRankData
                    {
                        RankID = i,
                        RankOrder = i,
                        RankName = GetSession().GetGuildRankNameById(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), i),
                        Flags = packet.ReadUInt32()
                    };

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        rank.WithdrawGoldLimit = packet.ReadInt32();

                        for (var j = 0; j < GuildConst.MaxBankTabs; j++)
                        {
                            rank.TabFlags[j] = packet.ReadUInt32();
                            rank.TabWithdrawItemLimit[j] = packet.ReadUInt32();
                        }
                    }
                    ranks.Ranks.Add(rank);
                }
                SendPacketToClient(ranks);
            }


            for (var i = 0; i < membersCount; i++)
            {
                GuildRosterMemberData member = new GuildRosterMemberData();
                PlayerCache cache = new PlayerCache();
                member.Guid = packet.ReadGuid().To128(GetSession().GameState);
                member.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                member.Status = packet.ReadUInt8();
                member.Name = cache.Name = packet.ReadCString();
                member.RankID = packet.ReadInt32();
                member.Level = cache.Level = packet.ReadUInt8();
                member.ClassID = cache.ClassId =(Class)packet.ReadUInt8();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                    member.SexID = cache.SexId = (Gender)packet.ReadUInt8();
                GetSession().GameState.UpdatePlayerCache(member.Guid, cache);
                member.AreaID = packet.ReadInt32();

                if (member.Status == 0)
                    member.LastSave = packet.ReadFloat();
                else
                    member.Authenticated = true;

                member.Note = packet.ReadCString();
                member.OfficerNote = packet.ReadCString();
                guild.MemberData.Add(member);
            }
            SendPacketToClient(guild);
        }

        [PacketHandler(Opcode.SMSG_GUILD_INVITE)]
        void HandleGuildInvite(WorldPacket packet)
        {
            GuildInvite invite = new()
            {
                InviterName = packet.ReadCString(),
                InviterVirtualRealmAddress = GetSession().RealmId.GetAddress(),
                GuildName = packet.ReadCString(),
                GuildVirtualRealmAddress = GetSession().RealmId.GetAddress()
            };
            invite.GuildGUID = GetSession().GetGuildGuid(invite.GuildName);
            SendPacketToClient(invite);
        }

        [PacketHandler(Opcode.MSG_TABARDVENDOR_ACTIVATE)]
        void HandleTabardVendorActivate(WorldPacket packet)
        {
            PlayerTabardVendorActivate activate = new()
            {
                DesignerGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(activate);
        }

        [PacketHandler(Opcode.MSG_SAVE_GUILD_EMBLEM)]
        void HandleSaveGuildEmblem(WorldPacket packet)
        {
            PlayerSaveGuildEmblem emblem = new()
            {
                Error = (GuildEmblemError)packet.ReadUInt32()
            };
            SendPacketToClient(emblem);
        }

        [PacketHandler(Opcode.SMSG_GUILD_INVITE_DECLINED)]
        void HandleGuildInviteDeclined(WorldPacket packet)
        {
            GuildInviteDeclined invite = new()
            {
                InviterName = packet.ReadCString(),
                InviterVirtualRealmAddress = GetSession().RealmId.GetAddress()
            };
            SendPacketToClient(invite);
        }

        [PacketHandler(Opcode.SMSG_GUILD_BANK_QUERY_RESULTS)]
        void HandleGuildBankQueryResults(WorldPacket packet)
        {
            GuildBankQueryResults result = new()
            {
                Money = packet.ReadUInt64(),
                Tab = packet.ReadUInt8(),
                WithdrawalsRemaining = packet.ReadInt32()
            };

            bool hasTabs = false;
            if (packet.ReadBool() && result.Tab == 0)
            {
                hasTabs = true;
                var size = packet.ReadUInt8();
                for (var i = 0; i < size; i++)
                {
                    GuildBankTabInfo tabInfo = new GuildBankTabInfo
                    {
                        TabIndex = i,
                        Name = packet.ReadCString(),
                        Icon = packet.ReadCString()
                    };
                    result.TabInfo.Add(tabInfo);
                }
            }

            var slots = packet.ReadUInt8();
            for (var i = 0; i < slots; i++)
            {
                GuildBankItemInfo itemInfo = new GuildBankItemInfo
                {
                    Slot = packet.ReadUInt8()
                };
                int entry = packet.ReadInt32();
                if (entry > 0)
                {
                    itemInfo.Item.ItemID = (uint)entry;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                        itemInfo.Flags = packet.ReadUInt32();

                    itemInfo.Item.RandomPropertiesID = packet.ReadUInt32();
                    if (itemInfo.Item.RandomPropertiesID != 0)
                        itemInfo.Item.RandomPropertiesSeed = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                        itemInfo.Count = packet.ReadInt32();
                    else
                        itemInfo.Count = packet.ReadUInt8();

                    itemInfo.EnchantmentID = packet.ReadInt32();
                    itemInfo.Charges = packet.ReadUInt8();

                    var enchantments = packet.ReadUInt8();
                    for (var j = 0; j < enchantments; j++)
                    {
                        byte slot = packet.ReadUInt8();
                        uint enchantId = packet.ReadUInt32();
                        if (enchantId != 0)
                        {
                            uint itemId = GameData.GetGemFromEnchantId(enchantId);
                            if (itemId != 0)
                            {
                                ItemGemData gem = new ItemGemData
                                {
                                    Slot = slot
                                };
                                gem.Item.ItemID = itemId;
                                itemInfo.SocketEnchant.Add(gem);
                            }
                        }
                    }
                }
                result.ItemInfo.Add(itemInfo);
            }

            result.FullUpdate = (hasTabs && slots > 0);

            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.MSG_QUERY_GUILD_BANK_TEXT)]
        void HandleQueryGuildBankText(WorldPacket packet)
        {
            GuildBankTextQueryResult result = new()
            {
                Tab = packet.ReadUInt8(),
                Text = packet.ReadCString()
            };
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.MSG_GUILD_BANK_LOG_QUERY)]
        void HandleGuildBankLongQuery(WorldPacket packet)
        {
            const int maxTabs = 6;

            GuildBankLogQueryResults result = new()
            {
                Tab = packet.ReadUInt8()
            };
            byte logSize = packet.ReadUInt8();
            for (byte i = 0; i < logSize; i++)
            {
                GuildBankLogEntry logEntry = new GuildBankLogEntry
                {
                    EntryType = packet.ReadInt8(),
                    PlayerGUID = packet.ReadGuid().To128(GetSession().GameState)
                };

                if (result.Tab != maxTabs)
                {
                    logEntry.ItemID = packet.ReadInt32();
                    logEntry.Count = packet.ReadUInt8();
                    if ((GuildBankEventType)logEntry.EntryType == GuildBankEventType.MoveItem ||
                        (GuildBankEventType)logEntry.EntryType == GuildBankEventType.MoveItem2)
                        logEntry.OtherTab = packet.ReadInt8();
                }
                else
                    logEntry.Money = packet.ReadUInt32();

                logEntry.TimeOffset = packet.ReadUInt32();
                result.Entry.Add(logEntry);
            }
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN)]
        void HandleGuildBankMoneyWithdrawn(WorldPacket packet)
        {
            GuildBankRemainingWithdrawMoney result = new()
            {
                RemainingWithdrawMoney = packet.ReadUInt32()
            };
            SendPacketToClient(result);
        }
    }
}
