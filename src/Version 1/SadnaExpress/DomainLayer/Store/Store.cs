using System;
using System.Collections.Generic;
using System.Diagnostics;
using SadnaExpress.DomainLayer.User;

namespace SadnaExpress.DomainLayer.Store
{
    public class Store
    {
        private string storeName;
        public string StoreName {get => storeName; set => storeName = value;}
        private Inventory itemsInventory;
        private Guid storeID;
        public Guid StoreID {get=>storeID;}
        //private Policy policy; // not for this version

        private bool active;
        public bool Active { get => active; set => active = value; }

        private int storeRating;
        public int StoreRating {  get => storeRating ; set => StoreRating = value; }

        public Store(string name) {
            storeName = name;
            itemsInventory = new Inventory();
            storeRating = 0;
            storeID = Guid.NewGuid();
            active = true;
            //this.policy = new Policy();
        }

        public Item GetItemsByName(string itemName, int minPrice, int maxPrice, string category, int ratingItem)
        {
            return itemsInventory.GetItemByName(itemName, minPrice, maxPrice,category, ratingItem);
        }
        public List<Item> GetItemsByCategory(string category, int minPrice, int maxPrice, int ratingItem)
        {
            return itemsInventory.GetItemsByCategory(category, minPrice, maxPrice, ratingItem);
        }
        public List<Item> GetItemsByKeysWord(string keyWords, int minPrice, int maxPrice, int ratingItem, string category)
        {
            return itemsInventory.GetItemsByKeysWord(keyWords, minPrice, maxPrice, ratingItem, category);
        }
        
        
        
        
        
        
        
        
        
        
        
        

        // add new Item to store, if item exists with the same name return false
        public void addItem(string name, string category, double price, int quantity)
        {
            itemsInventory.AddItem(name, category, price, quantity);
        }

        public void AddQuantity(int itemID, int addedQuantity)
        {
            itemsInventory.AddQuantity(this.itemsInventory.getItemById(itemID), addedQuantity);
        }

        public void RemoveQuantity(int itemId, int removedQuantity)
        {
            itemsInventory.RemoveQuantity(this.itemsInventory.getItemById(itemId), removedQuantity);
        }

        public void EditItemName(int itemId, string name)
        {
            itemsInventory.getItemById(itemId);
        }

        public void EditItemCategory(int itemId, string category)
        {
            this.itemsInventory.EditItemCategory(itemId,category);
        }

        public void EditItemPrice(int itemId, double price)
        {
            this.itemsInventory.EditItemPrice(itemId,price);
        }

        public void RemoveItemById(int itemId)
        {
            this.itemsInventory.RemoveItem(this.itemsInventory.getItemById(itemId));
        }

       /* public void RemoveItemByName(string itemName)
        {
            itemsInventory.RemoveItem(this.itemsInventory.GetItemByName(itemName));
        }
        */
    }
}