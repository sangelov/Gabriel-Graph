using System.Collections.Generic;
using DelaunayTriangulator;

namespace Gabriel_Graph
{
	public class GabrielGraph
	{
		public IEnumerable<Vertex> Vertices { get; set; }
		public IEnumerable<DelaunayEdge> Edges { get; set; }
	}
}