// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.Unity;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal class EditorToolsInstance : EditorTools 
    {
        public override ISyncEditor CreateSyncEditor(StoreContext context) {
            var syncEditor = new SyncEditor(context);
            syncEditor.AddStore(context.EntityStore);
            return syncEditor;
        }
    }
}