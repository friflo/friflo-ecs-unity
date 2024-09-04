// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    // Used to synchronize Transform.rotation in Editor
    internal class SyncEditorRotationEuler : QuerySystem<RotationEuler, GameObjectLink>
    {
        internal SyncEditorRotationEuler() => Filter.WithDisabled();
        
        protected override void OnUpdate()
        {
            foreach (var (rotations, links, _) in Query.Chunks) {
                links.CopyTo(rotations);
            }
        }
    }
}
