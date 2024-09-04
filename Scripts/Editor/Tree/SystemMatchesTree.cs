// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// ReSharper disable RedundantJumpStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor 
{
    internal class SystemMatchesTree : SystemTree
    {
        private readonly    List<SystemTreeMatch>   matchingSystems = new();
        internal            bool                    groupMatches;        
        
        public SystemMatchesTree(TreeViewState treeViewState) : base (treeViewState) {
            rowHeight = 20;
            // state.selectedIDs.Clear();
            Reload();
        }

        protected override TreeViewItem BuildRoot() {
            var root        = new SystemRow(-1, -1, "Root",  null, null, -1);
            var items       = new List<TreeViewItem>();
            var systemCount = 0;
            foreach (var match in matchingSystems) {
                if (match.system is QuerySystemBase) systemCount++;
            }
            BuildAddItem(items, 0, 0, "matching queries", null, null, systemCount);

            foreach (var match in matchingSystems) {
                if (match.system is QuerySystemBase querySystem) {
                    BuildAddItem(items, match.id, match.depth, querySystem.Name, match.systemSet, querySystem, -1);
                }
                else if (match.system is SystemGroup group) {
                    BuildAddItem(items, match.id, match.depth, group.Name, match.systemSet, group, match.count);
                }
                else {
                    var name = match.systemSet.gameObject.name;
                    BuildAddItem(items, match.id, match.depth,  name, match.systemSet, null, -1);
                }
            }
            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths (root, items);
            
            // Return root of the tree
            return root;
        }
        
        private static void BuildAddItem(List<TreeViewItem> items, int id, int depth, string name, ECSSystemSet systemSet, BaseSystem system, int count)
        {
            name = count >= 0 ? $"{name} [{count}]" : name;
            items.Add(new SystemRow (id, depth, name, systemSet, system, count));
        }
        
    #region draw items
        protected override void  RowGUI(RowGUIArgs args)
        {
            var item = (SystemRow)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
            {
                CellGUI(args.GetCellRect(i), item, ref args);
            }
        }
        private void CellGUI (Rect cellRect, SystemRow item, ref RowGUIArgs args)
        {
            cellRect.x += 15 + item.depth * 14;
            if (item.system is QuerySystemBase querySystem) {
                DrawSystem(cellRect, item, querySystem);
                SelectSystem(cellRect, item);
                return;
            }
            if (item.system is SystemGroup systemGroup)
            {
                DrawSystemGroup(cellRect, item, systemGroup, args);
                SelectSystem(cellRect, item);
                return;
            }
            if (item.systemSet) {
                DrawStore(cellRect, item);
            } else {
                DrawRoot(cellRect, item);
            }
        }
        
        private void DrawRoot(Rect cellRect, SystemRow item)
        {
            var labelStyle = GetLabelStyle(item.id, false);
            var toggle      = cellRect;
            toggle.x        = treeWidth - 20 - 2;
            toggle.width    = 20;
            groupMatches    = GUI.Toggle(toggle, groupMatches, (string)null);
            toggle.x       -= 45;
            toggle.width    = 45;
            GUI.Label(toggle, "groups", labelStyle);
            GUI.Label(cellRect, item.displayName, labelStyle);
        }
        
        private void DrawStore(Rect cellRect, SystemRow item)
        {
            cellRect.y ++;
            cellRect.height -= 2;
            GUI.Label(cellRect, "System Set", GetLabelStyle(item.id, false));
            
            var labelWidth = EditorGUIUtility.labelWidth;
            cellRect.x = labelWidth;
            cellRect.width = treeWidth - labelWidth - 33;
            GUI.enabled = false;
            EditorGUI.ObjectField(cellRect, item.systemSet, typeof(ECSSystemSet), true);
            GUI.enabled = true;
        }
        
        private void SelectSystem(Rect cellRect, SystemRow item) {
            if (RowButton(cellRect, "⇨", "Edit System   F2")) { // ➞►⇨
                item.systemSet.treeViewFocusId = item.system.Id;
                Selection.activeGameObject = item.systemSet.gameObject;
            }
        }
        #endregion
        
        internal bool AreMatchesEqual(List<SystemTreeMatch> treeMatches)
        {
            if (matchingSystems.Count != treeMatches.Count) {
                return false;
            }
            for (int n = 0; n < treeMatches.Count; n++) {
                if (treeMatches[n].system    != matchingSystems[n].system ||
                    treeMatches[n].systemSet != matchingSystems[n].systemSet)
                {
                    return false;
                }
            }
            return true;
        }
        
        internal void SetMatches(List<SystemTreeMatch> matches)
        {
            matchingSystems.Clear();
            foreach (var match in matches) {
                matchingSystems.Add(match);
            }
            Reload();
        }
        
    #region key event
        protected override void KeyEvent()
        {
            Event ev = Event.current;
            // Debug.Log($"Event - {ev}");
            if (ev.type != EventType.KeyDown) {
                return;
            }
            switch (ev.keyCode) {
                case KeyCode.F2:
                    EditSystem(ev);
                    break;
                case KeyCode.Space:
                    ToggleEnabled(ev);
                    break;
            }
        }
        
        private void EditSystem(Event ev)
        {
            var selectedRow = (SystemRow)FindItem(state.lastClickedID, rootItem);
            var systemSet   = selectedRow.systemSet;
            if (systemSet == null) {
                return;
            }
            ev.Use();
            systemSet.treeViewFocusId   = selectedRow.system.Id;
            Selection.activeGameObject  = systemSet.gameObject;
        }
        
        private void ToggleEnabled(Event ev)
        {
            var selectedRow = (SystemRow)FindItem(state.lastClickedID, rootItem);
            var system = selectedRow.system; 
            if (system != null) {
                ev.Use();
                system.Enabled = !system.Enabled; 
            }
        }
        #endregion
        
    #region mouse event
        protected override void DoubleClickedItem(int id)
        {
            var selectedRow = (SystemRow)FindItem(id, rootItem);
            var systemSet   = selectedRow.systemSet;
            if (systemSet == null) {
                return;
            }
            systemSet.treeViewFocusId   = selectedRow.system.Id;
            Selection.activeGameObject  = systemSet.gameObject;
        }
        #endregion
    }
}