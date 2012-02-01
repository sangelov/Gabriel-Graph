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

		private List<Vertex> points;
		private List<Triad> triads;

		private Dictionary<Triad, Polygon> triadsDict;
		private Dictionary<Polygon, Triad> triangles;
		private Dictionary<Polygon, Path> circles;

		private HashSet<Vertex> gabrielVertices;

		private const double GabrielVertexRadius = 3;
		private const double GabrielVertexTickness = 2;
		private static readonly Color gabrielVertexColor = Colors.Red;

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

		public IEnumerable<Vertex> GetGabrielVertices()
		{
			if (this.gabrielVertices == null)
			{
				this.gabrielVertices = new HashSet<Vertex>();
				foreach (var triad in this.triads)
				{
					Triad neigbour;
					if (triad.Ab > -1 && triad.Ab < triads.Count)
					{
						neigbour = this.triads[triad.Ab];
						if (LinesIntersect(new Vertex(triad.CircumcircleX, triad.CircumcircleY),
							new Vertex(neigbour.CircumcircleX, neigbour.CircumcircleY),
							this.points[triad.A], this.points[triad.B]))
						{
							this.gabrielVertices.Add(this.points[triad.A]);
							this.gabrielVertices.Add(this.points[triad.B]);
						}
					}
					if (triad.Ac > -1 && triad.Ac < triads.Count)
					{
						neigbour = this.triads[triad.Ac];
						if (LinesIntersect(new Vertex(triad.CircumcircleX, triad.CircumcircleY),
							new Vertex(neigbour.CircumcircleX, neigbour.CircumcircleY),
							this.points[triad.A], this.points[triad.C]))
						{
							{
								this.gabrielVertices.Add(this.points[triad.A]);
								this.gabrielVertices.Add(this.points[triad.C]);
							}
						}
					}
					if (triad.Bc > -1 && triad.Bc < triads.Count)
					{
						neigbour = this.triads[triad.Bc];
						if (LinesIntersect(new Vertex(triad.CircumcircleX, triad.CircumcircleY),
							new Vertex(neigbour.CircumcircleX, neigbour.CircumcircleY),
							this.points[triad.B], this.points[triad.C]))
						{
							this.gabrielVertices.Add(this.points[triad.B]);
							this.gabrielVertices.Add(this.points[triad.C]);
						}
					}
				}
			}
			foreach (var vertex in this.gabrielVertices)
			{
				yield return vertex;
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
	}
}