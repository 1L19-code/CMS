using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    public class PopupsWrapper : VisualElement
    {
        private Vector2 _mousePosition;

        public PopupsWrapper()
        {
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                var root = evt.destinationPanel.visualTree;
                root.RegisterCallback<MouseMoveEvent>(OnMousePositionChanged, TrickleDown.TrickleDown);
                root.RegisterCallback<MouseDownEvent>(OnMousePositionChanged, TrickleDown.TrickleDown);
                root.RegisterCallback<MouseUpEvent>(OnMousePositionChanged, TrickleDown.TrickleDown);
            });
            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                var root = evt.originPanel.visualTree;
                root.UnregisterCallback<MouseMoveEvent>(OnMousePositionChanged, TrickleDown.TrickleDown);
                root.UnregisterCallback<MouseDownEvent>(OnMousePositionChanged, TrickleDown.TrickleDown);
                root.UnregisterCallback<MouseUpEvent>(OnMousePositionChanged, TrickleDown.TrickleDown);
            });

            style.position = Position.Absolute;
            style.top = 0;
            style.left = 0;
            style.width = new StyleLength(Length.Percent(100));
            style.height = new StyleLength(Length.Percent(100));
            
            pickingMode = PickingMode.Ignore;
            
            var root = new VisualElement()
            {
                style =
                {
                    display = DisplayStyle.Flex,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                },
                pickingMode = PickingMode.Ignore
            };
            root.AddToClassList("Body");
            Add(root);
        }

        public void ShowPopup(PopupDefinition definition)
        {
            var body = this.Q<VisualElement>(className: "Body");
            var mousePosition = body.WorldToLocal(_mousePosition);
            body.Clear();
            var popup = new Popup(definition)
            {
                style =
                {
                    position = Position.Absolute,
                    top = mousePosition.y,
                    left = mousePosition.x
                }
            };

            body.Add(popup);
        }

        private void OnMousePositionChanged(IMouseEvent evt)
        {
            _mousePosition = evt.mousePosition;
        }
    }
}
