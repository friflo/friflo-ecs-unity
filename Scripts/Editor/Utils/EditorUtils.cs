// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Reflection;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal static class EditorUtils
    {
        private static EditorWindow _inspectorWindow;
        
        internal static EditorWindow GetInspectorWindow()
        {
            if (_inspectorWindow != null) {
                return _inspectorWindow;
            }
            var assembly    = typeof(Editor).Assembly;
            var type        = assembly.GetType("UnityEditor.InspectorWindow");
            return _inspectorWindow = EditorWindow.GetWindow(type);
        }
        
        /// <summary>
        /// Use this instead of <c>EditorUtility.SetDirty(detector.gameObject);</c> <br/>
        /// SetDirty() will modify the Scene without Redo functionality.  
        /// </summary>
        internal static void RepaintInspector()
        {
            var inspector= GetInspectorWindow();
            inspector.Repaint();
        }
        
        internal static readonly UnityEngine.UIElements.ScrollView InspectorScrollView = GetInspectorScrollView();
        
        private static UnityEngine.UIElements.ScrollView GetInspectorScrollView()
        {
            var inspector       = GetInspectorWindow(); 
            var scrollViewInfo  = inspector.GetType().GetField("m_ScrollView", BindingFlags.NonPublic | BindingFlags.Instance);
            if (scrollViewInfo == null) {
                return null;
            }
            return (UnityEngine.UIElements.ScrollView)scrollViewInfo.GetValue(inspector);
        } 
    }
}
