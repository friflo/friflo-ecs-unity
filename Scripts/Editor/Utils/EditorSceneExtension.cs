// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.Unity;
using Friflo.Engine.Unity.Internal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    [InitializeOnLoad]
    internal static class EditorSceneExtension
    {
        private static void Log(string message) {
            // Debug.Log($"--- SceneLog - {message}");
        }
        
        static EditorSceneExtension()
        {
            // Required: sceneOpened is not called on Unity startup
            EditorApplication.delayCall += OnInitializeOnLoad;
            
            // This event is called before opening an existing Scene. Note: Is not called on Unity startup
            // EditorSceneManager.sceneOpening += OnSceneOpening;
            
            // This event is called after a Scene has been opened in the editor.
            EditorSceneManager.sceneOpening     += OnSceneOpening;
            EditorSceneManager.sceneOpened      += OnSceneOpened;
            EditorSceneManager.sceneClosing     += OnSceneClosing;
            EditorSceneManager.newSceneCreated  += OnNewSceneCreated;
            
            // Undo.undoRedoEvent                  += OnUndoRedoInfo;
            Undo.undoRedoPerformed              += OnUndoRedoPerformed;
            
            EditorApplication.update            += UpdateCallback;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            
            // EditorApplication.projectChanged += () => { Log("projectChanged"); };
            
            // AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }
        
        private static void OnInitializeOnLoad()
        {
            var isPlaying = EditorApplication.isPlaying;
            Log($"InitializeOnLoad. isPlaying: {isPlaying}");
        }
        
        private static void OnSceneOpening (string path, OpenSceneMode mode)
        {
            Log("sceneOpening");
        }
        
        private static void OnSceneOpened (Scene scene, OpenSceneMode mode) {
            Log("sceneOpened");
        }
        
        private static void OnSceneClosing (Scene scene, bool removingScene) {
            Log("sceneClosing");
        }
        
        private static void OnNewSceneCreated (Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            Log("newSceneCreated");
        }
        
        // private static void OnUndoRedoInfo(in UndoRedoInfo undo)
        private static void OnUndoRedoPerformed()
        {
            var ecsEntities = new List<ECSEntity>();
            SceneExtension.FindObjectsByTypeInScenes(ecsEntities);
            foreach (var ecsEntity in ecsEntities) {
                ecsEntity.UpdateEntityOwner();
            }
            Log($"undoRedoPerformed");
        }
        
        private static void OnPlayModeStateChange(PlayModeStateChange change)
        {
            Log($"playModeStateChanged: {change}");
            
            switch (change) {
                // --- EditMode
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    /* var scenes = SceneManager.GetAllScenes();
                    foreach (var scene in scenes) {
                        SceneExtension.SaveEntityStores(scene, StoreType.PlayMode);
                    } */
                    break;
                // --- PlayMode
                case PlayModeStateChange.EnteredPlayMode:
                    // store loaded in SceneExtension.OnSceneLoaded()
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private static void UpdateCallback ()
        {
            var ecsEntities = new List<ECSEntity>();
            SceneExtension.FindObjectsByTypeInScenes(ecsEntities);
            foreach (var ecsEntity in ecsEntities) {
                if (!ecsEntity.IsChanged) {
                    continue;
                }
                // Debug.Log("Undo / Redo");
                ECSEntitySerializer.Instance.WriteLinkComponentsToEntity(ecsEntity);
            }
        }
    }
}