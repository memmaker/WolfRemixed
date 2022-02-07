using System;
using Artemis;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using XNAHelper;
using ActorState = raycaster.Scripts.ActorState;

namespace raycaster.States
{

    public class BreakFree : ActorGameState
    {
        public bool LeaveRemains { get; set; }
        public override string AnimationName
        {
            get { return "BreakFree"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.BreakFree; }
        }

        public override void FinishedAnimating()
        {
            if (LeaveRemains)
            {
                RaycastSprite raycastSprite = mSelf.GetComponent<RaycastSprite>();
                EntitySpawn.CreateDecoSprite(raycastSprite.SpriteSheet, mTransform.Position, raycastSprite.FrameIndex + 1,
                                             false);
            }
            NextActorState = ActorState.Chase;
            FollowPlayer(mPlayer);
        }
        public void FollowPlayer(Entity player)
        {
            Vector2 directionToPlayer = (mPlayerTransform.Position - mTransform.Position);
            mPhysics.IntendedMovementDirection = directionToPlayer;
            mTransform.Rotation = TwenMath.DirectionVectorToRotation(directionToPlayer);
        }

    }
}
