// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.Unity;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    internal partial class ComponentDrawerMap
    {
        private static Rect GameObjectLinkDrawer(in ComponentContext context)
        {
            var link    = (GameObjectLink)context.Value;
            var rect    = EditorGUILayout.BeginHorizontal(context.Styles.componentStyle);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(nameof(GameObjectLink), link.gameObject, typeof(GameObject), true);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(28);
            return rect;
        }
    }
}