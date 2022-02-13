using Artemis;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using XNAHelper;
using ActorState = raycaster.Scripts.ActorState;

namespace raycaster.States
{
    public class Chase : ActorGameState
    {

        public override string AnimationName
        {
            get { return "Walk"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Chase; }
        }

        public override void Update()
        {
            base.Update();
            if (CanShootAtPlayer())
            {
                NextActorState = ActorState.PrepareShoot;
            }
            else
            {
                FollowPlayer(mPlayer);
                NextActorState = ActorState.Chase;
            }
        }

        public override void BeginState()
        {
            NextActorState = ActorState.Chase;
        }


        public void FollowPlayer(Entity player)
        {
            Vector2 directionToPlayer = (mPlayerTransform.Position - mTransform.Position);
            mPhysics.IntendedMovementDirection = directionToPlayer;
            mTransform.Rotation = TwenMath.DirectionVectorToRotation(directionToPlayer);
        }

    }
}
