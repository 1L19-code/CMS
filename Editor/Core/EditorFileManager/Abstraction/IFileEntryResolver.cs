using System.Collections.Generic;

namespace CMS.Editor
{
    internal interface IFileEntryResolver
    {
        FileEntry[] GetAllEntries();
    }
}
