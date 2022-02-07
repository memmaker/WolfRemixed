using System;
using Artemis;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;

namespace raycaster.States
{
    public class StandHidden : ActorGameState
    {
        public override string AnimationName
        {
            get { return "Idle"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.StandHidden; }
        }

        public override void Update()
        {
            base.Update();
            if (EnemyWasWounded())
            {
                NextActorState = ActorState.BreakFree;
            }
            else
            {
                NextActorState = ActorState.StandHidden;
            }

        }

    }
}
