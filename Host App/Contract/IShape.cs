using System;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);
        public int Thickness { get; set; }
        public Color Color { get; set; }
        public DoubleCollection StrokeType { get; set; }
        UIElement Draw();
        IShape Clone();
    }
}
