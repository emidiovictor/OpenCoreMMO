﻿using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Talks;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items;
using NeoServer.Game.Contracts.World;
using NeoServer.Game.Contracts.World.Tiles;
using NeoServer.Game.Creature.Model;
using NeoServer.Game.Creatures.Enums;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NeoServer.Game.Creatures.Model
{

    public abstract class Creature : IEquatable<Creature>, ICreature
    {
        public event RemoveCreature OnCreatureRemoved;

        public event ChangeOutfit OnChangedOutfit;

        public event Say OnSay;

        protected readonly ICreatureType CreatureType;

        public Creature(ICreatureType type, IOutfit outfit = null, uint healthPoints = 0)
        {
            if (string.IsNullOrWhiteSpace(type.Name))
            {
                throw new ArgumentNullException(nameof(type.Name));
            }
            MaxHealthPoints = type.MaxHealth;
            HealthPoints = Math.Min(MaxHealthPoints, healthPoints == 0 ? MaxHealthPoints : healthPoints);

            CreatureType = type;

            CreatureId = RandomCreatureIdGenerator.Generate(this);
            Outfit = outfit ?? new Outfit()
            {
                LookType = type.Look[LookType.Type],
                Body = (byte)type.Look[LookType.Body],
                Feet = (byte)type.Look[LookType.Feet],
                Head = (byte)type.Look[LookType.Head],
                Legs = (byte)type.Look[LookType.Legs],
            };
            MaxHealthPoints = type.MaxHealth;

        }

        private IDynamicTile tile;
        public IDynamicTile Tile
        {
            get
            {
                return tile;
            }
            protected set
            {
                tile = value;
                Location = tile.Location;
            }
        }
        public Action<ICreature> NextAction { get; protected set; }
        public uint HealthPoints { get; protected set; }
        public uint MaxHealthPoints { get; protected set; }
        public new string Name => CreatureType.Name;
        public uint CreatureId { get; }
        public ushort CorpseType => CreatureType.Look[LookType.Corpse];
        public IThing Corpse { get; set; }
        public virtual BloodType BloodType => BloodType.Blood;
        public abstract bool CanBeSeen { get; }
        public abstract IOutfit Outfit { get; protected set; }
        public IOutfit LastOutfit { get; private set; }
        public Direction Direction { get; protected set; }

        public Direction SafeDirection
        {
            get
            {
                switch (Direction)
                {
                    case Direction.North:
                    case Direction.East:
                    case Direction.South:
                    case Direction.West:
                        return Direction;
                    case Direction.NorthEast:
                    case Direction.SouthEast:
                        return Direction.East;
                    case Direction.NorthWest:
                    case Direction.SouthWest:
                        return Direction.West;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void ChangeOutfit(ushort lookType, ushort id, byte head, byte body, byte legs, byte feet, byte addon)
        {
            LastOutfit = null;
            Outfit.Change(lookType, id, head, body, legs, feet, addon);

            OnChangedOutfit?.Invoke(this, Outfit);
        }
        public void SetTemporaryOutfit(ushort lookType, ushort id, byte head, byte body, byte legs, byte feet, byte addon)
        {
            LastOutfit = Outfit.Clone();
            Outfit.Change(lookType, id, head, body, legs, feet, addon);
            OnChangedOutfit?.Invoke(this, Outfit);
        }

        public void BackToOldOutfit()
        {
            Outfit = LastOutfit;
            LastOutfit = null;
            OnChangedOutfit?.Invoke(this, Outfit);
        }
        public byte LightBrightness { get; protected set; }
        public byte LightColor { get; protected set; }
        public  bool IsInvisible { get; protected set; } // TODO: implement.
        public abstract bool CanSeeInvisible { get; }

        public virtual bool CanSee(ICreature otherCreature)
        {
            return !otherCreature.IsInvisible || CanSeeInvisible;
        }
        
        public bool CanSee(Location pos, int viewPortX, int viewPortY)
        {
            if (Location.IsSurface || Location.IsAboveSurface)
            {
                if (pos.IsUnderground) return false;
            }
            else if (Location.IsUnderground)
            {
                if (Math.Abs(Location.Z - pos.Z) > 2) return false;
            }

            var offsetZ = Location.Z - pos.Z;

            if (pos.X >= Location.X - viewPortX + offsetZ && pos.X <= Location.X + viewPortX + offsetZ &&
                pos.Y >= Location.Y - viewPortY + offsetZ && pos.Y <= Location.Y + viewPortY + offsetZ)
            {
                return true;
            }

            return false;
        }

        public virtual void OnCreatureAppear(Location location, ICylinderSpectator[] spectators)
        {
            foreach (var cylinder in spectators)
            {
                var spectator = cylinder.Spectator;
                if (this == (Creature)spectator) continue;

                if (spectator is ICombatActor actor) actor.SetAsEnemy(this);
                if (this is ICombatActor a) a.SetAsEnemy(spectator);
            }
        }

        protected void ExecuteNextAction(ICreature creature)
        {
            NextAction?.Invoke(creature);
            NextAction = null;
        }
        public bool CanSee(Location pos)
        {
            var viewPortX = 9;
            var viewPortY = 7;

            if (Location.IsSurface || Location.IsAboveSurface)
            {
                if (pos.IsUnderground) return false;
            }
            else if (Location.IsUnderground)
            {
                if (Math.Abs(Location.Z - pos.Z) > 2) return false;
            }

            var offsetZ = Location.Z - pos.Z;

            if (pos.X >= Location.X - (viewPortX - 1) + offsetZ && pos.X <= Location.X + viewPortX + offsetZ &&
                pos.Y >= Location.Y - (viewPortY - 1) + offsetZ && pos.Y <= Location.Y + viewPortY + offsetZ)
            {
                return true;
            }

            return false;
        }

        public byte Skull { get; protected set; } // TODO: implement.

        public virtual byte Emblem { get; } // TODO: implement.
        public bool IsHealthHidden { get; protected set; }
        public Location Location { get; set; }

        protected void SetDirection(Direction direction) => Direction = direction;

        public virtual void Say(string message, SpeechType talkType, ICreature receiver = null)
        {
            if (string.IsNullOrWhiteSpace(message) || talkType == SpeechType.None) return;
            OnSay?.Invoke(this, talkType, message, receiver);
        }
    
        public override bool Equals(object obj) => obj is ICreature creature && creature.CreatureId == CreatureId;

        public override int GetHashCode() => HashCode.Combine(CreatureId);

        public bool Equals([AllowNull] Creature other)
        {
            return this == other;
        }

        public void OnMoved() { }

        public static bool operator ==(Creature creature1, Creature creature2) => creature1.CreatureId == creature2.CreatureId;
        public static bool operator !=(Creature creature1, Creature creature2) => creature1.CreatureId != creature2.CreatureId;
    }
}
