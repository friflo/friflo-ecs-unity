// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using Position = Friflo.Engine.ECS.Position;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class SelectComponentItem : VisualElement
    {
        private readonly    Label           label;
        private readonly    Toggle          toggle;
        private readonly    SymbolDrawer    symbolDrawer;
        private             IComponent      componentValue;
        private             Entity          entity;
        private             SchemaType      schemaType;
        
        internal            bool            Enabled => toggle.value;
        
        internal Action<SelectComponentItem, ChangeEvent<bool>> handler;
        
        internal SelectComponentItem(SymbolDrawer symbolDrawer)
        {
            this.symbolDrawer   = symbolDrawer;    
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            label   = new Label();
            label.style.alignSelf = new StyleEnum<Align>(Align.Center);
            toggle  = new Toggle();
            toggle.focusable = false;
            var imgui   = new IMGUIContainer(OnImGui);
            imgui.style.width  = 30;
            imgui.style.height = 20;
            Add(toggle);
            Add(imgui);
            Add(label);
        }
        
        private void OnImGui() {
            var rect = new Rect(30 - 3, 3, 30, 20);
            var width = 0f;
            symbolDrawer.DrawSymbol(ref rect, schemaType, ref width);
        }

        public void Init(Entity entity, SchemaType type)
        {
            this.entity     = entity;
            schemaType      = type;
            label.text      = type.Name;
            var hasComponent= HasComponent();
            toggle.value    = hasComponent;
            componentValue  = GetComponent(hasComponent);
            toggle.RegisterValueChangedCallback(evt => { ModifyComponent(evt.newValue); });
        }
        
        private bool HasComponent()
        {
            if (schemaType is TagType tagType) {
                return entity.Tags.HasAll(new Tags(tagType));
            }
            if (schemaType is ComponentType componentType) {
                var type = new ComponentTypes(componentType);
                return entity.Archetype.ComponentTypes.HasAll(type);
            }
            throw new InvalidOperationException();
        }
        
        internal void ModifyComponent(bool add)
        {
            if (HasComponent() == add) return;
            // Debug.Log($"ModifyComponent - type: {schemaType}  value: {add}");
            if (schemaType is TagType tagType) {
                if (add) {
                    entity.AddTags   (new Tags(tagType));
                } else {
                    entity.RemoveTags(new Tags(tagType));
                }
            }
            if (schemaType is ComponentType componentType) {
                if (add) {
                    AddComponent(componentType);
                } else {
                    EntityUtils.RemoveEntityComponent(entity, componentType);
                }
            }
            toggle.value = add;
        }
        
        private void AddComponent(ComponentType componentType)
        {
            if (componentValue != null) {
                EntityUtils.AddEntityComponentValue   (entity, componentType, componentValue);
                return;
            }
            var type = componentType.Type;
            if (entity.TryGetComponent<GameObjectLink>(out var link))
            {
                if (type == typeof(Position)) {
                    var position = new Position();
                    position.value.Set(link.transform.position);
                    entity.AddComponent(position);    
                    return;
                }
                if (type == typeof(Scale3)) {
                    var scale = new Scale3();
                    scale.value.Set(link.transform.localScale);
                    entity.AddComponent(scale);    
                    return;
                }
                if (type == typeof(RotationEuler)) {
                    var rotation = new RotationEuler();
                    rotation.value.Set(link.transform.localEulerAngles);
                    entity.AddComponent(rotation);
                    return;
                }
                if (type == typeof(EntityName)) {
                    var entityName = new EntityName();
                    entityName.value = link.transform.name;
                    entity.AddComponent(entityName);    
                    return;
                }
            }
            if (type == typeof(Scale3)) {
                entity.AddComponent(new Scale3(1,1,1));
            } else {
                EntityUtils.AddEntityComponent   (entity, componentType);
            }
        }
        
        private IComponent GetComponent(bool hasComponent)
        {
            if (hasComponent == false) {
                return null;
            }
            if (schemaType is ComponentType componentType) {
                return EntityUtils.GetEntityComponent(entity, componentType);
            }
            return null;
        }
    }
}