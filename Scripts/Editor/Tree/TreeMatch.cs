// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal struct SystemTreeMatch
    {
        internal int            id;
        internal ECSSystemSet   systemSet;
        internal BaseSystem     system;
        internal int            depth;
        internal int            count;
    }
}