#if UNITY_EDITOR
namespace Cms.Build
{
    public interface IContentCompilePipeline
    {
        void Build();
    }
}
#endif
