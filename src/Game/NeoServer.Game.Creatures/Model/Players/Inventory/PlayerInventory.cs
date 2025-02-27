﻿using NeoServer.Game.Common;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Players;
using NeoServer.Game.Contracts;
using NeoServer.Game.Contracts.Bases;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items;
using NeoServer.Game.Contracts.Items.Types;
using NeoServer.Game.Contracts.Items.Types.Body;
using NeoServer.Game.Contracts.Items.Types.Containers;
using NeoServer.Game.DataStore;
using System;
using System.Collections.Generic;

namespace NeoServer.Server.Model.Players
{
    public class PlayerInventory : Store, IInventory
    {

        public event AddItemToSlot OnItemAddedToSlot;
        public event RemoveItemFromSlot OnItemRemovedFromSlot;

        public event FailAddItemToSlot OnFailedToAddToSlot;
        private IDictionary<Slot, Tuple<IPickupable, ushort>> Inventory { get; }

        public ushort TotalAttack
        {
            get
            {
                ushort attack = 0;

                if (Weapon is IWeaponItem weapon) return weapon.Attack;

                if (Weapon is IDistanceWeaponItem distance)
                {
                    attack += distance.ExtraAttack;
                    if (Ammo != null)
                    {
                        attack += distance.ExtraAttack;
                    }

                }

                return attack;
            }
        }

        public IAmmoItem Ammo => Inventory.ContainsKey(Slot.Ammo) && Inventory[Slot.Ammo].Item1 is IAmmoItem ammo ? Inventory[Slot.Ammo].Item1 as IAmmoItem : null;

        public ushort TotalDefense
        {
            get
            {
                var totalDefense = 0;
                if (Weapon is IWeaponItem weapon)
                {
                    totalDefense += weapon.Defense;
                }

                totalDefense += Shield?.DefenseValue ?? 0;

                return (ushort)totalDefense;
            }
        }

        public IDefenseEquipmentItem Shield => Inventory.ContainsKey(Slot.Right) ? Inventory[Slot.Right].Item1 as IDefenseEquipmentItem : null;

        public IWeapon Weapon => Inventory.ContainsKey(Slot.Left) ? Inventory[Slot.Left].Item1 as IWeapon : null;

        public IDictionary<ushort, uint> Map
        {
            get
            {
                var map = BackpackSlot?.Map ?? new Dictionary<ushort, uint>();

                Action<IItem> addOrUpdate = (item) =>
                {
                    if (item is null) return;
                    if (map.TryGetValue(item.Metadata.TypeId, out var val)) map[item.Metadata.TypeId] = val + item.Amount;
                    else map.Add(item.Metadata.TypeId, item.Amount);
                };

                addOrUpdate(this[Slot.Head]);
                addOrUpdate(this[Slot.Necklace]);
                addOrUpdate(this[Slot.Body]);
                addOrUpdate(this[Slot.Right]);
                addOrUpdate(this[Slot.Left]);
                addOrUpdate(this[Slot.Legs]);
                addOrUpdate(this[Slot.Ring]);
                addOrUpdate(this[Slot.Ammo]);

                return map;
            }
        }
        public ulong GetTotalMoney(IDictionary<ushort, uint> inventoryMap)
        {
            uint total = 0;

            foreach (var coinType in CoinTypeStore.Data.All)
            {
                if (coinType is null) continue;
                if (!inventoryMap.TryGetValue(coinType.TypeId, out var coinAmount)) continue;

                var worthMultiplier = coinType?.Attributes?.GetAttribute<uint>(ItemAttribute.Worth) ?? 0;
                total += worthMultiplier * coinAmount;
            }

            return total;
        }

        public ulong TotalMoney
        {
            get
            {
                if (BackpackSlot?.Map is null) return 0;
                return GetTotalMoney(BackpackSlot.Map);
            }
        }

