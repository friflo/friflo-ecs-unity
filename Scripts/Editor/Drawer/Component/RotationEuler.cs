// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    internal partial class ComponentDrawerMap
    {
        private static Rect RotationEulerDrawer(in ComponentContext context)
        {
            var rotation= (RotationEuler)context.Value;
            var value   = new Vector3(rotation.x, rotation.y, rotation.z);
            var rect    = EditorGUILayout.BeginVertical(context.Styles.componentStyle);
            var result  = EditorGUILayout.Vector3Field(context.Label, value);
            EditorGUILayout.EndVertical();
            if (result == value)
                return rect;
            rotation = new RotationEuler(result.x, result.y, result.z);
            EntityUtils.AddEntityComponentValue(context.Entity, context.Type, rotation);
            return rect;
        }
    }
}