// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Json.Fliox.Mapper;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemGroup
    {
        [Test]
        public static void Test_SystemGroup_MoveTo()
        {
            var baseGroup   = new SystemRoot("base");
            int count = 0;
            var group1      = new SystemGroup("group1");
            var group2      = new SystemGroup("group2");
            var group3      = new SystemGroup("group3");
            baseGroup.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Add - Group 'group1' to 'base'", str);   return;
                    case 1: AreEqual("Add - Group 'group2' to 'base'", str);   return;
                    case 2: AreEqual("Add - Group 'group3' to 'base'", str);   return;
                    case 3: AreEqual("Move - Group 'group1' from 'base' to 'group3'", str);  return;
                    case 4: AreEqual("Move - Group 'group2' from 'base' to 'group3'", str);  return;
                    case 5: AreEqual("Move - Group 'group1' from 'group3' to 'group3'", str);  return; 
                }
            };
            baseGroup.Add(group1);
            baseGroup.Add(group2);
            baseGroup.Add(group3);
            
            AreEqual(0, group1.MoveSystemTo(group3, -1)); // -1  => add at tail
            AreEqual(1, group2.MoveSystemTo(group3,  1));
            AreEqual(1, group1.MoveSystemTo(group3,  2)); // returned index != passed index

            AreEqual(1, baseGroup.ChildSystems.Count);
            AreEqual(2, group3.ChildSystems.Count);
            
            AreEqual(6, count);
        }
        
        [Test]
        public static void Test_SystemGroup_RemoveSystem()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var root        = new SystemRoot (store, "base");
            var testSystem1 = new TestSystem1();
            var count = 0;
            root.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) { 
                    case 0: AreEqual("Add - System 'TestSystem1' to 'base'",        str);   return;
                    case 1: AreEqual("Remove - System 'TestSystem1' from 'base'",    str);   return;
                }
            };
            root.Add(testSystem1);
            AreEqual(1, testSystem1.Queries.Count);
            AreEqual(1, root.ChildSystems.Count);
            
            AreEqual(1, count);
        }
        
        [Test]
        public static void Test_SystemGroup_MoveTo_exception()
        {
            var baseGroup   = new SystemGroup("base");
            var group1      = new SystemGroup("group1");
            baseGroup.Add(group1);
            Exception e = Throws<ArgumentException>(() => {
                group1.MoveSystemTo(baseGroup, -2);
            });
            AreEqual("invalid index: -2", e!.Message);
            
            e = Throws<ArgumentException>(() => {
                group1.MoveSystemTo(baseGroup, 2);
            });
            AreEqual("invalid index: 2", e!.Message);
            
            var group2      = new SystemGroup("group2");
            e = Throws<InvalidOperationException>(() => {
                group2.MoveSystemTo(baseGroup, 1);
            });
            AreEqual("System 'group2' has no parent", e!.Message);
        }
        
        [Test]
        public static void Test_System_serialize()
        {
            var typeStore = new TypeStore();
            var positionMapper = typeStore.GetTypeMapper<PositionSystem>();
            NotNull(positionMapper);
            
            var mapper = new ObjectMapper(typeStore);
            var positionSystem = new PositionSystem { x = 2 };
            var json = mapper.Write(positionSystem);
            AreEqual("{\"id\":0,\"enabled\":true}", json);
        }
    }
    
    public class PositionSystem : QuerySystem<Position>
    {
        internal float x = 1;
        
        protected override void OnUpdate() {
            Query.ForEachEntity((ref Position position, Entity _) => {
                position.x += x;
            });
        }
    }
}