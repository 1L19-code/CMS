using System.Text;

namespace CMS
{
    public static class EntityConstants
    {
        public const string FileExtension = ".ent";
        
        public static readonly byte[] FileMagic = Encoding.UTF8.GetBytes("ENTITY");
        public static readonly ReaderId ReaderId = ReaderId.FromString("Entity_Plugin");
    }
}