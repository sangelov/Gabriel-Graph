using System;
using System.Collections.Generic;
using System.Linq;
using DelaunayTriangulator;

namespace S_hull
{
	internal static class Input
	{
		private const int numberOfPoints = 20;
		private const int maxCoordinate = 699;

		public static List<Vertex> Points
		{
			get
			{
				List<Vertex> points = new List<Vertex>();
				Random rand = new Random();
				for (int i = 0; i < numberOfPoints; i++)
				{
					points.Add(new Vertex(rand.Next(maxCoordinate), rand.Next(maxCoordinate)));
				}
				return points;
			}
		}
	}
}
