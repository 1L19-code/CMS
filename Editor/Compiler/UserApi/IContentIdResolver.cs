#if UNITY_EDITOR
using CMS;

namespace Cms.Build
{
    public interface IContentIdResolver
    {
        ContentId GetContentId(string path);
    }
}
#endif
