using System.Threading.Tasks;

namespace CMS
{
    public interface IContent
    {
        ValueTask IndexAsync();
        T Get<T>(ContentId contentId); 
    }
}