﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SadnaExpress.DomainLayer.Store;
using SadnaExpress.ServiceLayer;

namespace SadnaExpressTests.Integration_Tests
{
    /// <summary>
    /// This class contains integration tests for the purchase process in the trading system.
    /// It tests the functionality of purchasing items as a guest user and handles various scenarios, such as successful purchases, failed payments, failed supplier service, and insufficient item quantities.
    /// </summary>
    [TestClass]
    public class PurchaseIT : TradingSystemIT
    {
        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        /// <summary>
        /// Tests the purchase of items by a guest user when payment is successful.
        /// </summary>
        [TestMethod]
        public void PurchaseItemsGuestSuccess()
        {
            //Arrange
            trading.SetPaymentService(new Mocks.Mock_PaymentService());
            trading.SetSupplierService(new Mocks.Mock_SupplierService());
            // Act
            trading.PurchaseCart(buyerID, "0502485415400", "Rabbi Akiva 5 Beer Sheva");
            // Assert
            // check the order not created
            Assert.AreEqual(1, trading.GetStorePurchases(userID, storeID1).Value.Count);
            Assert.AreEqual(1, trading.GetStorePurchases(userID, storeID2).Value.Count);
            // see the prices
            Assert.AreEqual(4000, trading.GetStorePurchases(userID, storeID1).Value[0].CalculatorAmount());
            Assert.AreEqual(3000, trading.GetStorePurchases(userID, storeID2).Value[0].CalculatorAmount());
            Assert.AreEqual(7000, Orders.Instance.GetOrdersByUserId(buyerID)[0].CalculatorAmount());
            // check the inventory updated
            Assert.AreEqual(1, trading.GetStore(storeID1).Value.GetItemByQuantity(itemID1));
            Assert.AreEqual(0, trading.GetStore(storeID2).Value.GetItemByQuantity(itemID2));
            //check that the shopping empty
            Assert.AreEqual(0, trading.GetDetailsOnCart(buyerID).Value.Baskets.Count);
        }

        /// <summary>
        /// Tests the purchase of items by a guest user when payment fails.
        /// </summary>
        [TestMethod]
        public void PurchaseItemsGuestPaymentFail()
        {
            //Arrange
            trading.SetPaymentService(new Mocks.Mock_Bad_PaymentService());
            trading.SetSupplierService(new Mocks.Mock_SupplierService());
            // Act
            Assert.IsTrue(trading.PurchaseCart(buyerID, "0502485415400", "Rabbi Akiva 5 Beer Sheva").ErrorOccured);
            // Assert
            // check the order not created
            Assert.IsNull(trading.GetStorePurchases(userID, storeID1).Value);
            Assert.IsNull(trading.GetStorePurchases(userID, storeID2).Value);
            // check the inventory stay
            Assert.AreEqual(3, trading.GetStore(storeID1).Value.GetItemByQuantity(itemID1));
            Assert.AreEqual(1, trading.GetStore(storeID2).Value.GetItemByQuantity(itemID2));
            //check that the shopping is same
            Assert.AreEqual(2, trading.GetDetailsOnCart(buyerID).Value.Baskets.Count);
        }

        /// <summary>
        /// Tests the purchase of items by a guest user when the supplier fails to provide the items.
        /// </summary>
        [TestMethod]
        public void PurchaseItemsGuestSupplierFail()
        {
            //Arrange
            trading.SetPaymentService(new Mocks.Mock_PaymentService());
            trading.SetSupplierService(new Mocks.Mock_Bad_SupplierService());
            // Act
            Assert.IsTrue(trading.PurchaseCart(buyerID, "0502485415400", "Rabbi Akiva 5 Beer Sheva").ErrorOccured);
            // Assert
            // check the order created
            Assert.IsNull(trading.GetStorePurchases(userID, storeID1).Value);
            Assert.IsNull(trading.GetStorePurchases(userID, storeID2).Value);
            // check the inventory stay
            Assert.AreEqual(3, trading.GetStore(storeID1).Value.GetItemByQuantity(itemID1));
            Assert.AreEqual(1, trading.GetStore(storeID2).Value.GetItemByQuantity(itemID2));
            //check that the shopping is same
            Assert.AreEqual(2, trading.GetDetailsOnCart(buyerID).Value.Baskets.Count);
        }

        /// <summary>
        /// Tests the purchase of items by a guest user when the quantity of the items is not valid.
        /// </summary>
        [TestMethod]
        public void PurchaseItemsGuestTheQuantitySmallerFail()
        {
            //Arrange
            trading.SetPaymentService(new Mocks.Mock_PaymentService());
            trading.SetSupplierService(new Mocks.Mock_SupplierService());
            trading.EditItemQuantity(userID, storeID1, itemID1, -2);
            // Act
            Assert.IsTrue(trading.PurchaseCart(buyerID, "0502485415400", "Rabbi Akiva 5 Beer Sheva").ErrorOccured);
            // Assert
            // check the order created
            Assert.IsNull(trading.GetStorePurchases(userID, storeID1).Value);
            Assert.IsNull(trading.GetStorePurchases(userID, storeID2).Value);
            // check the inventory stay
            Assert.AreEqual(1, trading.GetStore(storeID1).Value.GetItemByQuantity(itemID1));
            Assert.AreEqual(1, trading.GetStore(storeID2).Value.GetItemByQuantity(itemID2));
            //check that the shopping is same
            Assert.AreEqual(2, trading.GetDetailsOnCart(buyerID).Value.Baskets.Count);
        }
        /// <summary>
        /// Tests the Items prices in order is the same after edit.
        /// </summary>
        [TestMethod]
        public void ItemPriceStayTheSameAfterEdit()
        {
            //Arrange
            trading.SetPaymentService(new Mocks.Mock_PaymentService());
            trading.SetSupplierService(new Mocks.Mock_SupplierService());
            trading.PurchaseCart(buyerID, "0502485415400", "Rabbi Akiva 5 Beer Sheva");
            // Act
            trading.EditItemPrice(userID, storeID1, itemID1, 10000);
            // Assert
            // see the prices
            Assert.AreEqual(4000, trading.GetStorePurchases(userID, storeID1).Value[0].CalculatorAmount());
            Assert.AreEqual(3000, trading.GetStorePurchases(userID, storeID2).Value[0].CalculatorAmount());
            Assert.AreEqual(7000, Orders.Instance.GetOrdersByUserId(buyerID)[0].CalculatorAmount());
            //The item price edit
            Assert.AreEqual(10000, trading.GetStore(storeID1).Value.GetItemById(itemID1).Price);
        }
        
        [TestCleanup]
        public override void CleanUp()
        {
            base.CleanUp();
        }
    }
}