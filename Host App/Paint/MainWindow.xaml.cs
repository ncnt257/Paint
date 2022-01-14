﻿using System;
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


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private readonly IMouseEvents _hook = Hook.GlobalEvents();
        private bool _isDrawing = false;
        readonly List<IShape> _shapes = new List<IShape>();
        private int? _selectedShapeIndex;
        private IShape CopiedShape;
        IShape _preview;
        string _selectedShapeName = "";

        private readonly Dictionary<string, IShape> _prototypes =
            new Dictionary<string, IShape>();

        //Properties menu
        new List<DoubleCollection> StrokeTypes = new List<DoubleCollection>() { new DoubleCollection() {1,0}, new DoubleCollection() { 6, 1 }, new DoubleCollection() { 1 }, new DoubleCollection() { 6, 1, 1, 1 } };
        public MainWindow()
        {

            InitializeComponent();
        }

        
        private void ReDraw()//xóa và vẽ lại
        {
            DrawCanvas.Children.Clear();
            foreach (var shape in _shapes)
            {
                UIElement element = shape.Draw();
                DrawCanvas.Children.Add(element);
            }
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

            //Set stroke properties
            _preview.Color = (Color)(buttonOutlineGallery.SelectedColor);
            _preview.Thickness = (int)buttonStrokeSize.Value;
            _preview.StrokeType = StrokeTypes[buttonStrokeType.SelectedIndex];
            //_preview.Fill = (Color)(buttonOutlineGallery.SelectedColor);
        }

        private void Hook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var temp = e.Location;
            Point screenPos = new Point(temp.X, temp.Y);
            Point pos = DrawCanvas.PointFromScreen(screenPos);
            
            
            
            CoordinateLabel.Content = $"{Math.Ceiling(pos.X)}, {Math.Ceiling(pos.Y)}px";
            if (pos.X < 0 || pos.X > DrawCanvas.ActualWidth || pos.Y < 0 || pos.Y >= DrawCanvas.ActualHeight)
            {
                CoordinateLabel.Content = "";
            }

            if (_isDrawing)
            {

                _preview.HandleEnd(pos.X, pos.Y);
                // Xoá hết các hình vẽ cũ
                ReDraw();

                // Vẽ hình preview đè lên
                DrawCanvas.Children.Add(_preview.Draw());


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
                _shapes.Add(_preview);

                // Sinh ra đối tượng mẫu kế
                _preview = _prototypes[_selectedShapeName].Clone();

                ReDraw();
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
        //        ReDraw();

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
        //    ReDraw();

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

        private void SelectShape(object sender,
            MouseButtonEventArgs e)
        {
            if (_selectedShapeIndex != null) _shapes[_selectedShapeIndex.Value].IsSelected = false;
            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].IsSelected) 
                {
                    _selectedShapeIndex = i;
                    PaintMainWindow.Title = _selectedShapeIndex.ToString();
                    return;
                }
                
            }

            _selectedShapeIndex = null;
            PaintMainWindow.Title = _selectedShapeIndex.ToString();

        }

        private void SelectButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _isDrawing = false;
            DrawCanvas.MouseDown -= Canvas_MouseDown;
            DrawCanvas.MouseLeftButtonDown += SelectShape;
            DrawCanvas.Cursor= Cursors.Arrow;
        }

        private void SelectButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            DrawCanvas.MouseLeftButtonDown -= SelectShape;
            DrawCanvas.MouseDown += Canvas_MouseDown;
            DrawCanvas.Cursor = Cursors.Cross;
        }

        private void CopyButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedShapeIndex != null)
            {
                CopiedShape = _shapes[_selectedShapeIndex.Value].Clone();

            }
        }

        private void PasteSplitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (CopiedShape != null)
            {
                var cs = CopiedShape.Clone();
                cs.Start.X += 10;
                cs.Start.Y += 10;
                cs.End.X += 10;
                cs.End.Y += 10;
                _shapes.Add(cs);
                CopiedShape = cs;
                ReDraw();
            }
        }
    }
}
