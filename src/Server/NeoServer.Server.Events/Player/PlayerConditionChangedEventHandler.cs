﻿using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Contracts;

namespace NeoServer.Server.Events.Player
{
    public class PlayerConditionChangedEventHandler
    {
        private readonly IGameServer game;

        public PlayerConditionChangedEventHandler(IGameServer game)
        {
            this.game = game;
        }

        public void Execute(ICreature creature, ICondition c)
        {
            if (creature is not IPlayer player) return;
            ushort icons = 0;
            foreach (var condition in player.Conditions)
            {
                icons |= (ushort)ConditionIconParser.Parse(condition.Key);
            }

            if (game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection))
            {
                connection.OutgoingPackets.Enqueue(new ConditionIconPacket(icons));
                connection.Send();
            }
        }

    }
}