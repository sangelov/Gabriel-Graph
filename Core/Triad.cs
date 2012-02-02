using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
copyright s-hull.org 2011
released under the contributors beerware license

contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
	public class Triad
	{
		private List<Vertex> points;

		public int A { get;set; }

		public int B { get;set; }

		public int C { get;set; }

		public int Ab { get;set; }

		public int Bc { get;set; }

		public int Ac { get;set; }// adjacent edges index to neighbouring triangle.

		// Position and radius squared of circumcircle
		public float CircumcircleR2 { get; set; }

		public float CircumcircleX { get; set; }

		public float CircumcircleY { get; set; }

		public Triad(int x, int y, int z, List<Vertex> points) 
		{
			this.points = points;
			A = x;
			B = y;
			C = z;
			Ab = -1;
			Bc = -1;
			Ac = -1; 
			CircumcircleR2 = -1; //x = 0; y = 0;
		}

		public Vertex VertexA
		{
			get
			{
				return points[this.A];
			}
		}

		public Vertex VertexB
		{
			get
			{
				return points[this.B];
			}
		}

		public Vertex VertexC
		{
			get
			{
				return points[this.C];
			}
		}

		public void Initialize(int a, int b, int c, int ab, int bc, int ac, List<Vertex> points)
		{
			this.A = a;
			this.B = b;
			this.C = c;
			this.Ab = ab;
			this.Bc = bc;
			this.Ac = ac;

			FindCircumcirclePrecisely(points);
		}

		/// <summary>
		/// If current orientation is not clockwise, swap b<->c
		/// </summary>
		internal void MakeClockwise(List<Vertex> points)
		{
			float centroidX = (points[A].X + points[B].X + points[C].X) / 3.0f;
			float centroidY = (points[A].Y + points[B].Y + points[C].Y) / 3.0f;

			float dr0 = points[A].X - centroidX, dc0 = points[A].Y - centroidY;
			float dx01 = points[B].X - points[A].X, dy01 = points[B].Y - points[A].Y;

			float df = -dx01 * dc0 + dy01 * dr0;
			if (df > 0)
			{
				// Need to swap vertices b<->c and edges ab<->bc
				int t = B;
				B = C;
				C = t;

				t = Ab;
				Ab = Ac;
				Ac = t;
			}
		}

		/// <summary>
		/// Find location and radius ^2 of the circumcircle (through all 3 points)
		/// This is the most critical routine in the entire set of code.  It must
		/// be numerically stable when the points are nearly collinear.
		/// </summary>
		public bool FindCircumcirclePrecisely(List<Vertex> points)
		{
			// Use coordinates relative to point `a' of the triangle
			Vertex pa = points[A], pb = points[B], pc = points[C];

			double xba = pb.X - pa.X;
			double yba = pb.Y - pa.Y;
			double xca = pc.X - pa.X;
			double yca = pc.Y - pa.Y;

			// Squares of lengths of the edges incident to `a'
			double balength = xba * xba + yba * yba;
			double calength = xca * xca + yca * yca;

			// Calculate the denominator of the formulae. 
			double d = xba * yca - yba * xca;
			if (d == 0)
			{
				CircumcircleX = 0;
				CircumcircleY = 0;
				CircumcircleR2 = -1;
				return false;
			}

			double denominator = 0.5 / d;

			// Calculate offset (from pa) of circumcenter
			double xC = (yca * balength - yba * calength) * denominator;
			double yC = (xba * calength - xca * balength) * denominator;

			double radius2 = xC * xC + yC * yC;
			if ((radius2 > 1e10 * balength || radius2 > 1e10 * calength))
			{
				CircumcircleX = 0;
				CircumcircleY = 0;
				CircumcircleR2 = -1;
				return false;
			}

			CircumcircleR2 = (float)radius2;
			CircumcircleX = (float)(pa.X + xC);
			CircumcircleY = (float)(pa.Y + yC);

			return true;
		}

		/// <summary>
		/// Return true iff Vertex p is inside the circumcircle of this triangle
		/// </summary>
		public bool InsideCircumcircle(Vertex p)
		{
			float dx = CircumcircleX - p.X;
			float dy = CircumcircleY - p.Y;
			float r2 = dx * dx + dy * dy;
			return r2 < CircumcircleR2;
		}

		/// <summary>
		/// Change any adjacent triangle index that matches fromIndex, to toIndex
		/// </summary>
		public void ChangeAdjacentIndex(int fromIndex, int toIndex)
		{
			if (Ab == fromIndex)
				Ab = toIndex;
			else if (Bc == fromIndex)
				Bc = toIndex;
			else if (Ac == fromIndex)
				Ac = toIndex;
			else
				Debug.Assert(false);
		}

		/// <summary>
		/// Determine which edge matches the triangleIndex, then which vertex the vertexIndex
		/// Set the indices of the opposite vertex, left and right edges accordingly
		/// </summary>
		public void FindAdjacency(int vertexIndex, int triangleIndex, out int indexOpposite, out int indexLeft, out int indexRight)
		{
			if (Ab == triangleIndex)
			{
				indexOpposite = C;

				if (vertexIndex == A)
				{
					indexLeft = Ac;
					indexRight = Bc;
				}
				else
				{
					indexLeft = Bc;
					indexRight = Ac;
				}
			}
			else if (Ac == triangleIndex)
			{
				indexOpposite = B;

				if (vertexIndex == A)
				{
					indexLeft = Ab;
					indexRight = Bc;
				}
				else
				{
					indexLeft = Bc;
					indexRight = Ab;
				}
			}
			else if (Bc == triangleIndex)
			{
				indexOpposite = A;

				if (vertexIndex == B)
				{
					indexLeft = Ab;
					indexRight = Ac;
				}
				else
				{
					indexLeft = Ac;
					indexRight = Ab;
				}
			}
			else
			{
				Debug.Assert(false);
				indexOpposite = indexLeft = indexRight = 0;
			}
		}

		public override string ToString()
		{
			return string.Format("Triad vertices {0} {1} {2} ; edges {3} {4} {5}", A, B, C, Ab, Ac, Bc);
		}
	}
}