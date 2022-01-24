using Contract;
using Fluent;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, INotifyPropertyChanged
    {
        public static string FilePath = "";
        private readonly IMouseEvents _hook = Hook.GlobalEvents();
        private bool _isDrawing = false;
        List<IShape> _shapes = new List<IShape>();

        private int? _selectedShapeIndex;
        private int? _cutSelectedShapeIndex;
        private IShape _copiedShape;
        IShape _preview;
        string _seletedPrototypeName = "";

        //binding
        Color _fillColor = Colors.Red;
        public string test { get; set; }
        public Color OutlineColor { get; set; }
        public Color FillColor { get; set; }
        public Color FontColor { get; set; }

        //shape
        private List<IShape> currentIShape = new List<IShape>();

        //Layer
        BindingList<Layer> layers = new BindingList<Layer>() { new Layer(0, true) };
        private int _currentLayer = 0;
        private int lowerLayersShapesCount;

        //zooming
        private float currentProportion = 100;
        private int startZooming = 0;

        //shortcut
        private Point mouseDownPoint;
        public StringBuilder shortcutText = new StringBuilder();
        public int shift;


        public static Dictionary<string, IShape> _prototypes =
            new Dictionary<string, IShape>();

        //Properties menu
        List<DoubleCollection> StrokeTypes = new List<DoubleCollection>() { new DoubleCollection() { 1, 0 }, new DoubleCollection() { 6, 1 }, new DoubleCollection() { 1 }, new DoubleCollection() { 6, 1, 1, 1 } };

        public event PropertyChangedEventHandler? PropertyChanged = null;

        public MainWindow()
        {
            InitializeComponent();
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;

        }

        private void GetPoint_MouseUp(object sender,
            MouseEventArgs e)
        {
            mouseDownPoint = e.GetPosition(DrawCanvas);

            return;
        }


        private void PasteShape()
        {
            var cs = _copiedShape.Clone();

            double lengthX = mouseDownPoint.X - cs.Start.X;
            double lengthY = mouseDownPoint.Y - cs.Start.Y;
            if ((mouseDownPoint.X == 0 && mouseDownPoint.Y == 0) || (mouseDownPoint.X == cs.Start.X && mouseDownPoint.Y == cs.Start.Y))
            {
                cs.Start.X += 10;
                cs.Start.Y += 10;
                cs.End.X += 10;
                cs.End.Y += 10;
            }
            else
            {
                cs.Start.X = mouseDownPoint.X;
                cs.Start.Y = mouseDownPoint.Y;
                cs.End.X += lengthX;
                cs.End.Y += lengthY;
            }

            cs.IsSelected = true;
            _shapes.Add(cs);
            if (_selectedShapeIndex is not null)
            {
                _shapes[_selectedShapeIndex.Value].IsSelected = false;
            }
            _copiedShape = cs;
            if (_cutSelectedShapeIndex is not null)
            {
                _shapes.RemoveAt(_cutSelectedShapeIndex.Value);
                _cutSelectedShapeIndex = null;
                _copiedShape = null;
            }
            _selectedShapeIndex = _shapes.Count - 1;
            int i = _shapes.Count - 1;

            ReDraw();

            //paste xong được sửa shape
            if (_shapes[i].Name != "Line")
            {
                AdornerLayer.GetAdornerLayer(DrawCanvas.Children[lowerLayersShapesCount + i])
                    .Add(new ResizeShapeAdorner(DrawCanvas.Children[lowerLayersShapesCount + i], _shapes[i]));
            }
            else
            {
                AdornerLayer.GetAdornerLayer(DrawCanvas.Children[lowerLayersShapesCount + i])
                    .Add(new ResizeLineAdorner(DrawCanvas.Children[lowerLayersShapesCount + i], _shapes[i]));
            }
        }

        private void ReDraw()//xóa và vẽ lại
        {

            DrawCanvas.Children.Clear();

            if (_currentLayer == -1)
                return;

            layers[_currentLayer]._shapes = _shapes;

            //Duyệt xem layer nào được check thì vẽ
            for (int i = 0; i < layers.Count(); i++)
            {


                if (layers[i].isChecked)
                {
                    foreach (var shape in layers[i]._shapes)
                    {
                        UIElement element = shape.Draw(SelectButton.IsChecked ?? false, i == _currentLayer, shift);

                        DrawCanvas.Children.Add(element);

                        //update acutual width và height để dùng adorner 
                        DrawCanvas.UpdateLayout();
                    }

                }


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
            //Kiểm tra chưa có layer nào được chọn
            if (_currentLayer == -1)
            {
                MessageBox.Show("Please choose atleast 1 layer");
                return;
            }
            //Kiểm tra chọn layer nhưng layer đang bị ẩn(icon closed eye)
            else if (!layers[_currentLayer].isChecked)
            {
                MessageBox.Show("Please display selected layer for drawing");
                return;
            }
            _isDrawing = true;

            Point pos = e.GetPosition(DrawCanvas);

            _preview.HandleStart(pos.X, pos.Y);

            //Set stroke properties
            _preview.Color = OutlineColor;
            _preview.Thickness = (int)buttonStrokeSize.Value;
            _preview.StrokeType = StrokeTypes[buttonStrokeType.SelectedIndex];
            _preview.Fill = FillColor;
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
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    shift = 1;
                }
                else
                {
                    shift = 2;
                }

                _preview.HandleEnd(pos.X, pos.Y);
                // Xoá hết các hình vẽ cũ
                ReDraw();

                // Vẽ hình preview đè lên

                //bool shift = shortcutText.ToString().Contains("shift");
                DrawCanvas.Children.Add(_preview.Draw(SelectButton.IsChecked ?? false, true, shift));

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

                double previewSize = Math.Sqrt(Math.Pow((_preview.End.X - _preview.Start.X), 2) +
                                               Math.Pow((_preview.End.Y - _preview.Start.Y), 2));
                if (previewSize < 1)
                {
                    ReDraw();
                    if (_selectedShapeIndex != null)
                    {
                        int index = _selectedShapeIndex.Value;
                        _shapes[_selectedShapeIndex.Value].IsSelected = false;

                        //remove adorner của shape khác
                        Adorner[] toRemoveArray =
                            AdornerLayer.GetAdornerLayer(DrawCanvas).GetAdorners(DrawCanvas.Children[lowerLayersShapesCount + index]);
                        if (toRemoveArray != null)
                        {
                            for (int x = 0; x < toRemoveArray.Length; x++)
                            {
                                AdornerLayer.GetAdornerLayer(DrawCanvas).Remove(toRemoveArray[x]);
                            }
                        }

                    };
                    _selectedShapeIndex = null;
                    return;
                };

                _shapes.Add(_preview);

                // Sinh ra đối tượng mẫu kế
                _preview = _prototypes[_seletedPrototypeName].Clone();

                ReDraw();
                int i = _shapes.Count - 1;
                _selectedShapeIndex = i;
                if (_shapes[i].Name != "Line")
                {
                    AdornerLayer.GetAdornerLayer(DrawCanvas.Children[lowerLayersShapesCount + i])
                        .Add(new ResizeShapeAdorner(DrawCanvas.Children[lowerLayersShapesCount + i], _shapes[i]));
                }
                else
                {
                    AdornerLayer.GetAdornerLayer(DrawCanvas.Children[lowerLayersShapesCount + i])
                        .Add(new ResizeLineAdorner(DrawCanvas.Children[lowerLayersShapesCount + i], _shapes[i]));
                }

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

            OutlineColor = Colors.Black;
            FillColor = Colors.Transparent;
            FontColor = Colors.Black;

            //set datacontext cho binding
            this.DataContext = this;
            ListViewLayers.ItemsSource = layers;


            Line2D.Line2D linePrototype = new();
            _prototypes.Add(linePrototype.Name, linePrototype);
            var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = new DirectoryInfo(exeFolder).GetFiles("*.dll");

            foreach (var dll in dlls)
            {
                if (dll.Name == "ControlzEx.dll") continue;
                if (dll.Name == "Line2D.dll") continue;
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
                ShapeGroupBox.Items.Add(button);
            }

            if (_prototypes.Count > 0)
            {
                (ShapeGroupBox.Items[0] as Fluent.ToggleButton).IsChecked = true;
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
            ShapeGroupBox.Items.Add(button);

        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "png";
            saveFileDialog.Filter = "PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPG Files (*.jpg)|*.jpg|Binary Files (*.bin)|*.bin";
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
                    case 4:
                        {
                            SaveNew();
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void Save()
        {
            if (FilePath == "")
            {
                //buttonSaveAs_Click(sender, e);
                SaveAs();
                return;
            }
            string ext = Path.GetExtension(FilePath);
            DrawCanvas.UpdateLayout();
            if (ext == ".bin")
            {
                SaveNew();
                return;
            }
            CreateBitmapFromVisual(DrawCanvas, FilePath, ext);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browseDialog = new OpenFileDialog();
            browseDialog.Filter = "PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPG Files (*.jpg)|*.jpg|Binary Files (*.bin)|*.bin";
            browseDialog.FilterIndex = 1;
            browseDialog.Multiselect = false;
            if (browseDialog.ShowDialog() != true)
            {
                return;
            }
            FilePath = browseDialog.FileName;
            if (Path.GetExtension(FilePath) == ".bin")
            {
                using (var stream = File.OpenRead(FilePath))
                {
                    using (var br = new BinaryReader(stream))
                    {
                        layers.Clear();
                        var layerData = Paint.Layer.ReadLayerListBinary(br);
                        foreach (var data in layerData)
                        {
                            layers.Add(data);
                        }
                        // layers = new BindingList<Layer>(layerData);
                    }
                }

                //Tính lại current layer và gán _shape = _shape của currentlayer
                _currentLayer = 0;
                ListViewLayers.SelectedIndex = _currentLayer;
                _shapes = layers[_currentLayer]._shapes;
                lowerLayersShapesCount = 0;
                for (int k = 0; k < _currentLayer; k++)
                {
                    if (layers[k].isChecked) lowerLayersShapesCount += layers[k]._shapes.Count;
                }
                ReDraw();

                return;
            }
            MemoryStream ms = new MemoryStream();
            BitmapImage bi = new BitmapImage();
            if (FilePath != null)
            {
                byte[] bytArray = File.ReadAllBytes(FilePath);
                ms.Write(bytArray, 0, bytArray.Length);
            }

            ms.Position = 0;
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




        public void SaveNew()
        {
            // layers[_currentLayer]._shapes = _shapes;
            using (var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))

            using (var bw = new BinaryWriter(stream))
            {
                Paint.Layer.WriteLayerListBinary(bw, layers.ToList());
            }
        }


        /*
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
        */

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _hook.MouseMove -= Hook_MouseMove;
            _hook.MouseUp -= Hook_MouseUp;
        }

        private void SelectButton_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var b in ShapeGroupBox.Items)
            {
                (b as Fluent.ToggleButton).IsChecked = false;
            }
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

        private void CopyShape()
        {
            if (_selectedShapeIndex != null)
            {
                _copiedShape = _shapes[_selectedShapeIndex.Value].Clone();
            }
        }

        private void CutShape()
        {
            if (_selectedShapeIndex != null)
            {
                _copiedShape = _shapes[_selectedShapeIndex.Value].Clone();
                _cutSelectedShapeIndex = _selectedShapeIndex;
            }
        }
        private void CopyButton_OnClick(object sender, RoutedEventArgs e)
        {
            CopyShape();
        }

        private void PasteSplitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_copiedShape != null)
            {
                PasteShape();
            }
        }

        private void CutButton_OnClick(object sender, RoutedEventArgs e)
        {
            CutShape();
        }

        private void SelectShape(object sender, MouseButtonEventArgs e)
        {

            if (_selectedShapeIndex != null)
            {
                int index = _selectedShapeIndex.Value;
                _shapes[_selectedShapeIndex.Value].IsSelected = false;

                //remove adorner của shape khác
                Adorner[] toRemoveArray =
                    AdornerLayer.GetAdornerLayer(DrawCanvas).GetAdorners(DrawCanvas.Children[lowerLayersShapesCount + index]);
                if (toRemoveArray != null)
                {
                    for (int x = 0; x < toRemoveArray.Length; x++)
                    {
                        AdornerLayer.GetAdornerLayer(DrawCanvas).Remove(toRemoveArray[x]);
                    }
                }

            };

            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].IsSelected)
                {
                    _selectedShapeIndex = i;
                    if (_shapes[i].Name != "Line")


                    {
                        AdornerLayer.GetAdornerLayer(DrawCanvas.Children[lowerLayersShapesCount + i])
                            .Add(new ResizeShapeAdorner(DrawCanvas.Children[lowerLayersShapesCount + i], _shapes[i]));
                    }
                    else
                    {
                        AdornerLayer.GetAdornerLayer(DrawCanvas.Children[lowerLayersShapesCount + i])
                            .Add(new ResizeLineAdorner(DrawCanvas.Children[lowerLayersShapesCount + i], _shapes[i]));
                    }
                    //ReDraw();
                    return;
                }

            }

            _selectedShapeIndex = null;
            //ReDraw();

        }

        private void Zoom(float newProp)
        {
            var st = new ScaleTransform();
            DrawCanvas.RenderTransform = st;
            float prop = (newProp / 100);
            st.ScaleX = prop;
            st.ScaleY = prop;
            DrawCanvas.Height = (this.ActualHeight - 170) * prop;
            DrawCanvas.Width = this.ActualWidth * prop;
            if (newProp == 50)
            {
                DrawCanvas.Height = this.ActualHeight - 170;
                DrawCanvas.Width = this.ActualWidth;
            }
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoomingSlider.Value = ZoomingSlider.Value + 50;
        }
        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            ZoomingSlider.Value = ZoomingSlider.Value - 50;
        }

        private void ZoomingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (startZooming < 3)
            {
                startZooming++;
                return;
            }
            var c = this.Height;
            var currentValue = (float)ZoomingSlider.Value;
            Zoom(currentValue);

            currentProportion = currentValue;
            Proportion.Text = $"{currentProportion}%";
        }


        private void DeleteShape()
        {
            if (_selectedShapeIndex is not null)
            {

                _shapes.RemoveAt(_selectedShapeIndex.Value);
                //khỏi phải vẽ lại
                DrawCanvas.Children.RemoveAt(lowerLayersShapesCount + _selectedShapeIndex.Value);
                _selectedShapeIndex = null;
            }
        }
        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteShape();
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            // The text box grabs all input.
            e.Handled = true;

            // Fetch the actual shortcut key.
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Ignore modifier keys.
            if (key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key.ToString());

            if (shortcutText.ToString() == "Ctrl+C" && _selectedShapeIndex != null)
            {
                DrawCanvas.MouseUp += GetPoint_MouseUp;
                _copiedShape = _shapes[_selectedShapeIndex.Value].Clone();
                testblock.Text = shortcutText.ToString();
                //shortcutText.Clear();
            }
            if (shortcutText.ToString() == "Delete")
            {
                DeleteShape();
                testblock.Text = shortcutText.ToString();
            }
            if (shortcutText.ToString() == "Ctrl+V" && _copiedShape != null)
            {
                PasteShape();
                testblock.Text = shortcutText.ToString();
            }
            if (shortcutText.ToString() == "Ctrl+X" && _selectedShapeIndex != null)
            {
                if (_selectedShapeIndex != null)
                {
                    DrawCanvas.MouseUp += GetPoint_MouseUp;
                    _copiedShape = _shapes[_selectedShapeIndex.Value].Clone();
                    _cutSelectedShapeIndex = _selectedShapeIndex;
                }
                testblock.Text = shortcutText.ToString();
            }
            if (shortcutText.ToString() == "Ctrl+S")
            {
                Save();
                testblock.Text = shortcutText.ToString();
            }


        }

        private void PaintMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualHeight < 300)
            {
                return;
            }

            DrawCanvas.Height = this.ActualHeight - 170;
            DrawCanvas.Width = this.ActualWidth;
        }
        private void buttonFill_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShapeIndex != null)
            {
                _shapes[_selectedShapeIndex.Value].Fill = FillColor;
                ReDraw();
            }

        }
        private void buttonOutline_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShapeIndex != null)
            {
                _shapes[_selectedShapeIndex.Value].Color = OutlineColor;
                ReDraw();
            }
        }


        private void LayerToggleBtn_Click(object sender, RoutedEventArgs e)
        {

            if (_selectedShapeIndex is not null)
            {
                _shapes[_selectedShapeIndex.Value].IsSelected = false;
                _selectedShapeIndex = null;
            }

            lowerLayersShapesCount = 0;
            for (int k = 0; k < _currentLayer; k++)
            {
                if (layers[k].isChecked) lowerLayersShapesCount += layers[k]._shapes.Count;
            }
            _cutSelectedShapeIndex = null;
            _copiedShape = null;
            ReDraw();
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            layers.Add(new Layer(layers.Count));
        }

        private void DeleteLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            //Đảm bảo luôn có ít nhất 1 layer
            if (layers.Count == 1)
            {
                MessageBox.Show("Can not delete this layer, you need to keep atleast 1 layer");
                return;
            }

            if (ListViewLayers.SelectedItems.Count == 0)
                return;
            layers.RemoveAt(ListViewLayers.SelectedIndex);
            _currentLayer = -1;

            //Cập nhật lại tên layer
            for (int i = 0; i < layers.Count(); i++)

            {
                layers[i].index = i;
            }

            //đây là hàm ListViewLayers_SelectionChanged
            //Check lúc xóa thì không có layer nào được chọn nên ListViewLayers.SelectedIndex=-1
            _currentLayer = ListViewLayers.SelectedIndex == -1 ? 0 : ListViewLayers.SelectedIndex;
            if (_selectedShapeIndex is not null)
            {
                _shapes[_selectedShapeIndex.Value].IsSelected = false;
                _selectedShapeIndex = null;
            }
            _shapes = layers[_currentLayer]._shapes;

            lowerLayersShapesCount = 0;
            for (int k = 0; k < _currentLayer; k++)
            {
                if (layers[k].isChecked) lowerLayersShapesCount += layers[k]._shapes.Count;
            }
            _cutSelectedShapeIndex = null;
            _copiedShape = null;

            ReDraw();
        }

        private void ListViewLayers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //Check lúc xóa thì không có layer nào được chọn nên ListViewLayers.SelectedIndex=-1
            if (layers.Count() == 0) return;
            _currentLayer = ListViewLayers.SelectedIndex == -1 ? 0 : ListViewLayers.SelectedIndex;
            if (_selectedShapeIndex is not null)
            {
                _shapes[_selectedShapeIndex.Value].IsSelected = false;
                _selectedShapeIndex = null;
            }

            _shapes = layers[_currentLayer]._shapes;

            lowerLayersShapesCount = 0;
            for (int k = 0; k < _currentLayer; k++)
            {
                if (layers[k].isChecked) lowerLayersShapesCount += layers[k]._shapes.Count;
            }
            _cutSelectedShapeIndex = null;
            _copiedShape = null;

            ReDraw();
        }

        private void UndoModule()
        {
            if (_shapes.Count == 0) return;
            if (_selectedShapeIndex is not null)
            {
                currentIShape.Add(_shapes[_shapes.Count - 1]);
                _shapes.RemoveAt(_shapes.Count - 1);
                //khỏi phải vẽ lại
                ReDraw();
            }
        }

        private void RedoModule()
        {
            if (currentIShape.Count == 0) return;
            _shapes.Add(currentIShape[currentIShape.Count - 1]);
            currentIShape.RemoveAt(currentIShape.Count - 1);
            ReDraw();
        }
        private void Undo_OnClick(object sender, RoutedEventArgs e)
        {
            UndoModule();
        }

        private void Redo_OnClick(object sender, RoutedEventArgs e)
        {
            RedoModule();
        }
    }
}
