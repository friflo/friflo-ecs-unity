// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;

#if UNITY_EDITOR

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    internal static class GUIUtils
    {
        internal static bool Button (string name, bool enabled = true, Color? color = null)
        {
            var  oldEnabled = GUI.enabled;
            GUI.enabled     = enabled;
            bool result;
            if (color.HasValue) {
                var oldBackground   = GUI.backgroundColor;
                GUI.backgroundColor = color.Value;
                result              = GUILayout.Button(name);
                GUI.backgroundColor = oldBackground;
            } else {
                result  = GUILayout.Button(name);
            }
            GUI.enabled         = oldEnabled;
            return result;
        }
    }
}

#endif
