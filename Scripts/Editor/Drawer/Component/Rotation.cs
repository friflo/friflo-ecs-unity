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
        private static Rect RotationDrawer(in ComponentContext context)
        {
            var rotation= (Rotation)context.Value;
            var value   = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
            var rect    = EditorGUILayout.BeginVertical(context.Styles.componentStyle);
            var result  = EditorGUILayout.Vector4Field(context.Label, value);
            EditorGUILayout.EndVertical();
            if (result == value)
                return rect;
            rotation = new Rotation(result.x, result.y, result.z, result.w);
            EntityUtils.AddEntityComponentValue(context.Entity, context.Type, rotation);
            return rect;
        }
    }
}