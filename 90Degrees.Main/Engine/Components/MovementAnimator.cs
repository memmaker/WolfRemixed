using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{
    class MovementAnimator : IComponent
    {
        public bool BeginAnimate { get; set; }
        public float AnimationSpeed { get; set; }
        public Vector2 MovementDirection { get; set; }
    }
}
