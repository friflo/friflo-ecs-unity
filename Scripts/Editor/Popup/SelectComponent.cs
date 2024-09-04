// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Friflo.Engine.ECS;
using Friflo.Engine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable CoVariantArrayConversion
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.UnityEditor
{
    public class SelectComponent : VisualElement
    {
        private             TextField               search;
        private             ListView                listView;
        public              Action                  close;
        private             long                    lastListEvent;
        private readonly    List<SchemaType>        typeList = new List<SchemaType>();
        private             SelectComponentItem[]   items;
        
        private static readonly ComponentType[]     ComponentTypes;
        private static readonly TagType[]           TagTypes;
        
        static SelectComponent() {
            var schema      = EntityStore.GetEntitySchema();
            ComponentTypes  = schema.Components.ToArray();
            TagTypes        = schema.Tags.ToArray();
        }
        
        public void SetFocus() {
            search.Focus();
        }
            
        internal static SelectComponent Create(string name, SchemaTypeKind kind, Entity entity, SymbolDrawer symbolDrawer)
        {
            var content     = new SelectComponent();
            var bg          = EditorGUIUtility.isProSkin ? new Color32(75,75,75,255)    : new Color32(200,200,200,255);
            var border      = EditorGUIUtility.isProSkin ? new Color32(0,0,0,255)       : new Color32(160,160,160,255); 
            content.style.opacity           = new StyleFloat(1);
            content.style.backgroundColor   = new StyleColor(bg);
            content.style.borderTopColor =
            content.style.borderBottomColor =
            content.style.borderLeftColor =
            content.style.borderRightColor = new StyleColor(border);
            content.style.borderTopWidth    = 1;
            content.style.borderBottomWidth = 1;
            content.style.borderLeftWidth   = 1;
            content.style.borderRightWidth  = 1;

            // --- search box
            var title       = new Label(name);
            
            var header = new VisualElement();
            header.style.backgroundColor =  new StyleColor(bg);
            header.style.borderBottomWidth = 2;
            content.Add(header);
            
            title.style.alignSelf = new StyleEnum<Align>(Align.Center);
            header.Add(title);
            
            var search      = content.search = new TextField();
            search.tabIndex = 1;
            search.RegisterValueChangedCallback(_ => {
                content.UpdateTypeList(kind);
                content.listView.RefreshItems();
            });
            header.Add(search);

            // --- component / tag list
            var listView = content.listView = new ListView();
            listView.itemsSource    = content.typeList;
            listView.bindItem       = (element, i) =>
            {
                var item = element as SelectComponentItem;
                content.items[i] = item;
                item!.Init(entity, content.typeList[i]);
            };
            content.UpdateTypeList(kind);
            
            listView.fixedItemHeight=20;

            listView.makeItem       = () => new SelectComponentItem(symbolDrawer);
            listView.selectionType  = SelectionType.Single;
            listView.style.flexGrow = 1.0f;
            listView.tabIndex       = 2;
            listView.RegisterCallback<KeyDownEvent>(evt => {
                // Debug.Log($"KeyDownEvent: {evt.keyCode}");
                switch (evt.keyCode) {
                    case KeyCode.Space:
                        content.lastListEvent = evt.eventTypeId;
                        break;
                }
            });
            listView.RegisterCallback<MouseDownEvent>(evt => {
                // Debug.Log($"MouseDownEvent");
                content.lastListEvent = evt.eventTypeId;
            });
            
            listView.itemsChosen += _ => {
                if (content.lastListEvent == KeyDownEvent.TypeId()) {
                    // Debug.Log($"itemsChosen - Space");
                    var item = content.items[listView.selectedIndex];
                    item.ModifyComponent(!item.Enabled);
                }
                if (content.lastListEvent == MouseDownEvent.TypeId()) {
                    content.items[listView.selectedIndex].ModifyComponent(true);
                    content.close.Invoke();
                }
            };

            search.RegisterCallback<KeyDownEvent>(evt => {
                switch (evt.keyCode) {
                    case KeyCode.UpArrow:
                        if (listView.itemsSource.Count != 0) {
                            // evt.PreventDefault();
                            content.SelectItem(0);
                        }
                        break;
                    case KeyCode.DownArrow:
                        if (listView.itemsSource.Count != 0) {
                            if (listView.selectedIndex == -1) {
                                listView.selectedIndex = 0;
                            }
                            // evt.PreventDefault();
                            content.SelectItem(0);
                        }
                        break;
                }
            });
            content.Add(listView);
            content.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.Escape) {
                    content.close?.Invoke();
                }
                if (evt.keyCode == KeyCode.Return) {
                    if (listView.selectedIndex < 0)  return;
                    content.items[listView.selectedIndex].ModifyComponent(true);
                    content.close?.Invoke();
                }
            }, TrickleDown.TrickleDown);
            content.RegisterCallback<BlurEvent> (_ => {
                var hasFocus = content.panel.focusController.focusedElement != null;
                if (!hasFocus) {
                    content.close?.Invoke();
                }
            }, TrickleDown.TrickleDown);

            return content;
        }
        
        private void UpdateTypeList(SchemaTypeKind kind)
        {
            var filter = search.value.ToLower();
            var tokens1 = filter.Split(' ');
            var tokens2 = tokens1.Where(token => token.Length > 0).ToArray();
            // listView.itemsSource    = new List<SchemaType>();
            typeList.Clear();
            SchemaType[] types = kind == SchemaTypeKind.Component ? ComponentTypes : TagTypes;
            foreach (var type in types)
            {
                if (type == null) continue;
                if (type.Type == typeof(GameObjectLink) ||
                    type.Type == typeof(Unresolved)) {
                    continue;
                }
                if (tokens2.Length > 0 && !HasToken(type.Name, tokens2)) {
                    continue;
                }
                typeList.Add(type);
            }
            items = new SelectComponentItem[typeList.Count];
            if (typeList.Count > 0) {
                listView.selectedIndex = 0;
            }
            // Debug.Log($"UpdateTypeList - filter: {filter}  tokens: {tokens2.Length}  matches: {items.Length}");
        }
        
        private static bool HasToken(string name, string[] tokens)
        {
            foreach (var token in tokens) {
                if (name.Contains(token, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        private void SelectItem(int offset)
        {
            // Debug.Log($"SelectItem {offset}");
            var index = listView.selectedIndex + offset;
            var max = listView.itemsSource.Count - 1;
            if (index < 0) {
                index = max;
            }
            else if (index > max) {
                index = 0;
            }
            listView.Focus();
            listView.selectedIndex = index;
        }
    }
}





















