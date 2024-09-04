// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using Friflo.Json.Fliox.Mapper;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// ReSharper disable RedundantJumpStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class SystemSetTree : SystemTree
    {
        private readonly    ECSSystemSet    ecsSystemSet;
        private static readonly TypeStore TypeStore = new ();


        public SystemSetTree(TreeViewState treeViewState, ECSSystemSet ecsSystemSet) : base(treeViewState) {
            this.ecsSystemSet = ecsSystemSet;
            Reload();
        }
        
    #region build tree items
        protected override TreeViewItem BuildRoot ()
        {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we create a fixed set of items. In a real world example,
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique. The root item is required to 
            // have a depth of -1, and the rest of the items increment from that.
            var rootGroup   = ecsSystemSet.groupRoot;
            var root        = new SystemRow(-1, -1, "Root",  rootGroup, null, false);
            var items       = new List<TreeViewItem>();
            BuildAddItem(items, 0, 0, "Systems", rootGroup, null, false);
            foreach (var system in rootGroup.ChildSystems) {
                BuildAddSystem(items, 1, system);
            }
            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths (root, items);
            
            // Return root of the tree
            return root;
        }
        
        private static void BuildAddSystem(List<TreeViewItem> items, int depth, BaseSystem system)
        {
            if (system is SystemGroup childGroup) {
                BuildAddItem(items, system.Id, depth, system.Name, system, null, false);
                foreach (var child in childGroup.ChildSystems) {
                    BuildAddSystem(items, depth + 1, child);    
                }
            }
            else if (system != null) {
                BuildAddItem(items, system.Id, depth, system.Name, system, null, false);
                var mapper = TypeStore.GetTypeMapper(system.GetType());
                var fields = mapper.PropFields.fields;
                var count  = 0;
                for (int n = 0; n < fields.Length; n++) {
                    var field = fields[n];
                    if (field.name is "id" or "enabled") {
                        continue;
                    }
                    var id = system.Id + (++count << 16);
                    var isLast = n == fields.Length - 1;
                    BuildAddItem(items, id, depth + 1, field.name, system, field, isLast);
                }
            }
            else {
                BuildAddItem(items, system.Id, depth + 1, system.Name, system, null, false);
            }
        }
        
        private void AddGroup(GenericMenu menu, string name)
        {
            var systems  = ecsSystemSet.groupRoot;
            var content = new GUIContent($"Add lifecycle Group/{name}");
            if (systems.FindGroup(name, false) != null) {
                menu.AddDisabledItem(content);
                return;
            }
            menu.AddItem(content, false, () => {
                var lifecycleGroup = new SystemGroup(name);
                AddAndSelect(systems, lifecycleGroup);
            });
        }
        #endregion
        
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
            var system = item.system;
            if (system == null) {
                return;
            }
            if (item.field != null) {
                DrawQueryField(cellRect, item);
                return;
            }
            if (system is SystemGroup systemGroup)
            {
                if (item.depth > 0) {
                    DrawSystemGroup(cellRect, item, systemGroup, args);
                    if (RowButton(cellRect, "...")) {
                        SystemGroupMenu(systemGroup, item);
                    }
                    return;
                }
                DrawHeader(cellRect, item);
                if (RowButton(cellRect, "...")) {
                    SystemsMenu();
                    return;
                }
                return;
            }
            DrawSystem(cellRect, item, system);
            if (RowButton(cellRect, "...")) {
                SystemMenu(system);
                return;
            }
        }
        #endregion
        
    #region context menus
        private void SystemsMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Group"), false, () => {
                var group = new SystemGroup("Group");
                AddAndSelect(ecsSystemSet.groupRoot, group);
            });
            AddGroup(menu, "Start");
            AddGroup(menu, "Update");
            AddGroup(menu, "FixedUpdate");
            AddGroup(menu, "LateUpdate");
            menu.AddItem(new GUIContent("Copy Perf Log"), false, () => {
                GUIUtility.systemCopyBuffer = ecsSystemSet.groupRoot.GetPerfLog();
            });
            menu.ShowAsContext();
        }
        
        private static void SystemMenu(BaseSystem querySystem)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove System"), false, () => {
                querySystem.ParentGroup.Remove(querySystem);
            });
            menu.AddSeparator(null);
            menu.AddItem(new GUIContent("Edit System _f2"), false, () => {
                ExternalEditor.OpenFileWithType(querySystem.GetType());
            });
            menu.ShowAsContext();
        }
        
        private void SystemGroupMenu(SystemGroup systemGroup, SystemRow item)
        {
            var menu = new GenericMenu();
            if (systemGroup.ParentGroup != null) {
                menu.AddItem(new GUIContent("Rename _f2"), false, () => {
                    BeginRename(item);
                });
            }
            menu.AddItem(new GUIContent("Remove Group"), false, () => {
                systemGroup.ParentGroup.Remove(systemGroup);
            });
            menu.AddItem(new GUIContent("Add Group/Empty"), false, () => {
                var empty = new SystemGroup("Group");
                AddAndSelect(systemGroup, empty);
            });
            menu.AddItem(new GUIContent("Add Group/ReadGameObjects"), false, () => {
                AddAndSelect(systemGroup, UnityGroupUtils.ReadGameObjects());
            });
            menu.AddItem(new GUIContent("Add Group/WriteGameObjects"), false, () => {
                AddAndSelect(systemGroup, UnityGroupUtils.WriteGameObjects());
            });
            menu.AddItem(new GUIContent("Copy Perf Log"), false, () => {
                GUIUtility.systemCopyBuffer = systemGroup.GetPerfLog();
            });

            menu.AddSeparator(null);
            var systemTypes = SystemTypeRegistry.GetSystemTypes(); 
            foreach (var type in systemTypes) {
                if (type.Namespace == "Friflo.Engine.UnityEditor") continue;
                if (type == typeof(SystemGroup)) continue;
                var name = type.Namespace == null ? type.Name : $"{type.Namespace}/{type.Name}";
                menu.AddItem(new GUIContent(name), false, () => {
                    var constructor = type.GetConstructor(System.Type.EmptyTypes);
                    var system = (BaseSystem)constructor!.Invoke(System.Array.Empty<object>());
                    AddAndSelect(systemGroup, system);
                });
            }
            menu.ShowAsContext();
        }
        
        private void AddAndSelect(SystemGroup systemGroup, BaseSystem system)
        {
            systemGroup.Add(system);
            SetExpanded(systemGroup.Id, true);
            state.selectedIDs.Clear();
            state.selectedIDs.Add(system.Id);
        }

        protected override void ContextClickedItem(int id)
        {
            var row = (SystemRow)FindItem(id, rootItem);
            var system = row.system;
            if (system is SystemGroup systemGroup) {
                if (row.depth == 0) {
                    SystemsMenu();
                    return;
                }
                SystemGroupMenu(systemGroup, row);
                return;
            }
            if (system != null) {
                SystemMenu(system);    
            }
        }
        #endregion
        
    #region rename system
        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename) return;
            var row = (SystemRow)FindItem(args.itemID, rootItem);
            row.displayName = args.newName;
            var group = (row.system as SystemGroup)!; 
            group.SetName(args.newName);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            var systemRow = (SystemRow)item;
            if (systemRow.system is SystemGroup) {
                return item.depth > 0;
            }
            return false;
        }
        #endregion
        
    #region key event
        protected override void KeyEvent()
        {
            Event ev = Event.current;
            // Debug.Log($"Event - {ev}");
            if (ev.type != EventType.KeyDown) {
                return;
            }
            switch (ev.keyCode) {
                case KeyCode.Delete:
                    DeleteSelection(ev);
                    break;
                case KeyCode.C:
                    if (ev.control) {
                        CopySelection(ev);
                    }
                    break;
                case KeyCode.None:
                    if (ev.character == (char)KeyCode.Tab) {
                        FocusField(ev);
                    }
                    break;
                case KeyCode.F2:
                    EditSystem(ev);
                    break;
                case KeyCode.Space:
                    ToggleEnabled(ev);
                    break;
            }
        }
        
        private void CopySelection(Event ev)
        {
            ev.Use();
            var selectedRow = (SystemRow)FindItem(state.lastClickedID, rootItem);
            var system      = selectedRow?.system;
            if (system == null) {
                return;
            }
            GUIUtility.systemCopyBuffer = system.GetPerfLog();
        }
    
        private void DeleteSelection(Event ev)
        {
            ev.Use();
            foreach (var id in state.selectedIDs) {
                var systemRow = (SystemRow)FindItem(id, rootItem);
                if (systemRow == null || systemRow.depth == 0) {
                    continue;
                }
                var system = systemRow.system;
                system.ParentGroup.Remove(system);
            }
            Reload(); // TODO remove
        }
        
        private void FocusField(Event ev)
        {
            var selectedRow = (SystemRow)FindItem(state.lastClickedID, rootItem);
            if (selectedRow.controlName != null) {
                GUI.FocusControl(selectedRow.controlName);
                ev.Use();
            }
        }
        
        private void EditSystem(Event ev)
        {
            var selectedRow = (SystemRow)FindItem(state.lastClickedID, rootItem);
            var system = selectedRow.system; 
            if (system != null && system is not SystemGroup) {
                ev.Use();
                ExternalEditor.OpenFileWithType(selectedRow.system.GetType());
            }
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
        
    #region drag & drop
        private readonly List<BaseSystem> dragSources = new List<BaseSystem>();
        
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            dragSources.Clear();
            foreach (var id in args.draggedItemIDs) {
                var systemRow = (SystemRow)FindItem(id, rootItem);
                if (systemRow.field != null) {
                    return false;
                }
                if (systemRow.system.ParentGroup == null) {
                    return false;
                }
                dragSources.Add(systemRow.system);
            }
            return true;
        }
        
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            string dragTitle;
            if (dragSources.Count == 1) {
                dragTitle = dragSources[0].Name;
            } else {
                dragTitle = $"{dragSources.Count} Systems";
            }
            state.selectedIDs.Clear();
            state.selectedIDs.AddRange(args.draggedItemIDs);
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.paths = new string[] { }; // required to enable dragging for macOS
            DragAndDrop.StartDrag (dragTitle);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (args.parentItem is not SystemRow targetRow) {
                return DragAndDropVisualMode.Rejected;
            }
            var dropTarget = targetRow.system;
            // Debug.Log($"{dragSource.Name} -> {dropTarget.Name}");
            if (dropTarget is not SystemGroup targetGroup) {
                return DragAndDropVisualMode.Rejected;
            }
            foreach (var dragSource in dragSources) {
                // Debug.Log($"HandleDragAndDrop - source: {dragSource.Name}, target: {dropTarget.Name}");
                if (dragSource is SystemGroup sourceGroup) {
                    if (dragSource == dropTarget) {
                        return DragAndDropVisualMode.Rejected;
                    }
                    if (sourceGroup.IsAncestorOf(dropTarget)) {
                        return DragAndDropVisualMode.Rejected;
                    }
                }
            }
            if (args.performDrop) {
                PerformDrop(targetGroup, args.insertAtIndex);
            }
            return DragAndDropVisualMode.Move;
        }
        
        private void PerformDrop(SystemGroup targetGroup, int insertAtIndex)
        {
            SetExpanded(targetGroup.Id, true);
            if (insertAtIndex == -1) {
                foreach (var dragSource in dragSources) {
                    // Debug.Log($"Drop {dragSource.Name} -> {targetGroup.Name}");
                    dragSource.MoveSystemTo(targetGroup, -1);
                }
                return;
            }
            for (int n = dragSources.Count - 1; n >= 0; n--) {
                var dragSource = dragSources[n];
                // Debug.Log($"Drop {dragSource.Name} -> {targetGroup.Name} at {insertAtIndex}");
                dragSource.MoveSystemTo(targetGroup, insertAtIndex);
            }
        }
        #endregion
        
        protected override float GetCustomRowHeight(int row, TreeViewItem item) {
            var systemRow = (SystemRow)item;
            var field = systemRow.field; 
            if (field != null) {
                if (SystemFieldDrawerMap.Map.TryGetValue(field.fieldType.type, out var guiField)) {
                    return guiField.height;
                }
            }
            return 20;
        }
    }
}

