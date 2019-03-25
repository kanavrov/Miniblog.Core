using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Miniblog.Core.Service.Exceptions;
using Miniblog.Core.Service.Settings;

namespace Miniblog.Core.Service.Persistence
{
	public class FilePersisterService : IFilePersisterService
	{
		private const double MEGABYTE = 1048576.0;
		private const string POSTS = "Posts";
		private const string FILES = "files";
		private readonly string _folder;
		private readonly IOptions<BlogSettings> _settings;

		public FilePersisterService(IHostingEnvironment env, IOptions<BlogSettings> settings)
		{
			_folder = Path.Combine(env.WebRootPath, POSTS);
			_settings = settings;
		}

		public async Task<string> SaveImage(Stream stream, string fileName, string contentType, string suffix = null)
		{
			using (var memoryStream = new MemoryStream())
        	{
            	await stream.CopyToAsync(memoryStream);
            	return await SaveImage(memoryStream.ToArray(), fileName, contentType, suffix = null);
        	}
		}

		public async Task<string> SaveFile(Stream stream, string fileName, string suffix = null)
		{
			using (var memoryStream = new MemoryStream())
        	{
            	await stream.CopyToAsync(memoryStream);
            	return await SaveFile(memoryStream.ToArray(), fileName, suffix = null);
        	}
		}

		public async Task<string> SaveImage(byte[] bytes, string fileName, string contentType, string suffix = null)
		{
			if(contentType == null)
			{
				throw new FilePersistException($"File {fileName} is not an image.");
			}

			return await SaveFile(bytes, fileName, suffix);
		}

		public async Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null)
		{
			try
			{
				if(_settings.Value.UploadSizeLimitMegabytes < ConvertToMegabytes(bytes.Length))
				{
					throw new FilePersistException($"File {fileName} exceeds the upload size limit of {_settings.Value.UploadSizeLimitMegabytes} MB.");
				}

				suffix = CleanFromInvalidChars(suffix ?? DateTime.UtcNow.Ticks.ToString());

				string ext = Path.GetExtension(fileName);
				string name = CleanFromInvalidChars(Path.GetFileNameWithoutExtension(fileName));

				string fileNameWithSuffix = $"{name}_{suffix}{ext}";

				string absolute = Path.Combine(_folder, FILES, fileNameWithSuffix);
				string dir = Path.GetDirectoryName(absolute);

				Directory.CreateDirectory(dir);
				using (var writer = new FileStream(absolute, FileMode.CreateNew))
				{
					await writer.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
				}

				return $"/{POSTS}/{FILES}/{fileNameWithSuffix}";
			}
			catch (Exception ex)
			{
				throw new FilePersistException($"File {fileName} could not be saved. See innter exception for details.", ex);
			}
		}

		private static double ConvertToMegabytes(long bytesLength) {
			return bytesLength / MEGABYTE;
		}

		private static string CleanFromInvalidChars(string input)
		{
			// ToDo: what we are doing here if we switch the blog from windows
			// to unix system or vice versa? we should remove all invalid chars for both systems

			var regexSearch = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()));
			var r = new Regex($"[{regexSearch}]");
			return r.Replace(input, "");
		}		
	}
}