        public bool HasShield => Inventory.ContainsKey(Slot.Right);
        public byte TotalArmor
        {
            get
            {
                byte totalArmor = 0;

                Func<Slot, ushort> getDefenseValue = (Slot slot) => (Inventory[slot].Item1 is  IDefenseEquipmentItem equipment) ? equipment.DefenseValue : default;

                totalArmor += (byte)(Inventory.ContainsKey(Slot.Necklace) ? getDefenseValue(Slot.Necklace) : 0);
                totalArmor += (byte)(Inventory.ContainsKey(Slot.Head) ? getDefenseValue(Slot.Head) : 0);
                totalArmor += (byte)(Inventory.ContainsKey(Slot.Body) ? getDefenseValue(Slot.Body) : 0);
                totalArmor += (byte)(Inventory.ContainsKey(Slot.Legs) ? getDefenseValue(Slot.Legs) : 0);
                totalArmor += (byte)(Inventory.ContainsKey(Slot.Feet) ? getDefenseValue(Slot.Feet) : 0);
                totalArmor += (byte)(Inventory.ContainsKey(Slot.Ring) ? getDefenseValue(Slot.Ring) : 0);

                return totalArmor;
            }
        }

        public byte AttackRange
        {
            get
            {
                var rangeLeft = 0;
                var rangeRight = 0;
                var twoHanded = 0;

                if (Inventory.ContainsKey(Slot.Left) && Inventory[Slot.Left] is IAmmoItem leftWeapon)
                {
                    rangeLeft = leftWeapon.Range;
                }
                if (Inventory.ContainsKey(Slot.Right) && Inventory[Slot.Right] is IAmmoItem rightWeapon)
                {
                    rangeRight = rightWeapon.Range;
                }
                if (Inventory.ContainsKey(Slot.TwoHanded) && Inventory[Slot.TwoHanded] is IAmmoItem twoHandedWeapon)
                {
                    rangeRight = twoHandedWeapon.Range;
                }

                return (byte)Math.Max(Math.Max(rangeLeft, rangeRight), twoHanded);
            }
        }

        public IPlayer Owner { get; }

        public PlayerInventory(IPlayer owner, IDictionary<Slot, Tuple<IPickupable, ushort>> inventory)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Owner = owner;
            Inventory = new Dictionary<Slot, Tuple<IPickupable, ushort>>();

            foreach (var item in inventory)
            {
                TryAddItemToSlot(item.Key, item.Value.Item1);
            }
        }

        public IItem this[Slot slot] => !Inventory.ContainsKey(slot) ? null : Inventory[slot].Item1;

        public IContainer BackpackSlot => this[Slot.Backpack] is IContainer container ? container : null;

        public float TotalWeight
        {
            get
            {
                var sum = 0F;
                foreach (var item in Inventory.Values)
                {
                    sum += item.Item1.Weight;
                }
                return sum;
            }
        }

        public bool RemoveItemFromSlot(Slot slot, byte amount, out IPickupable removedItem)
        {
            removedItem = null;

            if (amount == 0) return false;
            if (Inventory[slot].Item1 is not IPickupable item) return false;
            if (item is null) return false;

            if (item is ICumulative cumulative && amount < cumulative.Amount)
            {
                removedItem = cumulative.Split(amount);
            }
            else
            {
                if (item is ICumulative c) c.ClearSubscribers();

                Inventory.Remove(slot);
                removedItem = item;
            }
            OnItemRemovedFromSlot?.Invoke(this, removedItem, slot, amount);
            return true;
        }

