// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Hub;
using Friflo.Engine.UnityEditor;
using Friflo.Json.Fliox;
using UnityEngine;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {
    
public class StoreContext
{
#region properties
    internal            bool            HasAddedEntityLink  => addedEntityLinkId != 0;
    internal            bool            CreatingGameObjects => creatingGameObjects;
    public              EntityStore     EntityStore         => entityStore ??= CreateDefaultStore();
    public override     string          ToString()          => $"name: {ecsStore.StoreName}";
    #endregion
    
#region fields 
    private             EntityStore             entityStore;
    private             StoreSync               storeSync;
    private  readonly   StoreClient             storeClient;
    private  readonly   MemoryDatabase          database;
    internal readonly   ECSStore                ecsStore;
    
    /// Store the individual entity id for each GameObject per store.
    /// Drag/Drop or Redo/Undo may move an entity to a different store which assigned a different entity id.
    internal readonly   Dictionary<int, int>    linkIds = new ();
    
    /// <summary>
    /// Used by <see cref="ECSEntity.Awake"/> to check if a <see cref="ECSEntity"/> was added by
    /// <see cref="GameObject.AddComponent{T}"/> or Instantiate().
    /// </summary>
    private             int                     addedEntityLinkId;
    private             bool                    editorDeleted;
    private             bool                    creatingGameObjects;
    
    private static SystemRoot    WriteGameObjects;
    #endregion

    
#region init
    public StoreContext(ECSStore ecsStore, bool enableORM)
    {
        this.ecsStore   = ecsStore;

        // var database = new FileDatabase("test", "test_db") { Pretty = false };
        var storeName   = ecsStore.StoreName;
        database        = new MemoryDatabase(storeName) {
            Pretty          = false,
            ContainerType   = MemoryType.NonConcurrent
        };
        if (enableORM)
        {
            var hub         = new FlioxHub(database);
            storeClient     = new StoreClient(hub);
        }
    }
    
    internal void SetEntityStore(EntityStore store)
    {
        if (entityStore != null)    throw new InvalidOperationException($"Can use SetEntityStore() only on a newly created {nameof(ECSStore)}");
        if (store == null)          throw new ArgumentNullException(nameof(store));
        SetStoreInternal(store);

        creatingGameObjects = true;
        foreach (var entity in store.Entities)
        {
            if (!entity.TryGetComponent(out GameObjectLink link)) {
                continue;
            }
            var go          = GetOrCreateGameObject(entity, link.gameObject, out _);
            var ecsEntity   = go.GetComponent<ECSEntity>();
            ecsEntity.UpdateComponents(entity.Id);
            ECSEntitySerializer.Instance.ReadLinkComponentsFromEntity(ecsEntity, entity);
        }
        creatingGameObjects = false;
        
        // --- sync entities to game objects
        if (WriteGameObjects == null) { 
            WriteGameObjects = new SystemRoot("Write");
            var writeGameObjects = UnityGroupUtils.WriteGameObjects();
            WriteGameObjects.Add(writeGameObjects);
        }
        var write = WriteGameObjects;
        write.AddStore(entityStore);
        write.Update(default);
        write.RemoveStore(entityStore);
    }
    
    private EntityStore CreateDefaultStore()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        SetStoreInternal(store);
        return store;
    }
    
    private void SetStoreInternal(EntityStore store)
    {
        entityStore = store;
        if (storeClient != null)
            storeSync   = new StoreSync(store, storeClient);
        
        store.OnComponentAdded    += OnComponentChanged;
        store.OnComponentRemoved  += OnComponentChanged;
        store.OnTagsChanged       += OnTagsChanged;
        store.OnEntityCreate      += OnEntityCreate;
        store.OnEntityDelete      += OnEntityDelete;
    }
    #endregion
    
    
    
#region component changed
    private void OnComponentChanged(ComponentChanged changed)
    {
        // should early out after PostSyncChanges();
        if (changed.ComponentType.Type == typeof(GameObjectLink)) {
            return;
        }
        var entity = changed.Entity;
        storeSync?.UpsertDataEntity(entity.Pid);
        PostSyncChanges();

        if (!entity.TryGetComponent<GameObjectLink>(out var link)) {
            return;
        }
        var componentType = new ComponentTypes(changed.ComponentType);
        UndoStore.RecordEntityChanges(link.EcsEntity, UndoStore.GetOperationName(changed), componentType, default);
        ECSEntitySerializer.Instance.ReadLinkComponentsFromEntity(link.EcsEntity, entity);
        
        switch (changed.Action)
        {
            case ComponentChangedAction.Update:
            case ComponentChangedAction.Add:
                var type = changed.ComponentType.Type;
                if (type == typeof(Position)) {
                    var pos = changed.Component<Position>();
                    link.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
                }
                if (type == typeof(Scale3)) {
                    var scale = changed.Component<Scale3>();
                    link.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
                }
                if (type == typeof(RotationEuler)) {
                    var rot = changed.Component<RotationEuler>();
                    link.transform.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);
                }
                break;
        }
    }
    #endregion
    
#region tag changed
    private void OnTagsChanged(TagsChanged changed)
    {
        var entity = changed.Entity;
        storeSync?.UpsertDataEntity(entity.Pid);
        PostSyncChanges();
        
        if (!entity.TryGetComponent<GameObjectLink>(out var link)) {
            return;
        }
        var changedTags = changed.ChangedTags;
        UndoStore.RecordEntityChanges(link.EcsEntity, UndoStore.GetOperationName(changed), default, changedTags);
        ECSEntitySerializer.Instance.ReadLinkComponentsFromEntity(link.EcsEntity, entity);
        
        if (changedTags.Has<Disabled>()) {
            link.gameObject.SetActive(changed.Entity.Enabled);
        }
    }
    #endregion
    
    
    
