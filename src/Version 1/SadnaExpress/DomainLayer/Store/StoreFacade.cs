using SadnaExpress.ServiceLayer;
using SadnaExpress.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SadnaExpress.DomainLayer.Store
{
    public class StoreFacade : IStoreFacade
    {
        private ConcurrentDictionary<Guid, Store> stores;
        private ConcurrentDictionary<Guid, LinkedList<Order>> storeOrders;
        private bool _isTSInitialized;
        private Orders _orders = Orders.Instance;

        public StoreFacade()
        {
            stores = new ConcurrentDictionary<Guid, Store>();
            storeOrders = new ConcurrentDictionary<Guid, LinkedList<Order>>();
        }
        
        public Guid OpenNewStore(string storeName)
        {
            IsTsInitialized();
            if (storeName.Length == 0)
                throw new Exception("Store name can not be empty");
            Store store = new Store(storeName);
            stores.TryAdd(store.StoreID, store);
            Logger.Instance.Info("store " + storeName + " opened.");
            return store.StoreID;
        }

        public void CloseStore(Guid storeID)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].Active = false;
            Logger.Instance.Info("store " + stores[storeID].StoreName + " closed.");
        }
        public void ReopenStore(Guid storeID)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].Active = true;
            Logger.Instance.Info("store " + stores[storeID].StoreName + " reopen.");
        }
        public void DeleteStore(Guid storeID)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores.TryRemove(storeID, out var store);
            Logger.Instance.Info("store " + store.StoreName + " deleted.");
        }
        
        public LinkedList<Order> GetStorePurchases(Guid storeID)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            return storeOrders[storeID];
        }
        public void PurchaseItems(string storeName, List<string> itemsName)
        {
            IsTsInitialized();
            throw new System.NotImplementedException();
        }

        public void GetAllStoreInfo(string storeName)
        {
            IsTsInitialized();
            throw new System.NotImplementedException();
        }

        public Guid AddItemToStore(Guid storeID, string itemName, string itemCategory, double itemPrice, int quantity)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            return stores[storeID].AddItem(itemName, itemCategory, itemPrice, quantity);
        }

        public void RemoveItemFromStore(Guid storeID, Guid itemId)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].RemoveItem(itemId);
        }
        
        public void EditItemName(Guid storeID, Guid itemID, string name)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].EditItemName(itemID, name);
        }
        
        public void EditItemCategory(Guid storeID, Guid itemID, string category)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].EditItemCategory(itemID, category);
        }
        public void EditItemPrice(Guid storeID, Guid itemID, int price)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].EditItemPrice(itemID, price);
        }
        public void EditItemQuantity(Guid storeID, Guid itemID, int quantity)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].EditItemQuantity(itemID, quantity);
        }
        public List<Item> GetItemsByName(string itemName, int minPrice, int maxPrice, int ratingItem, string category, int ratingStore)
        {
            IsTsInitialized();
            List<Item> allItems = new List<Item>(); 
            foreach (Store store in stores.Values)
            {
                if (!store.Active)
                    continue;
                if (ratingStore != -1 && store.StoreRating != ratingStore)
                    continue;
                Item item = store.GetItemsByName(itemName, minPrice, maxPrice, category, ratingItem);
                if (item != null)
                    allItems.Add(item);
            }
            return allItems;
        }
        public List<Item> GetItemsByCategory(string category, int minPrice, int maxPrice, int ratingItem, int ratingStore)
        {
            IsTsInitialized();
            List<Item> allItems = new List<Item>(); 
            foreach (Store store in stores.Values)
            {
                if (!store.Active)
                    continue;
                if (ratingStore != -1 && store.StoreRating != ratingStore)
                    continue;
                allItems.AddRange(store.GetItemsByCategory(category, minPrice, maxPrice, ratingItem));
            }
            return allItems;
        }
        public List<Item> GetItemsByKeysWord(string keyWords, int minPrice, int maxPrice, int ratingItem, string category, int ratingStore)
        {
            IsTsInitialized();
            List<Item> allItems = new List<Item>(); 
            foreach (Store store in stores.Values)
            {
                if (!store.Active)
                    continue;
                if (ratingStore != -1 && store.StoreRating != ratingStore)
                    continue;
                allItems.AddRange(store.GetItemsByKeysWord(keyWords, minPrice, maxPrice, ratingItem, category));
            }
            return allItems;
        }
        public void CleanUp()
        {
           storeOrders.Clear();
           stores.Clear();
        }

        public ConcurrentDictionary<Guid, Store> GetStores()
        {
            return stores;
        }

        public void SetIsSystemInitialize(bool isInitialize)
        {
            _isTSInitialized = isInitialize;
        }

        private void IsTsInitialized()
        {
            if (_isTSInitialized == false)
                throw new Exception("Cannot preform any action because system trading is closed");
        }

        private void IsStoreExist(Guid storeID)
        {
            if (!stores.ContainsKey(storeID))
                throw new Exception("Store with this id does not exist");
        }
            

        public void WriteItemReview(Guid userID, Guid storeID, Guid itemID, string reviewText)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            Store store = stores[storeID];
            store.WriteItemReview(userID, itemID, reviewText);
        }

        public void AddItemToCart(Guid storeID, Guid itemID, int quantity)
        {
            IsTsInitialized();
            IsStoreExist(storeID);
            stores[storeID].AddItemToCart(itemID, quantity);
        }
    }
}
