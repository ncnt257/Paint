using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Rectangle2D
{
    class Square2D : IShape
    {
        private Point2D _leftTop = new Point2D();
        private Point2D _rightBottom = new Point2D();
        public string Name => "Square";

        public UIElement Draw(int thickness, string color)
        {
            var width = _rightBottom.X - _leftTop.X;
            var height = _rightBottom.Y - _leftTop.Y;
            var square = new Rectangle()
            {
                Width = (int)Math.Abs(width),
                Height = (int)Math.Abs(height),
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = thickness
            };
            if (width > 0)
            {
                Canvas.SetLeft(square, _leftTop.X);
                Canvas.SetTop(square, _leftTop.Y);
            }
            else
            {
                Canvas.SetLeft(square, _rightBottom.X);
                Canvas.SetTop(square, _rightBottom.Y);
            }

            return square;
        }

        public void HandleStart(double x, double y)
        {
            _leftTop.X = x;
            _leftTop.Y = y;
        }

        public void HandleEnd(double x, double y)
        {
            _rightBottom.X = x;
            _rightBottom.Y = y;
        }

        public IShape Clone()
        {
            return new Square2D();
        }
    }
}
