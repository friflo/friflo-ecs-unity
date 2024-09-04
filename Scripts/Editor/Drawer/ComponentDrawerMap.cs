// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using Friflo.Json.Fliox.Mapper.Map;
using UnityEditor;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {
    
    internal partial class ComponentDrawerMap 
    {
        private             bool            expanded;
        private  readonly   PropField[]     fields;
        private  readonly   ComponentType   componentType;
        
        private static readonly Dictionary<Type, GuiEntityComponent> Map;

        static ComponentDrawerMap()
        {
            Map = CreateComponentDrawerMap();
            foreach (var pair in CustomizationRegistry.CustomGuiEntityComponentMap) {
                Map[pair.Key] = pair.Value;
            }
        } 
        
        internal ComponentDrawerMap(TypeMapper classMapper, ComponentType componentType)
        {
            this.componentType  = componentType;
            var propFields      = classMapper.PropFields.fields;
            int fieldCount      = propFields.Length;
            fields              = new PropField[fieldCount];
            for (int n = 0; n < fieldCount; n++) {
                fields[n] = propFields[n];
            }
            expanded            = false;
        }
        
        private static void DrawComponentSymbol(Rect rect, SchemaType type, SymbolDrawer symbolDrawer)
        {
            if (!Settings.Instance.inspectorComponents) return;
            
            float width = 0;
            rect.x = EditorGUIUtility.labelWidth + 10;
            rect.y += 4.5f;
            symbolDrawer.DrawSymbol(ref rect, type, ref width);
        }
        
        internal void AddComponent(Entity entity, object component, GuiStyles styles, SymbolDrawer symbolDrawer)
        {
            Rect rect;
            if (Map.TryGetValue(componentType.Type, out var entityComponent)) {
                var context = new ComponentContext(entity, componentType, component, styles);
                rect = entityComponent.drawer(context);
                DrawComponentSymbol(rect, componentType, symbolDrawer);
                return;
            }
            if (fields.Length == 1) {
                var f = fields[0];
                rect = EditorGUILayout.BeginVertical(styles.componentStyle);
                DrawComponentSymbol(rect, componentType, symbolDrawer);
                FieldDrawerMap.AddField(entity, componentType, component, f.fieldType.type, f.member, componentType.Name);
                EditorGUILayout.EndVertical();
                return;
            }
            rect = EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical(styles.foldoutStyle);
            if (fields.Length == 0) {
                EditorGUILayout.LabelField(componentType.Name); 
            } else {
                expanded = EditorGUILayout.Foldout(expanded, componentType.Name, true);
            }
            EditorGUILayout.EndVertical();
            DrawComponentSymbol(rect, componentType, symbolDrawer);

            if (expanded) {
                for (int n = 0; n < fields.Length; n++) {
                    var f = fields[n];
                    // EditorGUILayout.LabelField(field.propField.name);
                    FieldDrawerMap.AddField(entity, componentType, component, f.fieldType.type, f.member, f.name);
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        // ------------------------------------ Component Drawer ------------------------------------
        private static Dictionary<Type, GuiEntityComponent> CreateComponentDrawerMap()
        {
            var map = new Dictionary<Type, GuiEntityComponent>
            {
                { typeof(Scale3),           new(Scale3Drawer)         },
                { typeof(Position),         new(PositionDrawer)       },
                { typeof(Rotation),         new(RotationDrawer)       },
                { typeof(RotationEuler),    new(RotationEulerDrawer)  },
                { typeof(GameObjectLink),   new(GameObjectLinkDrawer) },
                { typeof(Unresolved),       new(UnresolvedDrawer)     }
            };
            return map;
        }
    }
}
