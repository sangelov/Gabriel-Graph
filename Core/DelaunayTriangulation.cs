using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DelaunayTriangulator;
using S_hull;

namespace Gabriel_Graph
{
	public class DelaunayTriangulation
	{
		private const int TriangleTickness = 1;
		private const int TriangleZIndex = 2;
		private static readonly Color triangleStrokeColor = Colors.Black;

		private const int CircleZIndex = 1;
		private const int CircleStrokeTickness = 3;
		private static readonly Color circleStrokeColor = Colors.Black;

		private const int GabrielEdgeTickness = 2;
		private static readonly Color gabrielEdgeColor = Colors.Blue;

		private List<Vertex> points;
		private List<Triad> triads;

		private Dictionary<Triad, Polygon> triadsDict;
		private Dictionary<Polygon, Triad> triangles;
		private Dictionary<Polygon, Path> circles;

		private HashSet<Vertex> gabrielVertices;
		private HashSet<DelaunayEdge> delaunayEdges;

		private const int GabrielVertexRadius = 3;
		private const int GabrielVertexTickness = 2;
		private static readonly Color gabrielVertexColor = Colors.Red;

		private const int MinimumSpanningTreeTickness = 4;
		private static readonly Color minimumSpanningTreeEdgeColor = Colors.Red;

		private GabrielGraph gabrielGraph;
		private HashSet<DelaunayEdge> gabrielEdges;

		private DelaunayTriangulation()
		{
		}

		public static DelaunayTriangulation Create()
		{
			DelaunayTriangulation delaunay = new DelaunayTriangulation();
			delaunay.points = Input.Points;
			delaunay.triangles = new Dictionary<Polygon, Triad>();
			delaunay.circles = new Dictionary<Polygon, Path>();
			delaunay.triadsDict = new Dictionary<Triad, Polygon>();
			Triangulator angulator = new Triangulator();
			delaunay.triads = angulator.Triangulation(delaunay.points, true);
			return delaunay;
		}

		public IEnumerable<Polygon> GetDelaunayTriangulationPolygons()
		{
			foreach (var triad in this.triads)
			{
				if (!triadsDict.ContainsKey(triad))
				{
					Polygon polygon = CreateTriangle(triad);
					this.triangles[polygon] = triad;
					this.triadsDict[triad] = polygon;
					yield return polygon;
				}
				else
				{
					yield return triadsDict[triad];
				}
			}
		}

		public Path CreateGabrielVertexPoint(Vertex vertex)
		{
			EllipseGeometry geometry = new EllipseGeometry();
			geometry.Center = new Point(vertex.X, vertex.Y);
			geometry.RadiusX = geometry.RadiusY = GabrielVertexRadius;
			geometry.Freeze();
			Path path = new Path();
			path.StrokeThickness = GabrielVertexTickness;
			path.Stroke = new SolidColorBrush(gabrielVertexColor);
			path.Data = geometry;
			return path;
		}

		public Path GetCircumCircle(Polygon polygon)
		{
			if (!circles.ContainsKey(polygon))
			{
				Triad triad = triangles[polygon];
				EllipseGeometry geometry = new EllipseGeometry();
				geometry.Center = new Point(triad.CircumcircleX, triad.CircumcircleY);
				geometry.RadiusX = geometry.RadiusY = Math.Sqrt(triad.CircumcircleR2);
				geometry.Freeze();
				Path path = new Path();
				Panel.SetZIndex(path, CircleZIndex);
				path.StrokeThickness = CircleStrokeTickness;
				path.Stroke = new SolidColorBrush(circleStrokeColor);
				path.Data = geometry;
				circles[polygon] = path;
			}
			return circles[polygon];
		}

		public GabrielGraph BuildGabrielGraph()
		{
			if (gabrielGraph == null)
			{
				BuildGabrielGraph1();
				gabrielGraph = new GabrielGraph()
				{
					Vertices = new List<Vertex>(this.gabrielVertices),
					Edges = new List<DelaunayEdge>(this.gabrielEdges)
				};
			}
			return gabrielGraph;
		}

