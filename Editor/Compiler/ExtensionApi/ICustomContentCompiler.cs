#if UNITY_EDITOR
namespace Cms.Build
{
    public interface ICustomContentCompiler
    {
        string[] FileExtensions { get; }
        void Compile();
        Segment[] GetCustomContentSegments();
    }
}
#endif
