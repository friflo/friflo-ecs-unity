// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Engine.Unity;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using UnityEngine;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    // ReSharper disable once InconsistentNaming
    internal class ECSEntitySerializer
    {
        internal static readonly ECSEntitySerializer    Instance = new ECSEntitySerializer();
        private  static          short                  _version;
        
        private  readonly   EntityConverter    entityConverter     = new EntityConverter();
        private  readonly   DataEntity         dataEntity          = new DataEntity { tags = new List<string>() };
        private  readonly   List<JsonValue>    componentMembers    = new List<JsonValue>();
        private             Bytes              jsonBuffer          = new Bytes(16);

        
        internal void ReadLinkComponentsFromEntity(ECSEntity ecsEntity, Entity entity)
        {
            // Debug.Log($"ReadLinkComponentsFromEntity: {change}");
            entityConverter.EntityComponentsToJsonMembers(entity, componentMembers);

            ecsEntity.components   = MembersToStringArray(componentMembers, entity);
            ecsEntity.version      = ++_version;
            ecsEntity.versionNew   = _version;
        }
        
        internal void WriteLinkComponentsToEntity(ECSEntity ecsEntity)
        {
            var entityStore = ecsEntity.storeContext.EntityStore;
            dataEntity.pid  = ecsEntity.EntityId;
            var entity = entityStore.GetEntityById(ecsEntity.EntityId);
            ComponentsToDataEntity(ecsEntity.components, dataEntity);
            ecsEntity.versionNew   = ecsEntity.version;
            var linkType            = ComponentTypes.Get<GameObjectLink>();
            entityConverter.DataEntityToEntityPreserve(dataEntity, entityStore, out var error, linkType, default);
            if (error != null) {
                Debug.LogError(error);
            }
            entity.AddComponent(new GameObjectLink(ecsEntity.gameObject));
        }
        
        private static string[] MembersToStringArray(List<JsonValue> members, Entity entity)
        {
            var result = new string[members.Count + entity.Tags.Count];
            int n = 0;
            for (; n < members.Count; n++) {
                result[n] = members[n].ToString();
            }
            foreach (var tag in entity.Tags) {
                result[n++] = tag.TagName;
            }
            return result;
        }
        
        private void ComponentsToDataEntity(string[] components, DataEntity dataEntity)
        {
            var tags = dataEntity.tags;
            tags.Clear();
            dataEntity.components = default;
            if (components == null) {
                return;
            }
            jsonBuffer.Clear();
            jsonBuffer.AppendChar('{');
            bool isFirst = true;
            
            foreach (var component in components)
            {
                // Is key/value component?
                if (component.Length > 0 && component[0] == '"') { 
                    if (isFirst) {
                        isFirst = false;
                    } else {
                        jsonBuffer.AppendChar(',');   
                    }
                    jsonBuffer.AppendString(component);
                    continue;
                }
                tags.Add(component);
            }
            jsonBuffer.AppendChar('}');
            dataEntity.components = new JsonValue(jsonBuffer);
        }
    }
}