#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine.UIElements;

namespace CMS.Editor.Drawers
{
    public sealed class ListDrawer : VisualElement
    {
        private const string StyleSheetPath =
            "Packages/com.ilya.cms/Editor/Core/Drawers/ListDrawer.uss";

        private readonly Type _itemType;
        private readonly IList _items;
        private readonly Type _selectedType;
        private readonly Action _changed;
        private readonly ScrollView _scrollView;

        public ListDrawer(Type itemType, IList items, Type selectedType, Action changed)
        {
            _itemType = itemType;
            _items = items;
            _selectedType = selectedType;
            _changed = changed;
            AddToClassList("ListDrawer");
#if UNITY_EDITOR
            styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath));
#endif
            _scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            _scrollView.AddToClassList("ListDrawer__ScrollView");
            Add(_scrollView);
            var add = new Button(() =>
            {
                _items.Add(_itemType == typeof(string) ? string.Empty : Activator.CreateInstance(_itemType));
                Refresh();
            }) { text = "Add" };
            add.AddToClassList("ListDrawer__Button");
            add.AddToClassList("ListDrawer__AddButton");
            Add(add);
            Draw();
        }

        private void Draw()
        {
            _scrollView.Clear();

            for (var index = 0; index < _items.Count; index++)
            {
                var itemIndex = index;
                var row = new VisualElement();
                row.AddToClassList("ListDrawer__Item");
                var controls = new VisualElement();
                controls.AddToClassList("ListDrawer__Controls");
                var number = new Label($"{index + 1}");
                number.AddToClassList("ListDrawer__Number");
                controls.Add(number);
                var up = new Button(() => Move(itemIndex, itemIndex - 1)) { text = "↑" };
                up.AddToClassList("ListDrawer__Button");
                up.AddToClassList("ListDrawer__MoveButton");
                up.SetEnabled(index > 0);
                var down = new Button(() => Move(itemIndex, itemIndex + 1)) { text = "↓" };
                down.AddToClassList("ListDrawer__Button");
                down.AddToClassList("ListDrawer__MoveButton");
                down.SetEnabled(index + 1 < _items.Count);
                controls.Add(up);
                controls.Add(down);
                var remove = new Button(() =>
                {
                    _items.RemoveAt(itemIndex);
                    Refresh();
                }) { text = "Remove" };
                remove.AddToClassList("ListDrawer__Button");
                remove.AddToClassList("ListDrawer__RemoveButton");
                controls.Add(remove);
                row.Add(controls);
                var content = BasicObjectDrawer.CreateValue(_itemType, _items[index], value =>
                {
                    _items[itemIndex] = value;
                    _changed();
                }, _selectedType);
                content.AddToClassList("ListDrawer__Content");
                row.Add(content);
                _scrollView.Add(row);
            }
        }

        private void Move(int from, int to)
        {
            var item = _items[from];
            _items.RemoveAt(from);
            _items.Insert(to, item);
            Refresh();
        }

        private void Refresh()
        {
            _changed();
            Draw();
        }
    }
}
#endif
