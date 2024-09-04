// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class SymbolDrawer
    {
        private readonly    Dictionary<SchemaType, SymbolStyle> symbolStyles = new Dictionary<SchemaType, SymbolStyle>();
        private readonly    List<SchemaType>                    typeList = new ();
        private             SymbolRowStyles                     styles;
        
    #region symbol
        internal void DrawSymbol(ref Rect rect, SchemaType type, ref float width)
        {
            if (!symbolStyles.TryGetValue(type, out var style)) {
                symbolStyles.Add(type, style = new SymbolStyle(type)); 
            }
            var symbolWidth = style.width;
            width      += symbolWidth;
            rect.width  = symbolWidth;
            rect.x     -= symbolWidth;
            var bg = EditorGUIUtility.isProSkin ? Color.black : Color.white; 
            EditorGUI.DrawRect(new Rect(rect.x,        rect.y,          rect.width,     11.5f), style.background);
            EditorGUI.DrawRect(new Rect(rect.x,        rect.y + 11f,    rect.width,     3.0f),  bg);
            GUI.Label(         new Rect(rect.x,        rect.y - 0.5f,   rect.width,     rect.height), style.character, style.guiStyle);
            // EditorGUI.DrawRect(new Rect(rect.x,        rect.y + 9.5f, rect.width,     3f), Color.white);
            EditorGUI.DrawRect(new Rect(rect.x + 0.5f, rect.y + 11.5f,  rect.width - 1, 2.0f),  style.color);
        }
        
        private static void DrawEllipsis(ref Rect rect, GUIStyle labelStyle)
        {
            var ellipsisWidth = labelStyle.CalcSize(new GUIContent( "..." )).x;
            
            rect.width  = ellipsisWidth;
            rect.x     -= ellipsisWidth;
            GUI.Label(rect, "...", labelStyle);
        }
        #endregion
        
        
    #region symbol row
        internal void DrawComponents(ref Rect rect, in ComponentTypes types, ref float width, GUIStyle labelStyle, float minX)
        {
            styles ??= new SymbolRowStyles();
            
            var count = 0;
            typeList.Clear();
            foreach (var component in types) typeList.Add(component);
            typeList.Reverse();
            
            foreach (var type in typeList) {
                if (rect.x < minX) {
                    DrawEllipsis(ref rect, labelStyle);
                    return;
                } 
                count++;
                DrawSymbol(ref rect, type, ref width);
            }
            if (count == 0) return;
            width  += 8;
            rect.x -= 8;
        }
        
        internal void DrawTags(ref Rect rect, in Tags tags, ref float width, GUIStyle labelStyle, float minX)
        {
            styles ??= new SymbolRowStyles();
            
            if (tags.Count == 0) return;
            typeList.Clear();
            foreach (var tag in tags) typeList.Add(tag);
            typeList.Reverse();
            
            foreach (var type in typeList) {
                if (rect.x < minX) {
                    DrawEllipsis(ref rect, labelStyle);
                    return;
                }
                DrawSymbol(ref rect, type, ref width);
            }
            if (tags.Count > 0) {
                var   w = styles.tagsGroupWidth + 2;
                width  += w;
                rect.x -= w;
                var bg  = EditorGUIUtility.isProSkin ? SymbolStyle.DarkBg : SymbolStyle.LightBg;
                EditorGUI.DrawRect(new Rect(rect.x,     rect.y + 2.0f, w, 8f), bg);
                GUI.Label         (new Rect(rect.x + 1, rect.y + 3.0f, w, 8f), "#", styles.tagsGroupLabel);
            }
        }
        #endregion
    }
}