// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Json.Fliox.Mapper;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor.IMGUI.Controls;
#endif

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {


[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Scripts/Friflo/ECS System Set")]  // removes also (Script) suffix in Inspector
// ReSharper disable once InconsistentNaming
public class ECSSystemSet : MonoBehaviour
{
#region fields
    [NonSerialized]  public     SystemRoot      groupRoot;
    [NonSerialized]  private    SystemGroup     groupStart;
    [NonSerialized]  private    SystemGroup     groupUpdate;
    [NonSerialized]  private    SystemGroup     groupFixedUpdate;
    [NonSerialized]  private    SystemGroup     groupLateUpdate;

    [SerializeField] public     List<ECSStore>  ecsStores = new (0);
    [SerializeField] internal   List<ECSSystem> ecsSystems;
#if UNITY_EDITOR
    [NonSerialized]  internal   TreeViewState   treeViewState;
    [NonSerialized]  internal   int             treeViewFocusId  = -1;
    internal static readonly    HashSet<ECSSystemSet>   AllSystemSets = new ();
#endif
    #endregion
    
    private static readonly TypeStore       TypeStore       = new TypeStore();
    private static readonly ObjectMapper    ObjectMapper    = new ObjectMapper(TypeStore);
    
#region Init Systems
    private void Awake() {
        Init();
#if UNITY_EDITOR
        AllSystemSets.Add(this);
#endif
    }
    
    private void OnDestroy() {
#if UNITY_EDITOR
        AllSystemSets.Remove(this);
#endif
    }

    internal void Init()
    {
        if (groupRoot != null) return;
        ReadEcsSystems();
    }
    
    internal void ReadEcsSystems()
    {
        groupRoot = new SystemRoot("Systems");
        if (ecsSystems != null) {
            foreach (var ecsSystem in ecsSystems) {
                ReadSystem(groupRoot, ecsSystem);
            }
            var entityStore = GetComponent<ECSStore>().EntityStore;
            groupRoot.AddStore(entityStore);
            groupRoot.SetMonitorPerf(true);
        }
        groupRoot.OnSystemChanged += OnSystemChanged;
        SetLifecycleEvents();
    }
    
    private void SetLifecycleEvents() {
        var systems         = groupRoot; 
        groupStart          = systems.FindGroup("Start",        false);
        groupUpdate         = systems.FindGroup("Update",       false);
        groupFixedUpdate    = systems.FindGroup("FixedUpdate",  false);
        groupLateUpdate     = systems.FindGroup("LateUpdate",   false);
    }
    
    private void OnSystemChanged(SystemChanged changed)
    {
        // Debug.Log(changed);
        SetLifecycleEvents();
        
        // record subsequent component changes in Undo/Redo buffer
        UndoStore.RecordComponent(this, changed.ToString());
        
        // write changes in system hierarchy to serialized ecsSystems
        var ecsSystemRoot   = WriteSystems(groupRoot);
        ecsSystems          = ecsSystemRoot.systems;
    }
    #endregion

#region Execute System
    private void Start() {
        if (!Application.isPlaying)  return;
        var tick = new UpdateTick(Time.deltaTime, Time.time);
        groupStart?.        Update(tick);
    }

    private void Update() {
        if (!Application.isPlaying)  return;
        var tick = new UpdateTick(Time.deltaTime, Time.time);
        groupUpdate?.       Update(tick);
    }
    
    private void FixedUpdate() {
        if (!Application.isPlaying)  return;
        var tick = new UpdateTick(Time.fixedDeltaTime, Time.fixedTime);
        groupFixedUpdate?.  Update(tick);
    }
    
    private void LateUpdate() {
        if (!Application.isPlaying)  return;
        var tick = new UpdateTick(Time.deltaTime, Time.time);
        groupLateUpdate?.   Update(tick);
    }
    #endregion

#region Write Systems
    private static ECSSystemRoot WriteSystems(SystemRoot root)
    {
        var rootSystem = new ECSSystemRoot { systems = new List<ECSSystem>() };
        foreach (var childSystem in root.ChildSystems) {
            WriteSystem(childSystem, rootSystem, 0);
        }
        return rootSystem;
    }

    private static void WriteSystem(BaseSystem system, IECSSystem ecsSystem, int depth)
    {
        if (system is SystemGroup systemGroup) {
            var json = ObjectMapper.WriteObject(systemGroup);
            var type = systemGroup.GetType();
            //  var ecsGroup = new ECSSystem { type = $"{type.Namespace}.{type.Name}", systems = new List<ECSSystem>(), value = json };
            //  ecsSystem.systems.Add(ecsGroup);
            var ecsGroup = EcsSystemUtils.AddEcsSystem(ecsSystem, type, json, depth);
            foreach (var childSystem in systemGroup.ChildSystems) {
                WriteSystem(childSystem, ecsGroup, depth + 1);
            }
            return;
        }
        if (system != null) {
            var json = ObjectMapper.WriteObject(system);
            var type = system.GetType();
            //  var ecsQuery = new ECSSystem { type = $"{type.Namespace}.{type.Name}", value = json };
            //  ecsSystem.systems.Add(ecsQuery);
            EcsSystemUtils.AddEcsSystem(ecsSystem, type, json, depth);
        }
    }
    #endregion
    
#region Read Systems
    private static void ReadSystem(SystemGroup group, IECSSystem ecsSystem)
    {
        var type        = ecsSystem.SystemType;
        var systemType  = FindSystemType(type);
        if (systemType == null) {
            Debug.LogError($"System not found: {type}");
            return;
        }
        var system      = (BaseSystem)ObjectMapper.ReadObject(ecsSystem.Value, systemType);
        if (system is SystemGroup systemGroup)
        {
            group.Add(systemGroup);
            var systems = (IList)ecsSystem.Systems;
            foreach (var childSystem in systems) {
                ReadSystem(systemGroup, (IECSSystem)childSystem);
            }
            return;
        }
        group.Add(system);
    }
    
    private static Type FindSystemType(string type)
    {
        var map = SystemTypeRegistry.GetSystemTypeMap();
        // get system by namespace/class name
        if (map.TryGetValue(type, out var systemType)) {
            return systemType;
        }
        // find system only by class name
        var lastDot     = type.LastIndexOf(".", StringComparison.InvariantCulture);
        var className   = lastDot == -1 ? type : type.Substring(lastDot + 1);
        foreach (var pair in map) {
            if (pair.Value.Name == className) {
                return pair.Value;
            }
        }
        return null;
    }
    #endregion
}

}
