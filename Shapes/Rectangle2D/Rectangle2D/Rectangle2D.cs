using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Rectangle2D
{
    class Rectangle2D : IShape
    {
        
        public string Name => "Rectangle";
        public Point2D Start { get; set; }
        public Point2D End { get; set; }
        public int Thickness { get; set; }
        public Color Color { get; set; }
        public Color Fill { get; set; }

        public DoubleCollection StrokeType { get; set; }
        public UIElement Draw()
        {
            var width = End.X - Start.X;
            var height = End.Y - Start.Y;
            var rectangle = new Rectangle()
            {
                Width = (int)Math.Abs(width),
                Height = (int)Math.Abs(height),

                StrokeThickness = Thickness,
                Stroke = new SolidColorBrush(Color),
                StrokeDashArray = StrokeType,
                Fill = new SolidColorBrush(Fill),
                Cursor = Cursors.Hand

            };
            if (width > 0 && height > 0)
            {
                Canvas.SetLeft(rectangle, Start.X);
                Canvas.SetTop(rectangle, Start.Y);
            }
            if (width > 0 && height < 0)
            {
                Canvas.SetLeft(rectangle, Start.X);
                Canvas.SetTop(rectangle, End.Y);
            }
            if (width < 0 && height > 0)
            {
                Canvas.SetLeft(rectangle, End.X);
                Canvas.SetTop(rectangle, Start.Y);
            }
            if (width < 0 && height < 0)
            {
                Canvas.SetLeft(rectangle, End.X);
                Canvas.SetTop(rectangle, End.Y);
            }


            rectangle.MouseLeftButtonDown += ShapeSelected;
            //rectangle.MouseLeftButtonDown += ShapeSelected;
            return rectangle;
        }
        private void ShapeSelected(object sender,
            MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        public bool IsSelected { get; set; }
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
        public Rectangle2D()
        {
            Start = new Point2D();
            End = new Point2D();
            Fill = Colors.Transparent;
        }
        public IShape Clone()
        {
            var rec = (Rectangle2D)MemberwiseClone();
            rec.IsSelected = false;
            if(Start is not null)
                rec.Start = new Point2D(this.Start);
            if (End is not null)
                rec.End = new Point2D(this.End);
            if (StrokeType is not null)
                rec.StrokeType = new DoubleCollection(this.StrokeType);
            return rec;
        }
    }
}
