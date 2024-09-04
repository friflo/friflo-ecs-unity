// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class SystemStyles
    {
        internal readonly     GUIStyle            labelUnselected;
        internal readonly     GUIStyle            labelSelected;
        
        internal readonly     GUIStyle            labelBoldUnselected;
        internal readonly     GUIStyle            labelBoldSelected;
        
        internal readonly     GUIStyle            countUnselected;
        internal readonly     GUIStyle            countSelected;
        
        internal readonly     GUIStyle            datedUnselected;
        internal readonly     GUIStyle            datedSelected;
        
        internal readonly     GUIStyle            addStore;
        
        internal SystemStyles()
        {
            // label
            labelUnselected    = new GUIStyle(GUI.skin.label);
            labelUnselected.alignment = TextAnchor.MiddleLeft;
            labelSelected      = new GUIStyle(GUI.skin.label);
            labelSelected.normal.textColor = Color.white;
            labelSelected.hover. textColor = Color.white;
            labelSelected.alignment = TextAnchor.MiddleLeft;
            
            // label bold
            labelBoldUnselected    = new GUIStyle(GUI.skin.label);
            labelBoldUnselected.alignment = TextAnchor.MiddleLeft;
            labelBoldUnselected.fontStyle = FontStyle.Bold;
            labelBoldSelected      = new GUIStyle(GUI.skin.label);
            labelBoldSelected.normal.textColor = Color.white;
            labelBoldSelected.hover. textColor = Color.white;
            labelBoldSelected.alignment = TextAnchor.MiddleLeft;
            labelBoldSelected.fontStyle = FontStyle.Bold;
            
            // count
            countUnselected    = new GUIStyle(GUI.skin.label);
            countUnselected.alignment = TextAnchor.UpperRight;
            countSelected      = new GUIStyle(GUI.skin.label);
            countSelected.alignment = TextAnchor.UpperRight;
            countSelected.normal.textColor = Color.white;
            countSelected.hover. textColor = Color.white;
            
            // dated - for outdated perf timings
            datedUnselected    = new GUIStyle(GUI.skin.label);
            datedUnselected.alignment = TextAnchor.MiddleLeft;
            datedUnselected.normal.textColor = ColorStyles.DatedUnselected;
            datedUnselected.hover. textColor = ColorStyles.DatedUnselected;
            datedSelected      = new GUIStyle(GUI.skin.label);
            datedSelected.alignment = TextAnchor.MiddleLeft;
            datedSelected.normal.textColor = ColorStyles.DatedSelected;
            datedSelected.hover. textColor = ColorStyles.DatedSelected;
            
            // add store + button
            addStore = new GUIStyle(GUI.skin.button);
            addStore.margin  = new RectOffset(0,0,0,0);
            addStore.padding = new RectOffset(0,0,0,0);
        }
    }
}