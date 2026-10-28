using Cms.DefinitionPlugin.Common;
using CMS.Editor.Drawers;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    public class TestDefinition : DefinitionBase
    {
        public float Speed;
        public int TickRate;

        [TypeSelector(typeof(VisualElement))]public string Type;
    }
}