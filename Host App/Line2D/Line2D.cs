using Contract;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Line2D
{
    public class Line2D : IShape
    {
        public string Name => "Line";
        public Point2D Start { get; set; }
        public Point2D End { get; set; }
        public int Thickness { get; set; }

        public Color Color { get; set; }
        public Color Fill { get; set; }
        public DoubleCollection StrokeType { get; set; }

        public void HandleStart(double x, double y)
        {
            Start = new Point2D() { X = x, Y = y };
        }

        public void HandleEnd(double x, double y)
        {
            End = new Point2D() { X = x, Y = y };
        }

        public UIElement Draw(bool isSelectMode)
        {
            Line l = new Line()
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                StrokeThickness = Thickness,
                StrokeDashArray = StrokeType,
                Stroke = new SolidColorBrush(Color),
                Fill = new SolidColorBrush(Color),
                
            };
            if (IsSelected)
            {
                
                l.Stroke = new SolidColorBrush(Colors.Blue);
                l.Fill = new SolidColorBrush(Colors.Blue);
            }

            if (isSelectMode)
            {
                l.Cursor = Cursors.Hand;
                l.MouseLeftButtonDown += ShapeSelected;
            }
            
            return l;
        }

        public Line2D()
        {
            Start = new Point2D();
            End = new Point2D();
            Color = Colors.Transparent;
        }

        private void ShapeSelected(object sender,
            MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        public bool IsSelected { get; set; }

        public IShape Clone()
        {
            var line = (Line2D)MemberwiseClone();
            line.IsSelected = false;
            if(Start is not null)
                line.Start = new Point2D(this.Start);
            if (End is not null)
                line.End = new Point2D(this.End);
            if (StrokeType is not null)
                line.StrokeType = new DoubleCollection(this.StrokeType);
            return line;
        }




    }

}

