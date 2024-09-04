// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    // Used to synchronize Transform.scale in Editor
    internal class SyncEditorScale : QuerySystem<Scale3, GameObjectLink>
    {
        internal SyncEditorScale() => Filter.WithDisabled();
        
        protected override void OnUpdate()
        {
            foreach (var (scales, links, _) in Query.Chunks) {
                links.CopyTo(scales);
            }
        }
    }
}
