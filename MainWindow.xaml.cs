using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Gabriel_Graph
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DelaunayTriangulation delaunay;

		public MainWindow()
		{
			InitializeComponent();
			GenerateDelaunayTriangulation();
		}

		private void GenerateDelaunayTriangulation()
		{
			delaunay = DelaunayTriangulation.Create();	
			DrawDelaunayTriangulation();
		}

		private void DrawDelaunayTriangulation()
		{
			this.Plane.Children.Clear();
			
			foreach (var polygon in this.delaunay.GetDelaunayTriangulationPolygons())
			{
				Plane.Children.Add(polygon);
			}
		}

		private void DrawGabrielGraph()
		{
			GabrielGraph graph = delaunay.BuildGabrielGraph();
			foreach (var vertex in graph.Vertices)
			{
				Path path = delaunay.CreateGabrielVertexPoint(vertex);
				this.Plane.Children.Add(path);
			}
			foreach (var edge in graph.Edges)
			{
				Path path = delaunay.CreateEdgeLine(edge);
				this.Plane.Children.Add(path);
			}
		}

		private void FillTriangles()
		{
			Random rand = new Random();
			foreach (var polygon in delaunay.GetDelaunayTriangulationPolygons())
			{
				polygon.Fill = new SolidColorBrush(Color.FromArgb(125, (byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255)));
				polygon.MouseEnter += OnHover;
				polygon.MouseLeave += OnLeave;
			}
		}

		private void ClearTriangles()
		{
			foreach (var polygon in this.delaunay.GetDelaunayTriangulationPolygons())
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
				Path path = delaunay.GetCircumCircle(polygon);
				this.Plane.Children.Remove(path);
			}
		}

		private void OnHover(object sender, MouseEventArgs e)
		{
			Polygon polygon = sender as Polygon;
			if (polygon != null)
			{
				Path path = delaunay.GetCircumCircle(polygon);
				this.Plane.Children.Add(path);
			}
		}

		private void GenerateClick(object sender, RoutedEventArgs e)
		{
			GenerateDelaunayTriangulation();
			if (ColorTrianglesButton.IsChecked.HasValue && ColorTrianglesButton.IsChecked.Value)
			{
				FillTriangles();
			}
		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton btn = sender as ToggleButton;
			if (btn != null)
			{
				if (btn.IsChecked.HasValue && btn.IsChecked.Value)
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
			DrawGabrielGraph();
		}
	}
}