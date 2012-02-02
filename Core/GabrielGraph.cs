using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using DelaunayTriangulator;

namespace Gabriel_Graph
{
	public class GabrielGraph
	{
		private SpanningTree minimumSpanningTree;
		private Dictionary<DelaunayEdge, Path> edgesPaths;
		private Dictionary<Vertex, Path> verticesPaths;
		private Dictionary<DelaunayEdge, Path> minimumSpanningTreePaths;

		private const int VertexRadius = 5;
		private const int VertexTickness = 2;
		private const int EdgeTickness = 2;
		private static readonly Color VertexColor = Colors.Red;
		private static readonly Color EdgeColor = Colors.Blue;

		private const int MinimumSpanningTreeTickness = 4;
		private static readonly Color minimumSpanningTreeEdgeColor = Colors.Red;

		public List<Vertex> Vertices { get; set; }
		public List<DelaunayEdge> Edges { get; set; }

		public SpanningTree GetMinimumSpanningTree()
		{
			if (this.minimumSpanningTree == null)
			{
				this.minimumSpanningTree = SpanningTree.Create(this.Vertices, this.Edges);
			}
			return this.minimumSpanningTree;
		}

		public Path CreateEdgeLine(DelaunayEdge edge)
		{
			if (this.edgesPaths == null)
			{
				this.edgesPaths = new Dictionary<DelaunayEdge, Path>();
			}

			if (!this.edgesPaths.ContainsKey(edge))
			{
				LineGeometry geometry = new LineGeometry(new Point(edge.Start.X, edge.Start.Y), new Point(edge.End.X, edge.End.Y));
				geometry.Freeze();
				Path path = new Path();
				path.Data = geometry;
				path.StrokeThickness = EdgeTickness;
				path.Stroke = new SolidColorBrush(EdgeColor);
				this.edgesPaths[edge] = path;
			}
			return this.edgesPaths[edge];
		}

		public Path CreateVertexPoint(Vertex vertex)
		{
			if (this.verticesPaths == null)
			{
				this.verticesPaths = new Dictionary<Vertex, Path>();
			}

			if (!this.verticesPaths.ContainsKey(vertex))
			{
				EllipseGeometry geometry = new EllipseGeometry();
				geometry.Center = new Point(vertex.X, vertex.Y);
				geometry.RadiusX = geometry.RadiusY = VertexRadius;
				geometry.Freeze();
				Path path = new Path();
				path.StrokeThickness = VertexTickness;
				path.Stroke = new SolidColorBrush(VertexColor);
				path.Data = geometry;
				this.verticesPaths[vertex] = path;
			}
			return this.verticesPaths[vertex];
		}

		public Path CreateLineForMinSpanningTreeEdge(DelaunayEdge edge)
		{
			if (this.minimumSpanningTreePaths == null)
			{
				this.minimumSpanningTreePaths = new Dictionary<DelaunayEdge, Path>();
			}

			if (!this.minimumSpanningTreePaths.ContainsKey(edge))
			{
				LineGeometry geometry = new LineGeometry(new Point(edge.Start.X, edge.Start.Y), new Point(edge.End.X, edge.End.Y));
				geometry.Freeze();
				Path path = new Path();
				path.Data = geometry;
				path.StrokeThickness = MinimumSpanningTreeTickness;
				path.Stroke = new SolidColorBrush(Color.FromArgb(120, minimumSpanningTreeEdgeColor.A, minimumSpanningTreeEdgeColor.G, minimumSpanningTreeEdgeColor.B));
				this.minimumSpanningTreePaths[edge] = path;
			}
			return this.minimumSpanningTreePaths[edge];
		}
	}
}