using System;
/*
  copyright s-hull.org 2011
  released under the contributors beerware license

  contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
	public class Vertex
	{
		public float X { get;set; }

		public float Y { get;set; }

		protected Vertex() { }

		public Vertex(float x, float y) 
		{
			this.X = x; this.Y = y;
		}

		public float Distance2To(Vertex other)
		{
			float dx = X - other.X;
			float dy = Y - other.Y;
			return dx * dx + dy * dy;
		}

		public float DistanceTo(Vertex other)
		{
			return (float)Math.Sqrt(Distance2To(other));
		}

		public override string ToString()
		{
			return string.Format("({0},{1})", X, Y);
		}
	}

}
