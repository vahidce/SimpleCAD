﻿using System;
using System.Linq;
using System.Windows;

namespace Viewer.Geometry
{
    public static class IntersectionHelper
    {
        /// <summary>
        /// Intersection of two line segments
        /// </summary>
        public static Point[] LineLineIntersection(LineGeometry la, LineGeometry lb)
        {
            if (!la.Bounds.IntersectsWith(lb.Bounds))
                return new Point[0];

            double dx1 = la.EndPoint.X - la.StartPoint.X;
            double dy1 = la.EndPoint.Y - la.StartPoint.Y;
            double dx2 = lb.EndPoint.X - lb.StartPoint.X;
            double dy2 = lb.EndPoint.Y - lb.StartPoint.Y;
            double d = dy2 * dx1 - dx2 * dy1;
            // Return if lines are parallel
            if (Math.Abs(Math.Round(d)) < MathHelper.Epsilon)
            {
                return new Point[0];
            }

            if (Math.Abs(la.StartPoint.X - la.EndPoint.X) < MathHelper.Epsilon &&
                Math.Abs(lb.StartPoint.X - lb.EndPoint.X) < MathHelper.Epsilon &&
                Math.Abs(la.StartPoint.X - lb.StartPoint.X) < MathHelper.Epsilon)
            {
                return new Point[0];
            }


            // We are using Cos of the angle between two lines to account for nearly parallel lines
            double cosAngle =
                Math.Abs((dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2)));
            if (cosAngle > 1 - 0.001)
            {
                return new Point[0];
            }

            double na = (lb.EndPoint.X - lb.StartPoint.X) * (la.StartPoint.Y - lb.StartPoint.Y) - (lb.EndPoint.Y - lb.StartPoint.Y) * (la.StartPoint.X - lb.StartPoint.X);
            double nb = (la.EndPoint.X - la.StartPoint.X) * (la.StartPoint.Y - lb.StartPoint.Y) - (la.EndPoint.Y - la.StartPoint.Y) * (la.StartPoint.X - lb.StartPoint.X);

            double ua = na / d;
            double ub = nb / d;

            Point[] pt = new Point[1] {new Point(la.StartPoint.X + ua * (la.EndPoint.X - la.StartPoint.X), la.StartPoint.Y + ua * (la.EndPoint.Y - la.StartPoint.Y)) };

            // The lines do not intersect (but they will if they are extended)
            if (ua + MathHelper.Epsilon < 0 ||
                ua - MathHelper.Epsilon > 1 ||
                ub + MathHelper.Epsilon < 0 ||
                ub - MathHelper.Epsilon > 1)
            {
                return new Point[0];
            }


            return pt;
        }

        public static Point[] CircleCircleIntersecton(CircleGeometry circle1, CircleGeometry circle2)
        {
            var cx0 = circle1.Center.X;
            var cx1 = circle2.Center.X;
            var cy0 = circle1.Center.Y;
            var cy1 = circle2.Center.Y;

            var radius0 = circle1.Radius;
            var radius1 = circle2.Radius;

            // Find the distance between the centers.
            double dx = cx0 - cx1;
            double dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                return new Point[0];
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                return new Point[0];
            }
            else if ((dist.AlmostEquals(0d)) && (radius0.AlmostEquals(radius1)))
            {
                // No solutions, the circles coincide.
                return new Point[0];
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                            radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                return new []
                {
                    new Point(
                        cx2 + h * (cy1 - cy0) / dist,
                        cy2 - h * (cx1 - cx0) / dist),
                    new Point(
                        cx2 - h * (cy1 - cy0) / dist,
                        cy2 + h * (cx1 - cx0) / dist)
                };
            }
        }

        public static Point[] PolygonPolygonIntersection(PolygonGeometry a, PolygonGeometry b)
        {
            if (!a.Bounds.IntersectsWith(b.Bounds))
                return new Point[0];

            LineGeometry[] edges = a.Edges;

            Point[] intersections = null;
            foreach (LineGeometry aEdge in edges)
            {
                intersections = intersections == null
                    ? GeometryPolygonIntersection(aEdge, b)
                    : intersections.Union(GeometryPolygonIntersection(aEdge, b)).ToArray();
            }

            return intersections;
        }

        public static Point[] LineCircleIntersection(LineGeometry line, CircleGeometry circle)
        {
            if (!line.Bounds.IntersectsWith(circle.Bounds))
                return new Point[0];

            Vector d = line.EndPoint - line.StartPoint;
            Vector f = line.StartPoint - circle.Center;

            double a = d * d;
            double b = 2 * f * (d);
            double c = f * (f) - circle.Radius * circle.Radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                return new Point[0];
            }


            discriminant = Math.Sqrt(discriminant);

            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            Point p1 = line.StartPoint + Vector.Multiply(d, t1);
            Point p2 = line.StartPoint + Vector.Multiply(d, t2);

            if (IsPointOnLineSegment(p1, line) && IsPointOnLineSegment(p2, line))
                return new Point[2] {p1, p2};

            if (IsPointOnLineSegment(p1, line))
                return new Point[] {p1};

            if (IsPointOnLineSegment(p2, line))
                return new Point[] {p2};

            return new Point[0];

        }

        private static bool IsPointOnLineSegment(Point a, LineGeometry line)
        {
            if (!IsPointOnLine(a, line))
                return false;

            double x1 = line.StartPoint.X;
            double x2 = line.EndPoint.X;
            double y1 = line.StartPoint.Y;
            double y2 = line.EndPoint.Y;

            double x = a.X;
            double y = a.Y;

            return Math.Min(x1, x2) <= x && x <= Math.Max(x1, x2) &&
                   Math.Min(y1, y2) <= y && y <= Math.Max(y1, y2);
        }

        private static bool IsPointOnLine(Point a, LineGeometry line)
        {
            double x1 = line.StartPoint.X;
            double x2 = line.EndPoint.X;
            double y1 = line.StartPoint.Y;
            double y2 = line.EndPoint.Y;

            double x = a.X;
            double y = a.Y;

            if (x1.AlmostEquals(x2))
            {
                return x1.AlmostEquals(x) && (Math.Min(y1, y2) <= y && y <= Math.Max(y1, y2));
            }

            if (y1.AlmostEquals(y2))
            {
                return y1.AlmostEquals(y) && Math.Min(x1, x2) <= x && x <= Math.Max(x1, x2);
            }

            return ((x - x1) / (x2 - x1)).AlmostEquals((y - y1) / (y2 - y1));
        }

        public static Point[] LinePolygonIntersection(LineGeometry line, PolygonGeometry polygon)
        {
            return GeometryPolygonIntersection(line, polygon);
        }

        public static Point[] CirclePolygonIntersection(CircleGeometry circle, PolygonGeometry polygon)
        {
            return GeometryPolygonIntersection(circle, polygon);
        }

        private static Point[] GeometryPolygonIntersection(Geometry geometry, PolygonGeometry polygon)
        {
            if (!geometry.Bounds.IntersectsWith(polygon.Bounds))
                return new Point[0];

            LineGeometry[] edges = polygon.Edges;

            Point[] intersections = null;
            foreach (LineGeometry edge in edges)
            {
                intersections = intersections == null
                    ? geometry.Intersect(edge)
                    : intersections.Union(geometry.Intersect(edge)).ToArray();
            }

            return intersections;
        }
    }
}