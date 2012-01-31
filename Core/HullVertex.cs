using System.Collections.Generic;

namespace DelaunayTriangulator
{
	/// <summary>
	/// Vertices belonging to the convex hull need to maintain a point and triad index
	/// </summary>
	internal class HullVertex : Vertex
	{
		public int PointsIndex { get; set; }
		public int TriadIndex { get; set; }

		public HullVertex(List<Vertex> points, int pointIndex)
		{
			X = points[pointIndex].X;
			Y = points[pointIndex].Y;
			PointsIndex = pointIndex;
			TriadIndex = 0;
		}
	}
}