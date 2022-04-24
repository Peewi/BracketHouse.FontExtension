using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace BracketHouse.FontExtension
{

	[ContentProcessor(DisplayName = "Field Font Processor")]
	internal class FieldFontProcessor : ContentProcessor<FontDescription, FieldFont>
	{
		[DisplayName("msdf-atlas-gen path")]
		[Description("Path to the msdf-atlas-gen binary used to generate the multi-spectrum signed distance field")]
		[DefaultValue("msdf-atlas-gen.exe")]
		public virtual string ExternalPath { get; set; } = "msdf-atlas-gen.exe";


		[DisplayName("resolution")]
		[Description("Resolution of the the of the glyphs in the texture atlas")]
		[DefaultValue(32)]
		public virtual uint Resolution { get; set; } = 32;

		[DisplayName("range")]
		[Description("Distance field range, in pixels in the output texture")]
		[DefaultValue(4)]
		public virtual uint Range { get; set; } = 4;


		public override FieldFont Process(FontDescription input, ContentProcessorContext context)
		{
			var msdfgen = Path.Combine(Directory.GetCurrentDirectory(), this.ExternalPath);
			var objPath = Path.Combine(Directory.GetCurrentDirectory(), "obj");

			if (File.Exists(msdfgen))
			{
				var (atlasBitmap, atlasJSON) = CreateAtlas(input, msdfgen, objPath);
				var bytes = File.ReadAllBytes(atlasBitmap);
				return FieldFont.FromJsonAndBitmapBytes(atlasJSON, bytes);
			}

			throw new FileNotFoundException(
				"Could not find msdf-atlas-gen. Check your content processor parameters",
				msdfgen);
		}
		/// <summary>
		/// Run msdf-atlas-gen on our font
		/// </summary>
		/// <param name="font">Filename for the font</param>
		/// <param name="msdfgen">Path for the msdf-atlas-gen executable</param>
		/// <param name="objPath">Path for the folder to store the output in</param>
		/// <returns>Tuple of paths to atlas bitmap and atlas json</returns>
		private (string atlasBitmap, string atlasJSON) CreateAtlas(FontDescription font, string msdfgen, string objPath)
		{
			var name = Path.GetFileNameWithoutExtension(font.Path);
			var outputPath = Path.Combine(objPath, $"{name}-atlas.png");
			var charsetPath = Path.Combine(objPath, $"{name}-charset.txt");
			var jsonPath = Path.Combine(objPath, $"{name}-layout.json");
			string charset = new string(font.Characters);
			charset = charset.Replace("\\", "\\\\");
			charset = charset.Replace("\"", "\\\"");
			File.WriteAllText(charsetPath, $"\"{charset}\"");

			var startInfo = new ProcessStartInfo(msdfgen)
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				Arguments = $"-font \"{font.Path}\" -imageout \"{outputPath}\" -type mtsdf -charset \"{charsetPath}\" -size {this.Resolution} -pxrange {this.Range} -json \"{jsonPath}\" -yorigin top"
			};
			var process = System.Diagnostics.Process.Start(startInfo);
			if (process == null)
			{
				throw new InvalidOperationException("Could not start msdf-atlas-gen.exe");
			}
			process.WaitForExit();
			return (outputPath, jsonPath);
		}
	}
}