using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{
    public class Physics : IComponent
    {
        public Physics(float maxspeed)
        {
            MaxSpeed = maxspeed;
            Mass = 1f;
            Acceleration = Vector2.Zero;
            Velocity = Vector2.Zero;
        }

        //public float Speed { get; set; }

        public float Mass { get; set; }
        public float MaxSpeed { get; set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Force { get; set; }
        private Vector2 mIntendedMovementDirection;
        public Vector2 IntendedMovementDirection
        {
            get { return mIntendedMovementDirection; }
            set { mIntendedMovementDirection = value == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(value); }
        }
    }
}
