// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    internal partial class ComponentDrawerMap
    {
        private static bool _expandUnresolved;
        private static bool _expandUnresolvedComponents  = true;
        private static bool _expandUnresolvedTags        = true;
        
        
        private static Rect UnresolvedDrawer(in ComponentContext context)
        {
            var unresolved      = (Unresolved)context.Value;
            var rect = EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical(context.Styles.foldoutStyle);
            _expandUnresolved    = EditorGUILayout.Foldout(_expandUnresolved, nameof(Unresolved), true);
            EditorGUILayout.EndVertical();
            if (!_expandUnresolved) {
                EditorGUILayout.EndVertical();
                return rect;
            }
            var components = unresolved.components;
            if (components?.Length > 0)
            {
                EditorGUILayout.BeginVertical(ECSEntityEditor.styles.unresolvedStyle);
                UnresolvedComponents(components);
                EditorGUILayout.EndVertical();
            }
            var tags = unresolved.tags;
            if (tags?.Length > 0)
            {
                EditorGUILayout.BeginVertical(ECSEntityEditor.styles.unresolvedStyle);
                UnresolvedTags(tags);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            return rect;
        }
        
        private static void UnresolvedComponents(UnresolvedComponent[] components)
        {
            _expandUnresolvedComponents = EditorGUILayout.Foldout(_expandUnresolvedComponents, "components", true);
            if (!_expandUnresolvedComponents) {
                return;
            }
            foreach (var unknown in components) {
                EditorGUILayout.BeginHorizontal();
                // EditorGUILayout.PrefixLabel(unknown.key); 
                EditorGUILayout.SelectableLabel(unknown.key, GUILayout.Height(15), GUILayout.Width(120));
                EditorGUILayout.SelectableLabel(unknown.value.ToString(), GUILayout.Height(15));
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private static void UnresolvedTags(string[] tags)
        {
            _expandUnresolvedTags = EditorGUILayout.Foldout(_expandUnresolvedTags, "tags", true);
            if (!_expandUnresolvedTags) {
                return;
            }
            foreach (var tag in tags) {
                EditorGUILayout.SelectableLabel(tag, GUILayout.Height(15));
            }
        }
    }
}