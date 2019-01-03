using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Miniblog.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace Miniblog.Core.Localization
{
	public class TranslationLoader : ITranslationLoader
	{
		private string FolderPath { get; }

		public TranslationLoader()
		{
			//TODO: Set default path to translation folder
			FolderPath = "";
		}

		public TranslationLoader(string folderPath)
		{
			FolderPath = folderPath;
		}

		public IDictionary<CultureInfo, ITranslationModel> Load()
		{
			IDictionary<CultureInfo, ITranslationModel> result = new Dictionary<CultureInfo, ITranslationModel>();
			if (!Directory.Exists(FolderPath))
			{
				throw new TranslationLoadException($"Translation folder path doesn't exist : {FolderPath}.");
			}
			string[] filePaths = Directory.GetFiles(FolderPath);

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