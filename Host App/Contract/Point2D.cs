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

        public Point2D()
        {
        }

        public Point2D(Point2D p)
        {
            X = p.X;
            Y = p.Y;
        }


    }

}
