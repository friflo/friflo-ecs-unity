// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    [CustomEditor(typeof(ECSEntity))]
    // ReSharper disable once InconsistentNaming
    internal sealed class ECSEntityEditor : Editor
    {
        internal                    VisualElement           imguiContainer;
        private static              SystemMatchesTree       _systemMatchesTree;
        private static readonly     TreeViewState           TreeViewState = new TreeViewState();
        private static readonly     List<SystemMatch>       MatchesBuffer = new();
        private static readonly     List<SystemTreeMatch>   SystemTreeMatches = new();
        
        internal static             EntityStyles        styles;
        internal static readonly    SymbolDrawer        SymbolDrawer = new SymbolDrawer();
        
        private static              string          _filter;
        private static              string[]        _filterTokens = Array.Empty<string>();
        
        // private void Reset() { Debug.Log("Reset"); }
        // private void Awake() { Debug.Log("Awake"); }
        private void OnEnable() {
            _systemMatchesTree  ??= new SystemMatchesTree(TreeViewState);
        }

        private const bool ShowComponents = false;
        
        private static void DrawEntityHeader(ECSEntity ecsEntity)
        {
            // --- entity id
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel($"id      {ecsEntity.EntityId}", styles.idLabel);
            GUILayout.FlexibleSpace();
            
            // --- filter
            EditorGUILayout.LabelField("filter", styles.filterLabel);
            var bg = GUI.backgroundColor;
            var isDark = EditorGUIUtility.isProSkin;
            if (string.IsNullOrEmpty(_filter)) {
                GUI.backgroundColor = isDark ? new Color32(255,255,255, 255) : new Color32(220,220,220, 255);
            } else {
                GUI.backgroundColor = isDark ? new Color32(0,0,0, 255) : new Color32(255,255,220, 255);
            }
            var newFilter = EditorGUILayout.TextField(_filter, styles.filterText);
            UpdateFilter(newFilter);
            GUI.backgroundColor = bg;
            
            if (GUILayout.Button("...", styles.moreButton)) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy as JSON"), false, () => {
                    GUIUtility.systemCopyBuffer = ecsEntity.Entity.DebugJSON;
                });
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);
        }
        
        private static void DrawJsonComponents(ECSEntity ecsEntity)
        {
            var sb = new StringBuilder();
            var components = ecsEntity.Components;
            if (components != null) {
                foreach (var component in components) {
                    sb.Append(component);
                    sb.Append('\n');
                }
            }
            EditorGUILayout.TextArea(sb.ToString(), GUILayout.Height(80));
        }
        
        private Entity GetEntity(ECSEntity ecsEntity)
        {
            var store = GetStoreContext();
            if (store == null) {
                EditorGUILayout.LabelField("Missing store");
                return default;
            }
            var entity = ecsEntity.Entity;
            if (entity.IsNull) { 
                EditorGUILayout.LabelField("Entity not found");
                return default;
            }
            return entity;
        }
        
        private static void DrawTags(Entity entity, ECSEntityEditor editor)
        {
            var rect = EntityDrawer.DrawTagHeader(entity, editor);
            int count = 0;

            if (!EntityDrawer.expandTags) {
                float width = 0;
                rect.x += rect.width - 45;
                rect.y += 4.5f;
                var tags = entity.Archetype.Tags;
                SymbolDrawer.DrawTags(ref rect, tags, ref width, styles.labelStyle, 70);
                return;
            }
            foreach (var tag in entity.Tags)
            {
                if (!editor.MatchesFilter(tag.Name)) {
                    continue;
                }
                var newHorizontal = count % 3 == 0;  
                if (newHorizontal) {
                    if (count > 0) EditorGUILayout.EndHorizontal(); 
                    EditorGUILayout.BeginHorizontal();
                }
                DrawTag(tag, entity);
                count++;
            }
            if (count > 0) EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2f, true);
        }
        
        private static void DrawTag(TagType tag, Entity entity)
        {
            var width   = 0f;
            var style   = Settings.Instance.inspectorTags ?  styles.tagStyleSymbols : styles.tagStyle;
            var pressed = GUILayout.Button(tag.Name, style, GUILayout.MaxWidth(100));
            var rect    = GUILayoutUtility.GetLastRect();
            rect.x     -= 2f;
            rect.y     += 3.5f;
            if (Settings.Instance.inspectorTags) {
                SymbolDrawer.DrawSymbol(ref rect, tag, ref width);
            }
            if (!pressed) {
                return;
            }
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Tag"), false, () => {
                entity.RemoveTags(new Tags(tag));
            });
            menu.AddItem(new GUIContent("Edit Tag"), false, () => {
                ExternalEditor.OpenFileWithType(tag.Type);
            });
            menu.ShowAsContext();
        }
        
        private static void DrawComponents(Entity entity, ECSEntityEditor editor)
        {
            var rect = EntityDrawer.DrawComponentHeader(entity, editor);
            if (!EntityDrawer.expandComponents) {
                float width = 0;
                rect.x += rect.width - 45;
                rect.y += 4.5f;
                var types = entity.Archetype.ComponentTypes;
                // types.Remove<GameObjectLink>();
                SymbolDrawer.DrawComponents(ref rect, types, ref width, styles.labelStyle, 120);
                return;
            }
            foreach (var component in entity.Components)
            {
                if (!Settings.Instance.inspectorGameObjectLink &&
                     component.Type.Type == typeof(GameObjectLink)) continue;
                if (!editor.MatchesFilter(component.Type.Name))     continue;
                EditorGUILayout.BeginHorizontal();
                DrawComponent(component, entity);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2f, true);
            }
        }
        

        
        private static void DrawComponent(EntityComponent component, Entity entity)
        {
            var entityDrawer    = EntityDrawer.Instance;
            
            var type            = component.Type.Type;
            var drawer          = entityDrawer.GetDrawer(type);
            var componentValue  = GetComponentValue(component);

            drawer.AddComponent(entity, componentValue, GuiStyles.instance, SymbolDrawer);
            if (component.Type.Type == typeof(GameObjectLink)) {
                return;
            }
            if (GUILayout.Button("...", styles.moreButton)) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove Component"), false, () => {
                    EntityUtils.RemoveEntityComponent(entity, component.Type);
                });
                menu.AddItem(new GUIContent("Edit Component"), false, () => {
                    ExternalEditor.OpenFileWithType(component.Type.Type);
                });
                menu.ShowAsContext();
            }
        }

        // Comment out to use standard Editor behavior
        public override VisualElement CreateInspectorGUI() {
            imguiContainer = new IMGUIContainer(IMGUIDraw);
            int offset = GuiStyles.IMGUIContainerOffset;
            imguiContainer.style.left = -offset;
            // imguiContainer.style.right = -IMGUIContainerOffset;
            imguiContainer.style.marginRight = -offset;
            return imguiContainer;
        }

        // https://github.com/Unity-Technologies/UIElementsExamples/blob/e940d1c9dddfe99fd500c9f34f4801856ad37f18/Assets/Examples/Editor/Bindings/InspectorComparerWindow.cs#L153
        private void IMGUIDraw()
        {
            EditorGUI.indentLevel = 1; // required to compensate IMGUIContainerOffset
            var editorWidth = GetEditorWith();
            
            EditorGUIUtility.hierarchyMode  = true;         // e.g. indentation of foldout triangle
            if (editorWidth > 400f) {
                EditorGUIUtility.wideMode   = true;     // E.g Vector3Field take one line
                // Note! Set labelWidth required for wideMode == true
                // To enable reliable mouse dragging of fields (FloatField, Vector3Field(), ...) in Play mode
                //  See: [unity game engine - How to make Vector3 fields behave like the ones from Transform in a CustomEditor - Stack Overflow]
                //  https://stackoverflow.com/questions/54814067/how-to-make-vector3-fields-behave-like-the-ones-from-transform-in-a-customeditor
                EditorGUIUtility.labelWidth = 140;
            } else {
                EditorGUIUtility.wideMode   = false;    // E.g Vector3Field take two lines
            }
            OnInspectorGUI();
        }
        
        public override void OnInspectorGUI()
        {
            styles              ??= new EntityStyles();
            GuiStyles.instance  ??= new GuiStyles();
            GuiStyles.instance.SetBackgrounds();
            var ecsEntity  = (ECSEntity)target;

            DrawEntityHeader(ecsEntity);
            
            var entity = GetEntity(ecsEntity);
            if (!entity.IsNull) {
                DrawTags      (entity, this);
                DrawComponents(entity, this);
                DrawSystems   (entity);
            }
            if (entity.IsNull || ShowComponents) {
                DrawJsonComponents(ecsEntity);
            }
            RepaintOnPlay(ecsEntity);
        }
        
        private float GetEditorWith()
        {
            // var width =  EditorGUIUtility.currentViewWidth; // Note: cannot be used. Occasionally returns 0
            var root = GetRootElement();
            var width = root.resolvedStyle.width;
            // Debug.Log($"Editor width: {width}");
            return width;
        }
        
        private VisualElement GetRootElement()
        {
            var element = imguiContainer;
            while (element.parent != null) {
                element = element.parent;
            }
            return element;
        }
        
        // can return null
        private StoreContext GetStoreContext() {
            var ecsEntity = (ECSEntity)target;
            return ecsEntity.StoreContext;
        }
        
        private static void UpdateFilter(string newFilter) {
            if (newFilter == _filter) {
                return;
            }
            _filter      = newFilter;
            var temp    = _filter.Split(' ');
            _filterTokens = temp.Where(token => token.Length > 0).ToArray();
        }
        
        private bool MatchesFilter(string componentName)
        {
            if (_filterTokens.Length == 0) {
                return true;
            }
            foreach (var token in _filterTokens) {
                if (componentName.Contains(token, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        
        private void DrawSystems(Entity entity)
        {
            ECSSystemSetEditor.systemStyles ??= new SystemStyles();
            var separator = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(new Rect(separator.x - 20, separator.y + 2, separator.width + 40, 1), ColorStyles.SeparatorColor);
            EditorGUILayout.EndHorizontal();
            
            var treeMatches = SystemTreeMatches;
            SetTreeMatches(treeMatches, entity);
            var tree = _systemMatchesTree;
            if (!tree.AreMatchesEqual(treeMatches)) {
                tree.SetMatches(treeMatches);
            }
            var rect            = GUILayoutUtility.GetLastRect();
            var startY          = rect.y + rect.height + 5;
            var treeWidth       = GetEditorWith() - GuiStyles.IMGUIContainerOffset - 3;
            var treeHeight      = tree.totalHeight;
            Rect treeViewRect   = EditorGUILayout.GetControlRect(false, startY + treeHeight);
            tree.treeWidth      = treeWidth;
            tree.showPerf       = Application.isPlaying;
            tree.multiColumnHeader.GetColumn(0).width = treeWidth - 50; 
            tree.treeViewScreenRect = GUIUtility.GUIToScreenRect(treeViewRect);
            if (!tree.HasFocus()) {
                tree.state.selectedIDs.Remove(0);
            }
            tree.OnGUI(new Rect(0, startY, treeWidth - 3, treeHeight));
        }
        
        private static void SetTreeMatches(List<SystemTreeMatch> treeMatches, Entity entity)
        {
            treeMatches.Clear();
            foreach (var systemSet in ECSSystemSet.AllSystemSets)
            {
                systemSet.groupRoot.GetMatchingSystems(entity.Archetype, MatchesBuffer, _systemMatchesTree.groupMatches);
                if (MatchesBuffer.Count == 0) {
                    continue;
                }
                treeMatches.Add(new SystemTreeMatch { id = -1, systemSet = systemSet, depth = 1 });
                foreach (var match in MatchesBuffer) {
                    treeMatches.Add(new SystemTreeMatch {
                        id          = match.System.Id,
                        systemSet   = systemSet,
                        system      = match.System,
                        depth       = match.Depth + 1,
                        count       = match.Count
                    });
                }
            }
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
            _systemMatchesTree.Repaint();
        }
        #endregion

#pragma warning disable CS0618 // Type or member is obsolete
        private static object GetComponentValue (EntityComponent component) => component.Value;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}