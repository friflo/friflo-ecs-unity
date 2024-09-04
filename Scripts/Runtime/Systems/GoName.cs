// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public class ReadGoName : QuerySystem<EntityName, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (names, links, _) in Query.Chunks) {
                links.CopyTo(names);
            }
        }
    }
    
    
    public class WriteGoName : QuerySystem<EntityName, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (names, links, _) in Query.Chunks) {
                names.CopyTo(links);
            }
        }
    }
}