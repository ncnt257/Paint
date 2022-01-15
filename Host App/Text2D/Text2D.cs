using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Text2D
{
    public class Text2D : IShape
    {
        public Point2D _start { get; set; }
        public Point2D _end { get; set; }
        public string content { get; set; }

        public string Name => "Text";

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }

        public void HandleEnd(double x, double y)
        {
            _end = new Point2D() { X = x, Y = y };
        }
       /* public TextBox Writing()
        {
            var textBlock = new TextBox();

            textBlock.Text = "thang is the best";

            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            textBlock.BorderThickness = new Thickness(0);
            textBlock.BorderBrush = Brushes.White;

            Canvas.SetLeft(textBlock, _start.X);

            Canvas.SetTop(textBlock, _start.Y);

            return textBlock;
        }*/
        public UIElement Draw(int thickness, string color)
        {
            var width = _end.X - _start.X;
            var height = _end.Y - _start.Y;
            var rectangle = new Rectangle()
            {
                Width = (int)Math.Abs(width),
                Height = (int)Math.Abs(height),
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = thickness
            };
            if (width > 0 && height > 0)
            {
                Canvas.SetLeft(rectangle, _start.X);
                Canvas.SetTop(rectangle, _start.Y);
            }
            if (width > 0 && height < 0)
            {
                Canvas.SetLeft(rectangle, _start.X);
                Canvas.SetTop(rectangle, _end.Y);
            }
            if (width < 0 && height > 0)
            {
                Canvas.SetLeft(rectangle, _end.X);
                Canvas.SetTop(rectangle, _start.Y);
            }
            if (width < 0 && height < 0)
            {
                Canvas.SetLeft(rectangle, _end.X);
                Canvas.SetTop(rectangle, _end.Y);
            }

            return rectangle;
        }

        public IShape Clone()
        {
            return new Text2D();
        }
    }
}
