// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    
    [InitializeOnLoad]
    internal static class ExternalEditor
    {
        private static Dictionary<string, NamespaceType> EcsTypes;
        
        static ExternalEditor()
        {
            var folder = GetFrifloFolder();
            if (Directory.Exists(folder)) {
                return;
            }
            // Create .friflo folder in project root.
            // If folder is present Friflo.Engine.Analyzer.dll will add *.csv file with type locations  
            Directory.CreateDirectory(folder);
        }
        
        internal static bool OpenFileWithType(Type type)
        {
            var types = GetTypes();
            var typeName = type.Namespace == null ? type.Name : $"{type.Namespace}.{type.Name}";
            if (!types.TryGetValue(typeName, out var location)) {
                return false;
            }
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(location.file, location.line + 1, location.column + 1);
            // var assetPaths = AssetDatabase.GetAllAssetPaths();
            // var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            return true;
        }
        
        private static string GetFrifloFolder() {
            var folder  = Application.dataPath;
            return folder.Replace("/Assets", "") + "/.friflo";
        }
        
        private static Dictionary<string, NamespaceType> GetTypes()
        {
            if (EcsTypes != null) {
                return EcsTypes;
            }
            EcsTypes    = new Dictionary<string, NamespaceType>();
            var folder  = GetFrifloFolder();
            if (!Directory.Exists(folder)) {
                return EcsTypes;
            }
            var files = Directory.EnumerateFiles(folder, "*.csv");
            foreach (var file in files) {
                var csv = File.ReadAllText(file);
                AddTypes(csv);
            }
            return EcsTypes;
        }
        
        private static void AddTypes(string csv)
        {
            var types = EcsTypes;
            var csvLines = csv.Split('\n');
            foreach (var csvLine in csvLines)
            {
                var rows = csvLine.Split(',');
                if (rows.Length < 5) continue;
                var type    = rows[0];
                var file    = rows[2];
                int.TryParse (rows[3], out int line);
                int.TryParse (rows[4], out int column);
                types[type] = new NamespaceType { file = file, line = line, column = column };
            }
        }
        
        private struct NamespaceType
        {
            internal string     ns;
            internal string     name;
            internal string     file;
            internal int        line;
            internal int        column;
            internal EcsType    ecsType;
        }
        
        private enum EcsType {
            Component   = 1,
            Tag         = 2,
            Script      = 3,
            System      = 4,
        }
    }
    
    
}