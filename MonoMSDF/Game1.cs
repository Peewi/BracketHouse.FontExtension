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
			TextRenderer.Initialize(graphics, Window);
			base.Initialize();
			frameWatch = new Stopwatch();
		}

		protected override void LoadContent()
		{
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
			graphics.ApplyChanges();
			mainFont = this.Content.Load<FieldFont>("arial");
			segoescriptFont = this.Content.Load<FieldFont>("segoescript");

			this.textRenderer = new TextRenderer(mainFont, GraphicsDevice, Content);
			this.segoescriptRenderer = new TextRenderer(segoescriptFont, GraphicsDevice, Content);
			this.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			this.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
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
			}
			else if (scroll < scrolled)
			{
				scale -= 0.1f;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				peakTicks = 0;
			}
			scrolled = scroll;
			var noformat = mainFont.MeasureString("[Red]Red");
			var yesformat = mainFont.MeasureString("[Red]Red", true);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			frameWatch.Restart();
			float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
			this.GraphicsDevice.Clear(Color.CornflowerBlue);

			var viewport = this.GraphicsDevice.Viewport;

			//var world = Matrix.CreateScale(0.01f) *  Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds);
			var world = Matrix.CreateScale(0.01f) * Matrix.Identity;
			var view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Forward, Vector3.Up);
			var projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.PiOver2,
				viewport.Width / (float)viewport.Height,
				0.01f,
				1000.0f);

			var wvp = world * view * projection;
			textRenderer.PositiveYIsDown = false;
			//textRenderer.WorldViewProjection = wvp;
			//this.textRenderer.ResetLayout();
			//this.textRenderer.LayoutText("→~!435&^%$", Vector2.Zero, Color.White, 32, MathF.Sin(totalTime) * 10);
			//this.textRenderer.RenderText(wvp);

			world = Matrix.CreateScale(0.01f) * Matrix.CreateRotationY(totalTime) * Matrix.CreateRotationZ(MathHelper.PiOver4);
			view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Forward, Vector3.Up);
			projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.PiOver2,
				viewport.Width / (float)viewport.Height,
				0.01f,
				1000.0f);

			wvp = world * view * projection;
			//this.textRenderer.ResetLayout();
			//this.textRenderer.LayoutText("To Infinity And Beyond!", Vector2.Zero, Color.Pink, Color.Black, 32, MathF.Sin(totalTime * 6) * 100);
			//this.textRenderer.RenderText(wvp);

			textRenderer.PositiveYIsDown = true;
			this.textRenderer.ResetLayout();
			//this.textRenderer.LayoutText("Look at this text!", new Vector2(0, 0), Color.Yellow, Color.Black, 32);
			//this.textRenderer.LayoutText("Text can be big.", new Vector2(0, 32), Color.Red, Color.Black, 64f);
			//this.textRenderer.LayoutText("Text can even be small.", new Vector2(0, 96), Color.White, 16f);
			//this.textRenderer.LayoutText("It's a piñata", new Vector2(0, 112), Color.Gold, Color.Black, 32);
			//this.textRenderer.LayoutText("Text with kerning:", new Vector2(0, 144), Color.Gold, Color.Black, 32);
			//this.textRenderer.LayoutText("AWAY", new Vector2(310, 144), Color.Gold, Color.Black, 32);
			//textRenderer.EnableKerning = false;
			//this.textRenderer.LayoutText("Text without kerning:", new Vector2(0, 172), Color.Red, Color.Black, 32, 20);
			//this.textRenderer.LayoutText("AWAY", new Vector2(310, 172), Color.Red, Color.Black, 32, 20);
			//textRenderer.EnableKerning = true;
			//this.textRenderer.LayoutText($"LESS BIG\nIN BACK", new Vector2(100, 300), Color.Blue, Color.Orange, 32 * 2, 0.1f);
			////this.textRenderer.LayoutText($"Hære's something. Comma: ,", new Vector2(0, 720-128), Color.Black, Color.Gold, 32);
			this.textRenderer.LayoutText($"Frame time: {frameTicks} ticks\nFrame time: {frameTime}ms\nPeak time: {peakTicks} ticks", new Vector2(0, 720 - 265), Color.Gold, Color.Black, 64);
			this.textRenderer.LayoutText($"Running for {gameTime.TotalGameTime.TotalSeconds} seconds", new Vector2(0, 720 - 40), Color.Gold, Color.Black, 32);
			//this.textRenderer.LayoutText($"REALLY BIG\nIN FRONT", new Vector2(0, 200), Color.Transparent, Color.Gold, 32 * 5, formatting: true);
			//string formatDemo1 = $"[scale 1.5][offset 1 0][#ff0000ff]Red[end scale]\n[kerning false][-128 0 0]Red[end kerning]\n[scale 1][fill green]Green\n[en\u200Bd scale][blue]Blue\n[end fill offset][stroke 128 0 0 100][nonsense]";
			string formatDemo1 = $"[\u200Bstroke white][\u200B#ff0000]Red[\u200Bfill 0 128 0]Green[\u200Bblue]Blue\nBecomes\n[stroke white][#ff0000]Red[fill 0 128 0]Green[blue]Blue";
			string formatDemo2 = $"[\u200Bscale 4][\u200Brainbow][\u200Bsine]RAINBOW\nBecomes\n\n\n[scale 4][rainbow][sine]RAINBOW";
			this.textRenderer.LayoutText(formatDemo1, new Vector2(20, 20), Color.White, Color.Black, 32, formatting: true, gameTime: gameTime);
			this.textRenderer.LayoutText(formatDemo2, new Vector2(300, 150), Color.White, Color.Black, 32, formatting: true, gameTime: gameTime);
			//this.textRenderer.RenderStroke();
			//this.textRenderer.RenderText();
			textRenderer.RenderStrokedText();
			
			this.segoescriptRenderer.ResetLayout();
			string cursorText = $"This is rotated.\nAnd a different font.";
			Vector2 ctMeasure = segoescriptFont.MeasureString(cursorText) * scale * 32;
			this.segoescriptRenderer.LayoutText(cursorText, Mouse.GetState().Position.ToVector2() - ctMeasure / 2, Color.Black, Color.White, scale * 32, totalTime, ctMeasure / 2);
			this.segoescriptRenderer.RenderStroke();
			this.segoescriptRenderer.RenderText();
			//segoescriptRenderer.RenderStrokedText();

			frameTicks = frameWatch.ElapsedTicks;
			peakTicks = Math.Max(peakTicks, frameTicks);
			frameTime = frameWatch.ElapsedMilliseconds;
			frameWatch.Stop();
		}
	}
}
