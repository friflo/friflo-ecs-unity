// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor {

    [CustomEditor(typeof(ECSStore))]
    // ReSharper disable once InconsistentNaming
    internal sealed class ECSStoreEditor : Editor
    {
    //  private SerializedProperty  storeName;
        private SerializedProperty  defaultObject;
        private SerializedProperty  enableORM;

        private void OnEnable()
        {
            // storeName    = serializedObject.FindProperty(nameof(ECSStore.storeName));
            defaultObject   = serializedObject.FindProperty(nameof(ECSStore.defaultObject));
            enableORM       = serializedObject.FindProperty(nameof(ECSStore.enableORM));
        }
        
        public override void OnInspectorGUI()
        {
            var ecsStore    = (ECSStore)target;
            var store       = ecsStore.EntityStore;
            
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            var count = store.Count;
            EditorGUILayout.LabelField("Entity Count", count.ToString());
            var oldBg           = GUI.backgroundColor;
            GUI.backgroundColor = ColorStyles.Green;
            var content         = new GUIContent ("Create Entity", "Creates an entity with Default Object.\nCtrl + E");
            bool createEntity   = GUILayout.Button(content, GUILayout.Width(100));
            GUI.backgroundColor = oldBg;
            var more            = GUILayout.Button("...", GUILayout.Width(22), GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.ObjectField(defaultObject, typeof(GameObject));
            EditorGUILayout.PropertyField(enableORM);
            if (more) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Save ECS Store as JSON"), false, () => {
                    ecsStore.StoreContext.SaveEntityStore();
                    var path = ecsStore.StoreContext.GetStorePath();
                    EditorUtility.RevealInFinder(path);
                });
                menu.ShowAsContext();
                return;
            }
            if (createEntity) {
                // Complexity of CreateEntity() must be dead simple!
                var entity  = store.CreateEntity(new GameObjectLink());
                var go      = entity.GetComponent<GameObjectLink>().gameObject;
                Selection.objects = new Object []{ go };
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}