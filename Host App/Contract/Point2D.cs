using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract
{
    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public string Name => "Point";
        public bool Selected { get; set; }

        public void HandleStart(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void HandleEnd(double x, double y)
        {
            X = x;
            Y = y;
        }

        public UIElement Draw(int thickness, string color)
        {
            Line l = new Line()
            {
                X1 = X,
                Y1 = Y,
                X2 = X,
                Y2 = Y,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(Colors.Red),
            };

            return l;
        }

        public Point2D Clone()
        {
            return new Point2D();
        }
    }

}
