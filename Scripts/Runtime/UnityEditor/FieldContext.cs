// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Json.Fliox.Mapper.Map;

#if UNITY_EDITOR

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    /// <summary>
    /// Draws a component field in Inspector > Entity Link.
    /// </summary>
    public delegate void FieldDrawer(in FieldContext context);
    
    public readonly struct GuiComponentField
    {
        public readonly FieldDrawer   drawer;
        
        public GuiComponentField(FieldDrawer drawer) {
            this.drawer = drawer;
        }
    }
    
    /// <summary>
    /// Provide component field data used for an <see cref="FieldDrawer"/> in Inspector > Entity Link.
    /// </summary>
    public readonly struct FieldContext
    {
        public  readonly    Var             Var;
        public  readonly    string          Label;
        
        private readonly    Entity          Entity;
        private readonly    ComponentType   ComponentType;
        private readonly    object          Parent;
        private readonly    Var.Member      Member;
        
        internal FieldContext(Entity entity, ComponentType type, object parent, Var.Member member, string label)
        {
            Entity          = entity;
            ComponentType   = type;
            Member          = member;
            Var             = member.GetVar(parent);
            Parent          = parent;
            Label           = label;
        }
        
        public void ChangeEntity (Var var)
        {
            Member.SetVar(Parent, var);
            EntityUtils.AddEntityComponentValue(Entity, ComponentType, Parent);
        }
    }
}

#endif