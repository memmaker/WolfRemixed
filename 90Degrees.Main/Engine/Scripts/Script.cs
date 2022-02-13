using Artemis;
using Twengine.Components;

namespace Twengine.Scripts
{
    public abstract class Script
    {
        protected EntityWorld mWorld;
        protected Entity mSelf;
        protected Physics Physics;
        protected Transform mTransform;
        protected MetaBehavior mMetaBehavior;

        public void Init(EntityWorld world, Entity entity, MetaBehavior meta)
        {
            mWorld = world;
            mSelf = entity;
            Physics = mSelf.GetComponent<Physics>();
            mTransform = mSelf.GetComponent<Transform>();
            mMetaBehavior = meta;
            Initialize();
        }

        protected virtual void Initialize() { }

        public abstract void Update(float delta);

        public T GetBehavior<T>() where T : Script
        {
            return mMetaBehavior.GetBehavior<T>();
        }

        public virtual void OnCollision(Entity collidingEntity)
        {

        }
    }
}
