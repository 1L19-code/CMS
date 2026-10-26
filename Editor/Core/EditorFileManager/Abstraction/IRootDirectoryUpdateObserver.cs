using System;

namespace CMS.Editor
{
    public interface IRootDirectoryChangeObserver
    {
        event Action Changed;
    }
}