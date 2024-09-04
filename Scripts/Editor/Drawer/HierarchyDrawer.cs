// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using UnityEditor;
using UnityEngine;
using Friflo.Engine.Unity;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    [InitializeOnLoad]
    internal class HierarchyDrawer
    {
        private  readonly   Texture2D               hubTexture;
        private  readonly   Texture2D               storeTexture;
        private  readonly   SymbolDrawer            symbolDrawer    = new SymbolDrawer();
        private  readonly   Settings                settings        = Settings.Instance;
        private  readonly   Dictionary<object, int> stores = new ();
        private  readonly   object                  missingStore = new ();
        private             HierarchyStyles         styles;
        private             GUIStyle                labelStyle;
        private             float                   minX;
        private             int                     idIndentRefresh;
        
        internal static readonly List<ArchetypeQuery> SelectedQueries = new ();
        
        static HierarchyDrawer ()
        {
            var drawer = new HierarchyDrawer();
            EditorApplication.hierarchyWindowItemOnGUI  += drawer.HierarchyItemCallback;
            EditorSceneManager.sceneClosed              += drawer.OnSceneClosed;
        }
        
        private HierarchyDrawer() {
            hubTexture      = AssetDatabase.LoadAssetAtPath ("Assets/Friflo.Engine.Unity/Images/hub.png",   typeof(Texture2D)) as Texture2D;
            storeTexture    = AssetDatabase.LoadAssetAtPath ("Assets/Friflo.Engine.Unity/Images/store.png", typeof(Texture2D)) as Texture2D;
        }
        
       
        private static bool HasFocus(string name)
        {
            var window = EditorWindow.focusedWindow; 
            if (window == null) {
                return false;
            }
            return window.titleContent.text == name;   
        }
        
        private GUIStyle GetLabelStyle(GameObject obj) {
            var hierarchyHasFocus   = HasFocus("Hierarchy");
            var isSelected          = Selection.Contains(obj);
            return isSelected && hierarchyHasFocus ? styles.idLabelSelected : styles.idLabel;
        }
        
        private void HierarchyItemCallback (int instanceID, Rect selectionRect)
        {
            try {
                DrawHierarchyItem(instanceID, selectionRect);
            }
            catch (Exception e) {
                // catch an exception so Hierarchy window remain intact
                Debug.LogException(e);
            }
        }

        private void DrawHierarchyItem (int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (!(obj is GameObject gameObject)) {
                return;
            }
            styles        ??= new HierarchyStyles();
            labelStyle      = GetLabelStyle(gameObject);
            float width     = 8;
            var ecsEntity   = gameObject.GetComponent<ECSEntity>();
            if (ecsEntity != null) {
                DrawEntity(ecsEntity, selectionRect, ref width);
            }
            var ecsStore = gameObject.GetComponent<ECSStore>();
            if (ecsStore != null) {
                DrawStoreComponent(ecsStore, selectionRect, ref width);
            }
            var hub = gameObject.GetComponent<ECSHub>();
            if (hub != null) {
                DrawHubTwin(selectionRect, ref width);
            }
        }
        
        private void DrawEntity(ECSEntity ecsEntity, Rect selectionRect, ref float width)
        {
            var right   = selectionRect.x + selectionRect.width - width;
            Rect rect   = selectionRect;
            rect.y     += 1;
            rect.x      = right;
            var context = ecsEntity.StoreContext;
            var store   = context?.EntityStore ?? missingStore;
            var showIds = settings.hierarchyEntityIds;
            var offset  = -8;
            if (showIds) {
                DrawEntityId(ref rect, ecsEntity, ref width);
                offset +=  12 + GetIdIndent(store, ecsEntity.EntityId) * 8;
            }
            var entity = ecsEntity.Entity;
            if (entity.IsNull) {
                return;
            }
            DrawQueryMatches(selectionRect, entity);
            if (entity.Components.Count == 0 && entity.Tags.Count == 0) {
                return;
            }
            width      += offset;
            rect.x      = right - offset;
            rect.height = 10;
            minX        = settings.hierarchyMargin;

            if (settings.hierarchyComponents) {
                var types = entity.Archetype.ComponentTypes;
                types.Remove<GameObjectLink>();
                symbolDrawer.DrawComponents(ref rect, types, ref width, labelStyle, minX);
                if (rect.x < minX) {
                    return;
                } 
            }
            if (settings.hierarchyTags) {
                var tags = entity.Archetype.Tags;
                symbolDrawer.DrawTags(ref rect, tags, ref width, labelStyle, minX);
                if (rect.x < minX) {
                    return;
                } 
            }
        }
        
        private static void DrawQueryMatches(Rect rect, Entity entity)
        {
            foreach (var query in SelectedQueries) {
                var type = entity.Archetype;
                if (type.Store != query.Store) continue;
                if (query.IsMatch(type.ComponentTypes, type.Tags))
                {
                    var selectedRect = new Rect(rect.x + rect.width + 10, rect.y + 2, 4, 12);
                    EditorGUI.DrawRect(selectedRect, ColorStyles.QueryMatch);
                    return;
                }
            }
        }
        
        private void DrawEntityId(ref Rect rect, ECSEntity ecsEntity, ref float width)
        {
            var id = ecsEntity.EntityId.ToString();
            var context = ecsEntity.StoreContext;
            if (context == null) {
                id = $"~ {id}"; 
            }
            Vector2 size = labelStyle.CalcSize( new GUIContent( id ) );
            width       = size.x + 4; // Math.Max(20, size.x);
            rect.x     -= width - 12;
            rect.width  = width;
            GUI.Label(rect, id, labelStyle);
        }

        private void DrawStoreComponent(ECSStore ecsStore, Rect selectionRect, ref float width)
        {
            var count       = "[" + ecsStore.EntityStore.Count + "]";
            Vector2 size    = labelStyle.CalcSize( new GUIContent( count ) );
            var countWidth  = size.x;
            Rect rect = new Rect(selectionRect.x + selectionRect.width - width - countWidth - 5, selectionRect.y + 1, countWidth, size.y);
            GUI.Label(rect, count, labelStyle);
            rect = new Rect(selectionRect.x + selectionRect.width - width - 4f , selectionRect.y, 18f, 18f);
            GUI.Label (rect, storeTexture);
            width += 20 + size.x;
        }
        
        private void DrawHubTwin(Rect selectionRect, ref float width)
        {
            Rect rect = new Rect(selectionRect.x + selectionRect.width - 4f - width, selectionRect.y, 18f, 18f);
            GUI.Label (rect, hubTexture);
        }
        
#region id indentation
        private void OnSceneClosed (Scene scene) {
            stores.Clear();
        }
        
        private int GetIdIndent(object store, int id)
        {
            var indent = (int)Math.Log10(id);
            stores.TryGetValue(store, out int curIndent);
            if (curIndent < indent) {
                stores[store] = curIndent = indent;
            }
            return curIndent;
        }
        #endregion
    }
}