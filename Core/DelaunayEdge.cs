using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelaunayTriangulator;

namespace Gabriel_Graph
{
	public class DelaunayEdge
	{
		private List<Triad> triads;
		private List<Vertex> points;
		private int start;
		private int end;
		private int neighbour1;
		private int neighbour2;

		public DelaunayEdge(List<Triad> triads, List<Vertex> points, int i1, int i2, int neigbour1, int neighbour2)
		{
			this.points = points;
			this.triads = triads;
			if (i1 < i2)
			{
				start = i1;
				end = i2;
			}
			else
			{
				start = i2;
				end = i1;
			}
			// let's make sure that neighbour1 always exists
			if (neighbour1 != -1)
			{
				this.neighbour1 = neigbour1;
				this.neighbour2 = neighbour2;
			}
			else
			{
				this.neighbour1 = neighbour2;
				this.neighbour2 = neighbour1;
			}
		}

		public Triad Neighbour1
		{
			get
			{
				if (neighbour1 >= 0 && neighbour1 < triads.Count)
				{
					return triads[neighbour1];
				}
				else
				{
					return null;
				}
			}
		}

		public Triad Neighbour2
		{
			get
			{
				if (neighbour2 >= 0 && neighbour2 < triads.Count)
				{
					return triads[neighbour2];
				}
				else
				{
					return null;
				}
			}
		}

		public Vertex Start
		{
			get
			{
				return this.points[start];
			}
		}

		public Vertex End
		{
			get
			{
				return this.points[end];
			}
		}
		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 31 + start;
			hash = hash * 31 + end;
			return hash;
		}
	}
}