        public Result<IPickupable> TryAddItemToSlot(Slot slot, IPickupable item)
        {
            bool canCarry = CanCarryItem(item, slot);

            if (!canCarry)
            {
                OnFailedToAddToSlot?.Invoke(InvalidOperation.TooHeavy);
                return new Result<IPickupable>(InvalidOperation.TooHeavy);
            }

            var canAddItemToSlot = CanAddItemToSlot(slot, item);
            if (!canAddItemToSlot.Value)
            {
                OnFailedToAddToSlot?.Invoke(canAddItemToSlot.Error);
                return new Result<IPickupable>(canAddItemToSlot.Error);
            }

            if (slot == Slot.Backpack)
            {
                if (Inventory.ContainsKey(Slot.Backpack))
                {
                    return new Result<IPickupable>(null, (Inventory[slot].Item1 as IPickupableContainer).AddItem(item).Error);
                }
                else if (item is IPickupableContainer container)
                {
                    container.SetParent(Owner);
                }
            }
            var slotHasItem = Inventory.ContainsKey(slot);

            //todo: refact
            if (slotHasItem)
            {
                Tuple<IPickupable, ushort> itemToSwap = null;

                if (item is ICumulative cumulative)
                {
                    if (NeedToSwap(cumulative, slot))
                    {
                        itemToSwap = SwapItem(slot, cumulative);
                    }
                    else
                    {
                        (Inventory[slot].Item1 as ICumulative).TryJoin(ref cumulative);
                        if (cumulative?.Amount > 0)
                        {
                            itemToSwap = new Tuple<IPickupable, ushort>(cumulative, cumulative.ClientId);
                        }
                    }

                    if (itemToSwap?.Item1 is ICumulative c) c.ClearSubscribers();
                }
                else
                {
                    itemToSwap = SwapItem(slot, item);
                }

                OnItemAddedToSlot?.Invoke(this, item, slot);
                return itemToSwap == null ? new Result<IPickupable>() : new Result<IPickupable>(itemToSwap.Item1);
            }
            else
            {
                if (item is ICumulative cumulative) cumulative.OnReduced += (item, amount) => OnItemReduced(item, slot, amount);
            }

            Inventory.Add(slot, new Tuple<IPickupable, ushort>(item, item.ClientId));

            item.SetNewLocation(Location.Inventory(slot));

            OnItemAddedToSlot?.Invoke(this, item, slot);
            return new Result<IPickupable>();
        }

        private void OnItemReduced(ICumulative item, Slot slot, byte amount)
        {
            if (item.Amount == 0)
            {
                RemoveItemFromSlot(slot, amount, out var removedItem);
                return;
            }
            OnItemRemovedFromSlot?.Invoke(this, item, slot, amount);
        }

        private Tuple<IPickupable, ushort> SwapItem(Slot slot, IPickupable item)
        {
            var itemToSwap = Inventory[slot];
            Inventory[slot] = new Tuple<IPickupable, ushort>(item, item.ClientId);

            if (item is ICumulative cumulative) cumulative.OnReduced += (item, amount) => OnItemReduced(item, slot, amount);

            return itemToSwap;
        }

        private bool NeedToSwap(IPickupable itemToAdd, Slot slotDestination)
        {
            if (!Inventory.ContainsKey(slotDestination))
            {
                return false;
            }

            var itemOnSlot = Inventory[slotDestination].Item1;

            if (itemToAdd is ICumulative cumulative && itemOnSlot.ClientId == cumulative.ClientId)
            {
                //will join
                return false;
            }

            if (slotDestination == Slot.Backpack)
            {
                // will add item to container
                return false;
            }

            return true;
        }

        private bool CanCarryItem(IPickupable item, Slot slot, byte amount = 1)
        {
            var itemWeight = item is ICumulative c ? c.CalculateWeight(amount) : item.Weight;

            if (NeedToSwap(item, slot))
            {
                var itemOnSlot = Inventory[slot].Item1;

                return (TotalWeight - itemOnSlot.Weight + itemWeight) <= Owner.TotalCapacity;
            }

            float weight = item.Weight;

            if (item is ICumulative cumulative && slot == Slot.Ammo)
            {
                byte amountToAdd = cumulative.Amount > cumulative.AmountToComplete ? cumulative.AmountToComplete : cumulative.Amount;
                weight = cumulative.CalculateWeight(amountToAdd);
            }

            var canCarry = (TotalWeight + weight) <= Owner.TotalCapacity;
            return canCarry;
        }

        public Result<bool> CanAddItemToSlot(Slot slot, IItem item)
        {
            var cannotDressFail = new Result<bool>(false, InvalidOperation.CannotDress);

            if (slot == Slot.Backpack)
            {
                if (item is IPickupableContainer && !Inventory.ContainsKey(Slot.Backpack))
                {
                    return new Result<bool>(true);
                }
                return Inventory.ContainsKey(Slot.Backpack) ? new Result<bool>(true) : cannotDressFail;
            }

            if (item is not IInventoryItem inventoryItem)
            {
                return cannotDressFail;
            }

            if (inventoryItem is IWeapon weapon)
            {
                if (slot != Slot.Left)
                {
                    return cannotDressFail;
                }

                var hasShieldDressed = this[Slot.Right] != null;

                if (weapon.TwoHanded && hasShieldDressed)
                {
                    //trying to add a two handed while right slot has a shield
                    return new Result<bool>(false, InvalidOperation.BothHandsNeedToBeFree);
                }

                return new Result<bool>(true);
            }

            if (slot == Slot.Right && this[Slot.Left] is IWeapon weaponOnLeft && weaponOnLeft.TwoHanded)
            {
                //trying to add a shield while left slot has a two handed weapon
                return new Result<bool>(false, InvalidOperation.BothHandsNeedToBeFree);
            }

            if (inventoryItem.Slot != slot)
            {
                return cannotDressFail;
            }

            return new Result<bool>(true);
        }

