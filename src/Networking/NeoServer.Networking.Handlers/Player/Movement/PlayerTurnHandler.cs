﻿using NeoServer.Game.Common.Location;
using NeoServer.Game.Contracts;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Server.Contracts;
using NeoServer.Server.Contracts.Network;
using NeoServer.Server.Contracts.Network.Enums;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Handlers.Players
{
    public class PlayerTurnHandler : PacketHandler
    {
        private readonly IGameServer game;
        private readonly IMap map;

        public PlayerTurnHandler(IGameServer game, IMap map)
        {
            this.game = game;

            this.map = map;
        }

        public override void HandlerMessage(IReadOnlyNetworkMessage message, IConnection connection)
        {
            Direction direction = ParseTurnPacket(message.IncomingPacket);

            if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out IPlayer player)) return;
            
            game.Dispatcher.AddEvent(new Event(() => player.TurnTo(direction)));
        }

        private Direction ParseTurnPacket(GameIncomingPacketType turnPacket)
        {
            var direction = Direction.North;

            switch (turnPacket)
            {
                case GameIncomingPacketType.TurnNorth:
                    direction = Direction.North;
                    break;
                case GameIncomingPacketType.TurnEast:
                    direction = Direction.East;
                    break;
                case GameIncomingPacketType.TurnSouth:
                    direction = Direction.South;
                    break;
                case GameIncomingPacketType.TurnWest:
                    direction = Direction.West;
                    break;
            }

            return direction;
        }
    }
}
