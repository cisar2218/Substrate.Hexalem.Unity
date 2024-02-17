using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UIElements.VisualElement;
using UnityEngine.UIElements;
using UnityEngine;

namespace Assets.Polkadot_Unity_SDK.DemoGame.Scripts
{
    public class HexalemTextField : VisualElement
    {
        private const string styleResource = "UIStylesheet";

        private const string ussHexalemTextField = "hexalem-text-field";
        private const string ussHexalemTextFieldLabel = "hexalem-text-field-label";
        private const string ussHexalemTextFieldElement = "hexalem-text-field-element";

        public TextField TextField { get; private set; }

        private readonly Label _label;
        private string _labelText;

        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                _label.text = _labelText;
            }
        }

        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<HexalemTextField, UxmlTraits>
        { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription labelTextAttr = new UxmlStringAttributeDescription()
            {
                name = "label_text",
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                (ve as HexalemTextField).LabelText = labelTextAttr.GetValueFromBag(bag, cc);
            }
        }

        public HexalemTextField()
        {
            styleSheets.Add(Resources.Load<StyleSheet>($"Assets/Polkadot Unity SDK/DemoGame/{styleResource}"));

            VisualElement mainVe = new();
            mainVe.style.width = Length.Percent(100);
            mainVe.style.height = Length.Percent(100);

            //TextField = new("Test");
            TextField = new("Test", -1, true, false, '*');
            TextField.AddToClassList(ussHexalemTextField);
            TextField.style.width = Length.Percent(100);
            TextField.style.height = Length.Percent(100);
            mainVe.Add(TextField);

            //var textFieldborderColor = Color.black;
            //var textFieldborderWidth = 4;
            //var textFieldborderRadius = 30;

            //textField.style.borderLeftColor = textFieldborderColor;
            //textField.style.borderRightColor = textFieldborderColor;
            //textField.style.borderTopColor = textFieldborderColor;
            //textField.style.borderBottomColor = textFieldborderColor;

            //textField.style.borderTopWidth = textFieldborderWidth;
            //textField.style.borderBottomWidth = textFieldborderWidth;
            //textField.style.borderLeftWidth = textFieldborderWidth;
            //textField.style.borderRightWidth = textFieldborderWidth;

            //textField.style.borderTopRightRadius = textFieldborderRadius;
            //textField.style.borderBottomRightRadius = textFieldborderRadius;
            //textField.style.borderTopLeftRadius = textFieldborderRadius;
            //textField.style.borderBottomLeftRadius = textFieldborderRadius;

            _label = (Label)TextField.Children().ElementAt(0);
            _label.AddToClassList(ussHexalemTextFieldLabel);

            var textInput = TextField.Children().ElementAt(1);
            SetLookAndFeel(textInput, Color.clear, Color.clear, 0, 0);
            textInput.style.whiteSpace = WhiteSpace.Normal;
            textInput.style.textOverflow = TextOverflow.Clip;

            if (textInput.Children().Any())
            {
                var textElement = textInput.Children().ElementAt(0);
                textElement.AddToClassList(ussHexalemTextFieldElement);
                textElement.style.whiteSpace = WhiteSpace.Normal;
                textElement.style.textOverflow = TextOverflow.Clip;
            }

            hierarchy.Add(mainVe);
        }

        private void SetLookAndFeel(VisualElement visualElement, Color backgroundColor, Color borderColor, int borderWidth, int borderRadius)
        {
            visualElement.style.backgroundColor = backgroundColor;

            visualElement.style.borderLeftColor = borderColor;
            visualElement.style.borderRightColor = borderColor;
            visualElement.style.borderTopColor = borderColor;
            visualElement.style.borderBottomColor = borderColor;

            visualElement.style.borderTopWidth = borderWidth;
            visualElement.style.borderBottomWidth = borderWidth;
            visualElement.style.borderLeftWidth = borderWidth;
            visualElement.style.borderRightWidth = borderWidth;

            visualElement.style.borderTopRightRadius = borderRadius;
            visualElement.style.borderBottomRightRadius = borderRadius;
            visualElement.style.borderTopLeftRadius = borderRadius;
            visualElement.style.borderBottomLeftRadius = borderRadius;
        }
    }
}
