using System.Threading.Tasks;

namespace Miniblog.Core.Services
{
    public interface IFilePersisterService
    {
         Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null);
    }
}