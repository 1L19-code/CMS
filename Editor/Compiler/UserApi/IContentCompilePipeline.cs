#if UNITY_EDITOR
namespace CMS.Editor
{
    public interface IContentCompilePipeline
    {
        void Build();
    }
}
#endif
