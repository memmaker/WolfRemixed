using System;
using System.Collections.Generic;
using Artemis;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using Twengine;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;

namespace raycaster.States
{
    public class Stand : ActorGameState
    {
        public override string AnimationName
        {
            get { return "Idle"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Stand; }
        }
        public List<string> DetectPlayerSoundCues { get; set; }
        public override void Update()
        {
            base.Update();
            if ((AttackOnSight && CanDetectPlayer()) || EnemyWasWounded())
            {
                if (DetectPlayerSoundCues != null)
                    ComponentTwengine.AudioManager.PlayRandomSound(DetectPlayerSoundCues);

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
                NextActorState = ActorState.Stand;
            }

        }

    }
}
