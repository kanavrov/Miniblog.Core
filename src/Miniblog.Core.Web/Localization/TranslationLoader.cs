using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Miniblog.Core.Web.Extensions;
using Newtonsoft.Json.Linq;

namespace Miniblog.Core.Web.Localization
{
	public class TranslationLoader : ITranslationLoader
	{

		private readonly string _folder;

		public TranslationLoader(IHostingEnvironment env)
		{
			_folder = Path.Combine(env.ContentRootPath, "App_Data/content/i18n");
		}

		public TranslationLoader(string folderPath)
		{
			_folder = folderPath;
		}

		public IDictionary<CultureInfo, ITranslationModel> Load()
		{
			IDictionary<CultureInfo, ITranslationModel> result = new Dictionary<CultureInfo, ITranslationModel>();
			if (!Directory.Exists(_folder))
			{
				throw new TranslationLoadException($"Translation folder path doesn't exist : {_folder}.");
			}
			string[] filePaths = Directory.GetFiles(_folder);

			foreach (string filePath in filePaths)
			{
				try
				{
					string fileName = Path.GetFileNameWithoutExtension(filePath);
					ITranslationModel model = new TranslationModel();
					JObject translation = File.ReadAllText(filePath).AsJObject();

					if (translation == null)
					{
						throw new TranslationLoadException($"Malformed translation file. Should contain a valid JSON object");
					}

					foreach (JProperty property in translation.Properties())
					{
						model[property.Name] = translation.GetString(property.Name);
					}

					result[CultureInfo.GetCultureInfo(fileName)] = model;
				}
				catch (Exception e)
				{
					throw new TranslationLoadException($"Error while loading translation file: {filePath}", e);
				}
			}

			return result;
		}
	}
}