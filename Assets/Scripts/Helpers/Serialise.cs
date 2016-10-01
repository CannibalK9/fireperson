using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Assets.Scripts.Helpers
{
	public static class Serialise
	{
		public static string Serialize<T>(this T value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			try
			{
				var xmlserializer = new XmlSerializer(typeof(T));
				var stringWriter = new StringWriter();
				using (var writer = XmlWriter.Create(stringWriter))
				{
					xmlserializer.Serialize(writer, value);
					return stringWriter.ToString();
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static T Deserialize<T>(this string value)
		{
			if (value == null)
			{
				return default(T);
			}
			try
			{
				var xmlserializer = new XmlSerializer(typeof(T));
				var stringReader = new StringReader(value);
				using (var reader = XmlReader.Create(stringReader))
				{
					return (T)xmlserializer.Deserialize(reader);
				}
			}
			catch (Exception)
			{
				return default(T);
			}
		}
	}
}
