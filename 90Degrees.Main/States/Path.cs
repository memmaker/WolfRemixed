using Microsoft.Xna.Framework;
using raycaster.Scripts;
using XNAHelper;

namespace raycaster.States
{
    public class Path : ActorGameState
    {


        public override string AnimationName
        {
            get { return "Walk"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Path; }
        }

        public override void Update()
        {
            base.Update();
            if ((AttackOnSight && CanDetectPlayer()) || EnemyWasWounded())
            {
                if (PlayerIsInRange(mTransform.Position, mEnemy.FiringRange))
                {
                    NextActorState = ActorState.PrepareShoot;
                }
                else
                {
                    NextActorState = ActorState.Chase;
                }
            }
            else
            {
                NextActorState = ActorState.Path;
                PatrolOnWaypoints();
            }
        }

        private void PatrolOnWaypoints()
        {
            char cellData = mMap.GetCellMetaDataByPosition(mTransform.Position);
            switch (cellData)
            {
                case '^':
                    Walk(-Vector2.UnitY);
                    break;
                case ',':
                    Walk(Vector2.UnitY);
                    break;
                case '<':
                    Walk(-Vector2.UnitX);
                    break;
                case '>':
                    Walk(Vector2.UnitX);
                    break;
                default:
                    Walk(mTransform.Forward);
                    break;
            }
        }

        private void Walk(Vector2 dir)
        {
            mPhysics.IntendedMovementDirection = dir;
            mTransform.Rotation = TwenMath.DirectionVectorToRotation(dir);
        }
    }
}
