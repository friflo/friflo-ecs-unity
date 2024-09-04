// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
#region Read
    public class ReadGoDisabled : QuerySystem<GameObjectLink>
    {
        private readonly List<Entity> enable = new ();
        
        public ReadGoDisabled() => Filter.WithDisabled().AllTags(Tags.Get<Disabled>());
        
        protected override void OnUpdate()
        {
            foreach (var (links, _) in Query.Chunks)
            {
                foreach (var link in links.Span) {
                    if (!link.gameObject.activeSelf) {
                        continue;
                    }
                    enable.Add(link.EcsEntity.Entity);
                }
            }
        }

        protected override void OnUpdateGroupBegin() => enable.Clear();

        protected override void OnUpdateGroupEnd() {
            foreach (var entity in enable) {
                entity.RemoveTag<Disabled>();
            }
        }
    }
    
    // Used to synchronize GameObject.activeSelf == false in Editor
    internal class ReadGoEnabled : QuerySystem<GameObjectLink>
    {
        private readonly List<Entity> disable = new ();
        
        protected override void OnUpdate()
        {
            foreach (var (links, _) in Query.Chunks)
            {
                foreach (var link in links.Span) {
                    if (link.gameObject.activeSelf) {
                        continue;
                    }
                    disable.Add(link.EcsEntity.Entity);
                }
            }
        }
        
        protected override void OnUpdateGroupBegin() => disable.Clear();
        
        protected override void OnUpdateGroupEnd()
        {
            foreach (var entity in disable) {
                entity.AddTag<Disabled>();
            }
        }
    }
    #endregion
    
#region Write
    public class WriteGoDisabled : QuerySystem<GameObjectLink>
    {
        public WriteGoDisabled() => Filter.WithDisabled().AllTags(Tags.Get<Disabled>());
        
        protected override void OnUpdate()
        {
            foreach (var (links, _) in Query.Chunks)
            {
                foreach (var link in links.Span) {
                    link.gameObject.SetActive(false);
                }
            }
        }
    }
    
    public class WriteGoEnabled : QuerySystem<GameObjectLink>
    {
        public WriteGoEnabled() => Filter.WithoutAllTags(Tags.Get<Disabled>());
        
        protected override void OnUpdate()
        {
            foreach (var (links, _) in Query.Chunks)
            {
                foreach (var link in links.Span) {
                    link.gameObject.SetActive(true);
                }
            }
        }
    }
    #endregion
}