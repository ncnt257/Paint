﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ResizeShapeAdorner : Adorner
    {
        const double THUMB_SIZE = 5;
        //kích thước nhỏ nhất shape có thể resize
        const double MINIMAL_SIZE = 10;
        const double MOVE_OFFSET = 20;
        private IShape _shape;

        //9 thump xung quanh shape
        Thumb moveThumb, topLeftThumb, middleLeftThumb, bottomLeftThumb, topMiddleThumb, topRightThumb, middleRightThumb, bottomRightThumb, bottomMiddleThumb;

        //Hình chữ nhật xung quanh shape
        Rectangle thumbRectangle;

        VisualCollection visualCollection;

        public ResizeShapeAdorner(UIElement adorned, IShape shape) : base(adorned)
        {
            

            //ngoài việc resize shape trên canvas, cần update start.x ,y và end.x, y của IShape để khi vẽ lại, shape cũng được cập nhật
            _shape = shape;
            visualCollection = new VisualCollection(this);

            //thêm hcn xung quanh shape
            visualCollection.Add(thumbRectangle = GeteResizeRectangle());

            //các thumb bên trái shape
            visualCollection.Add(topLeftThumb = GetResizeThumb(Cursors.SizeNWSE, HorizontalAlignment.Left, VerticalAlignment.Top));
            visualCollection.Add(middleLeftThumb = GetResizeThumb(Cursors.SizeWE, HorizontalAlignment.Left, VerticalAlignment.Center));
            visualCollection.Add(bottomLeftThumb = GetResizeThumb(Cursors.SizeNESW, HorizontalAlignment.Left, VerticalAlignment.Bottom));

            //các thumb bên phải shape
            visualCollection.Add(topRightThumb = GetResizeThumb(Cursors.SizeNESW, HorizontalAlignment.Right, VerticalAlignment.Top));
            visualCollection.Add(middleRightThumb = GetResizeThumb(Cursors.SizeWE, HorizontalAlignment.Right, VerticalAlignment.Center));
            visualCollection.Add(bottomRightThumb = GetResizeThumb(Cursors.SizeNWSE, HorizontalAlignment.Right, VerticalAlignment.Bottom));

            //2 thumb ở giữa shape
            visualCollection.Add(topMiddleThumb = GetResizeThumb(Cursors.SizeNS, HorizontalAlignment.Center, VerticalAlignment.Top));
            visualCollection.Add(bottomMiddleThumb = GetResizeThumb(Cursors.SizeNS, HorizontalAlignment.Center, VerticalAlignment.Bottom));

            //thumb ở trên shape để move
            visualCollection.Add(moveThumb = GetMoveThumb());
        }

        //hcn xung quanh shape
        private Rectangle GeteResizeRectangle()
        {
            var rectangle = new Rectangle()
            {
                Width = AdornedElement.RenderSize.Width,
                Height = AdornedElement.RenderSize.Height,
                Fill = Brushes.Transparent,
                Stroke = Brushes.SlateBlue,
                StrokeDashArray = new DoubleCollection() { 3, 3 },
                StrokeThickness = (double)1
            };
            return rectangle;
        }

        //thumb xung quanh shape, ví dụ horizontal left, vertical top là thumb góc trái trên
        private Thumb GetResizeThumb(Cursor cur, HorizontalAlignment horizontal, VerticalAlignment vertical)
        {
            var thumb = new Thumb()
            {
                //Background = Brushes.Red,
                Width = THUMB_SIZE,
                Height = THUMB_SIZE,
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                Cursor = cur,
                Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = GetThumbTemplate(new SolidColorBrush(Colors.White))
                }
            };
            //lamda expression, dùng lại thumb trong hàm
            thumb.DragDelta += (s, e) =>
            {
                var element = AdornedElement as FrameworkElement;

                if (element == null)
                    return;

                this.ElementResize(element);

                switch (thumb.VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        if (element.Height + e.VerticalChange > MINIMAL_SIZE)
                        {
                            element.Height += e.VerticalChange;
                            thumbRectangle.Height += e.VerticalChange;
                           
                            _shape.End.Y += e.VerticalChange;
                        }
                        break;
                    case VerticalAlignment.Top:
                        if (element.Height - e.VerticalChange > MINIMAL_SIZE)
                        {
                            element.Height -= e.VerticalChange;
                            thumbRectangle.Height -= e.VerticalChange;

                            Canvas.SetTop(element, Canvas.GetTop(element) + e.VerticalChange);
                            
                            _shape.Start.Y += e.VerticalChange;
                        }
                        break;
                }
                switch (thumb.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        if (element.Width - e.HorizontalChange > MINIMAL_SIZE)
                        {
                            element.Width -= e.HorizontalChange;
                            thumbRectangle.Width -= e.HorizontalChange;
                            Canvas.SetLeft(element, Canvas.GetLeft(element) + e.HorizontalChange);
                            _shape.Start.X += e.HorizontalChange;
                        }
                        break;
                    case HorizontalAlignment.Right:
                        if (element.Width + e.HorizontalChange > MINIMAL_SIZE)
                        {
                            element.Width += e.HorizontalChange;
                            thumbRectangle.Width += e.HorizontalChange;
                            _shape.End.X += e.HorizontalChange;
                        }
                        break;
                }

                e.Handled = true;
            };
            return thumb;
        }

        private void ElementResize(FrameworkElement frameworkElement)
        {
            if (Double.IsNaN(frameworkElement.Width))
                frameworkElement.Width = frameworkElement.RenderSize.Width;
            if (Double.IsNaN(frameworkElement.Height))
                frameworkElement.Height = frameworkElement.RenderSize.Height;
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

        private Thumb GetMoveThumb()
        {
            var thumb = new Thumb()
            {
                Width = THUMB_SIZE,
                Height = THUMB_SIZE,
                Cursor = Cursors.Hand,
                Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = GetThumbTemplate(GetMoveEllipseBack())
                }
            };
            //lamda expression, dùng lại thumb trong hàm
            thumb.DragDelta += (s, e) =>
            {
                var element = AdornedElement as FrameworkElement;
                if (element == null)
                    return;

                Canvas.SetLeft(element, Canvas.GetLeft(element) + e.HorizontalChange);
                Canvas.SetTop(element, Canvas.GetTop(element) + e.VerticalChange);

                _shape.Start.X = Canvas.GetLeft(element);
                _shape.Start.Y = Canvas.GetTop(element);
                
                _shape.End.X = _shape.Start.X + element.ActualWidth;
                _shape.End.Y = _shape.Start.Y + element.ActualHeight;


            };
            return thumb;
        }

        private Brush GetMoveEllipseBack()
        {
            string lan = "M841.142857 570.514286c0 168.228571-153.6 336.457143-329.142857 336.457143s-329.142857-153.6-329.142857-336.457143c0-182.857143 153.6-336.457143 329.142857-336.457143v117.028571l277.942857-168.228571L512 0v117.028571c-241.371429 0-438.857143 197.485714-438.857143 453.485715S270.628571 1024 512 1024s438.857143-168.228571 438.857143-453.485714h-109.714286z m0 0";
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            var geometry = (Geometry)converter.ConvertFrom(lan);
            TileBrush bsh = new DrawingBrush(new GeometryDrawing(Brushes.Transparent, new Pen(Brushes.Black, 2), geometry));
            bsh.Stretch = Stretch.Fill;
            return bsh;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double offset = (THUMB_SIZE / 2);
            Size sz = new Size(THUMB_SIZE, THUMB_SIZE);

            topLeftThumb.Arrange(new Rect(new Point(-offset, -offset), sz));
            topMiddleThumb.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width / 2 - THUMB_SIZE / 2, -offset), sz));
            topRightThumb.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width - offset, -offset), sz));

            bottomLeftThumb.Arrange(new Rect(new Point(-offset, AdornedElement.RenderSize.Height - offset), sz));
            bottomMiddleThumb.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width / 2 - THUMB_SIZE / 2, AdornedElement.RenderSize.Height - offset), sz));
            bottomRightThumb.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width - offset, AdornedElement.RenderSize.Height - offset), sz));

            middleLeftThumb.Arrange(new Rect(new Point(-offset, AdornedElement.RenderSize.Height / 2 - THUMB_SIZE / 2), sz));
            middleRightThumb.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width - offset, AdornedElement.RenderSize.Height / 2 - THUMB_SIZE / 2), sz));

            moveThumb.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width / 2 - THUMB_SIZE / 2, -MOVE_OFFSET), sz));

            thumbRectangle.Arrange(new Rect(new Point(-offset, -offset), new Size(Width = AdornedElement.RenderSize.Width + THUMB_SIZE, Height = AdornedElement.RenderSize.Height + THUMB_SIZE)));

            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualCollection[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return visualCollection.Count;
            }
        }

    }
}
