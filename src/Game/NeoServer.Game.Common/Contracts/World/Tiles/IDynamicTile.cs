﻿using NeoServer.Game.Common.Location;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items;
using System.Collections.Generic;

namespace NeoServer.Game.Contracts.World.Tiles
{

    public interface IDynamicTile : ITile
    {
        IGround Ground { get; }
        Stack<IItem> TopItems { get; }
        Stack<IItem> DownItems { get; }
        Dictionary<uint, IWalkableCreature> Creatures { get; }
        ushort StepSpeed { get; }
        bool CannotLogout { get; }
        bool ProtectionZone { get; }
        FloorChangeDirection FloorDirection { get; }
        byte MovementPenalty { get; }
        bool HasCreature { get; }
        IMagicField MagicField { get; }

        byte[] GetRaw(IPlayer playerRequesting = null);
        ICreature GetTopVisibleCreature(ICreature creature);
        bool TryGetStackPositionOfItem(IItem item, out byte stackPosition);
        byte GetCreatureStackPositionIndex(IPlayer observer);

        bool HasBlockPathFinding { get; }
    }
}
