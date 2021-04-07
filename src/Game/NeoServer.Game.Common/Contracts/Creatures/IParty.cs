﻿using NeoServer.Server.Model.Players.Contracts;
using System;
using System.Collections.Generic;

namespace NeoServer.Game.Common.Contracts.Creatures
{
    public interface IParty
    {
        IReadOnlyCollection<uint> Members { get; }
        bool IsEmpty { get; }
        IPlayer Leader { get; }
        IReadOnlyCollection<uint> Invites { get; }

        event Action OnPartyOver;

        Result ChangeLeadership(IPlayer from, IPlayer to);
        Result Invite(IPlayer by, IPlayer invitedPlayer);
        bool IsInvited(IPlayer player);
        bool IsLeader(IPlayer player);
        bool IsLeader(uint creatureId);
        bool IsMember(uint creatureId);
        bool IsMember(IPlayer player);
        bool JoinPlayer(IPlayer player);
        void RemoveMember(IPlayer player);
        void RevokeInvite(IPlayer by, IPlayer invitedPlayer);
    }
}
