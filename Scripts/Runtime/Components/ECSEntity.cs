// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Engine.UnityEditor;
using UnityEngine;
using Transform = UnityEngine.Transform;

// ReSharper disable CanSimplifyDictionaryTryGetValueWithGetValueOrDefault

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {


[DisallowMultipleComponent]
[ExecuteInEditMode]
[AddComponentMenu("Scripts/Friflo/ECS Entity")]  // removes also (Script) suffix in Inspector
// ReSharper disable once InconsistentNaming
public class ECSEntity : MonoBehaviour
{
#region properties
    public          int                     EntityId        => entityId;
    
    /// <summary> Can return a null entity - <see cref="ECS.Entity.IsNull"/> == true </summary>
    public          Entity                  Entity          => GetStoreContext()?.EntityStore.GetEntityById(entityId) ?? default;
    
    /// <summary> cn return null </summary>
    public          StoreContext            StoreContext    => GetStoreContext();
    
    public          ReadOnlySpan<string>    Components      => components;
    
    internal        bool                    IsChanged       => version != versionNew;

    public override string                  ToString()      => $"EntityId: {EntityId} gameObject: {gameObject}";
    #endregion

#region fields
    [NonSerialized]     internal    StoreContext    storeContext;
    [NonSerialized]     private     bool            entityCreated;
    [SerializeField]    private     int             entityId;
    /// <summary>version is serialized to scene file always as 0. Used only for Copy/Paste & Undo/Redo. </summary>
    [SerializeField]    internal    short           version;
    [SerializeField]    internal    string[]        components;
    [NonSerialized]     internal    short           versionNew;
    #endregion
    
    // can return null
    private StoreContext GetStoreContext()
    {
        if (storeContext != null) {
            return storeContext;
        }
        var storeComponent = GetParentStore(transform);
        if (storeComponent == null) {
            return null;
        }
        return storeContext = storeComponent.GetStoreContext();
    }
    
    // can return null
    internal static ECSStore GetParentStore(Transform transform)
    {
        do {
            var storeComponent = transform.GetComponent<ECSStore>();
            if (storeComponent != null) {
                return storeComponent;
            }
            transform = transform.parent;
        }
        while(transform != null);
        return null;
    }
    
    // Is not called on Redo/Undo even if parent was changed
    private void OnTransformParentChanged()
    {
        // Debug.Log("OnTransformParentChanged"); 
        UpdateEntityOwner();
    }

    internal void UpdateComponents(int id) {
        entityId        = id;
        entityCreated   = true;
    }
    
    internal void UpdateEntityOwner()
    {
        var context     = storeContext;
        var parentStore = GetParentStore(transform);
        if (context?.ecsStore == parentStore) {
            return;
        }
        if (context != null) {
            // remove entity from old entity store
            storeContext = null;
            var id = entityId;
            if (context.linkIds.TryGetValue(gameObject.GetInstanceID(), out int oldId)) {
                id = oldId;
            }
            var entity = context.EntityStore.GetEntityById(id);
            context.UnityDeleteEntity(entity);
        }
        CreateEntity();
    }
    
    // private void OnEnable() { Debug.Log($"OnEnable - {gameObject.name}"); }
    
    /// <summary>
    /// Note! Awake() is not called if owning <see cref="GameObject.activeSelf"/> is false aka disabled.<br/>
    /// Therefore, SyncEditorDestroyed is used to detect destroyed <see cref="ECSEntity"/>'s and delete their linked entity.
    /// </summary>
    internal void Awake()
    {
        // if (!SceneExtension.Instance.IsActive) { return; }
        if (entityCreated) {
            return;
        }
        CreateEntity();
    }
    
    internal void CreateEntity()
    {
        var context = GetStoreContext();
        if (context == null) {
            return;
        }
        if (context.CreatingGameObjects) {
            return;
        }
        var entity      = Create(context, out CreateType type);
        entityId        = entity.Id;
        entityCreated   = true;
        // Debug.Log($"ECSEntity.CreateEntity - type: {type}");
        ECSEntitySerializer.Instance.WriteLinkComponentsToEntity(this);
    }
    
    private Entity Create(StoreContext context, out CreateType type)
    {
        if (context.HasAddedEntityLink) {
            // case:    ECSEntity was added by Store.OnEntityCreate() - which is called by store.CreateEntity()
            type = CreateType.OnEntityCreate;
            return context.GetAddedEntity();
        }
        if (!context.linkIds.TryGetValue(gameObject.GetInstanceID(), out int id)) {
            id = entityId;
        }
        if (id == 0) {
            // case:    ECSEntity was added by 'Editor > Add Component' or gameObject.AddComponent<EntityLink>()
            type = CreateType.UnityAddComponent;
            return context.UnityCreateEntity(gameObject);
        }
        var entityStore = context.EntityStore;
        if (entityStore.TryGetEntityByPid(id, out var entity)) {
            // case:    ECSEntity was added by Object.Instantiate() or Editor Duplicate / Paste game object => create entity with new id
            type = CreateType.EditorDuplicatePaste;
            return context.UnityCreateEntity(gameObject);
        }
        // case:        ECSEntity was created via Unity Serialization - by load scene, Redo add or Undo delete
        type = CreateType.UnitySerialization;
        return context.UnityCreateEntity(gameObject, id);
    }

    /// <summary>
    /// Note! OnDestroy() not called if owning <see cref="GameObject.activeSelf"/> is false aka disabled.<br/>
    /// Therefore, SyncEditorDestroyed is used to detect destroyed <see cref="ECSEntity"/>'s and delete their linked entity.
    /// </summary>
    private void OnDestroy() {
        DeleteEntity();
    }
    
    private void DeleteEntity()
    {
        if (storeContext == null) {
            return;
        }
        var entity = Entity;
        storeContext.UnityDeleteEntity(entity);
    }
}

internal enum CreateType
{
    OnEntityCreate,
    UnityAddComponent,
    EditorDuplicatePaste,
    UnitySerialization
} 

}
