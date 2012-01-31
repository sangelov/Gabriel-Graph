using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DelaunayTriangulator;
using S_hull;
using System.Windows.Controls.Primitives;

namespace Gabriel_Graph
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int triangleTickness = 1;
		private const int triangleZIndex = 2;
		private static readonly Color triangleStrokeColor = Colors.Black;

		private const int circleZIndex = 1;
		private const int circleStrokeTickness = 3;
		private static readonly Color circleStrokeColor = Colors.Black;

		private const float gabrielVertexRadius = 2;
		private const int gabrielVertexTickness = 3;
		private static readonly Color gabrielVertexColor = Colors.Red;

		private List<Vertex> points;
		private List<Triad> triads;
		private HashSet<Vertex> gabrielVertices;

		private Dictionary<Polygon, Triad> triangles;
		private Dictionary<Polygon, Path> circles;

		public MainWindow()
		{
			InitializeComponent();
			GenerateDelaunayTriangulation();
		}

		private void GenerateDelaunayTriangulation()
		{
			//get random points
			this.points = Input.Points;
			Triangulator angulator = new Triangulator();
			this.triads = angulator.Triangulation(points, true);
			DrawDelaunayTriangulation(points, triads);
		}

		private void DrawDelaunayTriangulation(List<Vertex> points, List<Triad> triangles)
		{
			this.Plane.Children.Clear();
			this.triangles = new Dictionary<Polygon, Triad>();
			this.circles = new Dictionary<Polygon, Path>();

			foreach (var triangle in triangles)
			{
				Polygon polygon = CreateTriangle(points, triangle);
				this.triangles[polygon] = triangle;
				Plane.Children.Add(polygon);
			}
		}

		private void DrawGabrielGraphVertices()
		{
			foreach (var vertex in gabrielVertices)
			{
				EllipseGeometry geometry = new EllipseGeometry();
				geometry.Center = new Point(vertex.X, vertex.Y);
				geometry.RadiusX = geometry.RadiusY = gabrielVertexRadius;
				geometry.Freeze();
				Path path = new Path();
				path.StrokeThickness = gabrielVertexTickness;
				path.Stroke = new SolidColorBrush(gabrielVertexColor);
				path.Data = geometry;
				this.Plane.Children.Add(path);
			}
		}

		private void GetGabrielVertices()
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

		private Polygon CreateTriangle(List<Vertex> points, Triad triangle)
		{
			Polygon polygon = new Polygon();
			Vertex a = points[triangle.A];
			Vertex b = points[triangle.B];
			Vertex c = points[triangle.C];
			polygon.Points = new PointCollection(new Point[] { new Point(a.X, a.Y), new Point(b.X, b.Y), new Point(c.X, c.Y) });
			polygon.StrokeThickness = triangleTickness;
			polygon.Stroke = new SolidColorBrush(triangleStrokeColor);
			Panel.SetZIndex(polygon, triangleZIndex);
			return polygon;
		}

		private Path CreateCircle(Polygon polygon, Triad triad)
		{
			EllipseGeometry geometry = new EllipseGeometry();
			geometry.Center = new Point(triad.CircumcircleX, triad.CircumcircleY);
			geometry.RadiusX = geometry.RadiusY = Math.Sqrt(triad.CircumcircleR2);
			geometry.Freeze();
			Path path = new Path();
			Panel.SetZIndex(path, circleZIndex);
			path.StrokeThickness = circleStrokeTickness;
			path.Stroke = new SolidColorBrush(circleStrokeColor);
			path.Data = geometry;
			return path;
		}

		private void FillTriangles()
		{
			Random rand = new Random();
			foreach (var polygon in triangles.Keys)
			{
				polygon.Fill = new SolidColorBrush(Color.FromArgb(125, (byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255)));
				polygon.MouseEnter += OnHover;
				polygon.MouseLeave += OnLeave;
			}
		}

		private void ClearTriangles()
		{
			foreach (var polygon in triangles.Keys)
			{
				polygon.Fill = new SolidColorBrush(Colors.White);
				polygon.MouseEnter -= OnHover;
				polygon.MouseLeave -= OnLeave;
			}
		}

		private void OnLeave(object sender, MouseEventArgs e)
		{
			Polygon polygon = sender as Polygon;
			if (polygon != null)
			{
				this.Plane.Children.Remove(circles[polygon]);
			}
		}

		private void OnHover(object sender, MouseEventArgs e)
		{
			Polygon polygon = sender as Polygon;
			if (polygon != null)
			{
				if (circles.ContainsKey(polygon) && !this.Plane.Children.Contains(circles[polygon]))
				{
					this.Plane.Children.Add(circles[polygon]);
				}
				else
				{
					Triad triad = this.triangles[polygon];
					if (triad != null)
					{
						Path path = CreateCircle(polygon, triad);
						this.circles[polygon] = path;
						this.Plane.Children.Add(path);
					}
				}
			}
		}

		private void GenerateClick(object sender, RoutedEventArgs e)
		{
			GenerateDelaunayTriangulation();
			if (ColorTrianglesButton.IsChecked.HasValue && (bool)ColorTrianglesButton.IsChecked.Value)
			{
				FillTriangles();
			}
		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton btn = sender as ToggleButton;
			if (btn != null)
			{
				if (btn.IsChecked.HasValue && (bool)btn.IsChecked.Value)
				{
					FillTriangles();
				}
				else
				{
					ClearTriangles();
				}
			}
		}

		private void BuildGabrielGraphClick(object sender, RoutedEventArgs e)
		{
			GetGabrielVertices();
			DrawGabrielGraphVertices();
		}

		private bool LinesIntersect(Vertex l1p1, Vertex l1p2, Vertex l2p1, Vertex l2p2)
		{
			float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

			if (d == 0)
			{
				return false;
			}

			float r = q / d;

			q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
			float s = q / d;

			if (r < 0 || r > 1 || s < 0 || s > 1)
			{
				return false;
			}

			return true;
		}
	}
}
