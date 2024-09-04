// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using UnityEngine;

#if UNITY_EDITOR

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    /// <summary>
    /// Draws a component in Inspector > Entity Link.
    /// </summary>
    public delegate Rect ComponentDrawer(in ComponentContext context);
    
    public readonly struct GuiEntityComponent
    {
        public readonly ComponentDrawer   drawer;
        
        public GuiEntityComponent(ComponentDrawer drawer) {
            this.drawer = drawer;
        }
    }
        
    /// <summary>
    /// Provide component data used for an <see cref="ComponentDrawer"/> in Inspector > Entity Link.
    /// </summary>
    public readonly struct ComponentContext
    {
        public readonly     Entity          Entity;
        public readonly     ComponentType   Type;
        public readonly     object          Value;
        public readonly     string          Label;
        public readonly     GuiStyles       Styles;
        
        public ComponentContext(Entity entity, ComponentType type, object value, GuiStyles styles) {
            Entity      = entity;
            Type        = type;
            Label       = type.Name;
            Value       = value;
            Styles      = styles;
        }
    }
}

#endif