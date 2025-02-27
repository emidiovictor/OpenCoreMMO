﻿using Moq;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Players;
using NeoServer.Game.Contracts.Creatures;
using NeoServer.Game.Contracts.Items;
using NeoServer.Game.Contracts.Items.Types;
using NeoServer.Game.Creatures.Events;
using NeoServer.Game.Creatures.Services;
using NeoServer.Game.DataStore;
using NeoServer.Game.Items.Tests;
using NeoServer.Game.Tests;
using NeoServer.Server.Model.Players;
using System;
using System.Collections.Generic;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Services
{
    public class DealTransationTest
    {
        [Fact]
        public void Buy_Player_Or_Seller_Or_ItemType_Is_Null_Or_Amount_Is_0_Returns_False()
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var playerMock = new Mock<IPlayer>();
            var shopperMock = new Mock<IShopperNpc>();
            var itemTypeMock = new Mock<IItemType>();

            var result = sut.Buy(null, shopperMock.Object, itemTypeMock.Object, 1);
            Assert.False(result);

            result = sut.Buy(playerMock.Object, null, itemTypeMock.Object, 1);
            Assert.False(result);

            result = sut.Buy(playerMock.Object, shopperMock.Object, null, 1);
            Assert.False(result);

            result = sut.Buy(playerMock.Object, shopperMock.Object, itemTypeMock.Object, 0);
            Assert.False(result);
        }

        [Fact]
        public void Buy_TotalMoney_Is_Less_Than_Cost_Returns_False()
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemTypeMock = new Mock<IItemType>();

            var playerMock = new Mock<IPlayer>();
            playerMock.Setup(x => x.TotalMoney).Returns(1000);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemTypeMock.Object, 10)).Returns(1100);

            var result = sut.Buy(playerMock.Object, shopperMock.Object, itemTypeMock.Object, 10);
            Assert.False(result);
        }

        [Fact]
        public void Buy_Remove_Coins_From_Backpack()
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new CoinTransaction(itemFactoryMock.Object);

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction);

            var itemToBuy = ItemTestData.CreateWeaponItem(10, "sword");

            var coin1 = ItemTestData.CreateCoin(1, 100, 1);
            var coin2 = ItemTestData.CreateCoin(1, 100, 1);
            var coin3 = ItemTestData.CreateCoin(1, 100, 1);

            CoinTypeStore.Data.Add(1, coin1.Metadata);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 3)).Returns(300);

            var container = ItemTestData.CreateBackpack();
            container.AddItem(coin1);
            container.AddItem(coin2);
            container.AddItem(coin3);

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) } });

            var inventory = new PlayerInventory(player, new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) } });

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 3);

            Assert.Empty(container.Items);
        }
        [Fact]
        public void Buy_Remove_Coins_From_Backpack_And_Bank()
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new CoinTransaction(itemFactoryMock.Object);

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction);

            var itemToBuy = ItemTestData.CreateWeaponItem(10, "sword");

            var container = ItemTestData.CreateBackpack();
            container.AddItem(ItemTestData.CreateCoin(1, 100, 1));
            container.AddItem(ItemTestData.CreateCoin(1, 100, 1));
            container.AddItem(ItemTestData.CreateCoin(1, 100, 1));

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) } });

            player.LoadBank(500);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 3)).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 3);

            Assert.Empty(container.Items);
            Assert.Equal(400ul, player.BankAmount);
        }

        [InlineData("head")]
        [InlineData("body")]
        [InlineData("weapon")]
        [InlineData("shield")]
        [InlineData("necklace")]
        [InlineData("ring")]
        [InlineData("feet")]
        [InlineData("ammo")]
        [Theory]
        public void Buy_Equipment_When_Slot_Is_Empty_Adds_There(string slot)
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemToBuy = ItemTestData.CreateBodyEquipmentItem(10, slot);

            itemFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<Location>(), null)).Returns(itemToBuy);

            var container = ItemTestData.CreateBackpack();
            container.AddItem(ItemTestData.CreateCoin(1, 100, 1));
            container.AddItem(ItemTestData.CreateCoin(1, 100, 1));
            container.AddItem(ItemTestData.CreateCoin(1, 100, 1));

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) } });

            player.LoadBank(500);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 1)).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 1);

            Assert.Equal(itemToBuy, player.Inventory[itemToBuy.Metadata.BodyPosition]);
        }
        [Fact]
        public void Buy_Backpack_When_Slot_Is_Empty_Adds_There()
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemToBuy = ItemTestData.CreateBackpack();

            itemFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<Location>(), null)).Returns(itemToBuy);

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>());

            player.LoadBank(500);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 1)).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 1);

            Assert.Equal(itemToBuy, player.Inventory[itemToBuy.Metadata.BodyPosition]);
        }

        [InlineData(30, 50, 80)]
        [InlineData(30, 100, 100)]
        [InlineData(30, 10, 40)]
        [InlineData(1, 99, 100)]
        [InlineData(1, 1, 2)]
        [InlineData(100, 1, 100)]
        [Theory]
        public void Buy_Ammo_When_Slot_Is_Empty_Adds_There(byte current, byte bought, byte expected)
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemToBuy = ItemTestData.CreateAmmoItem(1, bought);

            itemFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<Location>(), null)).Returns(itemToBuy);

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() {
               { Slot.Ammo, new Tuple<IPickupable, ushort>(ItemTestData.CreateAmmoItem(1, current), 1) },
               { Slot.Backpack, new Tuple<IPickupable, ushort>(ItemTestData.CreateBackpack(), 2) }  });

            player.LoadBank(500);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 1)).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, bought);

            Assert.Equal(itemToBuy.ClientId, player.Inventory[itemToBuy.Metadata.BodyPosition].ClientId);
            Assert.Equal(expected, player.Inventory[itemToBuy.Metadata.BodyPosition].Amount);
        }

        [InlineData("head")]
        [InlineData("body")]
        [InlineData("weapon")]
        [InlineData("shield")]
        [InlineData("necklace")]
        [InlineData("ring")]
        [InlineData("feet")]
        [InlineData("ammo")]
        [Theory]
        public void Buy_Equipment_When_Slot_Is_Full_Adds_To_Backpack(string slot)
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemToBuy = ItemTestData.CreateBodyEquipmentItem(10, slot);

            itemFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<Location>(), null)).Returns(itemToBuy);

            var container = ItemTestData.CreateBackpack();
         
            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) },
             { itemToBuy.Metadata.BodyPosition, new Tuple<IPickupable, ushort>(ItemTestData.CreateBodyEquipmentItem(10, slot),10) }});

            player.LoadBank(5000);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 1)).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 1);

            Assert.Equal(itemToBuy, player.Inventory.BackpackSlot.Items[0]);
        }
        [InlineData("head")]
        [InlineData("body")]
        [InlineData("weapon")]
        [InlineData("shield")]
        [InlineData("necklace")]
        [InlineData("ring")]
        [InlineData("feet")]
        [InlineData("ammo")]
        [Theory]
        public void Buy_5_Equipment_When_Slot_Is_Full_Adds_To_Backpack(string slot)
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemToBuy = ItemTestData.CreateBodyEquipmentItem(10, slot);

            itemFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<Location>(), null)).Returns(itemToBuy);

            var container = ItemTestData.CreateBackpack();

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) }});

            player.LoadBank(5000);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(It.IsAny<IItemType>(), It.IsAny<byte>())).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 5);

            Assert.Equal(itemToBuy, player.Inventory[itemToBuy.Metadata.BodyPosition]);

            Assert.Equal(4, player.Inventory.BackpackSlot.Items.Count);

            Assert.Equal(itemToBuy, player.Inventory.BackpackSlot.Items[0]);
            Assert.Equal(itemToBuy, player.Inventory.BackpackSlot.Items[1]);
            Assert.Equal(itemToBuy, player.Inventory.BackpackSlot.Items[2]);
            Assert.Equal(itemToBuy, player.Inventory.BackpackSlot.Items[3]);
        }

        [InlineData(30, 100,30)]
        [InlineData(100, 100, 100)]
        [InlineData(100, 1, 1)]
        [InlineData(99, 2, 1)]
        [InlineData(100, 99, 99)]
        [Theory]
        public void Buy_Ammo_When_Slot_Is_Partial_Adds_To_Backpack(byte currentOnInventory, byte bought, byte expectedOnBackpack)
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new Mock<ICoinTransaction>();

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction.Object);

            var itemToBuy = ItemTestData.CreateAmmoItem(10, bought);

            itemFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<Location>(), null)).Returns(itemToBuy);

            var container = ItemTestData.CreateBackpack();

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 2) },
             { itemToBuy.Metadata.BodyPosition, new Tuple<IPickupable, ushort>(ItemTestData.CreateAmmoItem(10, currentOnInventory),10) }});

            player.LoadBank(5000);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 1)).Returns(400);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, bought);

            Assert.Equal(itemToBuy.ClientId, player.Inventory.BackpackSlot.Items[0].ClientId);
            Assert.Equal(expectedOnBackpack, player.Inventory.BackpackSlot.Items[0].Amount);
        }

        [Fact]
        public void Buy_Item_When_Backpack_Is_Full_Keep_Change_On_Bank()
        {
            var itemFactoryMock = new Mock<IItemFactory>();
            var coinTransaction = new CoinTransaction(itemFactoryMock.Object);

            var sut = new DealTransaction(itemFactoryMock.Object, coinTransaction);

            var itemToBuy = ItemTestData.CreateWeaponItem(10, "sword");

            itemFactoryMock.Setup(x => x.Create(10, It.IsAny<Location>(), null)).Returns(itemToBuy);

            var platinum = ItemTestData.CreateCoin(1, 2, 100);
            var gold = ItemTestData.CreateCoin(2, 1, 1);

            itemFactoryMock.Setup(x => x.CreateCoins(It.IsAny<ulong>())).Returns(new List<ICoin> { (ICoin)ItemTestData.CreateCoin(1, 1, 100), (ICoin)ItemTestData.CreateCoin(2, 70, 1) });

            itemFactoryMock.Setup(x => x.Create(1, It.IsAny<Location>(), null)).Returns(platinum);
            itemFactoryMock.Setup(x => x.Create(2, It.IsAny<Location>(), null)).Returns(gold);

            CoinTypeStore.Data.Add(1, platinum.Metadata);
            CoinTypeStore.Data.Add(2, gold.Metadata);

            var container = ItemTestData.CreatePickupableContainer(2);
            container.AddItem(ItemTestData.CreateWeaponItem(11, "axe"));
            container.AddItem(platinum);

            var player = PlayerTestDataBuilder.BuildPlayer(1000, inventory: new Dictionary<Slot, Tuple<IPickupable, ushort>>() { 
                { Slot.Backpack, new Tuple<IPickupable, ushort>(container, 1) }});

            player.LoadBank(5000);

            var shopperMock = new Mock<IShopperNpc>();
            shopperMock.Setup(x => x.CalculateCost(itemToBuy.Metadata, 1)).Returns(30);

            var result = sut.Buy(player, shopperMock.Object, itemToBuy.Metadata, 1);

            Assert.Single(player.Inventory.BackpackSlot.Items);
            Assert.Equal(5170ul, player.BankAmount);
        }

    }
}
