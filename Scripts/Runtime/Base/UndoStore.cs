// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    internal class UndoStore
    {
        internal static void RecordComponent      (MonoBehaviour behaviour, string name) => Instance.OnRecordComponent(behaviour, name);
        internal static void RecordEntityChanges  (ECSEntity ecsEntity, string name, in ComponentTypes components, in Tags tags)
            => Instance.OnRecordEntityChanges  (ecsEntity, name, components, tags);
        
        internal static void RegisterCreatedObject(GameObject gameObject, string name)  => Instance.OnRegisterCreatedObject(gameObject, name);
        internal static void DestroyLinkObject    (GameObject gameObject)               => Instance.OnDestroyLinkObject    (gameObject);
        
        internal static string GetOperationName<T>(T operation) {
#if UNITY_EDITOR
            return operation.ToString();
#else
            return null;
#endif
        }
            
        private static readonly UndoStore Instance
#if UNITY_EDITOR
        = new UndoStoreEditor();
#else
        = new UndoStore();
#endif
        protected virtual void OnRecordComponent      (MonoBehaviour behaviour, string name) { }
        protected virtual void OnRecordEntityChanges  (ECSEntity ecsEntity, string name, in ComponentTypes components, in Tags tags) { }
        protected virtual void OnRegisterCreatedObject(GameObject gameObject, string name) { }
        protected virtual void OnDestroyLinkObject    (GameObject gameObject) {
            // Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(gameObject);
        }
    }
    
#if UNITY_EDITOR
    internal class UndoStoreEditor : UndoStore
    {
        // stores an entity per Undo group having changes on a synchronized component: position, scale & rotation
        private static readonly Dictionary<int, UndoChange> GroupChanges = new ();
        private static readonly ComponentTypes              SynchronizedComponents  = ComponentTypes.Get<Position, Scale3, RotationEuler>();
        private static readonly Tags                        SynchronizedTags        = Tags.Get<Disabled>();
        
        internal UndoStoreEditor() {
            Undo.undoRedoEvent += OnUndoRedoEvent;
            EditorSceneManager.sceneClosed += scene => {
                GroupChanges.Clear();
            };
        }
        
        protected override void OnRecordComponent      (MonoBehaviour component, string name) {
            Undo.RecordObject (component, name);
            GroupChanges[Undo.GetCurrentGroup()] = new UndoChange(default, component);
        }
        
        protected override void OnRecordEntityChanges(ECSEntity ecsEntity, string name, in ComponentTypes components, in Tags tags)
        {
            // Debug.Log($"Record - {Undo.GetCurrentGroup()} - {name}");
            Undo.RecordObject (ecsEntity, name);
            UpdateEditor();
            var hasSynchronizedComponent = components.HasAny(SynchronizedComponents) || tags.HasAny(SynchronizedTags);
            if (hasSynchronizedComponent) {
                GroupChanges[Undo.GetCurrentGroup()] = new UndoChange(ecsEntity.Entity, null);
            }
        }
        
        protected override void OnRegisterCreatedObject(GameObject gameObject, string name) {
            Undo.RegisterCreatedObjectUndo(gameObject, name);
            UpdateEditor();
        }
        
        protected override void OnDestroyLinkObject (GameObject gameObject) {
            Undo.DestroyObjectImmediate(gameObject);
            UpdateEditor();
        }
        
        private static void UpdateEditor()
        {
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
        
        /// <summary>
        /// In case synchronizing <see cref="GameObject.transform"/> with Position, Scale3 and RotationEuler
        /// Undo/Redo changes of these components need to be copied to <see cref="GameObject.transform"/>.
        /// </summary>
        private static void OnUndoRedoEvent(in UndoRedoInfo undo)
        {
            // Debug.Log($"OnUndoRedoEvent: {undo.undoName}");
            if (!GroupChanges.TryGetValue(undo.undoGroup, out var change)) {
                // Debug.Log($"Undo not found: {undo.undoName}");
                return;
            }
            if (change.component != null) {
                if (change.component is ECSSystemSet ecsSystemSet) {
                    ecsSystemSet.ReadEcsSystems();
                }
                return;
            }
            if (change.entity.IsNull) {
                return;
            }
            var entity = change.entity;
            if (entity.IsNull) {
                // Debug.Log($"Undo - entity not found. id: {entity.Id}  undo: {undo.undoName}");
                return;
            }
            if (!entity.TryGetComponent(out GameObjectLink gameObjectLink)) {
                Debug.Log($"Undo - entity has no {nameof(GameObjectLink)}: id: {entity.Id}  undo: {undo.undoName}");
                return;
            }
            // --- Apply ECSEntity.components to entity
            var ecsEntity = gameObjectLink.EcsEntity;
            ECSEntitySerializer.Instance.WriteLinkComponentsToEntity(ecsEntity);
            
            // --- copy synchronized components from entity to transform: position, scale, rotation
            var transform  = gameObjectLink.transform;
            if (entity.TryGetComponent<Position>(out var pos)) {
                transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
            }
            if (entity.TryGetComponent<Scale3>(out var scale3)) {
                transform.localScale = new Vector3(scale3.x, scale3.y, scale3.z);
            }
            if (entity.TryGetComponent<RotationEuler>(out var rot)) {
                transform.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);
            }
            var disabled = entity.Tags.Has<Disabled>();
            gameObjectLink.gameObject.SetActive(!disabled);
        }
    }
    
    internal readonly struct UndoChange
    {
        internal readonly Entity        entity;
        internal readonly MonoBehaviour component;
        
        internal UndoChange(Entity entity, MonoBehaviour component) {
            this.entity     = entity;
            this.component  = component;
        }
    }
#endif
    
}