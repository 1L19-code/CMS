using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    public readonly struct PopupDefinition
    {
        public readonly string Title;
        public readonly TextFieldDefinition? TextField;
        public readonly ButtonDefinition[] Buttons;

        private PopupDefinition(string title, TextFieldDefinition? textField, ButtonDefinition[] buttons)
        {
            Title = title;
            TextField = textField;
            Buttons = buttons;
        }

        public static PopupDefinition WithTitle(string title)
        {
            return new PopupDefinition(title, null, null);
        }

        public PopupDefinition WithButtons(params ButtonDefinition[] buttons)
        {
            return new PopupDefinition(Title, TextField, buttons);
        }

        public PopupDefinition WithTextField(TextFieldDefinition textField)
        {
            return new PopupDefinition(Title, textField, Buttons);
        }

        public static PopupDefinition CreateFilePopup(Action<string> onSave, string fileExtension = null)
        {
            return WithTitle("Create")
                .WithButtons(new ButtonDefinition("Save", onSave))
                .WithTextField(new TextFieldDefinition("File name", fileExtension));
        }
    }

    public readonly struct TextFieldDefinition
    {
        [CanBeNull] public readonly string Title;
        [CanBeNull] public readonly string Suffix;

        public TextFieldDefinition(string title, string suffix)
        {
            Title = title;
            Suffix = suffix;
        }

        public TextFieldDefinition([CanBeNull] string title) : this()
        {
            Title = title;
        }
    }

    public readonly struct ButtonDefinition
    {
        public readonly string Text;

        /// <summary>
        /// If there is a text field in popup, inputed text will be recived in OnClick Action
        /// </summary>
        public readonly Action<string> OnClick;

        public ButtonDefinition(string text, Action<string> onClick)
        {
            Text = text;
            OnClick = onClick;
        }
    }

    internal class Popup : VisualElement
    {
        private const string TemplatePath = "Packages/com.ilya.cms/Editor/src/PopupTemplate.uxml";

        public Popup(PopupDefinition definition)
        {
            Create(definition);
        }

        private void Create(PopupDefinition definition)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);
            var template = asset.CloneTree();
            var body = template.Q<VisualElement>("Body");

            template.Q<Label>("Title").text = definition.Title;
            var textFieldWrapper = body.Q<VisualElement>("TextFieldWrapper");

            if (definition.TextField is null)
            {
                textFieldWrapper.RemoveFromHierarchy();
            }
            else
            {
                template.Q<TextField>().Q<Label>().text = definition.TextField.Value.Title;
                textFieldWrapper.Q<Label>(className: "Suffix").text = definition.TextField.Value.Suffix;
            }

            var buttonTemplate = template.Q<Button>();

            if (definition.Buttons is not null)
            {
                buttonTemplate.RemoveFromHierarchy();

                textFieldWrapper = body.Q<VisualElement>("TextFieldWrapper");
                var textField = textFieldWrapper?.Q<TextField>();

                foreach (var definitionButton in definition.Buttons)
                {
                    var button = new Button(() => OnClick(textField?.value, definitionButton))
                    {
                        text = definitionButton.Text,
                    };

                    body.Add(button);
                }

                body.Add(new Button(RemoveFromHierarchy)
                {
                    text = "Close"
                });
            }

            Add(template);

            return;

            void OnClick(string text, ButtonDefinition buttonDefinition)
            {
                buttonDefinition.OnClick(text);
                RemoveFromHierarchy();
            }
        }
    }
}