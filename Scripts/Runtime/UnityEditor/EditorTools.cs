// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using Friflo.Engine.Unity;

#if UNITY_EDITOR

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal interface ISyncEditor
    {
        public void Sync();
    }
    

    /// <summary>
    /// A single instance of this class is only available when compiling in UNITY_EDITOR mode
    /// </summary>
    internal abstract class EditorTools
    {
        internal static EditorTools Instance => _instance ??= GetInstance();
        
        private static EditorTools _instance;
        
        private static EditorTools GetInstance()
        {
            var assembly    = System.Reflection.Assembly.Load("Friflo.Engine.UnityEditor");
            var type        = assembly.GetType("Friflo.Engine.UnityEditor.EditorToolsInstance");
            return (EditorTools)Activator.CreateInstance(type); 
        }
        
        public abstract ISyncEditor CreateSyncEditor(StoreContext context);
    }
}

#endif
