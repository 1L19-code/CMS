#if UNITY_EDITOR
namespace CMS.Editor
{
    public interface IReaderIdService
    {
        bool TryGetId(string path, out ReaderId readerId);
    }
}
#endif