        #region Store Methods
        public override Result CanAddItem(IItem thing, byte amount = 1, byte? slot = null)
        {
            if (thing is not IPickupable item) return Result.NotPossible;
            if (!CanCarryItem(item, (Slot)slot, amount)) return new Result(InvalidOperation.TooHeavy);

            return CanAddItemToSlot((Slot)slot, item).ResultValue;
        }
        public override Result<uint> CanAddItem(IItemType itemType)
        {
            if (itemType is null) return Result<uint>.NotPossible;
            if (itemType.BodyPosition == Slot.None) return new Result<uint>(InvalidOperation.NotEnoughRoom);

            var itemOnSlot = this[itemType.BodyPosition];
            if (itemOnSlot is not null && itemType.TypeId != itemOnSlot.Metadata.TypeId) return new Result<uint>(InvalidOperation.NotEnoughRoom);

            byte possibleAmountToAdd;
            if (ICumulative.IsApplicable(itemType))
            {
                var amountOnSlot = this[itemType.BodyPosition]?.Amount ?? 0;
                possibleAmountToAdd = (byte)Math.Abs(100 - amountOnSlot);
            }
            else
            {
                if (itemOnSlot is not null) return new Result<uint>(InvalidOperation.NotEnoughRoom);
                possibleAmountToAdd = 1;
            }

            if (possibleAmountToAdd == 0) return new Result<uint>(InvalidOperation.NotEnoughRoom);

            return new Result<uint>(possibleAmountToAdd);
        }

        public override uint PossibleAmountToAdd(IItem item, byte? toPosition = null)
        {
            if (toPosition is null) return 0;

            var slot = (Slot)toPosition;

            if (slot == Slot.Backpack)
            {
                if (this[slot] is null) return 1;
                if (this[slot] is IContainer container) return container.PossibleAmountToAdd(item);
            }

            if (slot != Slot.Left && slot != Slot.Ammo) return 1;

            if (item is not ICumulative) return 1;
            if (item is ICumulative c1 && this[slot] is IItem i && c1.ClientId != i.ClientId) return 100;
            if (item is ICumulative && this[slot] is null) return 100;
            if (item is ICumulative cumulative) return (uint)(100 - this[slot].Amount);

            return 0;
        }

        public override bool CanRemoveItem(IItem item) => true;

        public override Result<OperationResult<IItem>> AddItem(IItem thing, byte? position = null)
        {
            if (thing is not IPickupable item) return Result<OperationResult<IItem>>.NotPossible;

            position = position ?? (byte)thing.Metadata.BodyPosition;

            if (position is null) return Result<OperationResult<IItem>>.NotPossible; 

            var swappedItem = TryAddItemToSlot((Slot)position, item);

            if (swappedItem.Value is null) return Result<OperationResult<IItem>>.Success;

            return new(new OperationResult<IItem>(Operation.Removed, swappedItem.Value));
        }

        public override Result<OperationResult<IItem>> RemoveItem(IItem thing, byte amount, byte fromPosition, out IItem removedThing)
        {
            removedThing = null;
            if (!RemoveItemFromSlot((Slot)fromPosition, amount, out var removedItem)) return Result<OperationResult<IItem>>.NotPossible;

            removedThing = removedItem;
            return new();
        }
        public override Result<OperationResult<IItem>> ReceiveFrom(IStore source, IItem thing, byte? toPosition)
        {
            var result = base.ReceiveFrom(source, thing, toPosition);

            if (!result.Value.HasAnyOperation) return result;

            foreach (var operation in result.Value.Operations)
            {
                if (operation.Item2 == Operation.Removed)
                {
                    source.ReceiveFrom(this, operation.Item1, null);
                }
            }

            return result;
        }
        #endregion
    }
}
