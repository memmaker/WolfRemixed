using System;
using System.Collections.Generic;
using raycaster.Scripts;
using Twengine;
using Twengine.Helper;

namespace raycaster.States
{
    public class Pain : ActorGameState
    {
        
        public override string AnimationName
        {
            get { return "Hit"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Pain; }
        }
        public Pain()
        {
            PainSoundCues = new List<string>();
        }
        public override void FinishedAnimating()
        {
            NextActorState = ActorState.Chase;
        }
        public override void BeginState()
        {
            NextActorState = ActorState.Pain;
            ComponentTwengine.AudioManager.PlayRandomSound(PainSoundCues);
        }

        public List<string> PainSoundCues { get; set; }


    }
}
