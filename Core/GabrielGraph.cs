using System;
using System.Collections.Generic;
using System.Linq;
using DelaunayTriangulator;

namespace Gabriel_Graph
{
	public class GabrielGraph
	{
		private SpanningTree minimumSpanningTree;

		public List<Vertex> Vertices { get; set; }
		public List<DelaunayEdge> Edges { get; set; }

		public SpanningTree GetMinimumSpanningTree()
		{
			if (minimumSpanningTree == null)
			{
				Dictionary<Vertex, int> vertexIndex = new Dictionary<Vertex, int>();
				
				int i = 0;
				foreach (var v in Vertices)
				{
					vertexIndex[v] = i++;
				}

				List<DelaunayEdge> mstEdges = new List<DelaunayEdge>();
				var sortedEdges = this.Edges.Quicksort();

				int[] parents = new int[this.Vertices.Count];
				for (int j = 0; j < this.Vertices.Count; j++)
				{
					parents[j] = -1;
				}

				foreach (var edge in sortedEdges)
				{
					var startRoot = GetRootVertexIndex(parents, vertexIndex[edge.Start]);
					var endRoot = GetRootVertexIndex(parents, vertexIndex[edge.End]);
					if (startRoot != endRoot)
					{
						mstEdges.Add(edge);
						parents[endRoot] = startRoot;
					}
					if (mstEdges.Count == this.Vertices.Count - 1)
					{
						break;
					}
				}
				
				minimumSpanningTree = new SpanningTree()
				{
					Edges = mstEdges
				};
			}
			return minimumSpanningTree;
		}

		private int GetRootVertexIndex(int[] roots, int vertexIndex)
		{
			int current = vertexIndex;
			while (roots[current] != -1)
			{
				current = roots[current];
			}
			return current;
		}
	}
}