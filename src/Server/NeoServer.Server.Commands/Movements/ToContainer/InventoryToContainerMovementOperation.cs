﻿using NeoServer.Game.Common.Location;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items.Types;
using NeoServer.Networking.Packets.Incoming;

namespace NeoServer.Server.Commands.Movement
{
    public class InventoryToContainerMovementOperation
    {
        public static void Execute(IPlayer player, ItemThrowPacket itemThrow)
        {
            var container = player.Containers[itemThrow.ToLocation.ContainerId];

            if (container is null) return;

            if (player.Inventory[itemThrow.FromLocation.Slot] is not IPickupable item) return;

            player.MoveItem(player.Inventory, container, item, itemThrow.Count, (byte)itemThrow.FromLocation.Slot, (byte) itemThrow.ToLocation.ContainerSlot);
        }

        public static bool IsApplicable(ItemThrowPacket itemThrowPacket) =>
          itemThrowPacket.FromLocation.Type == LocationType.Slot
          && itemThrowPacket.ToLocation.Type == LocationType.Container;
    }
}
