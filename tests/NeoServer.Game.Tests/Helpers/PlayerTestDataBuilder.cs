﻿using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Players;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items.Types;
using NeoServer.Game.Creature.Model;
using NeoServer.Server.Model.Players;
using System;
using System.Collections.Generic;

namespace NeoServer.Game.Tests
{
    public  class PlayerTestDataBuilder
    {
        public static IPlayer BuildPlayer(uint capacity = 100, ushort hp = 100, ushort mana = 30, ushort speed = 200, Dictionary<Slot, Tuple<IPickupable, ushort>> inventory = null, Dictionary<SkillType, ISkill> skills = null)
        {
            inventory = inventory ?? new Dictionary<Slot, Tuple<IPickupable, ushort>>();
            var sut = new Player(1,"PlayerA", ChaseMode.Stand, capacity: capacity, healthPoints: hp, maxHealthPoints: 100, vocation: 1, Gender.Male, online: true, mana: mana, maxMana: 30, fightMode: FightMode.Attack,
              soulPoints: 100, soulMax: 100, skills: skills ?? new Dictionary<SkillType, ISkill>() { { SkillType.Level, new Skill(SkillType.Level,1,10,1)} } , staminaMinutes: 300, outfit: new Outfit(), inventory: inventory, speed, new Location(100, 100, 7));
            return sut;
        }
    }
}
