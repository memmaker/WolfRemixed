using System;
using System.Collections.Generic;
using Artemis;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using Twengine;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using XNAHelper;
using ActorState = raycaster.Scripts.ActorState;

namespace raycaster.States
{

    public class Fire : ActorGameState
    {
        private bool mIsShooting;
        public Fire()
        {
            mIsShooting = false;
            FireSoundCues = new List<string>();
        }
        public override void Update()
        {
            base.Update();
            if (CanShootAtPlayer())
            {
                AimAtPlayer();
                if (!mIsShooting)
                {
                    StartFire();
                }
                NextActorState = ActorState.Shoot;
            }
            else
            {
                NextActorState = ActorState.Chase;
            }
        }

        

        public override string AnimationName
        {
            get { return "Fire"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Shoot; }
        }

        private void StartFire()
        {
            if (FireSoundCues != null)
                ComponentTwengine.AudioManager.PlayRandomSound(FireSoundCues);
            mIsShooting = true;
            OnResetAnimation();
        }

       
        private void DealDamage()
        {
            float dist = Vector2.Distance(mTransform.Position, mPlayerTransform.Position);
            if (PlayerIsHit(dist))
            {
                // mPlayerHealthPoints.DealDamage(CalculateDamage(dist),mSelf); // TODO: enable for damage
            }
            else
            {
                RaycastGame.AudioManager.PlayRandomSound(FireSoundCues);
            }
        }

        public override void FinishedAnimating()
        {
            mIsShooting = false;
            DealDamage();
        }

        public List<string> FireSoundCues { get; set; }

        public override void BeginState()
        {
            mIsShooting = false;
        }

        private bool PlayerIsHit(float dist)
        {
            // http://wolfenstein.wikia.com/wiki/Damage
            int rand1 = TwenMath.Random.Next(0, 255);
            int speed = 160;
            int look = 16;
            return rand1 < (speed - (dist*look));
        }

        private int CalculateDamage(float dist)
        {
            // http://wolfenstein.wikia.com/wiki/Damage
            int damage = 0;
            int rand2 = TwenMath.Random.Next(0, 255);
            if (dist < 2) damage = rand2 / 4;
            else if (dist < 4 && dist > 2) damage = rand2 / 8;
            else if (dist > 4) damage = rand2 / 16;
            return damage;
        }

    }
}
