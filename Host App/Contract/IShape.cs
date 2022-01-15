using System;
using System.Windows;

namespace Contract
{
    public interface IShape
    {
        public Point2D _start { get; set; }
        public Point2D _end { get; set; }
        string Name { get; }
        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);

        UIElement Draw(int thickness, string color);
        IShape Clone();
    }
}
