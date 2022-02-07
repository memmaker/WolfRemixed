using System;
using System.Collections.Generic;
using Artemis;
using Twengine.SubSystems;

namespace Twengine.Components
{
    public enum Shape
    {
        Circle,
        Rectangle
    }
    public class Collider : Component
    {
        public event EventHandler<CollisionEventArgs> CollidedWithEntity;
        public event EventHandler<EventArgs> CollidedWithWall;
        private float mRadius;
        public int CollisionGroup { get; set; }

        public Collider(float radius)
        {
            mRadius = radius;
            OccupiedGridCells = new List<CellIndex>();
            CollisionShape = Shape.Circle;
        }


        public float Radius
        {
            get { return mRadius; }
        }

        public bool IsTrigger { get; set; }

        public List<CellIndex> OccupiedGridCells { get; set; }

        public Shape CollisionShape { get; set; }

        public void OnCollisionWithEntity(Entity collidingEntity)
        {
            if (CollidedWithEntity == null) return;
            CollisionEventArgs collisionArgs = new CollisionEventArgs {CollidingEntity = collidingEntity};
            CollidedWithEntity(this, collisionArgs);
        }

        public void OnCollisionWithWall()
        {
            if (CollidedWithWall == null) return;
            CollidedWithWall(this, new EventArgs());
        }
    }

    public class CollisionEventArgs : EventArgs
    {
        public Entity CollidingEntity { get; set; }
    }
}
