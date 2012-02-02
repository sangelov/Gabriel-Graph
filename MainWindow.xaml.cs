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
				Path path = graph.CreateVertexPoint(vertex);
				this.Plane.Children.Add(path);
			}
			foreach (var edge in graph.Edges)
			{
				Path path = graph.CreateEdgeLine(edge);
				this.Plane.Children.Add(path);
			}
		}

		private void ClearGabrielGraph()
		{
			GabrielGraph graph = delaunay.BuildGabrielGraph();
			foreach (var vertex in graph.Vertices)
			{
				Path path = graph.CreateVertexPoint(vertex);
				this.Plane.Children.Remove(path);
			}
			foreach (var edge in graph.Edges)
			{
				Path path = graph.CreateEdgeLine(edge);
				this.Plane.Children.Remove(path);
			}
		}

		private void DrawGabrielMinimumSpanningTree()
		{
			GabrielGraph graph = delaunay.BuildGabrielGraph();
			var minimumSpanningTree = graph.GetMinimumSpanningTree();
			foreach (var edge in minimumSpanningTree.Edges)
			{
				var path = graph.CreateLineForMinSpanningTreeEdge(edge);
				this.Plane.Children.Add(path);
			}
		}

		private void ClearGabrielMinimumSpanningTree()
		{
			GabrielGraph graph = delaunay.BuildGabrielGraph();
			var minimumSpanningTree = graph.GetMinimumSpanningTree();
			foreach (var edge in minimumSpanningTree.Edges)
			{
				var path = graph.CreateLineForMinSpanningTreeEdge(edge);
				this.Plane.Children.Remove(path);
			}
		}

		private void DrawMinimumSpanningTree()
		{
			var minimumSpanningTree = delaunay.GetMinimumSpanningTree();
			foreach (var edge in minimumSpanningTree.Edges)
			{
				var path = delaunay.CreateLineForMinSpanningTreeEdge(edge);
				this.Plane.Children.Add(path);
			}
		}

		private void ClearMinimumSpanningTree()
		{
			var minimumSpanningTree = delaunay.GetMinimumSpanningTree();
			foreach (var edge in minimumSpanningTree.Edges)
			{
				var path = delaunay.CreateLineForMinSpanningTreeEdge(edge);
				this.Plane.Children.Remove(path);
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
			ToggleButton button = sender as ToggleButton;
			if (button != null)
			{
				if (button.IsChecked.HasValue && button.IsChecked.Value)
				{
					DrawGabrielGraph();
				}
				else
				{
					ClearGabrielGraph();
				}
			}
		}

		private void BuildGabrielMinimumSpanningTreeClick(object sender, RoutedEventArgs e)
		{
			ToggleButton button = sender as ToggleButton;
			if (button != null)
			{
				if (button.IsChecked.HasValue && button.IsChecked.Value)
				{
					DrawGabrielMinimumSpanningTree();
				}
				else
				{
					ClearGabrielMinimumSpanningTree();
				}
			}
		}

		private void BuildMinimumSpanningTreeClick(object sender, RoutedEventArgs e)
		{
			ToggleButton button = sender as ToggleButton;
			if (button != null)
			{
				if (button.IsChecked.HasValue && button.IsChecked.Value)
				{
					DrawMinimumSpanningTree();
				}
				else
				{
					ClearMinimumSpanningTree();
				}
			}
		}
	}
}