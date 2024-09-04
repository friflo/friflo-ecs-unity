// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Unity {

    internal static class SystemTypeRegistry
    {
        private static    Type[]                      _systemTypes;
        private static    Dictionary<string, Type>    _systemTypeMap;
        
    
    #region Register System Types
        internal static Dictionary<string, Type> GetSystemTypeMap()
        {
            if (_systemTypeMap != null) return _systemTypeMap;
            var map = new Dictionary<string, Type>();
            foreach (var type in GetSystemTypes()) {
                if (type.Namespace == null) {
                    map[$"{type.Name}"] = type; 
                    continue;
                }
                map[$"{type.Namespace}.{type.Name}"] = type;
            }
            return _systemTypeMap = map;
        }
        
        internal static Type[] GetSystemTypes()
        {
            if (_systemTypes != null) return _systemTypes;
            var schema = EntityStore.GetEntitySchema();
            var schemaTypes = new List<Type>();
            foreach (var dependant in schema.EngineDependants) {
                var types = dependant.Assembly.GetTypes();
                foreach (var type in types) {
                    if (type.IsGenericType) {
                        continue;
                    }
                    if (!type.IsSubclassOf(typeof(BaseSystem))) {
                        continue;
                    }
                    var flags =  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    if (type.GetConstructor(flags, null, Type.EmptyTypes, null) == null) {
                        continue;
                    }
                    schemaTypes.Add(type);
                }
            }
            var result = schemaTypes.ToArray();
            Array.Sort(result, new SystemComparer());
            return _systemTypes = result;
        }

        private class SystemComparer: IComparer<Type>
        {
            public int Compare(Type left, Type right) {
                var namespaceCompare = string.Compare(left!.Namespace, right!.Namespace, StringComparison.OrdinalIgnoreCase);
                if (namespaceCompare != 0) {
                    return namespaceCompare;
                }
                return string.Compare(left!.Name, right!.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
        #endregion
    }
}