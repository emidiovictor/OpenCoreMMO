﻿using NeoServer.Game.Contracts.Creatures;
using NeoServer.Server.Contracts.Network;
using System.Collections.Generic;

namespace NeoServer.Server.Contracts
{
    public interface IGameCreatureManager
    {
        void AddKilledMonsters(IMonster monster);
        IPlayer AddPlayer(IPlayer player, IConnection connection);
        IEnumerable<IPlayer> GetAllLoggedPlayers();
        IEnumerable<ICreature> GetCreatures();
        bool GetPlayerConnection(uint playerId, out IConnection connection);
        bool IsPlayerLogged(IPlayer player);
        bool RemoveCreature(ICreature creature);
        bool RemovePlayer(IPlayer player);
        bool TryGetCreature(uint id, out ICreature creature);
        bool TryGetLoggedPlayer(uint playerId, out IPlayer player);
        bool TryGetPlayer(string name, out IPlayer player);
        bool TryGetPlayer(uint id, out IPlayer player);
    }
}
