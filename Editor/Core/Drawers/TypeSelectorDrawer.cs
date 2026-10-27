#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace CMS.Editor.Drawers
{
    public class TypeSelectorDrawer : VisualElement, INotifyValueChanged<string>
    {
        private readonly DropdownField _choicesField;

        public TypeSelectorDrawer(Type type)
        {
            Add(_choicesField = Draw(type));
        }

        public string value
        {
            get => _choicesField.value;
            set => _choicesField.value = value;
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _choicesField.SetValueWithoutNotify(newValue);
        }

        private static DropdownField Draw(Type type)
        {
            var childen = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a
                    .GetTypes()
                    .Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && !t.ContainsGenericParameters)
                )
                .ToArray();

            if (!childen.Any())
                throw new Exception($"Type '{type.FullName}' is not assignable to any type");

            var list = new List<string>
            {
                "None"
            };
            list.AddRange(childen.Select(t => t.FullName).OrderBy(name => name));
            
            var field = new DropdownField()
            {
                name = "ChoseType",
                choices = list,
            };
            
            return field;
        }
    }
}
#endif
