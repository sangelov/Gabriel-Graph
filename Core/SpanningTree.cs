using System.Collections.Generic;
using DelaunayTriangulator;

namespace Gabriel_Graph
{
	public class SpanningTree
	{
		public IEnumerable<DelaunayEdge> Edges { get; set; }

		public static SpanningTree Create(List<Vertex> vertices, List<DelaunayEdge> edges)
		{
			Dictionary<Vertex, int> vertexIndex = new Dictionary<Vertex, int>();

			int i = 0;
			foreach (var v in vertices)
			{
				vertexIndex[v] = i++;
			}

			List<DelaunayEdge> mstEdges = new List<DelaunayEdge>();
			var sortedEdges = edges.Quicksort();

			int[] parents = new int[vertices.Count];
			for (int j = 0; j < vertices.Count; j++)
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
				if (mstEdges.Count == vertices.Count - 1)
				{
					break;
				}
			}

			return new SpanningTree()
			{
				Edges = mstEdges
			};
		}

		private static int GetRootVertexIndex(int[] roots, int vertexIndex)
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