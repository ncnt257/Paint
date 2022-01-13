using System;
using System.Windows;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
        bool Selected { get; set; }

        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);

        UIElement Draw(int thickness, string color);
        IShape Clone();
    }
}
