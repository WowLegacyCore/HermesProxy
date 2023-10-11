using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
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
                DBReply reply = new();
                reply.RecordID = id;
                reply.TableHash = query.TableHash;
                reply.Status = HotfixStatus.Invalid;
                reply.Timestamp = (uint)Time.UnixTime;

                Log.PrintNet(LogType.Debug, LogNetDir.C2P, $"DB_QUERY_BULK requested ({query.TableHash}) #{id}");

                if (query.TableHash == DB2Hash.BroadcastText)
                {
                    BroadcastText bct = GameData.GetBroadcastText(id);
                    if (bct == null)
                    {
                        bct = new BroadcastText();
                        bct.Entry = id;
                        bct.MaleText = "Clear your cache!";
                        bct.FemaleText = "Clear your cache!";
                    }

                    //Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"Sending broadcast text #{id}");
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
                else if (query.TableHash == DB2Hash.Item)
                {
                    ItemTemplate item = GameData.GetItemTemplate(id);
                    if (item != null)
                    {
                        //Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"Sending custom ({DB2Hash.Item}) #{id}");
                        reply.Status = HotfixStatus.Valid;
                        GameData.WriteItemHotfix(item, reply.Data);
                    }
                    else if (!GetSession().GameState.RequestedItemHotfixes.Contains(id) &&
                              GetSession().WorldClient != null && GetSession().WorldClient.IsConnected())
                    {
                        //Log.PrintNet(LogType.Storage, LogNetDir.P2S, $"Item #{id} not cached, requesting server data...");
                        GetSession().GameState.RequestedItemHotfixes.Add(id);
                        WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                        packet2.WriteUInt32(id);
                        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                            packet2.WriteGuid(WowGuid64.Empty);
                        SendPacketToServer(packet2);
                        continue;
                    }
                }
                else if (query.TableHash == DB2Hash.ItemSparse)
                {
                    ItemTemplate item = GameData.GetItemTemplate(id);
                    if (item != null)
                    {
                        //Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"Sending custom ({DB2Hash.ItemSparse}) #{id}");
                        reply.Status = HotfixStatus.Valid;
                        GameData.WriteItemSparseHotfix(item, reply.Data);
                    }
                    else if (!GetSession().GameState.RequestedItemSparseHotfixes.Contains(id) &&
                              GetSession().WorldClient != null && GetSession().WorldClient.IsConnected())
                    {
                        GetSession().GameState.RequestedItemSparseHotfixes.Add(id);
                        //Log.PrintNet(LogType.Storage, LogNetDir.P2S, $"ItemSparse #{id} not cached, requesting server data...");
                        WorldPacket packet2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
                        packet2.WriteUInt32(id);
                        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                            packet2.WriteGuid(WowGuid64.Empty);
                        SendPacketToServer(packet2);
                        continue;
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
