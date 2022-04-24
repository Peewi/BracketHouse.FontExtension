using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BracketHouse.FontExtension
{
	/// <summary>
	/// Vertex type with position, texture coordinate and two colors.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct FontVertex : IVertexType
	{
		public Vector3 Position;
		public Color Color;
		public Color StrokeColor;
		public Vector2 TextureCoordinate;
		public static readonly VertexDeclaration VertexDeclaration;
		VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

		public override int GetHashCode()
		{
			return HashCode.Combine(Position, Color, StrokeColor, TextureCoordinate);
		}

		public static bool operator ==(FontVertex left, FontVertex right)
		{
			return (left.Position == right.Position) && (left.Color == right.Color) && (left.StrokeColor == right.StrokeColor) && (left.TextureCoordinate == right.TextureCoordinate);
		}

		public static bool operator !=(FontVertex left, FontVertex right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType() != base.GetType())
				return false;

			return (this == ((FontVertex)obj));
		}

		static FontVertex()
		{
			var elements = new VertexElement[]
			{
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
				new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 1),
				new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
			};
			VertexDeclaration = new VertexDeclaration(elements);
		}
	}
}
