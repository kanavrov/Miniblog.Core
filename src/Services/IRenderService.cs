using System.Threading.Tasks;
using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
    public interface IRenderService
    {
        Task SaveImagesAndReplace(Post post);
    }
}