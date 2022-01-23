using Contract;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Line2D
{
    [Serializable]
    public class Line2D : IShape
    {
        public int isShift { get; set; }
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

        public UIElement Draw(bool isSelectMode, bool isOnTopLayer, int shift)

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


            if (isSelectMode && isOnTopLayer)
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


        public void ShapeSelected(object sender,
            MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        public bool IsSelected { get; set; }

        public IShape Clone()
        {
            var line = (Line2D)MemberwiseClone();
            line.IsSelected = false;
            if (Start is not null)
                line.Start = new Point2D(this.Start);
            if (End is not null)
                line.End = new Point2D(this.End);
            if (StrokeType is not null)
                line.StrokeType = new DoubleCollection(this.StrokeType);
            return line;
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
            var result = new Line2D();
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
    }

}

