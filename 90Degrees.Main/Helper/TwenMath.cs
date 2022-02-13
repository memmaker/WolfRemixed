using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace XNAHelper
{
    public enum RelativeDirection
    {
        Left,
        Right,
        SameDirection
    }
    /// <summary>
    /// Gets called on every cell the line tracing algorithm finds.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>true if the line tracing should continue and false if the trace should be canceled.</returns>
    public delegate bool CellHandler(int x, int y);
    public class TwenMath
    {
        public static Random Random = new Random();
        public static Vector2 RotationToDirectionVector(float rotation)
        {
            Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);
            return Vector2.Transform(-Vector2.UnitY, rotationMatrix);
        }

        public static float DirectionVectorToRotation(Vector2 direction)
        {
            return (float)Math.Atan2(direction.X, -direction.Y);
        }

        public static float RadianAngleBetween2DVectors(Vector2 from, Vector2 to)
        {
            return (float)(Math.Atan2(to.Y, to.X) - Math.Atan2(from.Y, from.X));
        }

        public static bool IsAtPosition(Vector2 source, Vector2 target, float accuracy)
        {
            Vector2 diff = source - target;
            return Math.Abs(diff.X) < accuracy && Math.Abs(diff.Y) < accuracy;
        }
        /// <summary>
        /// Note: This is for infintely long lines defined by the points
        /// </summary>
        /// <param name="l1Start"></param>
        /// <param name="l1End"></param>
        /// <param name="l2Start"></param>
        /// <param name="l2End"></param>
        /// <returns></returns>
        public static Vector2 LineIntersection(Vector2 l1Start, Vector2 l1End, Vector2 l2Start, Vector2 l2End)
        {
            float x1 = l1Start.X;
            float y1 = l1Start.Y;

            float x2 = l1End.X;
            float y2 = l1End.Y;

            float x3 = l2Start.X;
            float y3 = l2Start.Y;

            float x4 = l2End.X;
            float y4 = l2End.Y;

            float px = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4) / (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            float py = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4) / (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            return new Vector2(px, py);
        }

        public static float DirectionToRotation(Direction lookingDir)
        {
            return lookingDir switch
            {
                Direction.North => 0,
                Direction.NorthEast => MathHelper.ToRadians(45),
                Direction.East => MathHelper.ToRadians(90),
                Direction.SouthEast => MathHelper.ToRadians(135),
                Direction.South => MathHelper.ToRadians(180),
                Direction.SouthWest => MathHelper.ToRadians(225),
                Direction.West => MathHelper.ToRadians(270),
                Direction.NorthWest => MathHelper.ToRadians(315),
                _ => 0,
            };
        }
        public static Direction RotationToDirection(float rotationInRadians)
        {
            float accuracy = MathHelper.ToRadians(360) / 8;
            if (rotationInRadians > 0 - accuracy && rotationInRadians < 0 + accuracy)
                return Direction.North;

            float east = MathHelper.ToRadians(90);
            if (rotationInRadians > east - accuracy && rotationInRadians < east + accuracy)
                return Direction.East;

            float south = MathHelper.ToRadians(180);
            if (rotationInRadians > south - accuracy && rotationInRadians < south + accuracy)
                return Direction.South;

            float west = MathHelper.ToRadians(270);
            if (rotationInRadians > west - accuracy && rotationInRadians < west + accuracy)
                return Direction.West;


            return Direction.Other;
        }

        public static Direction RotationToDirectionEight(float rotationInRadians)
        {
            float accuracy = MathHelper.ToRadians(360) / 16;

            if (rotationInRadians > 0 - accuracy && rotationInRadians < 0 + accuracy)
                return Direction.North;

            float northeast = MathHelper.ToRadians(45);
            if (rotationInRadians > northeast - accuracy && rotationInRadians < northeast + accuracy)
                return Direction.NorthEast;

            float east = MathHelper.ToRadians(90);
            if (rotationInRadians > east - accuracy && rotationInRadians < east + accuracy)
                return Direction.East;

            float southeast = MathHelper.ToRadians(135);
            if (rotationInRadians > southeast - accuracy && rotationInRadians < southeast + accuracy)
                return Direction.SouthEast;

            float south = MathHelper.ToRadians(180);
            if (rotationInRadians > south - accuracy && rotationInRadians < south + accuracy)
                return Direction.South;

            float southwest = MathHelper.ToRadians(225);
            if (rotationInRadians > southwest - accuracy && rotationInRadians < southwest + accuracy)
                return Direction.SouthWest;

            float west = MathHelper.ToRadians(270);
            if (rotationInRadians > west - accuracy && rotationInRadians < west + accuracy)
                return Direction.West;

            float northwest = MathHelper.ToRadians(315);
            if (rotationInRadians > northwest - accuracy && rotationInRadians < northwest + accuracy)
                return Direction.NorthWest;


            return Direction.Other;
        }

        public static double NormalizeAngle(double angleF)
        {
            float threeSixty = MathHelper.ToRadians(360);
            while (angleF < 0)
                angleF += threeSixty;

            while (angleF > threeSixty)
                angleF -= threeSixty;
            return angleF;
        }

        public static bool IsInViewCone(Vector2 sourcePos, Vector2 targetPos, Vector2 lookDirection, float fovInDegrees)
        {
            Vector2 dirToTarget = targetPos - sourcePos;
            Vector2 dirToLook = lookDirection;
            double angleBetween = Math.Abs(RadianAngleBetween2DVectors(dirToTarget, dirToLook));

            if (angleBetween > Math.PI)
                angleBetween = Math.Abs((2 * Math.PI) - angleBetween);
            return angleBetween < (MathHelper.ToRadians(fovInDegrees) / 2);
        }

        public static bool GridRayTrace(double x0, double y0, double x1, double y1, CellHandler callback)
        {
            double dx = Math.Abs(x1 - x0);
            double dy = Math.Abs(y1 - y0);

            int x = (int)Math.Floor(x0);
            int y = (int)Math.Floor(y0);

            int n = 1;
            int x_inc, y_inc;
            double error;

            if (dx == 0)
            {
                x_inc = 0;
                error = double.PositiveInfinity;
            }
            else if (x1 > x0)
            {
                x_inc = 1;
                n += (int)Math.Floor(x1) - x;
                error = (Math.Floor(x0) + 1 - x0) * dy;
            }
            else
            {
                x_inc = -1;
                n += x - (int)Math.Floor(x1);
                error = (x0 - Math.Floor(x0)) * dy;
            }

            if (dy == 0)
            {
                y_inc = 0;
                error -= double.PositiveInfinity;
            }
            else if (y1 > y0)
            {
                y_inc = 1;
                n += (int)Math.Floor(y1) - y;
                error -= (Math.Floor(y0) + 1 - y0) * dx;
            }
            else
            {
                y_inc = -1;
                n += y - (int)Math.Floor(y1);
                error -= (y0 - Math.Floor(y0)) * dx;
            }

            for (; n > 0; --n)
            {
                if (!callback(x, y)) return false;

                if (error > 0)
                {
                    y += y_inc;
                    error -= dx;
                }
                else
                {
                    x += x_inc;
                    error += dy;
                }
            }
            return true;
        }

        public static bool BresenhamLine(int x0, int y0, int x1, int y1, CellHandler callback)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx, sy;

            if (x0 < x1) sx = 1; else sx = -1;
            if (y0 < y1) sy = 1; else sy = -1;

            int err = dx - dy;

            while (true)
            {
                if (!callback(x0, y0)) return false;
                if ((x0 == x1) && (y0 == y1)) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 >= dx) continue;

                err = err + dx;
                y0 = y0 + sy;
            }
            return true;
        }


        public static float DistanceToTile(Vector2 position, Point cell)
        {
            Vector2 vectorPos = cell.ToVector2();
            vectorPos.X += 0.5f;
            vectorPos.Y += 0.5f;
            return Vector2.Distance(position, vectorPos);
        }

        /// <summary>
        /// This is based off an explanation and expanded math presented by Paul Bourke:
        /// 
        /// It takes two lines as inputs and returns true if they intersect, false if they 
        /// don't.
        /// If they do, ptIntersection returns the point where the two lines intersect.  
        /// </summary>
        /// <param name="line1">The first line</param>
        /// <param name="line2">The second line</param>
        /// <param name="ptIntersection">The point where both lines intersect (if they do).</param>
        /// <returns></returns>
        /// <remarks>See http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/ </remarks>
        public static bool DoLinesIntersect(Line line1, Line line2, ref Vector2 ptIntersection)
        {
            // Denominator for ua and ub are the same, so store this calculation
            double d =
               (line2.Y2 - line2.Y1) * (line1.X2 - line1.X1)
               -
               (line2.X2 - line2.X1) * (line1.Y2 - line1.Y1);

            //nA and nB are calculated as seperate values for readability
            double nA =
               (line2.X2 - line2.X1) * (line1.Y1 - line2.Y1)
               -
               (line2.Y2 - line2.Y1) * (line1.X1 - line2.X1);

            double nB =
               (line1.X2 - line1.X1) * (line1.Y1 - line2.Y1)
               -
               (line1.Y2 - line1.Y1) * (line1.X1 - line2.X1);

            // Make sure there is not a division by zero - this also indicates that
            // the lines are parallel.  
            // If nA and nB were both equal to zero the lines would be on top of each 
            // other (coincidental).  This check is not done because it is not 
            // necessary for this implementation (the parallel check accounts for this).
            if (d == 0)
                return false;

            // Calculate the intermediate fractional point that the lines potentially intersect.
            double ua = nA / d;
            double ub = nB / d;

            // The fractional point will be between 0 and 1 inclusive if the lines
            // intersect.  If the fractional calculation is larger than 1 or smaller
            // than 0 the lines would need to be longer to intersect.
            if (ua >= 0d && ua <= 1d && ub >= 0d && ub <= 1d)
            {
                ptIntersection.X = (float)(line1.X1 + (ua * (line1.X2 - line1.X1)));
                ptIntersection.Y = (float)(line1.Y1 + (ua * (line1.Y2 - line1.Y1)));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the Direction of targetDir relative to the fwd vector.
        /// </summary>
        /// <param name="fwd">The forward vector which is basically the point of view. For an agent you would use his personal forward vector. Found in Agent.ForwardVector.</param>
        /// <param name="targetDir">A vector to the target location. Most common use for this is to find out if an object is to the left or the right of the agent. So you would insert a direction vector calculated from the positions of the object and the agent.</param>
        /// <param name="up">You should pass the up vector relative to the first two in here. This would usually be Agent.UpVector.</param>
        /// <returns>The side one has to turn to in order to face in the targetDir if one was looking in the fwd direction before.</returns>
        public static RelativeDirection AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);

            if (dir > 0f)
            {
                return RelativeDirection.Right;
            }
            if (dir < 0f)
            {
                return RelativeDirection.Left;
            }
            return RelativeDirection.SameDirection;
        }
        public static RelativeDirection GetDirection(Vector2 forward, Vector2 direction)
        {
            double angleBetween2DVectors = MathHelper.ToDegrees((float)NormalizeAngle(RadianAngleBetween2DVectors(forward, direction)));
            RelativeDirection dir = RelativeDirection.SameDirection;
            if (angleBetween2DVectors > 0 && angleBetween2DVectors < 180)
                dir = RelativeDirection.Right;
            else if (angleBetween2DVectors > 180 && angleBetween2DVectors < 360)
                dir = RelativeDirection.Left;
            return dir;
        }

        public static T ChooseRandomly<T>(List<T> collection)
        {
            return collection[Random.Next(0, collection.Count)];
        }
    }
}