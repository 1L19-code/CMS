#if UNITY_EDITOR
using System;

namespace CMS.Editor.Drawers
{
    public class TypeSelectorAttribute : Attribute
    {
        public Type RootType { get; }
        
        public TypeSelectorAttribute(Type rootType)
        {
            RootType = rootType;
        }
    }
}
#endif
