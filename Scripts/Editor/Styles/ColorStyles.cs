// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;
using static UnityEditor.EditorGUIUtility;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
internal static class ColorStyles
{
    internal static readonly Color32 Green          = isProSkin ? new Color32(  0, 255,   0, 255) : new Color32(230, 255, 230, 255);
    internal static readonly Color32 ComponentBg    = isProSkin ? new Color32( 72,  72,  72, 255) : new Color32(184, 184, 184, 255);
    internal static readonly Color32 Bg             = isProSkin ? new Color32( 56,  56,  56, 255) : new Color32(200, 200, 200, 255);
    internal static readonly Color32 QueryMatch     = isProSkin ? new Color32(200, 200, 200, 255) : new Color32( 80,  80,  80, 255);
    internal static readonly Color32 SeparatorColor = isProSkin ? new Color32(  0,   0,   0,  50) : new Color32(  0,   0,   0,  16);
    internal static readonly Color32 DatedSelected  = isProSkin ? new Color32(150, 150, 150, 255) : new Color32(200, 200, 200, 255);
    internal static readonly Color32 DatedUnselected= isProSkin ? new Color32(130, 130, 130, 255) : new Color32(100, 100, 100, 255);
    
}
}