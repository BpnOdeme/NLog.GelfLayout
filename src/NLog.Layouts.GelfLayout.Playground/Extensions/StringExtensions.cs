using System.Text.Json;

namespace NLog.Layouts.GelfLayout.Playground.Extensions
{
	public static class StringExtensions
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
		{
			WriteIndented = true
		};

		public static string JsonPrettify(this string json)
		{
			using var jsonDoc = JsonDocument.Parse(json);
			return JsonSerializer.Serialize(jsonDoc, _jsonSerializerOptions);
		}
	}
}