﻿using System;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
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
        IShape Clone();//clone dùng đc ngay cả khi Shape có thuộc tính null
        IShape Copy();//copy một shape đã hoàn chỉnh


    }
}
