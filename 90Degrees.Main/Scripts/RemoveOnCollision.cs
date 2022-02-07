using System;
using Artemis;
using Twengine.Components;
using Twengine.Helper;
using Twengine.Scripts;

namespace raycaster.Scripts
{
    public delegate bool RemovePredicate();

    public class RemoveOnCollision : Script
    {
        private RemovePredicate mRemovalCheck;

        public RemoveOnCollision() : this(Always)
        {
        }

        private static bool Always()
        {
            return true;
        }

        public RemoveOnCollision(RemovePredicate predicate)
        {
            mRemovalCheck = predicate;
        }

        public override void Update(float delta)
        {
            
        }

        public override void OnCollision(Entity collider)
        {
            if (mRemovalCheck())
                mSelf.Delete();
        }

    }
}