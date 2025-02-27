﻿using NeoServer.Game.Common;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items.Types;
using System;
using System.Collections.Generic;

namespace NeoServer.Game.Contracts.Items
{
    public delegate void CreateItem(IItem item);
    public interface IItemFactory: IFactory
    {
        IItem Create(ushort typeId, Location location, IDictionary<ItemAttribute, IConvertible> attributes);
        IItem Create(string name, Location location, IDictionary<ItemAttribute, IConvertible> attributes);
        IEnumerable<ICoin> CreateCoins(ulong amount);
        IItem CreateLootCorpse(ushort typeId, Location location, ILoot loot);
    }
}
