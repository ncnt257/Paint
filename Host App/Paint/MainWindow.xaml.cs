using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Gma.System.MouseKeyHook;
using TextBox = System.Windows.Controls.TextBox;


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : RibbonWindow
    {
        private IMouseEvents _hook = Hook.GlobalEvents();
        bool _isDrawing = false;
        bool _moved = false;
        List<IShape> _shapes = new List<IShape>();
        IShape _preview;
        string _selectedShapeName = "";
        Dictionary<int, string> textBoxContent = new Dictionary<int, string>();
        Dictionary<string, IShape> _prototypes =
            new Dictionary<string, IShape>();

        public MainWindow()
        {
            InitializeComponent();
            
            
            
        }
        private void DrawCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            _hook.MouseMove += Hook_MouseMove;
            _hook.MouseUp += Hook_MouseUp;
        }
        

        private void Canvas_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            _isDrawing = true;

            Point pos = e.GetPosition(DrawCanvas);

            _preview.HandleStart(pos.X, pos.Y); 
        }

        private void Hook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var temp = e.Location;
            Point screenPos = new Point(temp.X, temp.Y);
            Point pos = DrawCanvas.PointFromScreen(screenPos);

            _moved = true;
            
            
            CoordinateLabel.Content = $"{Math.Ceiling(pos.X)}, {Math.Ceiling(pos.Y)}px";
            if (pos.X < 0 || pos.X > DrawCanvas.ActualWidth || pos.Y < 0 || pos.Y >= DrawCanvas.ActualHeight)
            {
                CoordinateLabel.Content = "";
            }

            if (_isDrawing)
            {
                _preview.HandleEnd(pos.X, pos.Y);
                // Xoá hết các hình vẽ cũ
                int count = textBoxContent.Count;

                foreach (var child in DrawCanvas.Children)
                {
                    string t = child.ToString();
                    if (child.ToString().Contains("TextBox"))
                    {
                        count++;
                        var idx = t.IndexOf(":");
                        string content = t.Substring(idx + 1);
                        textBoxContent.Add(count, content);
                    }
                }

                DrawCanvas.Children.Clear();

                count = 1;
                // Vẽ lại các hình trước đó
                foreach (var shape in _shapes)
                {
                    if (shape.Name == "Text")
                    {
                        var textBlock = new TextBlock();

                        textBlock.Text = textBoxContent[count];
                        textBlock.Foreground = new SolidColorBrush(Colors.Black);
                        textBlock.Background = new SolidColorBrush(Colors.Gray);

                        Canvas.SetLeft(textBlock, shape._start.X);
                        Canvas.SetTop(textBlock, shape._start.Y);

                        DrawCanvas.Children.Add(textBlock);
                        count++;
                    }
                    else
                    {
                        var element = shape.Draw(1, "Red");
                        DrawCanvas.Children.Add(element);

                    }

                }
                // Vẽ hình preview đè lên
                DrawCanvas.Children.Add(_preview.Draw(1, "Red"));
            }
        }
        private void Hook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(_isDrawing){
                _isDrawing = false;

                var temp = e.Location;
                Point screenPos = new Point(temp.X, temp.Y);
                Point pos = DrawCanvas.PointFromScreen(screenPos);
                // Thêm đối tượng cuối cùng vào mảng quản lí

                _preview.HandleEnd(pos.X, pos.Y);
                
                if (_shapes.Count > textBoxContent.Count)
                {
                    foreach (UIElement child in DrawCanvas.Children)
                    {
                        if (child.ToString().Contains("TextBox"))
                        {
                            DrawCanvas.Children.Remove(child);
                            //var shape = child as IShape;
                            _shapes.RemoveAt(_shapes.Count-1);
                            break;
                        }
                    }
                }

                // Ve lai Xoa toan bo
                DrawCanvas.Children.Clear();

                // Ve lai tat ca cac hinh
                var count = 1;
                foreach (var shape in _shapes)
                {
                    if (shape.Name == "Text")
                    {
                        var textBlock = new TextBlock();

                        textBlock.Text = textBoxContent[count];
                        textBlock.Foreground = new SolidColorBrush(Colors.Black);
                        textBlock.Background = new SolidColorBrush(Colors.Gray);

                        Canvas.SetLeft(textBlock, shape._start.X);
                        Canvas.SetTop(textBlock, shape._start.Y);

                        DrawCanvas.Children.Add(textBlock);

                        count++;

                    }
                    else
                    {
                        var element = shape.Draw(1, "Red");
                        DrawCanvas.Children.Add(element);
                    }
                }


                var clonePreview = _preview;


                if (clonePreview.Name == "Text")
                {
                    var textBox = new TextBox();

                    textBox.Text = "     ";

                    textBox.Foreground = new SolidColorBrush(Colors.Black);
                    textBox.BorderThickness = new Thickness(0);
                    textBox.BorderBrush = Brushes.White;
                    textBox.Background = new SolidColorBrush(Colors.Gray);

                    Canvas.SetLeft(textBox, clonePreview._start.X);
                    Canvas.SetTop(textBox, clonePreview._start.Y);

                    DrawCanvas.Children.Add(textBox);
                }
                else
                {
                    var element = clonePreview.Draw(1, "Red");
                    DrawCanvas.Children.Add(element);
                }

                _shapes.Add(_preview);
                // Sinh ra đối tượng mẫu kế
                _preview = _prototypes[_selectedShapeName].Clone();
            }

        }


        private void prototypeButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedShapeName = (sender as Fluent.ToggleButton).Tag as string;

            _preview = _prototypes[_selectedShapeName];
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = new DirectoryInfo(exeFolder).GetFiles("*.dll");

            foreach (var dll in dlls)
            {
                if (dll.Name == "ControlzEx.dll") continue;
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
            foreach (var item in _prototypes)
            {
                var shape = item.Value as IShape;
                var button = new Fluent.ToggleButton()
                {
                    Icon = $"pack://application:,,,/{shape.Name}2D;Component/{shape.Name}.png",
                    Header = shape.Name,
                    SizeDefinition = "Small",
                    GroupName = "Shape",
                    Tag = shape.Name,
                    
                };
                button.Click += prototypeButton_Click;
                Shape.Items.Add(button);
            }

            if (_prototypes.Count > 0)
            {
                _selectedShapeName = _prototypes.First().Value.Name;
                _preview = _prototypes[_selectedShapeName].Clone();
            }
        }

        private void TestAddShapeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = new Fluent.ToggleButton();
            button.Icon = "Resource/IMAGE/60340.PNG";
            button.SizeDefinition = "Small";
            button.GroupName = "Shape";
            Shape.Items.Add(button);

        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            
            
        }
        //private void Canvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    Point pos = e.GetPosition(DrawCanvas);
        //    CoordinateLabel.Content = $"{Math.Ceiling(pos.X)}, {Math.Ceiling(pos.Y)}px";
        //    if (_isDrawing)
        //    {

        //        _preview.HandleEnd(pos.X, pos.Y);
        //        // Xoá hết các hình vẽ cũ
        //        DrawCanvas.Children.Clear();

        //        // Vẽ lại các hình trước đó
        //        foreach (var shape in _shapes)
        //        {
        //            UIElement element = shape.Draw(1, "Red");//Draw(thickness, color) để làm improve, color hiện chưa cần xài tới
        //            DrawCanvas.Children.Add(element);
        //        }

        //        // Vẽ hình preview đè lên
        //        DrawCanvas.Children.Add(_preview.Draw(1, "Red"));


        //    }

        //}

        //private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    _isDrawing = false;

        //    // Thêm đối tượng cuối cùng vào mảng quản lí
        //    Point pos = e.GetPosition(DrawCanvas);
        //    _preview.HandleEnd(pos.X, pos.Y);
        //    _shapes.Add(_preview);

        //    // Sinh ra đối tượng mẫu kế
        //    _preview = _prototypes[_selectedShapeName].Clone();

        //    // Ve lai Xoa toan bo
        //    DrawCanvas.Children.Clear();

        //    // Ve lai tat ca cac hinh
        //    foreach (var shape in _shapes)
        //    {
        //        var element = shape.Draw(1, "Red");
        //        DrawCanvas.Children.Add(element);
        //    }

        //}




        //<Thumb Name = "CanvasThumb" Canvas.Right="-5" Canvas.Bottom="-5" Background="Black" 
        //Width="5" Height="5" DragDelta="OnDragDelta" 
        //Cursor="SizeNWSE"
        //Style="{StaticResource ScrollBarThumb}"
        ///>

        //void OnDragDelta(object sender, DragDeltaEventArgs e)
        //{
        //    //Move the Thumb to the mouse position during the drag operation
        //    double yadjust = DrawCanvas.Height + e.VerticalChange;
        //    double xadjust = DrawCanvas.Width + e.HorizontalChange;
        //    if ((xadjust >= 0) && (yadjust >= 0))
        //    {
        //        DrawCanvas.Width = xadjust;
        //        CanvasBorder.Width = xadjust;
        //        DrawCanvas.Height = yadjust;
        //        CanvasBorder.Height = yadjust;

        //        Canvas.SetLeft(CanvasThumb, Canvas.GetLeft(CanvasThumb) +
        //                                    e.HorizontalChange);
        //        Canvas.SetTop(CanvasThumb, Canvas.GetTop(CanvasThumb) +
        //                                   e.VerticalChange);

        //    }
        //}


        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _hook.MouseMove -= Hook_MouseMove;
            _hook.MouseUp -= Hook_MouseUp;
            
        }

        private void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            foreach (var child in DrawCanvas.Children)
            {
                string t = child.ToString();
                if (child.ToString().Contains("TextBox"))
                {
                    var idx = t.IndexOf(":");
                    string content = t.Substring(idx+1);
                }
                count++;
            }
        }
    }
}
