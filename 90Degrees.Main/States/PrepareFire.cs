using raycaster.Scripts;
using ActorState = raycaster.Scripts.ActorState;

namespace raycaster.States
{
    public enum FireState
    {
        Idle,
        PrepareFire,
        Fire
    }
    public class PrepareFire : ActorGameState
    {
        private bool mFinishedAnimating;

        public PrepareFire()
        {
            mFinishedAnimating = false;
        }
        public override void Update()
        {
            base.Update();
            //DebugDrawer.DrawString("Finished Animating: " + mFinishedAnimating);
            if (mFinishedAnimating)
            {
                NextActorState = ActorState.Shoot;
                mFinishedAnimating = false;
            }
            else if (CanShootAtPlayer())
            {
                NextActorState = ActorState.PrepareShoot;
                AimAtPlayer();
            }
            else
            {
                NextActorState = ActorState.Chase;
                mFinishedAnimating = true;
            }
        }

        public override string AnimationName
        {
            get { return "PrepareFire"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.PrepareShoot; }
        }


        public override void FinishedAnimating()
        {
            mFinishedAnimating = true;
        }


    }
}
