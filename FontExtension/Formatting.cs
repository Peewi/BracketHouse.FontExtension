using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FontExtension
{
	public static class Formatting
	{
		/// <summary>
		/// Contains methods used for text formatting.
		/// </summary>
		[Flags]
		internal enum TagType
		{
			Unknown = 0,
			Fill = FillColor,
			Color = FillColor,
			FillColor = 1,
			Stroke = StrokeColor,
			StrokeColor = 2,
			Offset = PositionOffset,
			Position = PositionOffset,
			PositionOffset = 4,
			LetterPosition = LetterPositionOffset,
			LetterPositionOffset = 16,
			Scale = 32,
			LineHeight = 64,
			Kerning = 128,
			Special = 256,
			EndFormat = 512,
		}
		/// <summary>
		/// Delegate to use for adding fill color formatting tags
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="baseColor">The fill color passed to <c>TextRenderer.LayoutText</c></param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A color that will be used for the text fill.</returns>
		public delegate Color? FillDelegate(GameTime gameTime, Color baseColor, string[] args);
		/// <summary>
		/// Delegate to use for adding stroke color formatting tags
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="baseColor">The stroke color passed to <c>TextRenderer.LayoutText</c></param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A color that will be used for the text stroke.</returns>
		public delegate Color? StrokeDelegate(GameTime gameTime, Color baseColor, string[] args);
		/// <summary>
		/// Delegate for offsetting the position of text.
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A vector that will be multiplied by the text scale and then added to letter positions.</returns>
		public delegate Vector2 PositionDelegate(GameTime gameTime, string[] args);
		/// <summary>
		/// Delegate for offsetting the position of individual letters in text. Will be called for each letter.
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="charNum">The index of the letter about to be rendered, ignoring formatting tags.</param>
		/// <param name="position">Position of the current character in the rendered text.</param>
		/// <param name="currentChar">Char for the glyph that is about to be rendered</param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A vector that will be multiplied by the text scale and then added to the letter position.</returns>
		public delegate Vector2 LetterPositionDelegate(GameTime gameTime, int charNum, Vector2 position, char currentChar, string[] args);
		/// <summary>
		/// Delegate for text scaling tags.
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A <c>Vector2</c> struct, used to scale text non-uniformly.</returns>
		public delegate float ScaleDelegate(GameTime gameTime, string[] args);
		/// <summary>
		/// Delegate for lineheight scaling tags.
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A <c>float</c>, used to scale lineheight.</returns>
		public delegate float LineHeightDelegate(GameTime gameTime, string[] args);
		/// <summary>
		/// Delegate for enabling or disabling kerning.
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns>A <c>bool</c>. <c>true</c> enables kerning and <c>false</c> disables kerning.</returns>
		public delegate bool KerningDelegate(GameTime gameTime, string[] args);
		/// <summary>
		/// A delegate that does not return a value to be used for text formatting. Can be used to run arbitrary code, triggered by text rendering.
		/// </summary>
		/// <param name="gameTime">A gametime object. Useful for animated effects.</param>
		/// <param name="charNum">The index of the letter about to be rendered, ignoring formatting tags.</param>
		/// <param name="position">Position of the current character in the rendered text.</param>
		/// <param name="currentChar">Char for the glyph that is about to be rendered</param>
		/// <param name="fillColor">The fill color passed to <c>TextRenderer.LayoutText</c></param>
		/// <param name="strokeColor">The stroke color passed to <c>TextRenderer.LayoutText</c></param>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		public delegate void SpecialDelegate(GameTime gameTime, int charNum, Vector2 position, char currentChar, Color fillColor, Color strokeColor, string[] args);
		/// <summary>
		/// Delegate for tag that resets the effects of other tags.
		/// </summary>
		/// <param name="args">Array of argument strings. Note that the first element will be the name of the command.</param>
		/// <returns><c>TagType</c> indicating which formatting to reset.</returns>
		internal delegate TagType EndFormatDelegate(string[] args);
		/// <summary>
		/// Names of all tags and their corresponding <c>TagType</c>.
		/// </summary>
		static readonly Dictionary<string, TagType> TagNames = new Dictionary<string, TagType>();
		static readonly Dictionary<string, FillDelegate> FillTags = new Dictionary<string, FillDelegate>();
		static readonly Dictionary<string, StrokeDelegate> StrokeTags = new Dictionary<string, StrokeDelegate>();
		static readonly Dictionary<string, PositionDelegate> PositionTags = new Dictionary<string, PositionDelegate>();
		static readonly Dictionary<string, LetterPositionDelegate> LetterPositionTags = new Dictionary<string, LetterPositionDelegate>();
		static readonly Dictionary<string, ScaleDelegate> ScaleTags = new Dictionary<string, ScaleDelegate>();
		static readonly Dictionary<string, LineHeightDelegate> LineHeightTags = new Dictionary<string, LineHeightDelegate>();
		static readonly Dictionary<string, KerningDelegate> KerningTags = new Dictionary<string, KerningDelegate>();
		static readonly Dictionary<string, SpecialDelegate> SpecialTags = new Dictionary<string, SpecialDelegate>();
		static readonly Dictionary<string, EndFormatDelegate> EndTags = new Dictionary<string, EndFormatDelegate>();

		static Formatting()
		{
			// register default tags here.
			RegisterTag("color", fillFunction: FormattingFunctions.ColorFunction);
			RegisterTag("fill", fillFunction: FormattingFunctions.ColorFunction);
			RegisterTag("stroke", strokeFunction: FormattingFunctions.ColorFunction);
			RegisterTag("offset", positionFunction: FormattingFunctions.ParseVector2);
			RegisterTag("scale", scaleFunction: FormattingFunctions.ParseFloat);
			RegisterTag("lineheight", lineHeightFunction: FormattingFunctions.ParseFloat);
			RegisterTag("kerning", kerningFunction: FormattingFunctions.ParseBool);
			RegisterTag("sine", letterPositionFunction: FormattingFunctions.Sine);
			RegisterTag("rainbow", fillFunction: FormattingFunctions.Rainbow);
			RegisterTag("rainbowfill", fillFunction: FormattingFunctions.Rainbow);
			RegisterTag("rainbowstroke", strokeFunction: FormattingFunctions.Rainbow);
			RegisterTag("end", endFunction: FormattingFunctions.EndFormat);
		}
		/// <summary>
		/// Add a new text fill color tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="fillFunction">Color function</param>
		public static void RegisterTag(string name, FillDelegate fillFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.FillColor;
			FillTags[name.ToLowerInvariant()] = fillFunction;
		}
		/// <summary>
		/// Add a new text stroke color tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive</param>
		/// <param name="strokeFunction">Color function</param>
		public static void RegisterTag(string name, StrokeDelegate strokeFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.StrokeColor;
			StrokeTags[name.ToLowerInvariant()] = strokeFunction;
		}
		/// <summary>
		/// Add a new position offset tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="positionFunction">Position offset function</param>
		public static void RegisterTag(string name, PositionDelegate positionFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.PositionOffset;
			PositionTags[name.ToLowerInvariant()] = positionFunction;
		}
		/// <summary>
		/// Add a new letter position offset tag. Note that while active, it will execute for every letter rendered.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="letterPositionFunction">Letter position function</param>
		public static void RegisterTag(string name, LetterPositionDelegate letterPositionFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.LetterPositionOffset;
			LetterPositionTags[name.ToLowerInvariant()] = letterPositionFunction;
		}
		/// <summary>
		/// Add a new scaling tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="scaleFunction">Scaling function</param>
		public static void RegisterTag(string name, ScaleDelegate scaleFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.Scale;
			ScaleTags[name.ToLowerInvariant()] = scaleFunction;
		}
		/// <summary>
		/// Add a new lineheight tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="lineHeightFunction">Lineheight function</param>
		public static void RegisterTag(string name, LineHeightDelegate lineHeightFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.LineHeight;
			LineHeightTags[name.ToLowerInvariant()] = lineHeightFunction;
		}
		/// <summary>
		/// Add a new kerning tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="kerningFunction">Kerning function</param>
		public static void RegisterTag(string name, KerningDelegate kerningFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.Kerning;
			KerningTags[name.ToLowerInvariant()] = kerningFunction;
		}
		/// <summary>
		/// Add a new special tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="specialFunction">Special function</param>
		public static void RegisterTag(string name, SpecialDelegate specialFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.Special;
			SpecialTags[name.ToLowerInvariant()] = specialFunction;
		}
		/// <summary>
		/// Add a new format ending tag.
		/// </summary>
		/// <param name="name">Name to use for tag. Case insensitive.</param>
		/// <param name="endFunction">End function</param>
		internal static void RegisterTag(string name, EndFormatDelegate endFunction)
		{
			TagNames[name.ToLowerInvariant()] = TagType.EndFormat;
			EndTags[name.ToLowerInvariant()] = endFunction;
		}
		/// <summary>
		/// Find a tag in a string and execut its delegate.
		/// </summary>
		/// <param name="text">Text string to search for tag in</param>
		/// <param name="startIndex">Index for opening tag [</param>
		/// <param name="gameTime">Used for animated effects</param>
		/// <param name="fillColor">Fill color passed to <c>TextRenderer.LayoutText</c>.</param>
		/// <param name="strokeColor">Stroke color passed to <c>TextRenderer.LayoutText</c>.</param>
		/// <returns>Result of tag execution, type of tag that was executed, the tag arguments, and how many characters to skip forward in text rendering.</returns>
		internal static (object returnValue, TagType tagType, string[] args, int tagStringLength) FindAndExecuteTag(string text, int startIndex, GameTime gameTime, Color fillColor, Color strokeColor)
		{
			(Delegate tagDelegate, TagType tagType, string[] tagArgs, int tagStringLength) = FindTag(text, startIndex);
			var delType = tagDelegate?.GetType();
			string[] args = tagArgs;
			if (tagDelegate is FillDelegate fill)
			{
				if (fill.Invoke(gameTime, fillColor, args) is Color fillAttempt)
				{
					return (fillAttempt, tagType, tagArgs, tagStringLength);
				}
			}
			if (tagDelegate is StrokeDelegate stroke)
			{
				if (stroke.Invoke(gameTime, strokeColor, args) is Color strokeAttempt)
				{
					return (strokeAttempt, tagType, tagArgs, tagStringLength);
				}
			}
			if (tagDelegate is PositionDelegate pos)
			{
				return (pos.Invoke(gameTime, args), tagType, tagArgs, tagStringLength);
			}
			if (tagDelegate is LetterPositionDelegate letpos)
			{
				return (letpos, tagType, tagArgs, tagStringLength);
			}
			if (tagDelegate is ScaleDelegate scale)
			{
				return (scale.Invoke(gameTime, args), tagType, tagArgs, tagStringLength);
			}
			if (tagDelegate is LineHeightDelegate lineheight)
			{
				return (lineheight.Invoke(gameTime, args), tagType, tagArgs, tagStringLength);
			}
			if (tagDelegate is KerningDelegate kern)
			{
				return (kern.Invoke(gameTime, args), tagType, tagArgs, tagStringLength);
			}
			if (tagDelegate is SpecialDelegate special)
			{
				return (special, tagType, tagArgs, tagStringLength);
			}
			if (tagDelegate is EndFormatDelegate end)
			{
				return (end.Invoke(args), tagType, tagArgs, tagStringLength);
			}
			Color? colorAttempt = FormattingFunctions.ColorFunction(gameTime, fillColor, args);
			if (colorAttempt is Color clr)
			{
				return (clr, TagType.FillColor, tagArgs, tagStringLength);
			}
			return (null, TagType.Unknown, null, tagStringLength);
		}
		/// <summary>
		/// Parse a text formatting tag.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <param name="startIndex">Index of where tag starts in string. Char at index should be [ opening square bracket.</param>
		/// <returns>Delegate for found tag, what type of tag it is, and the tag arguments</returns>
		internal static (Delegate tagDelegate, TagType tagType, string[] tagArgs, int tagStringLength) FindTag(string text, int startIndex)
		{
			if (startIndex >= text.Length || text[startIndex] != '[')
			{
				return (null, TagType.Unknown, null, 0);
			}
			string[] tagArgs = null;
			int tagStringLength = 0;
			for (int i = startIndex + 1; i < text.Length; i++)
			{
				if (text[i] == ']')
				{
					tagStringLength = i - startIndex;
					tagArgs = text[(startIndex + 1)..i].Split();
					for (int j = 0; j < tagArgs.Length; j++)
					{
						tagArgs[j] = tagArgs[j].ToLowerInvariant();
					}
					break;
				}
			}
			var result = TagNames.TryGetValue(tagArgs[0], out TagType tagType);
			if (result)
			{
				return tagType switch
				{
					TagType.Unknown => (null, TagType.Unknown, null, tagStringLength),
					TagType.FillColor => (FillTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.StrokeColor => (StrokeTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.PositionOffset => (PositionTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.LetterPositionOffset => (LetterPositionTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.Scale => (ScaleTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.LineHeight => (LineHeightTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.Kerning => (KerningTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.Special => (SpecialTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					TagType.EndFormat => (EndTags[tagArgs[0]], tagType, tagArgs, tagStringLength),
					_ => (null, TagType.Unknown, null, tagStringLength),
				};
			}
			if (FormattingFunctions.ColorNames.ContainsKey(tagArgs[0]))
			{
				return (FillTags["fill"], TagType.FillColor, tagArgs, tagStringLength);
			}
			return (null, TagType.Unknown, tagArgs, tagStringLength);
		}
	}
}
