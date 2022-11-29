using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_GAME_OBJECT_DESPAWN)]
        void HandleGameObjectDespawn(WorldPacket packet)
        {
            WowGuid64 guid = packet.ReadGuid();
            GameObjectDespawn despawn = new GameObjectDespawn
            {
                ObjectGUID = guid.To128(GetSession().GameState)
            };
            SendPacketToClient(despawn);
            GetSession().GameState.DespawnedGameObjects.Add(guid);
        }

        [PacketHandler(Opcode.SMSG_GAME_OBJECT_RESET_STATE)]
        void HandleGameObjectResetState(WorldPacket packet)
        {
            GameObjectResetState reset = new GameObjectResetState
            {
                ObjectGUID = packet.ReadGuid().To128(GetSession().GameState)
            };
            SendPacketToClient(reset);
        }

        [PacketHandler(Opcode.SMSG_GAME_OBJECT_CUSTOM_ANIM)]
        void HandleGameObjectCustomAnim(WorldPacket packet)
        {
            GameObjectCustomAnim anim = new GameObjectCustomAnim
            {
                ObjectGUID = packet.ReadGuid().To128(GetSession().GameState),
                CustomAnim = packet.ReadUInt32()
            };
            SendPacketToClient(anim);
        }

        [PacketHandler(Opcode.SMSG_FISH_NOT_HOOKED)]
        void HandleFishNotHooked(WorldPacket packet)
        {
            FishNotHooked fish = new FishNotHooked();
            SendPacketToClient(fish);
        }

        [PacketHandler(Opcode.SMSG_FISH_ESCAPED)]
        void HandleFishEscaped(WorldPacket packet)
        {
            FishEscaped fish = new FishEscaped();
            SendPacketToClient(fish);
        }
    }
}
