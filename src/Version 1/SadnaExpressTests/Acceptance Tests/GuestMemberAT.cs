using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SadnaExpress.DomainLayer.Store;
using SadnaExpress.ServiceLayer;
using static SadnaExpressTests.Mocks;

namespace SadnaExpressTests.Acceptance_Tests
{
    [TestClass]
    public class GuestMemberAT: TradingSystemAT
    {
        protected Guid storeidNew;

        [TestInitialize]
        public override void SetUp()
        {

            base.SetUp();
            proxyBridge.GetMember(memberId).Value.LoggedIn = true;
            proxyBridge.GetMember(memberId2).Value.LoggedIn = true;
            proxyBridge.GetMember(memberId3).Value.LoggedIn = true;
            proxyBridge.GetMember(memberId4).Value.LoggedIn = true;
          
        }

        #region Logout 3.1
        [TestMethod]
        public void UserLogout_HappyTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid =proxyBridge.Login(tempid, "RotemSela@gmail.com", "AS87654askj").Value;
                return proxyBridge.Logout(loggedid);
            });

            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error accured 
            Assert.IsFalse(task.Result.Value==Guid.Empty); //when logged out member gets valid id
            Assert.IsNotNull(proxyBridge.GetUser(task.Result.Value).Value); //when member logout he moves to be user in the TS
        }

        [TestMethod]
        public void UserLogoutWithoutEnter_BadTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                return proxyBridge.Logout(systemManagerid);
            });

            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //error accured 
            Assert.IsTrue(task.Result.Value == Guid.Empty); //Guid returns empty (default value)
        }

        [TestMethod]
        public void UserLogoutWithoutLogin_BadTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                return proxyBridge.Logout(tempid);
            });

            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //error accured 
            Assert.IsTrue(task.Result.Value == Guid.Empty); //Guid returns empty (default value)
        }


        [TestMethod]
        public void UserLogoutTwice_BadTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "RotemSela@gmail.com", "AS87654askj").Value;
                proxyBridge.Logout(loggedid);
                return proxyBridge.Logout(loggedid);
            });

            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //error accured 
            Assert.IsTrue(task.Result.Value == Guid.Empty); //Guid returns empty (default value)
        }

        [TestMethod]
        public void UserLogoutFirstBadSecGood_HappyTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "RotemSela@gmail.com", "AS87654askj").Value;
                proxyBridge.Logout(tempid);
                return proxyBridge.Logout(loggedid);
            });

            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //error accured 
            Assert.IsFalse(task.Result.Value == Guid.Empty); //Guid returns empty (default value)
        }
        #endregion

        #region Opening a store 3.2
        [TestMethod]
        public void MemberOpenStore_HappyTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "RotemSela@gmail.com", "AS87654askj").Value;
                return proxyBridge.OpenNewStore(loggedid,"My store");
            });

            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error accured 
            Assert.IsFalse(task.Result.Value == Guid.Empty); 
            Assert.IsNotNull(proxyBridge.GetStore(task.Result.Value)); //check that store exist
        }

        [TestMethod]
        public void MemberOpenStoreWithEmptyStoreName_BadTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "RotemSela@gmail.com", "AS87654askj").Value;
                return proxyBridge.OpenNewStore(loggedid, "");
            });

            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //no error accured 
            Assert.IsTrue(task.Result.Value == Guid.Empty);
            Assert.IsTrue(proxyBridge.GetStore(task.Result.Value).ErrorOccured); //check that store does not exists
        }

        [TestMethod]
        public void MemberOpenStoreNameThatAlreadyExist_BadTest()
        {
            Guid tempid = Guid.Empty;
            Task<ResponseT<Guid>> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "RotemSela@gmail.com", "AS87654askj").Value;
                return proxyBridge.OpenNewStore(loggedid, "Zara");
            });

            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //error accured 
            Assert.IsTrue(task.Result.Value == Guid.Empty);
            Assert.IsTrue(proxyBridge.GetStore(task.Result.Value).ErrorOccured); //check that store does not exists
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleMembersAddStores_HappyTest()
        {
            Guid id1 = Guid.Empty;
            Guid id2 = Guid.Empty;
            Guid id3 = Guid.Empty;
            Guid id4 = Guid.Empty;
            Task<ResponseT<Guid>>[] clientTasks = new Task<ResponseT<Guid>>[] {
                Task.Run(() => {
                    id1=proxyBridge.Enter().Value;
                    Guid loggedid = proxyBridge.Login(id1, "RotemSela@gmail.com", "AS87654askj").Value;
                    return proxyBridge.OpenNewStore(loggedid, "Zara");
                }),
                Task.Run(() => {
                    id2=proxyBridge.Enter().Value;
                    Guid loggedid = proxyBridge.Login(id2, "gil@gmail.com", "asASD876!@").Value;
                    return proxyBridge.OpenNewStore(loggedid, "Apple");
                }),
                Task.Run(() => {
                    id3=proxyBridge.Enter().Value;
                    proxyBridge.Register(id3, "tal.galmor@weka.io","tal", "galmor", "123AaC!@#");
                    Guid loggedid = proxyBridge.Login(id3, "tal.galmor@weka.io", "123AaC!@#").Value;
                    return proxyBridge.OpenNewStore(loggedid, "Jumbo");
                })
             };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsTrue(clientTasks[0].Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetStore(clientTasks[0].Result.Value).ErrorOccured);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured);//no error occurred
            Assert.IsNotNull(proxyBridge.GetStore(clientTasks[1].Result.Value));

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured);//no error occurred
            Assert.IsNotNull(proxyBridge.GetStore(clientTasks[2].Result.Value));
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleMembersAddStoresSameName_HappyTest()
        {
            Guid id1 = Guid.Empty;
            Guid id2 = Guid.Empty;
            Task<ResponseT<Guid>>[] clientTasks = new Task<ResponseT<Guid>>[] {
                Task.Run(() => {
                    id1=proxyBridge.Enter().Value;
                    Guid loggedid = proxyBridge.Login(id1, "gil@gmail.com", "asASD876!@").Value;
                    return proxyBridge.OpenNewStore(loggedid, "Jumbo");
                }),
                Task.Run(() => {
                    id2=proxyBridge.Enter().Value;
                    proxyBridge.Register(id2, "tal.galmor@weka.io","tal", "galmor", "123AaC!@#");
                    Guid loggedid = proxyBridge.Login(id2, "tal.galmor@weka.io", "123AaC!@#").Value;
                    return proxyBridge.OpenNewStore(loggedid, "Jumbo");
                })
             };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsTrue(proxyBridge.GetStore(clientTasks[0].Result.Value).Value==null && proxyBridge.GetStore(clientTasks[1].Result.Value).Value != null||
                proxyBridge.GetStore(clientTasks[0].Result.Value).Value != null && proxyBridge.GetStore(clientTasks[1].Result.Value).Value == null);
        }


        #endregion

        #region Writing a review on items the user purchased 3.3
        [TestMethod]
        public void MemberWriteReviewOnItemPurchased_HappyTest()
        {
            //Arrange
            Mock_Orders mock_Orders = new Mock_Orders();
            mock_Orders.AddOrderToUser(memberId, new Order(new List<ItemForOrder> { new ItemForOrder(proxyBridge.GetItemByID(storeid1, itemid1).Value,storeid1) }));
            proxyBridge.SetTSOrders(mock_Orders);

            //Act
            Guid tempid = Guid.Empty;
            Task<Response> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "gil@gmail.com", "asASD876!@").Value;
                return proxyBridge.WriteItemReview(memberId, storeid1, itemid1, "very good item!");
            });

            task.Wait();

            //Assert
            Assert.IsFalse(task.Result.ErrorOccured); //no error accured 
            Assert.IsTrue(proxyBridge.GetItemReviews(storeid1, itemid1).Value[memberId].Count==1);
        }

        [TestMethod]
        public void MemberWriteReviewOnItemHeDidNotPurchased_BadTest()
        {
            //Arrange
            Mock_Orders mock_Orders = new Mock_Orders();
            proxyBridge.SetTSOrders(mock_Orders);

            //Act
            Guid tempid = Guid.Empty;
            Task<Response> task = Task.Run(() => {
                tempid = proxyBridge.Enter().Value;
                Guid loggedid = proxyBridge.Login(tempid, "gil@gmail.com", "asASD876!@").Value;
                return proxyBridge.WriteItemReview(memberId, storeid1, itemid1, "very good item!");
            });

            task.Wait();

            //Assert
            Assert.IsTrue(task.Result.ErrorOccured); // error accured 
            Assert.IsFalse(proxyBridge.GetItemReviews(storeid1, itemid1).Value.ContainsKey(memberId));
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleMembersWriteAreviewOnSameItem_HappyTest()
        {
            //Arrange
            Mock_Orders mock_Orders = new Mock_Orders();
            mock_Orders.AddOrderToUser(memberId, new Order(new List<ItemForOrder> { new ItemForOrder(proxyBridge.GetItemByID(storeid1, itemid1).Value, storeid1) }));
            mock_Orders.AddOrderToUser(systemManagerid, new Order(new List<ItemForOrder> { new ItemForOrder(proxyBridge.GetItemByID(storeid1, itemid1).Value, storeid1) }));
            proxyBridge.SetTSOrders(mock_Orders);


            Guid id1 = Guid.Empty;
            Guid id2 = Guid.Empty;
            Guid id3 = Guid.Empty;
            Guid id4 = Guid.Empty;
            Task<Response>[] clientTasks = new Task<Response>[] {
                Task.Run(() => {
                    id1=proxyBridge.Enter().Value;
                    Guid loggedid = proxyBridge.Login(id1, "RotemSela@gmail.com", "AS87654askj").Value;
                    return proxyBridge.WriteItemReview(systemManagerid, storeid1, itemid1, "very good item!");
                }),
                Task.Run(() => {
                    id2 = proxyBridge.Enter().Value;
                    Guid loggedid = proxyBridge.Login(id2, "gil@gmail.com", "asASD876!@").Value;
                    return proxyBridge.WriteItemReview(memberId, storeid1, itemid1, "very bad item!");
                })
             };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsFalse(clientTasks[0].Result.ErrorOccured);//no error occurred
            Assert.IsFalse(clientTasks[1].Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetItemReviews(storeid1, itemid1).Value.Count==2);            
        }
        #endregion

        /// <summary>
        /// Guest buying actions are the same for members with little diffrences 
        /// </summary>

        #region Member Getting information about stores in the market and the products in the stores 2.1
        [TestMethod]
        public void StoreInfoUpdated1_HappyTest()
        {
            int count = 0;
            Console.WriteLine(count);
            Task<ResponseT<List<Store>>> task = Task.Run(() =>
            {
                count = proxyBridge.GetAllStoreInfo().Value.Count;
                Store newStore = new Store("Pull&Bear");
                 stores.TryAdd(newStore.StoreID,newStore);
                 return proxyBridge.GetAllStoreInfo();
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == count + 1 );
        }
        [TestMethod]
        public void StoreInfoUpdated2_HappyTest()
        {
            int count = 0;
            Console.WriteLine(count);
            Task<ResponseT<List<Store>>> task = Task.Run(() =>
            {
                count = proxyBridge.GetAllStoreInfo().Value.Count;
                Store newStore = new Store("Pull&Bear");
                stores.TryAdd(newStore.StoreID,newStore);
                stores.TryRemove(newStore.StoreID,out newStore);

                return proxyBridge.GetAllStoreInfo();
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == count);
        }
        
        [TestMethod]
        public void MultipleClientsGetInfo_HappyTest()
        {
            Store newStore = new Store("Pull&Bear");
            stores.TryAdd(newStore.StoreID,newStore);

            int count = proxyBridge.GetAllStoreInfo().Value.Count;

            Task<ResponseT<List<Store>>>[] clientTasks = new Task<ResponseT<List<Store>>>[] {
                Task.Run(() => {
                    return proxyBridge.GetAllStoreInfo();
                }),
                Task.Run(() => {
                    return proxyBridge.GetAllStoreInfo();
                }),
            };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsFalse(clientTasks[0].Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetAllStoreInfo().Value.Count == count);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured);//error occurred
            Assert.IsTrue(proxyBridge.GetAllStoreInfo().Value.Count == count);
            
        }

        
        
        #endregion

        #region Member search products by general search or filters 2.2

        [TestMethod]
        public void MemberSearchProductsByCategoryHome_HappyTest()
        {
            Task<ResponseT<List<Item>>> task = Task.Run(() =>
            {
                return proxyBridge.GetItemsByCategory(memberId, "Home");
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == 1);
        }

        [TestMethod]
        public void MemberSearchProductsByCategoryClothes_HappyTest()
        {
            Task<ResponseT<List<Item>>> task = Task.Run(() =>
            {
                return proxyBridge.GetItemsByCategory(memberId, "clothes");
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == 3);
        }

        [TestMethod]
        public void MemberSearchProductsByCategoryClothesMixMaxPrice_HappyTest()
        {
            Task<ResponseT<List<Item>>> task = Task.Run(() =>
            {
                return proxyBridge.GetItemsByCategory(memberId, "clothes", 90);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == 2);
        }

        [TestMethod]
        public void MemberSearchProductsByCategoryThatDoesntExist_BadTest()
        {
            Task<ResponseT<List<Item>>> task = Task.Run(() =>
            {
                return proxyBridge.GetItemsByCategory(memberId, "shay");
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == 0);
        }

        [TestMethod]
        public void MemberSearchProductsByNameTowel_HappyTest()
        {
            Task<ResponseT<List<Item>>> task = Task.Run(() => { return GetItemsByName("Towel"); });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(task.Result.Value.Count == 1);
        }

        private ResponseT<List<Item>> GetItemsByName(string itemName, int minPrice = 0, int maxPrice = Int32.MaxValue,
            int ratingItem = -1, string category = null, int ratingStore = -1)
        {
            return proxyBridge.GetItemsByName(memberId, itemName, minPrice, maxPrice, ratingItem, category,
                ratingStore);
        }

        private ResponseT<List<Item>> GetItemsByCategory(string category, int minPrice = 0,
            int maxPrice = Int32.MaxValue, int ratingItem = -1, int ratingStore = -1)
        {

            return proxyBridge.GetItemsByCategory(memberId, category, minPrice, maxPrice, ratingItem, ratingStore);
        }

        private ResponseT<List<Item>> GetItemsByKeysWord(string keyWords, int minPrice = 0,
            int maxPrice = Int32.MaxValue, int ratingItem = -1, string category = null, int ratingStore = -1)
        {
            return proxyBridge.GetItemsByKeysWord(memberId, keyWords, minPrice, maxPrice, ratingItem, category,
                ratingStore);
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleClientsSearchForItems1_HappyTest()
        {
            Task<ResponseT<List<Item>>>[] clientTasks = new Task<ResponseT<List<Item>>>[]
            {
                Task.Run(() => GetItemsByName("Towel")),
                Task.Run(() => GetItemsByCategory("clothes")),
                Task.Run(() => GetItemsByCategory("clothes", 90))
            };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsFalse(clientTasks[0].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[0].Result.Value.Count == 1);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[1].Result.Value.Count == 3);

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[2].Result.Value.Count == 2);
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleClientsSearchForItems2_HappyTest()
        {
            Task<ResponseT<List<Item>>>[] clientTasks = new Task<ResponseT<List<Item>>>[]
            {
                Task.Run(() => GetItemsByName("Towel")),
                Task.Run(() => GetItemsByCategory("Home")),
                Task.Run(() => GetItemsByCategory("clothes", 90)),
                Task.Run(() => GetItemsByKeysWord("to"))
            };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsFalse(clientTasks[0].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[0].Result.Value.Count == 1);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[1].Result.Value.Count == 1);

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[2].Result.Value.Count == 2);

            Assert.IsFalse(clientTasks[3].Result.ErrorOccured);
            Assert.IsTrue(clientTasks[3].Result.Value.Count == 2);
        }

        #endregion

        #region Member saving item in the shopping cart for some store 2.3

        public void MemberSave1ItemInShoppingCart_HappyTest()
        {
            Task<Response> task = Task.Run(() =>
            {
                return proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);

        }

        [TestMethod]
        public void MemberSaveItemsInShoppingCartFromDiffStore_HappyTest()
        {
            Task<Response> task = Task.Run(() =>
            {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.AddItemToCart(memberId, storeid2, itemid2, 1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 2);

        }

        [TestMethod]
        public void MemberSave2ItemsInShoppingCartFromSameStore_HappyTest()
        {
            Task<Response> task = Task.Run(() =>
            {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.AddItemToCart(memberId, storeid1, itemid11, 1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);
        }

        [TestMethod]
        public void MemberSaveItemsInShoppingCartItemDoesNotExist_HappyTest()
        {
            Task<Response> task = Task.Run(() =>
            {
                return proxyBridge.AddItemToCart(memberId, storeid1, Guid.NewGuid(), 1);
            });
            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);
        }

        [TestMethod]
        public void MemberSaveItemsInShoppingCartStoreDoesNotExist_BadTest()
        {
            Task<Response> task = Task.Run(() =>
            {
                return proxyBridge.AddItemToCart(memberId, Guid.NewGuid(), itemid1, 1);
            });
            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured); //error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);
        }

        private Response AddItemToCart(Guid id, Guid storeid, Guid itemid, int itemAmount)
        {
            return proxyBridge.AddItemToCart(id, storeid, itemid, itemAmount);
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleClientsAddSameItemToShoppingCart_HappyTest()
        {

            Task<Response>[] clientTasks = new Task<Response>[]
            {
                Task.Run(() =>
                {
                    return AddItemToCart(memberId, storeid1, itemid1, 1);
                }),
                Task.Run(() =>
                {
                    return AddItemToCart(memberId2, storeid1, itemid1, 1);
                }),
                Task.Run(() =>
                {
                    return AddItemToCart(memberId3, storeid1, itemid1, 1);
                }),
                Task.Run(() =>
                {
                    return AddItemToCart(memberId4, storeid1, itemid1, 1);
                }),
            };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsFalse(clientTasks[0].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId2).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId3).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[3].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId4).Value.Baskets.Count == 1);
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleClientsAddSameItemToShoppingCartOneNotEnter_HappyTest()
        {

            proxyBridge.GetMember(memberId).Value.LoggedIn = false;

            Task<Response>[] clientTasks = new Task<Response>[]
            {
                Task.Run(() => { return AddItemToCart(memberId, storeid1, itemid1, 1); }),
                Task.Run(() =>
                {
                    memberId2 = proxyBridge.Enter().Value;
                    return AddItemToCart(memberId2, storeid1, itemid1, 1);
                }),
                Task.Run(() =>
                {
                    memberId3 = proxyBridge.Enter().Value;
                    return AddItemToCart(memberId3, storeid1, itemid1, 1);
                }),
                Task.Run(() =>
                {
                    memberId4 = proxyBridge.Enter().Value;
                    return AddItemToCart(memberId4, storeid1, itemid1, 1);
                }),
            };


            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsTrue(clientTasks[0].Result.ErrorOccured); //error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).ErrorOccured);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId2).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId3).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[3].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId4).Value.Baskets.Count == 1);
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleClientsAddSameItemToCartOneNotEnterAndOneChooseItemNotInStock_HappyTest()
        {
            proxyBridge.GetMember(memberId).Value.LoggedIn = false;


            Task<Response>[] clientTasks = new Task<Response>[]
            {
                Task.Run(() => { return AddItemToCart(memberId, storeid1, itemid1, 1); }),
                Task.Run(() =>
                {
                    return AddItemToCart(memberId2, storeid2, itemNoStock, 1);
                }),
                Task.Run(() =>
                {
                    return AddItemToCart(memberId3, storeid1, itemid1, 1);
                }),
                Task.Run(() =>
                {
                    return AddItemToCart(memberId4, storeid1, itemid1, 1);
                }),
            };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsTrue(clientTasks[0].Result.ErrorOccured); //error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).ErrorOccured);

            Assert.IsTrue(clientTasks[1].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId2).Value.Baskets.Count == 0);

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId3).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[3].Result.ErrorOccured); //no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId4).Value.Baskets.Count == 1);
        }

        #endregion

        #region Member checking the content of the shopping cart and making changes 2.4

        [TestMethod]
        public void MemberShoppingCartSavedAfterLogOut_HappyTest()
        {
            Task<Response> task = Task.Run(() => {
                Guid id  = proxyBridge.Enter().Value;
                proxyBridge.Login(id, "gil@gmail.com", "asASD876!@");
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 2);
                proxyBridge.Logout(memberId);
                Guid id2  = proxyBridge.Enter().Value;
                proxyBridge.Login(id2, "gil@gmail.com", "asASD876!@");
                return proxyBridge.AddItemToCart(memberId, storeid1, itemid1,1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(
                (proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1) == 3));
        }
        
        [TestMethod]
        public void GuestAddItemsAndThenLoggedInAsMember_HappyTest()
        {
            proxyBridge.GetMember(memberId).Value.LoggedIn = false;

            Task<Response> task = Task.Run(() => {
                Guid id  = proxyBridge.Enter().Value;
                proxyBridge.AddItemToCart(id, storeid1, itemid1, 2);
                proxyBridge.Login(id, "gil@gmail.com", "asASD876!@");
                return proxyBridge.AddItemToCart(memberId, storeid1, itemid1,1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Console.WriteLine((proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1)));
            Assert.IsTrue(
                (proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1) == 3));
        }
        [TestMethod]
        public void MemberShoppingCartSavedAfterExit_HappyTest()
        {
            Task<Response> task = Task.Run(() => {
                Guid id  = proxyBridge.Enter().Value;
                proxyBridge.Login(id, "gil@gmail.com", "asASD876!@");
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 2);
                proxyBridge.Exit(memberId);
                Guid id2  = proxyBridge.Enter().Value;
                proxyBridge.Login(id2, "gil@gmail.com", "asASD876!@");
                return proxyBridge.AddItemToCart(memberId, storeid1, itemid1,1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(
                (proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1) == 3));
        }

    

    [TestMethod]
        public void MemberEditShoppingCartAddAndRemove_HappyTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.RemoveItemFromCart(memberId, storeid1, itemid1);
            });

            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);
        }

        [TestMethod]
        public void MemberEditShoppingCartAddAndEditQuantity_HappyTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.EditItemFromCart(memberId, storeid1, itemid1, 3);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1,itemid1)==3);
        }

        [TestMethod]
        public void MemberEditShoppingCartAddAndEditQuantityNoChange_HappyTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.EditItemFromCart(memberId, storeid1, itemid1, 1);
            });
            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);
        }

        [TestMethod]
        public void MemberEditShoppingCartItemThatDoesNotExist_BadTest()
        {
            Task<Response> task = Task.Run(() => {
                return proxyBridge.EditItemFromCart(memberId, storeid1, itemid1, 1);
            });
            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);
        }

        [TestMethod]
        public void MemberEditShoppingCartWithBadStoreId_BadTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.EditItemFromCart(memberId, storeid2, itemid1, 4);
            });
            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1) == 1);
        }

        [TestMethod]
        public void MemberRemoveItemFromShoppingCartThatDoesNotExist_BadTest()
        {
            Task<Response> task = Task.Run(() => {
                return proxyBridge.RemoveItemFromCart(memberId, storeid1, itemid1);
            });

            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);
        }

        [TestMethod]
        public void MemberRemoveItemFromShoppingCart_HappyTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 4);
                return proxyBridge.RemoveItemFromCart(memberId, storeid1, itemid1);
            });

            task.Wait();
            Assert.IsFalse(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);
        }

        [TestMethod]
        public void MemberRemoveItemFromShoppingCartBadStoreId_BadTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.EditItemFromCart(memberId, Guid.NewGuid(), itemid1, 4);
            });
            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1) == 1);
        }

        [TestMethod]
        public void MemberRemoveItemFromShoppingCartBadItemId_BadTest()
        {
            Task<Response> task = Task.Run(() => {
                proxyBridge.AddItemToCart(memberId, storeid1, itemid1, 1);
                return proxyBridge.EditItemFromCart(memberId, storeid1, Guid.NewGuid(), 4);
            });
            task.Wait();
            Assert.IsTrue(task.Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 1);
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.GetItemQuantityInCart(storeid1, itemid1) == 1);
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public void MultipleClientsEditShoppingCart_HappyTest()
        {
            Task<Response>[] clientTasks = new Task<Response>[] {
                Task.Run(() => {
                    AddItemToCart(memberId,storeid1,itemid1,1);
                    AddItemToCart(memberId,storeid1,itemid1,1);
                    return proxyBridge.RemoveItemFromCart(memberId,storeid1,itemid1);
                }),
                Task.Run(() => {
                    AddItemToCart(memberId2,storeid1,itemid1,1);
                    AddItemToCart(memberId2,storeid2,itemid2,1);
                    AddItemToCart(memberId2,storeid1,itemid1,1);
                    return proxyBridge.EditItemFromCart(memberId2,storeid1,itemid1,0);
                }),
                Task.Run(() => {
                    AddItemToCart(memberId3,storeid1,itemid1,1);
                    proxyBridge.EditItemFromCart(memberId3,storeid1,itemid2,0);
                    return proxyBridge.EditItemFromCart(memberId3,storeid1,itemid1,0);
                })
             };

            // Wait for all clients to complete
            Task.WaitAll(clientTasks);

            Assert.IsFalse(clientTasks[0].Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId).Value.Baskets.Count == 0);

            Assert.IsFalse(clientTasks[1].Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId2).Value.Baskets.Count == 1);

            Assert.IsFalse(clientTasks[2].Result.ErrorOccured);//no error occurred
            Assert.IsTrue(proxyBridge.GetUserShoppingCart(memberId3).Value.Baskets.Count == 0);
        }
        #endregion

        #region Member making a purchase of the shopping cart 2.5
        #endregion

        [TestCleanup]
        public override void CleanUp()
        {
            base.CleanUp();
        }
    }
}