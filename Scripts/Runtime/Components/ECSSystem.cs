// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS.Systems;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    /// <summary>
    /// interface <see cref="IECSSystem"/> and classes <see cref="ECSSystem1"/>, ..., <see cref="ECSSystem6"/>
    /// exists only to overcome compiler warning:<br/>
    /// Serialization depth limit 10 exceeded ... . There may be an object composition cycle in one or more of your serialized classes.<br/>
    /// The obvious class declaration is:
    /// </summary>
    /*
    [Serializable]
    public class ECSSystem {
        [SerializeField] internal   string          type;
        [SerializeField] internal   string          value;
        [SerializeField] internal   List<ECSSystem> systems;
    }
    */
    public interface IECSSystem
    {
        string              SystemType   { get; set; }
        string              Value        { get; set; }
        object              Systems      { get; set; }
    }
    
    public class ECSSystemRoot : IECSSystem
    {
        private    string              type;
        private    string              value;
        internal   List<ECSSystem>     systems;

        public override             string              ToString() => ECSSystem.GetString(type, systems);
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem>)value; }
    }
    
    [Serializable]
    public class ECSSystem : IECSSystem
    {
        /// <summary> Group Name or qualified Type name of a System</summary>
        [SerializeField] internal   string              type;
        /// <summary> Is serialized into the System of <see cref="Type"/> with the qualified <see cref="type"/> 
        /// if not a <see cref="SystemGroup"/> - <see cref="systems"/>.Count == 0.
        /// </summary>
        [SerializeField] internal   string              value;
        /// <summary> System is a <see cref="SystemGroup"/> if <see cref="systems"/>.Count > 0 </summary>
        [SerializeField] internal   List<ECSSystem1>    systems;

        public override             string              ToString() => GetString(type, systems);
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem1>)value; }
        
        internal static string GetString(string type, IList systems) {
            if (systems.Count > 0) {
                return $"{type} Group - systems: {systems.Count}";
            }
            return type;
        }
    }
    
    [Serializable]
    public class ECSSystem1 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem2>    systems;

        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem2>)value; }
    }
    
    [Serializable]
    public class ECSSystem2 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem3>    systems;
        public override             string              ToString() => ECSSystem.GetString(type, systems);
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem3>)value; }
    }
    
    [Serializable]
    public class ECSSystem3 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem4>    systems;
        public override             string              ToString() => ECSSystem.GetString(type, systems);
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem4>)value; }
    }
    
    [Serializable]
    public class ECSSystem4 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem5>    systems;
        public override             string              ToString() => ECSSystem.GetString(type, systems);
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem5>)value; }
    }
    
    [Serializable]
    public class ECSSystem5 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem6>    systems;
        public override             string              ToString() => ECSSystem.GetString(type, systems);
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem6>)value; }
    }
    
    [Serializable]
    public class ECSSystem6 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem7>    systems;
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem7>)value; }
    }
    
    [Serializable]
    public class ECSSystem7 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem8>    systems;
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem8>)value; }
    }
    
    [Serializable]
    public class ECSSystem8 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem9>    systems;
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem9>)value; }
    }
    
    [Serializable]
    public class ECSSystem9 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        [SerializeField] internal   List<ECSSystem10>    systems;
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => systems; set => systems    = (List<ECSSystem10>)value; }
    }
    
    [Serializable]
    public class ECSSystem10 : IECSSystem
    {
        [SerializeField] internal   string              type;
        [SerializeField] internal   string              value;
        
        public string               SystemType  { get => type;    set => type       = value; }
        public string               Value       { get => value;   set => this.value = value; }
        public object               Systems     { get => null;    set => throw new NotImplementedException(); }
    }
    
    
    // -------------------------------------------- utils --------------------------------------------
    internal static class EcsSystemUtils
    {
        internal static IECSSystem AddEcsSystem(IECSSystem ecsSystem, Type type, string json, int depth)
        {
            var typeName = type.Namespace == null ? type.Name : $"{type.Namespace}.{type.Name}";
            switch (depth) {
                case 0:
                    var s0 = new ECSSystem { type = typeName, value = json, systems = new List<ECSSystem1>() };
                    (ecsSystem.Systems as List<ECSSystem>)!.Add(s0);
                    return s0;
                case 1:
                    var s1 = new ECSSystem1 { type = typeName, value = json, systems = new List<ECSSystem2>() };
                    (ecsSystem.Systems as List<ECSSystem1>)!.Add(s1);
                    return s1;
                case 2:
                    var s2 = new ECSSystem2 { type = typeName, value = json, systems = new List<ECSSystem3>() };
                    (ecsSystem.Systems as List<ECSSystem2>)!.Add(s2);
                    return s2;
                case 3:
                    var s3 = new ECSSystem3 { type = typeName, value = json, systems = new List<ECSSystem4>() };
                    (ecsSystem.Systems as List<ECSSystem3>)!.Add(s3);
                    return s3;
                case 4:
                    var s4 = new ECSSystem4 { type = typeName, value = json, systems = new List<ECSSystem5>() };
                    (ecsSystem.Systems as List<ECSSystem4>)!.Add(s4);
                    return s4;
                case 5:
                    var s5 = new ECSSystem5 { type = typeName, value = json, systems = new List<ECSSystem6>() };
                    (ecsSystem.Systems as List<ECSSystem5>)!.Add(s5);
                    return s5;
                case 6:
                    var s6 = new ECSSystem6 { type = typeName, value = json, systems = new List<ECSSystem7>()  };
                    (ecsSystem.Systems as List<ECSSystem6>)!.Add(s6);
                    return s6;
                case 7:
                    var s7 = new ECSSystem7 { type = typeName, value = json, systems = new List<ECSSystem8>()  };
                    (ecsSystem.Systems as List<ECSSystem7>)!.Add(s7);
                    return s7;
                case 8:
                    var s8 = new ECSSystem8 { type = typeName, value = json, systems = new List<ECSSystem9>()  };
                    (ecsSystem.Systems as List<ECSSystem8>)!.Add(s8);
                    return s8;
                case 9:
                    var s9 = new ECSSystem9 { type = typeName, value = json, systems = new List<ECSSystem10>()  };
                    (ecsSystem.Systems as List<ECSSystem9>)!.Add(s9);
                    return s9;
                case 10:
                    var s10 = new ECSSystem10 { type = typeName, value = json };
                    (ecsSystem.Systems as List<ECSSystem10>)!.Add(s10);
                    return s10; 
            }
            return null;
        }
    }
}
