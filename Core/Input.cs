using System;
using System.Collections.Generic;
using System.Linq;
using DelaunayTriangulator;

namespace S_hull
{
	internal static class Input
	{
		public static List<Vertex> Points
		{
			get
			{
				List<Vertex> points = new List<Vertex>();
				Random rand = new Random();
				for (int i = 0; i < 40; i++)
				{
					points.Add(new Vertex(rand.Next(600), rand.Next(600)));
				}
				return points;
			}
		}
	}
}
