// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public class ReadGoScale : QuerySystem<Scale3, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (scales, links, _) in Query.Chunks) {
                links.CopyTo(scales);
            }
        }
    }
    
    
    public class WriteGoScale : QuerySystem<Scale3, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (scales, links, _) in Query.Chunks) {
                scales.CopyTo(links);
            }
        }
    }
}