using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Extensions;

namespace Mitten
{
    /*
    public class StraightLine
    {
        float a, b, c, m, q;
        public StraightLine(Vector2 axis, Vector2 origin)
        {
            if (axis.X != 0)
            {
                m = axis.Y / axis.X;
                a = m;
                b = -1;
            }
            else
            {
                m = float.PositiveInfinity;
                b = 0;
                //a = origin.X;
            }
            q = origin.Y - m * origin.X;
        }

        public StraightLine(Vector2 A, Vector2 B)
        {
            if (A.X == B.X)
            {
                m = float.PositiveInfinity;
                b = 0;
            }
            if (A.Y == B.Y)
            {
                m = 0;

            }

        }
    }*/

    public class LineEquation
    {
        public LineEquation(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;

            IsVertical = Math.Abs(End.X - start.X) < 0.00001f;
            M = (End.Y - Start.Y) / (End.X - Start.X);
            A = -M;
            B = 1;
            C = Start.Y - M * Start.X;

            Point a = new Point(1, 1);
        }

        

        public bool IsVertical { get; private set; }

        public float M { get; private set; }

        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }

        public bool IntersectsWithLine(LineEquation otherLine, out Vector2 intersectionPoint)
        {
            intersectionPoint = new Vector2(0, 0);
            if (IsVertical && otherLine.IsVertical)
                return false;
            if (IsVertical || otherLine.IsVertical)
            {
                intersectionPoint = GetIntersectionPointIfOneIsVertical(otherLine, this);
                return true;
            }
            float delta = A * otherLine.B - otherLine.A * B;
            bool hasIntersection = Math.Abs(delta - 0) > 0.0001f;
            if (hasIntersection)
            {
                float x = (otherLine.B * C - B * otherLine.C) / delta;
                float y = (A * otherLine.C - otherLine.A * C) / delta;
                intersectionPoint = new Vector2(x, y);
            }
            return hasIntersection;
        }

        private static Vector2 GetIntersectionPointIfOneIsVertical(LineEquation line1, LineEquation line2)
        {
            LineEquation verticalLine = line2.IsVertical ? line2 : line1;
            LineEquation nonVerticalLine = line2.IsVertical ? line1 : line2;

            float y = (verticalLine.Start.X - nonVerticalLine.Start.X) *
                       (nonVerticalLine.End.Y - nonVerticalLine.Start.Y) /
                       ((nonVerticalLine.End.X - nonVerticalLine.Start.X)) +
                       nonVerticalLine.Start.Y;
            float x = line1.IsVertical ? line1.Start.X : line2.Start.X;
            return new Vector2(x, y);
        }

        public bool IntersectWithSegmentOfLine(LineEquation otherLine, out Vector2 intersectionPoint)
        {
            bool hasIntersection = IntersectsWithLine(otherLine, out intersectionPoint);
            if (hasIntersection)
                return intersectionPoint.IsBetweenTwoPoints(otherLine.Start, otherLine.End);
            return false;
        }

        public bool GetIntersectionLineForRay(OBB rectangle, out LineEquation intersectionLine)
        {
            if (Start == End)
            {
                intersectionLine = null;
                return false;
            }
            List<LineEquation> lines = new List<LineEquation>();
            for (int i = 0; i < 3; i++)
            {
                lines.Add(new LineEquation(rectangle.Vertex(i), rectangle.Vertex(i+1)));
            }
            lines.Add(new LineEquation(rectangle.Vertex(3), rectangle.Vertex(0)));
            //IEnumerable<LineEquation> lines = rectangle.GetLinesForRectangle();
            intersectionLine = new LineEquation(new Vector2(0, 0), new Vector2(0, 0));
            var intersections = new Dictionary<LineEquation, Vector2>();
            foreach (LineEquation equation in lines)
            {
                Vector2 point;
                if (IntersectWithSegmentOfLine(equation, out point))
                    intersections[equation] = point;
            }
            if (!intersections.Any())
                return false;

            var intersectionPoints = new SortedDictionary<double, Vector2>();
            foreach (var intersection in intersections)
            {
                if (End.IsBetweenTwoPoints(Start, intersection.Value) ||
                    intersection.Value.IsBetweenTwoPoints(Start, End))
                {
                    float distanceToPoint = Start.DistanceToPoint(intersection.Value);
                    intersectionPoints[distanceToPoint] = intersection.Value;
                }
            }
            if (intersectionPoints.Count == 1)
            {
                Vector2 endPoint = intersectionPoints.First().Value;
                intersectionLine = new LineEquation(Start, endPoint);
                return true;
            }

            if (intersectionPoints.Count == 2)
            {
                Vector2 start = intersectionPoints.First().Value;
                Vector2 end = intersectionPoints.Last().Value;
                intersectionLine = new LineEquation(start, end);
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return "[" + Start + "], [" + End + "]";
        }
    }
}
