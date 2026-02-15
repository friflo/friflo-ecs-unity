using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Friflo.Engine.Unity
{
    [ComponentKey("trans")]
    [StructLayout(LayoutKind.Explicit)]
    public struct Transform : IComponent
    {
        [Ignore]
        [FieldOffset(0)] public Matrix4x4 value;  // 64

        // --- 1st row
        [FieldOffset(0)] public float m00;
        [FieldOffset(4)] public float m10;
        [FieldOffset(8)] public float m20;
        [FieldOffset(12)] public float m30;
        // --- 2nd row    
        [FieldOffset(16)] public float m01;
        [FieldOffset(20)] public float m11;
        [FieldOffset(24)] public float m21;
        [FieldOffset(28)] public float m31;
        // --- 3rd row
        [FieldOffset(32)] public float m02;
        [FieldOffset(36)] public float m12;
        [FieldOffset(40)] public float m22;
        [FieldOffset(44)] public float m32;
        // --- 4th row
        [FieldOffset(48)] public float m03;
        [FieldOffset(52)] public float m13;
        [FieldOffset(56)] public float m23;
        [FieldOffset(60)] public float m33;
    }
}
