// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using Friflo.Json.Fliox.Mapper.Map;
using UnityEditor.IMGUI.Controls;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    // [Serializable]
    //The TreeElement data class is extended to hold extra data, which you can show and edit in the front-end TreeView.
    internal class SystemRow : TreeViewItem
    {
        internal readonly   BaseSystem      system;
        internal readonly   PropField       field;
        internal readonly   bool            isLast;
        internal readonly   int             count;
        internal readonly   ECSSystemSet    systemSet;
        internal            string          controlName;
        
        public SystemRow (int id, int depth, string name, BaseSystem system, PropField field, bool isLast) : base (id, depth, name)
        {
            this.system = system;
            this.field  = field;
            this.isLast = isLast;
            this.count  = -1;
        }
        
        public SystemRow (int id, int depth, string name, ECSSystemSet systemSet, BaseSystem system, int count) : base (id, depth, name)
        {
            this.system     = system;
            this.systemSet  = systemSet;
            this.count      = count;
        }
    }
}