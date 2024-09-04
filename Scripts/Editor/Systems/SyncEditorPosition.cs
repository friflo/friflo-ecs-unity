// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    // Used to synchronize Transform.position in Editor
    internal class SyncEditorPosition : QuerySystem<Position, GameObjectLink>
    {
        internal SyncEditorPosition() => Filter.WithDisabled();
        
        protected override void OnUpdate()
        {
            foreach (var (position, links, _) in Query.Chunks) {
                links.CopyTo(position);
            }
        }
    }
}
