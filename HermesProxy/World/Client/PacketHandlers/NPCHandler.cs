using Framework;
using Framework.GameMath;
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
        [PacketHandler(Opcode.SMSG_GOSSIP_MESSAGE)]
        void HandleGossipmessage(WorldPacket packet)
        {
            GossipMessagePkt gossip = new GossipMessagePkt();
            gossip.GossipGUID = packet.ReadGuid().To128();
            GetSession().GameState.CurrentInteractedWithNPC = gossip.GossipGUID;

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                gossip.GossipID = packet.ReadInt32();
            else
                gossip.GossipID = (int)gossip.GossipGUID.GetEntry();

            gossip.TextID = packet.ReadInt32();

            uint optionsCount = packet.ReadUInt32();

            for (uint i = 0; i < optionsCount; i++)
            {
                ClientGossipOption option = new ClientGossipOption();
                option.OptionIndex = packet.ReadInt32();
                option.OptionIcon = packet.ReadUInt8();
                bool hasBox = packet.ReadBool();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    option.OptionCost = packet.ReadInt32();

                option.Text = packet.ReadCString();

                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    option.Confirm = packet.ReadCString();
                gossip.GossipOptions.Add(option);
            }

            uint questsCount = packet.ReadUInt32();

            for (uint i = 0; i < questsCount; i++)
            {
                ClientGossipQuest quest = ReadGossipQuestOption(packet);
                gossip.GossipQuests.Add(quest);
            }

            SendPacketToClient(gossip);
        }

        [PacketHandler(Opcode.SMSG_GOSSIP_COMPLETE)]
        void HandleGossipComplete(WorldPacket packet)
        {
            GossipComplete gossip = new GossipComplete();
            SendPacketToClient(gossip);
        }

        [PacketHandler(Opcode.SMSG_GOSSIP_POI)]
        void HandleGossipPoi(WorldPacket packet)
        {
            GossipPOI poi = new();
            poi.Flags = packet.ReadUInt32();
            poi.Pos = new Vector3(packet.ReadVector2());
            poi.Icon = packet.ReadUInt32();
            poi.Importance = packet.ReadUInt32();
            poi.Name = packet.ReadCString();
            SendPacketToClient(poi);
        }

        [PacketHandler(Opcode.SMSG_BINDER_CONFIRM)]
        void HandleBinderConfirm(WorldPacket packet)
        {
            BinderConfirm confirm = new BinderConfirm();
            confirm.Guid = packet.ReadGuid().To128();
            GetSession().GameState.CurrentInteractedWithNPC = confirm.Guid;
            SendPacketToClient(confirm);
        }

        [PacketHandler(Opcode.SMSG_VENDOR_INVENTORY)]
        void HandleVendorInventory(WorldPacket packet)
        {
            VendorInventory vendor = new VendorInventory();
            vendor.VendorGUID = packet.ReadGuid().To128();
            GetSession().GameState.CurrentInteractedWithNPC = vendor.VendorGUID;
            byte itemsCount = packet.ReadUInt8();

            if (itemsCount == 0)
            {
                vendor.Reason = packet.ReadUInt8();
                SendPacketToClient(vendor);
                return;
            }

            for (byte i = 0; i < itemsCount; i++)
            {
                VendorItem vendorItem = new();
                vendorItem.Slot = packet.ReadInt32();
                vendorItem.Item.ItemID = packet.ReadUInt32();
                packet.ReadUInt32(); // Display Id
                vendorItem.Quantity = packet.ReadInt32();
                vendorItem.Price = packet.ReadUInt32();
                vendorItem.Durability = packet.ReadInt32();
                vendorItem.StackCount = packet.ReadUInt32();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    vendorItem.ExtendedCostID = packet.ReadInt32();
                GetSession().GameState.SetItemBuyCount(vendorItem.Item.ItemID, vendorItem.StackCount);
                vendor.Items.Add(vendorItem);
            }

            SendPacketToClient(vendor);
        }

        [PacketHandler(Opcode.SMSG_SHOW_BANK)]
        void HandleShowBank(WorldPacket packet)
        {
            ShowBank bank = new ShowBank();
            bank.Guid = packet.ReadGuid().To128();
            GetSession().GameState.CurrentInteractedWithNPC = bank.Guid;
            SendPacketToClient(bank);
        }

        [PacketHandler(Opcode.SMSG_TRAINER_LIST)]
        void HandleTrainerList(WorldPacket packet)
        {
            TrainerList trainer = new TrainerList();
            trainer.TrainerGUID = packet.ReadGuid().To128();
            GetSession().GameState.CurrentInteractedWithNPC = trainer.TrainerGUID;
            trainer.TrainerID = trainer.TrainerGUID.GetEntry();
            trainer.TrainerType = packet.ReadInt32();
            int count = packet.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                TrainerListSpell spell = new();
                uint spellId = packet.ReadUInt32();
                if (ModernVersion.GetExpansionVersion() > 1 &&
                    LegacyVersion.GetExpansionVersion() <= 1)
                {
                    // in vanilla the server sends learn spell with effect 36
                    // in expansions the server sends the actual spell
                    uint realSpellId = GameData.GetRealSpell(spellId);
                    if (realSpellId != spellId)
                    {
                        GetSession().GameState.StoreRealSpell(realSpellId, spellId);
                        spellId = realSpellId;
                    }
                }
                spell.SpellID = spellId;
                TrainerSpellStateLegacy stateOld = (TrainerSpellStateLegacy)packet.ReadUInt8();
                TrainerSpellStateModern stateNew = (TrainerSpellStateModern)Enum.Parse(typeof(TrainerSpellStateModern), stateOld.ToString());
                spell.Usable = stateNew;
                spell.MoneyCost = packet.ReadUInt32();
                packet.ReadInt32(); // Profession Dialog
                packet.ReadInt32(); // Profession Button
                spell.ReqLevel = packet.ReadUInt8();
                spell.ReqSkillLine = packet.ReadUInt32();
                spell.ReqSkillRank = packet.ReadUInt32();
                spell.ReqAbility[0] = packet.ReadUInt32();
                spell.ReqAbility[1] = packet.ReadUInt32();
                spell.ReqAbility[2] = packet.ReadUInt32();
                trainer.Spells.Add(spell);
            }
            trainer.Greeting = packet.ReadCString();
            SendPacketToClient(trainer);
        }

        [PacketHandler(Opcode.SMSG_TRAINER_BUY_FAILED)]
        void HandleTrainerBuyFailed(WorldPacket packet)
        {
            TrainerBuyFailed buy = new();
            buy.TrainerGUID = packet.ReadGuid().To128();
            buy.SpellID = packet.ReadUInt32();
            buy.TrainerFailedReason = packet.ReadUInt32();
            SendPacketToClient(buy);
            ChatPkt chat = new ChatPkt(GetSession(), ChatMessageTypeModern.System, 0, null, "", null, "", $"Failed to learn Spell {buy.SpellID} (Reason {buy.TrainerFailedReason}).", "", ChatFlags.None, 0);
            SendPacketToClient(chat);
        }

        [PacketHandler(Opcode.MSG_TALENT_WIPE_CONFIRM)]
        void HandleTalentWipeConfirm(WorldPacket packet)
        {
            RespecWipeConfirm respec = new();
            respec.TrainerGUID = packet.ReadGuid().To128();
            respec.Cost = packet.ReadUInt32();
            SendPacketToClient(respec);
        }

        [PacketHandler(Opcode.SMSG_SPIRIT_HEALER_CONFIRM)]
        void HandleSpiritHealerConfirm(WorldPacket packet)
        {
            SpiritHealerConfirm confirm = new SpiritHealerConfirm();
            confirm.Guid = packet.ReadGuid().To128();
            SendPacketToClient(confirm);
        }
    }
}
