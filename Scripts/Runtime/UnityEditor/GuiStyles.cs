// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    public class GuiStyles
    {
        internal static     GuiStyles   instance;
        
        public   readonly   GUIStyle    componentStyle;
        public   readonly   GUIStyle    foldoutStyle;
        
        /// <summary>
        /// <see cref="UnityEngine.UIElements.IMGUIContainer"/> is displaced by 15 pixels for some reason.
        /// This offset is used to compensate this.
        /// </summary>
        internal const int IMGUIContainerOffset = 13; 

        
        private static  Texture2D _componentBackground;
        
        /// <summary>
        /// Texture2D gets disposed arbitrary. E.g. when switching from Play-> Edit Mode. Recreate Texture2D in this case.
        /// </summary>
        internal void SetBackgrounds()
        {
            bool exists = (bool) (Object) _componentBackground;
            if (exists) {
                return;
            }
            var componentColor  = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.08f) : new Color(0, 0, 0, 0.08f);
            _componentBackground = CreateTexture(componentColor);
            componentStyle.normal.background = _componentBackground;
            foldoutStyle.  normal.background = _componentBackground;
        }
        
        internal GuiStyles()
        {
            // componentColor = EditorGUIUtility.isProSkin ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.74f, 0.74f, 0.74f);
            int offset = IMGUIContainerOffset;
            componentStyle  = new GUIStyle {
                padding         =  new RectOffset(0,0,0,0),
                margin          =  new RectOffset(offset,0,0,0)
            };
            foldoutStyle    = new GUIStyle {
                padding         =  new RectOffset(0,2,2,2),
                margin          =  new RectOffset(offset,0,0,0)
            };

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

#endif
