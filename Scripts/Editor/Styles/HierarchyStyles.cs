// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class HierarchyStyles
    {
        internal readonly   GUIStyle    idLabel;
        internal readonly   GUIStyle    idLabelSelected;

        
        internal HierarchyStyles()
        {
            idLabel = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.UpperRight,
                normal = {
                    // background = GUIUtils.CreateTexture(Color.white),
                    // textColor = Color.black
                },
                padding = new RectOffset(2, 2, -1, -1)
            };
            idLabelSelected = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.UpperRight,
                normal = {
                    // background = GUIUtils.CreateTexture(Color.white),
                    textColor = Color.white
                },
                padding = new RectOffset(2, 2, -1, -1)
            };
        }
    }

}