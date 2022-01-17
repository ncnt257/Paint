﻿using Contract;
using Fluent;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Path = System.IO.Path;
using Point = System.Windows.Point;


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public static string FilePath = "";
        public static string FileName = Path.GetFileName(FilePath);
        private readonly IMouseEvents _hook = Hook.GlobalEvents();
        private bool _isDrawing = false;
        readonly List<IShape> _shapes = new List<IShape>();
        private int? _selectedShapeIndex;
        private Point _moveDelta;
        private int? _cutSelectedShapeIndex;
        private IShape _copiedShape;
        IShape _preview;
        string _seletedPrototypeName = "";

        private readonly Dictionary<string, IShape> _prototypes =
            new Dictionary<string, IShape>();

        //Properties menu
        new List<DoubleCollection> StrokeTypes = new List<DoubleCollection>() { new DoubleCollection() { 1, 0 }, new DoubleCollection() { 6, 1 }, new DoubleCollection() { 1 }, new DoubleCollection() { 6, 1, 1, 1 } };
        public MainWindow()
        {

            InitializeComponent();
        }


        private void ReDraw()//xóa và vẽ lại
        {
            DrawCanvas.Children.Clear();
            foreach (var shape in _shapes)
            {
                UIElement element = shape.Draw(SelectButton.IsChecked ?? false);
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
                DrawCanvas.Children.Add(_preview.Draw(SelectButton.IsChecked ?? false));


            }
        }
        private void Hook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;

                var temp = e.Location;
                Point screenPos = new Point(temp.X, temp.Y);
                Point pos = DrawCanvas.PointFromScreen(screenPos);
                // Thêm đối tượng cuối cùng vào mảng quản lí

                _preview.HandleEnd(pos.X, pos.Y);
                _shapes.Add(_preview);

                // Sinh ra đối tượng mẫu kế
                _preview = _prototypes[_seletedPrototypeName].Clone();

                ReDraw();
            }

        }


        private void prototypeButton_Click(object sender, RoutedEventArgs e)
        {
            _seletedPrototypeName = (sender as Fluent.ToggleButton).Tag as string;

            _preview = _prototypes[_seletedPrototypeName].Clone();

            SelectButton.IsChecked = false;
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
                _seletedPrototypeName = _prototypes.First().Value.Name;
                _preview = _prototypes[_seletedPrototypeName].Clone();
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
            if (FilePath == "")
            {
                buttonSaveAs_Click(sender, e);
                return;
            }
            string ext = Path.GetExtension(FilePath);
            Debug.Write(ext);
            DrawCanvas.UpdateLayout();
            CreateBitmapFromVisual(DrawCanvas, FilePath, ext);
        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "png";
            saveFileDialog.Filter = "PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPG Files (*.jpg)|*.jpg";
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        {
                            CreateBitmapFromVisual(DrawCanvas, saveFileDialog.FileName, ".png");
                            break;
                        }
                    case 2:
                        {
                            CreateBitmapFromVisual(DrawCanvas, saveFileDialog.FileName, ".bmp");
                            break;
                        }
                    case 3:
                        {
                            CreateBitmapFromVisual(DrawCanvas, saveFileDialog.FileName, ".jpg");
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browseDialog = new OpenFileDialog();
            browseDialog.Filter = "PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPG Files (*.jpg)|*.jpg";
            browseDialog.FilterIndex = 1;
            browseDialog.Multiselect = false;
            if (browseDialog.ShowDialog() != true)
            {
                return;
            }
            FilePath = browseDialog.FileName;

            //need to fix right here
            // ImageBrush ib = new ImageBrush();
            // BitmapImage inputFile = new BitmapImage(new Uri(FilePath, UriKind.RelativeOrAbsolute));
            // ib.ImageSource = inputFile;
            // DrawCanvas.Background = ib;

            MemoryStream ms = new MemoryStream();
            BitmapImage bi = new BitmapImage();
            byte[] bytArray = File.ReadAllBytes(FilePath);
            ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            ImageBrush ib = new ImageBrush();
            ib.ImageSource = bi;
            DrawCanvas.Background = ib;
        }

        void CreateBitmapFromVisual(Visual target, string filename, string filerType)
        {
            if (target == null)
                return;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);
            switch (filerType)
            {
                case ".png":
                    PngBitmapEncoder png = new PngBitmapEncoder();

                    png.Frames.Add(BitmapFrame.Create(rtb));
                    using (Stream stm = File.Create(filename))
                    {
                        png.Save(stm);
                    }
                    break;
                case ".bmp":
                    BitmapEncoder bmp = new BmpBitmapEncoder();
                    bmp.Frames.Add(BitmapFrame.Create(rtb));
                    using (Stream stm = File.OpenWrite(filename))
                    {
                        bmp.Save(stm);
                    }

                    break;
                case ".jpg":
                    JpegBitmapEncoder jpg = new JpegBitmapEncoder();
                    jpg.Frames.Add(BitmapFrame.Create(rtb));
                    using (Stream stm = File.OpenWrite(filename))
                    {
                        jpg.Save(stm);
                    }
                    break;
                default: break;
            }

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
        //    _preview = _prototypes[_seletedPrototypeName].Clone();

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



        private void SelectButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _isDrawing = false;
            DrawCanvas.MouseDown -= Canvas_MouseDown;
            DrawCanvas.MouseLeftButtonDown += SelectShape;
            DrawCanvas.Cursor = Cursors.Arrow;
            ReDraw();
        }

        private void SelectButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_selectedShapeIndex != null)
            {
                _shapes[_selectedShapeIndex.Value].IsSelected = false;
                _selectedShapeIndex = null;
            }
            
            

            DrawCanvas.MouseLeftButtonDown -= SelectShape;
            DrawCanvas.MouseDown += Canvas_MouseDown;
            DrawCanvas.Cursor = Cursors.Cross;
            ReDraw();
        }

        private void CopyButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedShapeIndex != null)
            {
                _copiedShape = _shapes[_selectedShapeIndex.Value].Clone();

            }
        }

        private void PasteSplitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_copiedShape != null)
            {
                var cs = _copiedShape.Clone();
                cs.Start.X += 10;
                cs.Start.Y += 10;
                cs.End.X += 10;
                cs.End.Y += 10;
                cs.IsSelected = true;
                _shapes.Add(cs);
                _shapes[_selectedShapeIndex.Value].IsSelected = false;
                _copiedShape = cs;
                if (_cutSelectedShapeIndex is not null)
                {
                    _shapes.RemoveAt(_cutSelectedShapeIndex.Value);
                    _cutSelectedShapeIndex = null;
                }
                _selectedShapeIndex = _shapes.Count - 1;
                
                ReDraw();
            }
        }

        private void CutButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedShapeIndex != null)
            {
                _copiedShape = _shapes[_selectedShapeIndex.Value].Clone();
                _cutSelectedShapeIndex = _selectedShapeIndex;
            }
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
                    _moveDelta.X = _shapes[_selectedShapeIndex.Value].Start.X - e.GetPosition(DrawCanvas).X;
                    _moveDelta.Y = _shapes[_selectedShapeIndex.Value].Start.Y - e.GetPosition(DrawCanvas).Y;

                    
                    ReDraw();
                    return;
                }

            }
            _selectedShapeIndex = null;
            ReDraw();
            

        }
        private void DrawCanvas_OnDragOver(object sender, DragEventArgs e)
        {
            Point dropPos = e.GetPosition(DrawCanvas);
            double deltaX = _shapes[_selectedShapeIndex.Value].Start.X - dropPos.X;
            double deltaY = _shapes[_selectedShapeIndex.Value].Start.Y - dropPos.Y;
            _shapes[_selectedShapeIndex.Value].Start.X -= deltaX - _moveDelta.X;
            _shapes[_selectedShapeIndex.Value].End.X -= deltaX - _moveDelta.X;
            _shapes[_selectedShapeIndex.Value].Start.Y -= deltaY - _moveDelta.Y;
            _shapes[_selectedShapeIndex.Value].End.Y -= deltaY - _moveDelta.Y;
            ReDraw();
        }
    }
}
