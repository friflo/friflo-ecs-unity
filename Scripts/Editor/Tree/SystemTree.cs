// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Json.Fliox.Mapper.Map;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// ReSharper disable RedundantJumpStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal abstract class SystemTree : TreeView
    {
        private readonly    List<int>       toggledMarkers = new ();
        internal            float           treeWidth;
        internal            bool            showPerf = false;
        internal            Rect            treeViewScreenRect;
        
        protected SystemTree(TreeViewState treeViewState)
            : base(treeViewState, CreateHeader())
        {
            multiColumnHeader.height = 0;
            rowHeight = 20;
            columnIndexForTreeFoldouts = 0;
        }
        
        internal void OnSystemChanged(SystemChanged changed) {
            // Debug.Log("TreeView - OnSystemChanged");
            Reload();
        }
        
        private static MultiColumnHeader CreateHeader()
        {
            return new MultiColumnHeader(
                new MultiColumnHeaderState(
                    new MultiColumnHeaderState.Column[] {
                        new() { width = 100 }
                    }));
        }

        internal static void BuildAddItem(List<TreeViewItem> items, int id, int depth, string name, BaseSystem system, PropField field, bool isLast)
        {
            items.Add(new SystemRow (id, depth, name, system, field, isLast));
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            UpdateEntityMarkers();
            ScrollToLastClicked();
        }
        
        private void ScrollToLastClicked()
        {
            // Debug.Log(state.lastClickedID);
            var focusedItem = FindItem(state.lastClickedID, rootItem);
            var focusedRow  = FindRowOfItem(focusedItem);
            var focusedRect = GetRowRect(focusedRow);
            focusedRect.y  += treeViewScreenRect.y;
            
            var inspectorEditor     = EditorUtils.GetInspectorWindow();
            if (inspectorEditor == null) {
                return;
            }
            var inspectorPosition   = inspectorEditor.position;
            var inspectorScrollView = EditorUtils.InspectorScrollView;
            // Debug.Log($"{focusedRect.y} - inspectorEditor: {inspectorEditor.position.y}");
            var offset = inspectorScrollView.scrollOffset;
            var topDif = focusedRect.y - inspectorPosition.y - 30;
            if (topDif < 0) {
                // Debug.Log($"scrollOffset: {inspectorScrollView.scrollOffset},  topDif: {topDif}");
                inspectorScrollView.scrollOffset = new Vector2(offset.x, offset.y + topDif);
                return;
            }
            var bottomDif = focusedRect.y + focusedRect.height - (inspectorPosition.y + inspectorPosition.height) - 10;
            if (bottomDif > 0) {
                inspectorScrollView.scrollOffset = new Vector2(offset.x, offset.y + bottomDif);
            }
        }
        
        private void UpdateEntityMarkers()
        {
            var queries = HierarchyDrawer.SelectedQueries;
            queries.Clear();
            foreach (var id in state.selectedIDs) {
                var item = (SystemRow)FindItem(id, rootItem);
                AddSelectedGroupSystems(queries, item.system);
            }
            EditorApplication.RepaintHierarchyWindow();
        }
        
        private static void AddSelectedGroupSystems(List<ArchetypeQuery> queries, BaseSystem system)
        {
            if (system is QuerySystemBase querySystem) {
                queries.AddRange(querySystem.Queries);
            }
            if (system is SystemGroup systemGroup) {
                foreach (var child in systemGroup.ChildSystems) {
                    AddSelectedGroupSystems(queries, child);    
                }
            }
        }
        
        internal void DrawHeader(Rect cellRect, SystemRow item)
        {
            GUI.Label(cellRect, item.displayName, GetLabelStyle(item.id, false));
            // ToggleMarker(cellRect);
        }
        
        // Button - Toggle Markers
        private void ToggleMarker(Rect cellRect)
        {
            var columnWidth = treeWidth - 45;
            var toggleRect = cellRect;
            toggleRect.y++;
            toggleRect.x = columnWidth - 48;
            toggleRect.width  = 60;
            toggleRect.height = 18;
            var toggle = GUI.Button(toggleRect, "Marker");
            if (toggle)
            {
                var selectedIDs = state.selectedIDs;
                if (selectedIDs.Count > 0) {
                    toggledMarkers.Clear();
                    toggledMarkers.AddRange(selectedIDs);
                    selectedIDs.Clear();
                } else {
                    if (toggledMarkers.Count == 0) {
                        selectedIDs.Add(2);
                    } else {
                        selectedIDs.AddRange(toggledMarkers);    
                    }
                }
                UpdateEntityMarkers();
            }
        }
        
        internal bool RowButton(Rect cellRect, string label, string tooltip = null) {
            var columnWidth = treeWidth - 45;
            var moreRect = cellRect;
            moreRect.y ++;
            moreRect.x = columnWidth + 19;
            moreRect.width  = 22;
            moreRect.height = 18;
            if (tooltip == null) {
                return GUI.Button(moreRect, label);    
            }
            return GUI.Button(moreRect, new GUIContent (label, tooltip));
        }
        
    #region styles
        internal GUIStyle GetLabelStyle(int id, bool bold) {
            var styles = ECSSystemSetEditor.systemStyles;
            var selected = HasFocus() & IsSelected(id);
            if (bold) {
                return selected ? styles.labelBoldSelected : styles.labelBoldUnselected;
            }
            return selected ? styles.labelSelected : styles.labelUnselected;
        }
        
        private GUIStyle GetPerfStyle(SystemRow systemRow) {
            var styles = ECSSystemSetEditor.systemStyles;
            var selected = HasFocus() & IsSelected(systemRow.id);
            if (systemRow.system.Tick.time >= Time.time) {
                return selected ? styles.labelSelected : styles.labelUnselected;
            }
            return selected ? styles.datedSelected : styles.datedUnselected;
        }
        
        private GUIStyle GetGroupStyle(SystemRow systemRow, SystemGroup systemGroup)
        {
            var bold = IsLifeCycleGroup(systemGroup);
            return GetLabelStyle(systemRow.id, bold);
        }
        
        private static bool IsLifeCycleGroup(SystemGroup systemGroup)
        {
            if (systemGroup.ParentGroup != systemGroup.SystemRoot) {
                return false;
            }
            switch (systemGroup.Name) {
                case "Start":
                case "Update":
                case "LateUpdate":
                case "FixedUpdate":
                    return true;
            }
            return false;
        }
        #endregion
        
    #region draw system
        protected void DrawSystemGroup(Rect cellRect, SystemRow item, SystemGroup systemGroup, in RowGUIArgs args)
        {
            var columnWidth = treeWidth - 45;
            // cellRect.y++;
            if (!args.isRenaming) {
                var labelStyle  = GetGroupStyle(item, systemGroup);
                GUI.Label(cellRect, item.displayName, labelStyle);
            }
            var toggle = cellRect;
            // toggle.y --;
            var offset      = showPerf ? 40 : 0;
            toggle.x        = columnWidth - 2 - offset;
            toggle.width    = 20;
            var currentEnabled = systemGroup.Enabled;
            systemGroup.Enabled = GUI.Toggle(toggle, systemGroup.Enabled, (string)null);
            if (showPerf) {
                var perf   = cellRect;
                perf.x     = columnWidth - 25;
                perf.width = 45;
                var perfMs = GetLastAvg(systemGroup);
                var perfStyle = GetPerfStyle(item);
                GUI.Label(perf, perfMs, perfStyle);
            }
            if (currentEnabled != systemGroup.Enabled) {
                // Send event. See: SEND_EVENT notes
                systemGroup.CastSystemUpdate("enabled", systemGroup.Enabled);
                return;
            }
        }

        protected void DrawSystem(Rect cellRect, SystemRow item, BaseSystem system)
        {
            // cellRect.y++;
            var columnWidth = treeWidth - 45; // multiColumnHeader.GetColumn(0).width + 20; cause jitter
            var styles = ECSSystemSetEditor.systemStyles;

            var bg = cellRect;
            bg.height -= 2;
            bg.y ++;
            bg.x = 13;
            bg.width = columnWidth + 2;
            if (!IsSelected(item.id)) {
                if (IsExpanded(item.id)) {
                    EditorGUI.DrawRect(new Rect(13 ,                 bg.y, 2, bg.height + 2), ColorStyles.ComponentBg);
                    EditorGUI.DrawRect(new Rect(bg.x + bg.width - 2, bg.y, 2, bg.height + 2), ColorStyles.ComponentBg);
                }
                EditorGUI.DrawRect(bg, ColorStyles.ComponentBg);
            }
            EditorGUI.DrawRect(new Rect(0 ,              bg.y - 1, bg.x, bg.height + 2), ColorStyles.Bg);
            EditorGUI.DrawRect(new Rect(bg.x + bg.width, bg.y - 1, 30,   bg.height + 2), ColorStyles.Bg);
            var offset      = showPerf ? 40 : 0;
            var toggle      = bg;
            toggle.x        = columnWidth - 2 - offset;
            toggle.width    = 20;
            var currentEnabled = system.Enabled;
            system.Enabled = GUI.Toggle(toggle, system.Enabled, (string)null);
            if (currentEnabled != system.Enabled) {
                // Send event. See: SEND_EVENT notes
                system.CastSystemUpdate("enabled", system.Enabled);
                return;
            }
            if (showPerf) {
                var perf   = cellRect;
                perf.x     = columnWidth - 25;
                perf.width = 45;
                var perfMs = GetLastAvg(system);
                var perfStyle = GetPerfStyle(item);
                GUI.Label(perf, perfMs, perfStyle);
            }
            if (system is QuerySystemBase querySystem) {
                var count   = cellRect;
                count.y    += 2;
                count.x     = columnWidth - 48 - offset;
                count.width = 45;
                var style   = HasFocus() & IsSelected(item.id) ? styles.countSelected : styles.countUnselected;
                GUI.Label(count, "" + querySystem.EntityCount, style);
                var symbol  = cellRect;
                float width = 0;
                symbol.x    = columnWidth - 45 - offset;
                symbol.y   += 3f;
                var types   = querySystem.ComponentTypes;
                ECSEntityEditor.SymbolDrawer.DrawComponents(ref symbol, types, ref width, style, 0);
            }
            cellRect.x += 1;
            var labelStyle = GetLabelStyle(item.id, false);
            GUI.Label(cellRect, item.displayName, labelStyle);
        }
        
        private static string GetLastAvg(BaseSystem system) {
            var lastAvg = system.Perf.LastAvgMs(10);
            if (lastAvg < 0) return "-.---";
            return lastAvg.ToString("0.000",  NumberFormatInfo.InvariantInfo);
        }
        
        protected void DrawQueryField(Rect cellRect, SystemRow item)
        {
            var field = item.field;
            var bg = cellRect;
            bg.height -= 2;
            bg.x = 13;
            bg.width = treeWidth - 43;
            var selected = IsSelected(item.id);
            if (selected) {
                EditorGUI.DrawRect(new Rect(0 ,              bg.y, bg.x, bg.height + 2), ColorStyles.Bg);
                EditorGUI.DrawRect(new Rect(bg.x + bg.width, bg.y, 30,   bg.height + 2), ColorStyles.Bg);
            } else {
                EditorGUI.DrawRect(new Rect(13 ,                 bg.y, 2, bg.height + 2), ColorStyles.ComponentBg);
                EditorGUI.DrawRect(new Rect(bg.x + bg.width - 2, bg.y, 2, bg.height + 2), ColorStyles.ComponentBg);
                if (item.isLast) {
                    EditorGUI.DrawRect(new Rect(13, bg.y + bg.height, bg.width, 2), ColorStyles.ComponentBg);
                }
            }
            var fieldRect = cellRect;
            fieldRect.x      = 18;
            fieldRect.y      ++;
            fieldRect.width  -= 2;
            fieldRect.height -= 2;
            var controlName = "field-" + item.id;
            GUI.SetNextControlName(controlName);
            SystemFieldDrawerMap.AddField(fieldRect, item.system, field.fieldType.type, field.member, field.varType, field.name);
            item.controlName = controlName;
            if (Event.current.type == EventType.Used && fieldRect.Contains (Event.current.mousePosition)) {
                state.selectedIDs.Clear();
            }
        }
        #endregion
        

    }
}