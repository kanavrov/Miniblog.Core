using System;
using Newtonsoft.Json.Linq;

namespace Miniblog.Core.Web.Extensions
{
    public static class JsonExtensions
	{
		/// <summary>
		/// Finds a JObject member of a JObject by key (extension method).
		/// </summary>
		public static JObject GetJObject(this JObject json, string key)
		{
			var value = json.GetValue(key);
			return value as JObject;
		}

		/// <summary>
		/// Finds a JObject member of a JObject by key (extension method).
		/// </summary>
		public static JArray GetJArray(this JObject json, string key)
		{
			var value = json.GetValue(key);
			return value as JArray;
		}

		/// <summary>
		/// Finds a member of a JObject by key and parses it into string (extension method).
		/// </summary>
		public static string GetString(this JObject json, string key)
		{
			var value = json.GetValue(key);
			return value?.ToString();
		}

		/// <summary>
		/// Parses string as JObject (extension method).
		/// </summary>
		public static JObject AsJObject(this string toParse)
		{
			try
			{
				return JObject.Parse(toParse);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Parses string as JArray (extension method).
		/// </summary>
		public static JArray AsJArray(this string toParse)
		{
			try
			{
				return JArray.Parse(toParse);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Add a string key / value pair to the given JObject (extension method).
		/// </summary>
		public static void AddString(this JObject json, string key, string value)
		{
			json.Add(key, new JValue(value));
		}

		/// <summary>
		/// Gets the JObject with the specified index from the array.
		/// </summary>
		public static JObject GetJObject(this JArray jsonArray, int index)
		{
			if (index <= jsonArray.Count)
			{
				return (JObject)jsonArray[index];
			}

			return null;
		}
	}
}