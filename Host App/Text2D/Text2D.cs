using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Text2D
{
    public class Text2D : IShape
    {
        public string Name => "Text";
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
                Stroke = new SolidColorBrush(Color),
                StrokeThickness = Thickness,
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
            Start = new Point2D() { X = x, Y = y };
        }

        public void HandleEnd(double x, double y)
        {
            End = new Point2D() { X = x, Y = y };
        }
       /* public TextBox Writing()
        {
            var textBlock = new TextBox();

            textBlock.Text = "thang is the best";

            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            textBlock.BorderThickness = new Thickness(0);
            textBlock.BorderBrush = Brushes.White;

            Canvas.SetLeft(textBlock, Start.X);

            Canvas.SetTop(textBlock, Start.Y);

            return textBlock;
        }*/
        


        public Text2D()
        {
            Start = new Point2D();
            End = new Point2D();

        }
        public IShape Clone()
        {
            var text = (Text2D)MemberwiseClone();
            text.IsSelected = false;
            if (Start is not null)
                text.Start = new Point2D(this.Start);
            if (End is not null)
                text.End = new Point2D(this.End);
            if (StrokeType is not null)
                text.StrokeType = new DoubleCollection(this.StrokeType);
            return text;
        }
    }
}
