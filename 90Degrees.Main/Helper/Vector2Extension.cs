using Microsoft.Xna.Framework;
using Point = System.Drawing.Point;

namespace XNAHelper
{
    public static class Vector2Extension
    {
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int) vector.X,(int) vector.Y);
        }
    }
}