#region create entity
    /// <summary> Will call <see cref="GetAddedEntity"/> </summary>
    private void OnEntityCreate(EntityCreate create)
    {
        var entity = create.Entity;
        storeSync?.UpsertDataEntity(entity.Pid);
        PostSyncChanges();
        
        if (!entity.TryGetComponent(out GameObjectLink link)) {
            // case:    entity has no GameObjectLink    
            return;
        }
        if (addedEntityLinkId != 0) throw new InvalidOperationException("Expect GameObject.AddComponent<>() is not called recursive"); 
        addedEntityLinkId = entity.Id;
        var go = GetOrCreateGameObject(entity, link.gameObject, out bool newGo);
        addedEntityLinkId = 0;
        if (newGo) {
            // Must register only self created GameObject's in Undo list.
            // Passed GameObject's must be registered where they are Instantiate()'ed if Undo/Redo shall be supported.
            UndoStore.RegisterCreatedObject(go, UndoStore.GetOperationName(create));
        }
    }
    
    /// <summary>
    /// Uses the passed GameObject or create a new GameObject if null
    /// adds an <see cref="ECSEntity"/> if missing and
    /// set <see cref="GameObjectLink.gameObject"/> to the GameObject.
    /// </summary>
    private GameObject GetOrCreateGameObject(Entity entity, GameObject go, out bool newGo)
    {
        if (go == null) {
            newGo = true;
            go = CreateGameObject(entity);
        } else {
            newGo = false;
        }
        var ecsEntity = go.GetComponent<ECSEntity>();
        if (ecsEntity == null) {
            // AddComponent() calls ECSEntity.Awake() only if GameObject is active.
            go.AddComponent<ECSEntity>();
        }
        // Call ECSEntity.Awake() manually see comment above.
        if (!go.activeSelf) {
            ecsEntity.Awake();
        }
        return go;
    }
    
    private GameObject CreateGameObject(Entity entity)
    {
        GameObject go;
        // Case:        GameObjectLink.gameObject == null
        //    =>        Create GameObject (from prefab or new GameObject) with ECSEntity and assign it to GameObjectLink
        if (ecsStore.defaultObject == null) {
            go = new GameObject("entity");
            go.transform.parent = ecsStore.transform;
        } else {
            // Instantiate() calls ECSEntity.Awake() if having an EntityLink component
            go = UnityEngine.Object.Instantiate(ecsStore.defaultObject, ecsStore.transform);
            go.name = ecsStore.defaultObject.name + "'";
        }
        entity.GetComponent<GameObjectLink>() = new GameObjectLink(go);
        return go;
    }

    internal Entity GetAddedEntity()
    {
        var entity = entityStore.GetEntityById(addedEntityLinkId);
        addedEntityLinkId = 0;
        return entity;
    }
    
    internal Entity UnityCreateEntity(GameObject gameObject, int id)
    {
        var entity = entityStore.Batch()
            .Add(new GameObjectLink(gameObject))
            .CreateEntity(id);
        linkIds[gameObject.GetInstanceID()] = id;
        return entity; 
    }
    
    internal Entity UnityCreateEntity(GameObject gameObject)
    {
        var entity = entityStore.Batch()
            .Add(new GameObjectLink(gameObject))
            .CreateEntity();
        linkIds[gameObject.GetInstanceID()] = entity.Id;
        return entity;
    }
    #endregion
    
    
    
#region delete entity
    private void OnEntityDelete(EntityDelete delete)
    {
        var pid = delete.Entity.Pid;
        storeSync?.DeleteDataEntity(pid);
        PostSyncChanges();
        
        if (editorDeleted) {
            return;
        }
        if (!delete.Entity.TryGetComponent(out GameObjectLink link)) {
            return;
        }
        link.EcsEntity.storeContext = null;
        
        UndoStore.DestroyLinkObject(link.gameObject);
    }
    
    internal void UnityDeleteEntity(Entity entity)
    {
        editorDeleted = true;
        try {
            entity.DeleteEntity();
        }
        finally{
            editorDeleted = false;
        }
    }
    #endregion



#region load / save entity store 
    public void LoadEntityStore(GameObject gameObject)
    {
        var path = GetStorePath();
        Debug.Log($"LoadEntityStore: {path}");
        var jsonBytes   = File.ReadAllBytes(path);
        var json        = new JsonValue(jsonBytes);
        var reader      = new JsonDatabaseDumpReader();
        var result = reader.Read(json, database);
        if (result.error != null) {
            Debug.LogError(result.error); 
        }
    }
    
    public void SaveEntityStore()
    {
        var path = GetStorePath();
        Debug.Log($"SaveEntityStore: {path}"); 
        
        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder)) {
            Directory.CreateDirectory(folder);
        }

        // storeSync.StoreEntities();
        
        var memoryStream = new MemoryStream();
        database.WriteToStream(memoryStream);
        memoryStream.Position = 0;
        
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        memoryStream.WriteTo(fileStream);
    }
    
    public string GetStorePath()
    {
        return $"{ecsStore.StoreName}.json";
    }
    #endregion

    
#region schedule changes
    private bool syncChangesPending;
    
    private void PostSyncChanges() {
        if (syncChangesPending) {
            return;
        }
        if (storeSync != null)
        { 
            ecsStore.Invoke(nameof(ECSStore.ExecutePendingChanges), 0);
            syncChangesPending = true;
        }
        else
            syncChangesPending = false;
    }
    
    internal async void ExecutePendingChanges() {
        syncChangesPending = false;
        // Debug.Log("ExecutePendingChanges");
        if (storeSync != null)
            await storeSync.SyncChangesAsync();   
    }
    #endregion
}
}