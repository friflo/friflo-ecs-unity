// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Systems;
using Friflo.Json.Fliox.Mapper.Map;
using UnityEditor;
using UnityEngine;
using Numerics = System.Numerics;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal static class SystemFieldDrawerMap
    { 
        internal static readonly    Dictionary<Type, GuiSystemField> Map;
        
        static SystemFieldDrawerMap()
        {
            Map = CreateFieldDrawerMap();
        }
        
        internal static void AddField(in Rect rect, BaseSystem system, Type fieldType, Var.Member member, VarType memberType, string label)
        {
            if (Map.TryGetValue(fieldType, out var systemField)) {
                var context = new SystemFieldContext(rect, system, member, memberType, label);
                systemField.drawer(context);
            }
        }
        
        // ------------------------------------ Field Drawer ------------------------------------
        private static Dictionary<Type, GuiSystemField> CreateFieldDrawerMap()
        {
            var map = new Dictionary<Type, GuiSystemField>
            {
                // --- standard types
                { typeof(byte),         new (ByteFieldDrawer,       20) },
                { typeof(short),        new (Int16FieldDrawer,      20) },
                { typeof(int),          new (Int32FieldDrawer,      20) },
                { typeof(long),         new (Int64FieldDrawer,      20) },
                    
                { typeof(sbyte),            new (SByteFieldDrawer,              20) },
                { typeof(ushort),           new (UInt16FieldDrawer,             20) },
                { typeof(uint),             new (UInt32FieldDrawer,             20) },
                { typeof(ulong),            new (UInt64FieldDrawer,             20) },
                    
                { typeof(float),            new (Flt32FieldDrawer,              20) },
                { typeof(double),           new (Flt64FieldDrawer,              20) },
                { typeof(bool),             new (BoolFieldDrawer,               20) },
                { typeof(string),           new (StringFieldDrawer,             20) },
                
                // --- System.Numerics
                { typeof(Numerics.Vector3), new (NumericsVector3FieldDrawer,    20) },
                    
                // --- Unity types  
                { typeof(Vector2),          new (Vector2FieldDrawer,            20) },
                { typeof(Vector3),          new (Vector3FieldDrawer,            20) },
                { typeof(Vector4),          new (Vector4FieldDrawer,            20) },
                { typeof(Vector2Int),       new (Vector2IntFieldDrawer,         20) },
                { typeof(Vector3Int),       new (Vector3IntFieldDrawer,         20) },
                
                { typeof(Bounds),           new (BoundsFieldDrawer,             60) },
                { typeof(BoundsInt),        new (BoundsIntFieldDrawer,          60) },
                { typeof(Rect),             new (RectFieldDrawer,               40) },
                { typeof(RectInt),          new (RectIntFieldDrawer,            40) }
            };
            return map;
        }
        
        // ------------------------------------- integer ------------------------------------- 
        private static void ByteFieldDrawer(in SystemFieldContext context)
        {
            var result = (byte)EditorGUI.IntField(context.Rect, context.Label, context.Var.Int8);
            if (result == context.Var.Int8)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Int16FieldDrawer(in SystemFieldContext context)
        {
            var result = (short)EditorGUI.IntField(context.Rect, context.Label, context.Var.Int16);
            if (result == context.Var.Int16)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Int32FieldDrawer(in SystemFieldContext context)
        {
            var result =  EditorGUI.IntField(context.Rect, context.Label, context.Var.Int32);
            if (result == context.Var.Int32)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Int64FieldDrawer(in SystemFieldContext context)
        {
            var result = EditorGUI.LongField(context.Rect, context.Label, context.Var.Int64);
            if (result == context.Var.Int64)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void SByteFieldDrawer(in SystemFieldContext context)
        {
            var result = (sbyte)EditorGUI.IntField(context.Rect, context.Label, context.Var.SInt8);
            if (result == context.Var.SInt8)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void UInt16FieldDrawer(in SystemFieldContext context)
        {
            var result = (ushort)EditorGUI.IntField(context.Rect, context.Label, context.Var.UInt16);
            if (result == context.Var.UInt16)  return;
            context.ChangeField(new Var(result));
        }
        
        private static  void UInt32FieldDrawer(in SystemFieldContext context)
        {
            var result = (uint)EditorGUI.LongField(context.Rect, context.Label, context.Var.UInt32);
            if (result == context.Var.UInt32)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void UInt64FieldDrawer(in SystemFieldContext context)
        {
            var result = (ulong)EditorGUI.LongField(context.Rect, context.Label, (long)context.Var.UInt64);
            if (result == context.Var.UInt64)  return;
            context.ChangeField(new Var(result));
        }
        
        // --- float / double
        private static void Flt32FieldDrawer(in SystemFieldContext context)
        {
            var result = EditorGUI.FloatField(context.Rect, context.Label, context.Var.Flt32);
            if (result == context.Var.Flt32)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Flt64FieldDrawer(in SystemFieldContext context)
        {
            var result = EditorGUI.DoubleField(context.Rect, context.Label, context.Var.Flt64);
            if (result == context.Var.Flt64)  return;
            context.ChangeField(new Var(result));
        }
        
        // --- bool / string
        private static void BoolFieldDrawer(in SystemFieldContext context)
        {
            var result = EditorGUI.Toggle(context.Rect, context.Label, context.Var.Bool);
            if (result == context.Var.Bool)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void StringFieldDrawer(in SystemFieldContext context)
        {
            var result = EditorGUI.TextField(context.Rect, context.Label, context.Var.String);
            if (result == context.Var.String)  return;
            context.ChangeField(new Var(result));
        }
        
        // --------------------------------- System.Numerics ---------------------------------
        private static void NumericsVector3FieldDrawer(in SystemFieldContext context)
        {
            var value   = (Numerics.Vector3)context.Var.Object;
            var v       = new Vector3(value.X, value.Y, value.Z);
            v           = EditorGUI.Vector3Field(context.Rect, context.Label, v);
            var result  = new Numerics.Vector3(v.x, v.y, v.z);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        // ----------------------------------- Unity types -----------------------------------
        private static void Vector2FieldDrawer(in SystemFieldContext context)
        {
            var value = (Vector2)context.Var.Object;
            var result = EditorGUI.Vector2Field(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Vector3FieldDrawer(in SystemFieldContext context)
        {
            var value = (Vector3)context.Var.Object;
            var result = EditorGUI.Vector3Field(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Vector4FieldDrawer(in SystemFieldContext context)
        {
            var value = (Vector4)context.Var.Object;
            var result = EditorGUI.Vector4Field(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Vector2IntFieldDrawer(in SystemFieldContext context)
        {
            var value = (Vector2Int)context.Var.Object;
            var result = EditorGUI.Vector2IntField(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void Vector3IntFieldDrawer(in SystemFieldContext context)
        {
            var value = (Vector3Int)context.Var.Object;
            var result = EditorGUI.Vector3IntField(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void BoundsFieldDrawer(in SystemFieldContext context)
        {
            var value = (Bounds)context.Var.Object;
            var result = EditorGUI.BoundsField(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void BoundsIntFieldDrawer(in SystemFieldContext context)
        {
            var value = (BoundsInt)context.Var.Object;
            var result = EditorGUI.BoundsIntField(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void RectFieldDrawer(in SystemFieldContext context)
        {
            var value = (Rect)context.Var.Object;
            var result = EditorGUI.RectField(context.Rect, context.Label, value);
            if (result == value)  return;
            context.ChangeField(new Var(result));
        }
        
        private static void RectIntFieldDrawer(in SystemFieldContext context)
        {
            var value = (RectInt)context.Var.Object;
            var result = EditorGUI.RectIntField(context.Rect, context.Label, value);
            if (result.Equals(value)) return;
            context.ChangeField(new Var(result));
        }
    }
}