﻿using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items;
using NeoServer.Game.Contracts.World;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Contracts;
using NeoServer.Server.Contracts.Network;

namespace NeoServer.Server.Events
{
    public class ThingAddedToTileEventHandler
    {
        private readonly IGameServer game;

        public ThingAddedToTileEventHandler(IGameServer game)
        {
            this.game = game;
        }
        public void Execute(IThing thing, ICylinder cylinder)
        {
            if (Guard.AnyNull(cylinder, cylinder.TileSpectators, thing)) return;
            var tile = cylinder.ToTile;
            if(tile.IsNull()) return;

            var spectators = cylinder.TileSpectators;

            foreach (var spectator in spectators)
            {
                if (!game.CreatureManager.GetPlayerConnection(spectator.Spectator.CreatureId, out IConnection connection)) continue;
                
                if (spectator.Spectator is not IPlayer) continue;
                
                connection.OutgoingPackets.Enqueue(new AddTileItemPacket((IItem)thing, spectator.ToStackPosition));

                connection.Send();
            }
        }
    }
}
