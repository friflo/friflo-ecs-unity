// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    [CustomEditor(typeof(ECSSystemSet))]
    // ReSharper disable once InconsistentNaming
    internal sealed class ECSSystemSetEditor : Editor
    {
        private             SerializedProperty  ecsStores;
        private             SystemRoot          currentRoot;
        
        internal static     SystemStyles        systemStyles;
        private  static     bool                _expandStores;

                         private SystemSetTree  systemSetTree; // TreeView is not serializable, so it should be reconstructed from the tree data.
        
        private void OnEnable()
        {
            // Check whether there is already a serialized view state (state 
            // that survived assembly reloading)

            var systemSet = (ECSSystemSet)target;
            // TreeViewState is not serialized - instead attached to its ECSSystems component.
            var newTreeViewState = systemSet.treeViewState == null;
            systemSet.treeViewState ??= new TreeViewState();
            systemSet.Init();
            systemSetTree  = new SystemSetTree(systemSet.treeViewState, systemSet);
            if (newTreeViewState) {
                systemSetTree.SetExpanded(new[]{ 0 }); // Always expand Systems item
            }
            ecsStores       = serializedObject.FindProperty(nameof(ECSSystemSet.ecsStores));
        }
        
        public override void OnInspectorGUI()
        {
            var systemSet = (ECSSystemSet)target;
            systemStyles ??= new SystemStyles();
            serializedObject.Update();
            
            var storesLabel = $"Stores   {systemSet.ecsStores.Count}";
            
            EditorGUILayout.BeginHorizontal();
            _expandStores = EditorGUILayout.Foldout(_expandStores, storesLabel, true);
            
            if (GUILayout.Button("+", systemStyles.addStore, GUILayout.Width(20), GUILayout.Height(18))) {
                ecsStores.InsertArrayElementAtIndex(ecsStores.arraySize);
            }
            EditorGUILayout.EndHorizontal();
            if (_expandStores) {
                DrawStores<ECSStore>(ecsStores);
            }
            var separator = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(new Rect(separator.x - 20, separator.y + 2, separator.width + 40, 1), ColorStyles.SeparatorColor);
            EditorGUILayout.EndHorizontal();
            
            // --- Systems TreeView
            var tree = systemSetTree;
            if (currentRoot != systemSet.groupRoot) {
                // Debug.Log("systemSet.root changed");
                currentRoot = systemSet.groupRoot;
                currentRoot.OnSystemChanged += tree.OnSystemChanged;
                tree.Reload(); // set tree state on Undo/Redo
            }
            var rect            = GUILayoutUtility.GetLastRect();
            var startY          = rect.y + rect.height + 5;
            var treeWidth       = EditorGUIUtility.currentViewWidth - 3;
            var treeHeight      = tree.totalHeight;
            Rect treeViewRect   = EditorGUILayout.GetControlRect(false, startY + treeHeight);
            tree.treeWidth      = treeWidth;
            tree.showPerf       = Application.isPlaying;
            tree.multiColumnHeader.GetColumn(0).width = treeWidth - 50;
            tree.treeViewScreenRect = GUIUtility.GUIToScreenRect(treeViewRect);
            FocusSystem(systemSet);
            tree.OnGUI(new Rect(3, startY, treeWidth - 3, treeHeight));
            
            serializedObject.ApplyModifiedProperties();
            RepaintOnPlay(systemSet);
        }

        private void FocusSystem(ECSSystemSet systemSet)
        {
            var tree = systemSetTree;
            if (systemSet.treeViewFocusId != -1) {
                tree.SetFocus();
                if (tree.HasFocus()) {
                    tree.state.selectedIDs.Clear();
                    tree.state.selectedIDs.Add(systemSet.treeViewFocusId);
                    tree.SetSelection(tree.state.selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
                    systemSet.treeViewFocusId = -1;
                }
                return;
            }
            if (!tree.HasFocus()) {
                tree.state.selectedIDs.Remove(0);
            }
        }
        

        
        // Unity, really??? Why not even more complicated 🙈 What a BS!
        // Some much crap code only to draw a generic List<> in the GUI.
        // No type safety at all. You could also use object as an interface everywhere.
        private static void DrawStores<T>(SerializedProperty list)
        {
            var itemType = typeof(T);
            for (int i = 0; i < list.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var item      = list.GetArrayElementAtIndex(i);
                item.objectReferenceValue = EditorGUILayout.ObjectField("Store", item.objectReferenceValue, itemType, true);
 
                if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(20))) {
                    list.DeleteArrayElementAtIndex(i);
                    i -= 1;
                }
                EditorGUILayout.EndHorizontal();
            }
            /*
            EditorGUILayout.BeginHorizontal();
            var newItem = EditorGUILayout.ObjectField(null, itemType, true);
            if (newItem != null) {
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = newItem;
            }
            EditorGUILayout.EndHorizontal(); */
        }
        
    #region repaint on play
        private bool repaintPending;
        
        private void RepaintOnPlay(MonoBehaviour systemSet)
        {
            var isPlaying = EditorApplication.isPlaying && !EditorApplication.isPaused;
            if (repaintPending || !isPlaying) {
                return;
            }
            repaintPending = true;
            systemSet.StartCoroutine(DelayedRepaint());
        }

        private IEnumerator DelayedRepaint()
        {
            yield return new WaitForSeconds(0.5f);
            repaintPending = false;
            systemSetTree.Repaint();
        }
        #endregion
    }
}



















