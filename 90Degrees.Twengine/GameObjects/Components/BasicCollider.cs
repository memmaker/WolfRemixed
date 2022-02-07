using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Helper;

namespace Twengine.GameObjects.Components
{
    public class BasicCollider : ICollidable
    {
        private Rectangle mBoundingBox;
        public Vector2 Position
        {
            get { return ParentGameObject.Position; }
        }

        public SpriteSheet Texture
        {
            get { return ParentGameObject.Sprite.SheetController; }
        }

        public Circle BoundingCircle
        {
            get
            {
                return new Circle(ParentGameObject.Position,ParentGameObject.Sprite.ScaledWidth/2);        // FIXME: we might want the size of the diagonal here..
            }
        }

        public bool UseRect
        {
            get { return false; }
        }

        public Rectangle BoundingBox
        {
            get
            {
                return mBoundingBox;
            }
        }

        public bool IsAlive { get { return ParentGameObject != null && ParentGameObject.IsCollider && ParentGameObject.IsSprite; } }

        public string Tag
        {
            get { return ParentGameObject.Tag; }
        }

        public void UpdateBoundingBox()
        {
            Matrix matrix = ParentGameObject.Sprite.GetTransformationMatrix();
            Vector2 tl = Vector2.Transform(Vector2.Zero, matrix);
            Vector2 tr = Vector2.Transform(new Vector2(ParentGameObject.Sprite.FrameWidth, 0), matrix);
            Vector2 bl = Vector2.Transform(new Vector2(0, ParentGameObject.Sprite.FrameHeight), matrix);
            Vector2 br = Vector2.Transform(new Vector2(ParentGameObject.Sprite.FrameWidth, ParentGameObject.Sprite.FrameHeight), matrix);

            float minX = Math.Min(tl.X, Math.Min(tr.X, Math.Min(bl.X, br.X)));
            float maxX = Math.Max(tl.X, Math.Max(tr.X, Math.Max(bl.X, br.X)));
            float minY = Math.Min(tl.Y, Math.Min(tr.Y, Math.Min(bl.Y, br.Y)));
            float maxY = Math.Max(tl.Y, Math.Max(tr.Y, Math.Max(bl.Y, br.Y)));
            Vector2 min = new Vector2(minX, minY);
            Vector2 max = new Vector2(maxX, maxY);

            mBoundingBox = new Rectangle((int) min.X, (int) min.Y, (int) (max.X - min.X), (int) (max.Y - min.Y));
        }

        private Rectangle SimpleBoundingBox()
        {
            int x = (int)(ParentGameObject.Position.X - (ParentGameObject.Sprite.ScaledWidth / 2));
            int y = (int)(ParentGameObject.Position.Y - (ParentGameObject.Sprite.ScaledHeight / 2));
            return new Rectangle(x, y, (int)ParentGameObject.Sprite.ScaledWidth, (int)ParentGameObject.Sprite.ScaledHeight);
        }

        public float Rotation
        {
            get { return ParentGameObject.Rotation; }
        }

        public GameObject ParentGameObject { get; set; }

        public void OnCollision(ICollidable collider)
        {
            ParentGameObject.OnCollide(collider);
        }
    }
}
