﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SadnaExpress.ServiceLayer;
using System;

using SadnaExpressTests;
using static SadnaExpressTests.Mocks;
using SadnaExpress.DomainLayer.User;
using System.Collections.Concurrent;
using SadnaExpress.DomainLayer.Store;

namespace SadnaExpressTests.Acceptance_Tests
{
    [TestClass()]
    public class TradingSystemAT
    {
        protected ProxyBridge proxyBridge;
        protected Guid userid;
        protected Guid mamberid;
        protected Guid systemManagerid;
        protected IPasswordHash passwordHash;
        [TestInitialize]
        public virtual void SetUp()
        {
            proxyBridge = new ProxyBridge();
            passwordHash = new PasswordHash();

            ConcurrentDictionary<Guid, Store> stores=new ConcurrentDictionary<Guid, Store>();
            Store store1 = new Store("Zara");
            store1.AddItem("Tshirt", "clothes", 99.8, 22);
            store1.AddItem("Dress", "clothes", 70, 45);
            Store store2 = new Store("Fox");
            store1.AddItem("Pants", "clothes", 150, 200);
            store1.AddItem("Towel", "Home", 40, 450);
            store1.AddItem("Teddy bear toy", "children toys", 65, 120);
            stores.TryAdd(store1.StoreID, store1);
            stores.TryAdd(store2.StoreID, store2);

            IStoreFacade storeFacade = new StoreFacade(stores);
            ConcurrentDictionary<Guid, User> current_users = new ConcurrentDictionary<Guid, User>();
            User entered_user = new User();
            userid = entered_user.UserId;
            current_users.TryAdd(userid, entered_user);

            ConcurrentDictionary<Guid, Member> members = new ConcurrentDictionary<Guid, Member>();
            systemManagerid = Guid.NewGuid();
            mamberid= Guid.NewGuid();
            Member member = new Member(mamberid, "gil@gmail.com", "Gil", "Gil", passwordHash.Hash("asASD876!@"));
            PromotedMember systemManager = new PromotedMember(systemManagerid, "RotemSela@gmail.com", "noga", "schwartz", passwordHash.Hash("AS87654askj"));
            systemManager.createSystemManager();
            members.TryAdd(systemManagerid, systemManager);
            members.TryAdd(mamberid, member);
            IUserFacade _userFacade = new UserFacade(current_users, members, new PasswordHash(), new Mock_PaymentService(), new Mock_SupplierService());
            TradingSystem Ts=new TradingSystem(_userFacade, storeFacade);
            Ts.TestMode = true;

            proxyBridge.SetBridge(Ts);
            proxyBridge.SetPaymentService(new Mock_PaymentService());
            proxyBridge.SetSupplierService(new Mock_SupplierService());
            proxyBridge.SetIsSystemInitialize(true);
        }


        protected class Mock_Bad_SupplierService : Mock_SupplierService
        {
            // bad connection
            public override bool Connect()
            {
                return false;
            }

        }

        protected class Mock_Bad_PaymentService : Mock_PaymentService
        {
            // bad connection
            public override bool Connect()
            {
                return false;
            }

        }

        [TestCleanup]
        public virtual void CleanUp()
        {
            proxyBridge.CleanUp();
        }

    }
}
