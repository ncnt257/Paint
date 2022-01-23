using Contract;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Rectangle2D
{
    class Rectangle2D : IShape
    {
        public int isShift { get; set; }
        public string Name => "Rectangle";
        public Point2D Start { get; set; }
        public Point2D End { get; set; }
        public int Thickness { get; set; }
        public Color Color { get; set; }
        public Color Fill { get; set; }
        public DoubleCollection StrokeType { get; set; }
        public bool IsSelected { get; set; }

        public Rectangle2D()
        {
            Start = new Point2D();
            End = new Point2D();
            Fill = Colors.Transparent;
        }
        public UIElement Draw(bool isSelectMode, bool isOnTopLayer, int shift)

        {
            double width = End.X - Start.X;
            double height = End.Y - Start.Y;

            if (isShift == 0)
            {
                isShift = shift;
            }
            if (isShift == 1)
            {
                if (width * height > 0)
                {
                    height = width;
                }
                else
                {
                    height = -1 * width;
                }
            }
            var rectangle = new Rectangle()
            {
                Width = (int)Math.Abs(width),
                Height = (int)Math.Abs(height),

                StrokeThickness = Thickness,
                Stroke = new SolidColorBrush(Color),
                StrokeDashArray = StrokeType,
                Fill = new SolidColorBrush(Fill),
            };
            if (isSelectMode && isOnTopLayer)
            {
                rectangle.Cursor = Cursors.Hand;
                rectangle.MouseLeftButtonDown += ShapeSelected;
            }
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


            return rectangle;
        }

        public void WriteShapeBinary(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Start.X);
            bw.Write(Start.Y);
            bw.Write(End.X);
            bw.Write(End.Y);
            bw.Write(Thickness);
            bw.Write(isShift);
            bw.Write(IsSelected);
            bw.Write(Color.ToString());
            bw.Write(Fill.ToString());
            bw.Write(StrokeType.Count);
            foreach (var item in StrokeType)
            {
                bw.Write(item);
            }
        }

        public IShape ReadShapeBinary(BinaryReader br)
        {
            var result = new Rectangle2D();
            result.Start.X = br.ReadDouble();
            result.Start.Y = br.ReadDouble();
            result.End.X = br.ReadDouble();
            result.End.Y = br.ReadDouble();
            result.Thickness = br.ReadInt32();
            result.isShift = br.ReadInt32();
            result.IsSelected = br.ReadBoolean();
            var tempColor = br.ReadString();
            result.Color = (Color)ColorConverter.ConvertFromString(tempColor);
            var tempFill = br.ReadString();
            result.Fill = (Color)ColorConverter.ConvertFromString(tempFill);
            var count = br.ReadInt32();
            result.StrokeType = new DoubleCollection();
            for (int i = 0; i < count; i++)
            {
                result.StrokeType.Add(br.ReadDouble());
            }
            return result;
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
        public IShape Clone()
        {
            var rec = (Rectangle2D)MemberwiseClone();

            rec.IsSelected = false;
            if (Start is not null)
                rec.Start = new Point2D(this.Start);
            if (End is not null)
                rec.End = new Point2D(this.End);
            if (StrokeType is not null)
                rec.StrokeType = new DoubleCollection(this.StrokeType);
            return rec;
        }
    }
}
