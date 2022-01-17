using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ellipse2D
{
    public class Ellipse2D : IShape
    {
        public string Name => "Ellipse";
        public Point2D Start { get; set; }
        public Point2D End { get; set; }
        public int Thickness { get; set; }
        public Color Color { get; set; }
        public Color Fill { get; set; }
        public DoubleCollection StrokeType { get; set; }
        public UIElement Draw(bool isSelectMode)
        {
            var width = End.X - Start.X;
            var height = End.Y - Start.Y;
            var ellipse = new Ellipse()
            {
                Width = (int)Math.Abs(width),
                Height = (int)Math.Abs(height),
                StrokeThickness = Thickness,
                Stroke = new SolidColorBrush(Color),
                StrokeDashArray = StrokeType,
                Fill = new SolidColorBrush(Fill),

            };
            if (IsSelected)
            {

                ellipse.Stroke = new SolidColorBrush(Colors.Blue);

            }
            if (isSelectMode)
            {
                ellipse.Cursor = Cursors.Hand;
                ellipse.MouseLeftButtonDown += ShapeSelected;
            }
            if (width > 0 && height > 0)
            {
                Canvas.SetLeft(ellipse, Start.X);
                Canvas.SetTop(ellipse, Start.Y);
            }
            if (width > 0 && height < 0)
            {
                Canvas.SetLeft(ellipse, Start.X);
                Canvas.SetTop(ellipse, End.Y);
            }
            if (width < 0 && height > 0)
            {
                Canvas.SetLeft(ellipse, End.X);
                Canvas.SetTop(ellipse, Start.Y);
            }
            if (width < 0 && height < 0)
            {
                Canvas.SetLeft(ellipse, End.X);
                Canvas.SetTop(ellipse, End.Y);
            }
            ellipse.MouseMove += MoveShape;
            return ellipse;
        }
        public void MoveShape(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsSelected)
            {
                DragDrop.DoDragDrop(sender as Ellipse, sender as Ellipse, DragDropEffects.Move);
            }
        }
        public void ShapeSelected(object sender,
            MouseButtonEventArgs e)
        {
            IsSelected = true;
        }
        public void HandleStart(double x, double y)
        {
            Start.X = x;
            Start.Y = y;
        }

        public void HandleEnd(double x, double y)
        {
            End.X = x;
            End.Y = y;
        }
        
        public Ellipse2D()
        {
            Start = new Point2D();
            End = new Point2D();
            Fill = Colors.Transparent;
        }
        public IShape Clone()
        {
            var ellipse = (Ellipse2D)MemberwiseClone();
            ellipse.IsSelected = false;
            if (Start is not null)
                ellipse.Start = new Point2D(this.Start);
            if (End is not null)
                ellipse.End = new Point2D(this.End);
            if (StrokeType is not null)
                ellipse.StrokeType = new DoubleCollection(this.StrokeType);
            return ellipse;
        }

        public bool IsSelected { get; set; }
        
    }
}
