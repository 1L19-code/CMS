#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor.Drawers
{
    public class BasicObjectDrawer : VisualElement
    {
        private object _instance;
        public event Action Changed;

        public void Draw(Type type, object instance = null)
        {
            Clear();
            _instance = type == null ? null : instance ?? Activator.CreateInstance(type);
            if (_instance != null)
                CreateFields(_instance, this, () => Changed?.Invoke());
        }

        public bool TryGet(out object obj)
        {
            obj = _instance;
            return obj != null;
        }

        private static void CreateFields(object instance, VisualElement root, Action changed)
        {
            root.style.paddingLeft = 10;
            foreach (var field in instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.IsNotSerialized || (field.IsInitOnly && !instance.GetType().IsValueType))
                    continue;

                var type = field.FieldType;
                var value = field.GetValue(instance);
                if (value == null && type != typeof(string))
                {
                    value = Activator.CreateInstance(type);
                    field.SetValue(instance, value);
                }

                var selectedType = field.GetCustomAttribute<TypeSelectorAttribute>()?.RootType;
                var container = new VisualElement();
                container.Add(new Label(field.Name));
                container.Add(CreateValue(type, value, updated =>
                {
                    field.SetValue(instance, updated);
                    changed();
                }, selectedType));
                root.Add(container);
            }
        }

        internal static VisualElement CreateValue(Type type, object value, Action<object> changed,
            Type selectedType = null)
        {
            if (type == typeof(AnimationCurve))
            {
                var field = new UnityEditor.UIElements.CurveField { value = (AnimationCurve)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));

                return field;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return new ListDrawer(type.GetGenericArguments()[0], (IList)value, selectedType, () => changed(value));

            if (type.IsEnum)
            {
                var field = new EnumField((Enum)value);
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(string) && selectedType != null)
            {
                var field = new TypeSelectorDrawer(selectedType);
                field.SetValueWithoutNotify((string)value ?? string.Empty);
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(string))
            {
                var field = new TextField { value = (string)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(int))
            {
                var field = new IntegerField { value = (int)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(float))
            {
                var field = new FloatField { value = (float)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(double))
            {
                var field = new DoubleField { value = (double)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(long))
            {
                var field = new LongField { value = (long)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            if (type == typeof(bool))
            {
                var field = new Toggle { value = (bool)value };
                field.RegisterValueChangedCallback(evt => changed(evt.newValue));
                return field;
            }

            var drawer = new BasicObjectDrawer();
            drawer.Draw(type, value);
            drawer.Changed += () =>
            {
                drawer.TryGet(out var updated);
                changed(updated);
            };
            return drawer;
        }
    }
}
#endif
