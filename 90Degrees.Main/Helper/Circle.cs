using Microsoft.Xna.Framework;

namespace XNAHelper
{
    /// <summary> 
    /// Represents a 2D circle. 
    /// </summary> 
    public class Circle
    {
        private Vector2 v;
        private Vector2 direction;
        private float distanceSquared;

        /// <summary> 
        /// Center position of the circle. 
        /// </summary> 
        public Vector2 Center { get; set; }

        /// <summary> 
        /// Radius of the circle. 
        /// </summary> 
        public float Radius { get; set; }

        /// <summary> 
        /// Constructs a new circle. 
        /// </summary> 
        public Circle(Vector2 position, float radius)
        {
            this.distanceSquared = 0f;
            this.direction = Vector2.Zero;
            this.v = Vector2.Zero;
            this.Center = position;
            this.Radius = radius;
        }

        public bool Intersects(Circle circle)
        {
            float distance = Vector2.Distance(Center, circle.Center);
            return distance < (Radius + circle.Radius);
        }

        /// <summary> 
        /// Determines if a circle intersects a rectangle. 
        /// </summary> 
        /// <returns>True if the circle and rectangle overlap. False otherwise.</returns> 
        public bool Intersects(Rectangle rectangle)
        {
            this.v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            this.direction = Center - v;
            this.distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }

    }
}
