// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Friflo.Engine.ECS;
using Friflo.Engine.UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("Friflo.Engine.UnityEditor")]

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    /// <summary>
    /// Used to add a <see cref="CustomizationRegistry"/> with the given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblyCustomizationAttribute : Attribute {
        internal readonly Type registryType;
        
        public AssemblyCustomizationAttribute (Type registryType) {
            this.registryType = registryType;
        }
    }
    
    /// <summary>
    /// Enable registration of custom <see cref="ComponentDrawer"/> and <see cref="FieldDrawer"/> in the Editor Inspector.
    /// </summary>
    public abstract class CustomizationRegistry
    {
        protected abstract void InitRegistry();
        
        static CustomizationRegistry()
        {
            try {
                Init();
            }
            catch (Exception e ) {
                Debug.LogException(e);
            }
        }

        
#if UNITY_EDITOR
        internal static readonly Dictionary<Type, GuiEntityComponent>   CustomGuiEntityComponentMap = new ();
        internal static readonly Dictionary<Type, GuiComponentField>    CustomGuiComponentFieldMap  = new ();
        

        protected void AddComponentDrawer<T>(ComponentDrawer drawer) where T : struct, IComponent {
            CustomGuiEntityComponentMap[typeof(T)] = new(drawer);
        }
        
        protected void AddFieldDrawer<T>(FieldDrawer drawer) {
            CustomGuiComponentFieldMap[typeof(T)] = new(drawer);
        }
#endif
        
        private static void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try  { 
                    InitAssemblyRegistry(assembly);
                }
                catch (Exception e) {
                    Debug.LogError(e);
                }
            }
        }
        
        private static void InitAssemblyRegistry(Assembly assembly)
        {
            if (assembly.IsDynamic) {
                return;
            }
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyCustomizationAttribute), false);
            if (attributes.Length == 0) {
                return;
            }
            var registryType = ((AssemblyCustomizationAttribute)attributes[0]).registryType;
            if (registryType == null || !registryType.IsSubclassOf(typeof(CustomizationRegistry))) {
                Debug.LogError($"Invalid type for {nameof(AssemblyCustomizationAttribute)}. Expect sub class of {nameof(CustomizationRegistry)}. Was: {registryType}");
                return;
            }
            var registry = (CustomizationRegistry)Activator.CreateInstance(registryType);
            registry.InitRegistry();
        }
    }
}