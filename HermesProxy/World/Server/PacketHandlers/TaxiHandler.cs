using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_TAXI_NODE_STATUS_QUERY)]
        [PacketHandler(Opcode.CMSG_TAXI_QUERY_AVAILABLE_NODES)]
        void HandleTaxiNodesQuery(InteractWithNPC interact)
        {
            WorldPacket packet = new(interact.GetUniversalOpcode());
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ENABLE_TAXI_NODE)]
        void HandleEnableTaxiNode(InteractWithNPC interact)
        {
            WorldPacket packet = new(Opcode.CMSG_TALK_TO_GOSSIP);
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ACTIVATE_TAXI)]
        void HandleActivateTaxi(ActivateTaxi taxi)
        {
            // direct path exist
            if (TaxiPathExist(GetSession().GameState.CurrentTaxiNode, taxi.Node))
            {
                WorldPacket packet = new(Opcode.CMSG_ACTIVATE_TAXI);
                packet.WriteGuid(taxi.FlightMaster.To64());
                packet.WriteUInt32(GetSession().GameState.CurrentTaxiNode);
                packet.WriteUInt32(taxi.Node);
                SendPacketToServer(packet);
            }
            else // find shortest path
            {
                HashSet<uint> path = GetTaxiPath(GetSession().GameState.CurrentTaxiNode, taxi.Node, GetSession().GameState.UsableTaxiNodes);
                if (path.Count <= 1) // no nodes found
                    return;

                WorldPacket packet = new(Opcode.CMSG_ACTIVATE_TAXI_EXPRESS);
                packet.WriteGuid(taxi.FlightMaster.To64());
                packet.WriteUInt32(0);                // total cost, not used
                packet.WriteUInt32((uint)path.Count); // node count
                foreach (uint itr in path)
                    packet.WriteUInt32(itr);
                SendPacketToServer(packet);
            }
            GetSession().GameState.IsWaitingForTaxiStart = true;
        }
        bool TaxiPathExist(uint from, uint to)
        {
            foreach (var itr in GameData.TaxiPaths)
            {
                if (itr.Value.From == from && itr.Value.To == to ||
                    itr.Value.From == to && itr.Value.To == from)
                    return true;
            }
            return false;
        }
        bool IsTaxiNodeKnown(uint node, List<byte> usableNodes)
        {
            byte field = (byte)((node - 1) / 8);
            uint submask = (uint)1 << (byte)((node - 1) % 8);
            return (usableNodes[field] & submask) == submask;
        }
        HashSet<uint> GetTaxiPath(uint from, uint to, List<byte> usableNodes)
        {
            // shortest path node list
            HashSet<uint> nodes = new() { from };
            // copy taxi nodes graph and disable unknown nodes
            int[,] graphCopy = new int[GameData.TaxiNodesGraph.GetLength(0), GameData.TaxiNodesGraph.GetLength(1)];
            Buffer.BlockCopy(GameData.TaxiNodesGraph, 0, graphCopy, 0, GameData.TaxiNodesGraph.Length * sizeof(uint));
            for (uint i = 1; i < graphCopy.GetLength(0); i++)
            {
                if (!IsTaxiNodeKnown(i, usableNodes))
                {
                    for (uint itr = 0; itr < graphCopy.GetLength(1); itr++)
                        graphCopy[i, itr] = 0;

                    for (uint itr = 0; itr < graphCopy.GetLength(0); itr++)
                        graphCopy[itr, i] = 0;
                }
            }
            int minDist = Dijkstra(graphCopy, (int)from, (int)to, graphCopy.GetLength(0), nodes);
            return nodes;
        }
        int MinDistance(int[] dist, bool[] sptSet, int vCnt)
        {
            int min = int.MaxValue, min_index = -1;
            for (int v = 0; v < vCnt; v++)
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }
            return min_index;
        }
        void SavePath(int[] parent, int j, HashSet<uint> nodes)
        {
            if (parent[j] == -1)
                return;
            SavePath(parent, parent[j], nodes);
            nodes.Add((uint)j);
        }
        // taken from https://www.geeksforgeeks.org/printing-paths-dijkstras-shortest-path-algorithm/
        int Dijkstra(int[,] graph, int src, int dest, int vCnt, HashSet<uint> nodes)
        {
            int[] dist = new int[vCnt];
            int[] parent = new int[vCnt];
            bool[] sptSet = new bool[vCnt];
            for (int i = 0; i < vCnt; i++)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
                parent[i] = -1;
            }
            dist[src] = 0;
            for (int count = 0; count < vCnt - 1; count++)
            {
                int u = MinDistance(dist, sptSet, vCnt);
                sptSet[u] = true;

                for (int v = 0; v < vCnt; v++)
                {
                    if (!sptSet[v] && graph[u, v] != 0 &&
                         dist[u] != int.MaxValue && dist[u] + graph[u, v] < dist[v])
                    {
                        parent[v] = u;
                        dist[v] = dist[u] + graph[u, v];
                    }
                }
            }
            // save shortest path
            SavePath(parent, dest, nodes);
            // return shortest path distance
            return dist[dest];
        }
    }
}
