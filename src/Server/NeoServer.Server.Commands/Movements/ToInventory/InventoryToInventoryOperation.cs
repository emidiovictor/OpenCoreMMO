﻿using NeoServer.Game.Common.Location;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items.Types;
using NeoServer.Networking.Packets.Incoming;

namespace NeoServer.Server.Commands.Movement.ToInventory
{
    public class InventoryToInventoryOperation
    {
        public static void Execute(IPlayer player, ItemThrowPacket itemThrow)
        {
            if (player.Inventory[itemThrow.FromLocation.Slot] is not IPickupable item) return;

            player.MoveItem(player.Inventory, player.Inventory, item, itemThrow.Count, (byte)itemThrow.FromLocation.Slot, (byte)itemThrow.ToLocation.Slot);
        }

        public static bool IsApplicable(ItemThrowPacket itemThrowPacket) =>
          itemThrowPacket.FromLocation.Type == LocationType.Slot
          && itemThrowPacket.ToLocation.Type == LocationType.Slot;
    }
}
