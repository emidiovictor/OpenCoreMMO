﻿using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Contracts;
using NeoServer.Server.Contracts.Network;
using NeoServer.Server.Handlers;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player
{
    public class PlayerRevokeInvitePartyHandler : PacketHandler
    {
        private readonly IGameServer game;

        public PlayerRevokeInvitePartyHandler(IGameServer game)
        {
            this.game = game;
        }

        public override void HandlerMessage(IReadOnlyNetworkMessage message, IConnection connection)
        {
            var creatureId = message.GetUInt32();
            if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;
            if (!game.CreatureManager.TryGetPlayer(creatureId, out var invitedPlayer) || !game.CreatureManager.IsPlayerLogged(invitedPlayer))
            {
                connection.Send(new TextMessagePacket("Revoked player is not online.", TextMessageOutgoingType.Small));
                return;
            }

            game.Dispatcher.AddEvent(new Event(() => player.RevokePartyInvite(invitedPlayer)));
        }
    }
}
