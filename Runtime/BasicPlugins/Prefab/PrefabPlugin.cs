using System;
using CMS.Editor;

namespace CMS
{
    public class PrefabPlugin : ICmsPlugin
    {
        public ReaderId ReaderId => PrefabConstants.ReaderId;
        
        public T Get<T>(ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}