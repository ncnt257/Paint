using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Contract;

namespace Paint
{
    public class ResizeLineAdorner : Adorner
    {
        const double THUMB_SIZE = 5;
        private IShape _shape;
        private Point start;
        private Point end;
        private Thumb startThumb;
        private Thumb endThumb;
        private Line selectedLine;
        private VisualCollection visualChildren;


        private Thumb GetResizeThumb()
        {
            var thumb = new Thumb()
            {
                //Background = Brushes.Red,
                Width = THUMB_SIZE,
                Height = THUMB_SIZE,
                Cursor = Cursors.SizeAll,
                Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = GetThumbTemplate(new SolidColorBrush(Colors.White))
                }
            };
            return thumb;
        }
        private FrameworkElementFactory GetThumbTemplate(Brush back)
        {
            back.Opacity = 1;
            var fef = new FrameworkElementFactory(typeof(Rectangle));
            fef.SetValue(Rectangle.FillProperty, back);
            fef.SetValue(Rectangle.StrokeProperty, Brushes.SlateBlue);
            fef.SetValue(Rectangle.StrokeThicknessProperty, (double)1);
            return fef;
        }

        // Constructor
        public ResizeLineAdorner(UIElement adornedElement, IShape shape) : base(adornedElement)
        {
            _shape = shape;
            visualChildren = new VisualCollection(this);

            startThumb = GetResizeThumb();
            endThumb = GetResizeThumb();

            startThumb.DragDelta += StartDragDelta;
            endThumb.DragDelta += EndDragDelta;

            visualChildren.Add(startThumb);
            visualChildren.Add(endThumb);

            selectedLine = AdornedElement as Line;
        }

        // Event for the Thumb Start Point
        private void StartDragDelta(object sender, DragDeltaEventArgs e)
        {
            Point position = Mouse.GetPosition(this);

            selectedLine.X1 = position.X;
            selectedLine.Y1 = position.Y;
            _shape.Start.X = position.X;
            _shape.Start.Y = position.Y;
        }

        // Event for the Thumb End Point
        private void EndDragDelta(object sender, DragDeltaEventArgs e)
        {
            Point position = Mouse.GetPosition(this);

            selectedLine.X2 = position.X;
            selectedLine.Y2 = position.Y;
            _shape.End.X = position.X;
            _shape.End.Y = position.Y;
        }

        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }


        protected override Size ArrangeOverride(Size finalSize)
        {
            selectedLine = AdornedElement as Line;

            double left = Math.Min(selectedLine.X1, selectedLine.X2);
            double top = Math.Min(selectedLine.Y1, selectedLine.Y2);

            var startRect = new Rect(selectedLine.X1 - (startThumb.Width / 2), selectedLine.Y1 - (startThumb.Width / 2), startThumb.Width, startThumb.Height);
            startThumb.Arrange(startRect);

            var endRect = new Rect(selectedLine.X2 - (endThumb.Width / 2), selectedLine.Y2 - (endThumb.Height / 2), endThumb.Width, endThumb.Height);
            endThumb.Arrange(endRect);

            return finalSize;
        }
    }
}
