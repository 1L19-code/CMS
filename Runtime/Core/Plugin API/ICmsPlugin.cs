using System;
using CMS.Editor;

namespace CMS
{
    public interface ICmsPlugin
    {
        ReaderId ReaderId { get; }
        T Get<T>(ReadOnlyMemory<byte> data);
    }
}