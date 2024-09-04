// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    /// <summary>
    /// Used to detect destroyed <see cref="GameObject"/>'s.
    /// Is required as MonoBehaviour.OnDestroy() is only called for active GameObject's
    /// </summary>
    /// <remarks>
    /// Workflow in Editor
    /// - set entity object disabled
    /// - duplicate disabled object
    /// - Undo duplicate
    /// Finished
    /// - To check call of ECSEntity.Awake() Redo duplicate
    /// </remarks>
    internal class SyncEditorDestroyed : QuerySystem<GameObjectLink>
    {
        private readonly EntityList     delete = new();
        private readonly StoreContext   context;
        
        internal SyncEditorDestroyed(StoreContext context)
        {
            this.context = context;
            Filter.WithDisabled();
        }
        
        protected override void OnUpdate()
        {
            delete.Clear();
            delete.SetStore(Query.Store);
            foreach (var (links, entities) in Query.Chunks)
            {
                var linkSpan = links.Span;
                for (int n = 0; n < entities.Length; n++) {
                    if (linkSpan[n].gameObject != null) continue;
                    delete.Add(entities[n]);   
                }
            }
            foreach (var entity in delete) {
                context.UnityDeleteEntity(entity);
            }
        }
        
        private static void Update(ref Position pos, ref GameObjectLink link, int id, EventFilter filter)
        {
            /* if (filter.HasEvent(id)) {
                link.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
                return;
            } */
            pos.value.Set(link.transform.localPosition);
        }
    }
}
