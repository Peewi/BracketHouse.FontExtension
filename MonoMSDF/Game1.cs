using BracketHouse.FontExtension;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace MonoMSDF
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private GraphicsDeviceManager graphics;
		private TextRenderer textRenderer;
		private TextRenderer segoescriptRenderer;
		FieldFont mainFont;
		FieldFont segoescriptFont;
		Stopwatch frameWatch;
		long frameTime = 0;
		long frameTicks = 0;
		long peakTicks = 0;
		float scale = 1;
		int scrolled = 0;
		int maxChars = 50;
		Texture2D Pixel;
		SpriteBatch spriteBatch;

		public Game1()
		{
			this.graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1280,
				PreferredBackBufferHeight = 720,
				SynchronizeWithVerticalRetrace = false,
				GraphicsProfile = GraphicsProfile.HiDef
			};
			IsFixedTimeStep = false;
			Window.AllowUserResizing = true;
			IsMouseVisible = true;
			this.Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
			frameWatch = new Stopwatch();
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void LoadContent()
		{
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
			graphics.ApplyChanges();
			TextRenderer.Initialize(graphics, Window, Content);
			mainFont = this.Content.Load<FieldFont>("arial");
			segoescriptFont = this.Content.Load<FieldFont>("segoescript");

			Pixel = new Texture2D(GraphicsDevice, 1, 1);
			Pixel.SetData( new Color[]{ Color.White });
			Formatting.RegisterTag("pixel", PixelIcon);

			this.textRenderer = new TextRenderer(mainFont, GraphicsDevice);
			this.segoescriptRenderer = new TextRenderer(segoescriptFont, GraphicsDevice);
			this.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			this.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		}
		/// <summary>
		/// Method for the [pixel] tag. Puts a white square in the text.
		/// </summary>
		/// <param name="gameTime">Unused</param>
		/// <param name="args">Unused</param>
		/// <returns>The texture for the sprite, which part of the sprite to draw (the whole thing in this case),
		/// and how wide the sprite should be in the text. The sprite will be scaled uniformly to achieve the desired width.
		/// Width will be multiplied with the text scale.</returns>
		(Texture2D texture, Rectangle srcRect, float width) PixelIcon(GameTime gameTime, string[] args)
		{
			return (Pixel, new Rectangle(0, 0, 1, 1), 0.75f);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
				|| Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			int scroll = Mouse.GetState().ScrollWheelValue;
			if (scroll > scrolled)
			{
				scale += 0.1f;
				maxChars++;
			}
			else if (scroll < scrolled)
			{
				scale -= 0.1f;
				maxChars--;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				peakTicks = 0;
			}
			scrolled = scroll;
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			frameWatch.Restart();
			float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
			this.GraphicsDevice.Clear(Color.CornflowerBlue);

			this.textRenderer.ResetLayout();
			this.textRenderer.SimpleLayoutText($"Frame time: {frameTicks} ticks\nFrame time: {frameTime}ms\nPeak time: {peakTicks} ticks", new Vector2(0, 720 - 265), Color.Gold, Color.Black, 64);
			this.textRenderer.SimpleLayoutText($"Running for {gameTime.TotalGameTime.TotalSeconds} seconds", new Vector2(0, 720 - 40), Color.Gold, Color.Black, 32);
			string formatDemo1 = $"[\u200Bstroke white][\u200Bfill #ff0000]Red[\u200Bfill 0 128 0]Green[\u200Bblue]Blue\nBecomes\n[stroke white][fill #ff0000]Red-[fill 0 128 0]Green-[blue]Blue";
			string formatDemo2 = $"[\u200Bscale 4][\u200Brainbow][\u200Bsine]RAINBOW\nBecomes\n\n\n[scale 4][rainbow][sine]RAINBOW";
			string formatDemo3 = $"Text can include icons\n(although this one is pure white):\nPress the [\u200bpixel] button!\nBecomes\nPress the [pixel] button!";
			textRenderer.LayoutText(gameTime, formatDemo1, new Vector2(20, 20), Color.White, Color.Black, 32);
			textRenderer.LayoutText(gameTime, formatDemo2, new Vector2(300, 150), Color.White, Color.Black, 32);
			textRenderer.LayoutText(gameTime, formatDemo3, new Vector2(800, 450), Color.White, Color.Black, 32);
			//this.textRenderer.RenderStroke();
			//this.textRenderer.RenderText();
			textRenderer.RenderStrokedText();
			
			this.segoescriptRenderer.ResetLayout();
			string cursorText = $"This[pixel]is rotated.\n[stroke gray]And a different font.";
			Vector2 ctMeasure = segoescriptFont.MeasureString(cursorText) * scale * 32;
			this.segoescriptRenderer.LayoutText(gameTime, cursorText, Mouse.GetState().Position.ToVector2() - ctMeasure / 2, Color.Black, Color.White, scale * 32, totalTime, ctMeasure / 2);
			this.segoescriptRenderer.RenderStroke();
			this.segoescriptRenderer.RenderText();
			//segoescriptRenderer.RenderStrokedText();

			spriteBatch.Begin();
			textRenderer.DrawSprites(spriteBatch);
			segoescriptRenderer.DrawSprites(spriteBatch);
			spriteBatch.End();

			frameTicks = frameWatch.ElapsedTicks;
			peakTicks = Math.Max(peakTicks, frameTicks);
			frameTime = frameWatch.ElapsedMilliseconds;
			frameWatch.Stop();
		}
	}
}
