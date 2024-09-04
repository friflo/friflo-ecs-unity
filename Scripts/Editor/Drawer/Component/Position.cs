// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    internal partial class ComponentDrawerMap
    {
        private static Rect PositionDrawer(in ComponentContext context)
        {
            var pos     = (Position)context.Value;
            var value   = new Vector3(pos.x, pos.y, pos.z);
            var rect    = EditorGUILayout.BeginVertical(context.Styles.componentStyle);
            var result  = EditorGUILayout.Vector3Field(context.Label, value);
            EditorGUILayout.EndVertical();
            if (result == value)
                return rect;
            pos = new Position(result.x, result.y, result.z);
            EntityUtils.AddEntityComponentValue(context.Entity, context.Type, pos);
            return rect;
        }
    }
}