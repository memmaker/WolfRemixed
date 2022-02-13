using Artemis;
using Microsoft.Xna.Framework;
using System;
using Twengine.Components;
using Twengine.Datastructures;
using XNAHelper;

namespace raycaster.Scripts
{
    public abstract class ActorGameState
    {
        private bool mFirstTimeInitDone;
        protected Entity mPlayer;
        protected Transform mPlayerTransform;
        protected HealthPoints mPlayerHealthPoints;
        protected Enemy mEnemy;
        protected Entity mSelf;
        protected Transform mTransform;
        protected Physics mPhysics;
        protected Tilemap mMap;
        private HealthPoints mHealthPoints;
        private SpriteAnimator mSpriteAnimator;

        public event EventHandler<EventArgs> ResetAnimation;
        public abstract string AnimationName { get; }
        public ActorState NextActorState { get; set; }
        public abstract ActorState ActorStateType { get; }

        public bool AttackOnSight { get; set; }

        public ActorGameState()
        {
            mFirstTimeInitDone = false;
            AttackOnSight = true;
        }

        public virtual void Update()
        {
            if (mPlayer != RaycastGame.Player)
            {
                mPlayer = RaycastGame.Player;
                mPlayerTransform = mPlayer.GetComponent<Transform>();
                mPlayerHealthPoints = mPlayer.GetComponent<HealthPoints>();
            }
        }

        public virtual void FinishedAnimating() { }

        protected bool EnemyWasWounded()
        {
            bool stillPlayingPainAnimation = mSpriteAnimator.CurrentAnimation == "Hit";
            return !stillPlayingPainAnimation && mHealthPoints.Health != mHealthPoints.MaxHealth;
        }

        protected bool PlayerIsInRange(Vector2 position, float range)
        {
            return Vector2.Distance(mPlayerTransform.Position, position) < range;
        }
        protected bool CanDetectPlayer()
        {
            return mPlayerTransform.IsVisible && (PlayerIsInRange(mTransform.Position, 1) || (PlayerIsInRange(mTransform.Position, mEnemy.VisibleRange) && mMap.HasLineOfSight(mTransform.Position, mPlayerTransform.Position, mTransform.Forward, mEnemy.FieldOfView)));
        }

        protected bool CanShootAtPlayer()
        {
            bool inRange = PlayerIsInRange(mTransform.Position, mEnemy.FiringRange);
            bool hasLineOfSight = mMap.HasLineOfSight(mTransform.Position, mPlayerTransform.Position, mTransform.Forward, mEnemy.FieldOfView);
            return (mPlayerTransform.IsVisible && inRange && hasLineOfSight);
        }
        protected void AimAtPlayer()
        {
            if (!mPlayerTransform.IsVisible) return;
            mPhysics.IntendedMovementDirection = Vector2.Zero;
            mTransform.Rotation = TwenMath.DirectionVectorToRotation(mPlayerTransform.Position - mTransform.Position);
        }

        protected void OnResetAnimation()
        {
            if (ResetAnimation == null) return;
            ResetAnimation(this, new EventArgs());
        }

        public void Begin(Entity self, Tilemap map)
        {

            if (!mFirstTimeInitDone)
            {
                mSelf = self;
                mMap = map;
                if (mSelf.HasComponent<Enemy>())
                {
                    mEnemy = self.GetComponent<Enemy>();
                }
                mHealthPoints = mSelf.GetComponent<HealthPoints>();
                mSpriteAnimator = mSelf.GetComponent<SpriteAnimator>();
                mTransform = mSelf.GetComponent<Transform>();
                mPhysics = mSelf.GetComponent<Physics>();
                mFirstTimeInitDone = true;
                NextActorState = ActorStateType;
                //OnResetAnimation();
                FirstTimeInit();
            }
            BeginState();
        }

        public virtual void BeginState()
        {
        }

        public virtual void FirstTimeInit()
        {

        }
    }


}
