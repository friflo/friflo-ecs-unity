// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    internal partial class ComponentDrawerMap
    {
        private static Rect Scale3Drawer(in ComponentContext context)
        {
            var scale   = (Scale3)context.Value;
            var value   = new Vector3(scale.x, scale.y, scale.z);
            var rect    = EditorGUILayout.BeginVertical(context.Styles.componentStyle);
            var result  = EditorGUILayout.Vector3Field(context.Label, value, GUILayout.MinWidth(40));
            EditorGUILayout.EndVertical();
            if (result == value)
                return rect;
            scale = new Scale3(result.x, result.y, result.z);
            EntityUtils.AddEntityComponentValue(context.Entity, context.Type, scale);
            return rect;
        }
    }
}