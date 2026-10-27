#if UNITY_EDITOR
namespace CMS.Editor
{
    public interface ICustomContentCompiler
    {
        string[] FileExtensions { get; }
        void Compile();
        Segment[] GetCustomContentSegments();
    }
}
#endif
