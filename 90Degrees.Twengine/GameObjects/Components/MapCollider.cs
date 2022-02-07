using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Twengine.Helper;

namespace Twengine.GameObjects.Components
{
    public class MapCollider : ICollidable
    {
        public Vector2 Position
        {
            get
            {
                return new Vector2(BoundingBox.Center.X,BoundingBox.Center.Y);
            }
        }

        public SpriteSheet Texture { get; set; }
        public Rectangle BoundingBox { get; set; }

        public Circle BoundingCircle
        {
            get { throw new NotImplementedException("No need, since all MapTiles are rects.."); }
        }

        public bool UseRect
        {
            get { return true; }
        }

        public bool IsAlive
        {
            get { return true; }
        }

        public string Tag
        {
            get { return "Scenery"; }
        }

        public float Rotation
        {
            get { return 0f; }
        }

        public GameObject ParentGameObject { get { return null; } set { } }

        public void UpdateBoundingBox()
        {
            
        }

        public void OnCollision(ICollidable collider)
        {
            
        }
    }
}
