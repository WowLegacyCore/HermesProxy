using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HermesProxy.World.Enums;
using HermesProxy.Enums;

namespace HermesProxy
{
    public abstract class HighGuid
    {
        protected HighGuidType highGuidType;

        public HighGuidType GetHighGuidType()
        {
            return highGuidType;
        }

    }

    public class HighGuidLegacy : HighGuid
    {
        HighGuidTypeLegacy high;
        static readonly Dictionary<HighGuidTypeLegacy, HighGuidType> HighLegacyToHighType
            = new Dictionary<HighGuidTypeLegacy, HighGuidType>
        {
            { HighGuidTypeLegacy.None, HighGuidType.Null },
            { HighGuidTypeLegacy.Player, HighGuidType.Player },
            { HighGuidTypeLegacy.Group, HighGuidType.RaidGroup },
            { HighGuidTypeLegacy.MOTransport, HighGuidType.MOTransport }, // ?? unused in wpp
            { HighGuidTypeLegacy.Item, HighGuidType.Item },
            { HighGuidTypeLegacy.DynamicObject, HighGuidType.DynamicObject },
            { HighGuidTypeLegacy.GameObject, HighGuidType.GameObject },
            { HighGuidTypeLegacy.Transport, HighGuidType.Transport },
            { HighGuidTypeLegacy.Creature, HighGuidType.Creature },
            { HighGuidTypeLegacy.Pet, HighGuidType.Pet },
            { HighGuidTypeLegacy.Vehicle, HighGuidType.Vehicle },
            { HighGuidTypeLegacy.Corpse, HighGuidType.Corpse },
        };

        public HighGuidLegacy(HighGuidTypeLegacy high)
        {
            this.high = high;
            if (!HighLegacyToHighType.ContainsKey(high))
                throw new ArgumentOutOfRangeException("0x" + high.ToString("X"));

            highGuidType = HighLegacyToHighType[high];
        }
    }

    public class HighGuid703 : HighGuid
    {
        protected byte high;
        static readonly Dictionary<HighGuidType703, HighGuidType> High703ToHighType
            = new Dictionary<HighGuidType703, HighGuidType>
        {
            { HighGuidType703.Null,              HighGuidType.Null },
            { HighGuidType703.Uniq,              HighGuidType.Uniq },
            { HighGuidType703.Player,            HighGuidType.Player },
            { HighGuidType703.Item,              HighGuidType.Item },
            { HighGuidType703.WorldTransaction,  HighGuidType.WorldTransaction },
            { HighGuidType703.StaticDoor,        HighGuidType.StaticDoor },
            { HighGuidType703.Transport,         HighGuidType.Transport },
            { HighGuidType703.Conversation,      HighGuidType.Conversation },
            { HighGuidType703.Creature,          HighGuidType.Creature },
            { HighGuidType703.Vehicle,           HighGuidType.Vehicle },
            { HighGuidType703.Pet,               HighGuidType.Pet },
            { HighGuidType703.GameObject,        HighGuidType.GameObject },
            { HighGuidType703.DynamicObject,     HighGuidType.DynamicObject },
            { HighGuidType703.AreaTrigger,       HighGuidType.AreaTrigger },
            { HighGuidType703.Corpse,            HighGuidType.Corpse },
            { HighGuidType703.LootObject,        HighGuidType.LootObject },
            { HighGuidType703.SceneObject,       HighGuidType.SceneObject },
            { HighGuidType703.Scenario,          HighGuidType.Scenario },
            { HighGuidType703.AIGroup,           HighGuidType.AIGroup },
            { HighGuidType703.DynamicDoor,       HighGuidType.DynamicDoor },
            { HighGuidType703.ClientActor,       HighGuidType.ClientActor },
            { HighGuidType703.Vignette,          HighGuidType.Vignette },
            { HighGuidType703.CallForHelp,       HighGuidType.CallForHelp },
            { HighGuidType703.AIResource,        HighGuidType.AIResource },
            { HighGuidType703.AILock,            HighGuidType.AILock },
            { HighGuidType703.AILockTicket,      HighGuidType.AILockTicket },
            { HighGuidType703.ChatChannel,       HighGuidType.ChatChannel },
            { HighGuidType703.Party,             HighGuidType.Party },
            { HighGuidType703.Guild,             HighGuidType.Guild },
            { HighGuidType703.WowAccount,        HighGuidType.WowAccount },
            { HighGuidType703.BNetAccount,       HighGuidType.BNetAccount },
            { HighGuidType703.GMTask,            HighGuidType.GMTask },
            { HighGuidType703.MobileSession,     HighGuidType.MobileSession },
            { HighGuidType703.RaidGroup,         HighGuidType.RaidGroup },
            { HighGuidType703.Spell,             HighGuidType.Spell },
            { HighGuidType703.Mail,              HighGuidType.Mail },
            { HighGuidType703.WebObj,            HighGuidType.WebObj },
            { HighGuidType703.LFGObject,         HighGuidType.LFGObject },
            { HighGuidType703.LFGList,           HighGuidType.LFGList },
            { HighGuidType703.UserRouter,        HighGuidType.UserRouter },
            { HighGuidType703.PVPQueueGroup,     HighGuidType.PVPQueueGroup },
            { HighGuidType703.UserClient,        HighGuidType.UserClient },
            { HighGuidType703.PetBattle,         HighGuidType.PetBattle },
            { HighGuidType703.UniqUserClient,    HighGuidType.UniqUserClient },
            { HighGuidType703.BattlePet,         HighGuidType.BattlePet },
            { HighGuidType703.CommerceObj,       HighGuidType.CommerceObj },
            { HighGuidType703.ClientSession,     HighGuidType.ClientSession },
            { HighGuidType703.Cast,              HighGuidType.Cast },
            { HighGuidType703.Invalid,           HighGuidType.Invalid }
        };

        public HighGuid703(byte high)
        {
            this.high = high;
            if (!High703ToHighType.ContainsKey((HighGuidType703)high))
                throw new ArgumentOutOfRangeException("0x" + high.ToString("X"));

            highGuidType = High703ToHighType[(HighGuidType703)high];
        }
    }
}
