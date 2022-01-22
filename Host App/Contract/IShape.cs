using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        int isShift { get; set; }

        string Name { get; }
        bool IsSelected { get; set; }
        Point2D Start { get; set; }
        Point2D End { get; set; }
        int Thickness { get; set; }
        Color Color { get; set; }
        Color Fill { get; set; }
        DoubleCollection StrokeType { get; set; }

        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);


        void ShapeSelected(object sender,
            MouseButtonEventArgs e);




        UIElement Draw(bool isSelectMode, bool isOnTopLayer,int shift);
        void WriteBinary(BinaryWriter bw);
        IShape ReadBinary(BinaryReader br);

        IShape Clone();


    }
}
