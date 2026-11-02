using System;

namespace CMS
{
    public interface ICmsPlugin
    {
        T Get<T>(ReadOnlyMemory<byte> data);
    }
}