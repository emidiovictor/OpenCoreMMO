﻿using NeoServer.Game.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Contracts;

namespace NeoServer.Server.Events
{
    public class PlayerWalkCancelledEventHandler
    {
        private readonly IGameServer game;

        public PlayerWalkCancelledEventHandler(IGameServer game)
        {
            this.game = game;
        }
        public void Execute(IPlayer player)
        {
            if (game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection))
            {
                connection.OutgoingPackets.Enqueue(new PlayerWalkCancelPacket(player));
                connection.Send();
            }
        }
    }
}