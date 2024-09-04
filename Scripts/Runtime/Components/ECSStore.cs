// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.UnityEditor;
using UnityEngine;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [AddComponentMenu("Scripts/Friflo/ECS Store")]  // removes also (Script) suffix in Inspector
    // ReSharper disable once InconsistentNaming
    public class ECSStore : MonoBehaviour
    {
    #region public properties
                        // Never null
                        public  EntityStore     EntityStore     => StoreContext.EntityStore;
                        // Never null
                        public  StoreContext    StoreContext    => GetStoreContext();
                        public  string          StoreName       => name;
        #endregion
        
    #region fields
        [SerializeField]public  GameObject      defaultObject;
        [NonSerialized] private SystemGroup     systemGroup;                 
        [NonSerialized] private StoreContext    storeContext;
#if UNITY_EDITOR
        [NonSerialized] private ISyncEditor     syncEditor;
#endif
        #endregion

        public void SetEntityStore(EntityStore store)
        {
            storeContext.SetEntityStore(store);
        }

        private void Awake() {
            // Debug.Log("EntityStoreComponent.Awake");
            GetStoreContext();
        }

        // private void Start() { }

        // private void OnEnable() { }

        private void Update()
        {
            var store = EntityStore;
#if UNITY_EDITOR
            syncEditor ??= EditorTools.Instance.CreateSyncEditor(storeContext); 
            syncEditor.Sync();
#endif
        }
        
        internal StoreContext GetStoreContext() {
            if (storeContext != null) {
                return storeContext;
            }
            return storeContext = new StoreContext(this);
        }
        
        // TODO check exception on domain reload. Call in Awake()?
        internal void CreateEntities()
        {
            var entities = gameObject.GetComponentsInChildren<ECSEntity>(true); // true: includeInactive
            foreach (var ecsEntity in entities) {
                ecsEntity.CreateEntity();
            }
        }

        // private void OnDestroy() { }
        
        public void ExecutePendingChanges() {
            storeContext.ExecutePendingChanges();
        }
    }
}
