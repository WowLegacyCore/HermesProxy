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
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class CreateObjectData
    {
        public ObjectType ObjectType;
        public MovementInfo MoveInfo;
        public ServerSideMovement MoveSpline;
        public bool NoBirthAnim;
        public bool EnablePortals;
        public bool PlayHoverAnim;
        public bool ThisIsYou;
        public WowGuid128 AutoAttackVictim;
    }
    public class ObjectUpdate
    {
        public ObjectUpdate(WowGuid128 guid, UpdateTypeModern type)
        {
            Type = type;
            Guid = guid;
            ObjectData = new ObjectData();

            switch (type)
            {
                case UpdateTypeModern.CreateObject1:
                case UpdateTypeModern.CreateObject2:
                    CreateData = new CreateObjectData();
                    break;
            }

            switch (guid.GetObjectType())
            {
                case ObjectType.Unit:
                    UnitData = new UnitData();
                    break;
                case ObjectType.Player:
                case ObjectType.ActivePlayer:
                    UnitData = new UnitData();
                    PlayerData = new PlayerData();
                    break;
            }

        }

        public UpdateTypeModern Type;
        public WowGuid128 Guid;
        public CreateObjectData CreateData;
        public ObjectData ObjectData;
        public UnitData UnitData;
        public PlayerData PlayerData;
    }
    
    public class UpdateObject : ServerPacket
    {
        public UpdateObject() : base(Opcode.SMSG_UPDATE_OBJECT, ConnectionType.Instance) { }

        public override void Write()
        {
            _worldPacket.WriteUInt32(NumObjUpdates);
            _worldPacket.WriteUInt16(MapID);
            _worldPacket.WriteBytes(Data);
        }

        public uint NumObjUpdates;
        public ushort MapID;
        public byte[] Data;

        List<WowGuid128> OutOfRangeGuids = new List<WowGuid128>();
        List<WowGuid128> DestroyedGuids = new List<WowGuid128>();
        List<ObjectUpdate> ObjectUpdates = new List<ObjectUpdate>();
    }
}
