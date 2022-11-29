using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_DB_QUERY_BULK)]
        void HandleDbQueryBulk(DBQueryBulk query)
        {
            foreach (uint id in query.Queries)
            {
                DBReply reply = new()
                {
                    RecordID = id,
                    TableHash = query.TableHash,
                    Status = HotfixStatus.Invalid,
                    Timestamp = (uint)Time.UnixTime
                };

                if (query.TableHash == DB2Hash.BroadcastText)
                {
                    BroadcastText bct = GameData.GetBroadcastText(id);
                    if (bct == null)
                    {
                        bct = new BroadcastText
                        {
                            Entry = id,
                            MaleText = "Clear your cache!",
                            FemaleText = "Clear your cache!"
                        };
                    }

                    reply.Status = HotfixStatus.Valid;
                    reply.Data.WriteCString(bct.MaleText);
                    reply.Data.WriteCString(bct.FemaleText);
                    reply.Data.WriteUInt32(bct.Entry);
                    reply.Data.WriteUInt32(bct.Language);
                    reply.Data.WriteUInt32(0); // ConditionId
                    reply.Data.WriteUInt16(0); // EmotesId
                    reply.Data.WriteUInt8(0); // Flags
                    reply.Data.WriteUInt32(0); // ChatBubbleDurationMs
                    if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
                        reply.Data.WriteUInt32(0); // VoiceOverPriorityID
                    for (int i = 0; i < 2; ++i)
                        reply.Data.WriteUInt32(0); // SoundEntriesID
                    for (int i = 0; i < 3; ++i)
                        reply.Data.WriteUInt16(bct.Emotes[i]);
                    for (int i = 0; i < 3; ++i)
                        reply.Data.WriteUInt16(bct.EmoteDelays[i]);
                }
                else if (query.TableHash == DB2Hash.ItemSparse)
                {
                    ItemTemplate item = GameData.GetItemTemplate(id);
                    if (item != null)
                    {
                        reply.Status = HotfixStatus.Valid;
                        reply.Data.WriteInt64(item.AllowedRaces);
                        reply.Data.WriteCString(item.Description);
                        reply.Data.WriteCString(item.Name[3]);
                        reply.Data.WriteCString(item.Name[2]);
                        reply.Data.WriteCString(item.Name[1]);
                        reply.Data.WriteCString(item.Name[0]);
                        reply.Data.WriteFloat(1);
                        reply.Data.WriteUInt32(item.Duration);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteUInt32(item.BagFamily);
                        reply.Data.WriteFloat(item.RangedMod);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteFloat(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(item.MaxStackSize);
                        reply.Data.WriteInt32(item.MaxCount);
                        reply.Data.WriteUInt32(item.RequiredSpell);
                        reply.Data.WriteUInt32(item.SellPrice);
                        reply.Data.WriteUInt32(item.BuyPrice);
                        reply.Data.WriteUInt32(item.BuyCount);
                        reply.Data.WriteFloat(1);
                        reply.Data.WriteFloat(1);
                        reply.Data.WriteUInt32(item.Flags);
                        reply.Data.WriteUInt32(item.FlagsExtra);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteInt32(0);
                        reply.Data.WriteUInt32(item.MaxDurability);
                        reply.Data.WriteUInt16(0);
                        reply.Data.WriteUInt16(0);
                        reply.Data.WriteUInt16((ushort)item.HolidayID);
                        reply.Data.WriteUInt16((ushort)item.ItemLimitCategory);
                        reply.Data.WriteUInt16((ushort)item.GemProperties);
                        reply.Data.WriteUInt16((ushort)item.SocketBonus);
                        reply.Data.WriteUInt16((ushort)item.TotemCategory);
                        reply.Data.WriteUInt16((ushort)item.MapID);
                        reply.Data.WriteUInt16((ushort)item.AreaID);
                        reply.Data.WriteUInt16(0);
                        reply.Data.WriteUInt16((ushort)item.ItemSet);
                        reply.Data.WriteUInt16((ushort)item.LockId);
                        reply.Data.WriteUInt16((ushort)item.StartQuestId);
                        reply.Data.WriteUInt16((ushort)item.PageText);
                        reply.Data.WriteUInt16((ushort)item.Delay);
                        reply.Data.WriteUInt16((ushort)item.RequiredRepFaction);
                        reply.Data.WriteUInt16((ushort)item.RequiredSkillLevel);
                        reply.Data.WriteUInt16((ushort)item.RequiredSkillId);
                        reply.Data.WriteUInt16((ushort)item.ItemLevel);
                        reply.Data.WriteInt16((short)item.AllowedClasses);
                        reply.Data.WriteUInt16((ushort)item.RandomSuffix);
                        reply.Data.WriteUInt16((ushort)item.RandomProperty);
                        reply.Data.WriteUInt16((ushort)item.DamageMins[0]);
                        reply.Data.WriteUInt16((ushort)item.DamageMins[1]);
                        reply.Data.WriteUInt16((ushort)item.DamageMins[2]);
                        reply.Data.WriteUInt16((ushort)item.DamageMins[3]);
                        reply.Data.WriteUInt16((ushort)item.DamageMins[4]);
                        reply.Data.WriteUInt16((ushort)item.DamageMaxs[0]);
                        reply.Data.WriteUInt16((ushort)item.DamageMaxs[1]);
                        reply.Data.WriteUInt16((ushort)item.DamageMaxs[2]);
                        reply.Data.WriteUInt16((ushort)item.DamageMaxs[3]);
                        reply.Data.WriteUInt16((ushort)item.DamageMaxs[4]);
                        reply.Data.WriteInt16((short)item.Armor);
                        reply.Data.WriteInt16((short)item.HolyResistance);
                        reply.Data.WriteInt16((short)item.FireResistance);
                        reply.Data.WriteInt16((short)item.NatureResistance);
                        reply.Data.WriteInt16((short)item.FrostResistance);
                        reply.Data.WriteInt16((short)item.ShadowResistance);
                        reply.Data.WriteInt16((short)item.ArcaneResistance);
                        reply.Data.WriteUInt16((ushort)item.ScalingStatDistribution);
                        reply.Data.WriteUInt8(254);
                        reply.Data.WriteUInt8(0);
                        reply.Data.WriteUInt8(0);
                        reply.Data.WriteUInt8(0);
                        reply.Data.WriteUInt8((byte)item.ItemSocketColors[0]);
                        reply.Data.WriteUInt8((byte)item.ItemSocketColors[1]);
                        reply.Data.WriteUInt8((byte)item.ItemSocketColors[2]);
                        reply.Data.WriteUInt8((byte)item.SheathType);
                        reply.Data.WriteUInt8((byte)item.Material);
                        reply.Data.WriteUInt8((byte)item.PageMaterial);
                        reply.Data.WriteUInt8((byte)item.Language);
                        reply.Data.WriteUInt8((byte)item.Bonding);
                        reply.Data.WriteUInt8((byte)item.DamageTypes[0]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[0]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[1]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[2]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[3]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[4]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[5]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[6]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[7]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[8]);
                        reply.Data.WriteInt8((sbyte)item.StatTypes[9]);
                        reply.Data.WriteUInt8((byte)item.ContainerSlots);
                        reply.Data.WriteUInt8((byte)item.RequiredRepValue);
                        reply.Data.WriteUInt8((byte)item.RequiredCityRank);
                        reply.Data.WriteUInt8((byte)item.RequiredHonorRank);
                        reply.Data.WriteUInt8((byte)item.InventoryType);
                        reply.Data.WriteUInt8((byte)item.Quality);
                        reply.Data.WriteUInt8((byte)item.AmmoType);
                        reply.Data.WriteInt8((sbyte)item.StatValues[0]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[1]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[2]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[3]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[4]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[5]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[6]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[7]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[8]);
                        reply.Data.WriteInt8((sbyte)item.StatValues[9]);
                        reply.Data.WriteInt8((sbyte)item.RequiredLevel);
                    }
                }

                SendPacket(reply);
            }
        }

        [PacketHandler(Opcode.CMSG_HOTFIX_REQUEST)]
        void HandleHotfixRequest(HotfixRequest request)
        {
            HotfixConnect connect = new HotfixConnect();
            foreach (uint id in request.Hotfixes)
            {
                HotfixRecord record;
                if (GameData.Hotfixes.TryGetValue(id, out record))
                {
                    Log.Print(LogType.Debug, $"Hotfix record {record.RecordId} from {record.TableHash}.");
                    connect.Hotfixes.Add(record);
                }
            }
            SendPacket(connect);
        }
    }
}
