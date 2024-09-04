// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Utils;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    
    public class Settings
    {
        public static readonly Settings Instance = new Settings();
        
        public bool hierarchyTags           = true;
        public bool hierarchyComponents     = true;
        public bool hierarchyEntityIds      = true;
        public int  hierarchyMargin         = 180;
        
        public bool inspectorTags           = true;
        public bool inspectorComponents     = true;
        public bool inspectorGameObjectLink = true;
    }
    
    internal class SettingsWindow : EditorWindow
    {
        private static      SettingsWindow  Instance    => GetInstance();
        private static      Texture2D       _logo;
        private readonly    Settings        settings    = Settings.Instance;
        private static      Texture2D       Logo        => _logo       ??= AssetDatabase.LoadAssetAtPath ("Assets/Friflo.Engine.Unity/Images/friflo.png",   typeof(Texture2D)) as Texture2D;
        
        private static SettingsWindow GetInstance() {
            var settings    = GetWindow<SettingsWindow>();
            var width       = 400;
            var height      = 350;
            var title   = new GUIContent();
            title.text  = "Settings";
            title.image = AssetDatabase.LoadAssetAtPath ("Assets/Friflo.Engine.Unity/Images/store.png",   typeof(Texture2D)) as Texture2D;
            settings.titleContent = title;
            settings.minSize = new Vector2(width, height);
            // settings.position = new Rect(settings.position.x, settings.position.y, width, height);
            return settings;
        }

        private bool expandHierarchy = true;
        private bool expandInspector = true;
        
            
        [MenuItem("Friflo/Settings",                 false, 100)]
        private static void OpenSettings() {
            Instance.Show();
        }
        
        [MenuItem("Friflo/Create Entity %e",         false, 1)]
        [MenuItem("GameObject/Friflo/Create Entity", false, 1)]
        private static void CreateEntity() {
            if (Selection.activeObject is not GameObject activeObject) {
                return;
            }
            var storeComponent = ECSEntity.GetParentStore(activeObject.transform);
            if (storeComponent == null) {
                return;
            }
            var context         = storeComponent.StoreContext;
            var store           = context.EntityStore;
            var entity          = store.CreateEntity(new GameObjectLink());
            var linkedObject    = entity.GetComponent<GameObjectLink>().gameObject;
            if (context.ecsStore.gameObject == activeObject) {
                linkedObject.transform.parent = activeObject.transform;
            } else {
                linkedObject.transform.parent = activeObject.transform.parent;
                linkedObject.transform.SetSiblingIndex(activeObject.transform.GetSiblingIndex() + 1);
            }
            Selection.activeObject  = linkedObject;
        }
        
        [MenuItem("Friflo/Copy as JSON",            false, 2)]
        [MenuItem("GameObject/Friflo/Copy as JSON", false, 2)]
        private static void CopyAsJson()
        {
            var entities = new List<Entity>();
            foreach (var obj in Selection.objects) {
                if (obj is not GameObject gameObject)   continue;
                var ecsEntity = gameObject.GetComponent<ECSEntity>();
                if (ecsEntity == null)                 continue;
                var entity = ecsEntity.Entity;
                if (entity.IsNull)                      continue;
                entities.Add(entity);
            }
            var result = TreeUtils.EntitiesToJsonArray(entities);
            GUIUtility.systemCopyBuffer = result.entities.AsString();
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 120;
            EditorGUIUtility.hierarchyMode  = true;
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("General", titleStyle, GUILayout.Height(24));
            EditorGUI.indentLevel = 1;

            HierarchySettings();
            EditorGUILayout.Space(10);
            InspectorSettings();

            EditorGUI.indentLevel = 0;
            EditorGUILayout.Space(10);
            
            Footer();
        }
        
        private void HierarchySettings()
        {
            expandHierarchy = EditorGUILayout.Foldout(expandHierarchy, "Hierarchy", true);
            if (expandHierarchy)
            {
                EditorGUILayout.BeginVertical();
                var ids             = settings.hierarchyEntityIds;
                var hiComponents    = settings.hierarchyComponents;
                var hiTags          = settings.hierarchyTags;
                var margin          = settings.hierarchyMargin;

                settings.hierarchyTags          = EditorGUILayout.Toggle("tags",        hiTags);
                settings.hierarchyComponents    = EditorGUILayout.Toggle("components",  hiComponents);
                settings.hierarchyEntityIds     = EditorGUILayout.Toggle("ids",         ids);
                settings.hierarchyMargin        = EditorGUILayout.IntField("margin",    margin, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                if (ids             != settings.hierarchyEntityIds ||
                    hiComponents    != settings.hierarchyComponents ||
                    hiTags          != settings.hierarchyTags ||
                    margin          != settings.hierarchyMargin)
                { 
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
        
        private void InspectorSettings()
        {
            expandInspector = EditorGUILayout.Foldout(expandInspector, "Inspector", true);
            if (expandInspector)
            {
                EditorGUILayout.BeginVertical();
                var inComponents    = settings.inspectorComponents;
                var inTags          = settings.inspectorTags;
                var link            = settings.inspectorGameObjectLink;
                settings.inspectorTags              = EditorGUILayout.Toggle("tags",            inTags); 
                settings.inspectorComponents        = EditorGUILayout.Toggle("components",      inComponents);
                settings.inspectorGameObjectLink    = EditorGUILayout.Toggle("GameObjectLink",  link);
                EditorGUILayout.EndVertical();

                if (inComponents    != settings.inspectorComponents ||
                    inTags          != settings.inspectorTags       ||
                    link            != settings.inspectorGameObjectLink)
                {
                    EditorUtils.RepaintInspector();
                }
            }
        }
        
        private static void Footer()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ECS documentation", GUILayout.Width(130));
            if (EditorGUILayout.LinkButton("Friflo.Engine.ECS · GitHub")) {
                Application.OpenURL("https://github.com/friflo/Friflo.Engine.ECS");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            var rect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copyright \u00a9 Ullrich Praetz\nhttps://github.com/friflo", GUILayout.Height(30));
            GUI.Label(new Rect(rect.x + 160, rect.y + 2, 25, 25), Logo);
            EditorGUILayout.EndHorizontal();
        }
    }
}