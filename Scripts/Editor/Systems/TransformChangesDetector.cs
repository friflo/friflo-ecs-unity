// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal readonly struct TransformChangesDetector
    {
        private  readonly   Position        oldPos;
        private  readonly   Scale3          oldScale;
        private  readonly   RotationEuler   oldRot;
        private  readonly   bool            hasPos;
        private  readonly   bool            hasScale;
        private  readonly   bool            hasRot;
        private  readonly   Entity          entity;
            
            
        internal TransformChangesDetector(Object obj)
        {
            entity      = default;
            hasPos      = false;
            hasScale    = false;
            hasRot      = false;
            oldPos      = default;
            oldScale    = default;
            oldRot      = default;
            
            var go = obj as GameObject;
            if (go == null) {
                return;
            }
            var ecsEntity = go.GetComponent<ECSEntity>();
            if (ecsEntity == null) {
                return;
            }
            entity = ecsEntity.Entity;
            if (entity.IsNull) {
                return;
            }
            hasPos      = entity.TryGetComponent(out oldPos);
            hasScale    = entity.TryGetComponent(out oldScale);
            hasRot      = entity.TryGetComponent(out oldRot);
        }
        
        internal bool HasChanges() {
            bool changed = false;
            if (hasPos) {
                changed |= entity.GetComponent<Position>().value != oldPos.value;
            }
            if (hasScale) {
                changed |= entity.GetComponent<Scale3>().value != oldScale.value;
            }
            if (hasRot) {
                changed |= entity.GetComponent<RotationEuler>().value != oldRot.value;
            }
            return changed;
        }
    }
}