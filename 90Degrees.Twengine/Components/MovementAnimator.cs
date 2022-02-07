using Artemis;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{
    class MovementAnimator : Component
    {
        public bool BeginAnimate { get; set; }
        public float AnimationSpeed { get; set; }
        public Vector2 MovementDirection { get; set; }
    }
}
