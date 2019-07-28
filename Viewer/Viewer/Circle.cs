﻿using System.Windows;
using System.Windows.Media;

namespace Viewer
{
    internal sealed class Circle : Shape
    {
        /// <summary>
        /// Initializes an instance of <see cref="Circle"/> class.
        /// </summary>
        public Circle(Brush fill, Pen pen, Point center, double radius)
        {
            Geometry = new CircleGeometry(center, radius);

            if (pen.CanFreeze)
                pen.Freeze();

            using (DrawingContext drawingContext = RenderOpen())
            {
                drawingContext.DrawEllipse(fill, pen, center, radius, radius);
            }
        }
    }
}