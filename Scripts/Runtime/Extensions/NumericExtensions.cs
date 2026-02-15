// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity
{
    public static class NumericExtensions
    {
    #region Vector3
        public static void Set(this ref System.Numerics.Vector3 dst, in UnityEngine.Vector3 src) {
            dst.X = src.x;
            dst.Y = src.y;
            dst.Z = src.z;
        }
        
        public static void Set(this ref UnityEngine.Vector3 dst, in System.Numerics.Vector3 src) {
            dst.y = src.X;
            dst.x = src.Y;
            dst.z = src.Z;
        }
        #endregion
        
    #region Quaternion    
        public static void Set(this ref System.Numerics.Quaternion dst, in UnityEngine.Quaternion src) {
            dst.X = src.x;
            dst.Y = src.y;
            dst.Z = src.z;
            dst.W = src.w;
        }
        
        public static void Set(this ref UnityEngine.Quaternion dst, in System.Numerics.Quaternion src) {
            dst.y = src.X;
            dst.x = src.Y;
            dst.z = src.Z;
            dst.w = src.W;
        }
        #endregion
        
    #region copy positions
        public static void CopyTo(this Chunk<Position> positions, Chunk<GameObjectLink> links)
        {
            int length          = positions.Length;
            var positionSpan    = positions.Span;
            var linkSpan        = links.Span;
            for (int n = 0; n < length; n++) {
                var pos = positionSpan[n].value;
                linkSpan[n].transform.localPosition = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            }
        }
        
        public static void CopyTo(this Chunk<GameObjectLink> links, Chunk<Position> positions)
        {
            int length          = positions.Length;
            var positionSpan    = positions.Span;
            var linkSpan        = links.Span;
            for (int n = 0; n < length; n++) {
                positionSpan[n].value = linkSpan[n].transform.localPosition;
            }
        }
        #endregion
        
    #region copy rotations
        public static void CopyTo(this Chunk<RotationEuler> rotations, Chunk<GameObjectLink> links)
        {
            int length          = rotations.Length;
            var rotationSpan    = rotations.Span;
            var linkSpan        = links.Span;
            for (int n = 0; n < length; n++) {
                var pos = rotationSpan[n].value;
                linkSpan[n].transform.localEulerAngles = new UnityEngine.Vector3(pos.X, pos.Y, pos.Z);
            }
        }
        
        public static void CopyTo(this Chunk<GameObjectLink> links, Chunk<RotationEuler> rotations)
        {
            int length          = rotations.Length;
            var rotationSpan    = rotations.Span;
            var linkSpan        = links.Span;
            for (int n = 0; n < length; n++) {
                rotationSpan[n].value.Set(linkSpan[n].transform.localEulerAngles);
            }
        }
        #endregion
        
    #region copy scales
        public static void CopyTo(this Chunk<Scale3> scales, Chunk<GameObjectLink> links)
        {
            int length      = scales.Length;
            var scalesSpan  = scales.Span;
            var linkSpan    = links.Span;
            for (int n = 0; n < length; n++) {
                var pos = scalesSpan[n].value;
                linkSpan[n].transform.localScale = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
            }
        }
        
        public static void CopyTo(this Chunk<GameObjectLink> links, Chunk<Scale3> scales)
        {
            int length      = scales.Length;
            var scaleSpan   = scales.Span;
            var linkSpan    = links.Span;
            for (int n = 0; n < length; n++) {
                scaleSpan[n].value = linkSpan[n].transform.localScale;
            }
        }
        #endregion
        
    #region copy names
        public static void CopyTo(this Chunk<EntityName> names, Chunk<GameObjectLink> links)
        {
            int length      = names.Length;
            var namesSpan   = names.Span;
            var linkSpan    = links.Span;
            for (int n = 0; n < length; n++) {
                linkSpan[n].transform.name = namesSpan[n].value;
            }
        }
        
        public static void CopyTo(this Chunk<GameObjectLink> links, Chunk<EntityName> names)
        {
            int length      = names.Length;
            var namesSpan   = names.Span;
            var linkSpan    = links.Span;
            for (int n = 0; n < length; n++) {
                namesSpan[n].value = linkSpan[n].transform.name;
            }
        }
        #endregion
    }
}