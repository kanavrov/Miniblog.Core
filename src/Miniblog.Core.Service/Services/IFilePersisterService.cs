using System.IO;
using System.Threading.Tasks;

namespace Miniblog.Core.Service.Services
{
	public interface IFilePersisterService
	{
		Task<string> SaveImage(byte[] bytes, string fileName, string contentType, string suffix = null);

		Task<string> SaveImage(Stream stream, string fileName, string contentType, string suffix = null);

		Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null);

		Task<string> SaveFile(Stream stream, string fileName, string suffix = null);
	}
}