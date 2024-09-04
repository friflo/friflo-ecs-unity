// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    // Used to synchronize Transform.name in Editor
    /// <summary>
    /// Transform.name is not synchronized by intention. Reasons:<br/>
    /// - Access Transform.name allocates memory for every call - the string with its char array.<br/>
    /// - Transform.name cannot be "" or null. It will write a log warning if doing so.
    ///   EntityName.value allows "" or null. So names are not in sync in these cases. 
    /// </summary>
    internal class SyncEditorName : QuerySystem<EntityName, GameObjectLink>
    {
        internal SyncEditorName()  => Filter.WithDisabled();
        
        protected override void OnUpdate()
        {
            var filter = Query.EventFilter;
            foreach (var (names, links, entities) in Query.Chunks) {
                for (int n = 0; n < entities.Length; n++) {
                    Update(ref names[n], ref links[n], entities[n], filter);    
                }
            }
        }
        
        private static void Update(ref EntityName name, ref GameObjectLink link, int id, EventFilter filter)
        {
            /* if (filter.HasEvent(id)) {
                link.transform.name = name.value;
                return;
            } */
            name.value = link.transform.name;
        }
    }
}
