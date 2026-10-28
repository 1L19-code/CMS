using System.Text;

namespace CMS
{
    public class ContentFileConstants
    {
        public const int GuidSize = 16;
        public static readonly byte[] Magic = Encoding.UTF8.GetBytes("CMS-F");
        public const int Version = 1;
    }
}