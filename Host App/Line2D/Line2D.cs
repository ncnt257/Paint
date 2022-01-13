using Contract;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Line2D
{
    public class Line2D : IShape
    {
        private Point2D _start = new Point2D();
        private Point2D _end = new Point2D();

        public string Name => "Line";

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }

        public void HandleEnd(double x, double y)
        {
            _end = new Point2D() { X = x, Y = y };
        }

        public UIElement Draw(int thickness, string color)
        {
            Line l = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(Colors.Red),
            };
            l.MouseLeftButtonDown += ShapeSelected;
            return l;
        }
        private void ShapeSelected(object sender,
            MouseButtonEventArgs e)
        {

            Selected = true;
        }

        public bool Selected { get; set; }
        public IShape Clone()
        {
            return new Line2D();
        }
        
    }

}
