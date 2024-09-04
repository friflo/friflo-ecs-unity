// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using UnityEditor;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    public class SyncEditor : SystemRoot, ISyncEditor
    {
        public SyncEditor(StoreContext context) : base ("SyncEditor")
        {
            Add(new SyncEditorDestroyed(context));
            Add(new SyncEditorPosition());
            Add(new SyncEditorRotationEuler());
            Add(new SyncEditorScale());
            //  Add(new SyncEditorName()); see class docs
            Add(new SyncEditorDisabled());
            Add(new SyncEditorEnabled());
        }
        
        public void Sync()
        {
            var detector = new TransformChangesDetector(Selection.activeObject);
            Update(default);
            
            if (!detector.HasChanges()) {
                return;
            }
            // Update ECSEntity (Position, Scale & RotationEuler) in Inspector caused by Transform changes in Scene window.
            EditorUtils.RepaintInspector();
            // EditorUtility.SetDirty(detector.gameObject);  Must not be called. The scene will be modified without Redo functionality.
        }
    }
}