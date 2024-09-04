﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemRoot
    {
        [Test]
        public static void Test_SystemRoot_Add_System_minimal()
        {
            var store   = new EntityStore(PidType.UsePidAsId);
            var entity  = store.CreateEntity(new Position());
            var root    = new SystemRoot(store);
            root.Add(new TestSystem1());
            root.Update(default);
            AreEqual(new Position(1,0,0), entity.Position);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_Group()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var root        = new SystemRoot("root");
            var group1      = new SystemGroup("group1");
            var testGroup   = new TestGroup();
            IsNull(root.FindGroup("group1",     true));
            IsNull(root.FindGroup("TestGroup",  true));
            root.Add(group1);
            root.Add(testGroup);
            
            AreSame(group1,     root.FindGroup("group1", true));
            AreSame(testGroup,  root.FindGroup("TestGroup", true));
            
            AreEqual(2, root.ChildSystems.Count);
            
            root.AddStore(store);
            AreEqual(1, root.Stores.Count);
            
            root.Update(default);
            AreEqual(1, testGroup.beginCalled);
            AreEqual(1, testGroup.endCalled);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_System()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var entity      = store.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot(store);    // create SystemRoot with store
            var group       = new SystemGroup("group");
            root.Add(group);
            var testSystem1 = new TestSystem1();
            AreEqual(0,     testSystem1.Queries.Count);
            group.Add(testSystem1);
            AreEqual(1,     testSystem1.Queries.Count);
            AreEqual(1,     testSystem1.EntityCount);
            AreSame(root,   testSystem1.SystemRoot);
            
            root.Update(default);
            AreEqual(new Scale3(4,5,6), entity.Scale3);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_RemoveStore()
        {
            var store1      = new EntityStore(PidType.UsePidAsId);
            var store2      = new EntityStore(PidType.UsePidAsId);
            store1.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot("root");   // create SystemRoot without store
            var group       = new SystemGroup("group");
            var testSystem1 = new TestSystem1();
            group.Add(testSystem1);
            root.Add(group);
            
            // --- add store
            AreEqual(0, testSystem1.Queries.Count);
            root.AddStore(store1);                      // add store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem1.Queries.Count);
            AreEqual(1, testSystem1.EntityCount);
            root.Update(default);
            
            root.AddStore(store2);                      // add store after system setup
            AreEqual(2, root.Stores.Count);
            AreEqual(2, testSystem1.Queries.Count);
            
            // --- remove store
            root.RemoveStore(store1);                   // remove store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem1.Queries.Count);
            
            root.RemoveStore(store2);                   // remove store after system setup
            AreEqual(0, root.Stores.Count);
            AreEqual(0, testSystem1.Queries.Count); 
        }
        
        [Test]
        public static void Test_System_Name()
        {
            var group = new SystemGroup("TestGroup");
            AreEqual("TestGroup", group.Name);
            
            group.SetName("changed name");
            AreEqual("changed name", group.Name);
            
            var testSystem1 = new TestSystem1();
            AreEqual("TestSystem1", testSystem1.Name);
            
            var mySystem1 = new MySystem1();
            AreEqual("MySystem1", mySystem1.Name);
            
            var mySystem2 = new MySystem2();
            AreEqual("MySystem2 - custom name", mySystem2.Name);
        }
    }
    
    public class TestSystem1 : QuerySystem<Position> {
        protected override void OnUpdate() {
            Query.ForEachEntity((ref Position position, Entity entity) => {
                position.x++;
                CommandBuffer.AddComponent(entity.Id, new Scale3(4,5,6));
            });
        }
    }
    
    public class TestGroup : SystemGroup {
        internal int beginCalled;
        internal int endCalled;
        
        public TestGroup() : base("TestGroup") { }

        protected override void OnUpdateGroupBegin() {
            AreEqual(1, SystemRoot.Stores.Count);
            beginCalled++;
        }

        protected override void OnUpdateGroupEnd() {
            AreEqual(1, SystemRoot.Stores.Count);
            endCalled++;
        }
    }
    
    // Ensure a custom System class can be declared without any overrides
    public class MySystem1 : BaseSystem { }
    
    // A custom System class with all possible overrides
    public class MySystem2 : BaseSystem {
        public      override string Name => "MySystem2 - custom name";
        
        protected   override void   OnUpdateGroupBegin() { }
        protected   override void   OnUpdateGroupEnd()   { }
        protected   override void   OnUpdateGroup()      { }
    }
}