using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace BracketHouse.FontExtension
{
	[ContentImporter(".bhfont", DisplayName = "Field Font Importer", DefaultProcessor = "FieldFontProcessor")]
	internal class FieldFontImporter : ContentImporter<FontDescription>
	{
		public override FontDescription Import(string filename, ContentImporterContext context)
		{
			return Parse(filename);
		}

		private static FontDescription Parse(string filename)
		{
			JsonDocument jdoc = JsonDocument.Parse(File.ReadAllText(filename));
			string path = jdoc.RootElement.GetProperty("path").GetString();
			var rs = jdoc.RootElement.GetProperty("ranges");
			char[] characters = ParseRanges(rs);
			return new FontDescription(path, characters);
		}

		private static char[] ParseRanges(JsonElement ranges)
		{
			var characters = new HashSet<char>();
			foreach (var item in ranges.EnumerateArray())
			{
				char startChar = CharFromJsonElement(item.GetProperty("start"));
				char endChar = CharFromJsonElement(item.GetProperty("end"));
				if (endChar < startChar)
				{
					throw new Exception($"end character {endChar} was lower value than start character {startChar}");
				}
				for (int i = startChar; i <= endChar; i++)
				{
					characters.Add((char)i);
				}
			}
			return characters.ToArray();
		}

		private static char CharFromJsonElement(JsonElement el)
		{
			if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out int startInt))
			{
				return (char)startInt;
			}
			else
			{
				string stringValue = el.GetString();
				if (stringValue.Length == 1)
				{
					return stringValue[0];
				}
				else if (stringValue.StartsWith("0x"))
				{
					return (char)Convert.ToInt32(stringValue, 16);
				}
				else
				{
					throw new Exception($"\"{stringValue}\" not recognized as integer, hex number or single character.");
				}
			}
		}
	}

}
