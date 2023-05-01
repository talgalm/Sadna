﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SadnaExpress.DomainLayer.Store;
using SadnaExpress.DomainLayer.Store.DiscountPolicy;

namespace SadnaExpressTests.Unit_Tests
{
    [TestClass()]
    public class DiscountPolicyUT
    {
        private Store store;
        private Guid item1;
        private Guid item2;
        private Guid item3;
        private DiscountPolicy policy1; //20% on all the store
        private DiscountPolicy policy2; //50% on bisli
        private DiscountPolicy policy3; //50% on Food
        private DiscountPolicy policy4; //30% on ipad

        private Condition cond1; // min value 50 nis
        private Condition cond2; // min quantity of ipad 2
        private Condition cond3; // min  quantity of bisli 1
        private Condition cond4; // min value food 100 nis
        private Dictionary<Item, int> basket;

        #region SetUp
        [TestInitialize]
        public void SetUp()
        {
            store = new Store("Hello");
            item1 = store.AddItem("Bisli", "Food", 10.0, 2);
            item2 = store.AddItem("Bamba", "Food", 8.0, 2);
            item3 = store.AddItem("Ipad", "electronic", 4000, 2);
            policy1 = store.CreateSimplePolicy(store, 20, new DateTime(2022, 4, 30), new DateTime(2024, 4, 30));
            policy2 = store.CreateSimplePolicy(store.GetItemById(item1), 50, new DateTime(2022, 4, 30),
                new DateTime(2024, 4, 30));
            policy3 = store.CreateSimplePolicy("Food", 50, new DateTime(2022, 4, 30),
                new DateTime(2024, 4, 30));
            policy4 = store.CreateSimplePolicy(store.GetItemById(item3), 30, new DateTime(2022, 4, 30),
                new DateTime(2024, 4, 30));
            basket = new Dictionary<Item, int> {{store.GetItemById(item1), 1}, {store.GetItemById(item2), 1},
                {store.GetItemById(item3), 1}};
            cond1 = store.AddCondition(store, "min value", 50);
            cond2 = store.AddCondition(store.GetItemById(item3), "min quantity", 2);
            cond3 = store.AddCondition(store.GetItemById(item1), "min quantity", 1);
            cond4 = store.AddCondition("Food", "min value", 100);

        }
        #endregion
        
