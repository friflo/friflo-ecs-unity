// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.Unity;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    [CustomEditor(typeof(ECSHub))]
    // ReSharper disable InconsistentNaming
    internal sealed class ECSHubEditor : Editor
    {
        
        // private bool     showClient = true;
        private bool        showHost   = true;
        // private string   testString = "abc";
        
        SerializedProperty host;
        SerializedProperty port;
        // private GUIUtils    gui;
        
        void OnEnable()
        {
            host = serializedObject.FindProperty(nameof(ECSHub.host));
            port = serializedObject.FindProperty(nameof(ECSHub.port));
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            // EditorGUILayout.TextField   ("testString", testString);
            showHost = EditorGUILayout.Foldout(showHost, "Connection", true);
            
            if (showHost) {
                EditorGUILayout.PropertyField (host);
                EditorGUILayout.PropertyField (port);
                Controls();
            }
            serializedObject.ApplyModifiedProperties();

            /*
            var hub         = (Hub)target;            
            hub.playerPrefab    = (GameObject)EditorGUILayout.ObjectField("Player Prefab", hub.playerPrefab, typeof(GameObject), true);
            // netSync.type     = (NetType)   EditorGUILayout.EnumPopup  ("Type",   netSync.type);

            showClient          = EditorGUILayout.Foldout(showClient, "Client", true);
            if (showClient) {
                hub.userId      = EditorGUILayout.TextField ("user id",     hub.userId);
                hub.clientId    = EditorGUILayout.TextField ("client id",   hub.clientId);
            } */
        }
        
        private void Controls()
        {
            var hub = (ECSHub)target;
            var type = hub.hubType;
            var server  = type == HubType.Server ? hub.IsServerRunning   ? Color.green : Color.yellow : (Color?)null;
            var client  = type == HubType.Client ? hub.IsClientConnected ? Color.green : Color.yellow : (Color?)null;
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            // if (gui.Button("Host",   type == HubType.Host   || type == HubType.None))           if (type ==  HubType.None) hub.StartHost();   else hub.StopHub();
            
            GUILayout.BeginHorizontal();
            if (GUIUtils.Button("Server", type == HubType.Server || type == HubType.None, server))   if (type ==  HubType.None) hub.StartServer(); else hub.StopHub();
            GUILayout.Label(type == HubType.Server ? "running": "", GUILayout.Width(60));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUIUtils.Button("Client", type == HubType.Client || type == HubType.None, client))   if (type ==  HubType.None) hub.StartClient(); else hub.StopHub();
            GUILayout.Label(type == HubType.Client ? "connected": "", GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (GUIUtils.Button("Hub Explorer"))        hub.OpenExplorer();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}