// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public class ReadGoPosition : QuerySystem<Position, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (positions, links, _) in Query.Chunks) {
                links.CopyTo(positions);
            }
        }
    }
    
    
    public class WriteGoPosition : QuerySystem<Position, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (positions, links, _) in Query.Chunks) {
                positions.CopyTo(links);
            }
        }
    }
}