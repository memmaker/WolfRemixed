using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using raycaster.States;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using Twengine.Scripts;

namespace raycaster.Scripts
{
    public enum ActorState
    {
        Stand,
        Path,
        Pain,
        PrepareShoot,
        Shoot,
        Chase,
        Die,
        StandHidden,
        BreakFree
    }
    class ActorStateMachine : Script
    {
        private ActorGameState mCurrentState;
        private Dictionary<ActorState, ActorGameState> mStates;
        private Tilemap mTilemap;
        private SpriteAnimator mSpriteAnimator;
        private ActorGameState mLastState;

        public ActorStateMachine(Tilemap tilemap)
        {
            mTilemap = tilemap;
            mStates = new Dictionary<ActorState, ActorGameState>();
            mLastState = null;
        }
        protected override void Initialize()
        {
            mSpriteAnimator = mSelf.GetComponent<SpriteAnimator>();
            mSpriteAnimator.FinishedPlaying += SpriteAnimatorFinishedPlaying;
            
        }

        private void SpriteAnimatorFinishedPlaying(object sender, EventArgs e)
        {
            mCurrentState.FinishedAnimating();
        }

        public void AddStartState(ActorGameState state)
        {
            AddState(state);
            mCurrentState = state;
            mCurrentState.Begin(mWorld, mSelf, mTilemap);
        }

        public void AddState(ActorGameState state)
        {
            mStates[state.ActorStateType] = state;
            state.ResetAnimation += StateResetAnimation;
        }

        void StateResetAnimation(object sender, EventArgs e)
        {
            mSpriteAnimator.ResetAndPlay();
        }

        public override void Update(float delta)
        {

            mCurrentState.Update();

            mCurrentState = mStates[mCurrentState.NextActorState];
            
            if (mCurrentState != mLastState)
            {
                BeginState();
            }
            mLastState = mCurrentState;
        }

        private void BeginState()
        {
            mCurrentState.Begin(mWorld, mSelf, mTilemap);
            mSpriteAnimator.CurrentAnimation = mCurrentState.AnimationName;
            mSpriteAnimator.ResetAndPlay();
        }

        public string CurrentState { get { return Enum.GetName(typeof (ActorState), mCurrentState.ActorStateType); } }

        public override string ToString()
        {
            return "State: " + CurrentState;
        }

        public void ActorDying(object sender, EventArgs args)
        {
            mCurrentState = mStates[ActorState.Die];
            BeginState();
        }

        public void ActorHit(object sender, DamageEventArgs damageEventArgs)
        {
            if (mCurrentState.ActorStateType == ActorState.BreakFree || mCurrentState.ActorStateType == ActorState.StandHidden) return;
            mCurrentState = mStates[ActorState.Pain];
            BeginState();
        }

        public void ActorAlert()
        {
            if (mCurrentState.ActorStateType == ActorState.StandHidden)
            {
                mCurrentState = mStates[ActorState.BreakFree];
                BeginState();
            }
            else if (mCurrentState.ActorStateType == ActorState.Stand || mCurrentState.ActorStateType == ActorState.Path)
            {
                mCurrentState = mStates[ActorState.Chase];
                BeginState();
            }
            
        }
    }
}
