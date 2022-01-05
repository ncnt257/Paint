using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Contract;
using Fluent;
using Button = Fluent.Button;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isDrawing = false;
        List<IShape> _shapes = new List<IShape>();
        IShape _preview;
        string _selectedShapeName = "";
        Dictionary<string, IShape> _prototypes =
            new Dictionary<string, IShape>();

        private void Canvas_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            _isDrawing = true;

            Point pos = e.GetPosition(Canvas);

            _preview.HandleStart(pos.X, pos.Y);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point pos = e.GetPosition(Canvas);
                _preview.HandleEnd(pos.X, pos.Y);

                // Xoá hết các hình vẽ cũ
                Canvas.Children.Clear();

                // Vẽ lại các hình trước đó
                foreach (var shape in _shapes)
                {
                    UIElement element = shape.Draw(1, "Red");//Draw(thickness, color) để làm improve, color hiện chưa cần xài tới
                    Canvas.Children.Add(element);
                }

                // Vẽ hình preview đè lên
                Canvas.Children.Add(_preview.Draw(1, "Red"));

                Title = $"{pos.X} {pos.Y}";
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;

            // Thêm đối tượng cuối cùng vào mảng quản lí
            Point pos = e.GetPosition(Canvas);
            _preview.HandleEnd(pos.X, pos.Y);
            _shapes.Add(_preview);

            // Sinh ra đối tượng mẫu kế
            _preview = _prototypes[_selectedShapeName].Clone();

            // Ve lai Xoa toan bo
            Canvas.Children.Clear();

            // Ve lai tat ca cac hinh
            foreach (var shape in _shapes)
            {
                var element = shape.Draw(1, "Red");
                Canvas.Children.Add(element);
            }

        }

        private void prototypeButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedShapeName = (sender as Button).Tag as string;

            _preview = _prototypes[_selectedShapeName];
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = new DirectoryInfo(exeFolder).GetFiles("*.dll");

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IShape).IsAssignableFrom(type))
                        {
                            var shape = Activator.CreateInstance(type) as IShape;
                            _prototypes.Add(shape.Name, shape);
                        }
                    }
                }
            }

            // Tạo ra các nút bấm hàng mẫu
            /*foreach (var item in _prototypes)
            {
                var shape = item.Value as IShape;
                var button = new Button()
                {
                    Header = shape.Name,
                    Width = 80,
                    Height = 35,
                    Margin = new Thickness(5, 0, 5, 0),
                    Tag = shape.Name
                };
                button.Click += prototypeButton_Click;
                ShapeArea.Items.Add(button);
            }

            _selectedShapeName = _prototypes.First().Value.Name;
            _preview = _prototypes[_selectedShapeName].Clone();*/
        }
    }
}
