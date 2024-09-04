// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {
 
    internal class EntityStyles
    {
        internal readonly   GUIStyle            tagStyle;
        internal readonly   GUIStyle            tagStyleSymbols;
        internal readonly   GUIStyle            unresolvedStyle;
        internal readonly   GUIStyle            labelStyle;

        internal readonly   GUILayoutOption[]   idLabel;
        internal readonly   GUILayoutOption[]   filterLabel;
        internal readonly   GUILayoutOption[]   filterText;
        internal readonly   GUILayoutOption[]   addButton;
        internal readonly   GUILayoutOption[]   moreButton;
        
        internal readonly   GUIContent addTags          = new GUIContent ("add", "Add tags");
        internal readonly   GUIContent addComponents    = new GUIContent ("add", "Add components");
        
        internal EntityStyles()
        {
            tagStyleSymbols = new GUIStyle (GUI.skin.button) {
                stretchWidth    = true,
                margin          = new RectOffset(15, 27, 0, 0)
            };
            tagStyle        = new GUIStyle (GUI.skin.button) {
                stretchWidth    = true,
                margin          = new RectOffset(0, 5, 0, 0)
            };
            unresolvedStyle  = new GUIStyle {
                normal          = { background = CreateTexture(new Color(0,0,0,0)) },
                padding         =  new RectOffset(16,0,0,0)
            };
            labelStyle      = new GUIStyle(GUI.skin.label) {
                alignment       = TextAnchor.UpperRight,
                padding         = new RectOffset(2, 2, -1, -1)
            };

            idLabel         = new [] {                      GUILayout.Height(15) };
            filterLabel     = new [] { GUILayout.Width(45) };
            filterText      = new [] { GUILayout.Width(80) };
            addButton       = new [] { GUILayout.Width(35), GUILayout.Height(18) };
            moreButton      = new [] { GUILayout.Width(22), GUILayout.Height(18) };
        }
        
        private static Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}