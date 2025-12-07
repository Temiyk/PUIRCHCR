using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Quizes2.Controls
{
    public partial class Cube3DControl : UserControl
    {
        public string Text1 { get; set; }
        public string ScoreText { get; set; }
        public string ResultText { get; set; }
        public ImageSource ImageSide { get; set; }

        public Cube3DControl()
        {
            

            InitializeComponent();
        }

        // ---------- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ----------

        private Material CreateTextBrush(string text)
        {
            const int size = 256;      // Размер текстуры (размер грани)
            int fontSize = 42;         // Начальный шрифт (потом уменьшим, если нужно)

            TextBlock tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                Background = Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Контейнер


            Grid grid = new Grid
            {
                Width = size,
                Height = size,
                Background = Brushes.Transparent  // фон грани ПРОЗРАЧНЫЙ
            };

            Border background = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(180, 220, 255)), // голубой фон только под текст
                CornerRadius = new CornerRadius(0),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            

            background.Child = tb;
            grid.Children.Add(background);



            // Автомасштабирование текста
            Size available = new Size(size - 10, size - 10); // отступы по краям
            tb.Margin = new Thickness(10);

            while (fontSize > 12)
            {
                tb.FontSize = fontSize;

                // Измеряем размер текста
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size desired = tb.DesiredSize;

                if (desired.Width <= available.Width && desired.Height <= available.Height)
                    break; // Помещается – отлично!

                fontSize -= 1; // Иначе уменьшаем шрифт
            }

            return new DiffuseMaterial(new VisualBrush(grid));
        }


        private Material CreateImageBrush(ImageSource img)
        {
            //Background = Brushes.SkyBlue;
            return new DiffuseMaterial(new ImageBrush(img));
        }

        private Material CreateBlueMaterial()
        {
            return new DiffuseMaterial(
                new SolidColorBrush(Color.FromRgb(180, 220, 255)) // приятный голубой
            );
        }

        private GeometryModel3D CreateFace(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection { p0, p1, p2, p3 },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 },
                TextureCoordinates = new PointCollection
                {
                    new Point(0,1),
                    new Point(1,1),
                    new Point(1,0),
                    new Point(0,0)
                }
            };

            return new GeometryModel3D(mesh, material);
        }

        // ---------- СОЗДАНИЕ КУБА ----------

        public void CreateCube()
        {
            CubeModel.Children.Clear();

            double s = 1;

            // FRONT  (Text1)
            CubeModel.Children.Add(CreateFace(
                new Point3D(-s, -s, s), new Point3D(s, -s, s),
                new Point3D(s, s, s), new Point3D(-s, s, s),
                CreateTextBrush(Text1)));

            // RIGHT  (ScoreText)
            CubeModel.Children.Add(CreateFace(
                new Point3D(s, -s, s), new Point3D(s, -s, -s),
                new Point3D(s, s, -s), new Point3D(s, s, s),
                CreateTextBrush(ScoreText)));

            // BACK (ResultText)
            CubeModel.Children.Add(CreateFace(
                new Point3D(s, -s, -s),     // p0
                new Point3D(-s, -s, -s),    // p1
                new Point3D(-s, s, -s),     // p2
                new Point3D(s, s, -s),      // p3
                CreateTextBrush(ResultText)));

            // LEFT  (Image)
            if (ImageSide != null)
                CubeModel.Children.Add(CreateFace(
                    new Point3D(-s, -s, -s), new Point3D(-s, -s, s),
                    new Point3D(-s, s, s), new Point3D(-s, s, -s),
                    CreateImageBrush(ImageSide)));

            CubeModel.Children.Add(CreateFace(
                new Point3D(-s, -s, s),   // p0
                new Point3D(s, -s, s),   // p1
                new Point3D(s, -s, -s),   // p2
                new Point3D(-s, -s, -s),   // p3
                CreateBlueMaterial()));

            CubeModel.Children.Add(CreateFace(
                new Point3D(-s, s, s),   // p0
                new Point3D(s, s, s),   // p1
                new Point3D(s, s, -s),   // p2
                new Point3D(-s, s, -s),   // p3
                CreateBlueMaterial()));

        }

        // ---------- ВРАЩЕНИЕ ----------

        bool dragging = false;
        Point last;

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
            last = e.GetPosition(this);
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;

            var p = e.GetPosition(this);
            double dx = p.X - last.X;
            double dy = p.Y - last.Y;
            

            RotY.Angle += dx * 0.5;
            RotY.Angle += dy * 0.5;
            

            last = p;
        }

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double z = Cam.Position.Z - e.Delta * 0.001;
            if (z < 1.7) z = 1.7;
            if (z > 20) z = 20;

            Cam.Position = new Point3D(0, 0.5, z);
        }
    }
}
