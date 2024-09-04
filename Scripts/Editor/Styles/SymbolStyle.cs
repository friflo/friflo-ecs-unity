// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using System.Text;
using Friflo.Engine.ECS;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{

internal readonly struct SymbolStyle
{
    internal readonly       string      character;
    internal readonly       GUIStyle    guiStyle;
    internal readonly       float       width;
    internal readonly       Color       color;
    internal readonly       Color       background;
    
    private static readonly MD5         MD5 = MD5.Create();
    
    internal static readonly Color32 LightBg = new Color32(255,255,255, 255);
    internal static readonly Color32 DarkBg  = new Color32(90,90,90, 255);
    
    private static Color GetColor(SchemaType schemaType)
    {
        if (schemaType.SymbolColor != null) {
            var c = schemaType.SymbolColor.Value;
            return new Color32(c.r, c.g, c.b, 255);
        }
        var hash    = MD5.ComputeHash(Encoding.UTF8.GetBytes(schemaType.Name));
        float colorH = hash[0] / 255f;
        float colorS = (float)Math.Sqrt(hash[1] / 255f) * 0.7f + 0.3f;
        float colorV = (float)Math.Sqrt(hash[2] / 255f) * 0.7f + 0.3f;
        return Color.HSVToRGB(colorH, colorS, colorV);
    }
    
    internal SymbolStyle (SchemaType schemaType)
    {
        bool isDark = EditorGUIUtility.isProSkin;
        color = GetColor(schemaType);
        
        guiStyle = new GUIStyle(GUI.skin.label) {
            fontStyle   = FontStyle.Bold, 
            fontSize    = 10,
            alignment   = TextAnchor.UpperCenter,
            margin      = new RectOffset(0,0,0,0),
            padding     = new RectOffset(0,0,0,-2) 
        };
        if (schemaType is ComponentType componentType) {
            character   = componentType.SymbolName;
            if (isDark) {
                background = DarkBg;
                guiStyle.normal.textColor = new Color32(255,255,255,255);
            } else {
                background = LightBg;
                guiStyle.normal.textColor = Color.black;
            }
            width = guiStyle.CalcSize(new GUIContent( character )).x;
        }
        else if (schemaType is TagType tagType) {
            character   = tagType.SymbolName;
            if (isDark) {
                background = DarkBg;
                guiStyle.normal.textColor = new Color32(255,255,255,255);
            } else {
                background = LightBg;
                guiStyle.normal.textColor = Color.black;
            }
            width = guiStyle.CalcSize(new GUIContent( character )).x;
        }
        else {
            character   = "-";
            color       = default;
            background  = default;
        }
        width = guiStyle.CalcSize(new GUIContent( character )).x + 2;
        if (width > 100) {
            throw new InvalidOperationException();
        }
    }
}

internal class SymbolRowStyles
{
    internal readonly   GUIStyle    tagsGroupLabel;
    internal readonly   float       tagsGroupWidth;
    
    internal SymbolRowStyles() {
        tagsGroupLabel = new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.UpperLeft,
            normal = {
                textColor = EditorGUIUtility.isProSkin ? new Color32(230,230,230, 255) : new Color32(90,90,90, 255) 
            },
            fontStyle   = FontStyle.Bold,
            fontSize    = 8,
            padding     = new RectOffset(0,0,-2,-2),
        };
        tagsGroupWidth = tagsGroupLabel.CalcSize(new GUIContent( "#" )).x;
    }
}
}