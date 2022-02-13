using Artemis;
using Artemis.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using Script = Twengine.Scripts.Script;

namespace Twengine.Components
{

    public class MetaBehavior : IComponent
    {

        // Do subsumption architecture here..
        // but first try the following:
        // convert this into a metabehavior where you can freely add more behaviors
        // then broadcast collision and update to those

        private Dictionary<Type, Script> mBehaviorList;
        private Entity mSelf;
        private EntityWorld mWorld;

        public MetaBehavior(EntityWorld world, Entity parentEntity)
        {
            mBehaviorList = new Dictionary<Type, Script>();
            mWorld = world;
            mSelf = parentEntity;
        }

        public void AddBehavior(Script behavior)
        {
            behavior.Init(mWorld, mSelf, this);
            mBehaviorList[behavior.GetType()] = behavior;
        }

        public T GetBehavior<T>() where T : Script
        {
            if (mBehaviorList.ContainsKey(typeof(T)))
            {
                return (T)mBehaviorList[typeof(T)];
            }
            return null;
        }

        public void Update(float delta)
        {
            foreach (Script behavior in mBehaviorList.Values)
            {
                behavior.Update(delta);
            }
        }

        public virtual void OnCollision(object sender, CollisionEventArgs e)
        {
            foreach (Script behavior in mBehaviorList.Values)
            {
                behavior.OnCollision(e.CollidingEntity);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Script script in mBehaviorList.Values)
            {
                sb.Append(script.ToString());
            }
            return sb.ToString();
        }
    }
}
