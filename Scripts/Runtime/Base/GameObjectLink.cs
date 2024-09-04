// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {
    
    [ComponentKey(null)]
    [ComponentSymbol("\u2934", "100,100,100")]
    public readonly struct GameObjectLink : IComponent
    {
        [Ignore] public readonly    UnityEngine.Transform   transform;
        [Ignore] public readonly    GameObject              gameObject; // storing gameObject is redundant but avoids additional dereferencing
        [Ignore] public             ECSEntity               EcsEntity  => gameObject.GetComponent<ECSEntity>();

        public override string ToString() => gameObject == null ? "null" : gameObject.ToString();

        public GameObjectLink(GameObject gameObject) {
            this.gameObject = gameObject;
            transform       = gameObject.transform;
        }
    }
}