		public IEnumerable<DelaunayEdge> GetDelaunayEdges()
		{
			if (this.delaunayEdges == null)
			{
				this.delaunayEdges = new HashSet<DelaunayEdge>();
				int i = 0;
				foreach (var triad in this.triads)
				{
					this.delaunayEdges.Add(new DelaunayEdge(this.triads, this.points, triad.A, triad.B, i, triad.Ab));
					this.delaunayEdges.Add(new DelaunayEdge(this.triads, this.points, triad.A, triad.C, i, triad.Ac));
					this.delaunayEdges.Add(new DelaunayEdge(this.triads, this.points, triad.B, triad.C, i, triad.Bc));
					i++;
				}
			}
			return this.delaunayEdges;
		}

		private void BuildGabrielGraph1()
		{
			if (this.gabrielVertices == null && this.gabrielEdges == null)
			{
				this.gabrielVertices = new HashSet<Vertex>();
				this.gabrielEdges = new HashSet<DelaunayEdge>();

				foreach (var edge in this.GetDelaunayEdges())
				{
					if (edge.Neighbour2 != null)
					{
						if (LinesIntersect(edge.Start, edge.End, new Vertex(edge.Neighbour1.CircumcircleX, edge.Neighbour1.CircumcircleY),
							new Vertex(edge.Neighbour2.CircumcircleX, edge.Neighbour2.CircumcircleY)))
						{
							this.gabrielVertices.Add(edge.Start);
							this.gabrielVertices.Add(edge.End);
							this.gabrielEdges.Add(edge);
						}
					}
				}
			}
		}

		private bool LinesIntersect(Vertex l1P1, Vertex l1P2, Vertex l2P1, Vertex l2P2)
		{
			float q = (l1P1.Y - l2P1.Y) * (l2P2.X - l2P1.X) - (l1P1.X - l2P1.X) * (l2P2.Y - l2P1.Y);
			float d = (l1P2.X - l1P1.X) * (l2P2.Y - l2P1.Y) - (l1P2.Y - l1P1.Y) * (l2P2.X - l2P1.X);

			if (d == 0)
			{
				return false;
			}

			float r = q / d;

			q = (l1P1.Y - l2P1.Y) * (l1P2.X - l1P1.X) - (l1P1.X - l2P1.X) * (l1P2.Y - l1P1.Y);
			float s = q / d;

			if (r < 0 || r > 1 || s < 0 || s > 1)
			{
				return false;
			}

			return true;
		}

		private Polygon CreateTriangle(Triad triangle)
		{
			Polygon polygon = new Polygon();
			Vertex a = points[triangle.A];
			Vertex b = points[triangle.B];
			Vertex c = points[triangle.C];
			polygon.Points = new PointCollection(new Point[] { new Point(a.X, a.Y), new Point(b.X, b.Y), new Point(c.X, c.Y) });
			polygon.StrokeThickness = TriangleTickness;
			polygon.Stroke = new SolidColorBrush(triangleStrokeColor);
			Panel.SetZIndex(polygon, TriangleZIndex);
			return polygon;
		}

		public Path CreateEdgeLine(DelaunayEdge edge)
		{
			LineGeometry geometry = new LineGeometry(new Point(edge.Start.X, edge.Start.Y), new Point(edge.End.X, edge.End.Y));
			geometry.Freeze();
			Path path = new Path();
			path.Data = geometry;
			path.StrokeThickness = GabrielEdgeTickness;
			path.Stroke = new SolidColorBrush(gabrielEdgeColor);
			return path;
		}

		public Path CreateLineForMinSpanningTreeEdge(DelaunayEdge edge)
		{
			LineGeometry geometry = new LineGeometry(new Point(edge.Start.X, edge.Start.Y), new Point(edge.End.X, edge.End.Y));
			geometry.Freeze();
			Path path = new Path();
			path.Data = geometry;
			path.StrokeThickness = MinimumSpanningTreeTickness;
			path.Stroke = new SolidColorBrush(Color.FromArgb(120, minimumSpanningTreeEdgeColor.A, minimumSpanningTreeEdgeColor.G, minimumSpanningTreeEdgeColor.B));
			return path;
		}
	}
}