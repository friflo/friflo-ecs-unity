// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public class ReadGoRotation : QuerySystem<RotationEuler, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (rotation, links, _) in Query.Chunks) {
                links.CopyTo(rotation);
            }
        }
    }
    
    
    public class WriteGoRotation : QuerySystem<RotationEuler, GameObjectLink>
    {
        protected override void OnUpdate()
        {
            foreach (var (rotation, links, _) in Query.Chunks) {
                rotation.CopyTo(links);
            }
        }
    }
}