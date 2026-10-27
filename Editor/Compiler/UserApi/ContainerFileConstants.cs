using System.Text;

namespace CMS.Editor
{
    public static class ContainerFileConstants
    {
        public const int GuidSize = 16;
        public static readonly byte[] Magic = Encoding.UTF8.GetBytes("CMS-F");
        public const int Version = 1;
    }
}
