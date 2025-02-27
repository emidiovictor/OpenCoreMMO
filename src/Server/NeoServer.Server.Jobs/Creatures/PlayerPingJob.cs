﻿using NeoServer.Game.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Commands;
using NeoServer.Server.Contracts;
using System;

namespace NeoServer.Server.Jobs
{
    public class PlayerPingJob
    {
        private const int PING_INTERVAL = 5000;
        private const int CONNECTION_LOST_INTERVAL = 60000;
        public static void Execute(IPlayer player, PlayerLogOutCommand playerLogOutCommand, IGameServer game)
        {
            if (player.IsDead)
            {
                return;
            }

            var now = DateTime.Now.Ticks;

            if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection))
            {
                return;
            }

            var remainingTime = TimeSpan.FromTicks(now - connection.LastPingRequest).TotalMilliseconds;

            if (remainingTime >= PING_INTERVAL)
            {
                connection.LastPingRequest = now;

                connection.Send(new PingPacket());
            }

            var noPongTime = TimeSpan.FromTicks(now - connection.LastPingResponse).TotalMilliseconds;

            if (noPongTime >= CONNECTION_LOST_INTERVAL && connection.LastPingResponse > 0)
            {
                playerLogOutCommand.Execute(player, forced: true);
            }
        }
    }
}
