// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using UnityEditor;
using UnityEngine;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class EntityDrawer
    {
        internal static readonly    EntityDrawer        Instance    = new EntityDrawer();
        private  static readonly    TypeStore           TypeStore   = new TypeStore();
        private  static readonly    EntitySchema        Schema      = EntityStore.GetEntitySchema();
        
        internal static bool expandTags         = false;
        internal static bool expandComponents   = true;
        
        
        private readonly Dictionary<Type, ComponentDrawerMap> components = new Dictionary<Type, ComponentDrawerMap>();

        internal ComponentDrawerMap GetDrawer(Type type)
        {
            if (!components.TryGetValue(type, out var drawer)) {
                var classMapper     = TypeStore.GetTypeMapper(type);
                var componentType   = Schema.ComponentTypeByType[type];
                drawer              = new ComponentDrawerMap(classMapper, componentType);
                components.Add(type, drawer);
            }
            return drawer;
        }
        
        internal static Rect DrawTagHeader(Entity entity, ECSEntityEditor editor)
        {
            var rect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(new Rect(rect.x - 20, rect.y - 2, rect.width + 40, 1), ColorStyles.SeparatorColor);
            expandTags = EditorGUILayout.Foldout(expandTags, $"tags   {entity.Tags.Count}", true);

            var oldBackground   = GUI.backgroundColor;
            GUI.backgroundColor = ColorStyles.Green;
            var addContent      = ECSEntityEditor.styles.addTags;
            var add             = GUILayout.Button(addContent, ECSEntityEditor.styles.addButton);
            GUI.backgroundColor = oldBackground;
            EditorGUILayout.EndHorizontal(); 
            EditorGUILayout.Space(4); 
            if (add) {
                var root = PopupUtils.FindRootEditor(editor.imguiContainer);
                var content = SelectComponent.Create("Tags", SchemaTypeKind.Tag, entity, ECSEntityEditor.SymbolDrawer);
                content.close = PopupUtils.ShowPopup(root, content);
                content.SetFocus();
            }
            return rect;
        }
        

        
        internal static Rect DrawComponentHeader(Entity entity, ECSEntityEditor editor)
        {
            EditorGUILayout.Space(4);

            var rect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(new Rect(rect.x - 20, rect.y - 2, rect.width + 40, 1), ColorStyles.SeparatorColor);
            expandComponents = EditorGUILayout.Foldout(expandComponents, $"components   {entity.Components.Count}", true);

            var oldBackground   = GUI.backgroundColor;
            GUI.backgroundColor = ColorStyles.Green;
            var addContent      = ECSEntityEditor.styles.addComponents;
            var add             = GUILayout.Button(addContent, ECSEntityEditor.styles.addButton);
            GUI.backgroundColor = oldBackground;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
            if (add) {
                var root = PopupUtils.FindRootEditor(editor.imguiContainer);
                var content = SelectComponent.Create("Components", SchemaTypeKind.Component, entity, ECSEntityEditor.SymbolDrawer);
                content.close = PopupUtils.ShowPopup(root, content);
                content.SetFocus();
            }
            return rect;
        }
    }
}