using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using SadnaExpress.DomainLayer.Store;
using SadnaExpress.Services;

namespace SadnaExpress.DomainLayer.User
{
    public interface IUserFacade
    {
        Guid Enter();
        void Exit(Guid userID);
        void Register(Guid userID, string email, string firstName, string lastLame, string password);
        Guid Login(Guid userID, string email, string password);
        Guid Logout(Guid userID);
        void AddItemToCart(Guid userID,Guid storeID, Guid itemID,  int itemAmount);
        void RemoveItemFromCart(Guid userID,Guid storeID, Guid itemID);
        void EditItemFromCart(Guid userID,Guid storeID, Guid itemID,  int itemAmount);
        Dictionary<Guid,List<Guid>> GetDetailsOnCart(Guid userID);
        void PurchaseCart(Guid userID);
        void EditItemCart(Guid userID,Guid storeID, string itemName);
        void OpenNewStore(Guid userID,Guid storeID);
        void AddReview(Guid userID,Guid storeID, string itemName);
        void AddItemToStore(Guid userID,Guid storeID);
        void RemoveItemFromStore(Guid userID,Guid storeID);
        void EditItem(Guid userID,Guid storeID);
        void AppointStoreOwner(Guid userID,Guid storeID, string email);
        void AppointStoreManager(Guid userID, Guid storeID, string email);
        void AddStoreManagerPermissions(Guid userID,Guid storeID, string email, string Permission);
        void RemoveStoreManagerPermissions(Guid userID,Guid storeID, string email, string Permission);
        void CloseStore(Guid userID,Guid storeID);
        void GetDetailsOnStore(Guid userID,Guid storeID);
        List<PromotedMember> GetEmployeeInfoInStore(Guid userID, Guid storeID);
        void UpdateFirst(Guid userID, string newFirst);
        void UpdateLast(Guid userID, string newLast);
        void UpdatePassword(Guid userID, string newPassword);
        bool InitializeTradingSystem(Guid userID);
        void CleanUp();
        ConcurrentDictionary<Guid, User> GetCurrent_Users();
        ConcurrentDictionary<Guid, Member> GetMembers();
        bool hasPermissions(Guid userId, Guid storeID, List<string> per);
        ShoppingCart ShowShoppingCart(Guid userID);
        void SetSecurityQA(Guid userID,string q, string a);
        void SetPaymentService(IPaymentService paymentService);
        bool PlacePayment(double amount, string transactionDetails);
        void SetSupplierService(ISupplierService supplierService);
        bool PlaceSupply(string orderDetails, string userDetails);
        ShoppingCart GetShoppingCartById(Guid userID);
        bool isLoggedIn(Guid userID);
        void SetIsSystemInitialize(bool isInitialize);
        User GetUser(Guid userID);
        Member GetMember(Guid userID);

        List<Order> GetStorePurchases(Guid userId, Guid storeId);
        Dictionary<Guid, List<Order>> GetAllStorePurchases(Guid userId);
    }
}