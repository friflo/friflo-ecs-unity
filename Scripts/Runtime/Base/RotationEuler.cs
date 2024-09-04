// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS
{
    [ComponentKey("rot3")]
    [StructLayout(LayoutKind.Explicit)]
    public struct  RotationEuler : IComponent, IEquatable<RotationEuler>
    {
        [Browse(Never)]
        [Ignore]
        [FieldOffset (0)] public    Vector3     value;  // 12
        //
        /// <summary>Rotation on X axis in degree </summary>
        [FieldOffset (0)] public    float       x;      // (4)
        
        /// <summary>Rotation on Y axis in degree </summary>
        [FieldOffset (4)] public    float       y;      // (4)
        
        /// <summary>Rotation on Z axis in degree </summary>
        [FieldOffset (8)] public    float       z;      // (4)
    
        public readonly override string ToString() => $"{x}, {y}, {z}";
    
        public RotationEuler (float x, float y, float z) {
            value  = default; // TODO remove
            this.x = x;
            this.y = y;
            this.z = z;
        }
    
        public          bool    Equals      (RotationEuler other)                       => value == other.value;
        public static   bool    operator == (in RotationEuler p1, in RotationEuler p2)  => p1.value == p2.value;
        public static   bool    operator != (in RotationEuler p1, in RotationEuler p2)  => p1.value != p2.value;

        [ExcludeFromCodeCoverage] public override   int     GetHashCode()       => throw new NotImplementedException("to avoid boxing");
        [ExcludeFromCodeCoverage] public override   bool    Equals(object obj)  => throw new NotImplementedException("to avoid boxing");
    }
}