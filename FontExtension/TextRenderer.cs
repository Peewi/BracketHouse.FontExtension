using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace BracketHouse.FontExtension
{
	public sealed class TextRenderer
	{
		private const string LargeTextTechnique = "LargeText";
		private const string SmallTextTechnique = "SmallText";
		private const string LargeStrokeTechnique = "LargeStroke";
		private const string SmallStrokeTechnique = "SmallStroke";
		private const string LargeStrokedTextTechnique = "LargeStrokedText";
		private const string SmallStrokedTextTechnique = "SmallStrokedText";

		private readonly Effect Effect;
		private readonly FieldFont Font;
		private readonly Texture2D AtlasTexture;
		private readonly GraphicsDevice Device;

		private FontVertex[] LayoutVertices = new FontVertex[100 * 4];
		private int[] LayoutIndices = new int[100 * 6];
		private int GlyphsLayouted = 0;
		private List<(Texture2D texture, Rectangle srcRect, Rectangle destRect, float rot)> Sprites = new List<(Texture2D texture, Rectangle srcRect, Rectangle destRect, float rot)>();

		private static bool Initialized = false;
		private static Effect SharedEffect;
		private static Matrix Shared2DMatrix;
		/// <summary>
		/// Create <c>TextRenderer</c> for a given font, using the shader from the library.
		/// Make sure to call <c>TextRenderer.Initialize</c> first.
		/// </summary>
		/// <param name="font"><c>FieldFont</c> that will be used by this renderer.</param>
		/// <param name="device">Used for rendering.</param>
		/// <param name="effect">Shader to use for text rendering. Will use shader provided by library if <c>null</c>.</param>
		/// <param name="content">Used for loading the shader that will be used for text rendering.</param>
		/// <exception cref="InvalidOperationException">Occurs when <c>TextRenderer.Initialize</c> has not been called first.</exception>
		public TextRenderer(FieldFont font, GraphicsDevice device, Effect effect = null)
		{
			if (!Initialized)
			{
				throw new InvalidOperationException("Call TextRenderer.Initialize first.");
			}
			Effect = effect ?? SharedEffect;
			Font = font;
			Device = device;
			using (var stream = new MemoryStream(font.Bitmap))
			{
				AtlasTexture = Texture2D.FromStream(Device, stream);
			}
			UseScreenSpace = true;
			EnableKerning = true;
			OptimizeForTinyText = false;
			PositiveYIsDown = true;
			PositionByBaseline = false;
		}
		/// <summary>
		/// Extract shader from assembly, load it, and delete the temp file.
		/// </summary>
		/// <param name="content">ContentManager to load shader with.</param>
		/// <returns>The field font effect, compiled for the current platform.</returns>
		private static Effect LoadDefaultShader(ContentManager content)
		{
			string shaderName = "FieldFontEffectWinDX";
			switch (PlatformInfo.GraphicsBackend)
			{
				case GraphicsBackend.DirectX:
					shaderName = "FieldFontEffectWinDX";
					break;
				case GraphicsBackend.OpenGL:
					shaderName = "FieldFontEffectDesktopGL";
					break;
				case GraphicsBackend.Vulkan:
					break;
				case GraphicsBackend.Metal:
					break;
			}
			shaderName = $"BracketHouse.FontExtension.{shaderName}";
			string tempName = $"{Path.GetTempFileName()}";
			using (Stream shader = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{shaderName}.xnb"))
			{
				using (FileStream target = File.Create($"{tempName}.xnb"))
				{
					target.Seek(0, SeekOrigin.Begin);
					shader.CopyTo(target);
				}	
			}
			var retval = content.Load<Effect>(tempName);
			File.Delete($"{tempName}.xnb");
			return retval;
		}
		/// <summary>
		/// Make sure <c>Shared2DMatrix</c> is kept updated.
		/// </summary>
		/// <param name="deviceMan"></param>
		/// <param name="window"></param>
		public static void Initialize(GraphicsDeviceManager deviceMan, GameWindow window, ContentManager content)
		{
			Initialized = true;
			SharedEffect = LoadDefaultShader(content);
			// Update the shared 2D matrix every time resolution is changed.
			window.ClientSizeChanged += (sender, e) =>
			{ // ClientSizeChanged is only necessary in DesktopGL, as in WinDX PreparingDeviceSettings is raised when changing window size and is therefore enough.
				SetShared2DMatrix(window.ClientBounds.Width, window.ClientBounds.Height);
			};
			deviceMan.PreparingDeviceSettings += (sender, e) =>
			{
				int width = e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth;
				int height = e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight;
				SetShared2DMatrix(width, height);
			};
			// Set the shared 2D matrix immediately.
			SetShared2DMatrix(deviceMan.PreferredBackBufferWidth, deviceMan.PreferredBackBufferHeight);
		}
		/// <summary>
		/// Set <c>Shared2DMatrix</c> so that drawing will be in screen coordinates.
		/// </summary>
		/// <param name="width">Screen width</param>
		/// <param name="height">Screen height</param>
		private static void SetShared2DMatrix(int width, int height)
		{
			Vector3 camTarget = new Vector3(0, 0, 100f);
			Vector3 camPosition = new Vector3(0, 0, 0f);
			Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, -height, 0, 0, 200);
			Matrix viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
			Matrix worldMatrix = Matrix.CreateWorld(camTarget, Vector3.Forward, Vector3.Down);
			Shared2DMatrix = worldMatrix * viewMatrix * projectionMatrix;
		}
		/// <summary>
		/// Layouting setting. Use kerning when layouting text.
		/// </summary>
		public bool EnableKerning { get; set; }
		/// <summary>
		/// Rendering setting. Disables text anti-aliasing which might cause blurry text when the text is rendered tiny
		/// </summary>
		public bool OptimizeForTinyText { get; set; }
		/// <summary>
		/// Layouting setting. Flip the Y axis, so that positive Y is down. It is up to you to provide a wvp matrix where that makes sense.
		/// </summary>
		public bool PositiveYIsDown { get; set; }
		/// <summary>
		/// Layouting setting. Position text by the baseline of the first line of text, instead of by the top.
		/// </summary>
		public bool PositionByBaseline { get; set; }
		/// <summary>
		/// Whether to use an auto-generated matrix that renders in screenspace coordinates.
		/// </summary>
		public bool UseScreenSpace { get; set; }
		/// <summary>
		/// WorldViewProjection Matrix to use during rendering when <c>UseScreenSpace</c> is false.
		/// </summary>
		public Matrix WorldViewProjection { get; set; }

		/// <summary>
		/// Create a matrix and set <c>WorldViewProjection</c> to it. Also sets <c>PositiveYIsDown = true</c>.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void SetOrtographicProjection(int width, int height)
		{
			PositiveYIsDown = true;
			Vector3 camTarget = new Vector3(0, 0, 100f);
			Vector3 camPosition = new Vector3(0, 0, 0f);
			Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, -height, 0, 0, 200);
			Matrix viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
			Matrix worldMatrix = Matrix.CreateWorld(camTarget, Vector3.Forward, Vector3.Down);

			WorldViewProjection = worldMatrix * viewMatrix * projectionMatrix;
		}

		/// <summary>
		/// Start over with layouting.
		/// </summary>
		public void ResetLayout()
		{
			GlyphsLayouted = 0;
			Sprites.Clear();
		}
		/// <summary>
		/// Perform layouting with rotation for a string so that the text can be rendered.
		/// </summary>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position to draw to.</param>
		/// <param name="depth">Z coordinate to use for glyph vertices</param>
		/// <param name="lineHeight">Override lineheight of font.</param>
		/// <param name="scale">How large to draw the text.</param>
		/// <param name="color">Color to draw text.</param>
		/// <param name="strokeColor">Color to draw text outlines.</param>
		/// <param name="kerning">Override <c>EnableKerning</c> property.</param>
		/// <param name="yIsDown">Override <c>PositiveYIsDown</c> property.</param>
		/// <param name="positionByBaseline">Override <c>PositionByBaseline</c> property.</param>
		/// <param name="rotation">Amount of rotation in radians</param>
		/// <param name="origin">Point to rotate around, relative to position</param>
		/// <param name="formatting">Whether to parse and apply formatting tags</param>
		/// <param name="gameTime"></param>
		/// <param name="maxChars">Stop after this many characters, not counting formatting tags. Negative numbers indicate no limit.</param>
		void LayoutText(string text, Vector2 position, float depth, float lineHeight, float scale, Color color, Color strokeColor, bool kerning, bool yIsDown, bool positionByBaseline, float rotation, Vector2 origin, bool formatting, GameTime gameTime, int maxChars)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (GlyphsLayouted + text.Length > LayoutVertices.Length / 4)
			{
				SetLayoutCacheSize(GlyphsLayouted + text.Length);
			}
			int yFlip = yIsDown ? -1 : 1;

			Vector2 advanceDir = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
			Vector2 upDir = yFlip * new Vector2(advanceDir.Y, -advanceDir.X);

			Color currentFill = color;
			Color currentStroke = strokeColor;
			float currentLineHeight = lineHeight;
			float currentScale = scale;
			Vector2 currentOffset = Vector2.Zero;
			bool currentKerning = kerning;
			Formatting.LetterPositionDelegate currentLetterDelegate = null;
			string[] currentLetterArgs = null;

			Vector2 cursorStart = position;
			// Rotation math: https://matthew-brett.github.io/teaching/rotation_2d.html
			Vector2 rotOrigin = new Vector2(
				advanceDir.X * -origin.X - advanceDir.Y * -origin.Y,
				advanceDir.Y * -origin.X + advanceDir.X * -origin.Y
			);
			cursorStart += origin + rotOrigin;
			if (!positionByBaseline)
			{
				cursorStart += upDir * scale * Font.Ascender * -1;
			}
			Vector2 cursor = cursorStart;
			int currentLine = 0;
			int numChars = 0;
			for (var i = 0; i < text.Length; i++)
			{
				if (maxChars >= 0 && numChars >= maxChars)
				{
					break;
				}
				FieldGlyph current = Font.GetGlyph(text[i]);
				bool skipLetter = char.IsWhiteSpace(text[i]);
				bool skipAdvance = false;
				if (formatting && text[i] == '[')
				{
					var (returnValue, tagType, args, tagStringLength) = Formatting.FindAndExecuteTag(text, i, gameTime, color, strokeColor);
					if (tagType != Formatting.TagType.Unknown)
					{
						skipLetter = true;
						skipAdvance = true;
						i += tagStringLength;
					}
					switch (tagType)
					{
						case Formatting.TagType.Unknown:
							break;
						case Formatting.TagType.FillColor:
							currentFill = (Color)returnValue;
							break;
						case Formatting.TagType.StrokeColor:
							currentStroke = (Color)returnValue;
							break;
						case Formatting.TagType.PositionOffset:
							currentOffset = (Vector2)returnValue;
							break;
						case Formatting.TagType.LetterPositionOffset:
							currentLetterDelegate = (Formatting.LetterPositionDelegate)returnValue;
							currentLetterArgs = args;
							break;
						case Formatting.TagType.Scale:
							currentScale = (float)returnValue * scale;
							break;
						case Formatting.TagType.LineHeight:
							currentLineHeight = (float)returnValue * lineHeight;
							break;
						case Formatting.TagType.Kerning:
							currentKerning = (bool)returnValue;
							break;
						case Formatting.TagType.Sprite:
							var (texture, srcRect, width) = ((Texture2D texture, Rectangle srcRect, float width))returnValue;
							int destWidth = (int)(width * currentScale);
							int destHeight;
							if (srcRect == null)
							{
								destHeight = (int)((float)texture.Height / texture.Width * width * currentScale);
							}
							else
							{
								destHeight = (int)((float)srcRect.Height / srcRect.Width * width * currentScale);
							}
							Rectangle dest = new Rectangle((int)cursor.X, (int)cursor.Y - destHeight, destWidth, destHeight);
							Sprites.Add((texture, srcRect, dest, rotation));
							cursor += advanceDir * width * currentScale;
							break;
						case Formatting.TagType.Special:
							((Formatting.SpecialDelegate)returnValue).Invoke(gameTime, i, cursor, currentFill, currentStroke, args);
							break;
						case Formatting.TagType.EndFormat:
							Formatting.TagType Ends = (Formatting.TagType)returnValue;
							if (Ends.HasFlag(Formatting.TagType.FillColor))
							{
								currentFill = color;
							}
							if (Ends.HasFlag(Formatting.TagType.StrokeColor))
							{
								currentStroke = strokeColor;
							}
							if (Ends.HasFlag(Formatting.TagType.PositionOffset))
							{
								currentOffset = Vector2.Zero;
							}
							if (Ends.HasFlag(Formatting.TagType.LetterPositionOffset))
							{
								currentLetterDelegate = null;
								currentLetterArgs = null;
							}
							if (Ends.HasFlag(Formatting.TagType.Scale))
							{
								currentScale = scale;
							}
							if (Ends.HasFlag(Formatting.TagType.LineHeight))
							{
								currentLineHeight = lineHeight;
							}
							if (Ends.HasFlag(Formatting.TagType.Kerning))
							{
								currentKerning = kerning;
							}
							break;
						default:
							break;
					}
				}
				if (!skipLetter)
				{
					Vector2 letterOffset = Vector2.Zero;
					if (currentLetterDelegate != null)
					{
						letterOffset = currentLetterDelegate.Invoke(gameTime, i, (cursor - cursorStart) / currentScale, text[i], currentLetterArgs);
					}
					Vector2 rotLeft = advanceDir * current.PlaneLeft * currentScale;
					Vector2 rotRight = advanceDir * current.PlaneRight * currentScale;
					Vector2 rotTop = upDir * current.PlaneTop * currentScale;
					Vector2 rotBottom = upDir * current.PlaneBottom * currentScale;

					LayoutVertices[GlyphsLayouted * 4 + 0].Position = new Vector3(cursor + (currentOffset + letterOffset) * currentScale + rotRight + rotBottom, depth);
					LayoutVertices[GlyphsLayouted * 4 + 1].Position = new Vector3(cursor + (currentOffset + letterOffset) * currentScale + rotLeft + rotBottom, depth);
					LayoutVertices[GlyphsLayouted * 4 + 2].Position = new Vector3(cursor + (currentOffset + letterOffset) * currentScale + rotLeft + rotTop, depth);
					LayoutVertices[GlyphsLayouted * 4 + 3].Position = new Vector3(cursor + (currentOffset + letterOffset) * currentScale + rotRight + rotTop, depth);

					LayoutVertices[GlyphsLayouted * 4 + 0].TextureCoordinate.X = current.AtlasRight;
					LayoutVertices[GlyphsLayouted * 4 + 0].TextureCoordinate.Y = current.AtlasBottom;

					LayoutVertices[GlyphsLayouted * 4 + 1].TextureCoordinate.X = current.AtlasLeft;
					LayoutVertices[GlyphsLayouted * 4 + 1].TextureCoordinate.Y = current.AtlasBottom;

					LayoutVertices[GlyphsLayouted * 4 + 2].TextureCoordinate.X = current.AtlasLeft;
					LayoutVertices[GlyphsLayouted * 4 + 2].TextureCoordinate.Y = current.AtlasTop;

					LayoutVertices[GlyphsLayouted * 4 + 3].TextureCoordinate.X = current.AtlasRight;
					LayoutVertices[GlyphsLayouted * 4 + 3].TextureCoordinate.Y = current.AtlasTop;

					LayoutVertices[GlyphsLayouted * 4 + 0].Color = currentFill;
					LayoutVertices[GlyphsLayouted * 4 + 1].Color = currentFill;
					LayoutVertices[GlyphsLayouted * 4 + 2].Color = currentFill;
					LayoutVertices[GlyphsLayouted * 4 + 3].Color = currentFill;

					LayoutVertices[GlyphsLayouted * 4 + 0].StrokeColor = currentStroke;
					LayoutVertices[GlyphsLayouted * 4 + 1].StrokeColor = currentStroke;
					LayoutVertices[GlyphsLayouted * 4 + 2].StrokeColor = currentStroke;
					LayoutVertices[GlyphsLayouted * 4 + 3].StrokeColor = currentStroke;

					LayoutIndices[GlyphsLayouted * 6 + 0] = GlyphsLayouted * 4 + 0;
					LayoutIndices[GlyphsLayouted * 6 + 1] = GlyphsLayouted * 4 + 1;
					LayoutIndices[GlyphsLayouted * 6 + 2] = GlyphsLayouted * 4 + 2;
					LayoutIndices[GlyphsLayouted * 6 + 3] = GlyphsLayouted * 4 + 2;
					LayoutIndices[GlyphsLayouted * 6 + 4] = GlyphsLayouted * 4 + 3;
					LayoutIndices[GlyphsLayouted * 6 + 5] = GlyphsLayouted * 4 + 0;

					GlyphsLayouted++;
				}

				if (!skipAdvance)
				{
					numChars++;
					cursor += advanceDir * current.Advance * currentScale;

					if (currentKerning && i < text.Length - 1)
					{
						if (Font.Kerning.TryGetValue((text[i], text[i + 1]), out float kern))
						{
							cursor += advanceDir * kern * currentScale;
						}
					}
					if (text[i] == '\n')
					{
						currentLine++;
						cursor = cursorStart + upDir * currentLineHeight * currentScale * currentLine;
					}
				}
			}
		}
		/// <summary>
		/// Perform layouting for a string so that the text can be rendered.
		/// </summary>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position to draw to.</param>
		/// <param name="depth">Z coordinate to use for glyph vertices</param>
		/// <param name="lineHeight">Override lineheight of font.</param>
		/// <param name="scale">How large to draw the text.</param>
		/// <param name="color">Color to draw text.</param>
		/// <param name="strokeColor">Color to draw text outlines.</param>
		/// <param name="kerning">Override <c>EnableKerning</c> property.</param>
		/// <param name="yIsDown">Override <c>PositiveYIsDown</c> property.</param>
		/// <param name="positionByBaseline">Override <c>PositionByBaseline</c> property.</param>
		void SimpleLayoutText(string text, Vector2 position, float depth, float lineHeight, float scale, Color color, Color strokeColor, bool kerning, bool yIsDown, bool positionByBaseline, int maxChars)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (GlyphsLayouted + text.Length > LayoutVertices.Length / 4)
			{
				SetLayoutCacheSize(GlyphsLayouted + text.Length);
			}
			int yFlip = yIsDown ? -1 : 1;

			Vector2 penStart = position;
			if (!positionByBaseline)
			{
				penStart.Y += scale * Font.Ascender * yFlip;
			}
			Vector2 pen = penStart;
			int numChars = 0;

			for (var i = 0; i < text.Length; i++)
			{
				if (maxChars >= 0 && numChars >= maxChars)
				{
					break;
				}
				FieldGlyph current = Font.GetGlyph(text[i]);

				if (!char.IsWhiteSpace(text[i]))
				{
					float left = pen.X + current.PlaneLeft * scale;
					float right = pen.X + current.PlaneRight * scale;
					float top = pen.Y - current.PlaneTop * scale * yFlip;
					float bottom = pen.Y - current.PlaneBottom * scale * yFlip;

					LayoutVertices[GlyphsLayouted * 4 + 0].Position.X = right;
					LayoutVertices[GlyphsLayouted * 4 + 0].Position.Y = bottom;
					LayoutVertices[GlyphsLayouted * 4 + 0].Position.Z = depth;

					LayoutVertices[GlyphsLayouted * 4 + 1].Position.X = left;
					LayoutVertices[GlyphsLayouted * 4 + 1].Position.Y = bottom;
					LayoutVertices[GlyphsLayouted * 4 + 1].Position.Z = depth;

					LayoutVertices[GlyphsLayouted * 4 + 2].Position.X = left;
					LayoutVertices[GlyphsLayouted * 4 + 2].Position.Y = top;
					LayoutVertices[GlyphsLayouted * 4 + 2].Position.Z = depth;

					LayoutVertices[GlyphsLayouted * 4 + 3].Position.X = right;
					LayoutVertices[GlyphsLayouted * 4 + 3].Position.Y = top;
					LayoutVertices[GlyphsLayouted * 4 + 3].Position.Z = depth;

					LayoutVertices[GlyphsLayouted * 4 + 0].TextureCoordinate.X = current.AtlasRight;
					LayoutVertices[GlyphsLayouted * 4 + 0].TextureCoordinate.Y = current.AtlasBottom;

					LayoutVertices[GlyphsLayouted * 4 + 1].TextureCoordinate.X = current.AtlasLeft;
					LayoutVertices[GlyphsLayouted * 4 + 1].TextureCoordinate.Y = current.AtlasBottom;

					LayoutVertices[GlyphsLayouted * 4 + 2].TextureCoordinate.X = current.AtlasLeft;
					LayoutVertices[GlyphsLayouted * 4 + 2].TextureCoordinate.Y = current.AtlasTop;

					LayoutVertices[GlyphsLayouted * 4 + 3].TextureCoordinate.X = current.AtlasRight;
					LayoutVertices[GlyphsLayouted * 4 + 3].TextureCoordinate.Y = current.AtlasTop;

					LayoutVertices[GlyphsLayouted * 4 + 0].Color = color;
					LayoutVertices[GlyphsLayouted * 4 + 1].Color = color;
					LayoutVertices[GlyphsLayouted * 4 + 2].Color = color;
					LayoutVertices[GlyphsLayouted * 4 + 3].Color = color;

					LayoutVertices[GlyphsLayouted * 4 + 0].StrokeColor = strokeColor;
					LayoutVertices[GlyphsLayouted * 4 + 1].StrokeColor = strokeColor;
					LayoutVertices[GlyphsLayouted * 4 + 2].StrokeColor = strokeColor;
					LayoutVertices[GlyphsLayouted * 4 + 3].StrokeColor = strokeColor;

					LayoutIndices[GlyphsLayouted * 6 + 0] = GlyphsLayouted * 4 + 0;
					LayoutIndices[GlyphsLayouted * 6 + 1] = GlyphsLayouted * 4 + 1;
					LayoutIndices[GlyphsLayouted * 6 + 2] = GlyphsLayouted * 4 + 2;
					LayoutIndices[GlyphsLayouted * 6 + 3] = GlyphsLayouted * 4 + 2;
					LayoutIndices[GlyphsLayouted * 6 + 4] = GlyphsLayouted * 4 + 3;
					LayoutIndices[GlyphsLayouted * 6 + 5] = GlyphsLayouted * 4 + 0;

					GlyphsLayouted++;
				}

				pen.X += current.Advance * scale;

				if (kerning && i < text.Length - 1)
				{
					numChars++;
					if (Font.Kerning.TryGetValue((text[i], text[i + 1]), out float kern))
					{
						pen.X += kern * scale;
					}
				}
				if (text[i] == '\n')
				{
					pen.X = penStart.X;
					pen.Y -= lineHeight * scale * yFlip;
				}
			}
		}
		/// <summary>
		/// Perform layouting with rotation for a string so that the text can be rendered, parsing formatting tags..
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position to draw to.</param>
		/// <param name="color">Color to draw text.</param>
		/// <param name="scale">How large to draw the text.</param>
		/// <param name="rotation">Amount of rotation in radians</param>
		/// <param name="origin">Point to rotate around, relative to position</param>
		/// <param name="maxChars">Stop after this many characters, not counting formatting tags. Negative numbers indicate no limit.</param>
		public void LayoutText(GameTime gameTime, string text, Vector2 position, Color color, Color strokeColor, float scale, float rotation, Vector2 origin, int maxChars = -1)
		{
			LayoutText(text, position, 1f, Font.LineHeight, scale, color, strokeColor, EnableKerning, PositiveYIsDown, PositionByBaseline, rotation, origin, true, gameTime, maxChars);
		}
		/// <summary>
		/// Perform layouting with rotation, but ignoring formatting tags, for a string so that the text can be rendered.
		/// </summary>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position to draw to.</param>
		/// <param name="color">Color to draw text.</param>
		/// <param name="strokeColor">Color to draw text outlines.</param>
		/// <param name="scale">How large to draw the text.</param>
		/// <param name="rotation">Amount of rotation in radians</param>
		/// <param name="origin">Point to rotate around, relative to position</param>
		/// <param name="maxChars">Stop after this many characters. Negative numbers indicate no limit.</param>
		public void LayoutText(string text, Vector2 position, Color color, Color strokeColor, float scale, float rotation, Vector2 origin, int maxChars = -1)
		{
			LayoutText(text, position, 1f, Font.LineHeight, scale, color, strokeColor, EnableKerning, PositiveYIsDown, PositionByBaseline, rotation, origin, false, null, maxChars);
		}
		/// <summary>
		/// Perform layouting for a string, parsing formatting tags, so that the text can be rendered.
		/// </summary>
		/// <param name="gameTime">Used for animated text effects.</param>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position to draw to.</param>
		/// <param name="color">Color to draw text.</param>
		/// <param name="strokeColor">Color to draw text outlines.</param>
		/// <param name="scale">How large to draw the text.</param>
		/// <param name="maxChars">Stop after this many characters, not counting formatting tags.</param>
		public void LayoutText(GameTime gameTime, string text, Vector2 position, Color color, Color strokeColor, float scale = 16, int maxChars = -1)
		{
			LayoutText(text, position, 1f, Font.LineHeight, scale, color, strokeColor, EnableKerning, PositiveYIsDown, PositionByBaseline, 0, Vector2.Zero, true, gameTime, maxChars);
		}
		/// <summary>
		/// Perform layouting for a string so that the text can be rendered.
		/// </summary>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position to draw to.</param>
		/// <param name="color">Color to draw text.</param>
		/// <param name="strokeColor">Color to draw text outlines.</param>
		/// <param name="scale">How large to draw the text.</param>
		/// <param name="maxChars">Stop after this many characters, not counting formatting tags.</param>
		public void SimpleLayoutText(string text, Vector2 position, Color color, Color strokeColor, float scale = 16, int maxChars = -1)
		{
			SimpleLayoutText(text, position, 1f, Font.LineHeight, scale, color, strokeColor, EnableKerning, PositiveYIsDown, PositionByBaseline, maxChars);
		}
		/// <summary>
		/// Render text with outline that has been layouted since last use of ResetLayout, overriding settings from TextRenderer.
		/// </summary>
		/// <param name="worldViewProjection">WorldViewProjection Matrix to use during rendering.</param>
		/// <param name="tinyText">Disables text anti-aliasing which might cause blurry text when the text is rendered tiny</param>
		private void RenderStrokedText(Matrix worldViewProjection, bool tinyText)
		{
			if (GlyphsLayouted == 0)
			{
				return;
			}
			var textureWidth = AtlasTexture.Width;
			var textureHeight = AtlasTexture.Height;
			Effect.Parameters["WorldViewProjection"].SetValue(UseScreenSpace ? Shared2DMatrix : worldViewProjection);
			Effect.Parameters["PxRange"].SetValue(Font.PxRange);
			Effect.Parameters["TextureSize"].SetValue(new Vector2(textureWidth, textureHeight));
			Effect.Parameters["GlyphTexture"].SetValue(AtlasTexture);
			if (tinyText)
			{
				Effect.CurrentTechnique = Effect.Techniques[SmallStrokedTextTechnique];
			}
			else
			{
				Effect.CurrentTechnique = Effect.Techniques[LargeStrokedTextTechnique];
			}
			Effect.CurrentTechnique.Passes[0].Apply();
			Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, LayoutVertices, 0, GlyphsLayouted * 4, LayoutIndices, 0, GlyphsLayouted * 2);
		}
		/// <summary>
		/// Render text with outline that has been layouted since last use of ResetLayout, overriding WorldViewProjection matrix from TextRenderer.
		/// </summary>
		/// <param name="worldViewProjection">WorldViewProjection Matrix to use during rendering.</param>
		private void RenderStrokedText(Matrix worldViewProjection)
		{
			RenderStrokedText(worldViewProjection, OptimizeForTinyText);
		}
		/// <summary>
		/// Render text with outline that has been layouted since last use of ResetLayout.
		/// </summary>
		public void RenderStrokedText()
		{
			RenderStrokedText(WorldViewProjection, OptimizeForTinyText);
		}
		/// <summary>
		/// Render text that has been layouted since last use of ResetLayout, overriding settings from TextRenderer.
		/// </summary>
		/// <param name="worldViewProjection">WorldViewProjection Matrix to use during rendering.</param>
		/// <param name="tinyText">Disables text anti-aliasing which might cause blurry text when the text is rendered tiny</param>
		private void RenderText(Matrix worldViewProjection, bool tinyText)
		{
			if (GlyphsLayouted == 0)
			{
				return;
			}
			var textureWidth = AtlasTexture.Width;
			var textureHeight = AtlasTexture.Height;
			Effect.Parameters["WorldViewProjection"].SetValue(UseScreenSpace ? Shared2DMatrix : worldViewProjection);
			Effect.Parameters["PxRange"].SetValue(Font.PxRange);
			Effect.Parameters["TextureSize"].SetValue(new Vector2(textureWidth, textureHeight));
			Effect.Parameters["GlyphTexture"].SetValue(AtlasTexture);
			if (tinyText)
			{
				Effect.CurrentTechnique = Effect.Techniques[SmallTextTechnique];
			}
			else
			{
				Effect.CurrentTechnique = Effect.Techniques[LargeTextTechnique];
			}
			Effect.CurrentTechnique.Passes[0].Apply();
			Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, LayoutVertices, 0, GlyphsLayouted * 4, LayoutIndices, 0, GlyphsLayouted * 2);
		}
		/// <summary>
		/// Render text that has been layouted since last use of ResetLayout, overriding WorldViewProjection matrix from TextRenderer.
		/// </summary>
		/// <param name="worldViewProjection">WorldViewProjection Matrix to use during rendering.</param>
		private void RenderText(Matrix worldViewProjection)
		{
			RenderText(worldViewProjection, OptimizeForTinyText);
		}
		/// <summary>
		/// Render text that has been layouted since last use of ResetLayout.
		/// </summary>
		public void RenderText()
		{
			RenderText(WorldViewProjection, OptimizeForTinyText);
		}
		/// <summary>
		/// Render text that has been layouted since last use of ResetLayout, as outlines.
		/// </summary>
		/// <param name="worldViewProjection"></param>
		/// <param name="tinyText"></param>
		private void RenderStroke(Matrix worldViewProjection, bool tinyText)
		{
			if (GlyphsLayouted == 0)
			{
				return;
			}
			var textureWidth = AtlasTexture.Width;
			var textureHeight = AtlasTexture.Height;
			Effect.Parameters["WorldViewProjection"].SetValue(UseScreenSpace ? Shared2DMatrix : worldViewProjection);
			Effect.Parameters["PxRange"].SetValue(Font.PxRange);
			Effect.Parameters["TextureSize"].SetValue(new Vector2(textureWidth, textureHeight));
			Effect.Parameters["GlyphTexture"].SetValue(AtlasTexture);
			if (tinyText)
			{
				Effect.CurrentTechnique = Effect.Techniques[SmallStrokeTechnique];
			}
			else
			{
				Effect.CurrentTechnique = Effect.Techniques[LargeStrokeTechnique];
			}
			Effect.CurrentTechnique.Passes[0].Apply();
			Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, LayoutVertices, 0, GlyphsLayouted * 4, LayoutIndices, 0, GlyphsLayouted * 2);
		}
		/// <summary>
		/// Render outlines for layouted text.
		/// </summary>
		public void RenderStroke()
		{
			RenderStroke(WorldViewProjection, OptimizeForTinyText);
		}

		/// <summary>
		/// Change the sizes of LayoutVertices and LayoutIndices
		/// </summary>
		/// <param name="newSize">New capacity of glyph layout cache, in number of glyphs.</param>
		private void SetLayoutCacheSize(int newSize)
		{
			FontVertex[] newVerts = new FontVertex[newSize * 4];
			int[] newIndices = new int[newSize * 6];
			int copyAmount = Math.Min(newSize, LayoutVertices.Length / 4);
			for (int i = 0; i < copyAmount; i++)
			{
				newVerts[i * 4 + 0] = LayoutVertices[i * 4 + 0];
				newVerts[i * 4 + 1] = LayoutVertices[i * 4 + 1];
				newVerts[i * 4 + 2] = LayoutVertices[i * 4 + 2];
				newVerts[i * 4 + 3] = LayoutVertices[i * 4 + 3];
				newIndices[i * 6 + 0] = LayoutIndices[i * 6 + 0];
				newIndices[i * 6 + 1] = LayoutIndices[i * 6 + 1];
				newIndices[i * 6 + 2] = LayoutIndices[i * 6 + 2];
				newIndices[i * 6 + 3] = LayoutIndices[i * 6 + 3];
				newIndices[i * 6 + 4] = LayoutIndices[i * 6 + 4];
				newIndices[i * 6 + 5] = LayoutIndices[i * 6 + 5];
			}
			LayoutVertices = newVerts;
			LayoutIndices = newIndices;
		}
		/// <summary>
		/// Draw sprites from sprite tags.
		/// </summary>
		/// <param name="batch">Used to draw sprites.</param>
		public void DrawSprites(SpriteBatch batch)
		{
			foreach ((Texture2D texture, Rectangle srcRect, Rectangle destRect, float rot) in Sprites)
			{
				Vector2 origin = new Vector2(0, srcRect.Height);
				Rectangle realDest = destRect;
				realDest.Y += realDest.Height;
				batch.Draw(texture, realDest, srcRect, Color.White, rot, origin, SpriteEffects.None, 0);
			}
		}
	}
}
