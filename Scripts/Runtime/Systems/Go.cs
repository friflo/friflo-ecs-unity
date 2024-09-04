// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public static class UnityGroupUtils
    {
        public static SystemGroup ReadGameObjects()
        {
            var group = new SystemGroup("Read GameObjects") {
                new ReadGoDisabled(),
                new ReadGoEnabled(),
                // new ReadGoName(),
                new ReadGoPosition(),
                new ReadGoRotation(),
                new ReadGoScale()
            };
            return group;
        }
        
        public static SystemGroup WriteGameObjects()
        {
            var group = new SystemGroup("Write GameObjects") {
                new WriteGoDisabled(),
                new WriteGoEnabled(),
                // new WriteGoName(),
                new WriteGoPosition(),
                new WriteGoRotation(),
                new WriteGoScale()
            };
            return group;
        }
    }
}