        #region Simple policy calculate
        [TestMethod]
        public void CalculatePolicyOnItemSuccess()
        {
            //Arrange
            store.AddPolicy(policy2);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(5, items[store.GetItemById(item1)].Key); // changed
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item2))); // not return 
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item3))); //not return
        }
        
        [TestMethod]
        public void CalculatePolicyOnStoreSuccess()
        {
            //Arrange
            store.AddPolicy(policy1);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculatePolicyOnCategorySuccess()
        {
            //Arrange
            store.AddPolicy(policy3);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(5, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(4, items[store.GetItemById(item2)].Key); // changed
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item3))); //not return
        }

        [TestMethod]
        public void EndDatePassNoDiscount()
        {
            //Arrange
            DiscountPolicy policyPass = store.CreateSimplePolicy(store, 20, new DateTime(2022, 4, 30), new DateTime(2022, 5, 30));
            store.AddPolicy(policyPass);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count); 
        }
        #endregion
        
        #region If condition Caculate

        [TestMethod]
        public void CalculateConditionalPolicySuccess()
        {
            //Arrange
            DiscountPolicy complex = store.CreateComplexPolicy("if", cond1, policy1); //if I buy more than 50 I get 20% on the items
            store.AddPolicy(complex);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateConditionalPolicyFail()
        {
            //Arrange
            DiscountPolicy complex = store.CreateComplexPolicy("if", cond2, policy1); //if I buy more than 2 from Ipad I get 20% on the items
            store.AddPolicy(complex);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count); //the cond not pass
        }
        #endregion
        
        #region And condition Caculate
        [TestMethod]
        public void CalculateAndPolicyTwoOkSuccess()
        {
            //Arrange
            DiscountPolicy and = store.CreateComplexPolicy("and", cond1, cond3, policy1); //if I buy more than 50 nis and more than one bisli I get 20% on the items
            store.AddPolicy(and);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateAndPolicyOneFail()
        {
            //Arrange
            DiscountPolicy and = store.CreateComplexPolicy("and", cond1, cond2, policy1); //if I buy more than 50 nis and more than two Ipad I get 20% on the items
            store.AddPolicy(and);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count);
        }
        [TestMethod]
        public void CalculateAndPolicyTwoFail()
        {
            //Arrange
            DiscountPolicy and = store.CreateComplexPolicy("and", cond2, cond4, policy1);  //if I buy more than 100 nis and more than two Ipad I get 20% on the items
            store.AddPolicy(and);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count);
        }
        #endregion
        
        #region Or condition Caculate
        [TestMethod]
        public void CalculateOrPolicyOneOkCondSuccess()
        {
            //Arrange
            DiscountPolicy or = store.CreateComplexPolicy("or", cond4, cond3, policy1); //if I buy more than 100 nis food or more than one bisli I get 20% on the items
            store.AddPolicy(or);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateOrPolicyTwoOkCondSuccess()
        {
            //Arrange
            DiscountPolicy or = store.CreateComplexPolicy("or", cond1, cond3, policy1); //if I buy more than 50 nis or more than one bisli I get 20% on the items
            store.AddPolicy(or);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateOrPolicyBothFail()
        {
            //Arrange
            DiscountPolicy or = store.CreateComplexPolicy("or", cond2, cond4, policy1); //if I buy more than 100 nis Food and more than one Ipad I get 20% on the items
            store.AddPolicy(or);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count);
        }
        #endregion
        
        #region Xor condition Caculate
        [TestMethod]
        public void CalculateXorPolicyTwoOkFail()
        {
            //Arrange
            DiscountPolicy xor = store.CreateComplexPolicy("xor", cond1, cond3, policy1); //if I buy more than 50 nis xor more than one bisli I get 20% on the items
            store.AddPolicy(xor);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count);
        }
        [TestMethod]
        public void CalculateXorPolicyOneSuccess()
        {
            //Arrange
            DiscountPolicy xor = store.CreateComplexPolicy("xor", cond1, cond2, policy1); //if I buy more than 50 nis xor more than two Ipad I get 20% on the items
            store.AddPolicy(xor);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateXorPolicyTwoFail()
        {
            //Arrange
            DiscountPolicy xor = store.CreateComplexPolicy("xor", cond2, cond4, policy1);  //if I buy more than 100 nis xor more than two Ipad I get 20% on the items
            store.AddPolicy(xor);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(0,items.Count);
        }
        #endregion
        
        #region Max condition Caculate
        [TestMethod]
        public void CalculateMaxPolicy1BetterSuccess()
        {
            //Arrange
            DiscountPolicy max = store.CreateComplexPolicy("max",policy1, policy2); 
            store.AddPolicy(max);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            //on all the items better
            Assert.AreEqual(8, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateMaxPolicy3BetterSuccess()
        {
            //Arrange
            DiscountPolicy max = store.CreateComplexPolicy("max", policy1, policy4); 
            store.AddPolicy(max);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(2800, items[store.GetItemById(item3)].Key); // changed
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item1))); //not return
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item2))); //not return

        }
        #endregion
        
        #region Add condition Caculate
        [TestMethod]
        public void CalculateAddPolicySuccess()
        {
            //Arrange
            DiscountPolicy add = store.CreateComplexPolicy("add", policy1, policy3); //20% on store and 50% on food 
            store.AddPolicy(add);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(3, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(2.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        #endregion
        
        #region All Kind condition Caculate
        [TestMethod]
        public void CalculateLongXorAndPolicy()
        {
            //Arrange
            DiscountPolicy xor = store.CreateComplexPolicy("xor", cond1, cond2, policy1); //if I buy more than 50 nis xor more than two ipad I get 20% on the items
            DiscountPolicy and = store.CreateComplexPolicy("and", cond1, cond3, policy2); //if I buy more than 50 nis and more than one bisli I get 50% on the bisli
            store.AddPolicy(xor);
            store.AddPolicy(and);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(5, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }
        [TestMethod]
        public void CalculateLongAddAndPolicy()
        {
            //Arrange
            DiscountPolicy add = store.CreateComplexPolicy("add", policy1, policy2); //I have 70% on bisli and the rest 20%
            DiscountPolicy xor = store.CreateComplexPolicy("xor", cond1, cond2, policy3); //if I buy more than 50 nis xor more than two ipad I get 50% on the "Food"
            DiscountPolicy and = store.CreateComplexPolicy("and", cond1, cond3, xor);  //if I buy more than 50 nis and more than one bisli
            store.AddPolicy(add);
            store.AddPolicy(and);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(3, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(3200, items[store.GetItemById(item3)].Key); // changed
        }

        [TestMethod]
        public void CalculateTwoNotCommonPolicys()
        {
            //Arrange
            DiscountPolicy add = store.CreateComplexPolicy("add", policy1, policy2); //I have 70% on bisli and the rest 20%

            store.AddPolicy(policy4);
            store.AddPolicy(add);
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.AreEqual(3, items[store.GetItemById(item1)].Key); // changed
            Assert.AreEqual(6.4, items[store.GetItemById(item2)].Key); // changed
            Assert.AreEqual(2800, items[store.GetItemById(item3)].Key); // changed
        }

        [TestMethod]
        public void CalculateOneCondNotPolicys()
        {
            //Arrange
            DiscountPolicy and = store.CreateComplexPolicy("and", cond1, cond2, policy2); //I have 70% on bisli and the rest 20%
            store.AddPolicy(policy4);
            store.AddPolicy(and);
            
            //Act
            Dictionary<Item, KeyValuePair<double, DateTime>> items = store.DiscountPolicyTree.calculate(store, basket);
            //Assert
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item1))); //not return
            Assert.IsFalse(items.ContainsKey(store.GetItemById(item2))); //not return
            Assert.AreEqual(2800, items[store.GetItemById(item3)].Key); // changed
        }
        #endregion
        
        #region Clean Up
        [TestCleanup]
        public void CleanUp()
        {
            store.DiscountPolicyTree = null;
        }
        #endregion
    }
}