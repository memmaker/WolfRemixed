using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Helper;

namespace Twengine.GameObjects.Components
{
    public interface ICollidable
    {
        Vector2 Position { get; }
        SpriteSheet Texture { get; }
        Rectangle BoundingBox { get; }
        Circle BoundingCircle { get; }
        bool UseRect { get; }
        bool IsAlive { get; }
        string Tag { get; }
        float Rotation { get; }
        GameObject ParentGameObject { get; set; }

        void UpdateBoundingBox();
        void OnCollision(ICollidable collider);
    }
}
