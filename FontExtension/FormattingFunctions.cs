using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BracketHouse.FontExtension
{
	internal static class FormattingFunctions
	{
		internal static readonly Dictionary<string, (byte red, byte green, byte blue)> ColorNames
	= new Dictionary<string, (byte red, byte green, byte blue)>()
	{
#region ColorNames
		{ "aliceblue" , (240, 248, 255) },
		{ "antiquewhite" , (250, 235, 215) },
		{ "aqua" , (0, 255, 255) },
		{ "aquamarine" , (127, 255, 212) },
		{ "azure" , (240, 255, 255) },
		{ "beige" , (245, 245, 220) },
		{ "bisque" , (255, 228, 196) },
		{ "black" , (0, 0, 0) },
		{ "blanchedalmond" , (255, 235, 205) },
		{ "blue" , (0, 0, 255) },
		{ "blueviolet" , (138, 43, 226) },
		{ "brown" , (165, 42, 42) },
		{ "burlywood" , (222, 184, 135) },
		{ "cadetblue" , (95, 158, 160) },
		{ "chartreuse" , (127, 255, 0) },
		{ "chocolate" , (210, 105, 30) },
		{ "coral" , (255, 127, 80) },
		{ "cornflowerblue" , (100, 149, 237) },
		{ "cornsilk" , (255, 248, 220) },
		{ "crimson" , (220, 20, 60) },
		{ "cyan" , (0, 255, 255) },
		{ "darkblue" , (0, 0, 139) },
		{ "darkcyan" , (0, 139, 139) },
		{ "darkgoldenrod" , (184, 134, 11) },
		{ "darkgray" , (169, 169, 169) },
		{ "darkgreen" , (0, 100, 0) },
		{ "darkkhaki" , (189, 183, 107) },
		{ "darkmagenta" , (139, 0, 139) },
		{ "darkolivegreen" , (85, 107, 47) },
		{ "darkorange" , (255, 140, 0) },
		{ "darkorchid" , (153, 50, 204) },
		{ "darkred" , (139, 0, 0) },
		{ "darksalmon" , (233, 150, 122) },
		{ "darkseagreen" , (143, 188, 139) },
		{ "darkslateblue" , (72, 61, 139) },
		{ "darkslategray" , (47, 79, 79) },
		{ "darkturquoise" , (0, 206, 209) },
		{ "darkviolet" , (148, 0, 211) },
		{ "deeppink" , (255, 20, 147) },
		{ "deepskyblue" , (0, 191, 255) },
		{ "dimgray" , (105, 105, 105) },
		{ "dodgerblue" , (30, 144, 255) },
		{ "firebrick" , (178, 34, 34) },
		{ "floralwhite" , (255, 250, 240) },
		{ "forestgreen" , (34, 139, 34) },
		{ "fuchsia" , (255, 0, 255) },
		{ "gainsboro" , (220, 220, 220) },
		{ "ghostwhite" , (248, 248, 255) },
		{ "gold" , (255, 215, 0) },
		{ "goldenrod" , (218, 165, 32) },
		{ "gray" , (128, 128, 128) },
		{ "green" , (0, 128, 0) },
		{ "greenyellow" , (173, 255, 47) },
		{ "honeydew" , (240, 255, 240) },
		{ "hotpink" , (255, 105, 180) },
		{ "indianred" , (205, 92, 92) },
		{ "indigo" , (75, 0, 130) },
		{ "ivory" , (255, 255, 240) },
		{ "khaki" , (240, 230, 140) },
		{ "lavender" , (230, 230, 250) },
		{ "lavenderblush" , (255, 240, 245) },
		{ "lawngreen" , (124, 252, 0) },
		{ "lemonchiffon" , (255, 250, 205) },
		{ "lightblue" , (173, 216, 230) },
		{ "lightcoral" , (240, 128, 128) },
		{ "lightcyan" , (224, 255, 255) },
		{ "lightgoldenrodyellow" , (250, 250, 210) },
		{ "lightgreen" , (144, 238, 144) },
		{ "lightgray" , (211, 211, 211) },
		{ "lightpink" , (255, 182, 193) },
		{ "lightsalmon" , (255, 160, 122) },
		{ "lightseagreen" , (32, 178, 170) },
		{ "lightskyblue" , (135, 206, 250) },
		{ "lightslategray" , (119, 136, 153) },
		{ "lightsteelblue" , (176, 196, 222) },
		{ "lightyellow" , (255, 255, 224) },
		{ "lime" , (0, 255, 0) },
		{ "limegreen" , (50, 205, 50) },
		{ "linen" , (250, 240, 230) },
		{ "magenta" , (255, 0, 255) },
		{ "maroon" , (128, 0, 0) },
		{ "mediumaquamarine" , (102, 205, 170) },
		{ "mediumblue" , (0, 0, 205) },
		{ "mediumorchid" , (186, 85, 211) },
		{ "mediumpurple" , (147, 112, 219) },
		{ "mediumseagreen" , (60, 179, 113) },
		{ "mediumslateblue" , (123, 104, 238) },
		{ "mediumspringgreen" , (0, 250, 154) },
		{ "mediumturquoise" , (72, 209, 204) },
		{ "mediumvioletred" , (199, 21, 133) },
		{ "midnightblue" , (25, 25, 112) },
		{ "mintcream" , (245, 255, 250) },
		{ "mistyrose" , (255, 228, 225) },
		{ "moccasin" , (255, 228, 181) },
		{ "navajowhite" , (255, 222, 173) },
		{ "navy" , (0, 0, 128) },
		{ "oldlace" , (253, 245, 230) },
		{ "olive" , (128, 128, 0) },
		{ "olivedrab" , (107, 142, 35) },
		{ "orange" , (255, 165, 0) },
		{ "orangered" , (255, 69, 0) },
		{ "orchid" , (218, 112, 214) },
		{ "palegoldenrod" , (238, 232, 170) },
		{ "palegreen" , (152, 251, 152) },
		{ "paleturquoise" , (175, 238, 238) },
		{ "palevioletred" , (219, 112, 147) },
		{ "papayawhip" , (255, 239, 213) },
		{ "peachpuff" , (255, 218, 185) },
		{ "peru" , (205, 133, 63) },
		{ "pink" , (255, 192, 203) },
		{ "plum" , (221, 160, 221) },
		{ "powderblue" , (176, 224, 230) },
		{ "purple" , (128, 0, 128) },
		{ "red" , (255, 0, 0) },
		{ "rosybrown" , (188, 143, 143) },
		{ "royalblue" , (65, 105, 225) },
		{ "saddlebrown" , (139, 69, 19) },
		{ "salmon" , (250, 128, 114) },
		{ "sandybrown" , (244, 164, 96) },
		{ "seagreen" , (46, 139, 87) },
		{ "seashell" , (255, 245, 238) },
		{ "sienna" , (160, 82, 45) },
		{ "silver" , (192, 192, 192) },
		{ "skyblue" , (135, 206, 235) },
		{ "slateblue" , (106, 90, 205) },
		{ "slategray" , (112, 128, 144) },
		{ "snow" , (255, 250, 250) },
		{ "springgreen" , (0, 255, 127) },
		{ "steelblue" , (70, 130, 180) },
		{ "tan" , (210, 180, 140) },
		{ "teal" , (0, 128, 128) },
		{ "thistle" , (216, 191, 216) },
		{ "tomato" , (255, 99, 71) },
		{ "turquoise" , (64, 224, 208) },
		{ "violet" , (238, 130, 238) },
		{ "wheat" , (245, 222, 179) },
		{ "white" , (255, 255, 255) },
		{ "whitesmoke" , (245, 245, 245) },
		{ "yellow" , (255, 255, 0) },
		{ "yellowgreen" , (154, 205, 50) },
		#endregion
	};

		/// <summary>
		/// A color for text stuff
		/// </summary>
		/// <param name="gameTime">Gametime. Not actually used.</param>
		/// <param name="baseColor">Base color. Only alpha value is used.</param>
		/// <param name="args">One or more strings to parse as a color. Accepts color name and RGB and RGBA values as either hex or numbers 0-255</param>
		/// <returns>A <c>Color</c> struct</returns>
		internal static Color? ColorFunction(GameTime gameTime, Color baseColor, string[] args)
		{
			int argIndex = 0;
			if (args[0] == "fill" || args[0] == "stroke")
			{
				argIndex = 1;
			}
			// parse RGB and RGBA
			if (args.Length - argIndex >= 3)
			{
				bool rResult = byte.TryParse(args[argIndex], out byte r);
				bool gResult = byte.TryParse(args[argIndex + 1], out byte g);
				bool bResult = byte.TryParse(args[argIndex + 2], out byte b);
				bool aResult = true;
				byte a = baseColor.A;
				if (args.Length - argIndex >= 4)
				{
					aResult = byte.TryParse(args[argIndex + 3], out a);
				}
				if (rResult && gResult && bResult && aResult)
				{
					return new Color(r, g, b, a);
				}
				return null;
			}
			if (args.Length - argIndex == 1)
			{
				// parse color name
				if (ColorNames.ContainsKey(args[argIndex]))
				{
					var (red, green, blue) = ColorNames[args[argIndex]];
					return new Color(red, green, blue, baseColor.A);
				}
				// parse hex
				if ((args[argIndex].Length == 7 || args[argIndex].Length == 9) && args[argIndex][0] == '#')
				{
					string hex = args[argIndex].ToLowerInvariant();
					const string HEXSYMBOLS = "0123456789abcdef";
					for (int i = 1; i < hex.Length; i++)
					{
						if (!HEXSYMBOLS.Contains(hex[i]))
						{
							return null;
						}
					}
					byte r = Convert.ToByte(hex[1..3], 16);
					byte g = Convert.ToByte(hex[3..5], 16);
					byte b = Convert.ToByte(hex[5..7], 16);
					byte a = baseColor.A;
					if (args[argIndex].Length == 9)
					{
						a = Convert.ToByte(hex[7..9], 16);
					}
					return new Color(r, g, b, a);
				}
			}
			return null;
		}
		/// <summary>
		/// Reset formatting started by a different tag
		/// </summary>
		/// <param name="args">Parses values in <c>args[1]</c> and forward</param>
		/// <returns><c>TagType</c> indicating which format type to end.</returns>
		internal static Formatting.TagType EndFormat(string[] args)
		{
			Formatting.TagType retVal = Formatting.TagType.Unknown;
			for (int i = 1; i < args.Length; i++)
			{
				if (Enum.TryParse(typeof(Formatting.TagType), args[i], true, out object tagType))
				{
					retVal |= (Formatting.TagType)tagType;
				}
			}
			return retVal;
		}
		/// <summary>
		/// Parse a vector 2.
		/// </summary>
		/// <param name="gameTime">Unused.</param>
		/// <param name="args">Value in <c>args[1]</c> used for both X and Y, or X in <c>args[1]</c> and Y in <c>args[2]</c></param>
		/// <returns>A <c>Vector2</c></returns>
		internal static Vector2 ParseVector2(GameTime gameTime, string[] args)
		{
			if (args.Length == 2)
			{
				bool xResult = float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
				if (xResult)
				{
					return new Vector2(x);
				}
			}
			if (args.Length == 3)
				{
				bool xResult = float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
				bool yResult = float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
				if (xResult && yResult)
				{
					return new Vector2(x, y);
				}
			}
			return Vector2.Zero;
		}
		/// <summary>
		/// Parse a float.
		/// </summary>
		/// <param name="gameTime">Unused.</param>
		/// <param name="args">Value in <c>args[1]</c> used</param>
		/// <returns>a <c>float</c>. Returns <c>0.0f</c> on failure.</returns>
		internal static float ParseFloat(GameTime gameTime, string[] args)
		{
			bool result = float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
			if (result)
			{
				return value;
			}
			return 0.0f;
		}
		/// <summary>
		/// Parse a bool.
		/// </summary>
		/// <param name="gameTime">Unused.</param>
		/// <param name="args">Value in <c>args[1]</c> used</param>
		/// <returns>A <c>bool</c>. Returns <c>false</c> on failure.</returns>
		internal static bool ParseBool(GameTime gameTime, string[] args)
		{
			bool result = bool.TryParse(args[1], out bool value);
			if (result)
			{
				return value;
			}
			return false;
		}
		/// <summary>
		/// Make text move in a sine wave.
		/// </summary>
		/// <param name="gameTime">Used for animation</param>
		/// <param name="charNum">Unused</param>
		/// <param name="position">Used to make text not all move together</param>
		/// <param name="currentChar">Unused</param>
		/// <param name="args">Unused</param>
		/// <returns>A <c>Vector2</c> to be added to letter position.</returns>
		internal static Vector2 Sine(GameTime gameTime, int charNum, Vector2 position, char currentChar, string[] args)
		{
			float val = MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 5f + position.X) / 3f;
			return new Vector2(0, val);
		}
		/// <summary>
		/// Make letters in text shake erratically.
		/// </summary>
		/// <param name="gameTime">Used for animation</param>
		/// <param name="charNum">Unused</param>
		/// <param name="position">Used to make text not all move together</param>
		/// <param name="currentChar">Unused</param>
		/// <param name="args">Unused</param>
		/// <returns>A <c>Vector2</c> to be added to letter position.</returns>
		internal static Vector2 Shake(GameTime gameTime, int charNum, Vector2 position, char currentChar, string[] args)
		{
			float counter = (float)gameTime.TotalGameTime.TotalSeconds + position.X + position.Y;
			counter *= 30;
			return new Vector2(MathF.Sin(counter), MathF.Cos(counter * 1.3f)) / 40f;
		}
		/// <summary>
		/// A pulsing color, based on gametime.
		/// </summary>
		/// <param name="gameTime">Used to determine color.</param>
		/// <param name="baseColor">Unused</param>
		/// <param name="args">Unused</param>
		/// <returns>A <c>Color</c> that changes over time.</returns>
		internal static Color? Rainbow(GameTime gameTime, Color baseColor, string[] args)
		{
			// https://en.wikipedia.org/wiki/HSL_and_HSV#HSL_to_RGB
			float h = (float)gameTime.TotalGameTime.TotalSeconds * 180 % 360 / 60;
			float s = 1;
			float l = 0.5f;
			float C = (1 - MathF.Abs(2 * l - 1)) * s;
			float X = C * (1 - MathF.Abs(h % 2 - 1));
			if (h < 1)
			{
				return new Color(C, X, 0, baseColor.A);
			}
			if (h < 2)
			{
				return new Color(X, C, 0, baseColor.A);
			}
			if (h < 3)
			{
				return new Color(0, C, X, baseColor.A);
			}
			if (h < 4)
			{
				return new Color(0, X, C, baseColor.A);
			}
			if (h < 5)
			{
				return new Color(X, 0, C, baseColor.A);
			}
			return new Color(C, 0, X, baseColor.A);
		}
	}
}
