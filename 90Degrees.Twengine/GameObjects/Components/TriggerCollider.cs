using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Twengine.Helper;

namespace Twengine.GameObjects.Components
{
    public class TriggerCollider : ICollidable
    {
        public Vector2 Position { get; set; }

        public SpriteSheet Texture {  get { return null; } }
        public Rectangle BoundingBox { get; set; }

        public Circle BoundingCircle { get; set; }

        public bool UseRect { get; set; }

        public bool IsAlive { get; set; }

        public string Tag { get; set; }

        public float Rotation { get; set; }

        public GameObject ParentGameObject { get; set; }

        public void UpdateBoundingBox() { }

        public void OnCollision(ICollidable collider) { }
    }
}
