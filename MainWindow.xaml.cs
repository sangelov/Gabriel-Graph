using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using DelaunayTriangulator;
using S_hull;

namespace Gabriel_Graph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<Vertex> points = Input.Points;
            Triangulator angulator = new Triangulator();
            List<Triad> triangles = angulator.Triangulation(points, true);
            DrawDelaunayTriangulation(points, triangles);
        }

        private void DrawDelaunayTriangulation(List<Vertex> points, List<Triad> triangles)
        {
            foreach (var triangle in triangles)
            {
                Polygon p = new Polygon();
                Vertex a = points[triangle.A];
                Vertex b = points[triangle.B];
                Vertex c = points[triangle.C];
                p.Points = new PointCollection(new Point[] { new Point(a.X, a.Y), new Point(b.X, b.Y), new Point(c.X, c.Y) });
                p.StrokeThickness = 1;
                p.Stroke = new SolidColorBrush(Colors.Black);
                area.Children.Add(p);
            }
        }
    }
}
