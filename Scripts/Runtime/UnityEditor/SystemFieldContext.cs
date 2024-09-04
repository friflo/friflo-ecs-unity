// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Systems;
using Friflo.Json.Fliox.Mapper.Map;
using UnityEngine;

#if UNITY_EDITOR

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    public delegate void SystemFieldDrawer(in SystemFieldContext context);
    
    public readonly struct GuiSystemField
    {
        public readonly SystemFieldDrawer   drawer;
        public readonly int                 height;
        
        public GuiSystemField(SystemFieldDrawer drawer, int height) {
            this.drawer = drawer;
            this.height = height;
        }
    }
    
    public readonly struct SystemFieldContext
    {
        public  readonly    Var             Var;
        public  readonly    string          Label;
        public  readonly    Rect            Rect;
        private readonly    BaseSystem      System;
        private readonly    Var.Member      Member;
        private readonly    VarType         MemberType;
        
        
        internal SystemFieldContext(in Rect rect, BaseSystem system, Var.Member member, VarType memberType, string label)
        {
            Rect        = rect;
            System      = system;
            Member      = member;
            MemberType  = memberType;
            Var         = member.GetVar(system);
            Label       = label;
        }
        
        public void ChangeField (Var var)
        {
            Member.SetVar(System, var);
            var value = MemberType.ToObject(var);
            // Send event. See: SEND_EVENT notes
            System.CastSystemUpdate(Label, value);
        }
    }
}

#endif