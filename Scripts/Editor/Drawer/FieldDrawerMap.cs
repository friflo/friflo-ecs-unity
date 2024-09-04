// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using Friflo.Json.Fliox.Mapper.Map;
using UnityEditor;
using UnityEngine;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    internal static class FieldDrawerMap
    { 
        private static readonly    Dictionary<Type, GuiComponentField> Map;
        
        static FieldDrawerMap()
        {
            Map = CreateFieldDrawerMap();
            foreach (var pair in CustomizationRegistry.CustomGuiComponentFieldMap) {
                Map[pair.Key] = pair.Value;
            }
        }
        
        internal static void AddField(Entity entity, ComponentType componentType, object parent, Type fieldType, Var.Member member, string label)
        {
            if (Map.TryGetValue(fieldType, out var field)) {
                var context = new FieldContext(entity, componentType, parent, member, label);
                field.drawer(context);
            }
        }
        
        // ------------------------------------ Field Drawer ------------------------------------
        private static Dictionary<Type, GuiComponentField> CreateFieldDrawerMap()
        {
            var map = new Dictionary<Type, GuiComponentField>
            {
                // --- standard types
                { typeof(byte),         new(ByteFieldDrawer)         },
                { typeof(short),        new(Int16FieldDrawer)        },
                { typeof(int),          new(Int32FieldDrawer)        },
                { typeof(long),         new(Int64FieldDrawer)        },
                    
                { typeof(sbyte),        new(SByteFieldDrawer)        },
                { typeof(ushort),       new(UInt16FieldDrawer)       },
                { typeof(uint),         new(UInt32FieldDrawer)       },
                { typeof(ulong),        new(UInt64FieldDrawer)       },
                    
                { typeof(float),        new(Flt32FieldDrawer)        },
                { typeof(double),       new(Flt64FieldDrawer)        },
                { typeof(bool),         new(BoolFieldDrawer)         },
                { typeof(string),       new(StringFieldDrawer)       },
                    
                // --- Unity types  
                { typeof(Vector2),      new(Vector2FieldDrawer)      },
                { typeof(Vector3),      new(Vector3FieldDrawer)      },
                { typeof(Vector4),      new(Vector4FieldDrawer)      },
                { typeof(Vector2Int),   new(Vector2IntFieldDrawer)   },
                { typeof(Vector3Int),   new(Vector3IntFieldDrawer)   },
                
                { typeof(Bounds),       new(BoundsFieldDrawer)       },
                { typeof(BoundsInt),    new(BoundsIntFieldDrawer)    },
                { typeof(Rect),         new(RectFieldDrawer)         },
                { typeof(RectInt),      new(RectIntFieldDrawer)      }
            };
            return map;
        }
        
        // ------------------------------------- integer ------------------------------------- 
        private static void ByteFieldDrawer(in FieldContext context)
        {
            var result = (byte)EditorGUILayout.IntField(context.Label, context.Var.Int8);
            if (result == context.Var.Int8)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Int16FieldDrawer(in FieldContext context)
        {
            var result = (short)EditorGUILayout.IntField(context.Label, context.Var.Int16);
            if (result == context.Var.Int16)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Int32FieldDrawer(in FieldContext context)
        {
            var result = EditorGUILayout.IntField(context.Label, context.Var.Int32);
            if (result == context.Var.Int32)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Int64FieldDrawer(in FieldContext context)
        {
            var result = EditorGUILayout.LongField(context.Label, context.Var.Int64);
            if (result == context.Var.Int64)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void SByteFieldDrawer(in FieldContext context)
        {
            var result = (sbyte)EditorGUILayout.IntField(context.Label, context.Var.SInt8);
            if (result == context.Var.SInt8)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void UInt16FieldDrawer(in FieldContext context)
        {
            var result = (ushort)EditorGUILayout.IntField(context.Label, context.Var.UInt16);
            if (result == context.Var.UInt16)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static  void UInt32FieldDrawer(in FieldContext context)
        {
            var result = (uint)EditorGUILayout.LongField(context.Label, context.Var.UInt32);
            if (result == context.Var.UInt32)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void UInt64FieldDrawer(in FieldContext context)
        {
            var result = (ulong)EditorGUILayout.LongField(context.Label, (long)context.Var.UInt64);
            if (result == context.Var.UInt64)  return;
            context.ChangeEntity(new Var(result));
        }
        
        // --- float / double
        private static void Flt32FieldDrawer(in FieldContext context)
        {
            var result = EditorGUILayout.FloatField(context.Label, context.Var.Flt32);
            if (result == context.Var.Flt32)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Flt64FieldDrawer(in FieldContext context)
        {
            var result = EditorGUILayout.DoubleField(context.Label, context.Var.Flt64);
            if (result == context.Var.Flt64)  return;
            context.ChangeEntity(new Var(result));
        }
        
        // --- bool / string
        private static void BoolFieldDrawer(in FieldContext context)
        {
            var result = EditorGUILayout.Toggle(context.Label, context.Var.Bool);
            if (result == context.Var.Bool)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void StringFieldDrawer(in FieldContext context)
        {
            var result = EditorGUILayout.TextField(context.Label, context.Var.String);
            if (result == context.Var.String)  return;
            context.ChangeEntity(new Var(result));
        }
        
        // ----------------------------------- Unity types -----------------------------------
        private static void Vector2FieldDrawer(in FieldContext context)
        {
            var value = (Vector2)context.Var.Object;
            var result = EditorGUILayout.Vector2Field(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Vector3FieldDrawer(in FieldContext context)
        {
            var value = (Vector3)context.Var.Object;
            var result = EditorGUILayout.Vector3Field(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Vector4FieldDrawer(in FieldContext context)
        {
            var value = (Vector4)context.Var.Object;
            var result = EditorGUILayout.Vector4Field(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Vector2IntFieldDrawer(in FieldContext context)
        {
            var value = (Vector2Int)context.Var.Object;
            var result = EditorGUILayout.Vector2IntField(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void Vector3IntFieldDrawer(in FieldContext context)
        {
            var value = (Vector3Int)context.Var.Object;
            var result = EditorGUILayout.Vector3IntField(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void BoundsFieldDrawer(in FieldContext context)
        {
            var value = (Bounds)context.Var.Object;
            var result = EditorGUILayout.BoundsField(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void BoundsIntFieldDrawer(in FieldContext context)
        {
            var value = (BoundsInt)context.Var.Object;
            var result = EditorGUILayout.BoundsIntField(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void RectFieldDrawer(in FieldContext context)
        {
            var value = (Rect)context.Var.Object;
            var result = EditorGUILayout.RectField(context.Label, value);
            if (result == value)  return;
            context.ChangeEntity(new Var(result));
        }
        
        private static void RectIntFieldDrawer(in FieldContext context)
        {
            var value = (RectInt)context.Var.Object;
            var result = EditorGUILayout.RectIntField(context.Label, value);
            if (result.Equals(value)) return;
            context.ChangeEntity(new Var(result));
        }
    }
}