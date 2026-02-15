using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Friflo.Engine.Unity
{
    [ComponentKey("pos")]
    [StructLayout(LayoutKind.Explicit)]
    [ComponentSymbol("P", "0, 170, 0")]
    public struct Position : IComponent, IEquatable<Position>
    {
        [Ignore]
        [FieldOffset(0)] public Vector3 value;  // 12
                                                //
        [FieldOffset(0)] public float x;      // (4)
        [FieldOffset(4)] public float y;      // (4)
        [FieldOffset(8)] public float z;      // (4)

        public readonly override string ToString() => $"{x}, {y}, {z}";

        public Position(float x, float y, float z)
        {
            value = new Vector3(x, y, z);
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Position(Vector3 vector)
        {
            value = vector;
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public bool Equals(Position other) => value == other.value;
        public static bool operator ==(in Position p1, in Position p2) => p1.value == p2.value;
        public static bool operator !=(in Position p1, in Position p2) => p1.value != p2.value;

        [ExcludeFromCodeCoverage] public override int GetHashCode() => throw new NotImplementedException("to avoid boxing");
        [ExcludeFromCodeCoverage] public override bool Equals(object obj) => throw new NotImplementedException("to avoid boxing");
    }
}
