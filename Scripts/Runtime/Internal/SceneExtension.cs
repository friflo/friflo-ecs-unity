// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal struct EntityData
{
    internal string[]   components;
    internal short      version;
}

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity.Internal
{
    [InitializeOnLoad]
    internal class SceneExtension
    {
        private  readonly   Dictionary<ECSEntity, EntityData>  entityDataMap = new ();
        
        private  static readonly    SceneExtension Instance;

        
        static SceneExtension() {
            Instance = new SceneExtension();
            CreateEntities();
            
            // This event is called before a Scene is saved disk after you have requested the Scene to be saved.
            EditorSceneManager.sceneSaving      += OnSceneSaving;
            EditorSceneManager.sceneSaved       += OnSceneSaved;
        }
        
        private static void CreateEntities()
        {
            var ecsStores = new List<ECSStore>(); 
            FindObjectsByTypeInScenes(ecsStores);
            foreach (var storeComponent in ecsStores) {
                storeComponent.CreateEntities(); 
            }
        }
        
        private static void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            Debug.Log("SceneManager.sceneLoaded"); 
        }
        
        // use instead of static Object.FindObjectsOfType();
        private static void FindObjectsByTypeInScene<T>(Scene scene, List<T> list)
        {
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                T[] components = root.GetComponentsInChildren<T>(true); // true: includeInactive
                list.AddRange(components);
            }
        }
        
        internal static void FindObjectsByTypeInScenes<T>(List<T> list)
        {
            var sceneCount = SceneManager.sceneCount;
            for (int n = 0; n < sceneCount; n++)
            {
                var scene = SceneManager.GetSceneAt(n);
                FindObjectsByTypeInScene(scene, list);
            }
        }
        
        private static void OnSceneSaving(Scene scene, string path)
        {
            Debug.Log("sceneSaving");
            Instance.ClearEntityLinkComponents(scene);
            SaveEntityStores(scene);
        }
        
        private static void OnSceneSaved(Scene scene)
        {
            Debug.Log("sceneSaved");
            Instance.RestoreEntityLinkComponents(scene);
        }
        
        private static void SaveEntityStores(Scene scene)
        {
            var ecsStores = new List<ECSStore>();
            FindObjectsByTypeInScene(scene, ecsStores);
            foreach (var storeComponent in ecsStores) {
                storeComponent.StoreContext.SaveEntityStore();
            }
        }
        
        private void ClearEntityLinkComponents(Scene scene)
        {
            var map = entityDataMap;
            map.Clear();
            var ecsEntities = new List<ECSEntity>(); 
            FindObjectsByTypeInScene(scene, ecsEntities);
            foreach (var ecsEntity in ecsEntities) {
                map[ecsEntity] = new EntityData { components = ecsEntity.components, version = ecsEntity.version };
                // ecsEntity.components   = null;
                ecsEntity.version      = 0;
            }
        }
        
        private void RestoreEntityLinkComponents(Scene scene)
        {
            var map = entityDataMap;
            foreach (var (ecsEntity, data) in map) {
                // ecsEntity.components = data.components;
                ecsEntity.version    = data.version;
            }
            map.Clear();
        }
    }
}

#endif
