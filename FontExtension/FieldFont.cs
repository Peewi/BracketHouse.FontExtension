using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BracketHouse.FontExtension
{
	/// <summary>
	/// A font for use with TextRenderer.
	/// </summary>
	public class FieldFont
	{
		[ContentSerializer] private readonly Dictionary<char, FieldGlyph> Glyphs;
		[ContentSerializer] private readonly string NameBackend;
		[ContentSerializer] private readonly float PxRangeBackend;
		[ContentSerializer] private readonly float LineHeightBackend;
		[ContentSerializer] private readonly float AscenderBackend;
		[ContentSerializer] private readonly float DescenderBackend;
		[ContentSerializer] private readonly Dictionary<(char, char), float> KerningBackend;
		[ContentSerializer] private readonly byte[] BitmapBackend;

		internal FieldFont()
		{

		}

		internal FieldFont(string name, IEnumerable<FieldGlyph> glyphs, Dictionary<(char, char), float> kerning, float pxRange, float lineHeight, float ascender, float descender, byte[] bitmap)
		{
			this.NameBackend = name;
			this.PxRangeBackend = pxRange;
			this.LineHeightBackend = lineHeight;
			this.AscenderBackend = ascender;
			this.DescenderBackend = descender;
			this.KerningBackend = kerning;

			this.Glyphs = new Dictionary<char, FieldGlyph>(glyphs.Count());
			foreach (var glyph in glyphs)
			{
				this.Glyphs.Add(glyph.Character, glyph);
			}
			this.BitmapBackend = bitmap;
		}
		/// <summary>
		/// Parse a json file and combine it with bitmap bytes
		/// </summary>
		/// <param name="jsonFile">Filename for json from msdf-atlast-gen</param>
		/// <param name="bitmap">A byte array of bitmap data</param>
		/// <returns>FieldFont</returns>
		internal static FieldFont FromJsonAndBitmapBytes(string jsonFile, byte[] bitmap)
		{
			string name = Path.GetFileNameWithoutExtension(jsonFile);
			var jdoc = JsonDocument.Parse(File.ReadAllText(jsonFile));
			float lineHeight = jdoc.RootElement.GetProperty("metrics").GetProperty("lineHeight").GetSingle();
			float ascender = jdoc.RootElement.GetProperty("metrics").GetProperty("ascender").GetSingle();
			float descender = jdoc.RootElement.GetProperty("metrics").GetProperty("descender").GetSingle();
			var jAtlas = jdoc.RootElement.GetProperty("atlas");
			float range = jAtlas.GetProperty("distanceRange").GetSingle();
			float width = jAtlas.GetProperty("width").GetSingle();
			float height = jAtlas.GetProperty("height").GetSingle();
			var jGlyphs = jdoc.RootElement.GetProperty("glyphs");
			List<FieldGlyph> glyphs = new List<FieldGlyph>(jGlyphs.GetArrayLength());
			foreach (var glyphElement in jGlyphs.EnumerateArray())
			{
				char c = (char)glyphElement.GetProperty("unicode").GetInt32();
				float adv = glyphElement.GetProperty("advance").GetSingle();
				float planeBot = 0;
				float planeTop = 0;
				float planeLeft = 0;
				float planeRight = 0;
				if (glyphElement.TryGetProperty("planeBounds", out JsonElement planeBounds))
				{
					planeBot = planeBounds.GetProperty("bottom").GetSingle();
					planeTop = planeBounds.GetProperty("top").GetSingle();
					planeLeft = planeBounds.GetProperty("left").GetSingle();
					planeRight = planeBounds.GetProperty("right").GetSingle();
				}
				float atlasBot = 0;
				float atlasTop = 0;
				float atlasLeft = 0;
				float atlasRight = 0;
				if (glyphElement.TryGetProperty("atlasBounds", out JsonElement atlasBounds))
				{
					atlasBot = atlasBounds.GetProperty("bottom").GetSingle() / height;
					atlasTop = atlasBounds.GetProperty("top").GetSingle() / height;
					atlasLeft = atlasBounds.GetProperty("left").GetSingle() / width;
					atlasRight = atlasBounds.GetProperty("right").GetSingle() / width;
				}
				glyphs.Add(new FieldGlyph(c, adv, planeLeft, planeRight, planeTop, planeBot, atlasLeft, atlasRight, atlasTop, atlasBot));
			}
			var jKerning = jdoc.RootElement.GetProperty("kerning");
			Dictionary<(char, char), float> kerning = new Dictionary<(char, char), float>(jKerning.GetArrayLength());
			foreach (var kernElement in jKerning.EnumerateArray())
			{
				char c1 = (char)kernElement.GetProperty("unicode1").GetInt32();
				char c2 = (char)kernElement.GetProperty("unicode2").GetInt32();
				float kernAdv = kernElement.GetProperty("advance").GetSingle();
				kerning.Add((c1, c2), kernAdv);
			}
			return new FieldFont(name, glyphs, kerning, range, lineHeight, ascender, descender, bitmap);
		}

		/// <summary>
		/// Name of the font
		/// </summary>
		public string Name => this.NameBackend;

		/// <summary>
		/// Distance field effect range in pixels
		/// </summary>
		internal float PxRange => this.PxRangeBackend;

		/// <summary>
		/// Kerning pairs available in this font
		/// </summary>
		internal Dictionary<(char, char), float> Kerning => this.KerningBackend;

		/// <summary>
		/// Lineheight for this font
		/// </summary>
		internal float LineHeight => this.LineHeightBackend;

		/// <summary>
		/// Ascender for this font
		/// </summary>
		internal float Ascender => this.AscenderBackend;

		/// <summary>
		/// Descender for this font
		/// </summary>
		internal float Descender => this.DescenderBackend;

		/// <summary>
		/// Distance field atlas for this font
		/// </summary>
		internal byte[] Bitmap => this.BitmapBackend;

		/// <summary>
		/// Characters supported by this font
		/// </summary>
		[ContentSerializerIgnore]
		public IEnumerable<char> SupportedCharacters => this.Glyphs.Keys;

		/// <summary>
		/// Returns the glyph for the given character, or throws an exception when the glyph is not supported by this font
		/// </summary>        
		internal FieldGlyph GetGlyph(char c)
		{
			if (this.Glyphs.TryGetValue(c, out FieldGlyph glyph))
			{
				return glyph;
			}
			if (this.Glyphs.TryGetValue('?', out FieldGlyph backupGlyph))
			{
				return backupGlyph;
			}
			throw new InvalidOperationException($"Character '{c}' not found in font {this.Name}. Did you forget to include it in the character ranges?");
		}
		/// <summary>
		/// Measure how large a string is.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="lineHeight">Lineheight to use instead of the lineheight of the font.</param>
		/// <param name="kerning">Whether to use kerning.</param>
		/// <param name="formatting">Whether to check for formatting tags and skip measuring them. Does not consider scale.</param>
		/// <returns>Width and height of the string if drawn with scale 1.0.</returns>
		public Vector2 MeasureString(string text, float lineHeight, bool kerning, bool formatting = false)
		{
			float currentLine = 0;
			Vector2 measure = Vector2.Zero;
			measure.Y = lineHeight;
			for (int i = 0; i < text.Length; i++)
			{
				bool skipLetter = false;
				if (formatting && text[i] == '[')
				{
					var (_, tagType, _, tagStringLength) = Formatting.FindTag(text, i);
					i += tagStringLength;
					if (tagType != Formatting.TagType.Unknown)
					{
						skipLetter = true;
					}
				}
				if (!skipLetter)
				{
					FieldGlyph current = GetGlyph(text[i]);
					currentLine += current.Advance;
					if (kerning && i < text.Length - 1)
					{
						if (Kerning.TryGetValue((text[i], text[i + 1]), out float kern))
						{
							currentLine += kern;
						}
					}
					measure.X = MathF.Max(measure.X, currentLine);
					if (text[i] == '\n')
					{
						currentLine = 0;
						measure.Y += lineHeight;
					}
				}
			}
			return measure;
		}
		/// <summary>
		/// Measure how large a string is, with kerning enabled and using the font's lineheight.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="formatting">Whether to check for formatting tags and skip measuring them. Does not consider scale.</param>
		/// <returns>Width and height of the string if drawn with scale 1.0.</returns>
		public Vector2 MeasureString(string text, bool formatting = true)
		{
			return MeasureString(text, LineHeight, true, formatting);
		}
		/// <summary>
		/// Measure how large a string is, using the font's lineheight.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="kerning">Whether to use kerning.</param>
		/// <param name="formatting">Whether to check for formatting tags and skip measuring them. Does not consider scale.</param>
		/// <returns>Width and height of the string if drawn with scale 1.0.</returns>
		public Vector2 MeasureString(string text, bool kerning, bool formatting = true)
		{
			return MeasureString(text, LineHeight, kerning, formatting);
		}
		/// <summary>
		/// Insert linebreaks in a text to fit a certain width. Linebreaks may be inserted after spaces, hyphens and zero width spaces.
		/// </summary>
		/// <param name="text">Text to insert spaces into.</param>
		/// <param name="maxWidth">Desired maximum line length. This number is for a <c>scale = 1</c> text. Returned text may contain longer lines if there's no appropriate point to break.</param>
		/// <param name="kerning">Whether to consider kerning when measuring text length.</param>
		/// <param name="formatting">Whether to check for formatting tags and skip measuring them. Does not consider scale.</param>
		/// <returns>Text with linebreaks inserted.</returns>
		public string InsertLineBreaks(string text, float maxWidth, bool kerning = true, bool formatting = true)
		{
			string editedText = text;
			float currentLine = 0;
			int lastPossible = 0;
			bool lineBreakable = false;
			float longestLine = 0;
			bool breakBefore = false;
			for (int i = 0; i < editedText.Length; i++)
			{
				bool skipLetter = false;
				if (formatting && editedText[i] == '[')
				{
					var (_, tagType, _, tagStringLength) = Formatting.FindTag(editedText, i);
					i += tagStringLength;
					if (tagType != Formatting.TagType.Unknown)
					{
						skipLetter = true;
					}
				}
				if (!skipLetter)
				{
					if ((editedText[i] == ' ' || editedText[i] == '-' || editedText[i] == ']' || editedText[i] == '\u200b'))
					{
						lastPossible = i;
						lineBreakable = true;
						breakBefore = false;
					}
					if (editedText[i] == '[' )
					{
						lastPossible = i;
						lineBreakable = true;
						breakBefore = false;
					}
					FieldGlyph current = GetGlyph(editedText[i]);
					currentLine += current.Advance;
					if (kerning && i < editedText.Length - 1)
					{
						if (Kerning.TryGetValue((editedText[i], editedText[i + 1]), out float kern))
						{
							currentLine += kern;
						}
					}
					if (editedText[i] == '\n')
					{
						currentLine = 0;
						lineBreakable = false;
					}
					if (lineBreakable && currentLine > maxWidth)
					{
						int breakOffset = 1;
						if (breakBefore)
						{
							breakOffset = 0;
						}
						editedText = editedText[0..(lastPossible + breakOffset)] + '\n' + editedText[(lastPossible + breakOffset)..^0];
						lineBreakable = false;
						currentLine = 0;
						i = lastPossible + 0;
					}
				}
				longestLine = MathF.Max(longestLine, currentLine);
			}
			return editedText;
		}
	}
}
