﻿using NeoServer.Game.Contracts;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Contracts;
using NeoServer.Server.Contracts.Network;

namespace NeoServer.Server.Events.Creature
{
    public class CreatureHealedEventHandler
    {
        private readonly IMap map;
        private readonly IGameServer game;

        public CreatureHealedEventHandler(IMap map, IGameServer game)
        {
            this.map = map;
            this.game = game;
        }
        public void Execute(ICreature creature, ushort amount)
        {
            foreach (var spectator in map.GetPlayersAtPositionZone(creature.Location))
            {
                if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out IConnection connection))
                {
                    continue;
                }

                if (creature == spectator) //myself
                {
                    connection.OutgoingPackets.Enqueue(new PlayerStatusPacket((IPlayer)creature));
                }

                connection.OutgoingPackets.Enqueue(new CreatureHealthPacket(creature));

                connection.Send();
            }
        }
    }
}
