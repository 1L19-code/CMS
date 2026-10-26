#if UNITY_EDITOR
namespace Cms.Build
{
    public interface IReaderIdService
    {
        bool TryGetId(string path, out ReaderId readerId);
    }
}
#endif
