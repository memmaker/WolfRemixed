using Microsoft.Xna.Framework;
using System;

namespace XNAHelper
{
    public static class XNAPointExtension
    {
        public static Vector2 ToCellCenteredVector2(this Point point)
        {
            return new Vector2(point.X + 0.5f, point.Y + 0.5f);
        }

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static bool IsNeighbor(this Point myPoint, Point otherPoint)
        {
            return (myPoint.X == otherPoint.X && Math.Abs(myPoint.Y - otherPoint.Y) == 1) ||
                (myPoint.Y == otherPoint.Y && Math.Abs(myPoint.X - otherPoint.X) == 1) ||
                ((Math.Abs(myPoint.X - otherPoint.X) == 1) && (Math.Abs(myPoint.Y - otherPoint.Y) == 1));
        }

        public static float Distance(this Point firstPoint, Point secondPoint)
        {
            return Vector2.Distance(firstPoint.ToCellCenteredVector2(), secondPoint.ToCellCenteredVector2());
        }

        public static Point Add(Point myPoint, Point otherPoint)
        {
            return new Point(myPoint.X + otherPoint.X, myPoint.Y + otherPoint.Y);
        }
    }

    public static class SystemDrawingPointExtension
    {
        public static Vector2 ToCellCenteredVector2(this System.Drawing.Point point)
        {
            return new Vector2(point.X + 0.5f, point.Y + 0.5f);
        }

        public static Vector2 ToVector2(this System.Drawing.Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static bool IsNeighbor(this System.Drawing.Point myPoint, System.Drawing.Point otherPoint)
        {
            return (myPoint.X == otherPoint.X && Math.Abs(myPoint.Y - otherPoint.Y) == 1) ||
                (myPoint.Y == otherPoint.Y && Math.Abs(myPoint.X - otherPoint.X) == 1) ||
                ((Math.Abs(myPoint.X - otherPoint.X) == 1) && (Math.Abs(myPoint.Y - otherPoint.Y) == 1));
        }

        public static float Distance(this System.Drawing.Point firstPoint, System.Drawing.Point secondPoint)
        {
            return Vector2.Distance(firstPoint.ToCellCenteredVector2(), secondPoint.ToCellCenteredVector2());
        }

        public static System.Drawing.Point Add(System.Drawing.Point myPoint, System.Drawing.Point otherPoint)
        {
            return new System.Drawing.Point(myPoint.X + otherPoint.X, myPoint.Y + otherPoint.Y);
        }
    }
}