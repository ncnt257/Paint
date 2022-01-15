﻿using System;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        public Point2D _start { get; set; }
        public Point2D _end { get; set; }
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
        
        UIElement Draw();
        IShape Clone();



    }
}
