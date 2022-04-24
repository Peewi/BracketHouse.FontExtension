using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BracketHouse.FontExtension
{
	internal class FieldGlyph
	{
		[ContentSerializer] private readonly char CharacterBackend;
		[ContentSerializer] private readonly float AdvanceBackend;
		[ContentSerializer] private readonly float PlaneLeftBackend;
		[ContentSerializer] private readonly float PlaneTopBackend;
		[ContentSerializer] private readonly float PlaneRightBackend;
		[ContentSerializer] private readonly float PlaneBottomBackend;
		[ContentSerializer] private readonly float AtlasLeftBackend;
		[ContentSerializer] private readonly float AtlasTopBackend;
		[ContentSerializer] private readonly float AtlasRightBackend;
		[ContentSerializer] private readonly float AtlasBottomBackend;

		public FieldGlyph()
		{

		}

		public FieldGlyph(char c, float advance, float pl, float pr, float pt, float pb, float al, float ar, float at, float ab)
		{
			CharacterBackend = c;
			AdvanceBackend = advance;
			PlaneLeftBackend = pl;
			PlaneRightBackend = pr;
			PlaneTopBackend = pt;
			PlaneBottomBackend = pb;
			AtlasLeftBackend = al;
			AtlasRightBackend = ar;
			AtlasTopBackend = at;
			AtlasBottomBackend = ab;
		}
		/// <summary>
		/// The character these metrics are for.
		/// </summary>
		public char Character => this.CharacterBackend;
		/// <summary>
		/// How far forward to move the cursor.
		/// </summary>
		public float Advance => this.AdvanceBackend;
		/// <summary>
		/// Left X coodinate of where to draw glyph.
		/// </summary>
		public float PlaneLeft => this.PlaneLeftBackend;
		/// <summary>
		/// Right X coordinate of where to draw glyph.
		/// </summary>
		public float PlaneRight => this.PlaneRightBackend;
		/// <summary>
		/// Top Y coordinate of where to draw glyph.
		/// </summary>
		public float PlaneTop => this.PlaneTopBackend;
		/// <summary>
		/// Bottom Y coordinate of where to draw glyph.
		/// </summary>
		public float PlaneBottom => this.PlaneBottomBackend;
		/// <summary>
		/// Left X texture coordinate of glyph in atlas.
		/// </summary>
		public float AtlasLeft => this.AtlasLeftBackend;
		/// <summary>
		/// Top Y texture coordinate of glyph in atlas.
		/// </summary>
		public float AtlasTop => this.AtlasTopBackend;
		/// <summary>
		/// Right X texture coordinate of glyph in atlas.
		/// </summary>
		public float AtlasRight => this.AtlasRightBackend;
		/// <summary>
		/// Bottom Y texture coordinate of glyph in atlas.
		/// </summary>
		public float AtlasBottom => this.AtlasBottomBackend;
	}
}
